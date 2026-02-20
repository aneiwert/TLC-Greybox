using System;
using UnityEditor;

namespace Unity.AssetManager.Editor
{
    /// <summary>
    /// Represents an asset to be uploaded to the Asset Manager.
    /// </summary>
    public class UploadAsset
    {
        /// <summary>
        /// Constructor for UploadAsset.
        /// </summary>
        /// <param name="assetGUID">The Unity asset GUID</param>
        /// <param name="dependencies">Dependencies to other Unity asset GUID</param>
        /// <param name="assetType">The asset type</param>
        internal UploadAsset(GUID assetGUID, GUID[] dependencies, AssetType assetType)
        {
            AssetGUID = assetGUID;
            Dependencies = dependencies;
            AssetType = assetType;
        }

        /// <summary>
        /// The GUID of the asset.
        /// </summary>
        public GUID AssetGUID { get; internal set; }

        /// <summary>
        /// The GUIDs of the asset's dependencies.
        /// </summary>
        public GUID[] Dependencies { get; internal set; }

        /// <summary>
        /// The name of the asset.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description of the asset.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The type of the asset.
        /// </summary>
        public AssetType AssetType { get; internal set; }

        /// <summary>
        /// The status of the asset.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The tags associated with the asset.
        /// </summary>
        public string[] Tags = Array.Empty<string>();

        /// <summary>
        /// The metadata associated with the asset.
        /// </summary>
        public MetadataContainer Metadata = new();
    }
}
