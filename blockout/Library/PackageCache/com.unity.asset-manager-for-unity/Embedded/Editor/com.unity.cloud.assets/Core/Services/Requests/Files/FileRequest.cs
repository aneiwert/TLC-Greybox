using System;
using System.Net.Http;
using System.Text;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// Represents a finalized upload asset file request.
    /// </summary>
    class FileRequest : DatasetRequest
    {
        readonly IFileBaseData m_Data;

        /// <summary>
        /// Creates an instance of a <see cref="FileRequest"/> for a file in a project.
        /// </summary>
        /// <param name="projectId">ID of the project.</param>
        /// <param name="assetId">ID the asset the file will link to.</param>
        /// <param name="assetVersion">The version of the asset the file will link to.</param>
        /// <param name="datasetId">ID the dataset. </param>
        /// <param name="filePath">The path to the file in the dataset.</param>
        public FileRequest(ProjectId projectId, AssetId assetId, AssetVersion assetVersion, DatasetId datasetId, string filePath)
            : base(projectId, assetId, assetVersion, datasetId)
        {
            m_RequestUrl += $"/files/{Uri.EscapeDataString(filePath)}";
        }

        /// <summary>
        /// Creates an instance of a <see cref="FileRequest"/> for a file in a project.
        /// </summary>
        /// <param name="projectId">ID of the project.</param>
        /// <param name="assetId">ID the asset the file will link to.</param>
        /// <param name="assetVersion">The version of the asset the file will link to.</param>
        /// <param name="datasetId">ID the dataset. </param>
        /// <param name="filePath">The path to the file in the dataset.</param>
        /// <param name="data">The object containing the data of the file.</param>
        public FileRequest(ProjectId projectId, AssetId assetId, AssetVersion assetVersion, DatasetId datasetId, string filePath, IFileBaseData data)
            : this(projectId, assetId, assetVersion, datasetId, filePath)
        {
            m_Data = data;
            AddParamToQuery("updateEvenIfFrozen", true.ToString().ToLowerInvariant());
        }

        /// <summary>
        /// Creates an instance of a <see cref="FileRequest"/> for a file in a library.
        /// </summary>
        /// <param name="assetLibraryId">ID of the library.</param>
        /// <param name="assetId">ID the asset the file will link to.</param>
        /// <param name="assetVersion">The version of the asset the file will link to.</param>
        /// <param name="datasetId">ID the dataset. </param>
        /// <param name="filePath">The path to the file in the dataset.</param>
        protected FileRequest(AssetLibraryId assetLibraryId, AssetId assetId, AssetVersion assetVersion, DatasetId datasetId, string filePath)
            : base(assetLibraryId, assetId, assetVersion, datasetId)
        {
            m_RequestUrl += $"/files/{Uri.EscapeDataString(filePath)}";
        }

        /// <summary>
        /// Creates an instance of a <see cref="FileRequest"/> for a file in a project.
        /// </summary>
        /// <param name="projectId">ID of the project.</param>
        /// <param name="assetId">ID the asset the file will link to.</param>
        /// <param name="assetVersion">The version of the asset the file will link to.</param>
        /// <param name="datasetId">ID the dataset. </param>
        /// <param name="filePath">The path to the file in the dataset.</param>
        /// <param name="includedFileFields">Sets the fields to be included in the response.</param>
        public FileRequest(ProjectId projectId, AssetId assetId, AssetVersion assetVersion, DatasetId datasetId, string filePath, FileFields includedFileFields)
            : this(projectId, assetId, assetVersion, datasetId, filePath)
        {
            includedFileFields.Parse(AddFieldFilterToQueryParams);
        }

        /// <summary>
        /// Creates an instance of a <see cref="FileRequest"/> for a dataset in a project.
        /// </summary>
        /// <param name="projectId">ID of the project.</param>
        /// <param name="assetId">ID the asset the file will link to.</param>
        /// <param name="assetVersion">The version of the asset the file will link to.</param>
        /// <param name="datasetId">ID the dataset. </param>
        /// <param name="includedFieldsFilter">Sets the fields to be included in the response.</param>
        /// <param name="token">Pagination token.</param>
        /// <param name="limit">Pagination limit.</param>
        public FileRequest(ProjectId projectId, AssetId assetId, AssetVersion assetVersion, DatasetId datasetId, FileFields includedFieldsFilter, string token = null, int? limit = null)
            : base(projectId, assetId, assetVersion, datasetId)
        {
            m_RequestUrl += "/files";

            includedFieldsFilter.Parse(AddFieldFilterToQueryParams);

            AddParamToQuery("Limit", limit?.ToString());
            AddParamToQuery("Token", token);
        }

        /// <summary>
        /// Creates an instance of a <see cref="FileRequest"/> for a dataset in a library.
        /// </summary>
        /// <param name="assetLibraryId">ID of the library.</param>
        /// <param name="assetId">ID the asset the file will link to.</param>
        /// <param name="assetVersion">The version of the asset the file will link to.</param>
        /// <param name="datasetId">ID the dataset. </param>
        /// <param name="includedFieldsFilter">Sets the fields to be included in the response.</param>
        /// <param name="token">Pagination token.</param>
        /// <param name="limit">Pagination limit.</param>
        public FileRequest(AssetLibraryId assetLibraryId, AssetId assetId, AssetVersion assetVersion, DatasetId datasetId, FileFields includedFieldsFilter, string token = null, int? limit = null)
            : base(assetLibraryId, assetId, assetVersion, datasetId)
        {
            m_RequestUrl += "/files";

            includedFieldsFilter.Parse(AddFieldFilterToQueryParams);

            AddParamToQuery("Limit", limit?.ToString());
            AddParamToQuery("Token", token);
        }

        public static FileRequest GetContentUpdateRequest(ProjectId projectId, AssetId assetId, AssetVersion assetVersion, DatasetId datasetId, string filePath, IFileBaseData data = null)
        {
            var request = new FileRequest(projectId, assetId, assetVersion, datasetId, filePath, data);
            request.m_RequestUrl += "/content";
            return request;
        }

        /// <inheritdoc />
        public override HttpContent ConstructBody()
        {
            if (m_Data == null)
            {
                return base.ConstructBody();
            }

            var body = IsolatedSerialization.SerializeWithConverters(m_Data);
            return new StringContent(body, Encoding.UTF8, "application/json");
        }
    }
}
