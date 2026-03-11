using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.AssetManager.Core.Editor;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.AssetManager.UI.Editor
{
    static partial class UssStyle
    {
        public const string UpdateAllButtonContainer = "unity-update-all-button-container";
        public const string UpdateAllButtonIcon = "unity-update-all-button-icon";
    }

    class UpdateAllButton : GridTool
    {
        readonly Button m_UpdateAllButton;
        readonly VisualElement m_Icon;
        readonly IAssetDataManager m_AssetDataManager;
        readonly HashSet<BaseAssetData> m_TrackedAssets = new();
        readonly IAssetsProvider m_AssetsProvider;
        readonly IApplicationProxy m_ApplicationProxy;
        
        CancellationTokenSource m_UpdateStatusCancellationTokenSource = new();

        bool IsAvailable => m_PageManager.ActivePage?.SupportsUpdateAll ?? false;

        public UpdateAllButton(IAssetImporter assetImporter, IPageManager pageManager, IProjectOrganizationProvider projectOrganizationProvider, IAssetsProvider assetsProvider, IApplicationProxy applicationProxy)
            : base(pageManager, projectOrganizationProvider)
        {
            m_UpdateAllButton = new Button(() =>
            {
                var project = projectOrganizationProvider.SelectedProjectOrLibrary;
                var collection = projectOrganizationProvider.SelectedCollection;

                TaskUtils.TrackException(assetImporter.UpdateAllToLatestAsync(ImportTrigger.UpdateAllToLatest, project, collection, CancellationToken.None));

                AnalyticsSender.SendEvent(new UpdateAllLatestButtonClickEvent());
            });
            Add(m_UpdateAllButton);

            var container = new VisualElement();
            container.AddToClassList(UssStyle.UpdateAllButtonContainer);
            m_UpdateAllButton.Add(container);

            m_Icon = new VisualElement();
            m_Icon.AddToClassList(UssStyle.UpdateAllButtonIcon);

            m_AssetDataManager = ServicesContainer.instance.Resolve<IAssetDataManager>();

            m_AssetDataManager.AssetDataChanged += OnAssetDataChanged;
            m_AssetDataManager.ImportedAssetInfoChanged += OnAssetDataChanged;

            EnableButton(false);

            container.Add(m_Icon);

            m_AssetsProvider = assetsProvider;
            m_ApplicationProxy = applicationProxy;

            tooltip = L10n.Tr(Constants.UpdateAllButtonTooltip);
        }

        protected override void OnActivePageChanged(IPage page)
        {
            // Don't enable the button until loading 
            EnableButton(false);
        }

        protected override void InitDisplay(IPage page)
        {
            base.InitDisplay(page);
            _ = UpdateButtonStatus();
        }

        public void Refresh()
        {
            _ = UpdateButtonStatus();
        }

        async Task UpdateButtonStatus()
        {
            m_UpdateStatusCancellationTokenSource?.Cancel();
            m_UpdateStatusCancellationTokenSource?.Dispose();
            m_UpdateStatusCancellationTokenSource = null;

            // Only update the button status if the button is available for the current page and the application is reachable
            if (!IsAvailable || !m_ApplicationProxy.InternetReachable)
            {
                EnableButton(false);
                return;
            }

            m_UpdateStatusCancellationTokenSource = new CancellationTokenSource();
            var token = m_UpdateStatusCancellationTokenSource.Token;

            // Unsubscribe from assets to be cleared
            foreach (var asset in m_TrackedAssets)
            {
                asset.AssetDataChanged -= OnAssetDataAttributesChanged;
            }

            m_TrackedAssets.Clear();

            IEnumerable<BaseAssetData> importedAssets = new List<BaseAssetData>();
            try
            {
                // Get the relevant imported assets for the current page
                if (m_ProjectOrganizationProvider.SelectedAssetLibrary == null)
                    importedAssets = await GetImportedAssetsForCurrentPage(token);
            }
            catch (OperationCanceledException)
            {
                // Don't log cancellation exceptions
                return;
            }

            if (!importedAssets.Any())
            {
                EnableButton(false);
                return;
            }

            // Subscribe to imported assets
            foreach (var asset in importedAssets)
            {
                asset.AssetDataChanged += OnAssetDataAttributesChanged;
                m_TrackedAssets.Add(asset);
            }

            CheckAssetsStatus(importedAssets);
        }

        async void OnAssetDataChanged(AssetChangeArgs obj)
        {
            // Wait for the UI to update before processing the asset data changes
            await Task.Yield();
            await UpdateButtonStatus();
        }

        async Task<IEnumerable<BaseAssetData>> GetImportedAssetsForCurrentPage(CancellationToken token)
        {
            var assets = new List<BaseAssetData>();

            // Populate the tracked assets with all imported assets based on local filtering
            var localFilters = m_PageManager.PageFilterStrategy.SelectedLocalFilters;
            if (localFilters != null && localFilters.Any())
            {
                // Filter the imported assets based on the local filters
                var filteredAssets = await FilteringUtils.GetFilteredImportedAssets(m_AssetDataManager.ImportedAssetInfos, localFilters, token);
                assets.AddRange(filteredAssets.Select(info => info.AssetData));
            }
            else
            {
                // If no local filters, use all imported assets
                assets.AddRange(m_AssetDataManager.ImportedAssetInfos.Select(info => info.AssetData));
            }

            return await FilterByLinkedProjectsAndCollections(assets, token);
        }

        async Task<IEnumerable<BaseAssetData>> FilterByLinkedProjectsAndCollections(IEnumerable<BaseAssetData> assetDatas, CancellationToken token)
        {
            await FilteringUtils.UpdateLinkedProjectsAndCollectionsForSelectionAsync(m_ProjectOrganizationProvider, m_AssetsProvider, assetDatas, token);

            var selectedProjectId = m_ProjectOrganizationProvider.SelectedProjectOrLibrary?.Id;
            if (!string.IsNullOrEmpty(selectedProjectId))
            {
                var selectedCollection = m_ProjectOrganizationProvider.SelectedCollection;
                if (selectedCollection != null && !string.IsNullOrEmpty(selectedCollection.Name))
                {
                    // Filter imported assets by linked collection
                    var selectedCollectionFullPath = selectedCollection.GetFullPath();
                    assetDatas = assetDatas.Where(x =>
                        x.LinkedCollections.Any(c =>
                            c.ProjectIdentifier.ProjectId == selectedProjectId
                            && c.CollectionPath == selectedCollectionFullPath));
                }
                else
                {
                    // Filter imported assets by imported project or linked project
                    assetDatas = assetDatas.Where(x => x.Identifier.ProjectId == selectedProjectId
                        || x.LinkedProjects.Any(p => p.ProjectId == selectedProjectId));
                }
            }

            return assetDatas;
        }

        void OnAssetDataAttributesChanged(BaseAssetData asset, AssetDataEventType changeType)
        {
            if (IsAvailable && changeType == AssetDataEventType.AssetDataAttributesChanged)
            {
                // Enable the button if the asset has an import attribute that is out of date
                var importAttribute = asset.AssetDataAttributeCollection?.GetAttribute<ImportAttribute>();
                if (importAttribute != null && importAttribute.Status == ImportAttribute.ImportStatus.OutOfDate)
                {
                    EnableButton(true);
                }
            }
        }

        void CheckAssetsStatus(IEnumerable<BaseAssetData> assets)
        {
            // Reset the button state, only enable if we find an asset that is out of date
            EnableButton(false);

            // Only update the button status if the button is available for the current page
            if (!IsAvailable || assets == null || !assets.Any())
                return;

            foreach (var asset in assets)
            {
                var importAttribute = asset.AssetDataAttributeCollection?.GetAttribute<ImportAttribute>();
                if (importAttribute != null && importAttribute.Status == ImportAttribute.ImportStatus.OutOfDate)
                {
                    EnableButton(true);
                    return;
                }
            }
        }

        ~UpdateAllButton()
        {
            foreach (var asset in m_TrackedAssets)
            {
                asset.AssetDataChanged -= OnAssetDataAttributesChanged;
            }

            m_TrackedAssets.Clear();

            m_AssetDataManager.AssetDataChanged -= OnAssetDataChanged;
            m_AssetDataManager.ImportedAssetInfoChanged -= OnAssetDataChanged;
        }

        void EnableButton(bool enable)
        {
            m_UpdateAllButton.visible = IsAvailable;

            m_Icon.RemoveFromClassList(enable ? "inactive" : "active");
            m_Icon.AddToClassList(enable ? "active" : "inactive");
            m_UpdateAllButton.SetEnabled(enable);
        }

        protected override bool IsDisplayed(IPage page)
        {
            if (page is BasePage basePage)
            {
                return basePage.DisplayUpdateAllButton;
            }

            return base.IsDisplayed(page);
        }
    }
}
