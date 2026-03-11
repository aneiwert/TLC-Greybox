using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Unity.AssetManager.Core.Editor;
using Unity.AssetManager.Upload.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AssetManager.UI.Editor
{
    static partial class UssStyle
    {
        public const string DetailsPageEntriesContainer = "details-page-entries-container";
        public const string DetailsPageThumbnailContainer = "details-page-thumbnail-container";
        public const string ImageContainer = "image-container";
    }

    class AssetDetailsTab : AssetTab, IEditableComponent
    {
        const string k_FileSizeName = "file-size";
        const string k_FileCountName = "file-count";

        readonly AssetPreview m_AssetPreview;
        readonly VisualElement m_OutdatedWarningBox;
        readonly VisualElement m_EntriesContainer;
        readonly IPageManager m_PageManager;
        readonly IStateManager m_StateManager;
        readonly IPopupManager m_PopupManager;
        readonly ISettingsManager m_SettingsManager;
        readonly IProjectOrganizationProvider m_projectOrganizationProvider;
        readonly IAssetDataManager m_AssetDataManager;

        BaseAssetData m_AssetData;

        public override AssetDetailsPageTabs.TabType Type => AssetDetailsPageTabs.TabType.Details;
        public override bool IsFooterVisible => true;
        public override bool EnabledWhenDisconnected => true;
        public override VisualElement Root { get; }

        readonly Func<bool> m_IsFilterActive;

        AssetDependenciesComponent m_DependenciesComponent;

        List<IEditableEntry> m_EditableEntries = new();

        public bool IsEditingEnabled { get; private set; }
        public event Action<AssetFieldEdit> FieldEdited;

        public AssetDetailsTab(VisualElement visualElement, Func<bool> isFilterActive = null, IPageManager pageManager = null, IStateManager stateManager = null, IPopupManager popupManager = null, ISettingsManager settingsManager = null, IProjectOrganizationProvider projectOrganizationProvider = null, IAssetDataManager assetDataManager = null)
        {
            var root = visualElement.Q("details-page-content-container");
            Root = root;
            m_PageManager = pageManager;
            m_StateManager = stateManager;
            m_PopupManager = popupManager;
            m_SettingsManager = settingsManager;
            m_projectOrganizationProvider = projectOrganizationProvider;
            m_AssetDataManager = assetDataManager;

            m_EntriesContainer = new VisualElement();
            m_EntriesContainer.AddToClassList(UssStyle.DetailsPageEntriesContainer);
            root.Add(m_EntriesContainer);
            m_EntriesContainer.SendToBack();

            m_OutdatedWarningBox = root.Q("outdated-warning-box");
            m_OutdatedWarningBox.Q<Label>().text = L10n.Tr(Constants.FilteredAssetOutdatedWarning);
            m_OutdatedWarningBox.SendToBack();

            m_AssetPreview = new AssetPreview {name = "details-page-asset-preview"};
            m_AssetPreview.AddToClassList(UssStyle.DetailsPageThumbnailContainer);
            m_AssetPreview.AddToClassList(UssStyle.ImageContainer);

            m_IsFilterActive = isFilterActive;
        }

        public override void OnSelection(BaseAssetData assetData)
        {
            m_AssetPreview.ClearPreview();
            m_AssetData = assetData;
            // TODO: This class should have been caching the selected data.
            // We need to clean up all the methods that accept new data as parameters...
            // Alternatively we could keep reference to the AssetDataManager
        }

        public override void RefreshUI(BaseAssetData assetData, bool isLoading = false)
        {
            // Remove asset preview from hierarchy to avoid it being destroyed when clearing the container
            m_AssetPreview.RemoveFromHierarchy();

            // TODO: Clearing/rebuilding all fields here means we cannot maintain any focus state
            // or values in any of the editable fields. Should be refactored to update existing fields
            m_EntriesContainer.Clear();
            m_EditableEntries.Clear();

            m_AssetPreview.SetAssetType(assetData.PrimaryExtension);
            m_EntriesContainer.Add(m_AssetPreview);
            SetPreviewImage(assetData.Thumbnail);

            UpdatePreviewStatus(AssetDataStatus.GetOverallStatus(assetData.AssetDataAttributeCollection));
            UpdateStatusWarning(assetData.AssetDataAttributeCollection);

            AddSpace(m_EntriesContainer);

            if (!string.IsNullOrWhiteSpace(assetData.Description) || IsEditingEnabled)
            {
                var assetId = (assetData as UploadAssetData)?.ExistingAssetIdentifier?.AssetId ?? assetData.Identifier.AssetId;
                var descriptionEntry = AddEditableText(m_EntriesContainer, assetId, Constants.DescriptionText, assetData.Description);
                descriptionEntry.EntryEdited += OnDescriptionEdited;
                descriptionEntry.IsEntryEdited += IsDescriptionEdited;
                descriptionEntry.EnableEditing(IsEditingEnabled);
                m_EditableEntries.Add(descriptionEntry);
            }

            if (!string.IsNullOrWhiteSpace(assetData.Status))
            {
                var assetId = (assetData as UploadAssetData)?.ExistingAssetIdentifier?.AssetId ?? assetData.Identifier.AssetId;
                var statusEntry = AddEditableStatusDropdown(m_EntriesContainer, assetId, Constants.StatusText, assetData.Status, assetData.ReachableStatusNames);
                statusEntry.EntryEdited += OnStatusEdited;
                statusEntry.IsEntryEdited += IsStatusEdited;
                statusEntry.EnableEditing(IsEditingEnabled);
                m_EditableEntries.Add(statusEntry);
            }

            var projectIds = GetProjectIdsDisplayList(assetData.Identifier.ProjectId, assetData.LinkedProjects);
            var projectEntryTitle = projectIds.Length > 1 ? Constants.ProjectsText : Constants.ProjectText;
            AddProjectChips(m_EntriesContainer, projectEntryTitle, projectIds, "entry-project");
            AddCollectionChips(m_EntriesContainer, Constants.CollectionsText, assetData.LinkedCollections);

            if (assetData.Tags.Any() || IsEditingEnabled)
            {
                var assetId = (assetData as UploadAssetData)?.ExistingAssetIdentifier?.AssetId ?? assetData.Identifier.AssetId;
                var tagsEntry = AddEditableTagList(m_EntriesContainer, assetId, Constants.TagsText, assetData.Tags);
                tagsEntry.EntryEdited += OnTagsEdited;
                tagsEntry.IsEntryEdited += AreTagsEdited;
                tagsEntry.EnableEditing(IsEditingEnabled);
                m_EditableEntries.Add(tagsEntry);
            }

            m_DependenciesComponent = new AssetDependenciesComponent(m_EntriesContainer, m_PageManager, m_PopupManager, m_SettingsManager, m_projectOrganizationProvider ,m_StateManager);
            m_DependenciesComponent.RefreshUI(assetData, isLoading);
            DisplayMetadata(assetData);

            AddText(m_EntriesContainer, Constants.FilesSizeText, "-", isSelectable:false, k_FileSizeName);
            AddText(m_EntriesContainer, Constants.TotalFilesText, "-", isSelectable:false, k_FileCountName);

            // Temporary solution to display targeted assets during re-upload. Very handy to understand which assets the system is re-uploading to.
            var identifier = assetData.Identifier;
            if (assetData is UploadAssetData uploadAssetData)
            {
                identifier = uploadAssetData.TargetAssetIdentifier ?? identifier;
            }
            AddAssetIdentifier(m_EntriesContainer, Constants.AssetIdText, identifier);

            var assetsProvider = ServicesContainer.instance.Resolve<IAssetsProvider>();
            AddText(m_EntriesContainer, Constants.AssetTypeText, assetsProvider.GetValueAsString(assetData.AssetType), isSelectable: true);

            AddText(m_EntriesContainer, Constants.LastModifiedText, Utilities.DatetimeToString(assetData.Updated));
            AddUser(m_EntriesContainer, Constants.LastEditByText, assetData.UpdatedBy, typeof(UpdatedByFilter));
            AddText(m_EntriesContainer, Constants.UploadDateText, Utilities.DatetimeToString(assetData.Created));
            AddUser(m_EntriesContainer, Constants.CreatedByText, assetData.CreatedBy, typeof(CreatedByFilter));

            if (isLoading)
            {
                AddLoadingText(m_EntriesContainer);
            }
        }

        void DisplayMetadata(BaseAssetData assetData)
        {
            // Dot not display metadata for local assets (a.k.a UploadAssetData)
            if (assetData.Identifier.IsLocal())
                return;

            foreach (var metadata in assetData.Metadata)
            {
                switch (metadata.Type)
                {
                    case MetadataFieldType.Text:
                    {
                        var textMetadata = (Core.Editor.TextMetadata)metadata;
                        AddText(m_EntriesContainer, textMetadata.Name, textMetadata.Value, isSelectable: true);
                        break;
                    }
                    case MetadataFieldType.Boolean:
                    {
                        var booleanMetadata = (Core.Editor.BooleanMetadata)metadata;
                        AddToggle(m_EntriesContainer, booleanMetadata.Name, booleanMetadata.Value);
                        break;
                    }
                    case MetadataFieldType.Number:
                    {
                        var numberMetadata = (Core.Editor.NumberMetadata)metadata;
                        AddText(m_EntriesContainer, numberMetadata.Name,
                            numberMetadata.Value.ToString(CultureInfo.CurrentCulture), isSelectable: true);
                        break;
                    }
                    case MetadataFieldType.Timestamp:
                    {
                        var timestampMetadata = (Core.Editor.TimestampMetadata)metadata;
                        AddText(m_EntriesContainer, timestampMetadata.Name,
                            Utilities.DatetimeToString(timestampMetadata.Value.DateTime), isSelectable: true);
                        break;
                    }
                    case MetadataFieldType.Url:
                    {
                        var urlMetadata = (Core.Editor.UrlMetadata)metadata;
                        AddText(m_EntriesContainer, urlMetadata.Name,
                            urlMetadata.Value.Uri == null ? string.Empty : urlMetadata.Value.Uri.ToString(), isSelectable: true);
                        break;
                    }
                    case MetadataFieldType.User:
                    {
                        var userMetadata = (Core.Editor.UserMetadata)metadata;
                        AddUser(m_EntriesContainer, metadata.Name, userMetadata.Value, null);
                        break;
                    }
                    case MetadataFieldType.SingleSelection:
                    {
                        var singleSelectionMetadata = (Core.Editor.SingleSelectionMetadata)metadata;
                        AddSelectionChips(m_EntriesContainer, metadata.Name, new List<string> {singleSelectionMetadata.Value}, isSelectable: true);
                        break;
                    }
                    case MetadataFieldType.MultiSelection:
                    {
                        var multiSelectionMetadata = (Core.Editor.MultiSelectionMetadata)metadata;
                        AddSelectionChips(m_EntriesContainer, metadata.Name, multiSelectionMetadata.Value, isSelectable: true);
                        break;
                    }
                    default:
                        throw new InvalidOperationException("Unexpected metadata field type was encountered.");
                }
            }
        }

        public override void RefreshButtons(UIEnabledStates enabled, BaseAssetData assetData, BaseOperation operationInProgress)
        {
            m_EntriesContainer.Q("entry-project")?.SetEnabled(enabled.HasFlag(UIEnabledStates.ServicesReachable));
        }

        public void UpdatePreviewStatus(IEnumerable<AssetPreview.IStatus> status)
        {
            m_AssetPreview.SetStatuses(status);
        }

        public void UpdateDependencyComponent(BaseAssetData assetData)
        {
            m_DependenciesComponent.RefreshUI(assetData, false);
        }

        public void UpdateStatusWarning(AssetDataAttributeCollection assetDataAttributeCollection)
        {
            // If the selected asset is outdated and selected via filtered results, we want to warn
            // the user that the asset may not be displaying all up-to-date information.

            var status = assetDataAttributeCollection?.GetAttribute<ImportAttribute>()?.Status;

            var filterActive = m_IsFilterActive?.Invoke() ?? false;
            var displayOutdatedWarning = filterActive && status == ImportAttribute.ImportStatus.OutOfDate;
            UIElementsUtils.SetDisplay(m_OutdatedWarningBox, displayOutdatedWarning);
        }

        public void SetPreviewImage(Texture2D texture)
        {
            m_AssetPreview.SetThumbnail(texture);
        }

        public void SetFileCount(string fileCount)
        {
            m_EntriesContainer.Q<DetailsPageEntry>(k_FileCountName)?.SetText(fileCount);
        }

        public void SetFileSize(string fileSize)
        {
            m_EntriesContainer.Q<DetailsPageEntry>(k_FileSizeName)?.SetText(fileSize);
        }

        public void SetPrimaryExtension(string primaryExtension)
        {
            m_AssetPreview.SetAssetType(primaryExtension);
        }

        static string[] GetProjectIdsDisplayList(string currentProjectId, IEnumerable<ProjectIdentifier> linkedProjects)
        {
            // Ensure the current project is first in the list
            var uniqueProjectIds = new HashSet<string>() { currentProjectId };
            foreach (var linkedProject in linkedProjects)
                uniqueProjectIds.Add(linkedProject.ProjectId);

            return uniqueProjectIds.ToArray();
        }

        void OnDescriptionEdited(object editValue)
        {
            OnEntryEdited(EditField.Description, editValue);
        }

        void OnTagsEdited(object editValue)
        {
            OnEntryEdited(EditField.Tags, editValue);
        }

        void OnStatusEdited(object editValue)
        {
            OnEntryEdited(EditField.Status, editValue);
        }

        bool IsDescriptionEdited(string assetId, object description)
        {
            var importedAssetData = m_AssetDataManager.GetImportedAssetInfo(assetId);
            return !string.Equals(importedAssetData?.AssetData?.Description, description as string, StringComparison.Ordinal);
        }

        bool AreTagsEdited(string assetId, object tags)
        {
            var importedAssetData = m_AssetDataManager.GetImportedAssetInfo(assetId);
            var tagsCollection = tags as IEnumerable<string> ?? Enumerable.Empty<string>();
            var importedTags = importedAssetData?.AssetData?.Tags ?? Enumerable.Empty<string>();

            return !importedTags.SequenceEqual(tagsCollection);
        }

        bool IsStatusEdited(string assetId, object value)
        {
            var importedAssetData = m_AssetDataManager.GetImportedAssetInfo(assetId);
            return !string.Equals(importedAssetData?.AssetData?.Status, value as string, StringComparison.Ordinal);
        }

        void OnEntryEdited(EditField fieldType, object editValue)
        {
            var edit = new AssetFieldEdit(m_AssetData.Identifier, fieldType, editValue);
            FieldEdited?.Invoke(edit);
        }

        public void EnableEditing(bool enable)
        {
            foreach (var editableEntry in m_EditableEntries)
            {
                editableEntry.EnableEditing(enable);
            }
            IsEditingEnabled = enable;
        }
    }
}
