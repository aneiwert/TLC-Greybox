using System;
using System.Collections.Generic;
using System.Linq;

namespace Unity.AssetManager.Core.Editor
{
    class StatusFlowIdentifier
    {
        public string StatusFlowId { get; }
        public string OrganizationId { get; }

        public StatusFlowIdentifier(string statusFlowId, string organizationId)
        {
            StatusFlowId = statusFlowId ?? throw new ArgumentNullException(nameof(statusFlowId));
            OrganizationId = organizationId ?? throw new ArgumentNullException(nameof(organizationId));
        }
    }

    class Status
    {
        public string Id { get; }
        public string Name { get; }

        public Status(string id, string name)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }

    class StatusTransition
    {
        public string FromStatusId { get; }
        public string ToStatusId { get; }

        public StatusTransition(string fromStatusId, string toStatusId)
        {
            FromStatusId = fromStatusId ?? throw new ArgumentNullException(nameof(fromStatusId));
            ToStatusId = toStatusId ?? throw new ArgumentNullException(nameof(toStatusId));
        }
    }

    class StatusFlowInfo
    {
        public IEnumerable<string> StatusNames { get; }
        public string StartStatusName { get; }
        public string FlowId { get; }
        public string StatusFlowName { get; }

        IEnumerable<Status> m_Statuses;
        Dictionary<string, Status> m_StatusesById;
        ILookup<string, StatusTransition> m_TransitionsByFromId;

        Dictionary<(string from, string to), string[]> m_TransitionPathCache = new();

        public StatusFlowInfo(string flowId, string statusFlowName, string startStatusId, IEnumerable<Status> statuses, IEnumerable<StatusTransition> statusTransitions)
        {
            if (string.IsNullOrEmpty(flowId))
                throw new ArgumentException("Flow ID cannot be null or empty", nameof(flowId));

            FlowId = flowId;
            StatusFlowName = statusFlowName;
            m_Statuses = statuses ?? Enumerable.Empty<Status>();

            m_StatusesById = m_Statuses.ToDictionary(s => s.Id);
            m_TransitionsByFromId = (statusTransitions ?? Enumerable.Empty<StatusTransition>()).ToLookup(t => t.FromStatusId);

            StatusNames = m_Statuses.Select(s => s.Name).ToList();
            StartStatusName = m_Statuses.FirstOrDefault(s => s.Id == startStatusId)?.Name;
        }

        public string[] GetTransitionPath(string fromStatusName, string toStatusName)
        {
            if (m_Statuses == null || m_TransitionsByFromId == null || string.IsNullOrEmpty(fromStatusName) || string.IsNullOrEmpty(toStatusName))
                return null;

            var key = (fromStatusName, toStatusName);
            if (m_TransitionPathCache.TryGetValue(key, out var cachedPath))
                return cachedPath;

            var fromStatus = m_Statuses.FirstOrDefault(s => s.Name == fromStatusName);
            var toStatus = m_Statuses.FirstOrDefault(s => s.Name == toStatusName);
            if (fromStatus == null || toStatus == null)
                return null;

            if (fromStatus.Id == toStatus.Id)
            {
                var result = new[] { fromStatus.Name };
                m_TransitionPathCache[key] = result;
                return result;
            }

            var visited = new HashSet<string>();
            var queue = new Queue<List<Status>>();
            queue.Enqueue(new List<Status> { fromStatus });
            visited.Add(fromStatus.Id);

            while (queue.Count > 0)
            {
                var path = queue.Dequeue();
                var currentStatus = path.Last();

                var transitions = m_TransitionsByFromId[currentStatus.Id];
                foreach (var transition in transitions)
                {
                    if (!m_StatusesById.TryGetValue(transition.ToStatusId, out var nextStatus) || visited.Contains(nextStatus.Id))
                        continue;

                    var newPath = new List<Status>(path) { nextStatus };
                    if (nextStatus.Id == toStatus.Id)
                    {
                        var result = newPath.Select(s => s.Name).ToArray();
                        m_TransitionPathCache[key] = result;
                        return result;
                    }

                    queue.Enqueue(newPath);
                    visited.Add(nextStatus.Id);
                }
            }

            m_TransitionPathCache[key] = null;
            return null;
        }
    }
}

