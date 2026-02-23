using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor.PackageManager;

namespace Unity.AssetManager.Core.Editor
{
    /// <summary>
    /// Service for managing package versions via Unity's Package Manager
    /// </summary>
    interface IPackageVersionService : IService
    {
        /// <summary>
        /// Installed version of the package, or null if not fetched
        /// </summary>
        string InstalledVersion { get; }
        /// <summary>
        /// Latest available version of the package, or null if not fetched
        /// </summary>
        string LatestVersion { get; }
        /// <summary>
        /// Fetches the installed and latest versions of the package
        /// </summary>
        /// <returns></returns>
        Task RefreshAsync();
        /// <summary>
        /// Installs the specified version of the package
        /// <param name="version">Semver version string to install.</param>
        /// <exception cref="ArgumentException">Thrown when the version is null or empty</exception>
        /// <exception cref="InvalidOperationException">Thrown when the installation fails</exception>
        /// </summary>
        Task InstallVersionAsync(string version);
    }

    [Serializable]
    class PackageVersionService : BaseService<IPackageVersionService>, IPackageVersionService
    {
        const string k_PackageName = "com.unity.asset-manager-for-unity";

        readonly IPackageManagerClientProxy m_PackageManagerClient;

        /// <summary>
        /// Installed version of the package, or null if not installed
        /// </summary>
        public string InstalledVersion { get; private set; }

        /// <summary>
        /// Latest available version of the package, or null if not found
        /// </summary>
        public string LatestVersion { get; private set; }

        public PackageVersionService() : this(new UnityPackageManagerClientProxy()) { }

        internal PackageVersionService(IPackageManagerClientProxy packageManagerClient)
        {
            m_PackageManagerClient = packageManagerClient;
        }

        /// <summary>
        /// Fetches the installed and latest versions of the package.
        /// </summary>
        public async Task RefreshAsync()
        {
            var remotePackageInfo = await GetRemotePackageInfo(k_PackageName);
            Utilities.DevLog($"Remote package info for {k_PackageName}: {(remotePackageInfo != null ? "found" : "not found")}");
            Utilities.DevLog($"Versions: {string.Join(", ", remotePackageInfo?.versions?.compatible ?? Array.Empty<string>())}");
            LatestVersion = GetLatestReleaseVersion(remotePackageInfo);
            Utilities.DevLog($"Latest version for {k_PackageName}: {LatestVersion ?? "none"}");

            var local = await GetLocalPackageInfo(k_PackageName);
            InstalledVersion = local?.version;
        }

        /// <summary>
        /// Installs the specified version of the package.
        /// </summary>
        /// <param name="version">Semver version string to install.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task InstallVersionAsync(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                throw new ArgumentException("Version cannot be null or empty", nameof(version));
            }

            var addRequest = m_PackageManagerClient.Add($"{k_PackageName}@{version}");
            while (!addRequest.IsCompleted)
                await Task.Yield();

            if (addRequest.Error != null)
            {
                var errorMessage = $"Failed to install package {k_PackageName} version {version}: {addRequest.Error.message}";
                throw new InvalidOperationException(errorMessage);
            }

            // Refresh the installed version after successful installation
            await RefreshAsync();
        }

        /// <summary>
        /// Fetches package info from the remote registry
        /// </summary>
        /// <param name="packageName">Name of the package to search for, e.g. "com.unity.asset-manager-for-unity" </param>
        /// <returns>Found CustomPackageInfo or null if not found</returns>
        async Task<PackageInfo> GetRemotePackageInfo(string packageName)
        {
            var searchRequest = m_PackageManagerClient.Search(packageName);
            while (!searchRequest.IsCompleted)
                await Task.Yield();
            return searchRequest.Status == StatusCode.Success ? searchRequest.Result.FirstOrDefault() : null;
        }

        /// <summary>
        /// Fetches package info from the locally installed packages
        /// </summary>
        /// <param name="packageName">Name of the package to search for, e.g. "com.unity.asset-manager-for-unity" </param>
        /// <returns>Found CustomPackageInfo or null if not found</returns>
        async Task<PackageInfo> GetLocalPackageInfo(string packageName)
        {
            var listRequest = m_PackageManagerClient.List();
            while (!listRequest.IsCompleted)
                await Task.Yield();

            var packageInfos = listRequest.Status == StatusCode.Success ? listRequest.Result : null;
            return packageInfos?.FirstOrDefault(x => x.name == packageName);
        }

        /// <summary>
        /// Returns the latest release version (without pre-release identifiers) from the package info
        /// </summary>
        /// <param name="packageInfo">CustomPackageInfo object containing available versions</param>
        /// <returns>Semver version string or null if not found</returns>
        static string GetLatestReleaseVersion(PackageInfo packageInfo)
        {
            if (packageInfo == null || packageInfo.versions.compatible.Length == 0)
                return null;

            var releaseVersions = new List<string>();

            // First try to find a release version (without pre-release identifiers)
            foreach (var package in packageInfo.versions.compatible)
            {
                if (SemanticVersion.TryParse(package, out var version) && version.IsRelease)
                {
                    releaseVersions.Add(package);
                }
            }

            if (releaseVersions.Count <= 0)
            {
                return packageInfo.versions.compatible.FirstOrDefault();
            }
            releaseVersions.Sort();
            return releaseVersions.Last();
        }
    }
}


