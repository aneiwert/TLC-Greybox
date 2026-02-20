using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// The <see cref="IAssetLibrary"/> implementation that represents an asset library.
    /// </summary>
    sealed class AssetLibraryEntity : IAssetLibrary
    {
        readonly IAssetDataSource m_DataSource;
        readonly AssetRepositoryCacheConfiguration m_DefaultCacheConfiguration;

        /// <inheritdoc />
        public AssetLibraryId Id { get; }

        /// <inheritdoc/>
        public AssetLibraryCacheConfiguration CacheConfiguration { get; }

        internal AssetLibraryProperties Properties { get; set; }

        internal AssetLibraryEntity(IAssetDataSource dataSource, AssetRepositoryCacheConfiguration defaultCacheConfiguration, AssetLibraryId assetLibraryId, AssetLibraryCacheConfiguration? localCacheConfiguration = null)
        {
            m_DataSource = dataSource;
            m_DefaultCacheConfiguration = defaultCacheConfiguration;
            Id = assetLibraryId;

            CacheConfiguration = localCacheConfiguration ?? new AssetLibraryCacheConfiguration(m_DefaultCacheConfiguration);
        }

        /// <inheritdoc />
        public Task<IAssetLibrary> WithCacheConfigurationAsync(AssetLibraryCacheConfiguration assetLibraryCacheConfiguration, CancellationToken cancellationToken)
        {
            return GetConfiguredAsync(m_DataSource, m_DefaultCacheConfiguration, Id, assetLibraryCacheConfiguration, cancellationToken);
        }

        /// <inheritdoc />
        public async Task RefreshAsync(CancellationToken cancellationToken)
        {
            if (CacheConfiguration.HasCachingRequirements)
            {
                var data = await m_DataSource.GetLibraryAsync(Id, cancellationToken);
                this.MapFrom(data);
            }
        }

        /// <inheritdoc />
        public async Task<AssetLibraryProperties> GetPropertiesAsync(CancellationToken cancellationToken)
        {
            if (CacheConfiguration.CacheProperties)
            {
                return Properties;
            }

            var data = await m_DataSource.GetLibraryAsync(Id, cancellationToken);
            return data.From();
        }

        /// <inheritdoc />
        public AssetQueryBuilder QueryAssets()
        {
            return new AssetQueryBuilder(m_DataSource, m_DefaultCacheConfiguration, Id);
        }

        /// <inheritdoc />
        public Task<IAsset> GetAssetAsync(AssetId assetId, AssetVersion assetVersion, CancellationToken cancellationToken)
        {
            var assetDescriptor = new AssetDescriptor(Id, assetId, assetVersion);
            return AssetEntity.GetConfiguredAsync(m_DataSource, m_DefaultCacheConfiguration, assetDescriptor, null, cancellationToken);
        }

        /// <inheritdoc />
        public VersionQueryBuilder QueryAssetVersions(AssetId assetId)
        {
            return new VersionQueryBuilder(m_DataSource, m_DefaultCacheConfiguration, Id, assetId);
        }

        /// <inheritdoc />
        public AssetLabelQueryBuilder QueryAssetLabels(AssetId assetId)
        {
            return new AssetLabelQueryBuilder(m_DataSource, Id, assetId);
        }

        /// <inheritdoc />
        public GroupAndCountAssetsQueryBuilder GroupAndCountAssets()
        {
            return new GroupAndCountAssetsQueryBuilder(m_DataSource, Id);
        }

        /// <inheritdoc />
        public Task<int> CountAssetsAsync(CancellationToken cancellationToken)
        {
            return m_DataSource.GetAssetCountAsync(Id, cancellationToken);
        }

        /// <inheritdoc />
        public CollectionQueryBuilder QueryCollections()
        {
            return new CollectionQueryBuilder(m_DataSource, m_DefaultCacheConfiguration, Id);
        }

        /// <inheritdoc />
        public Task<IAssetCollection> GetAssetCollectionAsync(CollectionPath collectionPath, CancellationToken cancellationToken)
        {
            var descriptor = new CollectionDescriptor(Id, collectionPath);
            return AssetCollection.GetConfiguredAsync(m_DataSource, m_DefaultCacheConfiguration, descriptor, null, cancellationToken);
        }

        /// <inheritdoc />
        public FieldDefinitionQueryBuilder QueryFieldDefinitions(IEnumerable<string> fieldDefinitionKeys)
        {
            return new FieldDefinitionQueryBuilder(m_DataSource, m_DefaultCacheConfiguration, Id, fieldDefinitionKeys);
        }

        /// <inheritdoc />
        public Task<IFieldDefinition> GetFieldDefinitionAsync(string fieldKey, CancellationToken cancellationToken)
        {
            var fieldDefinitionDescriptor = new FieldDefinitionDescriptor(Id, fieldKey);
            return FieldDefinitionEntity.GetConfiguredAsync(m_DataSource, m_DefaultCacheConfiguration, fieldDefinitionDescriptor, null, cancellationToken);
        }

        /// <inheritdoc />
        public LabelQueryBuilder QueryLabels()
        {
            return new LabelQueryBuilder(m_DataSource, m_DefaultCacheConfiguration, Id);
        }

        /// <inheritdoc />
        public Task<ILabel> GetLabelAsync(string labelName, CancellationToken cancellationToken)
        {
            var labelDescriptor = new LabelDescriptor(Id, labelName);
            return LabelEntity.GetConfiguredAsync(m_DataSource, m_DefaultCacheConfiguration, labelDescriptor, null, cancellationToken);
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<IAssetLibraryJob> StartCopyAssetsJobAsync(ProjectDescriptor destinationProjectDescriptor, AssetsToCopy assetsToCopy, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (assetsToCopy?.Data.Any() != true)
            {
                throw new InvalidOperationException("No assets have been selected for copy.");
            }

            var request = m_DataSource.StartLibraryJobAsync(Id, destinationProjectDescriptor.ProjectId, assetsToCopy.Data, cancellationToken);
            await foreach (var result in request)
            {
                yield return result.From(m_DataSource, m_DefaultCacheConfiguration);
            }
        }

        /// <summary>
        /// Returns a library configured with the specified cache configuration.
        /// </summary>
        internal static async Task<IAssetLibrary> GetConfiguredAsync(IAssetDataSource dataSource, AssetRepositoryCacheConfiguration defaultCacheConfiguration, AssetLibraryId assetLibraryId, AssetLibraryCacheConfiguration? configuration, CancellationToken cancellationToken)
        {
            var library = new AssetLibraryEntity(dataSource, defaultCacheConfiguration, assetLibraryId, configuration);

            if (library.CacheConfiguration.HasCachingRequirements)
            {
                await library.RefreshAsync(cancellationToken);
            }

            return library;
        }
    }
}
