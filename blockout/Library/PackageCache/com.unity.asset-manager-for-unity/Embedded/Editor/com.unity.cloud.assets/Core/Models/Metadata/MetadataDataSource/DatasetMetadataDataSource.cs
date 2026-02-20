using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    class DatasetMetadataDataSource : MetadataDataSource
    {
        readonly DatasetDescriptor m_Descriptor;

        protected override AssetDescriptor AssetDescriptor => m_Descriptor.AssetDescriptor;

        internal DatasetMetadataDataSource(DatasetDescriptor datasetDescriptor, IAssetDataSource dataSource, MetadataDataSourceSpecification specification)
            : base(dataSource, specification)
        {
            m_Descriptor = datasetDescriptor;
        }

        /// <inheritdoc />
        public override Task AddOrUpdateAsync(Dictionary<string, object> properties, CancellationToken cancellationToken)
        {
            ThrowIfPathToLibrary();

            var data = new DatasetUpdateData
            {
                Metadata = properties
            };
            return m_DataSource.UpdateDatasetAsync(m_Descriptor, data, cancellationToken);
        }

        /// <inheritdoc />
        public override Task RemoveAsync(IEnumerable<string> keys, CancellationToken cancellationToken)
        {
            ThrowIfPathToLibrary();

            return m_DataSource.RemoveDatasetMetadataAsync(m_Descriptor, m_MetadataSpecification.ToString(), keys, cancellationToken);
        }

        /// <inheritdoc />
        public override void ThrowIfPathToLibrary()
        {
            if (m_Descriptor.IsPathToAssetLibrary())
            {
                throw new InvalidOperationException("Cannot modify the metadata of library datasets.");
            }
        }

        /// <inheritdoc />
        protected override FieldsFilter GetFieldsFilter()
        {
            return new FieldsFilter
            {
                DatasetFields = m_MetadataSpecification == MetadataDataSourceSpecification.metadata ? DatasetFields.metadata : DatasetFields.systemMetadata,
            };
        }

        /// <inheritdoc />
        protected override async Task<IMetadataInfo> GetMetadataInfoAsync(FieldsFilter filter, CancellationToken cancellationToken)
        {
            return await m_DataSource.GetDatasetAsync(m_Descriptor, filter, cancellationToken);
        }
    }
}
