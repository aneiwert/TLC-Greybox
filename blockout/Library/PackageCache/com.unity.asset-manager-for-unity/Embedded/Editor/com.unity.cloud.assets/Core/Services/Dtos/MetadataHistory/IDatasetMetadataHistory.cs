using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Unity.Cloud.AssetsEmbedded
{
    interface IDatasetMetadataHistory : IEntityMetadataHistory
    {
        /// <inheritdoc cref="IDatasetBaseData.Name"/>
        [DataMember(Name = "name")]
        string Name { get; }

        /// <inheritdoc cref="IDatasetBaseData.Type"/>
        [DataMember(Name = "primaryType")]
        string Type { get; }

        /// <inheritdoc cref="IDatasetUpdateData.FileOrder"/>
        [DataMember(Name = "fileOrder")]
        IEnumerable<string> FileOrder { get; }

        /// <inheritdoc cref="IDatasetBaseData.IsVisible"/>
        [DataMember(Name = "isVisible")]
        bool IsVisible { get; }

        /// <inheritdoc cref="IDatasetBaseData.Description"/>
        [DataMember(Name = "description")]
        string Description { get; }

        /// <inheritdoc cref="IDatasetBaseData.Tags"/>
        [DataMember(Name = "tags")]
        IEnumerable<string> Tags { get; }
    }
}
