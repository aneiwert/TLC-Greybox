using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// Get the list of collections from a project.
    /// </summary>
    class GetCollectionListRequest : ProjectOrLibraryRequest
    {
        /// <summary>
        /// Creates an instance of a <see cref="GetCollectionListRequest"/> for a project.
        /// </summary>
        /// <param name="projectId">ID of the project</param>
        /// <param name="offset">The pagination offset.</param>
        /// <param name="limit">The limit of returning records for pagination.</param>
        public GetCollectionListRequest(ProjectId projectId, int offset, int limit)
            : base(projectId)
        {
            m_RequestUrl += "/collections";
            AddParamToQuery("offset", offset.ToString());
            AddParamToQuery("limit", limit.ToString());
        }

        /// <summary>
        /// Creates an instance of a <see cref="GetCollectionListRequest"/> for a library.
        /// </summary>
        /// <param name="assetLibraryId">ID of the library</param>
        /// <param name="offset">The pagination offset.</param>
        /// <param name="limit">The limit of returning records for pagination.</param>
        public GetCollectionListRequest(AssetLibraryId assetLibraryId, int offset, int limit)
            : base(assetLibraryId)
        {
            m_RequestUrl += "/collections";
            AddParamToQuery("offset", offset.ToString());
            AddParamToQuery("limit", limit.ToString());
        }
    }
}
