using System;
using System.Net.Http;
using System.Text;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// Represents a change asset's status request.
    /// </summary>
    class AssetRequest : ProjectOrLibraryRequest
    {
        readonly IAssetBaseData m_Data;

        /// <summary>
        /// Creates an instance of a <see cref="AssetRequest"/> for an asset in a project.
        /// </summary>
        /// <param name="projectId">ID of the project.</param>
        /// <param name="assetId">ID of the asset.</param>
        protected AssetRequest(ProjectId projectId, AssetId assetId)
            : base(projectId)
        {
            m_RequestUrl += $"/assets/{assetId}";
        }

        /// <summary>
        /// Creates an instance of a <see cref="AssetRequest"/> for an asset in a library.
        /// </summary>
        /// <param name="assetLibraryId">ID of the library.</param>
        /// <param name="assetId">ID of the asset.</param>
        protected AssetRequest(AssetLibraryId assetLibraryId, AssetId assetId)
            : base(assetLibraryId)
        {
            m_RequestUrl += $"/assets/{assetId}";
        }

        /// <summary>
        /// Creates an instance of a <see cref="AssetRequest"/> for an asset version in project.
        /// </summary>
        /// <param name="projectId">ID of the project.</param>
        /// <param name="assetId">ID of the asset the file is linked to.</param>
        /// <param name="assetVersion">The version of the asset the file is linked to.</param>
        public AssetRequest(ProjectId projectId, AssetId assetId, AssetVersion assetVersion)
            : this(projectId, assetId)
        {
            m_RequestUrl += $"/versions/{assetVersion}";
        }

        /// <summary>
        /// Creates an instance of a <see cref="AssetRequest"/> for an asset version in project.
        /// </summary>
        /// <param name="projectId">ID of the project.</param>
        /// <param name="assetId">ID of the asset the file is linked to.</param>
        /// <param name="assetVersion">The version of the asset the file is linked to.</param>
        /// <param name="data">The data of the asset.</param>
        public AssetRequest(ProjectId projectId, AssetId assetId, AssetVersion assetVersion, IAssetBaseData data)
            : this(projectId, assetId, assetVersion)
        {
            m_Data = data;
            AddParamToQuery("updateEvenIfFrozen", true.ToString().ToLowerInvariant());
        }

        /// <summary>
        /// Creates an instance of a <see cref="AssetRequest"/> for an asset version in a library.
        /// </summary>
        /// <param name="assetLibraryId">ID of the library</param>
        /// <param name="assetId">ID of the asset</param>
        /// <param name="assetVersion">Version of the asset</param>
        protected AssetRequest(AssetLibraryId assetLibraryId, AssetId assetId, AssetVersion assetVersion)
            : this(assetLibraryId, assetId)
        {
            m_RequestUrl += $"/versions/{assetVersion}";
        }

        /// <summary>
        /// Creates an instance of a <see cref="AssetRequest"/> for an asset version in a project.
        /// </summary>
        /// <param name="projectId">ID of the project</param>
        /// <param name="assetId">ID of the asset</param>
        /// <param name="assetVersion">Version of the asset</param>
        /// <param name="includedFieldsFilter">Sets the fields to be included in the response.</param>
        public AssetRequest(ProjectId projectId, AssetId assetId, AssetVersion assetVersion, FieldsFilter includedFieldsFilter)
            : this(projectId, assetId, assetVersion)
        {
            includedFieldsFilter?.Parse(AddFieldFilterToQueryParams);
        }

        /// <summary>
        /// Creates an instance of a <see cref="AssetRequest"/> for a labelled asset in a project.
        /// </summary>
        /// <param name="projectId">ID of the project</param>
        /// <param name="assetId">ID of the asset</param>
        /// <param name="label">The labelled version of the asset</param>
        /// <param name="includedFieldsFilter">Sets the fields to be included in the response.</param>
        public AssetRequest(ProjectId projectId, AssetId assetId, string label, FieldsFilter includedFieldsFilter)
            : this(projectId, assetId)
        {
            m_RequestUrl += $"/labels/{Uri.EscapeDataString(label)}";
            includedFieldsFilter?.Parse(AddFieldFilterToQueryParams);
        }

        /// <summary>
        /// Creates an instance of a <see cref="AssetRequest"/> for an asset version in a library.
        /// </summary>
        /// <param name="assetLibraryId">ID of the library</param>
        /// <param name="assetId">ID of the asset</param>
        /// <param name="assetVersion">Version of the asset</param>
        /// <param name="includedFieldsFilter">Sets the fields to be included in the response.</param>
        public AssetRequest(AssetLibraryId assetLibraryId, AssetId assetId, AssetVersion assetVersion, FieldsFilter includedFieldsFilter)
            : this(assetLibraryId, assetId, assetVersion)
        {
            includedFieldsFilter?.Parse(AddFieldFilterToQueryParams);
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

        /// <summary>
        /// Creates a request which checks whether an asset belongs to a project.
        /// </summary>
        /// <param name="projectId">ID of the project.</param>
        /// <param name="assetId">ID of the asset.</param>
        public static AssetRequest CheckAssetBelongsToProjectRequest(ProjectId projectId, AssetId assetId)
        {
            var request = new AssetRequest(projectId, assetId);
            request.m_RequestUrl += "/check";
            return request;
        }

        /// <summary>
        /// Creates a request which Checks whether a project is an asset's source project.
        /// </summary>
        /// <param name="projectId">ID of the project.</param>
        /// <param name="assetId">ID of the asset.</param>
        public static AssetRequest CheckProjectIsAssetSourceProjectRequest(ProjectId projectId, AssetId assetId)
        {
            var request = new AssetRequest(projectId, assetId);
            request.m_RequestUrl += "/is-source-project";
            return request;
        }

        /// <summary>
        /// Creates a request which gets an asset's collections.
        /// </summary>
        /// <param name="projectId">ID of the project.</param>
        /// <param name="assetId">ID of the asset.</param>
        public static AssetRequest GetAssetCollectionsRequest(ProjectId projectId, AssetId assetId)
        {
            var request = new AssetRequest(projectId, assetId);
            request.m_RequestUrl += "/collections";
            return request;
        }

        protected void AddFieldFilterToQueryParams(string value)
        {
            AddParamToQuery("IncludeFields", value);
        }
    }
}
