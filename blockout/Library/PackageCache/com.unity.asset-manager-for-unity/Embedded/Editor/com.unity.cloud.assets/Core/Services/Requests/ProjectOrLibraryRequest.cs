using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// Base class for api requests on assets.
    /// </summary>
    class ProjectOrLibraryRequest : ApiRequest
    {
        /// <summary>
        /// Creates an instance of a <see cref="ProjectOrLibraryRequest"/>.
        /// </summary>
        /// <param name="projectId">ID of the project</param>
        protected ProjectOrLibraryRequest(ProjectId projectId)
        {
            m_RequestUrl = $"/projects/{projectId}";
        }

        /// <summary>
        /// Creates an instance of a <see cref="ProjectOrLibraryRequest"/>.
        /// </summary>
        /// <param name="assetLibraryId">ID of the project</param>
        protected ProjectOrLibraryRequest(AssetLibraryId assetLibraryId)
        {
            m_RequestUrl = $"/libraries/{assetLibraryId}";
        }

        /// <summary>
        /// Creates an instance of a <see cref="ProjectOrLibraryRequest"/> for a project.
        /// </summary>
        /// <param name="projectId">ID of the project</param>
        public static ProjectOrLibraryRequest GetProjectRequest(ProjectId projectId)
        {
            var request = new ProjectOrLibraryRequest(projectId);
            request.AddParamToQuery("IncludeFields", "hasCollection");
            return request;
        }

        /// <summary>
        /// Creates an instance of a <see cref="ProjectOrLibraryRequest"/> for a library.
        /// </summary>
        /// <param name="assetLibraryId">ID of the library.</param>
        public static ProjectOrLibraryRequest GetLibraryRequest(AssetLibraryId assetLibraryId)
        {
            var request = new ProjectOrLibraryRequest(assetLibraryId);
            request.AddParamToQuery("IncludeFields", "hasCollection");
            return request;
        }

        /// <summary>
        /// Creates an instance of a <see cref="ProjectOrLibraryRequest"/> for a project.
        /// </summary>
        /// <param name="projectId">ID of the project</param>
        public static ProjectOrLibraryRequest GetEnableProjectRequest(ProjectId projectId)
        {
            var request = new ProjectOrLibraryRequest(projectId);
            request.m_RequestUrl += "/enable";
            return request;
        }

        /// <summary>
        /// Creates an instance of a <see cref="ProjectOrLibraryRequest"/> for a project.
        /// </summary>
        /// <param name="projectId">ID of the project</param>
        public static ProjectOrLibraryRequest GetAssetCountRequest(ProjectId projectId)
        {
            var projectRequest = new ProjectOrLibraryRequest(projectId);
            projectRequest.m_RequestUrl += "/assets/count";
            return projectRequest;
        }
        
        /// <summary>
        /// Creates an instance of a <see cref="ProjectOrLibraryRequest"/> for a library.
        /// </summary>
        /// <param name="assetLibraryId"></param>
        /// <returns></returns>
        public static ProjectOrLibraryRequest GetAssetCountRequest(AssetLibraryId assetLibraryId)
        {
            var projectRequest = new ProjectOrLibraryRequest(assetLibraryId);
            projectRequest.m_RequestUrl += "/assets/count";
            return projectRequest;
        }

        /// <summary>
        /// Creates an instance of a <see cref="ProjectOrLibraryRequest"/> for a project.
        /// </summary>
        /// <param name="projectId">ID of the project</param>
        public static ProjectOrLibraryRequest GetCollectionCountRequest(ProjectId projectId)
        {
            var projectRequest = new ProjectOrLibraryRequest(projectId);
            projectRequest.m_RequestUrl += "/collections-count";
            return projectRequest;
        }
    }
}
