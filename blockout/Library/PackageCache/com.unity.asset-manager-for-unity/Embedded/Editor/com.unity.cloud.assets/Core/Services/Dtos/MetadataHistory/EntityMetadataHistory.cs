using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Unity.Cloud.AssetsEmbedded
{
    [DataContract]
    class EntityMetadataHistory : IEntityMetadataHistory
    {
        public int MetadataSequenceNumber { get; set; }

        public string CreatedBy { get; set; }

        public DateTime Created { get; set; }

        public int? ParentSequenceNumber { get; set; }

        public Dictionary<string, object> Metadata { get; set; }
    }
}
