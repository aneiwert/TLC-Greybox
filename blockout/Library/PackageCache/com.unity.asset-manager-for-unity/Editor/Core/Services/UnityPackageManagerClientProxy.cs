using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace Unity.AssetManager.Core.Editor
{
    /// <summary>
    /// Interface for PackageManager operations to enable mocking in tests
    /// </summary>
    interface IPackageManagerClientProxy
    {
        ISearchRequest Search(string packageName);
        IListRequest List();
        IAddRequest Add(string packageId);
    }

    /// <summary>
    /// Concrete implementation using Unity's PackageManager.Client
    /// </summary>
    class UnityPackageManagerClientProxy : IPackageManagerClientProxy
    {
        public ISearchRequest Search(string packageName) => new UnitySearchRequest(Client.Search(packageName));
        public IListRequest List() => new UnityListRequest(Client.List());
        public IAddRequest Add(string packageId) => new UnityAddRequest(Client.Add(packageId));
    }

    /// <summary>
    /// Custom package info class
    /// </summary>
    class PackageInfo
    {
        public string name { get; set; }
        public string version { get; set; }
        public VersionsInfo versions { get; set; }

        public PackageInfo()
        {
            versions = new VersionsInfo();
        }

        public PackageInfo(string name, string version, string[] compatibleVersions = null)
        {
            this.name = name;
            this.version = version;
            this.versions = new VersionsInfo(compatibleVersions);
        }
    }

    /// <summary>
    /// Custom versions info class
    /// </summary>
    class VersionsInfo
    {
        public string[] compatible { get; set; }

        public VersionsInfo()
        {
            compatible = Array.Empty<string>();
        }

        public VersionsInfo(string[] compatibleVersions)
        {
            compatible = compatibleVersions ?? Array.Empty<string>();
        }
    }

    /// <summary>
    /// Custom error class
    /// </summary>
    class Error
    {
        public string message { get; set; }

        public Error()
        {
            message = string.Empty;
        }

        public Error(string errorMessage)
        {
            message = errorMessage ?? string.Empty;
        }
    }

    // Interfaces for PackageManager request types to enable mocking in tests

    interface ISearchRequest
    {
        PackageInfo[] Result { get; }
        StatusCode Status { get; }
        bool IsCompleted { get; }
    }

    interface IListRequest
    {
        IList<PackageInfo> Result { get; }
        StatusCode Status { get; }
        bool IsCompleted { get; }
    }

    interface IAddRequest
    {
        bool IsCompleted { get; }
        Error Error { get; }
    }

    // Wrapper classes for Unity's PackageManager request types
    class UnitySearchRequest : ISearchRequest
    {
        private readonly SearchRequest m_Request;
        public UnitySearchRequest(SearchRequest request) => m_Request = request;
        public PackageInfo[] Result => PackageInfoConverter.ConvertToCustomPackageInfo(m_Request.Result);
        public StatusCode Status => m_Request.Status;
        public bool IsCompleted => m_Request.IsCompleted;
    }

    class UnityListRequest : IListRequest
    {
        private readonly ListRequest m_Request;
        public UnityListRequest(ListRequest request) => m_Request = request;
        public IList<PackageInfo> Result => PackageInfoConverter.ConvertToCustomPackageInfo(m_Request.Result.ToList());
        public StatusCode Status => m_Request.Status;
        public bool IsCompleted => m_Request.IsCompleted;
    }

    class UnityAddRequest : IAddRequest
    {
        private readonly AddRequest m_Request;
        public UnityAddRequest(AddRequest request) => m_Request = request;
        public bool IsCompleted => m_Request.IsCompleted;
        public Error Error => m_Request.Error != null ? new Error(m_Request.Error.message) : null;
    }

    // Helper methods to convert Unity PackageInfo to custom types
    static class PackageInfoConverter
    {
        public static PackageInfo[] ConvertToCustomPackageInfo(UnityEditor.PackageManager.PackageInfo[] packageInfos)
        {
            if (packageInfos == null) return Array.Empty<PackageInfo>();

            var result = new PackageInfo[packageInfos.Length];
            for (int i = 0; i < packageInfos.Length; i++)
            {
                result[i] = ConvertToCustomPackageInfo(packageInfos[i]);
            }
            return result;
        }

        public static IList<PackageInfo> ConvertToCustomPackageInfo(IList<UnityEditor.PackageManager.PackageInfo> packageInfos)
        {
            if (packageInfos == null) return new List<PackageInfo>();

            var result = new List<PackageInfo>();
            foreach (var packageInfo in packageInfos)
            {
                result.Add(ConvertToCustomPackageInfo(packageInfo));
            }
            return result;
        }

        public static PackageInfo ConvertToCustomPackageInfo(UnityEditor.PackageManager.PackageInfo packageInfo)
        {
            if (packageInfo == null) return null;

            return new PackageInfo(
                packageInfo.name,
                packageInfo.version,
                packageInfo.versions?.compatible
            );
        }
    }
}
