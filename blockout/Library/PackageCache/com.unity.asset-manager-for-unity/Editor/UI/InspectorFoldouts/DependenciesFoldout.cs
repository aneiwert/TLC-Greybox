using System;
using Unity.AssetManager.Core.Editor;
using UnityEngine.UIElements;

namespace Unity.AssetManager.UI.Editor
{
    class DependenciesFoldout : ItemFoldout<AssetIdentifier, DetailsPageDependencyItem>
    {
        readonly IPageManager m_PageManager;
        readonly IPopupManager m_PopupManager;
        readonly ISettingsManager m_SettingsManager;
        readonly IProjectOrganizationProvider m_ProjectOrganizationProvider;

        public DependenciesFoldout(VisualElement parent, string foldoutTitle, IPageManager pageManager, IPopupManager popupManager, ISettingsManager settingsManager, IProjectOrganizationProvider projectOrganizationProvider)
            : base(parent, foldoutTitle, "dependencies-foldout", "dependencies-list", "details-files-foldout", "details-files-list")
        {
            m_PageManager = pageManager;
            m_PopupManager = popupManager;
            m_SettingsManager = settingsManager;
            m_ProjectOrganizationProvider = projectOrganizationProvider;
        }

        protected override DetailsPageDependencyItem MakeItem()
        {
            return new DetailsPageDependencyItem(m_PageManager, m_PopupManager, m_SettingsManager, m_ProjectOrganizationProvider);
        }

        protected override void BindItem(DetailsPageDependencyItem element, int index)
        {
            var dependency = (AssetIdentifier)Items[index];
            TaskUtils.TrackException(element.Bind(dependency));
        }
    }
}
