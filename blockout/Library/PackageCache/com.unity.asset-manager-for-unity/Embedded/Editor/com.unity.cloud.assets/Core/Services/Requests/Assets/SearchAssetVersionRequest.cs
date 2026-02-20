using System.Net.Http;
using System.Text;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// Represents a change asset's status request.
    /// </summary>
    class SearchAssetVersionRequest : AssetRequest
    {
        readonly SearchRequestParameters m_Parameters;

        /// <summary>
        /// Creates an instance of a <see cref="SearchAssetVersionRequest"/> for an asset in a project.
        /// Get a single asset by id and version.
        /// </summary>
        /// <param name="projectId">ID of the project</param>
        /// <param name="assetId">ID of the asset</param>
        /// <param name="parameters">The search asset request criteria.</param>
        public SearchAssetVersionRequest(ProjectId projectId, AssetId assetId, SearchRequestParameters parameters = default)
            : base(projectId, assetId)
        {
            m_RequestUrl += "/versions/search";

            m_Parameters = parameters;
        }

        /// <summary>
        /// Creates an instance of a <see cref="SearchAssetVersionRequest"/> for an asset in a library.
        /// Get a single asset by id and version.
        /// </summary>
        /// <param name="assetLibraryId">ID of the project</param>
        /// <param name="assetId">ID of the asset</param>
        /// <param name="parameters">The search asset request criteria.</param>
        public SearchAssetVersionRequest(AssetLibraryId assetLibraryId, AssetId assetId, SearchRequestParameters parameters = default)
            : base(assetLibraryId, assetId)
        {
            m_RequestUrl += "/versions/search";

            m_Parameters = parameters;
        }

        /// <inheritdoc />
        public override HttpContent ConstructBody()
        {
            var body = IsolatedSerialization.SerializeWithDefaultConverters(m_Parameters);
            return new StringContent(body, Encoding.UTF8, "application/json");
        }
    }
}
