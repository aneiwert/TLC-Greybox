using System;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    class MetadataHistoryRequest : AssetRequest
    {
        MetadataHistoryRequest(ProjectId projectId, AssetId assetId, AssetVersion assetVersion)
            : base(projectId, assetId, assetVersion)
        {
            m_RequestUrl += "/metadata";
        }

        MetadataHistoryRequest(ProjectId projectId, AssetId assetId, AssetVersion assetVersion, DatasetId datasetId)
            : base(projectId, assetId, assetVersion)
        {
            m_RequestUrl += $"/datasets/{datasetId}/metadata";
        }

        MetadataHistoryRequest(ProjectId projectId, AssetId assetId, AssetVersion assetVersion, DatasetId datasetId, string filePath)
            : base(projectId, assetId, assetVersion)
        {
            m_RequestUrl += $"/datasets/{datasetId}/files/{Uri.EscapeDataString(filePath)}/metadata";
        }

        MetadataHistoryRequest Configure(int limit, int offset)
        {
            m_RequestUrl += "/history";
            AddParamToQuery("limit", limit.ToString());
            AddParamToQuery("offset", offset.ToString());
            return this;
        }

        MetadataHistoryRequest Configure(int sequenceNumber)
        {
            m_RequestUrl += $"/rollback/{sequenceNumber}";
            return this;
        }

        public static MetadataHistoryRequest Get(ProjectId projectId, AssetId assetId, AssetVersion assetVersion, bool includeChildren, int limit, int offset = 0)
        {
            var request = new MetadataHistoryRequest(projectId, assetId, assetVersion).Configure(limit, offset);
            request.AddParamToQuery("getChildHistory", includeChildren.ToString());
            return request;
        }

        public static MetadataHistoryRequest Get(ProjectId projectId, AssetId assetId, AssetVersion assetVersion, DatasetId datasetId, int limit, int offset = 0)
        {
            return new MetadataHistoryRequest(projectId, assetId, assetVersion, datasetId).Configure(limit, offset);
        }

        public static MetadataHistoryRequest Get(ProjectId projectId, AssetId assetId, AssetVersion assetVersion, DatasetId datasetId, string filePath, int limit, int offset = 0)
        {
            return new MetadataHistoryRequest(projectId, assetId, assetVersion, datasetId, filePath).Configure(limit, offset);
        }

        public static MetadataHistoryRequest Rollback(ProjectId projectId, AssetId assetId, AssetVersion assetVersion, int sequenceNumber)
        {
            return new MetadataHistoryRequest(projectId, assetId, assetVersion).Configure(sequenceNumber);
        }

        public static MetadataHistoryRequest Rollback(ProjectId projectId, AssetId assetId, AssetVersion assetVersion, DatasetId datasetId, int sequenceNumber)
        {
            return new MetadataHistoryRequest(projectId, assetId, assetVersion, datasetId).Configure(sequenceNumber);
        }

        public static MetadataHistoryRequest Rollback(ProjectId projectId, AssetId assetId, AssetVersion assetVersion, DatasetId datasetId, string filePath, int sequenceNumber)
        {
            return new MetadataHistoryRequest(projectId, assetId, assetVersion, datasetId, filePath).Configure(sequenceNumber);
        }
    }
}
