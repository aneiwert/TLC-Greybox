using System.Collections.Generic;
using System.Linq;

namespace Unity.Cloud.IdentityEmbedded
{
    /// <summary>
    /// A class that provides entitlements information for an <see cref="Organization"/>.
    /// </summary>
    internal class Entitlements : IEntitlements
    {
        /// <inheritdoc/>
        public IEnumerable<string> OrganizationEntitlements { get; }

        /// <inheritdoc/>
        public IEnumerable<string> UserSeats { get; }

        internal Entitlements(EntitlementsJson entitlementsJson)
        {
            // Clean duplicate entries
            OrganizationEntitlements = entitlementsJson.Entitlements.Distinct();
            // Only return seats that are also in OrganizationEntitlements
            UserSeats = entitlementsJson.UserSeats.Distinct().Where(x => OrganizationEntitlements.Contains(x));
        }
    }
}
