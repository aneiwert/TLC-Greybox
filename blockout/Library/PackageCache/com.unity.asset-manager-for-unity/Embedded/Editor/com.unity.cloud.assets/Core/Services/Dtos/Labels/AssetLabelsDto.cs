using System.Runtime.Serialization;

namespace Unity.Cloud.AssetsEmbedded
{
    [DataContract]
    class AssetLabelsDto
    {
        [DataMember(Name = "assetVersion")]
        public string AssetVersion { get; set; }

        [DataMember(Name = "labels")]
        public LabelData[] Labels { get; set; }

        [DataMember(Name = "archivedLabels")]
        public LabelData[] ArchivedLabels { get; set; }
    }
}
