using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Unity.Cloud.AssetsEmbedded
{
    interface IFileMetadataHistory : IEntityMetadataHistory
    {
        /// <inheritdoc cref="IFileBaseData.Description"/>
        [DataMember(Name = "description")]
        string Description { get; }

        /// <inheritdoc cref="IFileBaseData.Tags"/>
        [DataMember(Name = "tags")]
        IEnumerable<string> Tags { get; }
    }
}
