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
        /// <inheritdoc />
        public IAsyncEnumerable<IAssetData> ListAssetVersionsAsync(AssetLibraryId assetLibraryId, AssetId assetId, SearchRequestParameters parameters, CancellationToken cancellationToken)
        {
            var request = new SearchAssetVersionRequest(assetLibraryId, assetId, parameters);
            return ListAssetVersionsAsync(request, parameters, cancellationToken);
        }

        /// <inheritdoc />
        public IAsyncEnumerable<IAssetData> ListAssetVersionsAsync(ProjectDescriptor projectDescriptor, AssetId assetId, SearchRequestParameters parameters, CancellationToken cancellationToken)
        {
            var request = new SearchAssetVersionRequest(projectDescriptor.ProjectId, assetId, parameters);
            return ListAssetVersionsAsync(request, parameters, cancellationToken);
        }

        async IAsyncEnumerable<IAssetData> ListAssetVersionsAsync(ApiRequest request, SearchRequestParameters parameters, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var (offset, length) = await parameters.PaginationRange.GetOffsetAndLengthAsync(_ => Task.FromResult(int.MaxValue), cancellationToken);
            if (length == 0) yield break;

            var results = ListAssetsAsync(request, parameters, offset, length, cancellationToken);
            await foreach (var asset in results)
            {
                yield return asset;
            }
        }

        /// <inheritdoc />
        public async Task<AssetVersion> CreateUnfrozenAssetVersionAsync(AssetDescriptor parentAssetDescriptor, string statusFlowId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = new CreateAssetVersionRequest(parentAssetDescriptor.ProjectId, parentAssetDescriptor.AssetId, parentAssetDescriptor.AssetVersion, statusFlowId);
            using var response = await m_ServiceHttpClient.PostAsync(GetPublicRequestUri(request), request.ConstructBody(),
                ServiceHttpClientOptions.Default(), cancellationToken);

            var jsonContent = await response.GetContentAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();

            var dto = IsolatedSerialization.Deserialize<AssetVersionDto>(jsonContent, IsolatedSerialization.defaultSettings);
            return new AssetVersion(dto.Version);
        }

        /// <inheritdoc />
        public async Task DeleteUnfrozenAssetVersionAsync(AssetDescriptor assetDescriptor, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = new AssetRequest(assetDescriptor.ProjectId, assetDescriptor.AssetId, assetDescriptor.AssetVersion);
            using var _ = await m_ServiceHttpClient.DeleteAsync(GetPublicRequestUri(request), ServiceHttpClientOptions.Default(), cancellationToken);
        }

        /// <inheritdoc />
        public async Task<int?> FreezeAssetVersionAsync(AssetDescriptor assetDescriptor, string changeLog, bool? forceFreeze, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            HttpResponseMessage response = null;
            string jsonContent;
            try
            {
                if (forceFreeze.HasValue)
                {
                    var request = new SubmitVersionRequest(assetDescriptor.ProjectId, assetDescriptor.AssetId, assetDescriptor.AssetVersion, changeLog, forceFreeze);
                    response = await m_ServiceHttpClient.PostAsync(GetPublicRequestUri(request), request.ConstructBody(),
                        ServiceHttpClientOptions.Default(), cancellationToken);
                }
                else
                {
                    response = await AutoFreezeAssetVersionAsync(assetDescriptor, changeLog, cancellationToken);
                }

                jsonContent = await response.GetContentAsStringAsync();
                cancellationToken.ThrowIfCancellationRequested();
            }
            finally
            {
                response?.Dispose();
            }

            var dto = IsolatedSerialization.Deserialize<VersionNumberDto>(jsonContent, IsolatedSerialization.defaultSettings);
            return dto.VersionNumber;
        }

        Task<HttpResponseMessage> AutoFreezeAssetVersionAsync(AssetDescriptor assetDescriptor, string changelog, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = new AutoSubmitAssetRequest(assetDescriptor.ProjectId, assetDescriptor.AssetId, assetDescriptor.AssetVersion, changelog);
            return m_ServiceHttpClient.PostAsync(GetPublicRequestUri(request), request.ConstructBody(), ServiceHttpClientOptions.Default(), cancellationToken);
        }

        /// <inheritdoc />
        public async Task CancelFreezeAssetVersionAsync(AssetDescriptor assetDescriptor, CancellationToken cancellationToken)
        {
            var request = AutoSubmitAssetRequest.GetDisableRequest(assetDescriptor.ProjectId, assetDescriptor.AssetId, assetDescriptor.AssetVersion);
            using var _ = await m_ServiceHttpClient.PostAsync(GetPublicRequestUri(request), request.ConstructBody(), ServiceHttpClientOptions.Default(), cancellationToken);
        }
    }
}
