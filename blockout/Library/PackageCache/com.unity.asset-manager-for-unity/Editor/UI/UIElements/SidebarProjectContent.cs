using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.AssetManager.Core.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AssetManager.UI.Editor
{
    class SidebarProjectContent : Foldout
    {
        readonly IProjectOrganizationProvider m_ProjectOrganizationProvider;
        readonly ISidebarContentEnabler m_SidebarContentEnabler;
        readonly IStateManager m_StateManager;
        readonly IMessageManager m_MessageManager;

        readonly Dictionary<string, SideBarCollectionFoldout> m_SideBarProjectFoldouts = new();
        readonly bool m_IsAssetLibraryFoldout;
        Task m_RefreshTask;

        public SidebarProjectContent(IProjectOrganizationProvider projectOrganizationProvider, ISidebarContentEnabler sidebarContentEnabler,
            IStateManager stateManager, IMessageManager messageManager, bool isAssetLibraryFoldout = false)
        {
            var toggle = this.Q<Toggle>();
            toggle.text = isAssetLibraryFoldout ? Constants.SidebarAssetLibrariesText : Constants.SidebarProjectsText;
            toggle.AddToClassList("SidebarContentTitle");
            toggle.focusable = false;

            m_ProjectOrganizationProvider = projectOrganizationProvider;
            m_SidebarContentEnabler = sidebarContentEnabler;
            m_StateManager = stateManager;
            m_MessageManager = messageManager;
            m_IsAssetLibraryFoldout = isAssetLibraryFoldout;

            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);

            if (m_IsAssetLibraryFoldout)
            {
                UIElementsUtils.Hide(this);
                m_ProjectOrganizationProvider.AssetLibrariesProjectsLoaded += RebuildProjectList;
            }
        }

        void OnAttachToPanel(AttachToPanelEvent _)
        {
            m_ProjectOrganizationProvider.ProjectInfoChanged += ProjectInfoChanged;
            m_SidebarContentEnabler.AllInvalidated += RefreshEnabledStates;
        }

        void OnDetachFromPanel(DetachFromPanelEvent _)
        {
            m_ProjectOrganizationProvider.ProjectInfoChanged -= ProjectInfoChanged;
            m_SidebarContentEnabler.AllInvalidated -= RefreshEnabledStates;
        }

        public void SelectProject(ProjectOrLibraryInfo projectOrLibraryInfo, CollectionInfo collectionInfo)
        {
            // First try to select the collection, if not available select the project
            var selectedId = GetCollectionId(collectionInfo);
            selectedId = string.IsNullOrEmpty(selectedId) ? projectOrLibraryInfo?.Id : selectedId;

            foreach (var projectFoldout in m_SideBarProjectFoldouts)
            {
                SetSelectedRecursive(projectFoldout.Value, selectedId);
            }
        }

        public void ClearSelectedProject()
        {
            foreach (var kvp in m_SideBarProjectFoldouts)
            {
                SetSelectedRecursive(kvp.Value, null);
            }
        }

        void ProjectInfoChanged(ProjectOrLibraryInfo projectOrLibraryInfo)
        {
            TryAddCollections(projectOrLibraryInfo);
        }

        async void RefreshEnabledStates()
        {
            try
            {
                var tasks = new List<Task>();

                foreach (var kvp in m_SideBarProjectFoldouts)
                {
                    RefreshEnabledStateRecursive(kvp.Value, tasks);
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                Utilities.DevLogException(e);
            }
        }

        void RefreshEnabledStateRecursive(SideBarFoldout foldout, List<Task> tasks)
        {
            tasks.Add(RefreshEnabledState(foldout));

            foreach (var child in foldout.Children())
            {
                if (child is SideBarFoldout childFoldout)
                {
                    RefreshEnabledStateRecursive(childFoldout, tasks);
                }
            }
        }

        Task RefreshEnabledState(SideBarFoldout foldout)
        {
            m_StateManager.SetCollectionFoldoutPopulatedState(foldout.name, true);
            foldout.SetPopulatedState(true);
            return Task.CompletedTask;
        }

        public void Refresh()
        {
            if (m_ProjectOrganizationProvider.SelectedOrganization != null)
            {
                if (m_IsAssetLibraryFoldout)
                {
                    RebuildProjectListAsync();
                }
                else
                {
                    var orderedProjectInfos =
                        m_ProjectOrganizationProvider.SelectedOrganization.ProjectInfos.OrderBy(p => p.Name);
                    RebuildProjectList(orderedProjectInfos.ToList());
                }
            }
            else
            {
                RebuildProjectList(null);
            }
        }

        async void RebuildProjectListAsync()
        {
            RebuildProjectList(await m_ProjectOrganizationProvider.GetAssetLibrariesProjectsAsync());
        }

        void RebuildProjectList(List<ProjectOrLibraryInfo> projectInfos)
        {
            foreach (var foldout in m_SideBarProjectFoldouts.Values)
                Remove(foldout);

            m_SideBarProjectFoldouts.Clear();

            if (projectInfos?.Any() != true)
                return;

            UIElementsUtils.Show(this);

            foreach (var projectInfo in projectInfos)
            {
                var projectFoldout = new SideBarCollectionFoldout(
                    m_StateManager, m_MessageManager, m_ProjectOrganizationProvider,
                    projectInfo.Name, projectInfo.Id, null,
                    m_StateManager.GetCollectionFoldoutPopulatedState(projectInfo.Id), m_IsAssetLibraryFoldout);
                Add(projectFoldout);
                m_SideBarProjectFoldouts[projectInfo.Id] = projectFoldout;

                TryAddCollections(projectInfo);
            }
        }

        void TryAddCollections(ProjectOrLibraryInfo projectOrLibraryInfo)
        {
            // Clean up any existing collection foldouts for the project
            if (!m_SideBarProjectFoldouts.TryGetValue(projectOrLibraryInfo.Id, out var projectFoldout))
                return;

            // Capture the state of any foldout that is currently in naming or renaming mode
            var namingState = FindNamingState(projectFoldout);

            projectFoldout.Clear();
            projectFoldout.ChangeBackToChildlessFolder();

            if (projectOrLibraryInfo.CollectionInfos == null)
                return;

            if (!projectOrLibraryInfo.CollectionInfos.Any())
                return;

            projectFoldout.value = false;
            var orderedCollectionInfos = projectOrLibraryInfo.CollectionInfos.OrderBy(c => c.GetFullPath());
            foreach (var collection in orderedCollectionInfos)
            {
                CreateFoldoutForParentsThenItself(collection, projectOrLibraryInfo, projectFoldout);
            }

            // Restore naming/renaming state after rebuilding
            if (namingState.HasValue)
            {
                RestoreNamingState(projectFoldout, namingState.Value);
            }
        }

        FoldoutNamingState? FindNamingState(SideBarFoldout foldout)
        {
            if (foldout is SideBarCollectionFoldout collectionFoldout)
            {
                var state = collectionFoldout.GetNamingState();
                if (state.IsInNamingMode)
                {
                    return state;
                }
            }

            foreach (var child in foldout.Children())
            {
                if (child is SideBarFoldout childFoldout)
                {
                    var result = FindNamingState(childFoldout);
                    if (result.HasValue)
                        return result;
                }
            }

            return null;
        }

        void RestoreNamingState(SideBarCollectionFoldout projectFoldout, FoldoutNamingState foldoutNamingState)
        {
            if (foldoutNamingState.IsRenaming)
            {
                // Find existing foldout and restore renaming state
                var existingFoldout = this.Q<SideBarCollectionFoldout>(foldoutNamingState.CollectionId);
                existingFoldout?.RestoreNamingState(foldoutNamingState);
            }
            else if (foldoutNamingState.IsNaming)
            {
                // Create temporary foldout for naming state
                CreateTemporaryNamingFoldout(projectFoldout, foldoutNamingState.CollectionId, foldoutNamingState);
            }
        }

        void CreateTemporaryNamingFoldout(SideBarCollectionFoldout projectFoldout, string collectionId, FoldoutNamingState foldoutNamingState)
        {
            // We need to recreate the temporary foldout used for naming a new collection

            // Find the parent foldout directly using the stored parent collection ID
            var parentFoldout = projectFoldout;
            if (!string.IsNullOrEmpty(foldoutNamingState.ParentCollectionId))
            {
                parentFoldout = this.Q<SideBarCollectionFoldout>(foldoutNamingState.ParentCollectionId) ?? projectFoldout;
            }

            // Create the temporary foldout
            var tempFoldout = new SideBarCollectionFoldout(m_StateManager, m_MessageManager, m_ProjectOrganizationProvider,
                foldoutNamingState.NamingInput, projectFoldout.name, parentFoldout.CollectionPath, true, m_IsAssetLibraryFoldout);

            // A temporary foldout for a new collection doesn't have a persistent ID. Assign a unique
            // name to prevent ID conflicts with its parent in the VisualElement hierarchy.
            tempFoldout.name = $"new-collection-naming-{projectFoldout.name}";
            parentFoldout.Add(tempFoldout);

            // The original OnNamingFailed action is stale as it refers to destroyed UI elements.
            // Create a new action that correctly removes the new temporary foldout from its new parent.
            Action newOnNamingFailed = () => parentFoldout.Remove(tempFoldout);
            foldoutNamingState.OnNamingFailed = newOnNamingFailed;

            // Restore the naming state
            tempFoldout.RestoreNamingState(foldoutNamingState);
        }

        void CreateFoldoutForParentsThenItself(CollectionInfo collectionInfo, ProjectOrLibraryInfo projectOrLibraryInfo,
            SideBarFoldout projectFoldout)
        {
            var collectionId = GetCollectionId(collectionInfo);
            if (this.Q<SideBarFoldout>(collectionId) != null)
                return;

            var collectionFoldout = new SideBarCollectionFoldout(
                m_StateManager, m_MessageManager, m_ProjectOrganizationProvider,
                collectionInfo.Name, projectOrLibraryInfo.Id, collectionInfo.GetFullPath(),
                m_StateManager.GetCollectionFoldoutPopulatedState(collectionId), m_IsAssetLibraryFoldout);
            collectionFoldout.SetSelected(GetCollectionId(m_ProjectOrganizationProvider.SelectedCollection) == collectionId);

            SideBarFoldout parentFoldout = null;
            if (!string.IsNullOrEmpty(collectionInfo.ParentPath))
            {
                var parentCollection = projectOrLibraryInfo.GetCollection(collectionInfo.ParentPath);
                CreateFoldoutForParentsThenItself(parentCollection, projectOrLibraryInfo, projectFoldout);

                parentFoldout = this.Q<SideBarFoldout>(GetCollectionId(parentCollection));
                Utilities.DevAssert(parentFoldout != null);
            }

            var immediateParent = parentFoldout ?? projectFoldout;
            immediateParent.AddFoldout(collectionFoldout);
        }

        static void SetSelectedRecursive(SideBarFoldout foldout, string selectedId)
        {
            foldout.SetSelected(foldout.name == selectedId);
            foreach (var child in foldout.Children())
            {
                if (child is SideBarFoldout childFoldout)
                {
                    SetSelectedRecursive(childFoldout, selectedId);
                }
            }
        }

        static string GetCollectionId(CollectionInfo collectionInfo) =>
            collectionInfo == null
                ? null
                : SideBarCollectionFoldout.GetCollectionId(collectionInfo.ProjectId, collectionInfo.GetFullPath());
    }
}
