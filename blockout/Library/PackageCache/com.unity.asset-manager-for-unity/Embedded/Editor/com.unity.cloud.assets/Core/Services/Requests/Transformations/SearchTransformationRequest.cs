using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    class SearchTransformationRequest : ProjectOrLibraryRequest
    {
        public SearchTransformationRequest(ProjectId projectId, TransformationSearchData searchData, int offset, int limit)
            : base(projectId)
        {
            m_RequestUrl += "/transformations";

            AddParamToQuery("assetId", searchData.AssetId);
            AddParamToQuery("assetVersion", searchData.AssetVersion);
            AddParamToQuery("datasetId", searchData.DatasetId);
            AddParamToQuery("userId", searchData.UserId);
            AddParamToQuery("status", searchData.Status?.ToString());
            AddParamToQuery("offset", offset.ToString());
            AddParamToQuery("limit", limit.ToString());
        }
    }
}
