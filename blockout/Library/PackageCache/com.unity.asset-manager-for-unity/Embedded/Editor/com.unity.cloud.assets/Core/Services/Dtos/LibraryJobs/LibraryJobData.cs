using System;
using System.Runtime.Serialization;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// The data of a library job.
    /// </summary>
    [DataContract]
    class LibraryJobData : ILibraryJobData
    {
        /// <inheritdoc />
        public AssetLibraryJobId Id { get; set; }

        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public string Type { get; set; }

        /// <inheritdoc />
        public LibraryJobProgressData Progress { get; set; }

        /// <inheritdoc />
        public string State { get; set; }

        /// <inheritdoc />
        public LibraryJobResultData Results { get; set; }

        /// <inheritdoc />
        public string FailedReason { get; set; }

        public LibraryJobData() { }

        internal LibraryJobData(AssetLibraryJobId id)
        {
            Id = id;
        }
    }
}
