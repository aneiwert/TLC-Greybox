using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Unity.Cloud.AssetsEmbedded
{
    [DataContract]
    class AssetMetadataHistory : EntityMetadataHistory, IAssetMetadataHistory
    {
        /// <inheritdoc />
        public MetadataHistoryChild? Child { get; set; }

        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public string Type { get; set; }

        /// <inheritdoc />
        public string PreviewFile { get; set; }
        
        /// <inheritdoc />
        public string Changelog { get; set; }

        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public IEnumerable<string> Tags { get; set; }
    }
}
