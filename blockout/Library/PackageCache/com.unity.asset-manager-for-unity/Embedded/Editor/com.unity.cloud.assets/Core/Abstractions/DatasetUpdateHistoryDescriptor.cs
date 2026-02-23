using System;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// This struct contains the identifiers for a dataset's update history entry.
    /// </summary>
    struct DatasetUpdateHistoryDescriptor
    {
        /// <summary>
        /// The sequence number of the history entry.
        /// </summary>
        public int SequenceNumber { get; internal set; }

        /// <summary>
        /// The dataset descriptor of the history entry.
        /// </summary>
        public DatasetDescriptor DatasetDescriptor { get; internal set; }

        /// <inheritdoc cref="ProjectDescriptor.OrganizationId"/>
        public OrganizationId OrganizationId => DatasetDescriptor.OrganizationId;

        /// <inheritdoc cref="ProjectDescriptor.ProjectId"/>
        public ProjectId ProjectId => DatasetDescriptor.ProjectId;

        /// <inheritdoc cref="AssetDescriptor.AssetId"/>
        public AssetId AssetId => DatasetDescriptor.AssetId;

        /// <inheritdoc cref="AssetDescriptor.AssetVersion"/>
        public AssetVersion AssetVersion => DatasetDescriptor.AssetVersion;

        /// <inheritdoc cref="CommonEmbedded.DatasetId"/>
        public DatasetId DatasetId => DatasetDescriptor.DatasetId;

        /// <summary>
        /// Creates an instance of the <see cref="DatasetUpdateHistoryDescriptor"/> struct.
        /// </summary>
        /// <param name="datasetDescriptor">The history entry's dataset descriptor.</param>
        /// <param name="sequenceNumber">The history entry's sequence number.</param>
        public DatasetUpdateHistoryDescriptor(DatasetDescriptor datasetDescriptor, int sequenceNumber)
        {
            SequenceNumber = sequenceNumber;
            DatasetDescriptor = datasetDescriptor;
        }

        /// <summary>
        /// Returns whether two <see cref="DatasetUpdateHistoryDescriptor"/> objects are equals.
        /// </summary>
        /// <param name="other">Compare the values with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instance have the same values;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool Equals(DatasetUpdateHistoryDescriptor other)
        {
            return DatasetDescriptor.Equals(other.DatasetDescriptor) &&
                SequenceNumber.Equals(other.SequenceNumber);
        }

        /// <summary>
        /// Validate <paramref name="obj"/> is a <see cref="DatasetUpdateHistoryDescriptor"/> instance and have the same values as this instance.
        /// </summary>
        /// <param name="obj">Compare the values with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instance have the same values;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public override bool Equals(object obj) => obj is DatasetUpdateHistoryDescriptor other && Equals(other);

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
                hashCode = (hashCode * 397) ^ DatasetDescriptor.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Checks whether two <see cref="DatasetUpdateHistoryDescriptor"/> are equal.
        /// </summary>
        /// <param name="left">Compare with this first instance.</param>
        /// <param name="right">Compare with this other instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instances represent the same;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator ==(DatasetUpdateHistoryDescriptor left, DatasetUpdateHistoryDescriptor right) => left.Equals(right);

        /// <summary>
        /// Checks whether two <see cref="DatasetUpdateHistoryDescriptor"/> aren't equal.
        /// </summary>
        /// <param name="left">Compare with this first instance.</param>
        /// <param name="right">Compare with this other instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instances are not the same;
        /// <see langword="false"/> if both instances are the same.
        /// </returns>
        public static bool operator !=(DatasetUpdateHistoryDescriptor left, DatasetUpdateHistoryDescriptor right) => !left.Equals(right);
    }
}
