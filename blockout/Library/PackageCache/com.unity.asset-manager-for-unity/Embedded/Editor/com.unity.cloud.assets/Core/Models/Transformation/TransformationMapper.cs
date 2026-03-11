using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    static partial class EntityMapper
    {
        internal static void MapFrom(this TransformationEntity transformation, ITransformationData data)
        {
            if (transformation.CacheConfiguration.CacheProperties)
                transformation.Properties = data.From();
        }

        internal static TransformationProperties From(this ITransformationData data)
        {
            return new TransformationProperties
            {
                OutputDatasetId = data.OutputDatasetId,
                LinkDatasetId = data.LinkDatasetId,
                InputFilePaths = data.InputFiles,
                WorkflowType = FromJsonValue(data.WorkflowType),
                WorkflowName = data.WorkflowType,
                Status = data.Status,
                ErrorMessage = data.ErrorMessage,
                Progress = data.Progress,
                Created = data.CreatedOn,
                Updated = data.UpdatedAt,
                Started = data.StartedAt,
                UserId = data.UserId,
                JobId = data.JobId
            };
        }

        internal static ITransformation From(this ITransformationData data, IAssetDataSource assetDataSource, AssetRepositoryCacheConfiguration defaultCacheConfiguration,
            TransformationDescriptor transformationDescriptor, TransformationCacheConfiguration? cacheConfigurationOverride = null)
        {
            var transformation = new TransformationEntity(assetDataSource, defaultCacheConfiguration, transformationDescriptor, cacheConfigurationOverride);
            transformation.MapFrom(data);
            return transformation;
        }

        internal static ITransformation From(this ITransformationData data, IAssetDataSource assetDataSource, AssetRepositoryCacheConfiguration defaultCacheConfiguration,
            ProjectDescriptor projectDescriptor, TransformationCacheConfiguration? cacheConfigurationOverride = null)
        {
            var assetDescriptor = new AssetDescriptor(projectDescriptor, data.AssetId, data.AssetVersion);
            var datasetDescriptor = new DatasetDescriptor(assetDescriptor, data.InputDatasetId);
            return data.From(assetDataSource, defaultCacheConfiguration, new TransformationDescriptor(datasetDescriptor, data.Id), cacheConfigurationOverride);
        }
        
        internal static string ToJsonValue(this WorkflowType workflowType)
        {
            return workflowType switch
            {
                WorkflowType.Thumbnail_Generation => "asset-manager-thumbnail-generator",
                WorkflowType.GLB_Preview => "asset-manager-glb-preview",
                WorkflowType.Data_Streaming => "3d-data-streaming",
                WorkflowType.Transcode_Video => "asset-manager-video-transcoding",
                WorkflowType.Metadata_Extraction => "asset-manager-metadata-extraction",
                WorkflowType.Optimize_Convert_Free => "free-tier-optimize-and-convert",
                WorkflowType.Optimize_Convert_Pro => "higher-tier-optimize-and-convert",
#pragma warning disable CS0618 // Type or member is obsolete - maintained for backwards compatibility
                WorkflowType.Generic_Polygon_Target => "generic-polygon-target",
#pragma warning restore CS0618 // Type or member is obsolete
                _ => string.Empty
            };
        }

        static WorkflowType FromJsonValue(string value)
        {
            return value switch
            {
                "asset-manager-thumbnail-generator" => WorkflowType.Thumbnail_Generation,
                "asset-manager-glb-preview" => WorkflowType.GLB_Preview,
                "3d-data-streaming" => WorkflowType.Data_Streaming,
                "asset-manager-video-transcoding" => WorkflowType.Transcode_Video,
                "asset-manager-metadata-extraction" => WorkflowType.Metadata_Extraction,
                "free-tier-optimize-and-convert" => WorkflowType.Optimize_Convert_Free,
                "higher-tier-optimize-and-convert" => WorkflowType.Optimize_Convert_Pro,
#pragma warning disable CS0618 // Type or member is obsolete - maintained for backwards compatibility
                "generic-polygon-target" => WorkflowType.Generic_Polygon_Target,
#pragma warning restore CS0618 // Type or member is obsolete

                // The following values are obsolete but maintained for backwards compatibility
                "thumbnail-generator" => WorkflowType.Thumbnail_Generation,
                "glb-preview" => WorkflowType.GLB_Preview,
                "video-transcoding" => WorkflowType.Transcode_Video,
                "metadata-extraction" => WorkflowType.Metadata_Extraction,
                _ => default
            };
        }
    }
}
