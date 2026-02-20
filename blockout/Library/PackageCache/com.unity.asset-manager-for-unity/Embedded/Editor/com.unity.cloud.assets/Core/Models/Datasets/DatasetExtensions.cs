using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// Extension methods for <see cref="IDataset"/>.
    /// </summary>
    static class DatasetExtensions
    {
        /// <summary>
        /// Returns the update histories of the dataset.
        /// </summary>
        /// <param name="dataset">The dataset to query. </param>
        /// <param name="range">The range of results to return. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>An async enumeration of <see cref="DatasetUpdateHistory"/> in descending order of <see cref="DatasetUpdateHistory.SequenceNumber"/>.</returns>
        public static IAsyncEnumerable<DatasetUpdateHistory> ListUpdateHistoriesAsync(this IDataset dataset, Range range, CancellationToken cancellationToken)
        {
            return dataset.QueryUpdateHistory()
                .LimitTo(range)
                .ExecuteAsync(cancellationToken);
        }

        /// <summary>
        /// Updates the dataset to its state at the specified update history sequence number.
        /// </summary>
        /// <param name="dataset">The dataset to query. </param>
        /// <param name="datasetUpdateHistory">The update history entry to which the dataset should be updated. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>A task with no result.</returns>
        public static Task UpdateAsync(this IDataset dataset, DatasetUpdateHistory datasetUpdateHistory, CancellationToken cancellationToken)
        {
            return dataset.UpdateAsync(datasetUpdateHistory.SequenceNumber, cancellationToken);
        }

        /// <summary>
        /// Updates the dataset to its state at the specified update history sequence number.
        /// </summary>
        /// <param name="dataset">The dataset to query. </param>
        /// <param name="datasetUpdateHistoryDescriptor">The update history descriptor to which the dataset should be updated. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>A task with no result.</returns>
        public static Task UpdateAsync(this IDataset dataset, DatasetUpdateHistoryDescriptor datasetUpdateHistoryDescriptor, CancellationToken cancellationToken)
        {
            return dataset.UpdateAsync(datasetUpdateHistoryDescriptor.SequenceNumber, cancellationToken);
        }
    }
}
