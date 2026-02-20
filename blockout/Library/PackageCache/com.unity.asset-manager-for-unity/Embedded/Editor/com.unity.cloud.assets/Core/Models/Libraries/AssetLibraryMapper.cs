using System;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    static partial class EntityMapper
    {
        internal static void MapFrom(this AssetLibraryEntity library, ILibraryData libraryData)
        {
            if (library.CacheConfiguration.CacheProperties)
                library.Properties = libraryData.From();
        }

        internal static AssetLibraryEntity From(this ILibraryData data, IAssetDataSource assetDataSource, AssetRepositoryCacheConfiguration defaultCacheConfiguration,
            AssetLibraryCacheConfiguration? cacheConfigurationOverride = null)
        {
            return data.From(assetDataSource, defaultCacheConfiguration, data.Id, cacheConfigurationOverride);
        }

        internal static AssetLibraryEntity From(this ILibraryData data, IAssetDataSource assetDataSource, AssetRepositoryCacheConfiguration defaultCacheConfiguration,
            AssetLibraryId assetLibraryId, AssetLibraryCacheConfiguration? cacheConfigurationOverride = null)
        {
            var library = new AssetLibraryEntity(assetDataSource, defaultCacheConfiguration, assetLibraryId, cacheConfigurationOverride);
            library.MapFrom(data);
            return library;
        }

        internal static AssetLibraryProperties From(this ILibraryData data)
        {
            return new AssetLibraryProperties
            {
                Name = data.Name,
                HasCollection = data.HasCollection
            };
        }
    }
}
