using System.Collections.Generic;

namespace Unity.Cloud.AssetsEmbedded
{
    class DatasetMetadataHistory : EntityMetadataHistory, IDatasetMetadataHistory
    {
        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public string Type { get; set; }

        /// <inheritdoc />
        public IEnumerable<string> FileOrder { get; set; }

        /// <inheritdoc />
        public bool IsVisible { get; set; }

        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public IEnumerable<string> Tags { get; set; }
    }
}
