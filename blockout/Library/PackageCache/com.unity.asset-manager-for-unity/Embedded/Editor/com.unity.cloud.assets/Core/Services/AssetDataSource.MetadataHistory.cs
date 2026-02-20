using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    partial class AssetDataSource
    {
        /// <inheritdoc />
        public Task<int> GetMetadataHistoryCountAsync(AssetDescriptor assetDescriptor, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var countRequest = MetadataHistoryRequest.Get(assetDescriptor.ProjectId, assetDescriptor.AssetId, assetDescriptor.AssetVersion, true, limit: 1);
            return GetTotalCount(countRequest, cancellationToken);
        }

        /// <inheritdoc />
        public Task<int> GetMetadataHistoryCountAsync(DatasetDescriptor datasetDescriptor, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var countRequest = MetadataHistoryRequest.Get(datasetDescriptor.ProjectId, datasetDescriptor.AssetId, datasetDescriptor.AssetVersion, datasetDescriptor.DatasetId, limit: 1);
            return GetTotalCount(countRequest, cancellationToken);
        }

        /// <inheritdoc />
        public Task<int> GetMetadataHistoryCountAsync(FileDescriptor fileDescriptor, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var countRequest = MetadataHistoryRequest.Get(fileDescriptor.ProjectId, fileDescriptor.AssetId, fileDescriptor.AssetVersion, fileDescriptor.DatasetId, fileDescriptor.Path, limit: 1);
            return GetTotalCount(countRequest, cancellationToken);
        }

        /// <inheritdoc />
        public IAsyncEnumerable<IAssetMetadataHistory> ListMetadataHistoryAsync(AssetDescriptor assetDescriptor, PaginationData pagination, bool includeChildren, CancellationToken cancellationToken)
        {
            var countRequest = MetadataHistoryRequest.Get(assetDescriptor.ProjectId, assetDescriptor.AssetId, assetDescriptor.AssetVersion, includeChildren, limit: 1);
            return ListEntitiesAsync<AssetMetadataHistory>(countRequest, GetListRequest, pagination.Range, cancellationToken);

            ApiRequest GetListRequest(int offset, int pageSize)
                => MetadataHistoryRequest.Get(assetDescriptor.ProjectId, assetDescriptor.AssetId, assetDescriptor.AssetVersion, includeChildren, pageSize, offset);
        }

        /// <inheritdoc />
        public IAsyncEnumerable<IDatasetMetadataHistory> ListMetadataHistoryAsync(DatasetDescriptor datasetDescriptor, PaginationData pagination, CancellationToken cancellationToken)
        {
            var countRequest = MetadataHistoryRequest.Get(datasetDescriptor.ProjectId, datasetDescriptor.AssetId, datasetDescriptor.AssetVersion, datasetDescriptor.DatasetId, limit: 1);
            return ListEntitiesAsync<DatasetMetadataHistory>(countRequest, GetListRequest, pagination.Range, cancellationToken);

            ApiRequest GetListRequest(int offset, int pageSize)
                => MetadataHistoryRequest.Get(datasetDescriptor.ProjectId, datasetDescriptor.AssetId, datasetDescriptor.AssetVersion, datasetDescriptor.DatasetId, pageSize, offset);
        }

        /// <inheritdoc />
        public IAsyncEnumerable<IFileMetadataHistory> ListMetadataHistoryAsync(FileDescriptor fileDescriptor, PaginationData pagination, CancellationToken cancellationToken)
        {
            var countRequest = MetadataHistoryRequest.Get(fileDescriptor.ProjectId, fileDescriptor.AssetId, fileDescriptor.AssetVersion, fileDescriptor.DatasetId, fileDescriptor.Path, limit: 1);
            return ListEntitiesAsync<FileMetadataHistory>(countRequest, GetListRequest, pagination.Range, cancellationToken);

            ApiRequest GetListRequest(int offset, int pageSize)
                => MetadataHistoryRequest.Get(fileDescriptor.ProjectId, fileDescriptor.AssetId, fileDescriptor.AssetVersion, fileDescriptor.DatasetId, fileDescriptor.Path, pageSize, offset);
        }

        /// <inheritdoc />
        public async Task RollbackMetadataHistoryAsync(AssetDescriptor assetDescriptor, int sequenceNumber, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = MetadataHistoryRequest.Rollback(assetDescriptor.ProjectId, assetDescriptor.AssetId, assetDescriptor.AssetVersion, sequenceNumber);
            using var _ = await m_ServiceHttpClient.PatchAsync(GetPublicRequestUri(request), request.ConstructBody(), ServiceHttpClientOptions.Default(), cancellationToken);
        }

        /// <inheritdoc />
        public async Task RollbackMetadataHistoryAsync(DatasetDescriptor datasetDescriptor, int sequenceNumber, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = MetadataHistoryRequest.Rollback(datasetDescriptor.ProjectId, datasetDescriptor.AssetId, datasetDescriptor.AssetVersion, datasetDescriptor.DatasetId, sequenceNumber);
            using var _ = await m_ServiceHttpClient.PatchAsync(GetPublicRequestUri(request), request.ConstructBody(), ServiceHttpClientOptions.Default(), cancellationToken);
        }

        /// <inheritdoc />
        public async Task RollbackMetadataHistoryAsync(FileDescriptor fileDescriptor, int sequenceNumber, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = MetadataHistoryRequest.Rollback(fileDescriptor.ProjectId, fileDescriptor.AssetId, fileDescriptor.AssetVersion, fileDescriptor.DatasetId, fileDescriptor.Path, sequenceNumber);
            using var _ = await m_ServiceHttpClient.PatchAsync(GetPublicRequestUri(request), request.ConstructBody(), ServiceHttpClientOptions.Default(), cancellationToken);
        }
    }
}
