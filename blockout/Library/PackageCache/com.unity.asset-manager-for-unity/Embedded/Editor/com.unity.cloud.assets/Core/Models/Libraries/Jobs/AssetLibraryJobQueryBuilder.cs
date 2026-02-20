using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// A class that builds and executes a query to return a set of library jobs.
    /// </summary>
    class AssetLibraryJobQueryBuilder
    {
        readonly IAssetDataSource m_AssetDataSource;
        readonly AssetRepositoryCacheConfiguration m_DefaultCacheConfiguration;

        Range m_Range = Range.All;

        internal AssetLibraryJobQueryBuilder(IAssetDataSource assetDataSource, AssetRepositoryCacheConfiguration defaultCacheConfiguration)
        {
            m_AssetDataSource = assetDataSource;
            m_DefaultCacheConfiguration = defaultCacheConfiguration;
        }

        /// <summary>
        /// Sets the range of results to return.
        /// </summary>
        /// <param name="range">The range of results. </param>
        /// <returns>The calling <see cref="AssetLibraryJobQueryBuilder"/>. </returns>
        public AssetLibraryJobQueryBuilder LimitTo(Range range)
        {
            m_Range = range;
            return this;
        }

        /// <summary>
        /// Executes the query and returns the results.
        /// </summary>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>An async enumeration of <see cref="IAssetLibraryJob"/>. </returns>
        public async IAsyncEnumerable<IAssetLibraryJob> ExecuteAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var query = m_AssetDataSource.ListLibraryJobsAsync(new PaginationData {Range = m_Range}, cancellationToken);
            await foreach (var data in query)
            {
                yield return data.From(m_AssetDataSource, m_DefaultCacheConfiguration);
            }
        }
    }
}
