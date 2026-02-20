using System;
using System.Net.Http;
using System.Text;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// SearchAndAggregateRequest
    /// Aggregations of assets that match a criteria by a defined field.
    /// </summary>
    class SearchAndAggregateRequest : ProjectOrLibraryRequest
    {
        /// <summary>Accessor for searchAndAggregateRequestParameter </summary>
        SearchAndAggregateRequestParameters Parameters { get; }

        /// <summary>
        /// Creates an instance of a <see cref="SearchAndAggregateRequest"/> for aggregating assets for a project.
        /// Aggregations of assets that match a criteria by a defined field.
        /// </summary>
        /// <param name="projectId">ID of the project</param>
        /// <param name="parameters">The request containing the read filter and the field to be used in the aggregation..</param>
        public SearchAndAggregateRequest(ProjectId projectId, SearchAndAggregateRequestParameters parameters = default)
            : base(projectId)
        {
            m_RequestUrl += "/assets/aggregations/search";

            Parameters = parameters;
        }

        /// <summary>
        /// Creates an instance of a <see cref="SearchAndAggregateRequest"/> for aggregating assets for a library.
        /// Aggregations of assets that match a criteria by a defined field.
        /// </summary>
        /// <param name="assetLibraryId">ID of the library</param>
        /// <param name="parameters">The request containing the read filter and the field to be used in the aggregation..</param>
        public SearchAndAggregateRequest(AssetLibraryId assetLibraryId, SearchAndAggregateRequestParameters parameters = default)
            : base(assetLibraryId)
        {
            m_RequestUrl += "/assets/aggregations/search";

            Parameters = parameters;
        }

        /// <inheritdoc />
        public override HttpContent ConstructBody()
        {
            var body = IsolatedSerialization.SerializeWithDefaultConverters(Parameters);
            return new StringContent(body, Encoding.UTF8, "application/json");
        }
    }
}
