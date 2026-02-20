using System;
using Unity.AssetManager.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace Unity.AssetManager.UI.Editor
{
    [Serializable]
    class AllAssetsInProjectPage : InProjectPage
    {
        public AllAssetsInProjectPage(IAssetDataManager assetDataManager, IAssetsProvider assetsProvider,
            IProjectOrganizationProvider projectOrganizationProvider, IMessageManager messageManager,
            IPageManager pageManager, IDialogManager dialogManager)
            : base(assetDataManager, assetsProvider, projectOrganizationProvider, messageManager, pageManager,
                dialogManager) { }

        public override bool DisplayBreadcrumbs => false;
        public override string Title => L10n.Tr(Constants.AllAssetsInProjectTitle);

        public override void OnActivated()
        {
            base.OnActivated();

            m_ProjectOrganizationProvider.SelectProject(string.Empty);
        }

        protected override void OnProjectSelectionChanged(ProjectOrLibraryInfo projectOrLibraryInfo, CollectionInfo collectionInfo)
        {
            if (projectOrLibraryInfo == null)
                return;

            m_PageManager.SetActivePage<InProjectPage>();
        }
    }
}
