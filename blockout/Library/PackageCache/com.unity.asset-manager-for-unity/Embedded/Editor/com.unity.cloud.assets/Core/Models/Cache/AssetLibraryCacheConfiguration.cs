namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// Sets the cache configuration for a library.
    /// </summary>
    struct AssetLibraryCacheConfiguration
    {
        /// <summary>
        /// Whether to cache properties of the asset library.
        /// </summary>
        public bool CacheProperties { get; set; }

        public static AssetLibraryCacheConfiguration NoCaching => new()
        {
            CacheProperties = false,
        };

        internal static AssetLibraryCacheConfiguration Legacy => new()
        {
            CacheProperties = true,
        };

        internal bool HasCachingRequirements => CacheProperties;

        internal AssetLibraryCacheConfiguration(AssetRepositoryCacheConfiguration defaultCacheConfiguration)
        {
            CacheProperties = defaultCacheConfiguration.AssetLibraryCacheConfiguration.CacheProperties;
        }
    }
}
