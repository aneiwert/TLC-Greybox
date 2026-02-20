using System;
using Unity.AssetManager.Core.Editor;
using UnityEngine.Analytics;

namespace Unity.AssetManager.UI.Editor
{
    class NewVersionNotificationEvent : IBaseEvent
    {
        const string k_UnknownValue = "Unknown";

        [Serializable]
#if UNITY_2023_2_OR_NEWER
        internal class NewVersionNotificationEventData : IAnalytic.IData
#else
        internal class NewVersionNotificationEventData : BaseEventData
#endif
        {
            /// <summary>
            /// The current version of the asset-manager-for-unity package
            /// </summary>
            public string CurrentVersion;

            /// <summary>
            /// The latest version of the asset-manager-for-unity package
            /// </summary>
            public string LatestVersion;

            /// <summary>
            /// The action
            /// </summary>
            public string Action;
        }

        internal const string k_EventName = AnalyticsSender.EventPrefix + "NewVersionNotification";
        internal const int k_EventVersion = 1;

        public string EventName => k_EventName;
        public int EventVersion => k_EventVersion;

        NewVersionNotificationEventData m_Data;

        internal NewVersionNotificationEvent(string currentVersion, string latestVersion, string action)
        {
            m_Data = new NewVersionNotificationEventData
            {
                CurrentVersion = currentVersion ?? k_UnknownValue,
                LatestVersion = latestVersion ?? k_UnknownValue,
                Action = action ?? k_UnknownValue
            };
        }

#if UNITY_2023_2_OR_NEWER
        [AnalyticInfo(eventName:k_EventName, vendorKey:AnalyticsSender.VendorKey, version:k_EventVersion, maxEventsPerHour:1000, maxNumberOfElements:1000)]
        internal class NewVersionNotificationEventAnalytic : IAnalytic
        {
            NewVersionNotificationEventData m_Data;

            public NewVersionNotificationEventAnalytic(NewVersionNotificationEventData data)
            {
                m_Data = data;
            }

            public bool TryGatherData(out IAnalytic.IData data, out Exception error)
            {
                error = null;
                data = m_Data;
                return data != null;
            }
        }

        public IAnalytic GetAnalytic()
        {
            return new NewVersionNotificationEventAnalytic(m_Data);
        }
#else
        public BaseEventData EventData => m_Data;
#endif
    }
}

