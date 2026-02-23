namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// Base class for api requests on assets.
    /// </summary>
    class ListLibrariesRequest : ApiRequest
    {
        /// <summary>
        /// Creates an instance of a <see cref="ListLibrariesRequest"/> for listing libraries.
        /// </summary>
        /// <param name="offset">The amount of entities to skip.</param>
        /// <param name="limit">The page size.</param>
        public ListLibrariesRequest(int offset, int limit)
        {
            m_RequestUrl = "/libraries";

            AddParamToQuery("IncludeFields", "hasCollection");
            AddParamToQuery("offset", offset.ToString());
            AddParamToQuery("limit", limit.ToString());
        }
    }
}
