using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    partial class AssetDataSource
    {
        /// <inheritdoc/>
        public IAsyncEnumerable<ILabelData> ListLabelsAsync(AssetLibraryId assetLibraryId, PaginationData pagination, bool? archived, bool? systemLabels, CancellationToken cancellationToken)
        {
            var countRequest = new ListLabelsRequest(assetLibraryId, 0, 1, archived, systemLabels);
            return ListEntitiesAsync<LabelData>(countRequest, GetListRequest, pagination.Range, cancellationToken);

            ApiRequest GetListRequest(int offset, int pageSize) => new ListLabelsRequest(assetLibraryId, offset, pageSize, archived, systemLabels);
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<ILabelData> ListLabelsAsync(OrganizationId organizationId, PaginationData pagination, bool? archived, bool? systemLabels, CancellationToken cancellationToken)
        {
            var countRequest = new ListLabelsRequest(organizationId, 0, 1, archived, systemLabels);
            return ListEntitiesAsync<LabelData>(countRequest, GetListRequest, pagination.Range, cancellationToken);

            ApiRequest GetListRequest(int offset, int pageSize) => new ListLabelsRequest(organizationId, offset, pageSize, archived, systemLabels);
        }

        /// <inheritdoc/>
        public async Task<ILabelData> GetLabelAsync(LabelDescriptor labelDescriptor, CancellationToken cancellationToken)
        {
            // Not yet implemented in backend, we need to pass through search all API
            /*
            cancellationToken.ThrowIfCancellationRequested();

            var request = new LabelRequest(labelDescriptor.OrganizationId, labelDescriptor.LabelName);
            using var response = await m_ServiceHttpClient.GetAsync(GetPublicRequestUri(request), ServiceHttpClientOptions.Default(), cancellationToken);

            var jsonContent = await response.GetContentAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();

            return JsonSerialization.Deserialize<LabelData>(jsonContent);
            */

            var results = labelDescriptor.IsPathToAssetLibrary()
                ? ListLabelsAsync(labelDescriptor.AssetLibraryId, new PaginationData {Range = Range.All}, null, null, cancellationToken)
                : ListLabelsAsync(labelDescriptor.OrganizationId, new PaginationData {Range = Range.All}, null, null, cancellationToken);
            await foreach (var result in results.WithCancellation(cancellationToken))
            {
                if (result.Name == labelDescriptor.LabelName)
                {
                    return result;
                }
            }

            throw new NotFoundException("Label does not exist.");
        }

        /// <inheritdoc/>
        public async Task<LabelDescriptor> CreateLabelAsync(OrganizationId organizationId, ILabelBaseData labelCreation, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = new CreateLabelRequest(organizationId, labelCreation);
            using var response = await m_ServiceHttpClient.PostAsync(GetPublicRequestUri(request), request.ConstructBody(),
                ServiceHttpClientOptions.Default(), cancellationToken);

            var jsonContent = await response.GetContentAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();

            var createdLabel = JsonSerialization.Deserialize<CreatedLabelDto>(jsonContent);
            if (createdLabel.Name != Uri.EscapeDataString(labelCreation.Name))
            {
                k_Logger.LogWarning($"The created label name '{createdLabel.Name}' does not match the requested label name '{labelCreation.Name}' when URL escaped as '{Uri.EscapeDataString(labelCreation.Name)}'.");
            }

            return new LabelDescriptor(organizationId, createdLabel.Name);
        }

        /// <inheritdoc/>
        public async Task UpdateLabelAsync(LabelDescriptor labelDescriptor, ILabelBaseData labelUpdate, CancellationToken cancellationToken)
        {
            var request = new LabelRequest(labelDescriptor.OrganizationId, labelDescriptor.LabelName, labelUpdate);
            using var _ = await m_ServiceHttpClient.PatchAsync(GetPublicRequestUri(request), request.ConstructBody(),
                ServiceHttpClientOptions.Default(), cancellationToken);
        }

        /// <inheritdoc/>
        public async Task UpdateLabelStatusAsync(LabelDescriptor labelDescriptor, bool archive, CancellationToken cancellationToken)
        {
            var request = new UpdateLabelStatusRequest(labelDescriptor.OrganizationId, labelDescriptor.LabelName, archive);
            using var _ = await m_ServiceHttpClient.PostAsync(GetPublicRequestUri(request), request.ConstructBody(), ServiceHttpClientOptions.Default(), cancellationToken);
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<AssetLabelsDto> ListLabelsAcrossAssetVersions(AssetLibraryId assetLibraryId, AssetId assetId, PaginationData pagination, CancellationToken cancellationToken)
        {
            var countRequest = new ListAssetLabelsRequest(assetLibraryId, assetId, 0, 1);
            return ListEntitiesAsync<AssetLabelsDto>(countRequest, GetListRequest, pagination.Range, cancellationToken);

            ApiRequest GetListRequest(int offset, int pageSize) => new ListAssetLabelsRequest(assetLibraryId, assetId, offset, pageSize);
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<AssetLabelsDto> ListLabelsAcrossAssetVersions(ProjectDescriptor projectDescriptor, AssetId assetId, PaginationData pagination, CancellationToken cancellationToken)
        {
            var countRequest = new ListAssetLabelsRequest(projectDescriptor.ProjectId, assetId, 0, 1);
            return ListEntitiesAsync<AssetLabelsDto>(countRequest, GetListRequest, pagination.Range, cancellationToken);

            ApiRequest GetListRequest(int offset, int pageSize) => new ListAssetLabelsRequest(projectDescriptor.ProjectId, assetId, offset, pageSize);
        }

        /// <inheritdoc/>
        public async Task AssignLabelsAsync(AssetDescriptor assetDescriptor, IEnumerable<string> labels, CancellationToken cancellationToken)
        {
            var request = new AssignLabelRequest(assetDescriptor.ProjectId, assetDescriptor.AssetId, assetDescriptor.AssetVersion, true, labels);
            using var _ = await m_ServiceHttpClient.PostAsync(GetPublicRequestUri(request), request.ConstructBody(), ServiceHttpClientOptions.Default(), cancellationToken);
        }

        /// <inheritdoc/>
        public async Task UnassignLabelsAsync(AssetDescriptor assetDescriptor, IEnumerable<string> labels, CancellationToken cancellationToken)
        {
            var request = new AssignLabelRequest(assetDescriptor.ProjectId, assetDescriptor.AssetId, assetDescriptor.AssetVersion, false, labels);
            using var _ = await m_ServiceHttpClient.PostAsync(GetPublicRequestUri(request), request.ConstructBody(), ServiceHttpClientOptions.Default(), cancellationToken);
        }

        async Task<int> GetTotalCount(ApiRequest apiRequest, CancellationToken cancellationToken)
        {
            using var response = await m_ServiceHttpClient.GetAsync(GetPublicRequestUri(apiRequest), ServiceHttpClientOptions.Default(), cancellationToken);
            var jsonContent = await response.GetContentAsStringAsync();
            var pageDto = IsolatedSerialization.Deserialize<PaginationDto>(jsonContent, IsolatedSerialization.defaultSettings);
            return pageDto.Total;
        }
    }
}
