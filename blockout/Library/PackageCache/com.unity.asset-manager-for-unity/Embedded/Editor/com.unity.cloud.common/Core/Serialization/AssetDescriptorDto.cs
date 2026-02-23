using System.Runtime.Serialization;

namespace Unity.Cloud.CommonEmbedded
{
    [DataContract]
    struct AssetDescriptorDto
    {
        [DataMember(Name = "projectDescriptor")]
        public ProjectDescriptorDto ProjectDescriptor { get; set; }

        [DataMember(Name = "assetId")]
        public string AssetId { get; set; }

        [DataMember(Name = "assetVersion")]
        public string AssetVersion { get; set; }
        
        [DataMember(Name = "libraryId")]
        public string AssetLibraryId { get; set; }

        public AssetDescriptorDto(AssetDescriptor assetDescriptor)
        {
            ProjectDescriptor = new ProjectDescriptorDto(assetDescriptor.ProjectDescriptor);
            AssetId = assetDescriptor.AssetId.ToString();
            AssetVersion = assetDescriptor.AssetVersion.ToString();
            AssetLibraryId = assetDescriptor.AssetLibraryId.ToString();
        }
    }
}
