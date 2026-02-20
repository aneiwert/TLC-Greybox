using System;
using System.Runtime.Serialization;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// The interface for the data of a library job.
    /// </summary>
    interface ILibraryJobData
    {
        /// <summary>
        /// The id of the job for copying.
        /// </summary>
        [DataMember(Name = "id")]
        AssetLibraryJobId Id { get; }

        /// <summary>
        /// The name of the job for copying.
        /// </summary>
        [DataMember(Name = "name")]
        string Name { get; }

        /// <summary>
        /// The type of the job for copying.
        /// </summary>
        [DataMember(Name = "type")]
        string Type { get; }

        /// <summary>
        /// Represents the progress of copying a library asset to a project.
        /// </summary>
        [DataMember(Name = "progress")]
        LibraryJobProgressData Progress { get; }

        /// <summary>
        /// The state of the job for copying.
        /// </summary>
        [DataMember(Name = "state")]
        string State { get; }

        /// <summary>
        /// Represents the result of copying a library asset to a project.
        /// </summary>
        [DataMember(Name = "results")]
        LibraryJobResultData Results { get; }

        /// <summary>
        /// The reason for the failure of the job for copying.
        /// </summary>
        [DataMember(Name = "failedReason")]
        string FailedReason { get; }
    }

    [DataContract]
    struct LibraryJobProgressData
    {
        /// <summary>
        /// The progress percentage.
        /// </summary>
        [DataMember(Name = "value")]
        public double Value;

        /// <summary>
        /// Description of the job's current step
        /// </summary>
        [DataMember(Name = "message")]
        public string Message;
    }

    [DataContract]
    struct LibraryJobResultData
    {
        public bool Exists => m_Asset is {Exists: true};
        public ProjectDescriptor ProjectDescriptor => ParseProjectDescriptor(m_Asset.SourceProjectUri);
        public AssetId AssetId => new(m_Asset.AssetId);
        public AssetVersion AssetVersion => new(m_Asset.AssetVersion);

        [DataMember(Name = "asset")]
        LibraryJobResultAssetData m_Asset;
        
        // For test purposes only, to allow creating a LibraryJobResultData with a specific asset.
        internal LibraryJobResultData(string assetId, string assetVersion, string sourceProjectUri)
        {
            m_Asset = new LibraryJobResultAssetData
            {
                AssetId = assetId,
                AssetVersion = assetVersion,
                SourceProjectUri = sourceProjectUri
            };
        }

        /// <remarks>
        /// Expected uri format: "udam:project://3573749814482/524f026e-817d-4b18-8cda-db635a6bdbfb"
        /// </remarks>
        static ProjectDescriptor ParseProjectDescriptor(string uri)
        {
            var split = uri?.Split(new[] {"://"}, StringSplitOptions.RemoveEmptyEntries);
            if (split is {Length: 2})
            {
                var parts = split[1].Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    return new ProjectDescriptor(new OrganizationId(parts[0]), new ProjectId(parts[1]));
                }
            }

            return default;
        }

        [DataContract]
        class LibraryJobResultAssetData
        {
            public bool Exists => !string.IsNullOrEmpty(SourceProjectUri) && !string.IsNullOrEmpty(AssetId) && !string.IsNullOrEmpty(AssetVersion);

            [DataMember(Name = "assetId")]
            public string AssetId;
            [DataMember(Name = "assetVersion")]
            public string AssetVersion;
            [DataMember(Name = "SourceProjectUri")]
            public string SourceProjectUri;
        }
    }
}
