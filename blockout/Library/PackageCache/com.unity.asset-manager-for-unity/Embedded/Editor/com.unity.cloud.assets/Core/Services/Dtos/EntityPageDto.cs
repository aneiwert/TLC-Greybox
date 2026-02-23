using System.Runtime.Serialization;

namespace Unity.Cloud.AssetsEmbedded
{
    [DataContract]
    class EntityPageDto<T>
    {
        // Non-standard API field.
        [DataMember(Name = "assets")]
        internal T[] m_Assets;

        // Non-standard API field.
        [DataMember(Name = "assetVersionLabels")]
        internal T[] m_AssetVersionLabelResults { get; set; }

        // Non-standard API field.
        [DataMember(Name = "statusFlows")]
        internal T[] m_StatusFlowResults { get; set; }

        // Non-standard API field.
        [DataMember(Name = "fieldDefinitions")]
        internal T[] m_FieldDefinitions { get; set; }

        // This is the API standard for paginated results.
        [DataMember(Name = "results")]
        T[] m_Results { get; set; }

        // This is the API standard for paginated results.
        [DataMember(Name = "total")]
        public int? Total { get; set; }

        // Non-standard API field.
        [DataMember(Name = "token")]
        internal string m_Token { get; set; }

        // This is the API standard for paginated results.
        [DataMember(Name = "next")]
        string m_Next { get; set; }

        [DataMember(Name = "previous")]
        public string Previous { get; set; }

        public T[] Results
        {
            get
            {
                // Check non-standard API fields first, then fall back to the standard 'results' field.
                if (m_Assets != null) return m_Assets;
                if (m_AssetVersionLabelResults != null) return m_AssetVersionLabelResults;
                if (m_StatusFlowResults != null) return m_StatusFlowResults;
                if (m_FieldDefinitions != null) return m_FieldDefinitions;
                return m_Results;
            }
            set => m_Results = value;
        }

        public string Next
        {
            get
            {
                // Check non-standard API fields first, then fall back to the standard 'next' field.
                if (!string.IsNullOrEmpty(m_Token)) return m_Token;
                return m_Next;
            }
            set => m_Next = value;
        }
    }
}
