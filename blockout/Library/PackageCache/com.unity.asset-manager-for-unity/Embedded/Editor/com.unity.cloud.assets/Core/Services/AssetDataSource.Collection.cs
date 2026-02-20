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
        public async IAsyncEnumerable<IAssetCollectionData> GetAssetCollectionsAsync(AssetDescriptor assetDescriptor, Range range, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var (start, length) = range.GetValidatedOffsetAndLength(int.MaxValue);

            if (length == 0) yield break;

            var request = AssetRequest.GetAssetCollectionsRequest(assetDescriptor.ProjectId, assetDescriptor.AssetId);
            using var response = await m_ServiceHttpClient.GetAsync(GetPublicRequestUri(request), ServiceHttpClientOptions.Default(),
                cancellationToken);

            var jsonContent = await response.GetContentAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();

            var assetCollectionDtos = DeserializeCollectionPath<AssetCollectionData[]>(jsonContent);
            for (var i = start; i < start + length && i < assetCollectionDtos.Length; ++i)
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return assetCollectionDtos[i];
            }
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<IAssetCollectionData> ListCollectionsAsync(AssetLibraryId assetLibraryId, Range range, CancellationToken cancellationToken)
        {
            const int maxPageSize = 1000;

            var countRequest = new GetCollectionListRequest(assetLibraryId, 0, 1);
            return ListEntitiesAsync<AssetCollectionData>(countRequest, GetListRequest, range, cancellationToken, maxPageSize);

            ApiRequest GetListRequest(int next, int pageSize) => new GetCollectionListRequest(assetLibraryId, next, pageSize);
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<IAssetCollectionData> ListCollectionsAsync(ProjectDescriptor projectDescriptor, Range range, CancellationToken cancellationToken)
        {
            return ListEntitiesAsync<AssetCollectionData>(GetListRequest, range, cancellationToken);

            ApiRequest GetListRequest(int offset, int pageSize) => new GetCollectionListRequest(projectDescriptor.ProjectId, offset, pageSize);
        }

        /// <inheritdoc/>
        public Task<IAssetCollectionData> GetCollectionAsync(CollectionDescriptor collectionDescriptor, CancellationToken cancellationToken)
        {
            return collectionDescriptor.IsPathToAssetLibrary()
                ? GetCollectionAsync_FromAssetLibrary(collectionDescriptor, cancellationToken)
                : GetCollectionAsync_FromProject(collectionDescriptor, cancellationToken);
        }

        async Task<IAssetCollectionData> GetCollectionAsync_FromAssetLibrary(CollectionDescriptor collectionDescriptor, CancellationToken cancellationToken)
        {
            var results = ListCollectionsAsync(collectionDescriptor.AssetLibraryId, Range.All, cancellationToken);
            await foreach (var result in results)
            {
                if (result.GetFullCollectionPath() == collectionDescriptor.Path)
                {
                    return result;
                }
            }

            throw new NotFoundException("Asset Collection does not exist.");
        }

        async Task<IAssetCollectionData> GetCollectionAsync_FromProject(CollectionDescriptor collectionDescriptor, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = new CollectionRequest(collectionDescriptor.ProjectId, collectionDescriptor.Path);
            using var response = await m_ServiceHttpClient.GetAsync(GetPublicRequestUri(request), ServiceHttpClientOptions.Default(),
                cancellationToken);

            var jsonContent = await response.GetContentAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();

            return DeserializeCollectionPath<AssetCollectionData>(jsonContent);
        }

        /// <inheritdoc/>
        public async Task<CollectionPath> CreateCollectionAsync(ProjectDescriptor projectDescriptor, IAssetCollectionData assetCollection, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = new CreateCollectionRequest(projectDescriptor.ProjectId, assetCollection);
            using var response = await m_ServiceHttpClient.PostAsync(GetPublicRequestUri(request), request.ConstructBody(),
                ServiceHttpClientOptions.Default(), cancellationToken);

            var jsonContent = await response.GetContentAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();

            var pathDto = DeserializeCollectionPath<AssetCollectionPathDto>(jsonContent);

            return pathDto.Path;
        }

        /// <inheritdoc/>
        public async Task UpdateCollectionAsync(CollectionDescriptor collectionDescriptor, IAssetCollectionData assetCollection, CancellationToken cancellationToken)
        {
            var request = new CollectionRequest(collectionDescriptor.ProjectId, collectionDescriptor.Path, assetCollection);
            using var _ = await m_ServiceHttpClient.PatchAsync(GetPublicRequestUri(request), request.ConstructBody(),
                ServiceHttpClientOptions.Default(), cancellationToken);
        }

        /// <inheritdoc/>
        public async Task DeleteCollectionAsync(CollectionDescriptor collectionDescriptor, CancellationToken cancellationToken)
        {
            var request = new CollectionRequest(collectionDescriptor.ProjectId, collectionDescriptor.Path);
            using var _ = await m_ServiceHttpClient.DeleteAsync(GetPublicRequestUri(request), ServiceHttpClientOptions.Default(), cancellationToken);
        }

        /// <inheritdoc />
        public async Task<int> GetCollectionCountAsync(CollectionDescriptor collectionDescriptor, bool includeSubcollections, CancellationToken cancellationToken)
        {
            var request = CollectionRequest.GetCollectionCountRequest(collectionDescriptor.ProjectId, collectionDescriptor.Path, includeSubcollections);
            using var response = await m_ServiceHttpClient.GetAsync(GetPublicRequestUri(request), ServiceHttpClientOptions.Default(),
                cancellationToken);

            var jsonContent = await response.GetContentAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();

            var pathDto = DeserializeCollectionPath<CounterDto>(jsonContent);

            return pathDto.Count;
        }

        /// <inheritdoc />
        public async Task<int> GetAssetCountAsync(CollectionDescriptor collectionDescriptor, bool includeSubcollections, CancellationToken cancellationToken)
        {
            var request = collectionDescriptor.IsPathToAssetLibrary()
                ? CollectionRequest.GetAssetCountRequest(collectionDescriptor.AssetLibraryId, collectionDescriptor.Path, includeSubcollections)
                : CollectionRequest.GetAssetCountRequest(collectionDescriptor.ProjectId, collectionDescriptor.Path, includeSubcollections);

            using var response = await m_ServiceHttpClient.GetAsync(GetPublicRequestUri(request), ServiceHttpClientOptions.Default(),
                cancellationToken);

            var jsonContent = await response.GetContentAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();

            var pathDto = DeserializeCollectionPath<CounterDto>(jsonContent);

            return pathDto.Count;
        }

        /// <inheritdoc />
        public async Task AddAssetsToCollectionAsync(CollectionDescriptor collectionDescriptor, IEnumerable<AssetId> assets, CancellationToken cancellationToken)
        {
            var request = new ModifyAssetsInCollectionRequest(collectionDescriptor.ProjectId, collectionDescriptor.Path, assets);
            using var _ = await m_ServiceHttpClient.PostAsync(GetPublicRequestUri(request), request.ConstructBody(),
                ServiceHttpClientOptions.Default(), cancellationToken);
        }

        /// <inheritdoc />
        public async Task RemoveAssetsFromCollectionAsync(CollectionDescriptor collectionDescriptor, IEnumerable<AssetId> assets, CancellationToken cancellationToken)
        {
            var request = new ModifyAssetsInCollectionRequest(collectionDescriptor.ProjectId, collectionDescriptor.Path, assets);
            using var _ = await m_ServiceHttpClient.PatchAsync(GetPublicRequestUri(request), request.ConstructBody(),
                ServiceHttpClientOptions.Default(), cancellationToken);
        }

        /// <inheritdoc />
        public async Task<CollectionPath> MoveCollectionToNewPathAsync(CollectionDescriptor collectionDescriptor, CollectionPath newCollectionPath, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = new MoveCollectionToNewPathRequest(collectionDescriptor.ProjectId, collectionDescriptor.Path, newCollectionPath);
            using var response = await m_ServiceHttpClient.PatchAsync(GetPublicRequestUri(request), request.ConstructBody(),
                ServiceHttpClientOptions.Default(), cancellationToken);

            var jsonContent = await response.GetContentAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();

            var pathDto = DeserializeCollectionPath<AssetCollectionPathDto>(jsonContent);

            return pathDto.Path;
        }

        static T DeserializeCollectionPath<T>(string json)
        {
            return IsolatedSerialization.DeserializeWithConverters<T>(json, IsolatedSerialization.CollectionPathConverter);
        }
    }
}
