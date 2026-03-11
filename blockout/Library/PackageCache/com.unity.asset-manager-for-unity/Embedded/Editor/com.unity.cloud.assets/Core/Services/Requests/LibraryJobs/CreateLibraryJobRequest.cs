using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// Class for api library job requests.
    /// </summary>
    class CreateLibraryJobRequest : ProjectOrLibraryRequest
    {
        readonly AssetToCopyData[] m_Data;

        /// <summary>
        /// Creates an instance of a <see cref="CreateLibraryJobRequest"/>.
        /// </summary>
        /// <param name="assetLibraryId">ID of the library. </param>
        /// <param name="projectId">ID of the destination project. </param>
        /// <param name="libraryJobData">The assets to be copied. </param>
        public CreateLibraryJobRequest(AssetLibraryId assetLibraryId, ProjectId projectId, IEnumerable<AssetToCopyData> libraryJobData = null)
            : base(assetLibraryId)
        {
            m_RequestUrl += $"/duplicate/projects/{projectId}";

            m_Data = libraryJobData?.ToArray();
        }

        /// <inheritdoc />
        public override HttpContent ConstructBody()
        {
            if (m_Data == null)
            {
                return base.ConstructBody();
            }

            var body = IsolatedSerialization.SerializeWithDefaultConverters(m_Data);
            return new StringContent(body, Encoding.UTF8, "application/json");
        }
    }
}
