using System;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// The properties of an <see cref="IAssetLibrary"/>.
    /// </summary>
    [Serializable]
struct AssetLibraryProperties
    {
        string m_Name;
        bool m_HasCollection;
        
        /// <summary>
        /// The library's name.
        /// </summary>
        public string Name 
        {
            get => m_Name;
            internal set => m_Name = value;
        }

        /// <summary>
        /// Whether the library has any collections
        /// </summary>
        public bool HasCollection
        {
            get => m_HasCollection;
            internal set => m_HasCollection = value;
        }
    }
}
