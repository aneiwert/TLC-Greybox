using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.AssetManager.Editor;
using Unity.Cloud.AssetsEmbedded;
using Unity.Cloud.CommonEmbedded;
using UnityEngine;

namespace Samples.Documentation.AssetManager
{
    public static class PublicApiUsage
    {
        public static async Task ImportApi()
        {
            #region Example_Import_SpecifyAssets

            var assetIds = new[]
            {
                "1f7448da91b8490baf6ed188",
                "d7da7fad2dd2410ba557b203",
                "a2f3b0c4d5e94f1a8b7c6e9d",
                "f3a2b0c4d5e94f1a8b7c6e9d",
            };

            var importResult = await AssetManagerClient.ImportAsync(new ImportSearchFilter
            {
                AssetIds = assetIds,
            }, new ImportSettings
            {
                ConflictResolutionOverride = ConflictResolutionOverride.PreventAssetVersionRollbackAndShowConflictResolver,
            });

            Debug.Log($"Imported {importResult.ImportedAssetIds?.Count()} assets.");

            #endregion

            #region Example_Import_SearchFilter

            var searchFilter = new ImportSearchFilter
            {
                ProjectIds = new[] {"14bb9dad-58e7-47c0-8875-1aae2963482d"},
                Tags = new[] {"Texture2D", "AudioClip"},
            };

            await AssetManagerClient.ImportAsync(searchFilter, new ImportSettings
            {
                ConflictResolutionOverride = ConflictResolutionOverride.PreventAssetVersionRollbackAndReplaceAll
            });

            #endregion
        }

        #region Example_Import_AdvancedSearchFilter

        public static async Task ImportApi(IAssetRepository assetRepository, CancellationToken cancellationToken = default)
        {
            // This filter will search for assets containing "blue" in their name, of type Prefab, and with at least 2 tags from the specified list.
            var assetSearchFilter = new AssetSearchFilter();
            assetSearchFilter.Include().Name.WithValue("*blue*");
            assetSearchFilter.Include().Type.WithValue(Unity.Cloud.AssetsEmbedded.AssetType.Prefab);
            assetSearchFilter.Any().Tags.WithValue("tag1", "tag2", "tag3", "tag4", "tag5");
            assetSearchFilter.Any().WhereMinimumMatchEquals(2);

            // The query will search for assets in the organization with ID "organizationId" and order them by name in descending order.
            var query = assetRepository.QueryAssets(new OrganizationId())
                .SelectWhereMatchesFilter(assetSearchFilter)
                .OrderBy("name", SortingOrder.Descending)
                .LimitTo(new Range(0, 60));

            // Execute the query and gather the results.
            var assetIds = new List<string>();
            await foreach (var asset in query.ExecuteAsync(cancellationToken))
            {
                assetIds.Add(asset.Descriptor.AssetId.ToString());
            }

            // Import the assets using the gathered asset IDs.
            var importResult = await AssetManagerClient.ImportAsync(assetIds, new ImportSettings
                {
                    ConflictResolutionOverride = ConflictResolutionOverride.PreventAssetVersionRollbackAndShowConflictResolver
                },
                cancellationToken);

            Debug.Log($"Imported {importResult.ImportedAssetIds?.Count()} assets.");
        }

        #endregion
    }

    #region Example_Custom_AssetManagerPostprocessor
    public class MyCustomAssetManagerPostprocessor : AssetManagerPostprocessor
    {
        public override int GetPostprocessOrder() => 0;

        public override void OnPostprocessUploadAsset(UploadAsset asset)
        {
            if (asset.AssetType == Unity.AssetManager.Editor.AssetType.Material)
            {
                Array.Resize(ref asset.Tags, asset.Tags.Length + 1);
                asset.Tags[^1] = "Custom Material Tag";
            }
        }
    }
    #endregion
}
