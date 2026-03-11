using System.Runtime.Serialization;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// This interface contains all the information about a cloud project.
    /// </summary>
    interface IProjectData : IProjectBaseData
    {
        /// <summary>
        /// The project ID.
        /// </summary>
        [DataMember(Name = "id")]
        ProjectId Id { get; }

        /// <summary>
        /// Whether the project has at least one collection.
        /// </summary>
        [DataMember(Name="hasCollection")]
        bool HasCollection { get; }
    }
}
