using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// A class that builds and executes a query to return a set of <see cref="AssetUpdateHistory"/> for an asset version.
    /// </summary>
    sealed class AssetUpdateHistoryQueryBuilder
    {
        readonly IAssetDataSource m_AssetDataSource;
        readonly AssetDescriptor m_Descriptor;

        AssetUpdateHistorySearchFilter m_SearchFilter;
        Range m_Range = Range.All;

        internal AssetUpdateHistoryQueryBuilder(IAssetDataSource dataSource, AssetDescriptor descriptor)
        {
            m_AssetDataSource = dataSource;
            m_Descriptor = descriptor;
        }

        /// <summary>
        /// Sets the filter to use for the query.
        /// </summary>
        /// <param name="searchFilter">The search criteria. </param>
        /// <returns>The calling <see cref="AssetUpdateHistoryQueryBuilder"/>. </returns>
        public AssetUpdateHistoryQueryBuilder SelectWhereMatchesFilter(AssetUpdateHistorySearchFilter searchFilter)
        {
            m_SearchFilter = searchFilter;
            return this;
        }

        /// <summary>
        /// Sets the range of results to return.
        /// </summary>
        /// <param name="range">The range of results. </param>
        /// <returns>The calling <see cref="AssetUpdateHistoryQueryBuilder"/>. </returns>
        public AssetUpdateHistoryQueryBuilder LimitTo(Range range)
        {
            m_Range = range;
            return this;
        }

        /// <summary>
        /// Returns the update history of the asset.
        /// </summary>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>An async enumeration of <see cref="AssetUpdateHistory"/>. </returns>
        public async IAsyncEnumerable<AssetUpdateHistory> ExecuteAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var includeChildren = m_SearchFilter?.IncludeDatasetsAndFiles.GetValue() ?? false;
            var query = m_AssetDataSource.ListMetadataHistoryAsync(m_Descriptor, new PaginationData {Range = m_Range}, includeChildren, cancellationToken);
            await foreach (var data in query)
            {
                yield return data.From(m_AssetDataSource, m_Descriptor);
            }
        }
    }
}
