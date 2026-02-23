using System;
using System.Net.Http;
using System.Text;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// Builds an API request that references a collection by path.
    /// </summary>
    class CollectionRequest : ProjectOrLibraryRequest
    {
        readonly IAssetCollectionData m_Data;

        /// <summary>
        /// Creates an instance of a <see cref="CollectionRequest"/> for a collection in a project.
        /// </summary>
        /// <param name="projectId">ID of the project. </param>
        /// <param name="collectionPath">The path to the collection. </param>
        /// <param name="data">The object containing the data of the collection. </param>
        public CollectionRequest(ProjectId projectId, CollectionPath collectionPath, IAssetCollectionData data = null)
            : base(projectId)
        {
            m_RequestUrl += $"/collections/{Uri.EscapeDataString(collectionPath)}";

            m_Data = data;
        }

        /// <summary>
        /// Creates an instance of a <see cref="CollectionRequest"/> for a collection in a library.
        /// </summary>
        /// <param name="assetLibraryId">ID of the library. </param>
        /// <param name="collectionPath">The path to the collection. </param>
        /// <param name="data">The object containing the data of the collection. </param>
        CollectionRequest(AssetLibraryId assetLibraryId, CollectionPath collectionPath, IAssetCollectionData data = null)
            : base(assetLibraryId)
        {
            m_RequestUrl += $"/collections/{Uri.EscapeDataString(collectionPath)}";

            m_Data = data;
        }

        static CollectionRequest GetCollectionCountRequest(CollectionRequest request, bool includeSubCollections = false)
        {
            request.m_RequestUrl += "/count";
            request.AddParamToQuery("includeSubCollections", includeSubCollections.ToString().ToLowerInvariant());
            return request;
        }

        static CollectionRequest GetAssetCountRequest(CollectionRequest request, bool includeSubCollections = false)
        {
            request.m_RequestUrl += "/assets/count";
            request.AddParamToQuery("includeSubCollections", includeSubCollections.ToString().ToLowerInvariant());
            return request;
        }

        /// <inheritdoc />
        public override HttpContent ConstructBody()
        {
            if (m_Data == null)
            {
                return base.ConstructBody();
            }

            var body = IsolatedSerialization.SerializeWithConverters(m_Data, IsolatedSerialization.CollectionPathConverter);
            return new StringContent(body, Encoding.UTF8, "application/json");
        }

        /// <summary>
        /// Creates an instance of a <see cref="CollectionRequest"/> for a collection in a project.
        /// </summary>
        /// <param name="projectId">ID of the project. </param>
        /// <param name="collectionPath">The path to the collection. </param>
        /// <param name="includeSubCollections">Whether to include subcollections in the request. </param>
        public static CollectionRequest GetAssetCountRequest(ProjectId projectId, CollectionPath collectionPath, bool includeSubCollections = false)
        {
            return GetAssetCountRequest(new CollectionRequest(projectId, collectionPath), includeSubCollections);
        }

        /// <summary>
        /// Creates an instance of a <see cref="CollectionRequest"/> for a collection in a library.
        /// </summary>
        /// <param name="assetLibraryId">ID of the library. </param>
        /// <param name="collectionPath">The path to the collection. </param>
        /// <param name="includeSubCollections">Whether to include subcollections in the request. </param>
        public static CollectionRequest GetAssetCountRequest(AssetLibraryId assetLibraryId, CollectionPath collectionPath, bool includeSubCollections = false)
        {
            return GetCollectionCountRequest(new CollectionRequest(assetLibraryId, collectionPath), includeSubCollections);
        }

        /// <summary>
        /// Creates an instance of a <see cref="CollectionRequest"/> for a collection in a project.
        /// </summary>
        /// <param name="projectId">ID of the project. </param>
        /// <param name="collectionPath">The path to the collection. </param>
        /// <param name="includeSubCollections">Whether to include subcollections in the request. </param>
        public static CollectionRequest GetCollectionCountRequest(ProjectId projectId, CollectionPath collectionPath, bool includeSubCollections = false)
        {
            return GetCollectionCountRequest(new CollectionRequest(projectId, collectionPath), includeSubCollections);
        }
    }
}
