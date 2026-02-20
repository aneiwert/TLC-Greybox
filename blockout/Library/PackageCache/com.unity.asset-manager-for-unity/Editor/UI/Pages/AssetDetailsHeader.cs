using System;
using Unity.AssetManager.Core.Editor;
using Unity.AssetManager.Upload.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AssetManager.UI.Editor
{
    class AssetDetailsHeader : IPageComponent, IEditableComponent
    {
        readonly VisualElement m_BorderLine;
        readonly Label m_AssetName;
        readonly Label m_AssetVersion;
        readonly Image m_AssetDashboardLink;

        TextField m_TextField;

        IAssetDataManager m_AssetDataManager;
        BaseAssetData m_AssetData;

        public bool IsEditingEnabled { get; private set; }

        public event Action OpenDashboard;
        public event Func<bool> CanOpenDashboard;
        public event Action<AssetFieldEdit> FieldEdited;

        public AssetDetailsHeader(VisualElement visualElement, IAssetDataManager assetDataManager)
        {
            m_AssetDataManager  = assetDataManager;

            m_BorderLine = visualElement.Q<VisualElement>("asset-name-borderline");
            m_BorderLine.AddToClassList("asset-entry-borderline-style");

            m_AssetName = visualElement.Q<Label>("asset-name");
            m_AssetName.selection.isSelectable = true;
            m_AssetVersion = visualElement.Q<Label>("asset-version");

            m_TextField = visualElement.Q<TextField>("asset-name-edit-field");
            m_TextField.RegisterCallback<KeyUpEvent>(OnKeyUpEvent);
            m_TextField.RegisterCallback<FocusOutEvent>(_ => OnEntryEdited(m_TextField.value));
            m_TextField.style.display = DisplayStyle.None;

            m_AssetDashboardLink = visualElement.Q<Image>("asset-dashboard-link");
            m_AssetDashboardLink.tooltip = L10n.Tr(Constants.DashboardLinkTooltip);
            m_AssetDashboardLink.RegisterCallback<ClickEvent>(_ =>
            {
                OpenDashboard?.Invoke();
            });

        }

        public void OnSelection(BaseAssetData assetData) { }

        public void RefreshUI(BaseAssetData assetData, bool isLoading = false)
        {
            m_AssetData = assetData;
            m_AssetName.text = assetData.Name;
            m_TextField.value = assetData.Name;

            UIElementsUtils.SetSequenceNumberText(m_AssetVersion, assetData);
            UIElementsUtils.SetDisplay(m_AssetDashboardLink, HasValidLink(assetData.Identifier));

            UpdateStyling();
        }

        public void RefreshButtons(UIEnabledStates enabled, BaseAssetData assetData, BaseOperation operationInProgress)
        {
            m_AssetDashboardLink.SetEnabled(enabled.HasFlag(UIEnabledStates.ServicesReachable));
        }

        bool HasValidLink(AssetIdentifier assetIdentifier)
        {
            if (assetIdentifier.IsAssetFromLibrary())
                return true;

            return (CanOpenDashboard?.Invoke() ?? false) &&
                !(string.IsNullOrEmpty(assetIdentifier.OrganizationId) ||
                    string.IsNullOrEmpty(assetIdentifier.ProjectId) ||
                    string.IsNullOrEmpty(assetIdentifier.AssetId));
        }

        public void EnableEditing(bool enable)
        {
            if (enable == IsEditingEnabled)
                return;

            m_TextField.value = m_AssetName.text;

            m_AssetName.style.display = enable ? DisplayStyle.None : DisplayStyle.Flex;
            m_TextField.style.display = enable ? DisplayStyle.Flex : DisplayStyle.None;

            IsEditingEnabled = enable;
        }

        void OnKeyUpEvent(KeyUpEvent evt)
        {
            if (evt.keyCode is KeyCode.Return or KeyCode.KeypadEnter)
                OnEntryEdited(m_TextField.value);
        }

        void OnEntryEdited(string newValue)
        {
            if (newValue == m_AssetName.text)
                return;

            // Empty values are not supported
            if (string.IsNullOrWhiteSpace(newValue))
            {
                m_TextField.value = m_AssetName.text;
                return;
            }

            m_AssetName.text = newValue;

            var fieldEdit = new AssetFieldEdit(m_AssetData.Identifier, EditField.Name, newValue);
            FieldEdited?.Invoke(fieldEdit);

            UpdateStyling();
        }

        void UpdateStyling()
        {
            var isEdited = IsNameEdited(m_AssetName.text);
            if (isEdited)
            {
                m_BorderLine.style.backgroundColor = UssStyle.EditedBorderColor;
                m_TextField.AddToClassList(UssStyle.DetailsPageEntryValueEdited);
            }
            else
            {
                m_BorderLine.style.backgroundColor = Color.clear;
                m_TextField.RemoveFromClassList(UssStyle.DetailsPageEntryValueEdited);
            }
        }

        bool IsNameEdited(string name)
        {
            var uploadAssetData = m_AssetData as UploadAssetData;
            if (uploadAssetData != null)
            {
                var assetId = (m_AssetData as UploadAssetData)?.ExistingAssetIdentifier?.AssetId ?? m_AssetData.Identifier.AssetId;
                var importedAssetData = m_AssetDataManager.GetImportedAssetInfo(assetId);
                return !string.Equals(importedAssetData?.AssetData?.Name, name, StringComparison.Ordinal);
            }
            return false;
        }
    }
}
