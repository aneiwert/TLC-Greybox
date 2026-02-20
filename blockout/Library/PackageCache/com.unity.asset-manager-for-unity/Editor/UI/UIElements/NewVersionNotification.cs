using System;
using System.Globalization;
using System.Threading.Tasks;
using Unity.AssetManager.Core.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AssetManager.UI.Editor
{
    class NewVersionNotification : VisualElement
    {
        // Struct to hold both the input version string and the parsed SemanticVersion
        struct TrackedVersion
        {
            public TrackedVersion(string inputVersion)
            {
                Input = inputVersion;
                Parsed = SemanticVersion.TryParse(Input, out var version) ? version : null;
            }

            public readonly string Input; // the version string as given by the PackageVersionService
            public readonly SemanticVersion? Parsed; // cached parsed version
        }

        readonly IPackageVersionService m_PackageVersionService;
        readonly IUnityConnectProxy m_UnityConnectProxy;
        readonly HelpBox m_NotificationHelpBox;

        const int k_DaysBetweenReminders = 4;

        // Notification messages
        const string k_NotificationMessage =
            "<b>New version available</b><br>You're using an outdated version of the Asset Manager for Unity package. Update to the latest version to improve stability and access new features.";

        private const string k_NotificationMessageTemplate =
            "<b>New version available</b><br>You're using an outdated version ({0}) of the Asset Manager for Unity package. Update to the latest version ({1}) to improve stability and access new features.";

        // Only one of these keys will be set at a time.
        const string k_EditorPrefSkipVersionKey = "AssetManagerForUnity_SkippedVersion";
        const string k_EditorPrefLastRemindTimeKey = "AssetManagerForUnity_LastRemindTime";

        // Session state (survive domain reloads). We use this session state to avoid sending multiple analytics events for the banner presentation in the same session.
        // Will reset on editor restart and if a user clicks an action (remind me later, skip version, update).
        const string k_SessionStateKey_NewVersionBannerPresented_AnalyticSent = "AssetManagerForUnity_NewVersionBannerPresented_AnalyticSent";

        // Action choices
        const string k_ActionRemindMeLater = "Remind Me Later";
        const string k_ActionSkipVersion = "Skip Version";
        const string k_ActionUpdate = "Update";
        const string k_ActionBannerPresented = "Banner Presented"; // not an actual action, just for analytics

        // Tooltip
        const string k_ActionUpdateTooltip = "Update to the latest version of the Asset Manager for Unity package.";
        const string k_MoreOptionsTooltip = "More options";

        // Cached versions
        TrackedVersion m_LatestVersion;
        TrackedVersion m_InstalledVersion;
        TrackedVersion m_SkippedVersion;

        // CheckForNewVersion task
        Task m_CheckForNewVersionTask; // Used to ensure we don't run many in parallel

        string SkippedVersionPrefs
        {
            get => EditorPrefs.GetString(k_EditorPrefSkipVersionKey, string.Empty);
            set
            {
                // When setting one, clear the other to ensure only one is set at a time.
                EditorPrefs.SetString(k_EditorPrefSkipVersionKey, value);
                EditorPrefs.DeleteKey(k_EditorPrefLastRemindTimeKey);
            }
        }

        DateTime LastRemindTimePrefs
        {
            get
            {
                var datetimeString = EditorPrefs.GetString(k_EditorPrefLastRemindTimeKey, string.Empty);
                if (string.IsNullOrEmpty(datetimeString))
                    return DateTime.MinValue;

                try
                {
                    var datetime = DateTime.Parse(datetimeString, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.RoundtripKind);
                    return datetime;
                }
                catch(FormatException)
                {
                    EditorPrefs.DeleteKey(k_EditorPrefLastRemindTimeKey);
                    return DateTime.MinValue;
                }
                catch(ArgumentNullException)
                {
                    EditorPrefs.DeleteKey(k_EditorPrefLastRemindTimeKey);
                    return DateTime.MinValue;
                }
                catch(ArgumentException)
                {
                    EditorPrefs.DeleteKey(k_EditorPrefLastRemindTimeKey);
                    return DateTime.MinValue;
                }
                catch(OverflowException)
                {
                    EditorPrefs.DeleteKey(k_EditorPrefLastRemindTimeKey);
                    return DateTime.MinValue;
                }
            }
            set
            {
                // When setting one, clear the other to ensure only one is set at a time.
                EditorPrefs.SetString(k_EditorPrefLastRemindTimeKey, value.ToString("o"));
                EditorPrefs.DeleteKey(k_EditorPrefSkipVersionKey);
            }
        }

        public NewVersionNotification(IPackageVersionService packageVersionService, IUnityConnectProxy unityConnectProxy)
        {
            m_PackageVersionService = packageVersionService;
            m_UnityConnectProxy = unityConnectProxy;

            AddToClassList("unity-new-version-notification");

            // Create help box
            m_NotificationHelpBox = new ();
            m_NotificationHelpBox.messageType = HelpBoxMessageType.Info;

            // Create the button container
            var buttonContainer = new VisualElement();
            buttonContainer.AddToClassList("unity-new-version-notification-buttons");

            // Create the update button
            var updateButton = new Button(OnUpdateClicked) {text = k_ActionUpdate, tooltip = k_ActionUpdateTooltip};
            updateButton.AddToClassList("unity-new-version-notification-button");
            buttonContainer.Add(updateButton);

            // Create the more options dropdown
            var moreOptionsDropdown = new DropdownField()
            {
                focusable = false,
                tooltip = k_MoreOptionsTooltip,
            };
            moreOptionsDropdown.choices.Add(k_ActionRemindMeLater);
            moreOptionsDropdown.choices.Add(k_ActionSkipVersion);

            moreOptionsDropdown.RegisterValueChangedCallback(evt =>
            {
                evt.StopPropagation();
                switch (evt.newValue)
                {
                    case k_ActionRemindMeLater:
                        OnRemindMeLaterClicked();
                        break;
                    case k_ActionSkipVersion:
                        OnSkipVersionClicked();
                        break;
                    default:
                        Utilities.DevLogError($"Unknown action selected: {evt.newValue}");
                        break;
                }
            });

            moreOptionsDropdown.AddToClassList("unity-new-version-notification-dropdown");

            // Hide the label part of the dropdown, we only want the arrow button
            UIElementsUtils.Hide(moreOptionsDropdown.Q(null, "unity-base-popup-field__text"));
            buttonContainer.Add(moreOptionsDropdown);

            m_NotificationHelpBox.Add(buttonContainer);
            Add(m_NotificationHelpBox);

            // Start hidden
            Hide();

            // Trigger a version check, not awaited on purpose
            CheckForNewVersion();

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        void OnAttachToPanel(AttachToPanelEvent evt)
        {
            m_UnityConnectProxy.CloudServicesReachabilityChanged += OnCloudServicesReachabilityChanged;
        }

        void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            m_UnityConnectProxy.CloudServicesReachabilityChanged -= OnCloudServicesReachabilityChanged;
        }

        void OnCloudServicesReachabilityChanged(bool areReachable)
        {
            CheckForNewVersion();
        }

        void CheckForNewVersion()
        {
            if (m_CheckForNewVersionTask != null && !m_CheckForNewVersionTask.IsCompleted)
                return;

            m_CheckForNewVersionTask = CheckForNewVersionInternal();
            async Task CheckForNewVersionInternal()
            {
                if (m_PackageVersionService.InstalledVersion == null || m_PackageVersionService.LatestVersion == null)
                {
                    await m_PackageVersionService.RefreshAsync();
                }

                CacheValues();
                RefreshUI();
            }
        }

        void CacheValues()
        {
            m_LatestVersion = new TrackedVersion(m_PackageVersionService.LatestVersion);
            m_InstalledVersion = new TrackedVersion(m_PackageVersionService.InstalledVersion);
            m_SkippedVersion = new TrackedVersion(SkippedVersionPrefs);
        }

        void RefreshUI()
        {
            UpdateMessage();
            SetVisibility();

            // Send analytics if we are showing the banner
            if (IsVisible())
            {
                SendAnalytics(k_ActionBannerPresented);
            }
        }

        void UpdateMessage()
        {
            if (!m_LatestVersion.Parsed.HasValue || !m_InstalledVersion.Parsed.HasValue)
            {
                // If we can't parse the latest or installed version, show a generic message.
                m_NotificationHelpBox.text = k_NotificationMessage;
                return;
            }

            m_NotificationHelpBox.text = string.Format(k_NotificationMessageTemplate, m_InstalledVersion.Parsed.Value.ToString(), m_LatestVersion.Parsed.Value.ToString());
        }

        void SetVisibility()
        {
            Hide(); // By default, hide the notification

            if (!m_UnityConnectProxy.AreCloudServicesReachable)
                return; // Simply hide if we are offline

            if (!m_LatestVersion.Parsed.HasValue || !m_InstalledVersion.Parsed.HasValue)
            {
                // If we can't parse the latest or installed version, just hide the notification.
                return;
            }

            if (m_InstalledVersion.Parsed.Value.CompareTo(m_LatestVersion.Parsed.Value) >= 0)
            {
                // If the installed version is greater than or equal to the latest version, hide the notification
                return;
            }

            // At this point, we know that installed < latest, so we should consider showing the notification
            Show();

            if (m_SkippedVersion.Parsed.HasValue)
            {
                if (m_SkippedVersion.Parsed.Value.CompareTo(m_LatestVersion.Parsed.Value) == 0)
                {
                    // If skipped exists and is equal to latest, hide the notification
                    Hide();
                    return;
                }
            }

            if (LastRemindTimePrefs != DateTime.MinValue)
            {
                var timeSinceLastRemind = DateTime.UtcNow - LastRemindTimePrefs;
                Utilities.DevLog("Time since last remind: " + timeSinceLastRemind);

                if (timeSinceLastRemind < TimeSpan.FromDays(k_DaysBetweenReminders))
                {
                    // If we already reminded the user in the last k_DaysBetweenReminders days, hide the notification
                    Hide();
                    return;
                }
            }
        }

        void SendAnalytics(string action)
        {
            if (action == null)
            {
                Utilities.DevLogError("Action cannot be null for analytics.");
                return;
            }

            if (action == k_ActionBannerPresented)
            {
                bool hasSentAnalyticThisSession = SessionState.GetBool(k_SessionStateKey_NewVersionBannerPresented_AnalyticSent, false);
                if (!hasSentAnalyticThisSession)
                {
                    SessionState.SetBool(k_SessionStateKey_NewVersionBannerPresented_AnalyticSent, true);
                    AnalyticsSender.SendEvent(new NewVersionNotificationEvent(m_InstalledVersion.Parsed?.ToString(),
                        m_LatestVersion.Parsed?.ToString(), action));
                }
            }
            else
            {
                AnalyticsSender.SendEvent(new NewVersionNotificationEvent(m_InstalledVersion.Parsed?.ToString(), m_LatestVersion.Parsed?.ToString(), action));
                SessionState.EraseBool(k_SessionStateKey_NewVersionBannerPresented_AnalyticSent);
            }
        }

        void OnRemindMeLaterClicked()
        {
            SendAnalytics(k_ActionRemindMeLater);

            LastRemindTimePrefs = DateTime.UtcNow;
            Hide();
        }

        void OnSkipVersionClicked()
        {
            string latestVersion = m_LatestVersion.Parsed?.ToString();
            if (!string.IsNullOrEmpty(latestVersion))
            {
                SendAnalytics(k_ActionSkipVersion);

                // Remember the skipped version so we don't show the notification again for this version
                SkippedVersionPrefs = latestVersion;
                Hide();
            }
            else
            {
                // Something went wrong, we couldn't get the latest version. Just behave like "Remind Me Later" to ensure the user isn't bothered again right away.
                Utilities.DevLogError("Could not determine latest version to skip. Revert to 'Remind Me Later' behavior.");
                OnRemindMeLaterClicked();
            }
        }

        void OnUpdateClicked()
        {
            try
            {
                var latestVersion = m_LatestVersion.Input;
                if (!string.IsNullOrEmpty(latestVersion))
                {
                    SendAnalytics(k_ActionUpdate);

                    // closing the window to prevent a UI bug (broken UI)
                    var window = EditorWindow.GetWindow<AssetManagerWindow>();
                    window.Close();

                    // no await intentionally since this will replace the current package
                    m_PackageVersionService.InstallVersionAsync(latestVersion);
                }
                else
                {
                    Debug.LogError("Could not determine latest version to install.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to update Asset Manager: {e.Message}");
            }
        }



        void Hide()
        {
            style.display = DisplayStyle.None;
        }

        void Show()
        {
            style.display = DisplayStyle.Flex;
        }

        bool IsVisible()
        {
            return style.display == DisplayStyle.Flex;
        }
    }
}
