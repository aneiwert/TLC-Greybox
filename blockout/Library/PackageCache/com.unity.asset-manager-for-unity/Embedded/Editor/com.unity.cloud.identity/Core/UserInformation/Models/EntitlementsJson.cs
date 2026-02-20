
using System.Collections.Generic;

namespace Unity.Cloud.IdentityEmbedded
{
    internal class EntitlementsJson
    {
        public IEnumerable<string> Entitlements { get; set; }

        public IEnumerable<string> UserSeats { get; set; }
    }
}
