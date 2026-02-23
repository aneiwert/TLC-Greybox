using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.AssetManager.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace Unity.AssetManager.UI.Editor
{
    [Serializable]
    class AllAssetsPage : BasePage
    {
        public AllAssetsPage(IAssetDataManager assetDataManager, IAssetsProvider assetsProvider,
            IProjectOrganizationProvider projectOrganizationProvider, IMessageManager messageManager,
            IPageManager pageManager, IDialogManager dialogManager)
            : base(assetDataManager, assetsProvider, projectOrganizationProvider, messageManager, pageManager,
                dialogManager) { }

        public override bool DisplayBreadcrumbs => true;

        public override void OnActivated()
        {
            base.OnActivated();

            m_ProjectOrganizationProvider.SelectProject(string.Empty);
        }

        protected internal override async IAsyncEnumerable<BaseAssetData> LoadMoreAssets(
            [EnumeratorCancellation] CancellationToken token)
        {
            if (m_ProjectOrganizationProvider?.SelectedOrganization == null)
                yield break;

            await foreach (var assetData in LoadMoreAssets(m_ProjectOrganizationProvider.SelectedOrganization, token))
            {
                yield return assetData;
            }
        }

        protected override void OnLoadMoreSuccessCallBack()
        {
            if (!CheckConnection())
                return;

            if (!AssetList.Any())
            {
                if (!TrySetNoResultsPageMessage())
                {
                    SetPageMessage(Messages.EmptyAllAssetsMessage);
                }
            }
            else
            {
                m_PageFilterStrategy.EnableFilters();

                m_MessageManager.ClearAllMessages();
            }
        }

        protected override void OnProjectSelectionChanged(ProjectOrLibraryInfo projectOrLibraryInfo, CollectionInfo collectionInfo)
        {
            if (projectOrLibraryInfo == null)
                return;

            m_PageManager.SetActivePage<CollectionPage>();
        }
    }
}
