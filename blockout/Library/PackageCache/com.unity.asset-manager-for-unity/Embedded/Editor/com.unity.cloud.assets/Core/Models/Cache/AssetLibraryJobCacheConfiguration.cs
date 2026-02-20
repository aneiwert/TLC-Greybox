namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// Sets the cache configuration for a library.
    /// </summary>
    struct AssetLibraryJobCacheConfiguration
    {
        /// <summary>
        /// Whether to cache properties of the asset library.
        /// </summary>
        public bool CacheProperties { get; set; }

        public static AssetLibraryJobCacheConfiguration NoCaching => new()
        {
            CacheProperties = false,
        };

        internal static AssetLibraryJobCacheConfiguration Legacy => new()
        {
            CacheProperties = true,
        };

        internal bool HasCachingRequirements => CacheProperties;

        internal AssetLibraryJobCacheConfiguration(AssetRepositoryCacheConfiguration _)
        {
            CacheProperties = true;
        }
    }
}
