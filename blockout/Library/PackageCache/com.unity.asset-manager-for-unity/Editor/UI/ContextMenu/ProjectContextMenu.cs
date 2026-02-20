using System.Linq;
using Unity.AssetManager.Core.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AssetManager.UI.Editor
{
    class ProjectContextMenu : ContextMenu
    {
        protected readonly IProjectOrganizationProvider m_ProjectOrganizationProvider;
        protected readonly IMessageManager m_MessageManager;
        readonly IStateManager m_StateManager;
        readonly IUnityConnectProxy m_UnityConnectProxy;
        readonly string m_ProjectId;

        protected bool IsEnabled => m_UnityConnectProxy.AreCloudServicesReachable;

        public ProjectContextMenu(string projectId, IProjectOrganizationProvider projectOrganizationProvider,
            IStateManager stateManager, IMessageManager messageManager)
        {
            m_ProjectId = projectId;
            m_ProjectOrganizationProvider = projectOrganizationProvider;
            m_StateManager = stateManager;
            m_MessageManager = messageManager;
            m_UnityConnectProxy = ServicesContainer.instance.Resolve<IUnityConnectProxy>();
        }

        public override void SetupContextMenuEntries(ContextualMenuPopulateEvent evt)
        {
            // Check the target to avoid adding the same menu entries multiple times add don't know which one is called
            if (evt.target == evt.currentTarget)
            {
                var targetElement = (VisualElement) evt.currentTarget;
                AddMenuEntry(evt, Constants.CollectionCreate,
                    IsEnabled && !string.IsNullOrEmpty(m_ProjectId),
                    _ =>
                    {
                        CreateCollection(targetElement);
                    });
            }
        }

        void CreateCollection(VisualElement target)
        {
            var name = Constants.CollectionDefaultName;
            var projectInfo = m_ProjectOrganizationProvider.GetProject(m_ProjectId);

            if (projectInfo?.CollectionInfos != null)
            {
                var index = 1;
                while (projectInfo.CollectionInfos.Any(c => c.Name == name && c.ParentPath == GetParentPath()))
                {
                    name = $"{Constants.CollectionDefaultName} ({index++})";
                }
            }

            var newFoldout = new SideBarCollectionFoldout(m_StateManager, m_MessageManager, m_ProjectOrganizationProvider,
                name, m_ProjectId, GetParentPath(), true);
            target.Add(newFoldout);
            newFoldout.StartNaming(() => OnNamingFailed(target, newFoldout));
        }

        protected virtual string GetParentPath() => string.Empty;

        static void OnNamingFailed(VisualElement target, SideBarCollectionFoldout newFoldout)
        {
            target.Remove(newFoldout);
        }
    }
}
