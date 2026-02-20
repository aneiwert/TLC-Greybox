using System.Collections.Generic;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    class GetFieldDefinitionListRequest : ApiRequest
    {
        /// <summary>
        /// Creates an instance of the <see cref="GetFieldDefinitionListRequest"/> for an organization.
        /// </summary>
        /// <param name="organizationId">ID of the organization.</param>
        /// <param name="limit">Pagination limit.</param>
        /// <param name="sortingOrder">Order of the results.</param>
        /// <param name="nextToken">Pagination token. </param>
        /// <param name="queryParameters"></param>
        public GetFieldDefinitionListRequest(OrganizationId organizationId, int limit, SortingOrder sortingOrder, string nextToken, Dictionary<string, string[]> queryParameters = null)
        {
            m_RequestUrl += $"/organizations/{organizationId}/templates/fields";

            if (queryParameters != null)
            {
                foreach (var param in queryParameters)
                {
                    AddParamToQuery(param.Key, param.Value);
                }
            }

            AddParamToQuery("SortingOrder", sortingOrder.ToString());
            AddParamToQuery("Limit", limit.ToString());
            AddParamToQuery("Next", nextToken);
        }
        
        /// <summary>
        /// Creates an instance of the <see cref="GetFieldDefinitionListRequest"/> for a library.
        /// </summary>
        /// <param name="assetLibraryId">ID of the library.</param>
        /// <param name="limit">Pagination limit.</param>
        /// <param name="sortingOrder">Order of the results.</param>
        /// <param name="nextToken">Pagination token. </param>
        /// <param name="queryParameters"></param>
        public GetFieldDefinitionListRequest(AssetLibraryId assetLibraryId, int limit, SortingOrder sortingOrder, string nextToken, Dictionary<string, string[]> queryParameters = null)
        {
            m_RequestUrl += $"/libraries/{assetLibraryId}/templates/fields";

            if (queryParameters != null)
            {
                foreach (var param in queryParameters)
                {
                    AddParamToQuery(param.Key, param.Value);
                }
            }

            AddParamToQuery("SortingOrder", sortingOrder.ToString());
            AddParamToQuery("Limit", limit.ToString());
            AddParamToQuery("Next", nextToken);
        }
    }
}
