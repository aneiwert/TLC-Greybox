using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    partial class AssetDataSource
    {
        /// <inheritdoc />
        public async Task<Uri> CreateFileAsync(DatasetDescriptor datasetDescriptor, IFileCreateData fileCreation, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = new CreateFileRequest(datasetDescriptor.ProjectId, datasetDescriptor.AssetId, datasetDescriptor.AssetVersion, datasetDescriptor.DatasetId, fileCreation);
            using var response = await m_ServiceHttpClient.PostAsync(GetPublicRequestUri(request), request.ConstructBody(),
                ServiceHttpClientOptions.Default(), cancellationToken);

            var jsonContent = await response.GetContentAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();

            var dto = JsonSerialization.Deserialize<UploadUrlDto>(jsonContent);

            var uploadUrl = Uri.TryCreate(dto.UploadUrl, UriKind.Absolute, out var uri) ? uri : null;
            return uploadUrl;
        }

        /// <inheritdoc />
        public IAsyncEnumerable<IFileData> ListFilesAsync(DatasetDescriptor datasetDescriptor, Range range, FieldsFilter includedFieldsFilter, CancellationToken cancellationToken)
        {
            var isPathToLibrary = datasetDescriptor.IsPathToAssetLibrary();

            var countRequest = isPathToLibrary
                ? new FileRequest(datasetDescriptor.AssetLibraryId, datasetDescriptor.AssetId, datasetDescriptor.AssetVersion, datasetDescriptor.DatasetId, FileFields.none, limit: 1)
                : new FileRequest(datasetDescriptor.ProjectId, datasetDescriptor.AssetId, datasetDescriptor.AssetVersion, datasetDescriptor.DatasetId, FileFields.none, limit: 1);
            var fileFields = includedFieldsFilter?.FileFields ?? FileFields.none;
            return ListEntitiesAsync<FileData>(countRequest, GetListRequest, range, cancellationToken);

            ApiRequest GetListRequest(string next, int pageSize)
            {
                return isPathToLibrary
                ? new FileRequest(datasetDescriptor.AssetLibraryId, datasetDescriptor.AssetId, datasetDescriptor.AssetVersion, datasetDescriptor.DatasetId, fileFields, next, pageSize)
                : new FileRequest(datasetDescriptor.ProjectId, datasetDescriptor.AssetId, datasetDescriptor.AssetVersion, datasetDescriptor.DatasetId, fileFields, next, pageSize);
            }
        }

        /// <inheritdoc />
        public Task<IFileData> GetFileAsync(FileDescriptor fileDescriptor, FieldsFilter includedFieldsFilter, CancellationToken cancellationToken)
        {
            return fileDescriptor.IsPathToAssetLibrary()
                ? GetFileAsync_FromLibrary(fileDescriptor, includedFieldsFilter, cancellationToken)
                : GetFileAsync_FromProject(fileDescriptor, includedFieldsFilter, cancellationToken);
        }

        async Task<IFileData> GetFileAsync_FromLibrary(FileDescriptor fileDescriptor, FieldsFilter includedFieldsFilter, CancellationToken cancellationToken)
        {
            await foreach(var file in ListFilesAsync(fileDescriptor.DatasetDescriptor, Range.All, includedFieldsFilter, cancellationToken))
            {
                if (file.Path == fileDescriptor.Path)
                {
                    return file;
                }
            }

            throw new NotFoundException("File does not exist.");
        }

        async Task<IFileData> GetFileAsync_FromProject(FileDescriptor fileDescriptor, FieldsFilter includedFieldsFilter, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = new FileRequest(fileDescriptor.ProjectId, fileDescriptor.AssetId, fileDescriptor.AssetVersion, fileDescriptor.DatasetId, fileDescriptor.Path,
                includedFieldsFilter?.FileFields ?? FileFields.none);
            using var response = await m_ServiceHttpClient.GetAsync(GetPublicRequestUri(request), ServiceHttpClientOptions.Default(),
                cancellationToken);

            var jsonContent = await response.GetContentAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();

            return IsolatedSerialization.DeserializeWithDefaultConverters<FileData>(jsonContent);
        }

        /// <inheritdoc />
        public async Task UpdateFileAsync(FileDescriptor fileDescriptor, IFileBaseData fileUpdate, CancellationToken cancellationToken)
        {
            var request = new FileRequest(fileDescriptor.ProjectId,
                fileDescriptor.AssetId,
                fileDescriptor.AssetVersion,
                fileDescriptor.DatasetId,
                fileDescriptor.Path,
                fileUpdate);

            using var _ = await m_ServiceHttpClient.PatchAsync(GetPublicRequestUri(request), request.ConstructBody(),
                ServiceHttpClientOptions.Default(), cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Uri> UpdateFileContentAsync(FileDescriptor fileDescriptor, IFileCreateData fileCreate, CancellationToken cancellationToken)
        {
            var request = FileRequest.GetContentUpdateRequest(fileDescriptor.ProjectId,
                fileDescriptor.AssetId,
                fileDescriptor.AssetVersion,
                fileDescriptor.DatasetId,
                fileDescriptor.Path,
                fileCreate);

            using var response = await m_ServiceHttpClient.PatchAsync(GetPublicRequestUri(request), request.ConstructBody(),
                ServiceHttpClientOptions.Default(), cancellationToken);

            var jsonContent = await response.GetContentAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();

            var dto = JsonSerialization.Deserialize<UploadUrlDto>(jsonContent);
            return Uri.TryCreate(dto.UploadUrl, UriKind.Absolute, out var uri) ? uri : null;
        }

        /// <inheritdoc />
        public Task<Uri> GetFileDownloadUrlAsync(FileDescriptor fileDescriptor, int? maxDimension, CancellationToken cancellationToken)
        {
            const int minDimension = 50;

            if (maxDimension is < minDimension)
            {
                throw new ArgumentOutOfRangeException(nameof(maxDimension), maxDimension, $"The minimum dimension for resize requests is {minDimension}.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            var request = fileDescriptor.IsPathToAssetLibrary()
                ? new GetFileDownloadUrlRequest(fileDescriptor.AssetLibraryId,
                    fileDescriptor.AssetId,
                    fileDescriptor.AssetVersion,
                    fileDescriptor.DatasetId,
                    fileDescriptor.Path,
                    maxDimension)
                : new GetFileDownloadUrlRequest(fileDescriptor.ProjectId,
                fileDescriptor.AssetId,
                fileDescriptor.AssetVersion,
                fileDescriptor.DatasetId,
                fileDescriptor.Path,
                maxDimension);

            return GetFileUrlAsync(request, cancellationToken);
        }

        /// <inheritdoc />
        public Task<Uri> GetFileUploadUrlAsync(FileDescriptor fileDescriptor, IFileData fileData, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = new GetFileUploadUrlRequest(fileDescriptor.ProjectId,
                fileDescriptor.AssetId,
                fileDescriptor.AssetVersion,
                fileDescriptor.DatasetId,
                fileDescriptor.Path,
                fileData);

            return GetFileUrlAsync(request, cancellationToken);
        }

        async Task<Uri> GetFileUrlAsync(ApiRequest request, CancellationToken cancellationToken)
        {
            using var response = await m_ServiceHttpClient.GetAsync(GetPublicRequestUri(request), ServiceHttpClientOptions.Default(), cancellationToken);

            var jsonContent = await response.GetContentAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();

            var dto = JsonSerialization.Deserialize<FileUrl>(jsonContent);

            return GetEscapedUri(dto.Url);
        }

        /// <inheritdoc />
        public async Task FinalizeFileUploadAsync(FileDescriptor fileDescriptor, bool disableAutomaticTransformations, CancellationToken cancellationToken)
        {
            var request = new FinalizeFileUploadRequest(fileDescriptor.ProjectId,
                fileDescriptor.AssetId,
                fileDescriptor.AssetVersion,
                fileDescriptor.DatasetId,
                fileDescriptor.Path,
                disableAutomaticTransformations);

            using var _ = await m_ServiceHttpClient.PostAsync(GetPublicRequestUri(request), request.ConstructBody(),
                ServiceHttpClientOptions.Default(), cancellationToken);
        }

        /// <inheritdoc />
        public async Task<FileTag[]> GenerateFileTagsAsync(FileDescriptor fileDescriptor, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = new GenerateFileTagsRequest(fileDescriptor.ProjectId,
                fileDescriptor.AssetId,
                fileDescriptor.AssetVersion,
                fileDescriptor.DatasetId,
                fileDescriptor.Path);
            using var response = await m_ServiceHttpClient.GetAsync(GetPublicRequestUri(request),
                ServiceHttpClientOptions.Default(), cancellationToken);

            var jsonContent = await response.GetContentAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();

            var dto = JsonSerialization.Deserialize<FileTags>(jsonContent);

            return dto.Tags;
        }

        /// <inheritdoc />
        public async Task RemoveFileMetadataAsync(FileDescriptor fileDescriptor, string metadataType, IEnumerable<string> keys, CancellationToken cancellationToken)
        {
            var request = RemoveMetadataRequest.Get(fileDescriptor.ProjectId,
                fileDescriptor.AssetId,
                fileDescriptor.AssetVersion,
                fileDescriptor.DatasetId,
                fileDescriptor.Path,
                metadataType,
                keys);
            using var _ = await m_ServiceHttpClient.DeleteAsync(GetPublicRequestUri(request), ServiceHttpClientOptions.Default(), cancellationToken);
        }
    }
}
