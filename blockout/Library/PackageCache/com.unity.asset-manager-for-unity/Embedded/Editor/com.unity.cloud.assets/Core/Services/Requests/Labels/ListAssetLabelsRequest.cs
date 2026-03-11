using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    class ListAssetLabelsRequest : AssetRequest
    {
        /// <summary>
        /// Creates an instance of a <see cref="ListAssetLabelsRequest"/> for an asset in a project.
        /// </summary>
        /// <param name="projectId">ID of the project.</param>
        /// <param name="assetId">ID of the asset.</param>
        /// <param name="offset">The amount of entities to skip.</param>
        /// <param name="limit">The page size.</param>
        public ListAssetLabelsRequest(ProjectId projectId, AssetId assetId, int offset, int limit)
            : base(projectId, assetId)
        {
            m_RequestUrl += "/labels";

            AddParamToQuery("Offset", offset.ToString());
            AddParamToQuery("Limit", limit.ToString());
        }
        
        /// <summary>
        /// Creates an instance of a <see cref="ListAssetLabelsRequest"/> for an asset in a library.
        /// </summary>
        /// <param name="assetLibraryId">ID of the library.</param>
        /// <param name="assetId">ID of the asset.</param>
        /// <param name="offset">The amount of entities to skip.</param>
        /// <param name="limit">The page size.</param>
        public ListAssetLabelsRequest(AssetLibraryId assetLibraryId, AssetId assetId, int offset, int limit)
            : base(assetLibraryId, assetId)
        {
            m_RequestUrl += "/labels";

            AddParamToQuery("Offset", offset.ToString());
            AddParamToQuery("Limit", limit.ToString());
        }
    }
}
