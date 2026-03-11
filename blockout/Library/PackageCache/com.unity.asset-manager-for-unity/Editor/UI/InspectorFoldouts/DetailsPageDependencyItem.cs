using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.AssetManager.Core.Editor;
using Unity.AssetManager.Upload.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AssetManager.UI.Editor
{
    class DetailsPageDependencyItem : VisualElement
    {
        const string k_DetailsPageFileItemUssStyle = "details-page-dependency-item";
        const string k_DetailsPageFileItemInfoUssStyle = "details-page-dependency-item-info";
        const string k_DetailsPageFileItemStatusUssStyle = "details-page-dependency-item-status";
        const string k_DetailsPageFileIconItemUssStyle = "details-page-dependency-item-icon";
        const string k_DetailsPageFileLabelItemUssStyle = "details-page-dependency-item-label";
        const string k_DetailsPageDependencySelectionItemDisabledUssStyle = "details-page-dependency-selection-disabled";
        const string k_DetailsPageDependencyButtonUssStyle = "details-page-dependency-selection-button";
        const string k_DetailsPageDependencyButtonCaretUssStyle = "details-page-dependency-selection-button-caret";

        readonly Button m_Button;
        readonly VisualElement m_Icon;
        readonly Label m_FileName;
        readonly VisualElement m_ImportedStatusIcon;
        readonly Label m_VersionNumber;
        readonly Button m_DependencyVersionButton;
        readonly TextElement m_DependencyVersionButtonText;

        readonly IPageManager m_PageManager;
        readonly IPopupManager m_PopupManager;
        readonly ISettingsManager m_SettingsManager;
        readonly IProjectOrganizationProvider m_ProjectOrganizationProvider;

        AssetIdentifier m_AssetIdentifier;
        BaseAssetData m_AssetData;
        Dictionary<string, string> m_VersionsIds = new();
        List<string> m_VersionLabels = new();

        CancellationTokenSource m_CancellationTokenSource;

        public DetailsPageDependencyItem(IPageManager pageManager, IPopupManager popupManager, ISettingsManager settingsManager, IProjectOrganizationProvider projectOrganizationProvider)
        {
            m_PageManager = pageManager;
            m_PopupManager = popupManager;
            m_ProjectOrganizationProvider = projectOrganizationProvider;

            m_SettingsManager = settingsManager;

            m_Button = new Button(() =>
            {
                if (m_AssetIdentifier != null)
                {
                    m_PageManager.ActivePage.SelectAsset(m_AssetIdentifier, false);
                }
            });

            Add(m_Button);

            m_Button.focusable = false;
            m_Button.AddToClassList(k_DetailsPageFileItemUssStyle);

            var infoElement = new VisualElement();
            infoElement.AddToClassList(k_DetailsPageFileItemInfoUssStyle);

            m_FileName = new Label("");
            m_Icon = new VisualElement();

            m_Icon.AddToClassList(k_DetailsPageFileIconItemUssStyle);
            m_FileName.AddToClassList(k_DetailsPageFileLabelItemUssStyle);

            infoElement.Add(m_Icon);
            infoElement.Add(m_FileName);

            var statusElement = new VisualElement();
            statusElement.AddToClassList(k_DetailsPageFileItemStatusUssStyle);

            m_ImportedStatusIcon = new VisualElement
            {
                pickingMode = PickingMode.Ignore
            };

            m_VersionNumber = new Label();
            m_VersionNumber.AddToClassList("asset-version");

            if (m_PageManager.ActivePage is UploadPage && settingsManager.IsDependencyVersionSelectionEnabled)
            {
                m_VersionNumber.visible = false;

                m_DependencyVersionButton = new Button();
                m_DependencyVersionButton.AddToClassList(k_DetailsPageDependencyButtonUssStyle);
                statusElement.Add(m_DependencyVersionButton);

                m_DependencyVersionButtonText = new TextElement();
                m_DependencyVersionButtonText.text = L10n.Tr(Constants.LoadingText);
                m_DependencyVersionButtonText.AddToClassList("unity-text-element");
                m_DependencyVersionButton.Add(m_DependencyVersionButtonText);

                var caret = new VisualElement();
                caret.AddToClassList(k_DetailsPageDependencyButtonCaretUssStyle);
                m_DependencyVersionButton.Add(caret);
            }
            else
            {
                statusElement.Add(m_VersionNumber);
            }

            statusElement.Add(m_ImportedStatusIcon);

            m_Button.Add(infoElement);
            m_Button.Add(statusElement);
            m_Button.SetEnabled(false);
        }

        ~DetailsPageDependencyItem()
        {
            m_CancellationTokenSource?.Dispose();
        }

        public async Task Bind(AssetIdentifier dependencyIdentifier)
        {
            // If the identifier is different, we need to reset the UI
            // otherwise we reload and rebind to new AssetData instance
            if (m_AssetIdentifier == null || m_AssetIdentifier != dependencyIdentifier)
            {
                m_FileName.text = "Loading...";
                m_Icon.style.backgroundImage = null;

                UIElementsUtils.SetDisplay(m_ImportedStatusIcon, false);
                UIElementsUtils.SetDisplay(m_VersionNumber, false);
            }

            m_Button.SetEnabled(false);

            var token = GetCancellationToken();

            m_AssetData = null;

            try
            {
                var assetData = await FetchAssetData(dependencyIdentifier, token);

                if (IsOffline()) // when offline or unity services are unreachable, we go back to default behaviour or showing local dependency
                {
                    m_AssetData = assetData;
                    m_FileName.text = assetData.Name;
                    m_AssetIdentifier = dependencyIdentifier;

                    RefreshUI();
                    return;
                }

                if (assetData is AssetData)
                {
                    // The local dependency vs the actual dependency might not be the same version.
                    // So we fetch the versions and select the correct one. This is both for the upload tab and the project detail tab
                    await assetData.RefreshVersionsAsync(CancellationToken.None);
                    m_AssetData = assetData.Versions
                        .FirstOrDefault(v => v.Identifier.Version == dependencyIdentifier.Version);
                }
                else
                {
                    m_AssetData = assetData;
                }

            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception)
            {
                Utilities.DevLog($"Dependency ({dependencyIdentifier.AssetId}) could not found for asset.");
            }

            m_AssetIdentifier = dependencyIdentifier;

            RefreshUI();

            await ResolveData(m_AssetData, token);
        }

        bool IsOffline()
        {
            var unityProxy = ServicesContainer.instance.Get<IUnityConnectProxy>();
            return !unityProxy.AreCloudServicesReachable;
        }

        async Task ResolveData(BaseAssetData assetData, CancellationToken token)
        {
            if (assetData == null)
                return;

            var tasks = new[]
            {
                assetData.RefreshAssetDataAttributesAsync(token),
                assetData.ResolveDatasetsAsync(token)
            };

            await Task.WhenAll(tasks);

            if (CanSelectVersion())
            {
                var uploadAssetData = (UploadAssetData)assetData;
                if ((uploadAssetData.Versions == null || !uploadAssetData.Versions.Any()) && !uploadAssetData.IsBeingAdded && !uploadAssetData.IsIgnored)
                    await uploadAssetData.RefreshVersionsAsync(CancellationToken.None);

                m_VersionsIds.Clear();
                m_VersionLabels = await m_ProjectOrganizationProvider.GetOrganizationVersionLabelsAsync();

                var versions = !uploadAssetData.IsBeingAdded ? uploadAssetData.Versions?.Where(v => v.SequenceNumber != 0)
                    .OrderByDescending(v => v.SequenceNumber).ToList() : new List<BaseAssetData>();
                if (versions != null)
                {
                    if (uploadAssetData.CanBeUploaded)
                    {
                        m_VersionsIds.Add(Constants.NewVersionText, AssetManagerCoreConstants.NewVersionId);
                    }

                    foreach (var version in versions)
                    {
                        var versionDisplay = $"Ver. {version.SequenceNumber}";
                        m_VersionsIds[versionDisplay] = version.Identifier.Version;
                    }

                    UpdateVersionSelectionDisplayText();
                    BuildVersionSelection();

                    m_DependencyVersionButton.RegisterCallback<ClickEvent>(evt =>
                    {
                        BuildVersionSelection();
                        m_PopupManager.Show(m_DependencyVersionButton, PopupContainer.PopupAlignment.BottomRight);
                    });
                }
            }

            // If by the time the tasks have completed, the target BaseAssetData has changed, don't continue
            if (m_AssetData != assetData)
                return;

            RefreshUI();
        }

        bool CanSelectVersion()
        {
            return m_PageManager.ActivePage is UploadPage && m_SettingsManager.IsDependencyVersionSelectionEnabled &&
                   (m_VersionsIds.Count == 0 || m_VersionLabels.Count == 0);
        }

        void RefreshUI()
        {
            m_Button.SetEnabled(m_AssetIdentifier != null);

            m_FileName.text = m_AssetData?.Name ?? $"{m_AssetIdentifier?.AssetId} (unavailable)";

            SetStatuses(m_AssetData?.AssetDataAttributeCollection.GetOverallStatus());

            m_Icon.style.backgroundImage = AssetDataTypeHelper.GetIconForExtension(m_AssetData?.PrimaryExtension);
            m_Icon.tooltip = m_AssetData?.PrimaryExtension;

            SetVersionNumber(m_AssetData);

            if (CanSelectVersion())
                UpdateVersionSelectionDisplayText();
        }

        void BuildVersionSelection()
        {
            m_PopupManager.Clear();
            var versionSelection = new ScrollView();

            var versionLabelTitle = new Label(L10n.Tr(Constants.VersionLabelSelectionTitle));
            versionLabelTitle.AddToClassList(UssStyle.k_FilterSectionLabel);
            versionSelection.Add(versionLabelTitle);

            foreach (var versionLabel in m_VersionLabels)
            {
                if (versionLabel == "Pending") // Skip the pending label (not allowed to be selected by user)
                    continue;

                var versionLabelSelection = new TextElement();
                versionLabelSelection.AddToClassList(UssStyle.k_FilterItemSelection);
                versionLabelSelection.text = versionLabel;
                versionLabelSelection.RegisterCallback<ClickEvent>(evt =>
                {
                    evt.StopPropagation();

                    m_AssetIdentifier.Version = AssetManagerCoreConstants.NewVersionId;
                    m_AssetIdentifier.VersionLabel = versionLabel;

                    m_DependencyVersionButtonText.text = versionLabel;
                    m_PopupManager.Hide();
                });
                versionSelection.Add(versionLabelSelection);
            }

            var line = new VisualElement();
            line.AddToClassList(UssStyle.k_FilterSeparatorLine);
            versionSelection.Add(line);

            var fixedVersionTitle = new Label(L10n.Tr(Constants.FixedVersionSelectionTitle));
            fixedVersionTitle.AddToClassList(UssStyle.k_FilterSectionLabel);
            versionSelection.Add(fixedVersionTitle);

            foreach (var versionNumber in m_VersionsIds.Keys)
            {
                var versionLabelSelection = new TextElement();
                versionLabelSelection.AddToClassList(UssStyle.k_FilterItemSelection);
                versionLabelSelection.text = versionNumber;
                versionLabelSelection.RegisterCallback<ClickEvent>(evt =>
                {
                    evt.StopPropagation();

                    m_AssetIdentifier.VersionLabel = "";
                    m_AssetIdentifier.Version = versionNumber == Constants.NewVersionText ? AssetManagerCoreConstants.NewVersionId : m_VersionsIds[versionNumber];

                    m_DependencyVersionButtonText.text = versionNumber;
                    m_PopupManager.Hide();
                });
                versionSelection.Add(versionLabelSelection);
            }

            m_PopupManager.Container.Add(versionSelection);
        }

        void UpdateVersionSelectionDisplayText()
        {
            if(!string.IsNullOrEmpty(m_AssetIdentifier.VersionLabel))
            {
                m_DependencyVersionButtonText.text = m_AssetIdentifier.VersionLabel;
            }
            else
            {
                var versionDisplayText = m_VersionsIds.FirstOrDefault(versionDisplayIds =>
                    versionDisplayIds.Value == m_AssetIdentifier.Version).Key;

                if (string.IsNullOrEmpty(versionDisplayText)) // that means it's trying to select "new version" but it's not present
                {
                    versionDisplayText = m_VersionsIds.FirstOrDefault().Key ?? string.Empty;
                }

                m_DependencyVersionButtonText.text = versionDisplayText;
            }
        }

        CancellationToken GetCancellationToken()
        {
            if (m_CancellationTokenSource != null)
            {
                m_CancellationTokenSource.Cancel();
                m_CancellationTokenSource.Dispose();
            }

            m_CancellationTokenSource = new CancellationTokenSource();
            return m_CancellationTokenSource.Token;
        }

        async Task<BaseAssetData> FetchAssetData(AssetIdentifier identifier, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var assetDataManager = ServicesContainer.instance.Resolve<IAssetDataManager>();

            // First, if the asset is imported, we want to display the imported version
            var info = assetDataManager.GetImportedAssetInfo(identifier);

            if (info != null)
            {
                return info.AssetData;
            }

            // If not, try to get it from the AssetDataManager cache
            // The AssetDataManager cache can only contain one version per asset (limitation or design choice?)
            // So we need to make sure the version returned is the one we need.
            var assetData = assetDataManager.GetAssetData(identifier);

            if (assetData != null && (assetData.Identifier == identifier || m_PageManager.ActivePage is UploadPage && assetData.Identifier.AssetId == identifier.AssetId))
            {
                return assetData;
            }

            // Otherwise fetch the asset from the server
            var assetsProvider = ServicesContainer.instance.Resolve<IAssetsProvider>();
            assetData = await assetsProvider.GetAssetAsync(identifier, token);

            token.ThrowIfCancellationRequested();

            return assetData;
        }

        // Common code between this and the AssetPreview class
        void SetStatuses(IEnumerable<AssetPreview.IStatus> statuses)
        {
            if (statuses == null)
            {
                UIElementsUtils.SetDisplay(m_ImportedStatusIcon, false);
                return;
            }

            var validStatuses = statuses.Where(s => s != null).ToList();
            validStatuses.Remove(AssetDataStatus.Linked); // Hack : we need to extract the linked from the status and have only one status instead of a list
            var hasStatuses = validStatuses.Any();
            UIElementsUtils.SetDisplay(m_ImportedStatusIcon, hasStatuses);

            m_ImportedStatusIcon.Clear();

            if (!hasStatuses)
                return;

            foreach (var status in validStatuses)
            {
                var statusElement = status.CreateVisualTree();
                statusElement.tooltip = L10n.Tr(status.Description);
                statusElement.style.position = Position.Relative;
                m_ImportedStatusIcon.Add(statusElement);
            }
        }

        // Common code between this and the AssetDetailsHeader class
        void SetVersionNumber(BaseAssetData assetData)
        {
            UIElementsUtils.SetDisplay(m_VersionNumber, assetData != null);
            if (assetData == null)
                return;

            UIElementsUtils.SetSequenceNumberText(m_VersionNumber, assetData, m_AssetIdentifier.VersionLabel);
        }
    }
}
