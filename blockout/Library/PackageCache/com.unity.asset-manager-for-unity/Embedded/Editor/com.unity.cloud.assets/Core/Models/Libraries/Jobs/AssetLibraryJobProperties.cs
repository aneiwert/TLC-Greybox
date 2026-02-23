using System;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// The properties of an <see cref="IAssetLibraryJob"/>.
    /// </summary>
    [Serializable]
struct AssetLibraryJobProperties
    {
        string m_Name;
        string m_Type;
        AssetLibraryJobState m_State;
        string m_FailedReason;
        int m_Progress;
        string m_ProgressDetails;
        AssetDescriptor? m_CopiedAssetDescriptor;

        /// <summary>
        /// The name of the job.
        /// </summary>
        public string Name
        {
            get => m_Name;
            internal set => m_Name = value;
        }

        /// <summary>
        /// The state of the job's progress.
        /// </summary>
        public AssetLibraryJobState State
        {
            get => m_State;
            internal set => m_State = value;
        }

        /// <summary>
        /// The reason why the job failed, if applicable.
        /// </summary>
        public string FailedReason
        {
            get => m_FailedReason;
            internal set => m_FailedReason = value;
        }

        /// <summary>
        /// The progress of the job.
        /// </summary>
        public int Progress
        {
            get => m_Progress;
            internal set => m_Progress = value;
        }
        
        /// <summary>
        /// The details of the job's progress, if available.
        /// </summary>
        public string ProgressDetails
        {
            get => m_ProgressDetails;
            internal set => m_ProgressDetails = value;
        }
        
        /// <summary>
        /// The resulting asset descriptor if the job copied an asset.
        /// </summary>
        public AssetDescriptor? CopiedAssetDescriptor
        {
            get => m_CopiedAssetDescriptor;
            internal set => m_CopiedAssetDescriptor = value;
        }
    }
}
