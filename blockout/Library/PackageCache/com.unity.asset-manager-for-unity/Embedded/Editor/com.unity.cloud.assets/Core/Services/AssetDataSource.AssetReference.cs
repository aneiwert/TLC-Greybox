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
        public IAsyncEnumerable<IAssetReferenceData> ListAssetReferencesAsync(ProjectDescriptor projectDescriptor, AssetId assetId, AssetVersion? assetVersion, string context, Range range, CancellationToken cancellationToken)
        {
            const int maxPageSize = 254;

            var countRequest = new AssetReferenceRequest(projectDescriptor.ProjectId, assetId, assetVersion, context, 0, 1);
            return ListEntitiesAsync<AssetReferenceData>(countRequest, GetListRequest, range, cancellationToken, maxPageSize);

            ApiRequest GetListRequest(int offset, int pageSize) => new AssetReferenceRequest(projectDescriptor.ProjectId, assetId, assetVersion, context, offset, pageSize);
        }

        public async Task<string> CreateAssetReferenceAsync(AssetDescriptor assetDescriptor, AssetIdentifierDto assetIdentifierDto, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var requestBody = new CreateAssetReferenceRequestBody
            {
                AssetVersion = assetDescriptor.AssetVersion.ToString(),
                Target = assetIdentifierDto
            };
            var request = new AssetReferenceRequest(assetDescriptor.ProjectId, assetDescriptor.AssetId, requestBody);
            using var response = await m_ServiceHttpClient.PostAsync(GetPublicRequestUri(request), request.ConstructBody(),
                ServiceHttpClientOptions.Default(), cancellationToken);

            var jsonContent = await response.GetContentAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();

            var dto = DeserializeCollectionPath<CreateAssetReferenceResponseBody>(jsonContent);

            return dto.ReferenceId;
        }

        public async Task DeleteAssetReferenceAsync(ProjectDescriptor projectDescriptor, AssetId assetId, string referenceId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = new AssetReferenceRequest(projectDescriptor.ProjectId, assetId, referenceId);
            using var _ = await m_ServiceHttpClient.DeleteAsync(GetPublicRequestUri(request), ServiceHttpClientOptions.Default(), cancellationToken);
        }
    }
}
