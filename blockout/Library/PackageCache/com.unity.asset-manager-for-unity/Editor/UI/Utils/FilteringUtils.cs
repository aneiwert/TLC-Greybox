using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.AssetManager.Core.Editor;

namespace Unity.AssetManager.UI.Editor
{
    static class FilteringUtils
    {
        const int k_DefaultBatchSize = 10000;

        internal static async Task<IEnumerable<ImportedAssetInfo>> GetFilteredImportedAssets(IReadOnlyCollection<ImportedAssetInfo> importedAssetInfos, IEnumerable<LocalFilter> localFilters, CancellationToken token)
        {
            var tasks = (from assetInfo in importedAssetInfos where assetInfo.AssetData != null select IsKeptByLocalFilterAsync(assetInfo, localFilters, token)).ToList();

            await Task.WhenAll(tasks);

            return tasks.Select(t => t.Result).Where(a => a != null);
        }

        static async Task<ImportedAssetInfo> IsKeptByLocalFilterAsync(ImportedAssetInfo assetInfo, IEnumerable<LocalFilter> localFilters, CancellationToken token)
        {
            if (await IsDiscardedByLocalFilter(assetInfo.AssetData, localFilters, token))
            {
                return null;
            }

            return assetInfo;
        }

        internal static async Task<bool> IsDiscardedByLocalFilter(BaseAssetData assetData, IEnumerable<LocalFilter> localFilters, CancellationToken token)
        {
            var tasks = localFilters.Select(filter => filter.Contains(assetData, token)).ToList();

            await Task.WhenAll(tasks);

            return tasks.Exists(t => !t.Result);
        }

        internal static async Task UpdateImportStatusAsync(IAssetsProvider assetsProvider, IEnumerable<BaseAssetData> assetDatas, CancellationToken token)
        {
            try
            {
                // Only refresh the status for those that don't have it.
                var results = await assetsProvider.GatherImportStatusesAsync(assetDatas, token);
                foreach (var assetData in assetDatas)
                {
                    if (results.TryGetValue(assetData.Identifier, out var status))
                    {
                        assetData.AssetDataAttributeCollection = new AssetDataAttributeCollection(new ImportAttribute(status));
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation exceptions
            }
            catch (Exception e)
            {
                // Catch other exceptions to avoid stalling the entire grid loading
                Utilities.DevLogException(e);
            }
        }

        internal static async Task UpdateLinkedProjectsAndCollectionsForSelectionAsync(IProjectOrganizationProvider projectOrganizationProvider, IAssetsProvider assetsProvider, IEnumerable<BaseAssetData> assetDatas, CancellationToken token)
        {
            var organizationId = projectOrganizationProvider.SelectedOrganization?.Id;

            // Filter to only those in the selected organization
            var assetIds = assetDatas
                .Where(x => x.Identifier.OrganizationId == organizationId)
                .Select(x => x.Identifier.AssetId).ToArray();

            Dictionary<string, AssetCollectionsAndProjects> assetCollectionsAndProjects = new();
            foreach (var assetId in assetIds)
            {
                assetCollectionsAndProjects[assetId] = new AssetCollectionsAndProjects();
            }

            var selectedProjectId = projectOrganizationProvider.SelectedProjectOrLibrary?.Id;
            if (!string.IsNullOrEmpty(selectedProjectId))
            {
#if AM4U_DEV
                var t = new Stopwatch();
                t.Start();
#endif
                IEnumerable<Task> tasks;
                var selectedCollection = projectOrganizationProvider.SelectedCollection;
                if (selectedCollection != null && !string.IsNullOrEmpty(selectedCollection.Name))
                {
                    tasks = SearchByBatchAsync(assetIds,
                        batchedIds => SearchCollectionAsync(assetsProvider, organizationId, selectedProjectId, selectedCollection.GetFullPath(), batchedIds, assetCollectionsAndProjects, token));
                }
                else
                {
                    tasks = SearchByBatchAsync(assetIds,
                        batchedIds => SearchProjectAsync(assetsProvider, organizationId, selectedProjectId, batchedIds, assetCollectionsAndProjects, token));
                }

                await Task.WhenAll(tasks);

                // This operation should never be destructive, only additive, so only update if we found something
                // This avoids removing linked projects and collections that may have been found by the global update task
                foreach (var assetData in assetDatas)
                {
                    if (!assetCollectionsAndProjects.TryGetValue(assetData.Identifier.AssetId, out var entry))
                        continue;

                    if (!entry.LinkedProjectIds.IsEmpty)
                    {
                        var newLinkedProjects = assetData.LinkedProjects == null
                            ? entry.LinkedProjectIds
                            : assetData.LinkedProjects.Union(entry.LinkedProjectIds);
                        assetData.LinkedProjects = newLinkedProjects.ToList();
                    }

                    if (!entry.LinkedCollections.IsEmpty)
                    {
                        var newLinkedCollections = assetData.LinkedCollections == null
                            ? entry.LinkedCollections
                            : assetData.LinkedCollections.Union(entry.LinkedCollections);
                        assetData.LinkedCollections = newLinkedCollections.ToList();
                    }
                }
#if AM4U_DEV
                t.Stop();
                Utilities.DevLog($"Refreshed projects and collections [for selection only] for {assetIds.Length} asset(s) {t.ElapsedMilliseconds} ms");
#endif
            }
        }

        static IEnumerable<Task> SearchByBatchAsync(string[] assetIds, Func<IEnumerable<string>, Task> getBatchedTask, int batchSize = k_DefaultBatchSize)
        {
            var tasks = new List<Task>();

            var startIndex = 0;
            while (startIndex < assetIds.Length)
            {
                var maxCount = Math.Min(batchSize, assetIds.Length - startIndex);
                tasks.Add(getBatchedTask(assetIds[startIndex..(startIndex + maxCount)]));

                startIndex += batchSize;
            }

            return tasks;
        }

        static async Task SearchProjectAsync(IAssetsProvider assetsProvider, string organizationId, string projectId, IEnumerable<string> assetIds, Dictionary<string, AssetCollectionsAndProjects> assetCollectionsAndProjectsMap, CancellationToken token)
        {
            var projectIdentifier = new ProjectIdentifier(organizationId, projectId);

            var searchFilter = new AssetSearchFilter {AssetIds = assetIds.ToList()};
            var query = assetsProvider.SearchLiteAsync(organizationId, new[] {projectId}, searchFilter, SortField.Name, SortingOrder.Ascending, 0, 0, token);
            await foreach (var assetIdentifier in query)
            {
                if (assetCollectionsAndProjectsMap.TryGetValue(assetIdentifier.AssetId, out var entry))
                {
                    entry.LinkedProjectIds.Add(projectIdentifier);
                }
            }
        }

        static async Task SearchCollectionAsync(IAssetsProvider assetsProvider, string organizationId, string projectId, string collectionPath, IEnumerable<string> assetIds, Dictionary<string, AssetCollectionsAndProjects> assetCollectionsAndProjectsMap, CancellationToken token)
        {
            var collectionIdentifier = new CollectionIdentifier(new ProjectIdentifier(organizationId, projectId), collectionPath);

            var searchFilter = new AssetSearchFilter
            {
                AssetIds = assetIds.ToList(),
                Collection = new List<string> {collectionPath}
            };
            var query = assetsProvider.SearchLiteAsync(organizationId, new[] {projectId}, searchFilter, SortField.Name, SortingOrder.Ascending, 0, 0, token);
            await foreach (var id in query)
            {
                if (assetCollectionsAndProjectsMap.TryGetValue(id.AssetId, out var entry))
                {
                    entry.LinkedCollections.Add(collectionIdentifier);
                }
            }
        }

        class AssetCollectionsAndProjects
        {
            public readonly ConcurrentBag<ProjectIdentifier> LinkedProjectIds = new();
            public readonly ConcurrentBag<CollectionIdentifier> LinkedCollections = new();
        }
    }
}
