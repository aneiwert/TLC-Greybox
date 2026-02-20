using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.AssetManager.Core.Editor
{
    [Serializable]
    class CollectionInfo
    {
        [FormerlySerializedAs("OrganizationId")]
        [SerializeField]
        string m_OrganizationId;
        
        [FormerlySerializedAs("ProjectId")]
        [SerializeField]
        string m_ProjectId;
        
        [FormerlySerializedAs("Name")]
        [SerializeField]
        string m_Name;
        
        [FormerlySerializedAs("ParentPath")]
        [SerializeField]
        string m_ParentPath;

        public string OrganizationId => m_OrganizationId;
        public string ProjectId => m_ProjectId;
        public string Name => m_Name;
        public string ParentPath => m_ParentPath;

        internal CollectionInfo(string organizationId, string projectId, string name, string parentPath = "")
        {
            m_OrganizationId = organizationId;
            m_ProjectId = projectId;
            m_Name = name;
            m_ParentPath = parentPath;
        }
        
        public string GetFullPath()
        {
            return string.IsNullOrEmpty(ParentPath) ? Name ?? string.Empty : $"{ParentPath}/{Name ?? string.Empty}";
        }

        public string GetUniqueIdentifier()
        {
            if (string.IsNullOrEmpty(OrganizationId) || string.IsNullOrEmpty(ProjectId))
            {
                return string.Empty;
            }

            return $"{OrganizationId}/{ProjectId}/{GetFullPath()}";
        }

        public static bool AreEquivalent(CollectionInfo left, CollectionInfo right)
        {
            var leftIdentifier = left?.GetUniqueIdentifier() ?? string.Empty;
            var rightIdentifier = right?.GetUniqueIdentifier() ?? string.Empty;
            return leftIdentifier == rightIdentifier;
        }

        public static CollectionInfo CreateFromFullPath(string organizationId, string projectId, string fullPath)
        {
            if (!string.IsNullOrEmpty(fullPath))
            {
                var slashIndex = fullPath.LastIndexOf('/');
                if (slashIndex > 0)
                {
                    var parentPath = fullPath.Substring(0, slashIndex);
                    var name = fullPath.Substring(slashIndex + 1);
                    return new CollectionInfo(organizationId, projectId, name, parentPath);
                }
            }

            return new CollectionInfo(organizationId, projectId, fullPath);
        }
    }
}
