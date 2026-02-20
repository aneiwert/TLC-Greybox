using System;
using System.Net.Http;
using System.Text;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    class DatasetRequest : AssetRequest
    {
        readonly IDatasetBaseData m_Data;

        /// <summary>
        /// Creates an instance of a <see cref="DatasetRequest"/> for a dataset in a project.
        /// </summary>
        /// <param name="projectId">ID of the project</param>
        /// <param name="assetId">ID of the asset</param>
        /// <param name="assetVersion">ID of the asset version</param>
        /// <param name="datasetId">ID of the dataset</param>
        protected DatasetRequest(ProjectId projectId, AssetId assetId, AssetVersion assetVersion, DatasetId datasetId)
            : base(projectId, assetId, assetVersion)
        {
            m_RequestUrl += $"/datasets/{datasetId}";
        }

        /// <summary>
        /// Creates an instance of a <see cref="DatasetRequest"/> for a dataset in a project.
        /// </summary>
        /// <param name="projectId">ID of the project</param>
        /// <param name="assetId">ID of the asset</param>
        /// <param name="assetVersion">ID of the asset version</param>
        /// <param name="datasetId">ID of the dataset</param>
        /// <param name="data">The object containing the data of the dataset</param>
        public DatasetRequest(ProjectId projectId, AssetId assetId, AssetVersion assetVersion, DatasetId datasetId, IDatasetBaseData data)
            : this(projectId, assetId, assetVersion, datasetId)
        {
            m_Data = data;
            AddParamToQuery("updateEvenIfFrozen", true.ToString().ToLowerInvariant());
        }

        /// <summary>
        /// Creates an instance of a <see cref="DatasetRequest"/> for a dataset in a library.
        /// </summary>
        /// <param name="assetLibraryId">ID of the library</param>
        /// <param name="assetId">ID of the asset</param>
        /// <param name="assetVersion">ID of the asset version</param>
        /// <param name="datasetId">ID of the dataset</param>
        protected DatasetRequest(AssetLibraryId assetLibraryId, AssetId assetId, AssetVersion assetVersion, DatasetId datasetId)
            : base(assetLibraryId, assetId, assetVersion)
        {
            m_RequestUrl += $"/datasets/{datasetId}";
        }

        /// <summary>
        /// Creates an instance of a <see cref="DatasetRequest"/> for a dataset in a project.
        /// </summary>
        /// <param name="projectId">ID of the project</param>
        /// <param name="assetId">ID of the asset</param>
        /// <param name="assetVersion">Version of the asset</param>
        /// <param name="datasetId">ID of the dataset</param>
        /// <param name="includedFieldsFilter">Sets the fields to be included in the response.</param>
        public DatasetRequest(ProjectId projectId, AssetId assetId, AssetVersion assetVersion, DatasetId datasetId, FieldsFilter includedFieldsFilter)
            : this(projectId, assetId, assetVersion, datasetId)
        {
            includedFieldsFilter ??= FieldsFilter.None;
            includedFieldsFilter.DatasetFields.Parse(AddFieldFilterToQueryParams);
            includedFieldsFilter.FileFields.Parse(AddFieldFilterToQueryParams, "files.");
        }

        /// <summary>
        /// Creates an instance of a <see cref="DatasetRequest"/> for an asset version in a project.
        /// </summary>
        /// <param name="projectId">ID of the project</param>
        /// <param name="assetId">ID of the asset</param>
        /// <param name="assetVersion">Version of the asset</param>
        /// <param name="includedFieldsFilter">Sets the fields to be included in the response.</param>
        /// <param name="token">Pagination token.</param>
        /// <param name="limit">Pagination limit.</param>
        public DatasetRequest(ProjectId projectId, AssetId assetId, AssetVersion assetVersion, FieldsFilter includedFieldsFilter, string token = null, int? limit = null)
            : base(projectId, assetId, assetVersion)
        {
            m_RequestUrl += "/datasets";

            includedFieldsFilter ??= FieldsFilter.None;
            includedFieldsFilter.DatasetFields.Parse(AddFieldFilterToQueryParams);
            includedFieldsFilter.FileFields.Parse(AddFieldFilterToQueryParams, "files.");

            AddParamToQuery("Limit", limit?.ToString());
            AddParamToQuery("Token", token);
        }

        /// <inheritdoc />
        public override HttpContent ConstructBody()
        {
            if (m_Data == null)
            {
                return base.ConstructBody();
            }

            var body = IsolatedSerialization.Serialize(m_Data, IsolatedSerialization.defaultSettings);
            return new StringContent(body, Encoding.UTF8, "application/json");
        }
    }
}
