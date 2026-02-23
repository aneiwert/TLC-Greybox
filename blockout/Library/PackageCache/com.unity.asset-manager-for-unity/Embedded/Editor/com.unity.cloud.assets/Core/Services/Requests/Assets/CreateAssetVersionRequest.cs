using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// Represents a change asset's status request.
    /// </summary>
    [DataContract]
    class CreateAssetVersionRequest : ProjectOrLibraryRequest
    {
        [DataMember(Name = "parentAssetVersion")]
        readonly string m_ParentVersion;

        [DataMember(Name = "statusFlowId")]
        readonly string m_StatusFlowId;

        /// <summary>
        /// Changes the asset's status Request Object.
        /// </summary>
        /// <param name="projectId">ID of the project.</param>
        /// <param name="assetId">ID of the asset.</param>
        /// <param name="parentVersion">The version of the asset the file is linked to.</param>
        /// <param name="statusFlowId">ID of the flow to apply to the new version.</param>
        public CreateAssetVersionRequest(ProjectId projectId, AssetId assetId, AssetVersion? parentVersion, string statusFlowId)
            : base(projectId)
        {
            m_RequestUrl += $"/assets/{assetId}/versions";

            m_ParentVersion = parentVersion?.ToString();
            m_StatusFlowId = statusFlowId;
        }

        /// <inheritdoc />
        public override HttpContent ConstructBody()
        {
            var body = IsolatedSerialization.Serialize(this, IsolatedSerialization.defaultSettings);
            return new StringContent(body, Encoding.UTF8, "application/json");
        }
    }
}
