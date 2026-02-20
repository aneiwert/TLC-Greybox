using System.Runtime.Serialization;

namespace Unity.Cloud.AssetsEmbedded
{
    [DataContract]
    struct CollectionDescriptorDto
    {
        [DataMember(Name = "projectDescriptor")]
        public string ProjectDescriptor { get; set; }

        [DataMember(Name = "collectionPath")]
        public string CollectionPath { get; set; }
        
        [DataMember(Name = "libraryId")]
        public string AssetLibraryId { get; set; }
    }
}
