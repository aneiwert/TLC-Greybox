using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Unity.Cloud.AssetsEmbedded
{
    interface IEntityMetadataHistory
    {
        [DataMember(Name = "metadataSequenceNumber")]
        int MetadataSequenceNumber { get; }

        [DataMember(Name = "createdBy")]
        string CreatedBy { get; }

        [DataMember(Name = "created")]
        DateTime Created { get; }

        [DataMember(Name = "parentSequenceNumber")]
        int? ParentSequenceNumber { get; }
        
        /// <inheritdoc cref="IMetadataInfo.Metadata"/>
        [DataMember(Name = "metadata")]
        Dictionary<string, object> Metadata { get; }
    }
}
