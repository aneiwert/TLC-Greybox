using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// Represents a link an asset to project request.
    /// </summary>
    [DataContract]
    class LinkAssetToProjectRequest : ProjectOrLibraryRequest
    {
        [DataMember(Name = "assetIds")]
        AssetId[] m_AssetIds;

        /// <summary>
        /// Link an Asset to a Project Request Object.
        /// </summary>
        /// <param name="projectId">ID of the project.</param>
        /// <param name="destinationProjectId">ID of the destination project.</param>
        /// <param name="assetId">ID of the asset.</param>
        public LinkAssetToProjectRequest(ProjectId projectId, ProjectId destinationProjectId, AssetId assetId)
            : base(projectId)
        {
            m_RequestUrl += $"/assets/{assetId}/link/projects/{destinationProjectId}";
        }

        /// <summary>
        /// Link an Asset to a Project Request Object.
        /// </summary>
        /// <param name="projectId">ID of the project.</param>
        /// <param name="destinationProjectId">ID of the destination project.</param>
        /// <param name="assetIds">IDs of the assets.</param>
        public LinkAssetToProjectRequest(ProjectId projectId, ProjectId destinationProjectId, IEnumerable<AssetId> assetIds)
            : base(projectId)
        {
            m_RequestUrl += $"/assets/link/projects/{destinationProjectId}";

            m_AssetIds = assetIds.ToArray();
        }

        public override HttpContent ConstructBody()
        {
            if (m_AssetIds == null)
            {
                return base.ConstructBody();
            }

            var body = IsolatedSerialization.SerializeWithDefaultConverters(this);
            return new StringContent(body, Encoding.UTF8, "application/json");
        }
    }
}
