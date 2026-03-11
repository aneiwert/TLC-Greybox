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
        public IAsyncEnumerable<IFieldDefinitionData> ListFieldDefinitionsAsync(AssetLibraryId assetLibraryId, PaginationData pagination, Dictionary<string, string[]> queryParameters, CancellationToken cancellationToken)
        {
            const int maxPageSize = 99;

            return ListEntitiesAsync<FieldDefinitionData>(_cancellationToken =>
            {
                _cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(int.MaxValue);
            }, GetListRequest, pagination.Range, cancellationToken, maxPageSize);

            ApiRequest GetListRequest(string next, int pageSize) => new GetFieldDefinitionListRequest(assetLibraryId, pageSize, pagination.SortingOrder, next, queryParameters);
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<IFieldDefinitionData> ListFieldDefinitionsAsync(OrganizationId organizationId, PaginationData pagination, Dictionary<string, string[]> queryParameters, CancellationToken cancellationToken)
        {
            const int maxPageSize = 99;

            return ListEntitiesAsync<FieldDefinitionData>(_cancellationToken =>
            {
                _cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(int.MaxValue);
            }, GetListRequest, pagination.Range, cancellationToken, maxPageSize);

            ApiRequest GetListRequest(string next, int pageSize) => new GetFieldDefinitionListRequest(organizationId, pageSize, pagination.SortingOrder, next, queryParameters);
        }

        /// <inheritdoc/>
        public Task<IFieldDefinitionData> GetFieldDefinitionAsync(FieldDefinitionDescriptor fieldDefinitionDescriptor, CancellationToken cancellationToken)
        {
            return fieldDefinitionDescriptor.IsPathToAssetLibrary()
                ? GetFieldDefinitionAsync(fieldDefinitionDescriptor.AssetLibraryId, fieldDefinitionDescriptor.FieldKey, cancellationToken)
                : GetFieldDefinitionAsync(fieldDefinitionDescriptor.OrganizationId, fieldDefinitionDescriptor.FieldKey, cancellationToken);
        }

        async Task<IFieldDefinitionData> GetFieldDefinitionAsync(AssetLibraryId assetLibraryId, string fieldKey, CancellationToken cancellationToken)
        {
            var searchParameters = new Dictionary<string, string[]>
            {
                {"name", new[] {fieldKey}}
            };
            var results = ListFieldDefinitionsAsync(assetLibraryId, new PaginationData {Range = Range.All}, searchParameters, cancellationToken);
            await foreach (var result in results)
            {
                if (result.Name == fieldKey)
                {
                    return result;
                }
            }

            throw new NotFoundException("Field Definition does not exist.");
        }

        async Task<IFieldDefinitionData> GetFieldDefinitionAsync(OrganizationId organizationId, string fieldKey, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = new FieldDefinitionRequest(organizationId, fieldKey);
            using var response = await m_ServiceHttpClient.GetAsync(GetPublicRequestUri(request), ServiceHttpClientOptions.Default(), cancellationToken);

            var jsonContent = await response.GetContentAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();

            return JsonSerialization.Deserialize<FieldDefinitionData>(jsonContent);
        }

        /// <inheritdoc/>
        public async Task<FieldDefinitionDescriptor> CreateFieldDefinitionAsync(OrganizationId organizationId, IFieldDefinitionCreateData fieldCreation, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = new CreateFieldDefinitionRequest(organizationId, fieldCreation);
            using var _ = await m_ServiceHttpClient.PostAsync(GetPublicRequestUri(request), request.ConstructBody(),
                ServiceHttpClientOptions.Default(), cancellationToken);

            return new FieldDefinitionDescriptor(organizationId, fieldCreation.Name);
        }

        /// <inheritdoc/>
        public async Task DeleteFieldDefinitionAsync(FieldDefinitionDescriptor fieldDefinitionDescriptor, CancellationToken cancellationToken)
        {
            var request = new FieldDefinitionRequest(fieldDefinitionDescriptor.OrganizationId, fieldDefinitionDescriptor.FieldKey);
            using var _ = await m_ServiceHttpClient.DeleteAsync(GetPublicRequestUri(request), ServiceHttpClientOptions.Default(), cancellationToken);
        }

        /// <inheritdoc/>
        public async Task UpdateFieldDefinitionAsync(FieldDefinitionDescriptor fieldDefinitionDescriptor, IFieldDefinitionBaseData fieldUpdate, CancellationToken cancellationToken)
        {
            var request = new FieldDefinitionRequest(fieldDefinitionDescriptor.OrganizationId, fieldDefinitionDescriptor.FieldKey, fieldUpdate);
            using var _ = await m_ServiceHttpClient.PatchAsync(GetPublicRequestUri(request), request.ConstructBody(),
                ServiceHttpClientOptions.Default(), cancellationToken);
        }

        public async Task AddAcceptedValuesToFieldDefinitionAsync(FieldDefinitionDescriptor fieldDefinitionDescriptor, IEnumerable<string> acceptedValues, CancellationToken cancellationToken)
        {
            var request = new ModifyFieldDefinitionSelectionRequest(fieldDefinitionDescriptor.OrganizationId, fieldDefinitionDescriptor.FieldKey, acceptedValues);
            using var _ = await m_ServiceHttpClient.PostAsync(GetPublicRequestUri(request), request.ConstructBody(),
                ServiceHttpClientOptions.Default(), cancellationToken);
        }

        public async Task RemoveAcceptedValuesFromFieldDefinitionAsync(FieldDefinitionDescriptor fieldDefinitionDescriptor, IEnumerable<string> acceptedValues, CancellationToken cancellationToken)
        {
            var request = new ModifyFieldDefinitionSelectionRequest(fieldDefinitionDescriptor.OrganizationId, fieldDefinitionDescriptor.FieldKey, acceptedValues);
            using var _ = await m_ServiceHttpClient.DeleteAsync(GetPublicRequestUri(request), ServiceHttpClientOptions.Default(), cancellationToken);
        }
    }
}
