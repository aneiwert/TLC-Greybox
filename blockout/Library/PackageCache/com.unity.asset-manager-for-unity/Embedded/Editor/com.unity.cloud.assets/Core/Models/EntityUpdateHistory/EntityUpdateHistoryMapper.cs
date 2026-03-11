using System.Collections.Generic;
using System.Linq;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    static partial class EntityMapper
    {
        internal static AssetUpdateHistory From(this IAssetMetadataHistory data, IAssetDataSource assetDataSource, AssetDescriptor assetDescriptor)
        {
            return new AssetUpdateHistory
            {
                SequenceNumber = data.MetadataSequenceNumber,
                UpdatedFromSequenceNumber = data.ParentSequenceNumber,
                UpdatedBy = new UserId(data.CreatedBy),
                Updated = data.Created,
                Metadata = data.ParseMetadata(assetDataSource, assetDescriptor),
                Name = data.Name,
                Description = data.Description,
                Type = ParseAssetType(data.Type),
                Changelog = data.Changelog,
                Tags = data.Tags ?? new List<string>(),
                PreviewFilePath = data.PreviewFile,
                ChildDatasetUpdateHistoryDescriptor = data.Child?.AsDataset(assetDescriptor),
                ChildFileUpdateHistoryDescriptor = data.Child?.AsFile(assetDescriptor),
            };
        }

        internal static DatasetUpdateHistory From(this IDatasetMetadataHistory data, IAssetDataSource assetDataSource, DatasetDescriptor datasetDescriptor)
        {
            return new DatasetUpdateHistory
            {
                SequenceNumber = data.MetadataSequenceNumber,
                UpdatedFromSequenceNumber = data.ParentSequenceNumber,
                UpdatedBy = new UserId(data.CreatedBy),
                Updated = data.Created,
                Metadata = data.ParseMetadata(assetDataSource, datasetDescriptor.AssetDescriptor),
                Name = data.Name,
                Description = data.Description,
                Type = ParseAssetType(data.Type),
                Tags = data.Tags ?? new List<string>(),
                FileOrder = data.FileOrder ?? new List<string>(),
                IsVisible = data.IsVisible
            };
        }

        internal static FileUpdateHistory From(this IFileMetadataHistory data, IAssetDataSource assetDataSource, FileDescriptor fileDescriptor)
        {
            return new FileUpdateHistory
            {
                SequenceNumber = data.MetadataSequenceNumber,
                UpdatedFromSequenceNumber = data.ParentSequenceNumber,
                UpdatedBy = new UserId(data.CreatedBy),
                Updated = data.Created,
                Metadata = data.ParseMetadata(assetDataSource, fileDescriptor.DatasetDescriptor.AssetDescriptor),
                Description = data.Description,
                Tags = data.Tags ?? new List<string>(),
            };
        }

        static AssetType ParseAssetType(string typeString)
        {
            if (string.IsNullOrEmpty(typeString) || !typeString.TryGetAssetTypeFromString(out var assetType))
            {
                assetType = AssetType.Other;
            }
            return assetType;
        }

        static DatasetUpdateHistoryDescriptor? AsDataset(this MetadataHistoryChild data, AssetDescriptor assetDescriptor)
        {
            if (data.Type == "Dataset")
            {
                var datasetDescriptor = new DatasetDescriptor(assetDescriptor, new DatasetId(data.Id));
                return new DatasetUpdateHistoryDescriptor(datasetDescriptor, data.SequenceNumber);
            }

            return null;
        }

        static FileUpdateHistoryDescriptor? AsFile(this MetadataHistoryChild data, AssetDescriptor assetDescriptor)
        {
            if (data is {Type: "File", FileInfo: not null})
            {
                var datasetDescriptor = new DatasetDescriptor(assetDescriptor, new DatasetId(data.FileInfo.Value.DatasetId));
                var fileDescriptor = new FileDescriptor(datasetDescriptor, data.FileInfo.Value.Path);
                return new FileUpdateHistoryDescriptor(fileDescriptor, data.SequenceNumber);
            }

            return null;
        }

        static Dictionary<string, MetadataValue> ParseMetadata(this IEntityMetadataHistory data, IAssetDataSource assetDataSource, AssetDescriptor assetDescriptor)
        {
            var metadataDictionary = data.Metadata?.From(assetDataSource, assetDescriptor) ?? new Dictionary<string, MetadataObject>();
            return metadataDictionary.ToDictionary(kvp => kvp.Key, kvp => (MetadataValue) kvp.Value);
        }
    }
}
