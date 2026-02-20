using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// Extension methods for <see cref="IFile"/>.
    /// </summary>
    static class FileExtensions
    {
        /// <summary>
        /// Returns the update histories of the file.
        /// </summary>
        /// <param name="file">The file to query. </param>
        /// <param name="range">The range of results to return. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>An async enumeration of <see cref="FileUpdateHistory"/> in descending order of <see cref="FileUpdateHistory.SequenceNumber"/>.</returns>
        public static IAsyncEnumerable<FileUpdateHistory> ListUpdateHistoriesAsync(this IFile file, Range range, CancellationToken cancellationToken)
        {
            return file.QueryUpdateHistory()
                .LimitTo(range)
                .ExecuteAsync(cancellationToken);
        }

        /// <summary>
        /// Updates the file to its state at the specified update history sequence number.
        /// </summary>
        /// <param name="file">The file to query. </param>
        /// <param name="fileUpdateHistory">The update history entry to which the file should be updated. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>A task with no result.</returns>
        public static Task UpdateAsync(this IFile file, FileUpdateHistory fileUpdateHistory, CancellationToken cancellationToken)
        {
            return file.UpdateAsync(fileUpdateHistory.SequenceNumber, cancellationToken);
        }

        /// <summary>
        /// Updates the file to its state at the specified update history sequence number.
        /// </summary>
        /// <param name="file">The file to query. </param>
        /// <param name="fileUpdateHistoryDescriptor">The update history descriptor to which the file should be updated. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>A task with no result.</returns>
        public static Task UpdateAsync(this IFile file, FileUpdateHistoryDescriptor fileUpdateHistoryDescriptor, CancellationToken cancellationToken)
        {
            return file.UpdateAsync(fileUpdateHistoryDescriptor.SequenceNumber, cancellationToken);
        }
    }
}
