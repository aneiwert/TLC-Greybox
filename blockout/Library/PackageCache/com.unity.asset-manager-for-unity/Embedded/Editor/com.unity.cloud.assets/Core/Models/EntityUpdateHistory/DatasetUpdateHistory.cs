using System;
using System.Collections.Generic;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// The update history properties of an asset version's dataset.
    /// </summary>
    struct DatasetUpdateHistory
    {
        /// <summary>
        /// The ordered sequence number of the update. The higher the number, the more recent the update.
        /// </summary>
        public int SequenceNumber { get; internal set; }

        /// <summary>
        /// The sequence number this update was created from, if any.
        /// </summary>
        public int? UpdatedFromSequenceNumber { get; internal set; }

        /// <summary>
        /// The ID of the user that made the update.
        /// </summary>
        public UserId UpdatedBy { get; internal set; }

        /// <summary>
        /// The date and time when the update occurred.
        /// </summary>
        public DateTime Updated { get; internal set; }

        /// <summary>
        /// The name of the dataset.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// A description of the dataset.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// The type of the dataset.
        /// </summary>
        public AssetType Type { get; internal set; }

        /// <summary>
        /// The user tags of the dataset.
        /// </summary>
        public IEnumerable<string> Tags { get; internal set; }

        /// <summary>
        /// The order of the files in the dataset.
        /// </summary>
        public IEnumerable<string> FileOrder { get; internal set; }

        /// <summary>
        /// Indicates whether the dataset is visible or not.
        /// </summary>
        public bool IsVisible { get; internal set; }

        /// <summary>
        /// The metadata of the dataset.
        /// </summary>
        public IReadOnlyDictionary<string, MetadataValue> Metadata { get; internal set; }
    }
}
