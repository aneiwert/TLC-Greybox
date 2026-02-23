using System;
using System.Collections.Generic;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// A class that defines collection of asset to be copied.
    /// </summary>
    sealed class AssetsToCopy
    {
        readonly List<AssetToCopyData> m_Data = new();

        internal IEnumerable<AssetToCopyData> Data => m_Data;

        /// <summary>
        /// The number of assets to copy in this collection.
        /// </summary>
        public int Count => m_Data.Count;

        /// <summary>
        /// Adds an asset to be copied.
        /// </summary>
        /// <param name="assetId">The id of the asset to copy. </param>
        /// <param name="assetVersion">The version of the asset to copy. </param>
        /// <param name="destinationCollectionPath">The collection path the asset will be copied to.</param>
        /// <param name="destinationStatusFlowId">The status flow to apply to the asset once copied.</param>
        public void Add(AssetId assetId, AssetVersion assetVersion,
            CollectionPath? destinationCollectionPath = null,
            string destinationStatusFlowId = null)
        {
            var collectionPath = destinationCollectionPath?.ToString();
            var assetToCopy = new AssetToCopyData
            {
                AssetId = assetId,
                AssetVersion = assetVersion,
                CollectionPath = string.IsNullOrEmpty(collectionPath) ? null : collectionPath,
                StatusFlowId = string.IsNullOrEmpty(destinationStatusFlowId) ? null : destinationStatusFlowId
            };
            m_Data.Add(assetToCopy);
        }

        /// <summary>
        /// Clears the collection of assets to be copied.
        /// </summary>
        public void Clear()
        {
            m_Data.Clear();
        }
    }
}
