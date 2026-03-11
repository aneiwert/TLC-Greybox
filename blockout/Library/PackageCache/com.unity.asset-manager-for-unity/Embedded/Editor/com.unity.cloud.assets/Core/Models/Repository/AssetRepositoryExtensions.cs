using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// Extension methods for <see cref="IAssetRepository"/>.
    /// </summary>
    static class AssetRepositoryExtensions
    {
        /// <summary>
        /// Lists an organization's <see cref="IAssetProject"/>.
        /// </summary>
        /// <param name="assetRepository">The <see cref="IAssetRepository"/>. </param>
        /// <param name="organizationId">The id of the organization. </param>
        /// <param name="range">The range of results to return. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>An async enumeration of <see cref="IAssetProject"/>. </returns>
        public static IAsyncEnumerable<IAssetProject> ListAssetProjectsAsync(this IAssetRepository assetRepository, OrganizationId organizationId, Range range, CancellationToken cancellationToken)
        {
            return assetRepository.QueryAssetProjects(organizationId).LimitTo(range).ExecuteAsync(cancellationToken);
        }

        /// <summary>
        /// Returns the total count of assets in the specified projects based on the provided criteria.
        /// </summary>
        /// <param name="assetRepository">The <see cref="IAssetRepository"/>. </param>
        /// <param name="projectDescriptors">The ids of the projects. </param>
        /// <param name="assetSearchFilter">The filter specifying the search criteria. Can be null. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>A task whose result is an asset count. </returns>
        public static async Task<int> CountAssetsAsync(this IAssetRepository assetRepository, IEnumerable<ProjectDescriptor> projectDescriptors, [AllowNull] IAssetSearchFilter assetSearchFilter, CancellationToken cancellationToken)
        {
            var count = 0;
            var asyncEnumerable = assetRepository.GroupAndCountAssets(projectDescriptors)
                .SelectWhereMatchesFilter(assetSearchFilter)
                .LimitTo(int.MaxValue)
                .ExecuteAsync((Groupable) GroupableField.Type, cancellationToken);
            await foreach (var kvp in asyncEnumerable)
            {
                count += kvp.Value;
            }

            return count;
        }

        /// <summary>
        /// Returns the total count of assets in the specified projects based on the provided criteria.
        /// </summary>
        /// <param name="assetRepository">The <see cref="IAssetRepository"/>. </param>
        /// <param name="organizationId">The id of the organization. </param>
        /// <param name="assetSearchFilter">The filter specifying the search criteria. Can be null. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>A task whose result is an asset count. </returns>
        public static async Task<int> CountAssetsAsync(this IAssetRepository assetRepository, OrganizationId organizationId, [AllowNull] IAssetSearchFilter assetSearchFilter, CancellationToken cancellationToken)
        {
            var count = 0;
            var asyncEnumerable = assetRepository.GroupAndCountAssets(organizationId)
                .SelectWhereMatchesFilter(assetSearchFilter)
                .LimitTo(int.MaxValue)
                .ExecuteAsync((Groupable) GroupableField.Type, cancellationToken);
            await foreach (var kvp in asyncEnumerable)
            {
                count += kvp.Value;
            }

            return count;
        }

        /// <summary>
        /// Lists an organization's <see cref="IFieldDefinition"/>.
        /// </summary>
        /// <param name="assetRepository">The <see cref="IAssetRepository"/>. </param>
        /// <param name="organizationId">The id of the organization. </param>
        /// <param name="range">The range of results to return. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>An async enumeration of <see cref="IFieldDefinition"/>. </returns>
        public static IAsyncEnumerable<IFieldDefinition> ListFieldDefinitionsAsync(this IAssetRepository assetRepository, OrganizationId organizationId, Range range, CancellationToken cancellationToken)
        {
            return assetRepository.QueryFieldDefinitions(organizationId).LimitTo(range).ExecuteAsync(cancellationToken);
        }

        /// <summary>
        /// Lists an organization's <see cref="ILabel"/>.
        /// </summary>
        /// <param name="assetRepository">The <see cref="IAssetRepository"/>. </param>
        /// <param name="organizationId">The id of the organization. </param>
        /// <param name="range">The range of results to return. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>An async enumeration of <see cref="ILabel"/>. </returns>
        public static IAsyncEnumerable<ILabel> ListLabelsAsync(this IAssetRepository assetRepository, OrganizationId organizationId, Range range, CancellationToken cancellationToken)
        {
            return assetRepository.QueryLabels(organizationId).LimitTo(range).ExecuteAsync(cancellationToken);
        }

        /// <summary>
        /// Lists an organization's <see cref="IStatusFlow"/>.
        /// </summary>
        /// <param name="assetRepository">The <see cref="IAssetRepository"/>. </param>
        /// <param name="organizationId">The id of the organization. </param>
        /// <param name="range">The range of results to return. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>An async enumeration of <see cref="IStatusFlow"/>. </returns>
        public static IAsyncEnumerable<IStatusFlow> ListStatusFlowsAsync(this IAssetRepository assetRepository, OrganizationId organizationId, Range range, CancellationToken cancellationToken)
        {
            return assetRepository.QueryStatusFlows(organizationId).LimitTo(range).ExecuteAsync(cancellationToken);
        }

        /// <summary>
        /// Returns the default status flow for the specified organization.
        /// </summary>
        /// <param name="assetRepository">The <see cref="IAssetRepository"/>. </param>
        /// <param name="organizationId">The id of the organization. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>An <see cref="IStatusFlow"/>. </returns>
        public static async Task<IStatusFlow> GetDefaultStatusFlowAsync(this IAssetRepository assetRepository, OrganizationId organizationId, CancellationToken cancellationToken)
        {
            var queryResults = assetRepository.QueryStatusFlows(organizationId).ExecuteAsync(cancellationToken);
            await foreach (var statusFlow in queryResults)
            {
                if (statusFlow.IsDefault)
                {
                    return statusFlow;
                }
            }

            throw new NotFoundException("No default status flow found.");
        }
        
        /// <summary>
        /// Lists available <see cref="IAssetLibrary"/>.
        /// </summary>
        /// <param name="assetRepository">The <see cref="IAssetRepository"/>. </param>
        /// <param name="range">The range of results to return. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>An async enumeration of <see cref="IAssetLibrary"/>. </returns>
        public static IAsyncEnumerable<IAssetLibrary> ListAssetLibrariesAsync(this IAssetRepository assetRepository, Range range, CancellationToken cancellationToken)
        {
            return assetRepository.QueryAssetLibraries().LimitTo(range).ExecuteAsync(cancellationToken);
        }
        
        /// <summary>
        /// Lists available <see cref="IAssetLibraryJob"/>.
        /// </summary>
        /// <param name="assetRepository">The <see cref="IAssetRepository"/>. </param>
        /// <param name="range">The range of results to return. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>An async enumeration of <see cref="IAssetLibraryJob"/>. </returns>
        public static IAsyncEnumerable<IAssetLibraryJob> ListAssetLibraryJobsAsync(this IAssetRepository assetRepository, Range range, CancellationToken cancellationToken)
        {
            return assetRepository.QueryAssetLibraryJobs().LimitTo(range).ExecuteAsync(cancellationToken);
        }
    }
}
