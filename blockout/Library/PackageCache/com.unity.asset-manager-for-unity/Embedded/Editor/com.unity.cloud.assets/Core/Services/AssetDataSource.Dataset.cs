using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    partial class AssetDataSource
    {
        /// <inheritdoc />
        public async Task<DatasetDescriptor> CreateDatasetAsync(AssetDescriptor assetDescriptor, IDatasetBaseData datasetCreation, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = new CreateDatasetRequest(assetDescriptor.ProjectId, assetDescriptor.AssetId, assetDescriptor.AssetVersion, datasetCreation);
            using var response = await m_ServiceHttpClient.PostAsync(GetPublicRequestUri(request), request.ConstructBody(),
                ServiceHttpClientOptions.Default(), cancellationToken);

            var jsonContent = await response.GetContentAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();

            var createdDatasetResponse = IsolatedSerialization.DeserializeWithConverters<CreatedDatasetDto>(jsonContent, IsolatedSerialization.DatasetIdConverter);

            return new DatasetDescriptor(assetDescriptor, createdDatasetResponse.DatasetId);
        }

        /// <inheritdoc />
        public IAsyncEnumerable<IDatasetData> ListDatasetsAsync(AssetDescriptor assetDescriptor, Range range, FieldsFilter includedFieldsFilter, CancellationToken cancellationToken)
        {
            return assetDescriptor.IsPathToAssetLibrary()
                ? ListDatasetsAsync_FromLibrary(assetDescriptor, range, includedFieldsFilter, cancellationToken)
                : ListDatasetsAsync_FromProject(assetDescriptor, range, includedFieldsFilter, cancellationToken);
        }

        async IAsyncEnumerable<IDatasetData> ListDatasetsAsync_FromLibrary(AssetDescriptor assetDescriptor, Range range, FieldsFilter includedFieldsFilter, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var assetFields = new FieldsFilter
            {
                AssetFields = AssetFields.datasets,
                DatasetFields = includedFieldsFilter?.DatasetFields ?? DatasetFields.none,
                FileFields = includedFieldsFilter?.FileFields ?? FileFields.none
            };
            var assetData = await GetAssetAsync(assetDescriptor, assetFields, cancellationToken);

            if (assetData?.Datasets == null) yield break;

            var datasets = assetData.Datasets.ToArray();
            if (assetFields.DatasetFields.HasFlag(DatasetFields.files) && assetData.Files != null)
            {
                foreach (var file in assetData.Files)
                {
                    var dataset = datasets.FirstOrDefault(d => d.DatasetId == file.DatasetIds.FirstOrDefault());
                    if (dataset == null) continue;

                    dataset.Files ??= new List<FileData>();

                    ((List<FileData>)dataset.Files).Add(file);
                }
            }

            var (start, length) = range.GetValidatedOffsetAndLength(datasets.Length);
            for (var i = start; i < start + length; ++i)
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return datasets[i];
            }
        }

        IAsyncEnumerable<IDatasetData> ListDatasetsAsync_FromProject(AssetDescriptor assetDescriptor, Range range, FieldsFilter includedFieldsFilter, CancellationToken cancellationToken)
        {
            var countRequest = new DatasetRequest(assetDescriptor.ProjectId, assetDescriptor.AssetId, assetDescriptor.AssetVersion, FieldsFilter.None, limit: 1);
            return ListEntitiesAsync<DatasetData>(countRequest, GetListRequest, range, cancellationToken);

            ApiRequest GetListRequest(string next, int pageSize) => new DatasetRequest(assetDescriptor.ProjectId, assetDescriptor.AssetId, assetDescriptor.AssetVersion, includedFieldsFilter, next, pageSize);
        }

        /// <inheritdoc />
        public Task<IDatasetData> GetDatasetAsync(DatasetDescriptor datasetDescriptor, FieldsFilter includedFieldsFilter, CancellationToken cancellationToken)
        {
            return datasetDescriptor.IsPathToAssetLibrary()
                ? GetDatasetAsync_FromLibrary(datasetDescriptor, includedFieldsFilter, cancellationToken)
                : GetDatasetAsync_FromProject(datasetDescriptor, includedFieldsFilter, cancellationToken);
        }

        async Task<IDatasetData> GetDatasetAsync_FromLibrary(DatasetDescriptor datasetDescriptor, FieldsFilter includedFieldsFilter, CancellationToken cancellationToken)
        {
            await foreach(var dataset in ListDatasetsAsync_FromLibrary(datasetDescriptor.AssetDescriptor, Range.All, includedFieldsFilter, cancellationToken))
            {
                if (dataset.DatasetId == datasetDescriptor.DatasetId)
                {
                    return dataset;
                }
            }

            throw new NotFoundException("Dataset does not exist.");
        }

        async Task<IDatasetData> GetDatasetAsync_FromProject(DatasetDescriptor datasetDescriptor, FieldsFilter includedFieldsFilter, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = new DatasetRequest(datasetDescriptor.ProjectId, datasetDescriptor.AssetId, datasetDescriptor.AssetVersion, datasetDescriptor.DatasetId, includedFieldsFilter);
            using var response = await m_ServiceHttpClient.GetAsync(GetPublicRequestUri(request), ServiceHttpClientOptions.Default(),
                cancellationToken);

            var jsonContent = await response.GetContentAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();

            return IsolatedSerialization.DeserializeWithDefaultConverters<DatasetData>(jsonContent);
        }

        /// <inheritdoc />
        public async Task UpdateDatasetAsync(DatasetDescriptor datasetDescriptor, IDatasetUpdateData datasetUpdate, CancellationToken cancellationToken)
        {
            var request = new DatasetRequest(datasetDescriptor.ProjectId, datasetDescriptor.AssetId, datasetDescriptor.AssetVersion, datasetDescriptor.DatasetId, datasetUpdate);
            using var _ = await m_ServiceHttpClient.PatchAsync(GetPublicRequestUri(request), request.ConstructBody(),
                ServiceHttpClientOptions.Default(), cancellationToken);
        }

        /// <inheritdoc />
        public async Task ReferenceFileFromDatasetAsync(DatasetDescriptor datasetDescriptor, DatasetId sourceDatasetId, string filePath, CancellationToken cancellationToken)
        {
            var request = new AddFileReferenceRequest(datasetDescriptor.ProjectId, datasetDescriptor.AssetId, datasetDescriptor.AssetVersion, sourceDatasetId, filePath, datasetDescriptor.DatasetId);
            using var _ = await m_ServiceHttpClient.PostAsync(GetPublicRequestUri(request), request.ConstructBody(),
                ServiceHttpClientOptions.Default(), cancellationToken);
        }

        public async Task RemoveFileFromDatasetAsync(DatasetDescriptor datasetDescriptor, string filePath, CancellationToken cancellationToken)
        {
            var request = new FileRequest(datasetDescriptor.ProjectId, datasetDescriptor.AssetId, datasetDescriptor.AssetVersion, datasetDescriptor.DatasetId, filePath);
            using var _ = await m_ServiceHttpClient.DeleteAsync(GetPublicRequestUri(request), ServiceHttpClientOptions.Default(), cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> CheckDatasetIsInProjectAssetVersionAsync(DatasetDescriptor datasetDescriptor, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = new CheckDatasetBelongsToAssetRequest(datasetDescriptor.ProjectId, datasetDescriptor.AssetId, datasetDescriptor.AssetVersion, datasetDescriptor.DatasetId);
            using var response = await m_ServiceHttpClient.GetAsync(GetPublicRequestUri(request), ServiceHttpClientOptions.Default(),
                cancellationToken);

            var jsonContent = await response.GetContentAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();

            var dto = JsonSerialization.Deserialize<DatasetAssetCheckDto>(jsonContent);

            return !string.IsNullOrEmpty(dto.DatasetVersionId);
        }

        /// <inheritdoc />
        public async Task RemoveDatasetMetadataAsync(DatasetDescriptor datasetDescriptor, string metadataType, IEnumerable<string> keys, CancellationToken cancellationToken)
        {
            var request = RemoveMetadataRequest.Get(datasetDescriptor.ProjectId,
                datasetDescriptor.AssetId,
                datasetDescriptor.AssetVersion,
                datasetDescriptor.DatasetId,
                metadataType,
                keys);
            using var _ = await m_ServiceHttpClient.DeleteAsync(GetPublicRequestUri(request), request.ConstructBody(),
                ServiceHttpClientOptions.Default(), cancellationToken);
        }
    }
}
