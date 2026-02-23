using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// Represents a get asset file URL request.
    /// </summary>
    class GetFileDownloadUrlRequest : FileRequest
    {
        /// <summary>
        /// Creates an instance of the <see cref="GetFileDownloadUrlRequest"/> for a file in a project.
        /// </summary>
        /// <param name="projectId">ID of the project.</param>
        /// <param name="assetId">ID of the asset the file is linked to.</param>
        /// <param name="assetVersion">The version of the asset the file is linked to.</param>
        /// <param name="datasetId">ID of the dataset.</param>
        /// <param name="filePath">Path of the file.</param>
        /// <param name="maxDimension">The desired length to resize the larger image dimension to, while maintaining the same aspect ratio. </param>
        public GetFileDownloadUrlRequest(ProjectId projectId, AssetId assetId, AssetVersion assetVersion, DatasetId datasetId, string filePath, int? maxDimension)
            : base(projectId, assetId, assetVersion, datasetId, filePath)
        {
            m_RequestUrl += "/download-url";

            if (maxDimension.HasValue)
                AddParamToQuery("maxDimension", maxDimension.Value.ToString());
        }

        /// <summary>
        /// Creates an instance of the <see cref="GetFileDownloadUrlRequest"/> for a file in a library.
        /// </summary>
        /// <param name="assetLibraryId">ID of the library.</param>
        /// <param name="assetId">ID of the asset the file is linked to.</param>
        /// <param name="assetVersion">The version of the asset the file is linked to.</param>
        /// <param name="datasetId">ID of the dataset.</param>
        /// <param name="filePath">Path of the file.</param>
        /// <param name="maxDimension">The desired length to resize the larger image dimension to, while maintaining the same aspect ratio. </param>
        public GetFileDownloadUrlRequest(AssetLibraryId assetLibraryId, AssetId assetId, AssetVersion assetVersion, DatasetId datasetId, string filePath, int? maxDimension)
            : base(assetLibraryId, assetId, assetVersion, datasetId, filePath)
        {
            m_RequestUrl += "/download-url";

            if (maxDimension.HasValue)
                AddParamToQuery("maxDimension", maxDimension.Value.ToString());
        }
    }
}
