using System;
using System.Collections.Generic;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// The update history properties of an asset version's file.
    /// </summary>
    struct FileUpdateHistory
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
        /// The description of the file.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// The tags of the file.
        /// </summary>
        public IEnumerable<string> Tags { get; internal set; }

        /// <summary>
        /// The metadata of the file.
        /// </summary>
        public IReadOnlyDictionary<string, MetadataValue> Metadata { get; internal set; }
    }
}
