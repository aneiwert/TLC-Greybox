using System.Threading;
using System.Threading.Tasks;

namespace Unity.Cloud.IdentityEmbedded
{
    internal interface IEntitlementsJsonProvider
    {
        public Task<EntitlementsJson> GetEntitlementsJsonAsync(CancellationToken cancellationToken);
    }
}

