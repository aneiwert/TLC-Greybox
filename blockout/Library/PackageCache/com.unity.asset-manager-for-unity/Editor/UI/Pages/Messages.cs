using Unity.AssetManager.Core.Editor;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.AssetManager.UI.Editor
{
    static class Messages
    {
        internal static readonly Message EmptyMessage = new(string.Empty,
            RecommendedAction.Retry);

        internal static readonly Message NoOrganizationMessage = new (
            L10n.Tr("It seems your project is not linked to an organization. Please link your project to a Unity project ID to start using the Asset Manager service."),
            RecommendedAction.OpenServicesSettingButton);

        internal static readonly Message ErrorRetrievingOrganizationMessage = new(
            L10n.Tr("It seems there was an error while trying to retrieve organization info."),
            RecommendedAction.Retry);

        internal static readonly Message CurrentProjectNotEnabledMessage = new (
            L10n.Tr("It seems your current project is not enabled for use in the Asset Manager."),
            RecommendedAction.EnableProject);

        internal static readonly HelpBoxMessage NoConnectionMessage = new(
            L10n.Tr("No network connection. Please check your internet connection."),
            RecommendedAction.None, 0);

        internal static readonly Message EmptyAllAssetsMessage = new(
            L10n.Tr(Constants.EmptyAllAssetsText),
            RecommendedAction.OpenAssetManagerDashboardLink);

        internal static readonly Message MissingSelectedProjectMessage = new(
            L10n.Tr("Select the destination cloud project for the upload."));

        internal static readonly Message ErrorRetrievingAssetsMessage = new(
            L10n.Tr("It seems there was an error while trying to retrieve assets."),
            RecommendedAction.Retry);
    }
}
