using System;

namespace Unity.Cloud.AssetsEmbedded
{
    /// <summary>
    /// This struct holds information about an asset library job's identifier.
    /// </summary>
    [Serializable]
struct AssetLibraryJobId
    {
        string m_String;

        /// <summary>
        /// Return the value of an identifier representing an invalid library id
        /// </summary>
        public static readonly AssetLibraryJobId None = new(Guid.Empty);

        /// <summary>
        /// Returns a <see cref="AssetLibraryJobId"/> using a <see cref="string"/>.
        /// </summary>
        /// <param name="value">The string representing the library identifier</param>
        public AssetLibraryJobId(string value) => m_String = value;

        /// <summary>
        /// Returns a <see cref="AssetLibraryJobId"/> using a <see cref="Guid"/>.
        /// </summary>
        /// <param name="value">The guid representing the library identifier</param>
        public AssetLibraryJobId(Guid value) => m_String = value.ToString();

        /// <summary>
        /// Returns whether two <see cref="AssetLibraryJobId"/> objects are equals.
        /// </summary>
        /// <param name="other">Another <see cref="AssetLibraryJobId"/></param>
        /// <returns>
        /// <see langword="true"/> if both instance have the same values;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool Equals(AssetLibraryJobId other) => m_String == other.m_String;

        /// <summary>
        /// Validate <paramref name="obj"/> is a <see cref="AssetLibraryJobId"/> instance and have the same values as this instance.
        /// </summary>
        /// <param name="obj">Compare the values with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instance have the same values;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public override bool Equals(object obj) => obj is AssetLibraryJobId other && Equals(other);

        /// <summary>
        /// Compute a hash code for the object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        /// <remarks>
        /// * You should not assume that equal hash codes imply object equality.
        /// * You should never persist or use a hash code outside the application domain in which it was created,
        ///   because the same object may hash differently across application domains, processes, and platforms.
        /// </remarks>
        public override int GetHashCode() => m_String != null ? m_String.GetHashCode() : 0;

        /// <summary>
        /// Get the string representation of this <see cref="AssetLibraryJobId"/>.
        /// </summary>
        /// <returns>The string result.</returns>
        public override string ToString() => m_String;

        /// <summary>
        /// Get if two <see cref="AssetLibraryJobId"/> represent the same.
        /// </summary>
        /// <param name="left">Compare with this first instance.</param>
        /// <param name="right">Compare with this other instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instances represent the same;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator ==(AssetLibraryJobId left, AssetLibraryJobId right) => left.Equals(right);

        /// <summary>
        /// Get if two <see cref="AssetLibraryJobId"/> does not represent the same.
        /// </summary>
        /// <param name="left">Compare with this first instance.</param>
        /// <param name="right">Compare with this other instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instances are not the same;
        /// <see langword="false"/> if both instances are the same.
        /// </returns>
        public static bool operator !=(AssetLibraryJobId left, AssetLibraryJobId right) => !left.Equals(right);

        /// <summary>
        /// Explicitly cast a <see cref="AssetLibraryJobId"/> to a <see cref="string"/>>
        /// </summary>
        /// <param name="pId">Object to cast</param>
        /// <returns>The resulting <see cref="string"/></returns>
        public static explicit operator string(AssetLibraryJobId pId) => pId.m_String;
    }
}
