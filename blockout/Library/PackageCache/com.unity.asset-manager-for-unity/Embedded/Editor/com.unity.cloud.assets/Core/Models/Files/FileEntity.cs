using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    class FileEntity : IFile
    {
        readonly IAssetDataSource m_DataSource;
        readonly CacheConfigurationWrapper m_CacheConfiguration;

        /// <inheritdoc />
        public FileDescriptor Descriptor { get; }

        /// <inheritdoc />
        public string Description => Properties.Description;

        /// <inheritdoc />
        public string Status => Properties.StatusName;

        /// <inheritdoc />
        public AuthoringInfo AuthoringInfo => Properties.AuthoringInfo;

        /// <inheritdoc />
        public IEnumerable<string> Tags => Properties.Tags;

        /// <inheritdoc />
        public IEnumerable<string> SystemTags => Properties.SystemTags;

        /// <inheritdoc />
        public IEnumerable<DatasetDescriptor> LinkedDatasets => Properties.LinkedDatasets;

        /// <inheritdoc />
        public long SizeBytes => Properties.SizeBytes;

        /// <inheritdoc />
        public string UserChecksum => Properties.UserChecksum;

        /// <inheritdoc />
        public IMetadataContainer Metadata => MetadataEntity;

        /// <inheritdoc />
        public IReadOnlyMetadataContainer SystemMetadata => SystemMetadataEntity;

        /// <inheritdoc />
        public FileCacheConfiguration CacheConfiguration => m_CacheConfiguration.FileConfiguration;

        AssetRepositoryCacheConfiguration DefaultCacheConfiguration => m_CacheConfiguration.DefaultConfiguration;

        internal FileProperties Properties { get; set; }
        internal Uri PreviewUrl { get; set; }
        internal Uri DownloadUrl { get; set; }
        internal MetadataContainerEntity MetadataEntity { get; }
        internal ReadOnlyMetadataContainerEntity SystemMetadataEntity { get; }

        internal FileEntity(IAssetDataSource dataSource, AssetRepositoryCacheConfiguration defaultCacheConfiguration, FileDescriptor descriptor, FileCacheConfiguration? cacheConfigurationOverride = null)
        {
            m_DataSource = dataSource;
            Descriptor = descriptor;

            m_CacheConfiguration = new CacheConfigurationWrapper(defaultCacheConfiguration);
            m_CacheConfiguration.SetFileConfiguration(cacheConfigurationOverride);

            MetadataEntity = new MetadataContainerEntity(new FileMetadataDataSource(Descriptor, m_DataSource, MetadataDataSourceSpecification.metadata));
            SystemMetadataEntity = new ReadOnlyMetadataContainerEntity(new FileMetadataDataSource(Descriptor, m_DataSource, MetadataDataSourceSpecification.systemMetadata));
        }

        /// <inheritdoc />
        public Task<IFile> WithCacheConfigurationAsync(FileCacheConfiguration fileConfiguration, CancellationToken cancellationToken)
        {
            return GetConfiguredAsync(m_DataSource, DefaultCacheConfiguration, Descriptor, fileConfiguration, cancellationToken);
        }

        /// <inheritdoc />
        public IFile WithDataset(DatasetDescriptor datasetDescriptor)
        {
            if (datasetDescriptor == Descriptor.DatasetDescriptor) return this;

            if (CacheConfiguration.CacheProperties && Properties.LinkedDatasets.All(d => d.DatasetId != datasetDescriptor.DatasetId))
            {
                throw new InvalidArgumentException("The file does not belong to the specified dataset.");
            }

            var descriptor = new FileDescriptor(datasetDescriptor, Descriptor.Path);
            return new FileEntity(m_DataSource, DefaultCacheConfiguration, descriptor, CacheConfiguration)
            {
                Properties = Properties,
                PreviewUrl = PreviewUrl,
                DownloadUrl = DownloadUrl,
                MetadataEntity = {Properties = MetadataEntity.Properties},
                SystemMetadataEntity = {Properties = SystemMetadataEntity.Properties},
            };
        }

        /// <inheritdoc />
        public Task<IFile> WithDatasetAsync(DatasetDescriptor datasetDescriptor, CancellationToken cancellationToken)
        {
            var fileDescriptor = new FileDescriptor(datasetDescriptor, Descriptor.Path);
            return GetConfiguredAsync(m_DataSource, DefaultCacheConfiguration, fileDescriptor, CacheConfiguration, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<FileProperties> GetPropertiesAsync(CancellationToken cancellationToken)
        {
            if (CacheConfiguration.CacheProperties)
            {
                return Properties;
            }

            var fieldsFilter = FieldsFilter.DefaultFileIncludes;
            var data = await m_DataSource.GetFileAsync(Descriptor, fieldsFilter, cancellationToken);
            return data.From(Descriptor, fieldsFilter.FileFields);
        }

        /// <inheritdoc />
        public FileUpdateHistoryQueryBuilder QueryUpdateHistory()
        {
            ThrowIfPathToLibrary();

            return new FileUpdateHistoryQueryBuilder(m_DataSource, Descriptor);
        }

        /// <inheritdoc />
        public async Task<FileUpdateHistory> GetUpdateHistoryAsync(int sequenceNumber, CancellationToken cancellationToken)
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

            throw new NotFoundException($"History with sequence number {sequenceNumber} not found for file.");
        }

        /// <inheritdoc />
        public async Task RefreshAsync(CancellationToken cancellationToken)
        {
            if (CacheConfiguration.HasCachingRequirements)
            {
                var fieldsFilter = m_CacheConfiguration.GetFileFieldsFilter();
                var data = await m_DataSource.GetFileAsync(Descriptor, fieldsFilter, cancellationToken);
                this.MapFrom(m_DataSource, data, fieldsFilter.FileFields);
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<IDataset> GetLinkedDatasetsAsync(Range range, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var properties = await GetPropertiesAsync(cancellationToken);

            var linkedDataset = properties.LinkedDatasets.ToArray();

            var (start, length) = range.GetValidatedOffsetAndLength(linkedDataset.Length);
            for (var i = start; i < start + length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return await DatasetEntity.GetConfiguredAsync(m_DataSource, DefaultCacheConfiguration, linkedDataset[i], null, cancellationToken);
            }
        }

        /// <inherticdoc />
        public async Task<Uri> GetPreviewUrlAsync(CancellationToken cancellationToken)
        {
            if (CacheConfiguration.CachePreviewUrl)
            {
                return PreviewUrl;
            }

            var filter = new FieldsFilter {FileFields = FileFields.previewURL};
            var data = await m_DataSource.GetFileAsync(Descriptor, filter, cancellationToken);
            this.MapFrom(m_DataSource, data, filter.FileFields);
            return PreviewUrl;
        }

        /// <inheritdoc />
        public async Task<Uri> GetDownloadUrlAsync(CancellationToken cancellationToken)
        {
            if (CacheConfiguration.CacheDownloadUrl)
            {
                return DownloadUrl;
            }

            return await m_DataSource.GetFileDownloadUrlAsync(Descriptor, null, cancellationToken);
        }

        /// <inheritdoc />
        public async Task DownloadAsync(Stream targetStream, IProgress<HttpProgress> progress, CancellationToken cancellationToken)
        {
            var downloadUrl = await GetDownloadUrlAsync(cancellationToken);

            if (downloadUrl == null)
            {
                return;
            }

            await m_DataSource.DownloadContentAsync(downloadUrl, targetStream, progress, cancellationToken);
        }

        /// <inheritdoc />
        public Task<Uri> GetResizedImageDownloadUrlAsync(int maxDimension, CancellationToken cancellationToken)
        {
            return m_DataSource.GetFileDownloadUrlAsync(Descriptor, maxDimension, cancellationToken);
        }

        /// <inheritdoc />
        public async Task UpdateAsync(IFileUpdate fileUpdate, CancellationToken cancellationToken)
        {
            ThrowIfPathToLibrary();

            await m_DataSource.UpdateFileAsync(Descriptor, fileUpdate.From(), cancellationToken);
        }

        /// <inheritdoc />
        public Task UpdateAsync(int updateHistorySequenceNumber, CancellationToken cancellationToken)
        {
            ThrowIfPathToLibrary();

            return m_DataSource.RollbackMetadataHistoryAsync(Descriptor, updateHistorySequenceNumber, cancellationToken);
        }

        /// <inheritdoc />
        public async Task UploadAsync(Stream sourceStream, IProgress<HttpProgress> progress, CancellationToken cancellationToken)
        {
            ThrowIfPathToLibrary();

            var creationData = new FileCreateData
            {
                SizeBytes = sourceStream.Length,
                UserChecksum = await Utilities.CalculateMD5ChecksumAsync(sourceStream, cancellationToken)
            };

            var uploadUrl = await m_DataSource.UpdateFileContentAsync(Descriptor, creationData, cancellationToken);

            if (uploadUrl == null)
            {
                uploadUrl = await m_DataSource.GetFileUploadUrlAsync(Descriptor, null, cancellationToken);
            }

            if (uploadUrl != null)
            {
                await m_DataSource.UploadContentAsync(uploadUrl, sourceStream, progress, cancellationToken);
                await m_DataSource.FinalizeFileUploadAsync(Descriptor, false, cancellationToken);
            }
        }

        /// <summary>
        /// Returns the entire list of linked datasets configured with no caching.
        /// </summary>
        async IAsyncEnumerable<IDataset> GetLinkedDatasetsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var properties = await GetPropertiesAsync(cancellationToken);

            foreach (var linkedDataset in properties.LinkedDatasets)
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return await DatasetEntity.GetConfiguredAsync(m_DataSource, AssetRepositoryCacheConfiguration.NoCaching, linkedDataset, null, cancellationToken);
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<GeneratedTag>> GenerateSuggestedTagsAsync(CancellationToken cancellationToken)
        {
            ThrowIfPathToLibrary("Cannot generate tags for library files.");

            var tags = await m_DataSource.GenerateFileTagsAsync(Descriptor, cancellationToken);
            return tags.Select(x => new GeneratedTag(x.Tag, x.Confidence));
        }

        /// <summary>
        /// Returns a file configured with the specified cache configuration.
        /// </summary>
        internal static async Task<IFile> GetConfiguredAsync(IAssetDataSource dataSource, AssetRepositoryCacheConfiguration defaultCacheConfiguration, FileDescriptor descriptor, FileCacheConfiguration? configuration, CancellationToken cancellationToken)
        {
            var file = new FileEntity(dataSource, defaultCacheConfiguration, descriptor, configuration);

            if (file.CacheConfiguration.HasCachingRequirements)
            {
                await file.RefreshAsync(cancellationToken);
            }

            return file;
        }

        void ThrowIfPathToLibrary(string message = "Cannot modify a library file.")
        {
            if (Descriptor.IsPathToAssetLibrary())
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
