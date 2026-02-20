using System.Linq;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// Represents a get asset download urls request.
    /// </summary>
    class GetAssetDownloadUrlsRequest : AssetRequest
    {
        /// <summary>
        /// Creates an instance of a <see cref="GetAssetDownloadUrlsRequest"/> for an asset in a project.
        /// Get a list of url for an Asset.
        /// </summary>
        /// <param name="projectId">ID of the project.</param>
        /// <param name="assetId">ID of the asset.</param>
        /// <param name="assetVersion">The version of the asset.</param>
        /// <param name="datasetIds">An optional collection of datasets with which to limit the search.</param>
        /// <param name="maxDimension">The desired length to resize the larger image dimension to, while maintaining the same aspect ratio. </param>
        public GetAssetDownloadUrlsRequest(ProjectId projectId, AssetId assetId, AssetVersion assetVersion, DatasetId[] datasetIds, int? maxDimension)
            : base(projectId, assetId, assetVersion)
        {
            m_RequestUrl += "/download-urls";

            if (datasetIds != null)
                AddParamToQuery("datasets", datasetIds.Select(x => x.ToString()));
            if (maxDimension.HasValue)
                AddParamToQuery("maxDimension", maxDimension.Value.ToString());
        }

        /// <summary>
        /// Creates an instance of a <see cref="GetAssetDownloadUrlsRequest"/> for an asset in a library.
        /// Get a list of url for an Asset.
        /// </summary>
        /// <param name="assetLibraryId">ID of the library.</param>
        /// <param name="assetId">ID of the asset.</param>
        /// <param name="assetVersion">The version of the asset.</param>
        /// <param name="datasetIds">An optional collection of datasets with which to limit the search.</param>
        /// <param name="offset">The pagination offset.</param>
        /// <param name="limit">The limit of returning records for pagination.</param>
        /// <param name="maxDimension">The desired length to resize the larger image dimension to, while maintaining the same aspect ratio. </param>
        public GetAssetDownloadUrlsRequest(AssetLibraryId assetLibraryId, AssetId assetId, AssetVersion assetVersion, DatasetId[] datasetIds, int offset, int limit, int? maxDimension)
            : base(assetLibraryId, assetId, assetVersion)
        {
            m_RequestUrl += "/download-urls";

            AddParamToQuery("offset", offset.ToString());
            AddParamToQuery("limit", limit.ToString());
            if (datasetIds != null)
                AddParamToQuery("datasets", datasetIds.Select(x => x.ToString()));
            if (maxDimension.HasValue)
                AddParamToQuery("maxDimension", maxDimension.Value.ToString());
        }
    }
}
