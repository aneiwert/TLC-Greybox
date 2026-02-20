using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Unity.Cloud.AssetsEmbedded
{
    interface IAssetMetadataHistory : IEntityMetadataHistory
    {
        [DataMember(Name = "child")]
        MetadataHistoryChild? Child { get; }
        
        /// <inheritdoc cref="IAssetBaseData.Name"/>
        [DataMember(Name = "name")]
        string Name { get; }

        /// <inheritdoc cref="IAssetBaseData.Type"/>
        [DataMember(Name = "primaryType")]
        string Type { get; }
        
        /// <inheritdoc cref="IAssetUpdateData.PreviewFile"/>
        [DataMember(Name = "previewFile")]
        string PreviewFile { get; }
        
        /// <inheritdoc cref="IAssetData.Changelog"/>
        [DataMember(Name = "changeLog")]
        string Changelog { get; }

        /// <inheritdoc cref="IAssetBaseData.Description"/>
        [DataMember(Name = "description")]
        string Description { get; }

        /// <inheritdoc cref="IAssetBaseData.Tags"/>
        [DataMember(Name = "tags")]
        IEnumerable<string> Tags { get; }
    }

    [DataContract]
    struct MetadataHistoryChild
    {
        [DataMember(Name = "childType")]
        public string Type { get; set; }

        [DataMember(Name = "sequenceNumber")]
        public int SequenceNumber { get; set; }
        
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "fileInfo")]
        public FileInfo? FileInfo { get; set; }
    }

    [DataContract]
    struct FileInfo
    {
        [DataMember(Name = "datasetId")]
        public string DatasetId { get; set; }
        
        [DataMember(Name = "filePath")]
        public string Path { get; set; }
    }
}
