using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    partial interface IAssetDataSource
    {
        /// <summary>
        /// Retrieves a list of <see cref="ILibraryData"/> for the current user.
        /// </summary>
        /// <param name="pagination">An object containing the necessary information return a range of results. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>An async enumeration of <see cref="ILibraryData"/>. </returns>
        IAsyncEnumerable<ILibraryData> ListLibrariesAsync(PaginationData pagination, CancellationToken cancellationToken);

        /// <summary>
        /// Gets an <see cref="ILibraryData"/>.
        /// </summary>
        /// <param name="assetLibraryId">ID of the library. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request.</param>
        /// <returns>A task whose result is a <see cref="ILibraryData"/>. </returns>
        Task<ILibraryData> GetLibraryAsync(AssetLibraryId assetLibraryId, CancellationToken cancellationToken);

        /// <summary>
        /// Returns the number of assets in a library.
        /// </summary>
        /// <param name="assetLibraryId">ID of the library. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request.</param>
        /// <returns>A task whose result is the number of assets in the library. </returns>
        Task<int> GetAssetCountAsync(AssetLibraryId assetLibraryId, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a list of <see cref="ILibraryJobData"/> for the current user.
        /// </summary>
        /// <param name="pagination">An object containing the necessary information return a range of results. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request.</param>
        /// <returns>An async enumeration of <see cref="ILibraryJobData"/>. </returns>
        IAsyncEnumerable<ILibraryJobData> ListLibraryJobsAsync(PaginationData pagination, CancellationToken cancellationToken);

        /// <summary>
        /// Gets an <see cref="ILibraryJobData"/>.
        /// </summary>
        /// <param name="assetLibraryJobId">ID of the library job. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request.</param>
        /// <returns>A task whose result is a <see cref="ILibraryJobData"/>. </returns>
        Task<ILibraryJobData> GetLibraryJobAsync(AssetLibraryJobId assetLibraryJobId, CancellationToken cancellationToken);

        /// <summary>
        /// Starts a library job for copying assets from a library to a project.
        /// </summary>
        /// <param name="assetLibraryId">ID of the library. </param>
        /// <param name="destinationProjectId">ID of the project. </param>
        /// <param name="libraryJobData">The assets to be copied. </param>
        /// <param name="cancellationToken">A token that can be used to cancel the request.</param>
        /// <returns>An async enumeration of <see cref="ILibraryData"/>. </returns>
        IAsyncEnumerable<ILibraryJobData> StartLibraryJobAsync(AssetLibraryId assetLibraryId, ProjectId destinationProjectId, IEnumerable<AssetToCopyData> libraryJobData, CancellationToken cancellationToken);
    }
}
