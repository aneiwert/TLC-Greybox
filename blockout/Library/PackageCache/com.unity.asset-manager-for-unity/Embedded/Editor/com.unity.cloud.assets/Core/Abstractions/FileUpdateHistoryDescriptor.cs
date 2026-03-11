using System;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// This struct contains the identifiers for a file's update history entry.
    /// </summary>
    struct FileUpdateHistoryDescriptor
    {
        /// <summary>
        /// The sequence number of the history entry.
        /// </summary>
        public int SequenceNumber { get; internal set; }

        /// <summary>
        /// The file descriptor of the history entry.
        /// </summary>
        public FileDescriptor FileDescriptor { get; internal set; }

        /// <inheritdoc cref="ProjectDescriptor.OrganizationId"/>
        public OrganizationId OrganizationId => FileDescriptor.OrganizationId;

        /// <inheritdoc cref="ProjectDescriptor.ProjectId"/>
        public ProjectId ProjectId => FileDescriptor.ProjectId;

        /// <inheritdoc cref="AssetDescriptor.AssetId"/>
        public AssetId AssetId => FileDescriptor.AssetId;

        /// <inheritdoc cref="AssetDescriptor.AssetVersion"/>
        public AssetVersion AssetVersion => FileDescriptor.AssetVersion;

        /// <inheritdoc cref="CommonEmbedded.DatasetId"/>
        public DatasetId DatasetId => FileDescriptor.DatasetId;
        
        /// <inheritdoc cref="FileDescriptor.Path"/>
        public string FilePath => FileDescriptor.Path;

        /// <summary>
        /// Creates an instance of the <see cref="FileUpdateHistoryDescriptor"/> struct.
        /// </summary>
        /// <param name="fileDescriptor">The history entry's file descriptor.</param>
        /// <param name="sequenceNumber">The history entry's sequence number.</param>
        public FileUpdateHistoryDescriptor(FileDescriptor fileDescriptor, int sequenceNumber)
        {
            SequenceNumber = sequenceNumber;
            FileDescriptor = fileDescriptor;
        }

        /// <summary>
        /// Returns whether two <see cref="FileUpdateHistoryDescriptor"/> objects are equals.
        /// </summary>
        /// <param name="other">Compare the values with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instance have the same values;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool Equals(FileUpdateHistoryDescriptor other)
        {
            return FileDescriptor.Equals(other.FileDescriptor) &&
                SequenceNumber.Equals(other.SequenceNumber);
        }

        /// <summary>
        /// Validate <paramref name="obj"/> is a <see cref="FileUpdateHistoryDescriptor"/> instance and have the same values as this instance.
        /// </summary>
        /// <param name="obj">Compare the values with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instance have the same values;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public override bool Equals(object obj) => obj is FileUpdateHistoryDescriptor other && Equals(other);

        /// <summary>
        /// Compute a hash code for the object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        /// <remarks>
        /// * You should not assume that equal hash codes imply object equality.
        /// * You should never persist or use a hash code outside the application domain in which it was created,
        ///   because the same object may hash differently across application domains, processes, and platforms.
        /// </remarks>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = SequenceNumber.GetHashCode();
                hashCode = (hashCode * 397) ^ FileDescriptor.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Checks whether two <see cref="FileUpdateHistoryDescriptor"/> are equal.
        /// </summary>
        /// <param name="left">Compare with this first instance.</param>
        /// <param name="right">Compare with this other instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instances represent the same;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator ==(FileUpdateHistoryDescriptor left, FileUpdateHistoryDescriptor right) => left.Equals(right);

        /// <summary>
        /// Checks whether two <see cref="FileUpdateHistoryDescriptor"/> aren't equal.
        /// </summary>
        /// <param name="left">Compare with this first instance.</param>
        /// <param name="right">Compare with this other instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instances are not the same;
        /// <see langword="false"/> if both instances are the same.
        /// </returns>
        public static bool operator !=(FileUpdateHistoryDescriptor left, FileUpdateHistoryDescriptor right) => !left.Equals(right);
    }
}
