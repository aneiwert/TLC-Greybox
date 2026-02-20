using System;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    static partial class EntityMapper
    {
        internal static void MapFrom(this AssetLibraryJobEntity job, ILibraryJobData jobData)
        {
            if (job.CacheConfiguration.CacheProperties)
                job.Properties = jobData.From();
        }

        internal static AssetLibraryJobEntity From(this ILibraryJobData data, IAssetDataSource assetDataSource, AssetRepositoryCacheConfiguration defaultCacheConfiguration,
            AssetLibraryJobCacheConfiguration? cacheConfigurationOverride = null)
        {
            return data.From(assetDataSource, defaultCacheConfiguration, data.Id, cacheConfigurationOverride);
        }

        internal static AssetLibraryJobEntity From(this ILibraryJobData data, IAssetDataSource assetDataSource, AssetRepositoryCacheConfiguration defaultCacheConfiguration,
            AssetLibraryJobId assetLibraryJobId, AssetLibraryJobCacheConfiguration? cacheConfigurationOverride = null)
        {
            var library = new AssetLibraryJobEntity(assetDataSource, defaultCacheConfiguration, assetLibraryJobId, cacheConfigurationOverride);
            library.MapFrom(data);
            return library;
        }

        internal static AssetLibraryJobProperties From(this ILibraryJobData data)
        {
            AssetDescriptor? copiedAssetDescriptor = null;
            if (data.Results.Exists)
            {
                copiedAssetDescriptor = new AssetDescriptor(data.Results.ProjectDescriptor, data.Results.AssetId, data.Results.AssetVersion);
            }
            
            return new AssetLibraryJobProperties
            {
                Name = data.Name,
                State = MapJobState(data.State),
                FailedReason = data.FailedReason,
                Progress = (int)MathF.Round((float)data.Progress.Value, MidpointRounding.AwayFromZero),
                ProgressDetails = data.Progress.Message,
                CopiedAssetDescriptor = copiedAssetDescriptor
            };
        }
        
        static AssetLibraryJobState MapJobState(string state)
        {
            return state switch
            {
                "active" => AssetLibraryJobState.Active,
                "completed" => AssetLibraryJobState.Completed,
                "failed" => AssetLibraryJobState.Failed,
                "prioritized" => AssetLibraryJobState.Prioritized,
                "delayed" => AssetLibraryJobState.Delayed,
                "waiting" => AssetLibraryJobState.Waiting,
                "waiting-children" => AssetLibraryJobState.Waiting, // This is a special case for jobs that are waiting for child jobs to complete.
                _ => AssetLibraryJobState.Unknown
            };
        }
    }
}
