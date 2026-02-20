using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.AssetManager.Core.Editor
{
    static class ProjectOrganizationProviderExtensions
    {
        public static async Task<StatusFlowInfo> GetStatusFlowInfoAsync(this OrganizationInfo organizationInfo,
            BaseAssetData assetData, CancellationToken cancellationToken = default)
        {
            if (organizationInfo == null)
                throw new ArgumentNullException(nameof(organizationInfo));

            if (assetData == null)
                throw new ArgumentNullException(nameof(assetData));

            if (string.IsNullOrEmpty(assetData.StatusFlowId))
                return await organizationInfo.GetLegacyStatusFlowInfoAsync(cancellationToken: cancellationToken);

            return await organizationInfo.GetStatusFlowInfoAsync(assetData.StatusFlowId, cancellationToken);
        }

        public static async Task<StatusFlowInfo> GetLegacyStatusFlowInfoAsync(this OrganizationInfo organizationInfo,
            CancellationToken cancellationToken = default)
        {
            if (organizationInfo == null)
                throw new ArgumentNullException(nameof(organizationInfo));

            var statusFlowInfos = await organizationInfo.GetStatusFlowInfosAsync(cancellationToken: cancellationToken);
            var legacyFlow = statusFlowInfos?.FirstOrDefault(s => s.StatusFlowName.Equals("Legacy", StringComparison.OrdinalIgnoreCase));

            if (legacyFlow == null)
                return await organizationInfo.GetDefaultStatusFlowInfoAsync(cancellationToken: cancellationToken);

            return legacyFlow;
        }
    }
}
