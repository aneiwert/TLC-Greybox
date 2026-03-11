using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// A class that builds and executes a query to return a set of <see cref="FileUpdateHistory"/> for an asset version's file.
    /// </summary>
    sealed class FileUpdateHistoryQueryBuilder
    {
        readonly IAssetDataSource m_AssetDataSource;
        readonly FileDescriptor m_Descriptor;

        Range m_Range = Range.All;

        internal FileUpdateHistoryQueryBuilder(IAssetDataSource dataSource, FileDescriptor descriptor)
        {
            m_AssetDataSource = dataSource;
            m_Descriptor = descriptor;
        }

        /// <summary>
        /// Sets the range of results to return.
        /// </summary>
        /// <param name="range">The range of results. </param>
        /// <returns>The calling <see cref="FileUpdateHistoryQueryBuilder"/>. </returns>
        public FileUpdateHistoryQueryBuilder LimitTo(Range range)
        {
            m_Range = range;
            return this;
        }

        /// <summary>
        /// Returns the update histories of the file.
        /// </summary>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>An async enumeration of <see cref="FileUpdateHistory"/>. </returns>
        public async IAsyncEnumerable<FileUpdateHistory> ExecuteAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var query = m_AssetDataSource.ListMetadataHistoryAsync(m_Descriptor, new PaginationData {Range = m_Range}, cancellationToken);
            await foreach (var data in query)
            {
                yield return data.From(m_AssetDataSource, m_Descriptor);
            }
        }
    }
}
