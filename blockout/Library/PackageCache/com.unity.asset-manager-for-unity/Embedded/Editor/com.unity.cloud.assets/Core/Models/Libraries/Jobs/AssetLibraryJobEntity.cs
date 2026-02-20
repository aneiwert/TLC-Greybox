using System.Threading;
using System.Threading.Tasks;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// The <see cref="IAssetLibraryJob"/> implementation that represents a job in the asset library.
    /// </summary>
    class AssetLibraryJobEntity : IAssetLibraryJob
    {
        readonly IAssetDataSource m_DataSource;
        readonly AssetRepositoryCacheConfiguration m_DefaultCacheConfiguration;
        
        /// <inheritdoc />
        public AssetLibraryJobId Id { get; set; }

        public AssetLibraryJobCacheConfiguration CacheConfiguration { get; set; }
        
        internal AssetLibraryJobProperties Properties { get; set; }
        
        internal AssetLibraryJobEntity(IAssetDataSource dataSource, AssetRepositoryCacheConfiguration defaultCacheConfiguration, AssetLibraryJobId assetLibraryJobId, AssetLibraryJobCacheConfiguration? localCacheConfiguration = null)
        {
            m_DataSource = dataSource;
            m_DefaultCacheConfiguration = defaultCacheConfiguration;
            Id = assetLibraryJobId;

            CacheConfiguration = localCacheConfiguration ?? new AssetLibraryJobCacheConfiguration(m_DefaultCacheConfiguration);
        }

        public Task<IAssetLibraryJob> WithCacheConfigurationAsync(AssetLibraryJobCacheConfiguration assetLibraryJobCacheConfiguration, CancellationToken cancellationToken)
        {
            return GetConfiguredAsync(m_DataSource, m_DefaultCacheConfiguration, Id, assetLibraryJobCacheConfiguration, cancellationToken);
        }

        /// <inheritdoc />
        public async Task RefreshAsync(CancellationToken cancellationToken)
        {
            if (CacheConfiguration.HasCachingRequirements)
            {
                var data = await m_DataSource.GetLibraryJobAsync(Id, cancellationToken);
                this.MapFrom(data);
            }
        }

        /// <inheritdoc />
        public async Task<AssetLibraryJobProperties> GetPropertiesAsync(CancellationToken cancellationToken)
        {
            if (CacheConfiguration.CacheProperties)
            {
                return Properties;
            }

            var data = await m_DataSource.GetLibraryJobAsync(Id, cancellationToken);
            return data.From();
        }

        /// <summary>
        /// Returns a library job configured with the specified cache configuration.
        /// </summary>
        internal static async Task<IAssetLibraryJob> GetConfiguredAsync(IAssetDataSource dataSource, AssetRepositoryCacheConfiguration defaultCacheConfiguration, AssetLibraryJobId assetLibraryJobId, AssetLibraryJobCacheConfiguration? configuration, CancellationToken cancellationToken)
        {
            var job = new AssetLibraryJobEntity(dataSource, defaultCacheConfiguration, assetLibraryJobId, configuration);

            if (job.CacheConfiguration.HasCachingRequirements)
            {
                await job.RefreshAsync(cancellationToken);
            }

            return job;
        }
    }
}
