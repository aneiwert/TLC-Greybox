using System;
using System.Collections.Generic;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    class RemoveMetadataRequest : AssetRequest
    {
        RemoveMetadataRequest(ProjectId projectId, AssetId assetId, AssetVersion assetVersion)
            : base(projectId, assetId, assetVersion) { }

        RemoveMetadataRequest(ProjectId projectId, AssetId assetId, AssetVersion assetVersion, DatasetId datasetId)
            : base(projectId, assetId, assetVersion)
        {
            m_RequestUrl += $"/datasets/{datasetId}";
        }

        RemoveMetadataRequest(ProjectId projectId, AssetId assetId, AssetVersion assetVersion, DatasetId datasetId, string filePath)
            : this(projectId, assetId, assetVersion, datasetId)
        {
            m_RequestUrl += $"/files/{Uri.EscapeDataString(filePath)}";
        }

        RemoveMetadataRequest Configure(string from, IEnumerable<string> keys)
        {
            m_RequestUrl += "/fields";
            AddParamToQuery("updateEvenIfFrozen", true.ToString().ToLowerInvariant());
            AddParamToQuery(from, keys);
            return this;
        }

        public static RemoveMetadataRequest Get(ProjectId projectId, AssetId assetId, AssetVersion assetVersion, string from, IEnumerable<string> keys)
        {
            return new RemoveMetadataRequest(projectId, assetId, assetVersion).Configure(from, keys);
        }

        public static RemoveMetadataRequest Get(ProjectId projectId, AssetId assetId, AssetVersion assetVersion, DatasetId datasetId, string from, IEnumerable<string> keys)
        {
            return new RemoveMetadataRequest(projectId, assetId, assetVersion, datasetId).Configure(from, keys);
        }

        public static RemoveMetadataRequest Get(ProjectId projectId, AssetId assetId, AssetVersion assetVersion, DatasetId datasetId, string filePath, string from, IEnumerable<string> keys)
        {
            return new RemoveMetadataRequest(projectId, assetId, assetVersion, datasetId, filePath).Configure(from, keys);
        }
    }
}
