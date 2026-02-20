using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    partial interface IAssetDataSource
    {
        Task<int> GetMetadataHistoryCountAsync(AssetDescriptor assetDescriptor, CancellationToken cancellationToken);
        Task<int> GetMetadataHistoryCountAsync(DatasetDescriptor datasetDescriptor, CancellationToken cancellationToken);
        Task<int> GetMetadataHistoryCountAsync(FileDescriptor fileDescriptor, CancellationToken cancellationToken);
        
        /// <summary>
        /// Retrieves a list of metadata history for an asset version.
        /// </summary>
        /// <param name="assetDescriptor">The object containing the necessary information to identify the asset. </param>
        /// <param name="pagination">An object containing the necessary information return a range of projects. </param>
        /// <param name="includeChildren">Whether the enumeration should include changes to child entities. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>A task whose result is an async enumeration of <see cref="IAssetMetadataHistory"/>. </returns>
        IAsyncEnumerable<IAssetMetadataHistory> ListMetadataHistoryAsync(AssetDescriptor assetDescriptor, PaginationData pagination, bool includeChildren, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a list of metadata history for a dataset.
        /// </summary>
        /// <param name="datasetDescriptor">The object containing the necessary information to identify the dataset. </param>
        /// <param name="pagination">An object containing the necessary information return a range of projects. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>A task whose result is an async enumeration of <see cref="IDatasetMetadataHistory"/>. </returns>
        IAsyncEnumerable<IDatasetMetadataHistory> ListMetadataHistoryAsync(DatasetDescriptor datasetDescriptor, PaginationData pagination, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a list of metadata history for a file.
        /// </summary>
        /// <param name="fileDescriptor">The object containing the necessary information to identify the file. </param>
        /// <param name="pagination">An object containing the necessary information return a range of projects. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>A task whose result is an async enumeration of <see cref="IFileMetadataHistory"/>. </returns>
        IAsyncEnumerable<IFileMetadataHistory> ListMetadataHistoryAsync(FileDescriptor fileDescriptor, PaginationData pagination, CancellationToken cancellationToken);
        
        /// <summary>
        /// Rollback the metadata of an asset to a previous state identified by the sequence number.
        /// </summary>
        /// <param name="assetDescriptor">The object containing the necessary information to identify the asset. </param>
        /// <param name="sequenceNumber">The id of the metadata history change. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>A task with no result. </returns>
        Task RollbackMetadataHistoryAsync(AssetDescriptor assetDescriptor, int sequenceNumber, CancellationToken cancellationToken);
        
        /// <summary>
        /// Rollback the metadata of a dataset to a previous state identified by the sequence number.
        /// </summary>
        /// <param name="datasetDescriptor">The object containing the necessary information to identify the dataset. </param>
        /// <param name="sequenceNumber">The id of the metadata history change. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>A task with no result. </returns>
        Task RollbackMetadataHistoryAsync(DatasetDescriptor datasetDescriptor, int sequenceNumber, CancellationToken cancellationToken);
        
        /// <summary>
        /// Rollback the metadata of a file to a previous state identified by the sequence number.
        /// </summary>
        /// <param name="fileDescriptor">The object containing the necessary information to identify the file. </param>
        /// <param name="sequenceNumber">The id of the metadata history change. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>A task with no result. </returns>
        Task RollbackMetadataHistoryAsync(FileDescriptor fileDescriptor, int sequenceNumber, CancellationToken cancellationToken);
    }
}
