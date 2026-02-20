using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    partial class AssetDataSource
    {
        /// <inheritdoc />
        public IAsyncEnumerable<IStatusFlowData> ListStatusFlowsAsync(OrganizationId organizationId, PaginationData paginationData, CancellationToken cancellationToken)
        {
            const int maxPageSize = 99;

            var countRequest = new StatusRequest(organizationId, 0, 1);
            return ListEntitiesAsync<StatusFlowData>(countRequest, GetListRequest, paginationData.Range, cancellationToken, maxPageSize);

            ApiRequest GetListRequest(int offset, int pageSize) => new StatusRequest(organizationId, offset, pageSize);
        }

        /// <inheritdoc />
        public async Task<(StatusFlowDescriptor, IStatusData)> GetAssetStatusAsync(AssetDescriptor assetDescriptor, CancellationToken cancellationToken)
        {
            var dto = await GetAssetStatusDtoAsync(assetDescriptor, cancellationToken);

            var statusFlowDescriptor = new StatusFlowDescriptor(assetDescriptor.OrganizationId, dto.StatusFlow.Id);
            var status = dto.StatusFlow.Statuses.FirstOrDefault(x => x.Id == dto.CurrentStatusId);

            return (statusFlowDescriptor, status);
        }

        /// <inheritdoc />
        public async Task<(StatusFlowDescriptor, IStatusData[])> GetReachableStatusesAsync(AssetDescriptor assetDescriptor, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = AssetStatusRequest.GetReachableStatusRequest(assetDescriptor.ProjectId, assetDescriptor.AssetId, assetDescriptor.AssetVersion);
            using var response = await m_ServiceHttpClient.GetAsync(GetPublicRequestUri(request), ServiceHttpClientOptions.Default(), cancellationToken);

            var jsonContent = await response.GetContentAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();

            var dto = IsolatedSerialization.Deserialize<ReachableStatusesDto>(jsonContent, IsolatedSerialization.defaultSettings);

            var statusFlowDescriptor = new StatusFlowDescriptor(assetDescriptor.OrganizationId, dto.StatusFlowId);
            var assetStatusDto = await GetAssetStatusDtoAsync(assetDescriptor, cancellationToken);
            var statuses = assetStatusDto.StatusFlow.Statuses
                .Where(x => dto.ReachableStatusNames.Contains(x.Name))
                .ToArray();

            return (statusFlowDescriptor, statuses);
        }

        /// <inheritdoc />
        public async Task<string[]> GetReachableStatusNamesAsync(AssetDescriptor assetDescriptor, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = AssetStatusRequest.GetReachableStatusRequest(assetDescriptor.ProjectId, assetDescriptor.AssetId, assetDescriptor.AssetVersion);
            using var response = await m_ServiceHttpClient.GetAsync(GetPublicRequestUri(request), ServiceHttpClientOptions.Default(), cancellationToken);

            var jsonContent = await response.GetContentAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();

            var dto = IsolatedSerialization.Deserialize<ReachableStatusesDto>(jsonContent, IsolatedSerialization.defaultSettings);

            return dto.ReachableStatusNames;
        }

        /// <inheritdoc />
        public async Task UpdateAssetStatusFlowAsync(AssetDescriptor assetDescriptor, StatusFlowDescriptor statusFlowDescriptor, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = new AssignAssetStatusFlowRequest(assetDescriptor.ProjectId, assetDescriptor.AssetId, assetDescriptor.AssetVersion, statusFlowDescriptor.StatusFlowId);
            using var _ = await m_ServiceHttpClient.PostAsync(GetPublicRequestUri(request), request.ConstructBody(), ServiceHttpClientOptions.Default(), cancellationToken);
        }

        async Task<AssetStatusDto> GetAssetStatusDtoAsync(AssetDescriptor assetDescriptor, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = AssetStatusRequest.GetCurrentStatusRequest(assetDescriptor.ProjectId, assetDescriptor.AssetId, assetDescriptor.AssetVersion);
            using var response = await m_ServiceHttpClient.GetAsync(GetPublicRequestUri(request), ServiceHttpClientOptions.Default(), cancellationToken);

            var jsonContent = await response.GetContentAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();

            return IsolatedSerialization.Deserialize<AssetStatusDto>(jsonContent, IsolatedSerialization.defaultSettings);
        }
    }
}
