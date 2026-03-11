using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    static class AssetsToCopyExtensions
    {
        /// <summary>
        /// Adds an asset to be copied.
        /// </summary>
        /// <param name="assetsToCopy">The builder to update. </param>
        /// <param name="assetDescriptor">The descriptor containing identifiers for the asset. </param>
        /// <param name="destinationCollectionDescriptor">The collection path the asset will be copied to.</param>
        /// <param name="destinationStatusFlowDescriptor">The status flow to apply to the asset once copied.</param>
        public static void Add(this AssetsToCopy assetsToCopy, AssetDescriptor assetDescriptor,
            CollectionDescriptor? destinationCollectionDescriptor = null,
            StatusFlowDescriptor? destinationStatusFlowDescriptor = null)
        {
            assetsToCopy.Add(assetDescriptor.AssetId, assetDescriptor.AssetVersion, destinationCollectionDescriptor?.Path, destinationStatusFlowDescriptor?.StatusFlowId);
        }

        /// <summary>
        /// Adds an asset to be copied.
        /// </summary>
        /// <param name="assetsToCopy">The builder to update. </param>
        /// <param name="asset">The asset to copy. </param>
        /// <param name="destinationCollectionDescriptor">The collection path the asset will be copied to.</param>
        /// <param name="destinationStatusFlowDescriptor">The status flow to apply to the asset once copied.</param>
        public static void Add(this AssetsToCopy assetsToCopy, IAsset asset,
            CollectionDescriptor? destinationCollectionDescriptor = null,
            StatusFlowDescriptor? destinationStatusFlowDescriptor = null)
        {
            assetsToCopy.Add(asset.Descriptor, destinationCollectionDescriptor, destinationStatusFlowDescriptor);
        }
    }
}
