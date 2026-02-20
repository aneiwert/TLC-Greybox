using System;
using System.Net.Http;
using System.Text;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// SearchRequest
    /// Search assets based on criteria.
    /// </summary>
    class SearchRequest : ProjectOrLibraryRequest
    {
        readonly SearchRequestParameters m_RequestParameters;

        /// <summary>
        /// Creates an instance of a <see cref="SearchRequest"/> for a project.
        /// Search assets based on criteria.
        /// </summary>
        /// <param name="projectId">ID of the project.</param>
        /// <param name="parameters">The search asset request criteria.</param>
        public SearchRequest(ProjectId projectId, SearchRequestParameters parameters = default)
            : base(projectId)
        {
            m_RequestUrl += "/assets/search";

            m_RequestParameters = parameters;
        }

        /// <summary>
        /// Creates an instance of a <see cref="SearchRequest"/> for a library.
        /// Search assets based on criteria.
        /// </summary>
        /// <param name="assetLibraryId">ID of the library.</param>
        /// <param name="parameters">The search asset request criteria.</param>
        public SearchRequest(AssetLibraryId assetLibraryId, SearchRequestParameters parameters = default)
            : base(assetLibraryId)
        {
            m_RequestUrl += "/assets/search";

            m_RequestParameters = parameters;
        }

        /// <inheritdoc />
        public override HttpContent ConstructBody()
        {
            var body = IsolatedSerialization.SerializeWithDefaultConverters(m_RequestParameters);
            return new StringContent(body, Encoding.UTF8, "application/json");
        }
    }
}
