using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.Cloud.AssetsEmbedded
{
    static class LibraryExtensions
    {
        /// <summary>
        /// Returns the total count of assets in the specified library based on the provided criteria.
        /// </summary>
        /// <param name="assetLibrary">The <see cref="IAssetLibrary"/>. </param>
        /// <param name="assetSearchFilter">The filter specifying the search criteria. Can be null. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>A task whose result is an asset count. </returns>
        public static async Task<int> CountAssetsAsync(this IAssetLibrary assetLibrary, [AllowNull] IAssetSearchFilter assetSearchFilter, CancellationToken cancellationToken)
        {
            var count = 0;
            var asyncEnumerable = assetLibrary.GroupAndCountAssets()
                .SelectWhereMatchesFilter(assetSearchFilter)
                .LimitTo(int.MaxValue).ExecuteAsync((Groupable) GroupableField.Type, cancellationToken);
            await foreach (var kvp in asyncEnumerable)
            {
                count += kvp.Value;
            }

            return count;
        }

        /// <summary>
        /// Returns the collections of the library.
        /// </summary>
        /// <param name="assetLibrary">The <see cref="IAssetLibrary"/>. </param>
        /// <param name="range">The range of results to return. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>A task whose result is an async enumeration of <see cref="IAssetCollection"/>. </returns>
        public static IAsyncEnumerable<IAssetCollection> ListCollectionsAsync(this IAssetLibrary assetLibrary, Range range, CancellationToken cancellationToken)
        {
            return assetLibrary.QueryCollections().LimitTo(range).ExecuteAsync(cancellationToken);
        }

        /// <summary>
        /// Returns the field definitions of the library.
        /// </summary>
        /// <param name="assetLibrary">The <see cref="IAssetLibrary"/>. </param>
        /// <param name="fieldDefinitionKeys">The keys of the field definitions to query. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>An async enumeration of <see cref="IFieldDefinition"/>. </returns>
        public static IAsyncEnumerable<IFieldDefinition> ListFieldDefinitionsAsync(this IAssetLibrary assetLibrary, IEnumerable<string> fieldDefinitionKeys, CancellationToken cancellationToken)
        {
            return assetLibrary.QueryFieldDefinitions(fieldDefinitionKeys).ExecuteAsync(cancellationToken);
        }

        /// <summary>
        /// Returns the labels of the library.
        /// </summary>
        /// <param name="assetLibrary">The <see cref="IAssetLibrary"/>. </param>
        /// <param name="range">The range of results to return. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>An async enumeration of <see cref="ILabel"/>. </returns>
        public static IAsyncEnumerable<ILabel> ListLabelsAsync(this IAssetLibrary assetLibrary, Range range, CancellationToken cancellationToken)
        {
            return assetLibrary.QueryLabels().LimitTo(range).ExecuteAsync(cancellationToken);
        }
    }
}
