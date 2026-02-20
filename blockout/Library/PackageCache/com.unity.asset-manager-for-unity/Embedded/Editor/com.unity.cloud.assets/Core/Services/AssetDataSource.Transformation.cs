using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    partial class AssetDataSource
    {
        /// <inheritdoc/>
        public async Task<TransformationId> StartTransformationAsync(DatasetDescriptor datasetDescriptor, string workflowType, string[] inputFiles, Dictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = new StartTransformationRequest(workflowType, inputFiles, parameters, datasetDescriptor.ProjectId, datasetDescriptor.AssetId, datasetDescriptor.AssetVersion, datasetDescriptor.DatasetId);
            using var response = await m_ServiceHttpClient.PostAsync(GetPublicRequestUri(request), request.ConstructBody(),
                ServiceHttpClientOptions.Default(), cancellationToken);

            var jsonContent = await response.GetContentAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();

            var startedTransformationResponse = IsolatedSerialization.DeserializeWithConverters<StartedTransformationDto>(jsonContent, IsolatedSerialization.TransformationIdConverter);

            return startedTransformationResponse.TransformationId;
        }

        /// <inheritdoc/>
        public async Task<ITransformationData> GetTransformationAsync(TransformationDescriptor transformationDescriptor, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = new GetTransformationRequest(transformationDescriptor.TransformationId,
                transformationDescriptor.ProjectId, transformationDescriptor.AssetId,
                transformationDescriptor.AssetVersion, transformationDescriptor.DatasetId);

            using var response = await m_ServiceHttpClient.GetAsync(GetPublicRequestUri(request), ServiceHttpClientOptions.Default(), cancellationToken);

            var jsonContent = await response.GetContentAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();

            return IsolatedSerialization.DeserializeWithDefaultConverters<TransformationData>(jsonContent);
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<ITransformationData> GetTransformationsAsync(ProjectDescriptor projectDescriptor, Range range, TransformationSearchData searchData, CancellationToken cancellationToken)
        {
            return ListEntitiesAsync<TransformationData>(GetListRequest, range, cancellationToken, 100);

            ApiRequest GetListRequest(int offset, int pageSize)
            {
                return new SearchTransformationRequest(projectDescriptor.ProjectId, searchData, offset, pageSize);
            }
        }

        /// <inheritdoc/>
        public async Task TerminateTransformationAsync(ProjectDescriptor projectDescriptor, TransformationId transformationId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = new TerminateTransformationRequest(projectDescriptor.ProjectId, transformationId);
            using var _ = await m_ServiceHttpClient.PostAsync(GetPublicRequestUri(request), request.ConstructBody(), ServiceHttpClientOptions.Default(), cancellationToken);
        }
    }
}
