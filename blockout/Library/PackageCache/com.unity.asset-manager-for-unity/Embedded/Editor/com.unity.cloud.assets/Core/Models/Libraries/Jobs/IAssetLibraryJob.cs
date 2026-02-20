using System.Threading;
using System.Threading.Tasks;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// This interface represents a job created by an asset library.
    /// </summary>
    interface IAssetLibraryJob
    {
        /// <summary>
        /// The identifier for the job.
        /// </summary>
        AssetLibraryJobId Id { get; }
        
        /// <summary>
        /// Retrieves and caches the job properties.
        /// </summary>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>A task with no result. </returns>
        Task RefreshAsync(CancellationToken cancellationToken);
        
        /// <summary>
        /// Returns the properties of the job.
        /// </summary>
        /// <param name="cancellationToken">A token that can be used to cancel the request. </param>
        /// <returns>A task whose result is the <see cref="AssetLibraryJobProperties"/> of the job. </returns>
        Task <AssetLibraryJobProperties> GetPropertiesAsync(CancellationToken cancellationToken);
    }
}
