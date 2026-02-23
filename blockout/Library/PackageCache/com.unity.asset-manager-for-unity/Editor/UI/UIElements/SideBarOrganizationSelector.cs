using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.AssetManager.Core.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using AuthenticationState = Unity.AssetManager.Core.Editor.AuthenticationState;

namespace Unity.AssetManager.UI.Editor
{
    class SideBarOrganizationSelector : VisualElement
    {
        const string k_UssClassName = "unity-org-selector";
        const string k_ButtonUssClassName = k_UssClassName + "-button";
        const string k_ButtonDisabledUssClassName = k_ButtonUssClassName + "--disabled";
        const string k_OrganizationChoice = k_UssClassName + "-choice";
        const string k_OrganizationChoiceSeparatorLine = k_UssClassName + "-choice-separator-line";
        const string k_OrganizationChoiceText = k_UssClassName + "-choice-text";
        const string k_OrganizationChoiceRoleContainer = k_UssClassName + "-choice-role-container";
        const string k_OrganizationChoiceRole = k_OrganizationChoice + "-role";
        const string k_OrganizationChoiceSeatWarning = k_OrganizationChoice + "-seat-warning";
        const string k_OrganizationChoiceCheckmark = k_OrganizationChoice + "-checkmark";
        const string k_DefaultOrgTooltip = "If configured, the Organization linked within " +
                                           "the Project Settings will display at the top of the Organization List.";

        [SerializeReference]
        IPermissionsManager m_PermissionsManager;

        [SerializeReference]
        IProjectOrganizationProvider m_ProjectOrganizationProvider;

        [SerializeReference]
        IUnityConnectProxy m_UnityConnectProxy;

        IPopupManager m_PopupManager;
        Button m_OrganizationButton;
        TextElement m_OrganizationButtonText;
        VisualElement m_Caret;

        static bool IsSelectionEnabled
        {
            get
            {
                var privateCloudSettings = PrivateCloudSettings.Load();
                return !privateCloudSettings.ServicesEnabled;
            }
        }

        Dictionary<string, NameAndId> m_OrganizationOptions = new();
        Dictionary<string, Role> m_OrganizationRoles = new();
        Dictionary<string, bool> m_OrganizationSeatValidity = new();
        Dictionary<string, Task> m_FetchOrganizationsTasks = new();
        string m_LinkedOrganizationName;

        public SideBarOrganizationSelector(IPermissionsManager permissionsManager,
            IProjectOrganizationProvider projectOrganizationProvider, IUnityConnectProxy unityConnectProxy,
            IPopupManager popupManager)
        {
            m_PermissionsManager = permissionsManager;
            m_ProjectOrganizationProvider = projectOrganizationProvider;
            m_UnityConnectProxy = unityConnectProxy;

            m_PermissionsManager.AuthenticationStateChanged += OnAuthenticationStateChanged;
            m_ProjectOrganizationProvider.OrganizationChanged += OnOrganizationChanged;
            m_UnityConnectProxy.OrganizationIdChanged += RefreshDropdown;
            m_PopupManager = popupManager;

            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);

            InitializeUI();
        }

        void OnAttachToPanel(AttachToPanelEvent evt)
        {
            PrivateCloudSettings.SettingsUpdated += UpdateSelectionEnabled;
        }

        void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            PrivateCloudSettings.SettingsUpdated -= UpdateSelectionEnabled;
        }

        void OnAuthenticationStateChanged(AuthenticationState authenticationState)
        {
            if (authenticationState == AuthenticationState.LoggedIn)
            {
                _ = FetchOrganizationsData();
            }
            else
            {
                ClearDropdown();
            }

            UpdateSelectionEnabled();
        }

        void OnOrganizationChanged(OrganizationInfo _)
        {
            RefreshDropdown();
        }

        void RefreshDropdown()
        {
            _ = FetchOrganizationsData();
        }

        void InitializeUI()
        {
            AddToClassList(k_UssClassName);

            m_OrganizationButton = new Button
            {
                tooltip = k_DefaultOrgTooltip
            };
            m_OrganizationButton.AddToClassList(k_ButtonUssClassName);

            m_Caret = new VisualElement();
            m_Caret.AddToClassList("unity-org-selector-caret");

            m_OrganizationButtonText = new TextElement();
            m_OrganizationButtonText.text = L10n.Tr(Constants.LoadingText);
            m_OrganizationButtonText.AddToClassList("unity-text-element");
            m_OrganizationButton.Add(m_OrganizationButtonText);

            m_OrganizationButton.Add(m_Caret);
            Add(m_OrganizationButton);

            if (m_PermissionsManager.AuthenticationState == AuthenticationState.LoggedIn)
                _ = FetchOrganizationsData();

            m_OrganizationButton.RegisterCallback<ClickEvent>( evt =>
            {
                if (!IsSelectionEnabled || m_OrganizationOptions.Count <= 1)
                    return;

                BuildOrganizationSelection();
                m_PopupManager.Show(m_OrganizationButton, PopupContainer.PopupAlignment.BottomLeft);
                m_OrganizationButton.tooltip = null;
            });

            m_PopupManager.Container.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                if (m_PopupManager.Container.style.display == DisplayStyle.None && string.IsNullOrEmpty(m_OrganizationButton.tooltip))
                    m_OrganizationButton.tooltip = IsSelectionEnabled ? k_DefaultOrgTooltip : null;
            });

            UpdateSelectionEnabled();
        }

        void OnSelectionChanged(string organizationName)
        {
            if (m_OrganizationOptions.TryGetValue(organizationName, out var organization))
                m_ProjectOrganizationProvider.SelectOrganization(organization.Id);

            AnalyticsSender.SendEvent(new OrganizationSelectedEvent());
        }

        async Task FetchOrganizationsData()
        {
            ClearDropdown();

            m_OrganizationOptions = new Dictionary<string, NameAndId>();
            await foreach (var organization in m_ProjectOrganizationProvider.ListOrganizationsAsync())
            {
                m_OrganizationOptions[organization.Name] = organization;
                if (!m_FetchOrganizationsTasks.ContainsKey(organization.Name))
                    m_FetchOrganizationsTasks[organization.Name] = FetchOrganizationRoleAndEntitlements(organization.Name, organization.Id);
            }

            var linkedOrganizationId = m_UnityConnectProxy.HasValidOrganizationId ? m_UnityConnectProxy.OrganizationId : null;
            m_LinkedOrganizationName = string.Empty;
            if (linkedOrganizationId != null)
                m_LinkedOrganizationName = m_OrganizationOptions.Values.FirstOrDefault(o => o.Id == linkedOrganizationId).Name;

            var selectedOrganization = m_ProjectOrganizationProvider.SelectedOrganization;
            if (selectedOrganization != null)
            {
                SetSelectedOrganizationWithoutNotify(selectedOrganization.Name);
            }
        }

        async Task FetchOrganizationRoleAndEntitlements(string organizationName, string organizationId)
        {
            if(!m_OrganizationRoles.ContainsKey(organizationName))
                m_OrganizationRoles[organizationName] = await m_PermissionsManager.GetRoleAsync(organizationId, string.Empty);

            if (!m_OrganizationSeatValidity.ContainsKey(organizationName))
                m_OrganizationSeatValidity[organizationName] = await m_PermissionsManager.CheckSeatValidity(organizationId);
        }

        void BuildOrganizationSelection()
        {
            m_PopupManager.Clear();
            var organizationSelection = new ScrollView();
            var selectedOrganizationName = m_ProjectOrganizationProvider.SelectedOrganization?.Name;

            if (!string.IsNullOrEmpty(m_LinkedOrganizationName))
            {
                var linkedOrganizationChoice = new VisualElement();
                linkedOrganizationChoice.AddToClassList(k_OrganizationChoice);

                // Add checkmark or spacer for alignment
                var checkmark = new VisualElement();
                checkmark.AddToClassList(k_OrganizationChoiceCheckmark);
                if (m_LinkedOrganizationName != selectedOrganizationName)
                {
                    checkmark.style.opacity = 0; // Invisible spacer for alignment
                }
                linkedOrganizationChoice.Add(checkmark);

                var linkedOrganizationChoiceName = new TextElement();
                linkedOrganizationChoiceName.AddToClassList(k_OrganizationChoiceText);
                linkedOrganizationChoiceName.text = m_LinkedOrganizationName;

                linkedOrganizationChoice.Add(linkedOrganizationChoiceName);

                var linkedOrgRoleAndSeatContainer = new VisualElement();
                linkedOrgRoleAndSeatContainer.AddToClassList(k_OrganizationChoiceRoleContainer);

                var linkedOrgRoleCapsule = new TextElement();
                if (m_OrganizationRoles.ContainsKey(m_LinkedOrganizationName))
                {
                    linkedOrgRoleCapsule.text = m_OrganizationRoles[m_LinkedOrganizationName].ToString();
                    linkedOrgRoleCapsule.AddToClassList(k_OrganizationChoiceRole);
                    linkedOrgRoleAndSeatContainer.Add(linkedOrgRoleCapsule);

                    if (m_OrganizationSeatValidity.ContainsKey(m_LinkedOrganizationName) &&
                        !m_OrganizationSeatValidity[m_LinkedOrganizationName])
                    {
                        //add a warning
                        var seatWarning = new Image();
                        seatWarning.AddToClassList(k_OrganizationChoiceSeatWarning);
                        seatWarning.tooltip = Constants.NoSeatAssignedWarning;
                        linkedOrgRoleAndSeatContainer.Add(seatWarning);
                    }
                }

                linkedOrganizationChoice.Add(linkedOrgRoleAndSeatContainer);

                linkedOrganizationChoice.RegisterCallback<ClickEvent>(evt =>
                {
                    evt.StopPropagation();
                    OnSelectionChanged(m_LinkedOrganizationName);
                    m_PopupManager.Hide();
                });

                organizationSelection.Add(linkedOrganizationChoice);

                var line = new VisualElement();
                line.AddToClassList(k_OrganizationChoiceSeparatorLine);
                organizationSelection.Add(line);
            }

            foreach (var organizationName in m_OrganizationOptions.Keys.OrderBy(organization => organization).ToList())
            {
                if (organizationName == m_LinkedOrganizationName)
                    continue;

                var organizationChoice = new VisualElement();
                organizationChoice.AddToClassList(k_OrganizationChoice);

                // Add checkmark or spacer for alignment
                var checkmark = new VisualElement();
                checkmark.AddToClassList(k_OrganizationChoiceCheckmark);
                if (organizationName != selectedOrganizationName)
                {
                    checkmark.style.opacity = 0; // Invisible spacer for alignment
                }
                organizationChoice.Add(checkmark);

                var organizationChoiceName = new TextElement();
                organizationChoiceName.AddToClassList(k_OrganizationChoiceText);
                organizationChoiceName.text = organizationName;
                organizationChoice.Add(organizationChoiceName);

                var roleAndSeatContainer = new VisualElement();
                roleAndSeatContainer.AddToClassList(k_OrganizationChoiceRoleContainer);

                if (m_OrganizationRoles.ContainsKey(organizationName))
                {
                    var orgRoleCapsule = new TextElement
                    {
                        text = m_OrganizationRoles[organizationName].ToString()
                    };
                    orgRoleCapsule.AddToClassList(k_OrganizationChoiceRole);
                    roleAndSeatContainer.Add(orgRoleCapsule);

                    if (m_OrganizationSeatValidity.ContainsKey(organizationName) && !m_OrganizationSeatValidity[organizationName])
                    {
                        //add a warning
                        var seatWarning = new Image();
                        seatWarning.AddToClassList(k_OrganizationChoiceSeatWarning);
                        seatWarning.tooltip = Constants.NoSeatAssignedWarning;
                        roleAndSeatContainer.Add(seatWarning);
                    }
                }

                organizationChoice.Add(roleAndSeatContainer);

                organizationChoice.RegisterCallback<ClickEvent>(evt =>
                {
                    evt.StopPropagation();
                    OnSelectionChanged(organizationName);
                    m_PopupManager.Hide();
                });


                organizationSelection.Add(organizationChoice);
            }

            m_PopupManager.Container.Add(organizationSelection);
        }

        void UpdateSelectionEnabled()
        {
            if (IsSelectionEnabled)
            {
                m_OrganizationButton.RemoveFromClassList(k_ButtonDisabledUssClassName);
                m_Caret.style.display = DisplayStyle.Flex;
                m_OrganizationButton.tooltip = k_DefaultOrgTooltip;
            }
            else
            {
                m_OrganizationButton.AddToClassList(k_ButtonDisabledUssClassName);
                m_Caret.style.display = DisplayStyle.None;
                m_OrganizationButton.tooltip = null;
            }
        }

        void ClearDropdown()
        {
            m_OrganizationOptions.Clear();
        }

        void SetSelectedOrganizationWithoutNotify(string organizationName)
        {
            if (m_OrganizationOptions.ContainsKey(organizationName))
                m_OrganizationButtonText.text = organizationName;
        }
    }
}
