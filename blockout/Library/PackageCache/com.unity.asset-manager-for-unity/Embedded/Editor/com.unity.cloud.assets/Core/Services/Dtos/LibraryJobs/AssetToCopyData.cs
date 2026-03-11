using System;
using System.Runtime.Serialization;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// The data required to create a library job for copying assets from a library to a project.
    /// </summary>
    [DataContract]
    struct AssetToCopyData
    {
        /// <summary>
        /// The ID of the asset to duplicate.
        /// </summary>
        [DataMember(Name = "assetId")]
        public AssetId AssetId;

        /// <summary>
        /// The version of the asset to duplicate.
        /// </summary>
        [DataMember(Name = "assetVersion")]
        public AssetVersion AssetVersion;

        /// <summary>
        /// The path to the collection to duplicate the asset to.
        /// </summary>
        [DataMember(Name = "collectionPath")]
        public string CollectionPath;

        /// <summary>
        /// The ID of the status flow to apply to the duplicated asset.
        /// </summary>
        [DataMember(Name = "statusFlowId")]
        public string StatusFlowId;
    }
}
