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
        public IAsyncEnumerable<ILibraryData> ListLibrariesAsync(PaginationData pagination, CancellationToken cancellationToken)
        {
            const int maxPageSize = 100;

            var countRequest = new ListLibrariesRequest(0, 1);
            return ListEntitiesAsync<LibraryData>(countRequest, GetListRequest, pagination.Range, cancellationToken, maxPageSize);

            ApiRequest GetListRequest(int next, int pageSize) => new ListLibrariesRequest(next, pageSize);
        }

        /// <inheritdoc/>
        public async Task<ILibraryData> GetLibraryAsync(AssetLibraryId assetLibraryId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = ProjectOrLibraryRequest.GetLibraryRequest(assetLibraryId);
            using var response = await m_ServiceHttpClient.GetAsync(GetPublicRequestUri(request), ServiceHttpClientOptions.Default(),
                cancellationToken);

            var jsonContent = await response.GetContentAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();

            return IsolatedSerialization.DeserializeWithDefaultConverters<LibraryData>(jsonContent);
        }

        /// <inheritdoc/>
        public Task<int> GetAssetCountAsync(AssetLibraryId assetLibraryId, CancellationToken cancellationToken)
        {
            var request = ProjectOrLibraryRequest.GetAssetCountRequest(assetLibraryId);
            return GetAssetCountAsync(request, cancellationToken);
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<ILibraryJobData> ListLibraryJobsAsync(PaginationData pagination, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var request = new LibraryJobRequest();
            using var response = await m_ServiceHttpClient.GetAsync(GetPublicRequestUri(request), ServiceHttpClientOptions.Default(),
                cancellationToken);

            var jsonContent = await response.GetContentAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();

            var jobData = IsolatedSerialization.DeserializeWithDefaultConverters<LibraryJobData[]>(jsonContent);

            var (offset, length) = pagination.Range.GetValidatedOffsetAndLength(jobData.Length);
            for (var i = offset; i < offset + length; ++i)
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return jobData[i];
            }
        }

        /// <inheritdoc/>
        public async Task<ILibraryJobData> GetLibraryJobAsync(AssetLibraryJobId assetLibraryJobId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = new LibraryJobRequest(assetLibraryJobId);
            using var response = await m_ServiceHttpClient.GetAsync(GetPublicRequestUri(request), ServiceHttpClientOptions.Default(),
                cancellationToken);

            var jsonContent = await response.GetContentAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();

            return IsolatedSerialization.DeserializeWithDefaultConverters<LibraryJobData>(jsonContent);
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<ILibraryJobData> StartLibraryJobAsync(AssetLibraryId assetLibraryId, ProjectId destinationProjectId, IEnumerable<AssetToCopyData> libraryJobData, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = new CreateLibraryJobRequest(assetLibraryId, destinationProjectId, libraryJobData);
            using var response = await m_ServiceHttpClient.PostAsync(GetPublicRequestUri(request), request.ConstructBody(),
                ServiceHttpClientOptions.Default(), cancellationToken);

            var jsonContent = await response.GetContentAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();

            var results = IsolatedSerialization.DeserializeWithDefaultConverters<LibraryJobData[]>(jsonContent);
            foreach (var result in results)
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return result;
            }
        }
    }
}
