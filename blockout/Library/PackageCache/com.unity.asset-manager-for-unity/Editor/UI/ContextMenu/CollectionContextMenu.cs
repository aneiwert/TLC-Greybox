using System;
using System.Threading.Tasks;
using Unity.AssetManager.Core.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AssetManager.UI.Editor
{
    class CollectionContextMenu : ProjectContextMenu
    {
        readonly IEditorUtilityProxy m_EditorUtilityProxy;
        readonly CollectionInfo m_CollectionInfo;

        public CollectionContextMenu(CollectionInfo collectionInfo, IProjectOrganizationProvider projectOrganizationProvider, IStateManager stateManager, IMessageManager messageManager)
            : base(collectionInfo.ProjectId, projectOrganizationProvider, stateManager, messageManager)
        {
            m_CollectionInfo = collectionInfo;
            m_EditorUtilityProxy = ServicesContainer.instance.Resolve<IEditorUtilityProxy>();
        }

        public override void SetupContextMenuEntries(ContextualMenuPopulateEvent evt)
        {
            base.SetupContextMenuEntries(evt);
            
            // Check the target to avoid adding the same menu entries multiple times add don't know which one is called
            if (evt.target == evt.currentTarget)
            {
                var targetElement = (VisualElement) evt.currentTarget;
                AddMenuEntry(evt, Constants.CollectionDelete, IsEnabled,
                    _ =>
                    {
                        TaskUtils.TrackException(DeleteCollectionAsync());
                    });
                AddMenuEntry(evt, Constants.CollectionRename, IsEnabled,
                    _ =>
                    {
                        RenameCollection(targetElement);
                    });
            }
        }

        protected override string GetParentPath() => $"{m_CollectionInfo.GetFullPath()}";

        static void RenameCollection(VisualElement target)
        {
            if (target is not SideBarCollectionFoldout foldout)
                return;

            foldout.StartRenaming();
        }

        async Task DeleteCollectionAsync()
        {
            var projectInfo = m_ProjectOrganizationProvider.GetProject(m_CollectionInfo.ProjectId);
            var path = $"{projectInfo.Name} > {m_CollectionInfo.GetFullPath().Replace("/", " > ")}";
            if (m_EditorUtilityProxy.DisplayDialog(L10n.Tr(Constants.CollectionDeleteTitle),
                    $"{L10n.Tr(Constants.CollectionDeleteMessage)}\n\n{path}", L10n.Tr(Constants.CollectionDeleteOk),
                    L10n.Tr(Constants.CollectionDeleteCancel)))
            {
                // Check if the collection is selected before it is deleted, otherwise the selection will be lost
                var isSelected = m_CollectionInfo.ProjectId == m_ProjectOrganizationProvider.SelectedCollection.ProjectId &&
                    m_CollectionInfo.GetFullPath() == m_ProjectOrganizationProvider.SelectedCollection.GetFullPath();

                try
                {
                    await m_ProjectOrganizationProvider.DeleteCollection(m_CollectionInfo);

                    AnalyticsSender.SendEvent(new ManageCollectionEvent(ManageCollectionEvent.CollectionOperationType.Delete));
                }
                catch (Exception e)
                {
                    var serviceExceptionInfo = ServiceExceptionHelper.GetServiceExceptionInfo(e);
                    if (serviceExceptionInfo != null)
                    {
                        m_MessageManager.SetHelpBoxMessage(new HelpBoxMessage(e.Message,
                            messageType: HelpBoxMessageType.Error));
                    }

                    throw;
                }

                if (isSelected)
                {
                    m_ProjectOrganizationProvider.SelectProject(projectInfo.Id, m_CollectionInfo.ParentPath);
                }
            }
        }
    }
}
