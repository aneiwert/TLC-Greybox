using System;
using UnityEngine;

namespace Unity.AssetManager.Core.Editor
{
    [Serializable]
    class CollectionIdentifier : IEquatable<CollectionIdentifier>
    {
        [SerializeField]
        ProjectIdentifier m_ProjectIdentifier;

        [SerializeField]
        string m_CollectionPath = string.Empty;

        public ProjectIdentifier ProjectIdentifier => m_ProjectIdentifier ?? new ProjectIdentifier();
        public string CollectionPath => m_CollectionPath ?? string.Empty;

        public CollectionIdentifier() { }

        public CollectionIdentifier(ProjectIdentifier projectIdentifier, string collectionPath)
        {
            m_ProjectIdentifier = projectIdentifier ?? new ProjectIdentifier();
            m_CollectionPath = collectionPath ?? string.Empty;
        }

        public override string ToString()
        {
            return $"[Org:{ProjectIdentifier.OrganizationId}, Proj:{ProjectIdentifier.ProjectId}, Coll:{m_CollectionPath}]";
        }

        public bool Equals(CollectionIdentifier other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return ProjectIdentifier == other.ProjectIdentifier &&
                m_CollectionPath == other.m_CollectionPath;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((CollectionIdentifier)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ProjectIdentifier, m_CollectionPath);
        }

        public static bool operator ==(CollectionIdentifier left, CollectionIdentifier right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CollectionIdentifier left, CollectionIdentifier right)
        {
            return !Equals(left, right);
        }
    }
}
