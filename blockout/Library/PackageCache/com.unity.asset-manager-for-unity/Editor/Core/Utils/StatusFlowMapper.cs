using System.Collections.Generic;
using System.Linq;
using Unity.Cloud.AssetsEmbedded;

namespace Unity.AssetManager.Core.Editor
{
    static class StatusFlowMapper
    {
        internal static Status From(IStatus externalStatus)
        {
            if (externalStatus == null)
                return null;

            return new Status(externalStatus.Id, externalStatus.Name);
        }

        internal static StatusTransition From(IStatusTransition externalTransition)
        {
            if (externalTransition == null)
                return null;

            return new StatusTransition(externalTransition.FromStatusId, externalTransition.ToStatusId);
        }

        internal static StatusFlowInfo From(
            IStatusFlow externalStatusFlow,
            IEnumerable<IStatus> externalStatuses,
            IEnumerable<IStatusTransition> externalTransitions)
        {
            if (externalStatusFlow == null)
                return null;

            var flowId = externalStatusFlow.Descriptor.StatusFlowId;
            var statusFlowName = externalStatusFlow.Name;
            var startStatusId = externalStatusFlow.StartStatusId;

            var internalStatuses = externalStatuses?.Select(From).Where(s => s != null).ToList()
                ?? new List<Status>();

            var internalTransitions = externalTransitions?.Select(From).Where(t => t != null).ToList()
                ?? new List<StatusTransition>();

            return new StatusFlowInfo(flowId, statusFlowName, startStatusId, internalStatuses, internalTransitions);
        }
    }
}

