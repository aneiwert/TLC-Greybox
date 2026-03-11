using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    class DatasetEntity : IDataset
    {
        readonly IAssetDataSource m_DataSource;
        readonly CacheConfigurationWrapper m_CacheConfiguration;

        /// <inheritdoc />
        public DatasetDescriptor Descriptor { get; }

        /// <inheritdoc />
        public string Name => Properties.Name;

        /// <inheritdoc />
        public string Description => Properties.Description;

        /// <inheritdoc />
        public IEnumerable<string> Tags => Properties.Tags;

        /// <inheritdoc />
        public IEnumerable<string> SystemTags => Properties.SystemTags;

        /// <inheritdoc />
        public string Status => Properties.StatusName;

        /// <inheritdoc />
        public AuthoringInfo AuthoringInfo => Properties.AuthoringInfo;

        /// <inheritdoc />
        public IMetadataContainer Metadata => MetadataEntity;

        /// <inheritdoc />
        public IReadOnlyMetadataContainer SystemMetadata => SystemMetadataEntity;

        /// <inheritdoc />
        public IEnumerable<string> FileOrder => Properties.FileOrder;

        /// <inheritdoc />
        public bool IsVisible => Properties.IsVisible;

        /// <inheritdoc />
        public DatasetCacheConfiguration CacheConfiguration => m_CacheConfiguration.DatasetConfiguration;

        AssetRepositoryCacheConfiguration DefaultCacheConfiguration => m_CacheConfiguration.DefaultConfiguration;

        internal DatasetProperties Properties { get; set; }
        internal List<IFileData> Files { get; } = new();
        internal Dictionary<string, IFileData> FileMap { get; } = new();
        internal MetadataContainerEntity MetadataEntity { get; }
        internal ReadOnlyMetadataContainerEntity SystemMetadataEntity { get; }

        internal DatasetEntity(IAssetDataSource assetDataSource, AssetRepositoryCacheConfiguration defaultCacheConfiguration, DatasetDescriptor datasetDescriptor, DatasetCacheConfiguration? cacheConfigurationOverride = null)
        {
            m_DataSource = assetDataSource;
            Descriptor = datasetDescriptor;

            m_CacheConfiguration = new CacheConfigurationWrapper(defaultCacheConfiguration);
            m_CacheConfiguration.SetDatasetConfiguration(cacheConfigurationOverride);

            MetadataEntity = new MetadataContainerEntity(new DatasetMetadataDataSource(Descriptor, m_DataSource, MetadataDataSourceSpecification.metadata));
            SystemMetadataEntity = new ReadOnlyMetadataContainerEntity(new DatasetMetadataDataSource(Descriptor, m_DataSource, MetadataDataSourceSpecification.systemMetadata));
        }

        /// <inheritdoc />
        public Task<IDataset> WithCacheConfigurationAsync(DatasetCacheConfiguration datasetConfiguration, CancellationToken cancellationToken)
        {
            return GetConfiguredAsync(m_DataSource, DefaultCacheConfiguration, Descriptor, datasetConfiguration, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<DatasetProperties> GetPropertiesAsync(CancellationToken cancellationToken)
        {
            if (CacheConfiguration.CacheProperties)
            {
                return Properties;
            }

            var fieldsFilter = FieldsFilter.DefaultDatasetIncludes;
            var data = await m_DataSource.GetDatasetAsync(Descriptor, fieldsFilter, cancellationToken);
            return data.From(fieldsFilter.DatasetFields);
        }

        /// <inheritdoc />
        public DatasetUpdateHistoryQueryBuilder QueryUpdateHistory()
        {
            ThrowIfPathToLibrary();

            return new DatasetUpdateHistoryQueryBuilder(m_DataSource, Descriptor);
        }

        /// <inheritdoc />
        public async Task<DatasetUpdateHistory> GetUpdateHistoryAsync(int sequenceNumber, CancellationToken cancellationToken)
        {
            ThrowIfPathToLibrary();

            var count = await m_DataSource.GetMetadataHistoryCountAsync(Descriptor, cancellationToken);

            if (sequenceNumber < 0 || sequenceNumber >= count)
            {
                throw new InvalidArgumentException($"The sequence number must be between 0 and {count}.");
            }

            var range = new Range(count - 1 - sequenceNumber, count - sequenceNumber);
            var query = m_DataSource
                .ListMetadataHistoryAsync(Descriptor, new PaginationData {Range = range}, cancellationToken)
                .GetAsyncEnumerator(cancellationToken);

            if (await query.MoveNextAsync())
            {
                if (query.Current.MetadataSequenceNumber == sequenceNumber)
                {
                    return query.Current.From(m_DataSource, Descriptor);
                }
            }

            throw new NotFoundException($"History with sequence number {sequenceNumber} not found for dataset.");
        }

        /// <inheritdoc />
        public async Task RefreshAsync(CancellationToken cancellationToken)
        {
            if (CacheConfiguration.HasCachingRequirements)
            {
                var fieldsFilter = m_CacheConfiguration.GetDatasetFieldsFilter();
                var data = await m_DataSource.GetDatasetAsync(Descriptor, fieldsFilter, cancellationToken);
                this.MapFrom(m_DataSource, data, fieldsFilter.DatasetFields);
            }
        }

        /// <inheritdoc />
        public async Task UpdateAsync(IDatasetUpdate datasetUpdate, CancellationToken cancellationToken)
        {
            ThrowIfPathToLibrary();

            await m_DataSource.UpdateDatasetAsync(Descriptor, datasetUpdate.From(), cancellationToken);
        }

        /// <inheritdoc />
        public Task UpdateAsync(int updateHistorySequenceNumber, CancellationToken cancellationToken)
        {
            ThrowIfPathToLibrary();

            return m_DataSource.RollbackMetadataHistoryAsync(Descriptor, updateHistorySequenceNumber, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IFile> AddExistingFileAsync(string filePath, DatasetId sourceDatasetId, CancellationToken cancellationToken)
        {
            ThrowIfPathToLibrary();

            await AddExistingFileLiteAsync(filePath, sourceDatasetId, cancellationToken);
            return await GetFileAsync(filePath, cancellationToken);
        }

        /// <inheritdoc />
        public Task AddExistingFileLiteAsync(string filePath, DatasetId sourceDatasetId, CancellationToken cancellationToken)
        {
            ThrowIfPathToLibrary();

            return m_DataSource.ReferenceFileFromDatasetAsync(Descriptor, sourceDatasetId, filePath, cancellationToken);
        }

        /// <inheritdoc />
        public Task RemoveFileAsync(string filePath, CancellationToken cancellationToken)
        {
            ThrowIfPathToLibrary();

            return m_DataSource.RemoveFileFromDatasetAsync(Descriptor, filePath, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IFile> GetFileAsync(string filePath, CancellationToken cancellationToken)
        {
            var fileDescriptor = new FileDescriptor(Descriptor, filePath);

            if (CacheConfiguration.CacheFileList)
            {
                var fileData = FileMap.GetValueOrDefault(filePath);
                return fileData?.From(m_DataSource, DefaultCacheConfiguration,
                    fileDescriptor, m_CacheConfiguration.GetFileFieldsFilter().FileFields,
                    CacheConfiguration.FileCacheConfiguration);
            }

            return await FileEntity.GetConfiguredAsync(m_DataSource, DefaultCacheConfiguration, fileDescriptor, CacheConfiguration.FileCacheConfiguration, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyDictionary<string, Uri>> GetDownloadUrlsAsync(CancellationToken cancellationToken)
        {
            var enumerable = m_DataSource.GetAssetDownloadUrlsAsync(Descriptor.AssetDescriptor, new[] {Descriptor.DatasetId}, Range.All, cancellationToken);

            var urls = new Dictionary<string, Uri>();
            await foreach (var url in enumerable)
            {
                urls.Add(url.FilePath, url.DownloadUrl);
            }

            return urls;
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<IFile> ListFilesAsync(Range range, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var fieldsFilter = m_CacheConfiguration.GetFileFieldsFilter();
            var data = CacheConfiguration.CacheFileList
                ? ListCachedFilesAsync(range, cancellationToken)
                : ListRemoteFilesAsync(range, fieldsFilter, cancellationToken);
            await foreach (var fileData in data.WithCancellation(cancellationToken))
            {
                yield return fileData.From(m_DataSource, DefaultCacheConfiguration, new FileDescriptor(Descriptor, fileData.Path), fieldsFilter.FileFields, CacheConfiguration.FileCacheConfiguration);
            }
        }

        async IAsyncEnumerable<IFileData> ListCachedFilesAsync(Range range, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var (start, length) = range.GetValidatedOffsetAndLength(Files.Count);
            for (var i = start; i < start + length; ++i)
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return Files[i];
            }

            await Task.CompletedTask;
        }

        IAsyncEnumerable<IFileData> ListRemoteFilesAsync(Range range, FieldsFilter includedFieldsFilter, CancellationToken cancellationToken)
        {
            return m_DataSource.ListFilesAsync(Descriptor, range, includedFieldsFilter, cancellationToken);
        }

        /// <inheritdoc />
        public Uri GetFileUrl(string filePath)
        {
            filePath = Uri.EscapeDataString(filePath);
            var uriRelativePath = $"/assets/storage/v1/projects/{Descriptor.ProjectId}/assets/{Descriptor.AssetId}/versions/{Descriptor.AssetVersion}/datasets/{Descriptor.DatasetId}/files/{filePath}";
            var fileUriBuilder = new UriBuilder(m_DataSource.GetServiceRequestUrl(uriRelativePath));

            return fileUriBuilder.Uri;
        }

        /// <inheritdoc />
        public async Task<IFile> UploadFileAsync(IFileCreation fileCreation, Stream sourceStream, IProgress<HttpProgress> progress, CancellationToken cancellationToken)
        {
            ThrowIfPathToLibrary();

            var fileDescriptor = await UploadFileLiteAsync(fileCreation, sourceStream, progress, cancellationToken);
            return await GetFileAsync(fileDescriptor.Path, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<FileDescriptor> UploadFileLiteAsync(IFileCreation fileCreation, Stream sourceStream, IProgress<HttpProgress> progress, CancellationToken cancellationToken)
        {
            ThrowIfPathToLibrary();

            var creationData = fileCreation.From();
            creationData.SizeBytes = sourceStream.Length;
            creationData.UserChecksum = await Utilities.CalculateMD5ChecksumAsync(sourceStream, cancellationToken);
            
            var filePath = creationData.Path;

            var uploadUrl = await m_DataSource.CreateFileAsync(Descriptor, creationData, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                await m_DataSource.RemoveFileFromDatasetAsync(Descriptor, filePath, default);
                cancellationToken.ThrowIfCancellationRequested();
            }

            // Creation may have failed to get the upload url, try to get it again
            if (uploadUrl == null)
            {
                uploadUrl = await m_DataSource.GetFileUploadUrlAsync(new FileDescriptor(Descriptor, filePath), null, cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                await m_DataSource.RemoveFileFromDatasetAsync(Descriptor, filePath, default);
                cancellationToken.ThrowIfCancellationRequested();
            }

            if (uploadUrl != null)
            {
                try
                {
                    await m_DataSource.UploadContentAsync(uploadUrl, sourceStream, progress, cancellationToken);
                    await m_DataSource.FinalizeFileUploadAsync(new FileDescriptor(Descriptor, filePath), fileCreation.DisableAutomaticTransformations, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    await m_DataSource.RemoveFileFromDatasetAsync(Descriptor, filePath, default);
                    throw;
                }
            }

            return new FileDescriptor(Descriptor, filePath);
        }

        /// <inheritdoc />
        public async Task<ITransformation> StartTransformationAsync(ITransformationCreation transformationCreation, CancellationToken cancellationToken)
        {
            ThrowIfPathToLibrary();

            var transformationDescriptor = await StartTransformationLiteAsync(transformationCreation, cancellationToken);
            return await GetTransformationAsync(transformationDescriptor.TransformationId, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<TransformationDescriptor> StartTransformationLiteAsync(ITransformationCreation transformationCreation, CancellationToken cancellationToken)
        {
            ThrowIfPathToLibrary("Cannot access transformations for library datasets.");

            string workflowName;
            switch (transformationCreation.WorkflowType)
            {
                case WorkflowType.Custom:
                    if (string.IsNullOrEmpty(transformationCreation.CustomWorkflowName))
                    {
                        throw new InvalidArgumentException($"A workflow name must be provided when {nameof(WorkflowType.Custom)} is selected.");
                    }

                    workflowName = transformationCreation.CustomWorkflowName;
                    break;
                default:
                    workflowName = transformationCreation.WorkflowType.ToJsonValue();
                    break;
            }

            var transformationId = await m_DataSource.StartTransformationAsync(Descriptor, workflowName, transformationCreation.InputFilePaths, transformationCreation.GetExtraParameters(), cancellationToken);
            return new TransformationDescriptor(Descriptor, transformationId);
        }

        /// <inheritdoc />
        public IAsyncEnumerable<ITransformation> ListTransformationsAsync(Range range, CancellationToken cancellationToken)
        {
            ThrowIfPathToLibrary("Cannot access transformations for library datasets.");

            var searchFilter = new TransformationSearchFilter();
            searchFilter.AssetId.WhereEquals(Descriptor.AssetId);
            searchFilter.AssetVersion.WhereEquals(Descriptor.AssetVersion);
            searchFilter.DatasetId.WhereEquals(Descriptor.DatasetId);

            return new TransformationQueryBuilder(m_DataSource, DefaultCacheConfiguration, Descriptor.AssetDescriptor.ProjectDescriptor)
                .SelectWhereMatchesFilter(searchFilter)
                .LimitTo(range)
                .ExecuteAsync(cancellationToken);
        }

        /// <inheritdoc />
        public Task<ITransformation> GetTransformationAsync(TransformationId transformationId, CancellationToken cancellationToken)
        {
            ThrowIfPathToLibrary("Cannot access transformations for library datasets.");

            var descriptor = new TransformationDescriptor(Descriptor, transformationId);
            return TransformationEntity.GetConfiguredAsync(m_DataSource, DefaultCacheConfiguration, descriptor, null, cancellationToken);
        }

        /// <summary>
        /// Returns a dataset configured with the specified cache configuration.
        /// </summary>
        internal static async Task<IDataset> GetConfiguredAsync(IAssetDataSource dataSource, AssetRepositoryCacheConfiguration defaultCacheConfiguration, DatasetDescriptor descriptor, DatasetCacheConfiguration? configuration, CancellationToken cancellationToken)
        {
            var dataset = new DatasetEntity(dataSource, defaultCacheConfiguration, descriptor, configuration);

            if (dataset.CacheConfiguration.HasCachingRequirements)
            {
                await dataset.RefreshAsync(cancellationToken);
            }

            return dataset;
        }

        void ThrowIfPathToLibrary(string message = "Cannot modify library datasets.")
        {
            if (Descriptor.IsPathToAssetLibrary())
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
