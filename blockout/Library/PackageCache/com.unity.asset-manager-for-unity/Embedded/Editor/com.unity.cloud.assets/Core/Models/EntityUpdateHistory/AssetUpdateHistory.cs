using System;
using System.Collections.Generic;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// The update history properties of an asset version.
    /// </summary>
    struct AssetUpdateHistory
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
        /// The name of the asset.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// The description of the asset.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// The type of the asset.
        /// </summary>
        public AssetType Type { get; internal set; }

        /// <summary>
        /// The tags of the asset.
        /// </summary>
        public IEnumerable<string> Tags { get; internal set; }
        
        /// <summary>
        /// The change log of the asset version.
        /// </summary>
        public string Changelog { get; internal set; }

        /// <summary>
        /// The descriptor for the preview file of the asset.
        /// </summary>
        public string PreviewFilePath { get; internal set; }

        /// <summary>
        /// The metadata of the asset.
        /// </summary>
        public IReadOnlyDictionary<string, MetadataValue> Metadata { get; internal set; }

        /// <summary>
        /// The descriptor of the child dataset update history entry that was changed, if any.
        /// </summary>
        public DatasetUpdateHistoryDescriptor? ChildDatasetUpdateHistoryDescriptor { get; internal set; }

        /// <summary>
        /// The descriptor of the child file update history entry that was changed, if any.
        /// </summary>
        public FileUpdateHistoryDescriptor? ChildFileUpdateHistoryDescriptor { get; internal set; }
    }
}
