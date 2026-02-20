using System;
using System.Collections.Generic;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// The properties of an <see cref="IFile"/>.
    /// </summary>
    struct FileProperties
    {
        /// <summary>
        /// The description of the file.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// The status of the file.
        /// </summary>
        /// <value>
        /// <c>Draft</c> The file is created, upload may be in progress. <br/>
        /// <c>Uploaded</c> All bytes have been uploaded and the file is finalized.
        /// </value>
        public string StatusName { get; internal set; }

        /// <summary>
        /// The authoring info of the file.
        /// </summary>
        public AuthoringInfo AuthoringInfo { get; internal set; }

        /// <summary>
        /// The size of the file in bytes.
        /// </summary>
        public long SizeBytes { get; internal set; }

        /// <summary>
        /// The checksum of the file.
        /// </summary>
        public string UserChecksum { get; internal set; }

        /// <summary>
        /// The tags of the file.
        /// </summary>
        public IEnumerable<string> Tags { get; internal set; }

        /// <summary>
        /// The system tags of the file.
        /// </summary>
        public IEnumerable<string> SystemTags { get; internal set; }

        /// <summary>
        /// The datasets the file is linked to.
        /// </summary>
        public IEnumerable<DatasetDescriptor> LinkedDatasets { get; internal set; }
    }
}
