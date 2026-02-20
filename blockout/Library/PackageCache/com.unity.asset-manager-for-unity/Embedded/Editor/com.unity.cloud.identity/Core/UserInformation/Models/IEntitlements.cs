using System.Collections.Generic;

namespace Unity.Cloud.IdentityEmbedded
{
    /// <summary>
    /// An interface that provides entitlements information for an <see cref="Organization"/>.
    /// </summary>
    interface IEntitlements
    {
        /// <summary>
        /// The organizations entitlements.
        /// </summary>
        public IEnumerable<string> OrganizationEntitlements { get; }

        /// <summary>
        /// The user seats in the organization.
        /// </summary>
        /// <remarks>User seats correspond to organization entitlements assigned to the user.</remarks>
        public IEnumerable<string> UserSeats { get; }
    }
}
