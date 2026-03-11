using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// A class that builds and executes a query to return a set of libraries.
    /// </summary>
    class AssetLibraryQueryBuilder
    {
        readonly IAssetDataSource m_AssetDataSource;
        readonly AssetRepositoryCacheConfiguration m_DefaultCacheConfiguration;

        AssetLibraryCacheConfiguration? m_AssetLibraryCacheConfiguration;
        Range m_Range = Range.All;

        internal AssetLibraryQueryBuilder(IAssetDataSource assetDataSource, AssetRepositoryCacheConfiguration defaultCacheConfiguration)
        {
            m_AssetDataSource = assetDataSource;
            m_DefaultCacheConfiguration = defaultCacheConfiguration;
        }

        /// <summary>
        /// Sets an override to the default cache configuration for the query.
        /// </summary>
        /// <param name="assetLibraryCacheConfiguration">The configuration to apply when populating the libraries. </param>
        /// <returns>The calling <see cref="AssetLibraryQueryBuilder"/>. </returns>
        public AssetLibraryQueryBuilder WithCacheConfiguration(AssetLibraryCacheConfiguration assetLibraryCacheConfiguration)
        {
            m_AssetLibraryCacheConfiguration = assetLibraryCacheConfiguration;
            return this;
        }

        /// <summary>
        /// Sets the range of results to return.
        /// </summary>
        /// <param name="range">The range of results. </param>
        /// <returns>The calling <see cref="AssetLibraryQueryBuilder"/>. </returns>
        public AssetLibraryQueryBuilder LimitTo(Range range)
        {
            m_Range = range;
            return this;
        }

        /// <summary>
        /// Executes the query and returns the results.
        /// </summary>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>An async enumeration of <see cref="IAssetLibrary"/>. </returns>
        public async IAsyncEnumerable<IAssetLibrary> ExecuteAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var query = m_AssetDataSource.ListLibrariesAsync(new PaginationData {Range = m_Range}, cancellationToken);
            await foreach (var libraryData in query)
            {
                yield return libraryData.From(m_AssetDataSource, m_DefaultCacheConfiguration, m_AssetLibraryCacheConfiguration);
            }
        }
    }
}
