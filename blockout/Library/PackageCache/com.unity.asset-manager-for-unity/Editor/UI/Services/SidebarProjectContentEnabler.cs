using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.AssetManager.Core.Editor;

namespace Unity.AssetManager.UI.Editor
{
    class SidebarProjectContentEnabler : ISidebarContentEnabler
    {
        readonly IPageManager m_PageManager;
        readonly IAssetDataManager m_AssetDataManager;
        
        bool m_Enabled;

        public event Action AllInvalidated;

        public bool Enabled
        {
            get => m_Enabled;
            set
            {
                if (m_Enabled == value)
                    return;

                if (value)
                {
                    OnEnable();
                }
                else
                {
                    OnDisable();
                }

                m_Enabled = value;
            }
        }

        public SidebarProjectContentEnabler(IPageManager pageManager, IAssetDataManager assetDataManager)
        {
            m_PageManager = pageManager;
            m_AssetDataManager = assetDataManager;
        }

        public async Task<bool> IsEntryEnabledAsync(string id, CancellationToken cancellationToken)
        {
            // Since this method does not do any real async work, we just yield once instead of returning Task.FromResult.
            // Can be removed if real async work is added in the future.
            await Task.CompletedTask;

            if (m_PageManager.ActivePage is InProjectPage)
            {
                if (TryParseIdAsCollection(id, out var projectId, out var collectionPath))
                {
                    return m_AssetDataManager.ImportedAssetInfos
                        .Select(x => x.AssetData)
                        .Any(x => ContainsLinkedCollection(x, projectId, collectionPath));
                }

                // else it's a project
                return m_AssetDataManager.ImportedAssetInfos
                    .Select(x => x.AssetData)
                    .Any(x => IsLinkedToProject(x, id));
            }

            return true;
        }

        void OnEnable()
        {
            m_PageManager.ActivePageChanged += OnActivePageChanged;

            m_AssetDataManager.AssetDataChanged += OnAssetDataChanged;
            m_AssetDataManager.ImportedAssetInfoChanged += OnAssetDataChanged;
        }

        void OnDisable()
        {
            m_PageManager.ActivePageChanged -= OnActivePageChanged;

            m_AssetDataManager.AssetDataChanged -= OnAssetDataChanged;
            m_AssetDataManager.ImportedAssetInfoChanged -= OnAssetDataChanged;
        }

        static bool IsLinkedToProject(BaseAssetData assetData, string projectId)
        {
            return assetData.Identifier.ProjectId == projectId || assetData.LinkedProjects != null && assetData.LinkedProjects.Any(p => p != null && p.ProjectId == projectId);
        }

        static bool ContainsLinkedCollection(BaseAssetData assetData, string projectId, string collectionPath)
        {
            return assetData.LinkedCollections != null && assetData.LinkedCollections.Any(c => c.ProjectIdentifier.ProjectId == projectId && c.CollectionPath == collectionPath);
        }

        void OnActivePageChanged(IPage page)
        {
            InvalidateAll();
        }

        void OnAssetDataChanged(AssetChangeArgs args)
        {
            InvalidateAll();
        }
        
        void InvalidateAll()
        {
            AllInvalidated?.Invoke();
        }

        static bool TryParseIdAsCollection(string id, out string projectId, out string collectionPath)
        {
            var parts = id.Split(new[] {"::"}, StringSplitOptions.None);
            if (parts.Length == 2)
            {
                projectId = parts[0];
                collectionPath = parts[1];
                return true;
            }

            projectId = null;
            collectionPath = null;
            return false;
        }
    }
}
