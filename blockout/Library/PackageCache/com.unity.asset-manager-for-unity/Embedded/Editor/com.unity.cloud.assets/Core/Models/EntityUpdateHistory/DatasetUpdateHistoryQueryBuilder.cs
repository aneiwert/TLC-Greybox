using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// A class that builds and executes a query to return a set of <see cref="DatasetUpdateHistory"/> for an asset version's dataset.
    /// </summary>
    sealed class DatasetUpdateHistoryQueryBuilder
    {
        readonly IAssetDataSource m_AssetDataSource;
        readonly DatasetDescriptor m_Descriptor;

        Range m_Range = Range.All;

        internal DatasetUpdateHistoryQueryBuilder(IAssetDataSource dataSource, DatasetDescriptor descriptor)
        {
            m_AssetDataSource = dataSource;
            m_Descriptor = descriptor;
        }

        /// <summary>
        /// Sets the range of results to return.
        /// </summary>
        /// <param name="range">The range of results. </param>
        /// <returns>The calling <see cref="DatasetUpdateHistoryQueryBuilder"/>. </returns>
        public DatasetUpdateHistoryQueryBuilder LimitTo(Range range)
        {
            m_Range = range;
            return this;
        }

        /// <summary>
        /// Returns the update histories of the dataset.
        /// </summary>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>An async enumeration of <see cref="DatasetUpdateHistory"/>. </returns>
        public async IAsyncEnumerable<DatasetUpdateHistory> ExecuteAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var query = m_AssetDataSource.ListMetadataHistoryAsync(m_Descriptor, new PaginationData {Range = m_Range}, cancellationToken);
            await foreach (var data in query)
            {
                yield return data.From(m_AssetDataSource, m_Descriptor);
            }
        }
    }
}
