using System.Runtime.Serialization;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// This interface contains all the information about a cloud library.
    /// </summary>
    interface ILibraryData
    {
        /// <summary>
        /// The library ID.
        /// </summary>
        [DataMember(Name = "id")]
        AssetLibraryId Id { get; }

        /// <summary>
        /// The library name.
        /// </summary>
        [DataMember(Name = "name")]
        string Name { get; }

        /// <summary>
        /// Whether the library has at least one collection.
        /// </summary>
        [DataMember(Name = "hasCollection")]
        bool HasCollection { get; }
    }
}
