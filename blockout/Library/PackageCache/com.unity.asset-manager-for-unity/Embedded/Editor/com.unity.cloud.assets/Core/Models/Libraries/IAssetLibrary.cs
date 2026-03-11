using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// This interface represents a library.
    /// </summary>
    interface IAssetLibrary
    {
        /// <summary>
        /// The identifier of the library.
        /// </summary>
        AssetLibraryId Id { get; }

        /// <summary>
        /// The cache configuration for the library.
        /// </summary>
        AssetLibraryCacheConfiguration CacheConfiguration { get; }

        /// <summary>
        /// Returns an asset library configured with the specified caching configuration.
        /// </summary>
        /// <param name="assetLibraryCacheConfiguration">The caching configuration for the library. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>A task whose result is an <see cref="IAssetLibrary"/> with cached values specified by the caching configuration. </returns>
        Task<IAssetLibrary> WithCacheConfigurationAsync(AssetLibraryCacheConfiguration assetLibraryCacheConfiguration, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves and caches the library properties.
        /// </summary>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>A task with no result. </returns>
        Task RefreshAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Returns the properties of the library.
        /// </summary>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>A task whose result is the <see cref="AssetLibraryProperties"/> of the library. </returns>
        Task<AssetLibraryProperties> GetPropertiesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Returns a builder to create a query to search the library's <see cref="IAsset"/>.
        /// </summary>
        /// <returns>An <see cref="AssetQueryBuilder"/>. </returns>
        AssetQueryBuilder QueryAssets();

        /// <summary>
        /// Retrieves an asset by its ID and version.
        /// </summary>
        /// <param name="assetId">The id of the asset. </param>
        /// <param name="assetVersion">The version of the asset. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>A task whose result is the requested asset. </returns>
        Task<IAsset> GetAssetAsync(AssetId assetId, AssetVersion assetVersion, CancellationToken cancellationToken);

        /// <summary>
        /// Returns an object that can be used to query the asset's versions.
        /// </summary>
        /// <param name="assetId">The id of the asset to query. </param>
        /// <returns>A <see cref="VersionQueryBuilder"/>. </returns>
        VersionQueryBuilder QueryAssetVersions(AssetId assetId);

        /// <summary>
        /// Returns an object that can be used to query asset labels across versions.
        /// </summary>
        /// <param name="assetId">The id of the asset to query. </param>
        /// <returns>A <see cref="AssetLabelQueryBuilder"/>. </returns>
        AssetLabelQueryBuilder QueryAssetLabels(AssetId assetId);

        /// <summary>
        /// Returns a builder to create a query to count the library's <see cref="IAsset"/>.
        /// </summary>
        /// <returns>An <see cref="GroupAndCountAssetsQueryBuilder"/>. </returns>
        GroupAndCountAssetsQueryBuilder GroupAndCountAssets();

        /// <summary>
        /// Returns the number of assets in the library.
        /// </summary>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>A task whose result is the number of assets in the library. </returns>
        Task<int> CountAssetsAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Returns a builder to create a query to search the library's <see cref="IAssetCollection"/>.
        /// </summary>
        /// <returns>A <see cref="CollectionQueryBuilder"/>. </returns>
        CollectionQueryBuilder QueryCollections();

        /// <summary>
        /// Returns the collection at the specified path.
        /// </summary>
        /// <param name="collectionPath">The path to the collection. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>A task whose result is the requested collection. </returns>
        Task<IAssetCollection> GetAssetCollectionAsync(CollectionPath collectionPath, CancellationToken cancellationToken);

        /// <summary>
        /// Returns a builder to create a query to search the library's <see cref="IFieldDefinition"/>.
        /// </summary>
        /// <param name="fieldDefinitionKeys">The keys of the field definitions to query. </param>
        /// <returns>A <see cref="FieldDefinitionQueryBuilder"/>. </returns>
        FieldDefinitionQueryBuilder QueryFieldDefinitions(IEnumerable<string> fieldDefinitionKeys);

        /// <summary>
        /// Retrieves an <see cref="IFieldDefinition"/>.
        /// </summary>
        /// <param name="fieldKey">The key identifying the field definition. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>A task whose result is an <see cref="IFieldDefinition"/>. </returns>
        Task<IFieldDefinition> GetFieldDefinitionAsync(string fieldKey, CancellationToken cancellationToken);

        /// <summary>
        /// Returns a builder to create a query to search the library's labels.
        /// </summary>
        /// <returns>A <see cref="LabelQueryBuilder"/>. </returns>
        LabelQueryBuilder QueryLabels();

        /// <summary>
        /// Retrieves a label by name.
        /// </summary>
        /// <param name="labelName">The name identifying the label. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>A task whose result is an <see cref="ILabel"/>. </returns>
        Task<ILabel> GetLabelAsync(string labelName, CancellationToken cancellationToken);

        /// <summary>
        /// Copies assets from the library to a project.
        /// </summary>
        /// <param name="destinationProjectDescriptor">The project to copy the assets to. </param>
        /// <param name="assetsToCopy">An object that defines the assets to be copied. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>An enumeration of <see cref="AssetLibraryJobId"/> equal to the number of assets requested. </returns>
        IAsyncEnumerable<IAssetLibraryJob> StartCopyAssetsJobAsync(ProjectDescriptor destinationProjectDescriptor, AssetsToCopy assetsToCopy, CancellationToken cancellationToken);
    }
}
