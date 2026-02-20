namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// Base class for api requests on assets.
    /// </summary>
    class LibraryJobRequest : ApiRequest
    {
        /// <summary>
        /// Creates an instance of a <see cref="LibraryJobRequest"/>.
        /// </summary>
        public LibraryJobRequest()
        {
            m_RequestUrl = "/libraries/jobs/duplication";
        }

        /// <summary>
        /// Creates an instance of a <see cref="LibraryJobRequest"/>.
        /// </summary>
        /// <param name="assetLibraryJobId">ID of the job.</param>
        public LibraryJobRequest(AssetLibraryJobId assetLibraryJobId)
            : this()
        {
            m_RequestUrl += $"/{assetLibraryJobId}";
        }
    }
}
