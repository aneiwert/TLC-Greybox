namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// The current state of an <see cref="IAssetLibraryJob"/>.
    /// </summary>
    enum AssetLibraryJobState
    {
        /// <summary>
        /// The job is in progress. See <see cref="AssetLibraryJobProperties.Progress"/> for current progress.
        /// </summary>
        Active,
        /// <summary>
        /// The job has finished. See <see cref="AssetLibraryJobProperties.CopiedAssetDescriptor"/> for resulting asset. <br/>
        /// </summary>
        Completed,
        /// <summary>
        /// The job has failed. See <see cref="AssetLibraryJobProperties.FailedReason"/> for details.
        /// </summary>
        Failed,
        /// <summary>
        /// The job is prioritized on the queue.
        /// </summary>
        Prioritized,
        /// <summary>
        /// The job is delayed.
        /// </summary>
        Delayed,
        /// <summary>
        /// The job is waiting for dependencies to finish processing.
        /// </summary>
        Waiting,
        /// <summary>
        /// An unknown job state. This is used when the job state is not recognized or not set.
        /// </summary>
        Unknown
    }
}
