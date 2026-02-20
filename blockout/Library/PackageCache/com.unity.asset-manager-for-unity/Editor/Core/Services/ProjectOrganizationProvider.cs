using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.AssetsEmbedded;
using Unity.Cloud.CommonEmbedded;
using Unity.Cloud.IdentityEmbedded;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace Unity.AssetManager.Core.Editor
{
    [Serializable]
    struct NameAndId
    {
        public string Name;
        public string Id;

        public NameAndId(string name, string id)
        {
            Name = name;
            Id = id;
        }
    }


    [Serializable]
    class OrganizationInfo
    {
        [SerializeReference]
        public List<IMetadataFieldDefinition> MetadataFieldDefinitions = new();

        public NameAndId nameAndId;

        public string Id
        {
            get => nameAndId.Id;
            set => nameAndId.Id = value;
        }

        public string Name
        {
            get => nameAndId.Name;
            set => nameAndId.Name = value;
        }

        public List<ProjectOrLibraryInfo> ProjectInfos = new();

        List<UserInfo> m_UserInfos;
        Task<List<UserInfo>> m_UserInfosTask;

        List<StatusFlowInfo> m_StatusFlowInfos;
        Task<List<StatusFlowInfo>> m_StatusFlowInfosTask;

        StatusFlowInfo m_DefaultStatusFlowInfo;
        Task<StatusFlowInfo> m_DefaultStatusFlowInfoTask;

        public async Task<string> GetUserNameAsync(string userId)
        {
            var userInfos = await GetUserInfosAsync();
            return userInfos.FirstOrDefault(u => u.UserId == userId)?.Name;
        }

        public async Task<string> GetUserIdAsync(string userName)
        {
            var userInfos = await GetUserInfosAsync();
            return userInfos.FirstOrDefault(u => u.Name == userName)?.UserId;
        }

        public async Task<List<UserInfo>> GetUserInfosAsync(Action<List<UserInfo>> callback = null, CancellationToken cancellationToken = default)
        {
            if (m_UserInfos != null && m_UserInfos.Count != 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                callback?.Invoke(m_UserInfos);
                return m_UserInfos;
            }

            m_UserInfosTask ??= GetUserInfosInternalAsync(cancellationToken);

            m_UserInfos = await m_UserInfosTask;
            m_UserInfosTask = null;

            callback?.Invoke(m_UserInfos);
            return m_UserInfos;
        }

        async Task<List<UserInfo>> GetUserInfosInternalAsync(CancellationToken cancellationToken)
        {
            var userInfos = new List<UserInfo>();
            await foreach (var userInfo in ServicesContainer.instance.Resolve<IProjectOrganizationProvider>()
                               .GetOrganizationUsersAsync(Id, Range.All, cancellationToken))
            {
                userInfos.Add(userInfo);
            }

            return userInfos;
        }

        public async Task<StatusFlowInfo> GetStatusFlowInfoAsync(string statusFlowId, CancellationToken cancellationToken = default)
        {
            var statusFlowInfos = await GetStatusFlowInfosAsync(cancellationToken: cancellationToken);
            return statusFlowInfos?.FirstOrDefault(s => s.FlowId.Equals(statusFlowId));
        }

        public async Task<List<StatusFlowInfo>> GetStatusFlowInfosAsync(Action<List<StatusFlowInfo>> callback = null, CancellationToken cancellationToken = default)
        {
            if (m_StatusFlowInfos != null && m_StatusFlowInfos.Count != 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                callback?.Invoke(m_StatusFlowInfos);
                return m_StatusFlowInfos;
            }

            m_StatusFlowInfosTask ??= GetStatusFlowInfosInternalAsync(cancellationToken);

            m_StatusFlowInfos = await m_StatusFlowInfosTask;
            m_StatusFlowInfosTask = null;

            callback?.Invoke(m_StatusFlowInfos);
            return m_StatusFlowInfos;
        }

        async Task<List<StatusFlowInfo>> GetStatusFlowInfosInternalAsync(CancellationToken cancellationToken)
        {
            var statusFlowInfos = new List<StatusFlowInfo>();
            await foreach (var statusFlowInfo in ServicesContainer.instance.Resolve<IProjectOrganizationProvider>()
                               .GetOrganizationStatusFlowsAsync(cancellationToken))
            {
                statusFlowInfos.Add(statusFlowInfo);
            }

            return statusFlowInfos;
        }

        public async Task<StatusFlowInfo> GetDefaultStatusFlowInfoAsync(Action<StatusFlowInfo> callback = null, CancellationToken cancellationToken = default)
        {
            if (m_DefaultStatusFlowInfo != null)
            {
                cancellationToken.ThrowIfCancellationRequested();

                callback?.Invoke(m_DefaultStatusFlowInfo);
                return m_DefaultStatusFlowInfo;
            }

            m_DefaultStatusFlowInfoTask ??= GetDefaultStatusFlowInfoInternalAsync(cancellationToken);

            m_DefaultStatusFlowInfo = await m_DefaultStatusFlowInfoTask;
            m_DefaultStatusFlowInfoTask = null;

            callback?.Invoke(m_DefaultStatusFlowInfo);
            return m_DefaultStatusFlowInfo;
        }

        async Task<StatusFlowInfo> GetDefaultStatusFlowInfoInternalAsync(CancellationToken cancellationToken)
        {
            return await ServicesContainer.instance.Resolve<IProjectOrganizationProvider>()
                .GetOrganizationDefaultStatusFlowAsync(cancellationToken);
        }
    }

    [Serializable]
    class ProjectOrLibraryInfo
    {
        [SerializeField]
        List<CollectionInfo> m_CollectionInfos;

        [SerializeField]
        bool m_IsAssetLibrary;

        public NameAndId nameAndId;

        public string Id => nameAndId.Id;

        public string Name => nameAndId.Name;

        public ProjectOrLibraryInfo(string id, string name = "", bool isAssetLibrary = false)
        {
            nameAndId.Id = id;
            nameAndId.Name = name;
            m_IsAssetLibrary = isAssetLibrary;
        }

        public IEnumerable<CollectionInfo> CollectionInfos => m_CollectionInfos;

        public void SetCollections(IEnumerable<CollectionInfo> collections)
        {
            m_CollectionInfos = collections?.ToList();
        }

        public CollectionInfo GetCollection(string collectionPath)
        {
            return m_CollectionInfos?.Find(c => c.GetFullPath() == collectionPath);
        }

        public bool IsAssetLibrary => m_IsAssetLibrary;

    }

    [Serializable]
    class UserInfo
    {
        public NameAndId nameAndId;

        public string UserId
        {
            get => nameAndId.Id;
            set => nameAndId.Id = value;
        }

        public string Name
        {
            get => nameAndId.Name;
            set => nameAndId.Name = value;
        }
    }

    interface IPreferencesStorage
    {
        string GetValue(string key, string fallbackValue = null);
        void SetValue(string key, string value);
    }

    class EditorPrefsStorage : IPreferencesStorage
    {
        public string GetValue(string key, string fallbackValue = null)
        {
            var projectSpecificKey = GenerateProjectSpecificKey(key);

            if (EditorPrefs.HasKey(projectSpecificKey))
            {
                var savedValue = EditorPrefs.GetString(projectSpecificKey);
                return string.IsNullOrWhiteSpace(savedValue) ? fallbackValue : savedValue;
            }

            // Migration: Check for old project-less key if project-specific key is not found
            if (EditorPrefs.HasKey(key))
            {
                var savedValue = EditorPrefs.GetString(key);
                if (!string.IsNullOrWhiteSpace(savedValue))
                {
                    // Migrate to new format and clean up old key
                    EditorPrefs.SetString(projectSpecificKey, savedValue);
                    EditorPrefs.DeleteKey(key);
                    return savedValue;
                }
            }

            return fallbackValue;
        }

        public void SetValue(string key, string value)
        {
            EditorPrefs.SetString(GenerateProjectSpecificKey(key), value);
        }

        static string GenerateProjectSpecificKey(string baseKey)
        {
            // Use the project path as a unique hash for this project
            var projectPath = Application.dataPath;
            var projectHash = projectPath.GetHashCode().ToString();
            return $"{baseKey}.{projectHash}";
        }
    }

    interface IProjectOrganizationProvider : IService
    {
        OrganizationInfo SelectedOrganization { get; }
        ProjectOrLibraryInfo SelectedProjectOrLibrary { get; }
        ProjectOrLibraryInfo SelectedAssetLibrary { get; }
        CollectionInfo SelectedCollection { get; }
        bool IsLoading { get; }
        event Action<OrganizationInfo> OrganizationChanged;
        event Action<bool> LoadingStateChanged;
        event Action<ProjectOrLibraryInfo, CollectionInfo> ProjectSelectionChanged;
        event Action<ProjectOrLibraryInfo> ProjectInfoChanged;
        event Action<List<ProjectOrLibraryInfo>> AssetLibrariesProjectsLoaded;

        IAsyncEnumerable<UserInfo> GetOrganizationUsersAsync(string organizationId, Range range, CancellationToken token);
        Task<StorageUsage> GetStorageUsageAsync(string organizationId, CancellationToken token = default);
        void SelectOrganization(string organizationId);
        void SelectProject(string projectId, string collectionPath = null, bool updateProject = false);
        void EnableProjectForAssetManager();
        ProjectOrLibraryInfo GetProject(string projectId);
        Task<Dictionary<string, string>> GetProjectIconUrlsAsync(string organizationId, CancellationToken token);
        Task CreateCollection(CollectionInfo collectionInfo);
        Task DeleteCollection(CollectionInfo collectionInfo);
        Task RenameCollection(CollectionInfo collectionInfo, string newName);
        IAsyncEnumerable<NameAndId> ListOrganizationsAsync();
        Task<List<string>> GetOrganizationVersionLabelsAsync();
        Task<List<ProjectOrLibraryInfo>> GetAssetLibrariesProjectsAsync();
        IAsyncEnumerable<StatusFlowInfo> GetOrganizationStatusFlowsAsync(CancellationToken token);
        Task<StatusFlowInfo> GetOrganizationDefaultStatusFlowAsync(CancellationToken token = default);
    }

    [Serializable]
    class ProjectOrganizationProvider : BaseSdkService, IProjectOrganizationProvider, ProjectOrganizationProvider.IDataMapper
    {
        public override Type RegistrationType => typeof(IProjectOrganizationProvider);

        AsyncLoadOperation m_LoadOrganizationOperation = new();

        [SerializeField]
        OrganizationInfo m_OrganizationInfo;

        [SerializeField]
        List<ProjectOrLibraryInfo> m_AssetLibrariesProjects;

        Task m_AssetLibrariesLoadTask;

        [SerializeField]
        string m_SelectedProjectId;

        [SerializeField]
        string m_CollectionPath;

        [SerializeReference]
        IMessageManager m_MessageManager;

        [SerializeReference]
        IUnityConnectProxy m_UnityConnectProxy;

        IPreferencesStorage m_PreferencesStorage;
        IPreferencesStorage PreferencesStorage
        {
            get
            {
                m_PreferencesStorage ??= new EditorPrefsStorage();
                return m_PreferencesStorage;
            }
        }

        internal static readonly string k_OrganizationPrefKey = "com.unity.asset-manager-for-unity.selectedOrganizationId";
        internal static readonly string k_ProjectPrefKey = "com.unity.asset-manager-for-unity.selectedProjectId";
        static readonly string k_CollectionPathPrefKey = "com.unity.asset-manager-for-unity.selectedCollectionPath";
        static readonly string k_DefaultCollectionDescription = "none";


        string SavedOrganizationId
        {
            set => PreferencesStorage.SetValue(k_OrganizationPrefKey, value);
            get
            {
                var savedId = PreferencesStorage.GetValue(k_OrganizationPrefKey);
                return string.IsNullOrWhiteSpace(savedId) ? m_UnityConnectProxy.OrganizationId : savedId;
            }
        }

        string SavedProjectId
        {
            set => PreferencesStorage.SetValue(k_ProjectPrefKey, value);
            get => PreferencesStorage.GetValue(k_ProjectPrefKey);
        }

        string SavedCollectionPath
        {
            set => PreferencesStorage.SetValue(k_CollectionPathPrefKey, value);
            get => PreferencesStorage.GetValue(k_CollectionPathPrefKey);
        }

        Dictionary<string, List<string>> m_VersionLabels = new();
        Task<List<string>> m_VersionLabelsTask;
        object m_VersionLabelsLock = new();

        public event Action<OrganizationInfo> OrganizationChanged;
        public event Action<ProjectOrLibraryInfo, CollectionInfo> ProjectSelectionChanged;
        public event Action<ProjectOrLibraryInfo> ProjectInfoChanged;
        public event Action<List<ProjectOrLibraryInfo>> AssetLibrariesProjectsLoaded;

        public bool IsLoading => m_LoadOrganizationOperation?.IsLoading ?? false;
        bool IsCloudReachable => m_UnityConnectProxy.AreCloudServicesReachable;
        bool IsLoggedIn => GetAuthenticationState() == Cloud.IdentityEmbedded.AuthenticationState.LoggedIn;

        public event Action<bool> LoadingStateChanged;

        public OrganizationInfo SelectedOrganization =>
            IsLoading || string.IsNullOrEmpty(m_OrganizationInfo?.Id) ? null : m_OrganizationInfo;

        public ProjectOrLibraryInfo SelectedProjectOrLibrary
        {
            get
            {
                return m_OrganizationInfo?.ProjectInfos?.Find(p => p.Id == m_SelectedProjectId) ?? SelectedAssetLibrary;

            }
        }

        public ProjectOrLibraryInfo SelectedAssetLibrary
        {
            get { return m_AssetLibrariesProjects?.Find(p => p.Id == m_SelectedProjectId); }
        }

        public CollectionInfo SelectedCollection
        {
            get
            {
                var collection = SelectedProjectOrLibrary?.GetCollection(m_CollectionPath);
                return collection ?? new CollectionInfo(SelectedOrganization?.Id, SelectedProjectOrLibrary?.Id, null);
            }
        }

        IDataMapper m_DataMapperOverride;

        IDataMapper DataMapper => m_DataMapperOverride ?? this;

        public ProjectOrganizationProvider() { }

        /// <inheritdoc />
        internal ProjectOrganizationProvider(SdkServiceOverride sdkServiceOverride)
            : base(sdkServiceOverride) { }

        [ServiceInjection]
        public void Inject(IUnityConnectProxy unityConnectProxy, IMessageManager messageManager)
        {
            m_UnityConnectProxy = unityConnectProxy;
            m_MessageManager = messageManager;
        }

        /// <summary>
        /// Sets parameter for testing
        /// </summary>
        internal ProjectOrganizationProvider With(IUnityConnectProxy unityConnectProxy)
        {
            m_UnityConnectProxy = unityConnectProxy;
            return this;
        }

        /// <summary>
        /// Sets parameter for testing
        /// </summary>
        internal ProjectOrganizationProvider With(IPreferencesStorage preferencesStorage)
        {
            m_PreferencesStorage = preferencesStorage;
            return this;
        }

        /// <summary>
        /// Sets parameter for testing
        /// </summary>
        internal ProjectOrganizationProvider With(IDataMapper dataMapperOverride)
        {
            m_DataMapperOverride = dataMapperOverride;
            return this;
        }

        /// <summary>
        /// Sets parameter for testing
        /// </summary>
        internal ProjectOrganizationProvider With(OrganizationInfo organizationInfo)
        {
            m_OrganizationInfo = organizationInfo;
            return this;
        }

        public override void OnEnable()
        {
            RegisterOnAuthenticationStateChanged(OnAuthenticationStateChanged);
            m_UnityConnectProxy.CloudServicesReachabilityChanged += OnCloudServicesReachabilityChanged;

            _ = TryLoadValidOrganizationAsync();
        }

        public override void OnDisable()
        {
            if (IsLoading)
            {
                m_LoadOrganizationOperation?.Cancel();
                m_LoadOrganizationOperation = null;
            }

            UnregisterOnAuthenticationStateChanged(OnAuthenticationStateChanged);
            m_UnityConnectProxy.CloudServicesReachabilityChanged -= OnCloudServicesReachabilityChanged;
        }

        void OnAuthenticationStateChanged()
        {
            // Cancel the current operation if the user is logged out
            if (IsLoading && GetAuthenticationState() == Unity.Cloud.IdentityEmbedded.AuthenticationState.LoggedOut)
            {
                m_LoadOrganizationOperation?.Cancel();
                m_OrganizationInfo = null;
            }

            _ = TryLoadValidOrganizationAsync();
            m_AssetLibrariesLoadTask = TryLoadAssetLibrariesAsync();
        }

        void OnCloudServicesReachabilityChanged(bool cloudServicesReachable)
        {
            _ = TryLoadValidOrganizationAsync();
            m_AssetLibrariesLoadTask = TryLoadAssetLibrariesAsync();
        }

        public async IAsyncEnumerable<UserInfo> GetOrganizationUsersAsync(string organizationId, Range range, [EnumeratorCancellation] CancellationToken token)
        {
            var organization = await GetOrganizationAsync(organizationId);
            if (organization == null)
            {
                yield break;
            }

            await foreach (var memberInfo in organization.ListMembersAsync(range, token))
            {
                yield return Map(memberInfo);
            }
        }

        public async Task<StorageUsage> GetStorageUsageAsync(string organizationId, CancellationToken token = default)
        {
            var organization = await GetOrganizationAsync(organizationId);
            if (organization == null)
            {
                return new StorageUsage();
            }

            var cloudStorageUsage = await organization.GetCloudStorageUsageAsync(token);
            return Map(cloudStorageUsage);
        }

        public void SelectOrganization(string organizationId)
        {
            if (string.IsNullOrEmpty(organizationId))
                return;

            SavedOrganizationId = organizationId;
            LoadOrganization(organizationId);
        }

        public void SelectProject(string projectId, string collectionPath = null, bool updateProject = false)
        {
            var currentProjectId = SelectedProjectOrLibrary?.Id;

            if (string.IsNullOrEmpty(projectId) && string.IsNullOrEmpty(currentProjectId))
                return;

            if (!string.IsNullOrEmpty(projectId) &&
                m_OrganizationInfo?.ProjectInfos?.Exists(p => p.Id == projectId) == false &&
                (m_AssetLibrariesProjects == null || m_AssetLibrariesProjects?.Exists(p => p.Id == projectId) == false))
            {
                Debug.LogWarning($"Project {projectId} is not part of the organization {m_OrganizationInfo.Name} '{m_OrganizationInfo.Id}'");
                return;
            }

            m_SelectedProjectId = projectId;
            m_CollectionPath = collectionPath;

            SavedProjectId = m_SelectedProjectId;
            SavedCollectionPath = m_CollectionPath;

            ProjectSelectionChanged?.Invoke(SelectedProjectOrLibrary ?? SelectedAssetLibrary, SelectedCollection);

            if (updateProject)
            {
                TaskUtils.TrackException(UpdateProjectAsync(projectId));
            }
        }

        public async void EnableProjectForAssetManager()
        {
            var projectDescriptor = new ProjectDescriptor(new OrganizationId(SavedOrganizationId),
                new ProjectId(m_UnityConnectProxy.ProjectId));

            try
            {
                await AssetRepository.EnableProjectForAssetManagerLiteAsync(projectDescriptor, CancellationToken.None);
                await LoadOrganization(SavedOrganizationId);
            }
            catch (Exception e)
            {
                m_MessageManager?.SetHelpBoxMessage(new HelpBoxMessage(
                    L10n.Tr("There was a problem enabling this project in the Asset Manager. Please ensure it has been configured for Unity Cloud and that you have the correct permissions."),
                    RecommendedAction.OpenUnityCloudConfigurationDocumentation,
                    HelpBoxMessageType.Error));

                Utilities.DevLogException(e);
            }
        }

        public ProjectOrLibraryInfo GetProject(string projectId)
        {
            return m_OrganizationInfo?.ProjectInfos.Find(p => p.Id == projectId);
        }

        public async Task<Dictionary<string, string>> GetProjectIconUrlsAsync(string organizationId, CancellationToken token)
        {
            var organization = await GetOrganizationAsync(organizationId);
            if (organization == null)
            {
                return null;
            }

            var projectIconUrls = new Dictionary<string, string>();
            await foreach (var project in organization.ListProjectsAsync(Range.All, token))
            {
                if (project == null)
                    continue;

                var iconUrl = project.IconUrl;
                if (!string.IsNullOrEmpty(iconUrl))
                {
                    projectIconUrls[project.Descriptor.ProjectId.ToString()] = iconUrl;
                }
            }

            return projectIconUrls;
        }

        public async Task CreateCollection(CollectionInfo collectionInfo)
        {
            if (collectionInfo == null)
                return;

            await CreateCollectionAsync(collectionInfo, CancellationToken.None);

            var projectInfo = await GetProjectAsync(collectionInfo.ProjectId);
            if (projectInfo == null)
                return;

            var collections = projectInfo.CollectionInfos.ToList();
            collections.Add(collectionInfo);
            projectInfo.SetCollections(collections);
            ProjectInfoChanged?.Invoke(projectInfo);

            SelectProject(projectInfo.Id, collectionInfo.GetFullPath());
        }

        public async Task DeleteCollection(CollectionInfo collectionInfo)
        {
            if (collectionInfo == null)
                return;

            await DeleteCollectionAsync(collectionInfo, CancellationToken.None);

            var projectInfo = await GetProjectAsync(collectionInfo.ProjectId);
            if (projectInfo == null)
                return;

            var collections = projectInfo.CollectionInfos?.ToList();
            collections?.RemoveAll(c => c.GetFullPath().Contains(collectionInfo.GetFullPath()));
            projectInfo.SetCollections(collections ?? new List<CollectionInfo>());
            ProjectInfoChanged?.Invoke(projectInfo);
        }

        public async Task RenameCollection(CollectionInfo collectionInfo, string newName)
        {
            if (collectionInfo == null || string.IsNullOrEmpty(newName))
                return;

            var oldPath = collectionInfo.GetFullPath();

            var newPath = newName;
            if (!string.IsNullOrEmpty(collectionInfo.ParentPath))
            {
                newPath = $"{collectionInfo.ParentPath}/{newName}";
            }

            var localProjectInfo = GetProject(collectionInfo.ProjectId);
            if (localProjectInfo?.CollectionInfos.Any(c => c.GetFullPath() == newPath) ?? false)
            {
                ProjectInfoChanged?.Invoke(localProjectInfo);
                throw new ServiceException(L10n.Tr("A collection with the same name already exists."));
            }

            await RenameCollectionAsync(collectionInfo, newName, CancellationToken.None);

            var projectInfo = await GetProjectAsync(collectionInfo.ProjectId);
            if (projectInfo == null)
                return;

            ProjectInfoChanged?.Invoke(projectInfo);

            // Ensure the new collection has been successfully loaded before selecting it (if applicable)
            var collections = projectInfo.CollectionInfos.ToList();
            if (collections.Any(c => c.GetFullPath() == newPath))
            {
                var isCollectionSelected = m_CollectionPath == oldPath;
                var isChildCollectionSelected = m_CollectionPath?.StartsWith(oldPath) ?? false;

                if (isCollectionSelected)
                {
                    SelectProject(projectInfo.Id, newPath);
                }
                else if (isChildCollectionSelected)
                {
                    var selectedSubPath = m_CollectionPath.Remove(0, oldPath.Length);
                    SelectProject(projectInfo.Id, newPath + selectedSubPath);
                }
            }
        }

        public async IAsyncEnumerable<NameAndId> ListOrganizationsAsync()
        {
            await foreach(var organization in OrganizationRepository.ListOrganizationsAsync(Range.All))
                yield return new NameAndId() { Id = organization.Id.ToString(), Name = organization.Name };
        }

        public async Task<List<string>> GetOrganizationVersionLabelsAsync()
        {
            if (m_VersionLabels.ContainsKey(m_OrganizationInfo.Id))
                return m_VersionLabels[m_OrganizationInfo.Id];

            lock (m_VersionLabelsLock)
            {
                m_VersionLabelsTask ??= GetOrganizationVersionLabelsInternalAsync();
            }

            return await m_VersionLabelsTask;
        }

        public async IAsyncEnumerable<StatusFlowInfo> GetOrganizationStatusFlowsAsync([EnumeratorCancellation] CancellationToken token)
        {
            await foreach (var flow in AssetRepository.ListStatusFlowsAsync(new OrganizationId(m_OrganizationInfo.Id), Range.All, token))
            {
                var statuses = new List<IStatus>();
                await foreach (var status in flow.ListStatusesAsync(Range.All, token))
                    statuses.Add(status);

                var statusTransitions = new List<IStatusTransition>();
                await foreach (var transition in flow.ListTransitionsAsync(Range.All, token))
                    statusTransitions.Add(transition);

                yield return StatusFlowMapper.From(flow, statuses, statusTransitions);
            }
        }

        public async Task<StatusFlowInfo> GetOrganizationDefaultStatusFlowAsync(CancellationToken token = default)
        {
            var defaultStatusFlow = await AssetRepository.GetDefaultStatusFlowAsync(new OrganizationId(m_OrganizationInfo.Id), token);
            if (defaultStatusFlow == null)
                return null;

            var statuses = new List<IStatus>();
            await foreach (var status in defaultStatusFlow.ListStatusesAsync(Range.All, token))
                statuses.Add(status);

            var statusTransitions = new List<IStatusTransition>();
            await foreach (var transition in defaultStatusFlow.ListTransitionsAsync(Range.All, token))
                statusTransitions.Add(transition);

            return StatusFlowMapper.From(defaultStatusFlow, statuses, statusTransitions);
        }

        async Task<List<string>> GetOrganizationVersionLabelsInternalAsync()
        {
            var query = AssetRepository.QueryLabels(new OrganizationId(m_OrganizationInfo.Id));
            var filter = new LabelSearchFilter();
            filter.IsArchived.WhereEquals(false);
            var labelsAsync = query.SelectWhereMatchesFilter(filter).ExecuteAsync(default);

            var labels = new List<ILabel>();
            await foreach (var item in labelsAsync)
            {
                labels.Add(item);
            }

            m_VersionLabels[m_OrganizationInfo.Id] = labels.Select(l => l.Descriptor.LabelName).ToList();
            return m_VersionLabels[m_OrganizationInfo.Id];
        }

        async Task<IOrganization> GetOrganizationAsync(string organizationId)
        {
            // Try the direct way as this is the preferred way to get an organization and avoids listing all organizations.
            // However, it may not be available for all services (e.g. private cloud).
            try
            {
                return await OrganizationRepository.GetOrganizationAsync(new OrganizationId(organizationId));
            }
            catch (Exception e)
            {
                Utilities.DevLogWarning($"Direct request of organization with ID '{organizationId}' returned error. {e.Message}");
            }

            // Fallback to listing organizations and selecting the organization by id
            try
            {
                await foreach (var organization in OrganizationRepository.ListOrganizationsAsync(Range.All))
                {
                    if (organization.Id.ToString() == organizationId)
                    {
                        return organization;
                    }
                }
            }
            catch (Exception e)
            {
                Utilities.DevLogWarning($"Failed to list organizations. {e.Message}");
            }

            return null;
        }

        internal async Task TryLoadValidOrganizationAsync()
        {
            if (!IsCloudReachable || !IsLoggedIn || IsLoading)
                return;

            var isOrganizationAvailable = await IsSavedOrganizationAvailableForUser();
            if (!isOrganizationAvailable)
            {
                if (m_UnityConnectProxy.HasValidOrganizationId)
                    SavedOrganizationId = m_UnityConnectProxy.OrganizationId;
                else
                {
                    // Get the first available organization for the user
                    await foreach(var organization in OrganizationRepository.ListOrganizationsAsync(Range.All))
                        SavedOrganizationId = organization.Id.ToString();
                }
            }

            if (string.IsNullOrWhiteSpace(m_OrganizationInfo?.Id) || m_OrganizationInfo?.Id != SavedOrganizationId)
            {
                await LoadOrganization(SavedOrganizationId);
            }
        }

        internal async Task TryLoadAssetLibrariesAsync()
        {
            if (!IsCloudReachable || !IsLoggedIn)
            {
                return;
            }

            var assetsSdkProvider = ServicesContainer.instance.Resolve<IAssetsProvider>();
            var assetLibraries = (await ListAssetLibrariesAsync(default)).ToList();

            if (assetLibraries.Count == 0)
            {
                return; // something went wrong in the refresh, we keep what was serialized instead
            }

            // we filter out Default Lite Asset Library as it's not meant to be shown and we don't have the flag yet in the asset sdk
            var assetLibrariesProjectInfos = new List<ProjectOrLibraryInfo>();
            foreach (var assetLibrary in assetLibraries)
            {
                var projectInfo = await DataMapper.From(assetLibrary, true, CancellationToken.None);
                if (projectInfo == null || projectInfo.Name == "Default Lite Asset Library") continue;

                assetLibrariesProjectInfos.Add(projectInfo);
            }
            m_AssetLibrariesProjects = assetLibrariesProjectInfos;

            if (SelectedOrganization != null)
                AssetLibrariesProjectsLoaded?.Invoke(m_AssetLibrariesProjects);
        }

        async Task<IEnumerable<IAssetLibrary>> ListAssetLibrariesAsync(CancellationToken token)
        {
            var queryBuilder = AssetRepository.QueryAssetLibraries();

            var assetLibraryCacheConfiguration = new AssetLibraryCacheConfiguration { CacheProperties = true };

            var results = queryBuilder
                .WithCacheConfiguration(assetLibraryCacheConfiguration)
                .LimitTo(Range.All)
                .ExecuteAsync(token);

            List<IAssetLibrary> libraries = new();
            await foreach (var library in results.WithCancellation(token))
            {
                libraries.Add(library);
            }

            return libraries;
        }

        public async Task<List<ProjectOrLibraryInfo>> GetAssetLibrariesProjectsAsync()
        {
            if (m_AssetLibrariesProjects != null && m_AssetLibrariesProjects.Any()) //we use what was serialized
                return m_AssetLibrariesProjects;

            m_AssetLibrariesLoadTask ??= TryLoadAssetLibrariesAsync();
            await m_AssetLibrariesLoadTask;

            return m_AssetLibrariesProjects;
        }

        Task LoadOrganization(string newOrgId)
        {
            if (!IsCloudReachable || !IsLoggedIn)
                return Task.CompletedTask;

            // Create a new instance if the id has changed
            if (m_OrganizationInfo?.Id != newOrgId)
            {
                m_OrganizationInfo = new OrganizationInfo {Id = newOrgId};
            }

            // Cancel any existing operation
            if (IsLoading)
                m_LoadOrganizationOperation?.Cancel();

            Utilities.DevLog($"Fetching organization info for '{newOrgId}'...");

            // If the organization is not set, invoke change notification
            if (string.IsNullOrEmpty(newOrgId) || newOrgId == "none")
            {
                InvokeOrganizationChanged();
                return Task.CompletedTask;
            }

            m_LoadOrganizationOperation ??= new();
            return m_LoadOrganizationOperation.Start(token => GetOrganizationInfoAsync(newOrgId, token),
                loadingStartCallback: () =>
                {
                    LoadingStateChanged?.Invoke(true);
                    InvokeOrganizationChanged();
                },
                cancelledCallback: () =>
                {
                    Utilities.DevLog("Cancelled loading organization.");
                    InvokeOrganizationChanged();
                },
                exceptionCallback: e =>
                {
                    Debug.LogException(e);
                    InvokeOrganizationChanged();
                },
                successCallback: result =>
                {
                    m_OrganizationInfo = result;
                    SavedOrganizationId = m_OrganizationInfo.Id;

                    InvokeOrganizationChanged();

                    if (m_OrganizationInfo?.ProjectInfos.Any() == true)
                        SelectProject(RestoreSelectedProjectId(), RestoreSelectedCollection());
                },
                finallyCallback: () => { LoadingStateChanged?.Invoke(false); }
            );
        }

        async Task<bool> IsSavedOrganizationAvailableForUser()
        {
            if (GetAuthenticationState() != Cloud.IdentityEmbedded.AuthenticationState.LoggedIn)
                return false;

            await foreach (var organization in OrganizationRepository.ListOrganizationsAsync(Range.All))
            {
                if (organization.Id.ToString() == SavedOrganizationId)
                    return true;
            }

            return false;
        }

        async Task<OrganizationInfo> GetOrganizationInfoAsync(string organizationId, CancellationToken token)
        {
#if AM4U_DEV
            var t = new Stopwatch();
            t.Start();
#endif

#if UNITY_2021
            var orgFromOrgName = await GetOrganizationFromOrganizationName(organizationId);
            organizationId = orgFromOrgName?.Id.ToString();
#endif

            var organizationInfo = new OrganizationInfo {Id = organizationId, Name = "none"};
            if (organizationId == "none")
            {
                return organizationInfo;
            }

            var organization = await GetOrganizationAsync(organizationInfo.Id);
            if (organization == null)
            {
                return organizationInfo;
            }

            organizationInfo.Name = organization.Name;

            // First call to backend requires a valid authentication state
            while (GetAuthenticationState() != Unity.Cloud.IdentityEmbedded.AuthenticationState.LoggedIn)
            {
                await Task.Delay(200, token);
            }

            var projectQuery = AssetRepository.QueryAssetProjects(new OrganizationId(organizationId))?
                .WithCacheConfiguration(new AssetProjectCacheConfiguration {CacheProperties = true})
                .LimitTo(Range.All);
            await foreach (var projectInfo in DataMapper.ListProjectsAsync(projectQuery, token))
            {
                if (projectInfo != null)
                {
                    organizationInfo.ProjectInfos.Add(projectInfo);
                }
            }

#if AM4U_DEV
            t.Stop();
            Utilities.DevLog($"Fetching {organizationInfo.ProjectInfos.Count} Projects took {t.ElapsedMilliseconds}ms");
#endif

            try
            {
                var filter = new FieldDefinitionSearchFilter();
                filter.Deleted.WhereEquals(false);
                filter.FieldOrigin.WhereEquals(FieldDefinitionOrigin.User);

                var metadataQuery = AssetRepository.QueryFieldDefinitions(new OrganizationId(organizationId))?
                    .WithCacheConfiguration(new FieldDefinitionCacheConfiguration {CacheProperties = true})
                    .SelectWhereMatchesFilter(filter);
                await foreach (var metadataFieldDefinition in DataMapper.ListFieldDefinitionsAsync(metadataQuery, token))
                {
                    organizationInfo.MetadataFieldDefinitions.Add(metadataFieldDefinition);
                }
            }
            catch (ForbiddenException)
            {
                // No rights to query field definitions
            }

            return organizationInfo;
        }

#if UNITY_2021
        async Task<IOrganization> GetOrganizationFromOrganizationName(string organizationName)
        {
            // First call to backend requires a valid authentication state
            while (GetAuthenticationState() != Unity.Cloud.Identity.AuthenticationState.LoggedIn)
            {
                await Task.Delay(200);
            }

            var organizationsAsync = OrganizationRepository.ListOrganizationsAsync(Range.All);

            await foreach (var organization in organizationsAsync)
            {
                if (CreateTagFromOrganizationName(organization.Name) == organizationName)
                {
                    return organization;
                }
            }

            return null;
        }

        // organization names that are coming out of CloudProjectSettings.organizationId are formatted as tag
        string CreateTagFromOrganizationName(string organizationName)
        {
            return organizationName.ToLowerInvariant().Replace(" ", "-");
        }
#endif


        string RestoreSelectedProjectId()
        {
            if (!string.IsNullOrEmpty(SavedProjectId) && m_OrganizationInfo?.ProjectInfos.Any(x => x.Id == SavedProjectId) == true)
            {
                return SavedProjectId;
            }

            return m_OrganizationInfo?.ProjectInfos.FirstOrDefault()?.Id;
        }

        string RestoreSelectedCollection()
        {
            return SavedCollectionPath;
        }

        void InvokeOrganizationChanged()
        {
            var selected = SelectedOrganization;

            Utilities.DevLog($"OrganizationChanged '{selected?.Id}'");

            OrganizationChanged?.Invoke(selected);
        }

        async Task UpdateProjectAsync(string projectId)
        {
            var projectInfo = await GetProjectAsync(projectId);
            ProjectInfoChanged?.Invoke(projectInfo);
        }

        async Task<ProjectOrLibraryInfo> GetProjectAsync(string projectId)
        {
            if (string.IsNullOrEmpty(projectId))
                return null;

            if (m_AssetLibrariesProjects != null && m_AssetLibrariesProjects.Exists(p => p.Id == projectId))
                return m_AssetLibrariesProjects.Find(p => p.Id == projectId);

            var projectInfo = await GetProjectInfoAsync(SelectedOrganization.Id, projectId, CancellationToken.None);
            if (projectInfo == null)
                return null;

            var index = m_OrganizationInfo.ProjectInfos.FindIndex(pi => pi.Id == projectId);
            if (index == -1)
            {
                m_OrganizationInfo.ProjectInfos.Add(projectInfo);
            }
            else
            {
                m_OrganizationInfo.ProjectInfos[index] = projectInfo;
            }

            return projectInfo;
        }

        async Task<ProjectOrLibraryInfo> GetProjectInfoAsync(string organizationId, string projectId, CancellationToken token)
        {
            try
            {
                var projectDescriptor = new ProjectDescriptor(new OrganizationId(organizationId), new ProjectId(projectId));
                var assetProject = await AssetRepository.GetAssetProjectAsync(projectDescriptor, token);

                return await DataMapper.From(assetProject, true, token);
            }
            catch (ForbiddenException)
            {
                return null;
            }
            catch (NotFoundException)
            {
                return null;
            }
        }

        async Task CreateCollectionAsync(CollectionInfo collectionInfo, CancellationToken token)
        {
            var collectionDescriptor = Map(collectionInfo);
            var project = await AssetRepository.GetAssetProjectAsync(collectionDescriptor.ProjectDescriptor, token);
            var collectionCreation = new AssetCollectionCreation(collectionInfo.Name, k_DefaultCollectionDescription)
            {
                ParentPath = collectionInfo.ParentPath
            };
            await project.CreateCollectionLiteAsync(collectionCreation, token);
        }

        async Task DeleteCollectionAsync(CollectionInfo collectionInfo, CancellationToken token)
        {
            var collectionDescriptor = Map(collectionInfo);
            var project = await AssetRepository.GetAssetProjectAsync(collectionDescriptor.ProjectDescriptor, token);
            await project.DeleteCollectionAsync(collectionDescriptor.Path, token);
        }

        async Task RenameCollectionAsync(CollectionInfo collectionInfo, string newName, CancellationToken token)
        {
            var collection = await AssetRepository.GetAssetCollectionAsync(Map(collectionInfo), token);
            var assetCollectionUpdate = new AssetCollectionUpdate { Name = newName, Description = k_DefaultCollectionDescription };
            await collection.UpdateAsync(assetCollectionUpdate, token);
        }

        static UserInfo Map(IMemberInfo memberInfo)
        {
            return new UserInfo {UserId = memberInfo.UserId.ToString(), Name = memberInfo.Name};
        }

        static StorageUsage Map(ICloudStorageUsage cloudStorageUsage)
        {
            return new StorageUsage(cloudStorageUsage.UsageBytes, cloudStorageUsage.TotalStorageQuotaBytes);
        }

        static CollectionDescriptor Map(CollectionInfo collectionInfo)
        {
            return new CollectionDescriptor(
                new ProjectDescriptor(
                    new OrganizationId(collectionInfo.OrganizationId),
                    new ProjectId(collectionInfo.ProjectId)),
                new CollectionPath(collectionInfo.GetFullPath()));
        }

        /// <summary>
        /// A wrapper for certain SDK functions and value types that are not easily mockable.
        /// </summary>
        internal interface IDataMapper
        {
            [ExcludeFromCoverage]
            async IAsyncEnumerable<ProjectOrLibraryInfo> ListProjectsAsync(AssetProjectQueryBuilder queryBuilder,
                [EnumeratorCancellation] CancellationToken token)
            {
                await foreach (var project in queryBuilder.ExecuteAsync(token))
                {
                    yield return await From(project, false, token);
                }
            }

            [ExcludeFromCoverage]
            async IAsyncEnumerable<IMetadataFieldDefinition> ListFieldDefinitionsAsync(FieldDefinitionQueryBuilder queryBuilder,
                [EnumeratorCancellation] CancellationToken token)
            {
                await foreach (var fieldDefinition in queryBuilder.ExecuteAsync(token))
                {
                    yield return await From(fieldDefinition, token);
                }
            }

            [ExcludeFromCoverage]
            async Task<ProjectOrLibraryInfo> From(IAssetProject project, bool waitForCollections, CancellationToken token)
            {
                var projectProperties = await project.GetPropertiesAsync(token);

                var projectInfo = new ProjectOrLibraryInfo(
                    project.Descriptor.ProjectId.ToString(),
                    projectProperties.Name
                );

                if (projectProperties.HasCollection)
                {
                    var collectionTask = GetCollections(project,
                        projectProperties.Name,
                        collections =>
                        {
                            projectInfo.SetCollections(collections.Select(c => new CollectionInfo
                            (
                                project.Descriptor.OrganizationId.ToString(),
                                project.Descriptor.ProjectId.ToString(),
                                c.Name,
                                c.ParentPath
                            )));
                        },
                        token);

                    if (waitForCollections)
                    {
                        await collectionTask;
                    }
                }

                return projectInfo;
            }

            [ExcludeFromCoverage]
            async Task<ProjectOrLibraryInfo> From(IAssetLibrary assetLibrary, bool waitForCollections, CancellationToken token)
            {
                var projectProperties = await assetLibrary.GetPropertiesAsync(token);

                var libraryInfo = new ProjectOrLibraryInfo(
                    assetLibrary.Id.ToString(),
                    projectProperties.Name,
                    true
                );

                if (!projectProperties.HasCollection)
                    return libraryInfo;

                var collectionTask = GetCollections(assetLibrary,
                    projectProperties.Name,
                    collections =>
                    {
                        libraryInfo.SetCollections(collections.Select(c => new CollectionInfo
                        (
                            string.Empty,
                            assetLibrary.Id.ToString(),
                            c.Name,
                            c.ParentPath
                        )));
                    },
                    token);

                if (waitForCollections)
                {
                    await collectionTask;
                }

                return libraryInfo;
            }

            [ExcludeFromCoverage]
            async Task<IMetadataFieldDefinition> From(IFieldDefinition fieldDefinition, CancellationToken token)
            {
                var properties = await fieldDefinition.GetPropertiesAsync(token);

                if (properties.Type == FieldDefinitionType.Selection)
                {
                    var selectionProperties = properties.AsSelectionFieldDefinitionProperties();
                    return new SelectionFieldDefinition(fieldDefinition.Descriptor.FieldKey, properties.DisplayName,
                        GetSelectionFieldType(selectionProperties), selectionProperties.AcceptedValues);
                }

                var fieldType = properties.Type switch
                {
                    FieldDefinitionType.Boolean => MetadataFieldType.Boolean,
                    FieldDefinitionType.Text => MetadataFieldType.Text,
                    FieldDefinitionType.Number => MetadataFieldType.Number,
                    FieldDefinitionType.Url => MetadataFieldType.Url,
                    FieldDefinitionType.Timestamp => MetadataFieldType.Timestamp,
                    FieldDefinitionType.User => MetadataFieldType.User,
                    _ => throw new InvalidOperationException("Unexpected field definition type was encountered.")
                };

                return new MetadataFieldDefinition(fieldDefinition.Descriptor.FieldKey, properties.DisplayName, fieldType);
            }

            [ExcludeFromCoverage]
            static Task GetCollections(IAssetProject project, string projectName, Action<IEnumerable<IAssetCollection>> successCallback, CancellationToken token)
            {
                var asyncLoad = new AsyncLoadOperation();
                return asyncLoad.Start(t => project.ListCollectionsAsync(Range.All, CancellationTokenSource.CreateLinkedTokenSource(t, token).Token),
                    null,
                    successCallback,
                    null,
                    () => Utilities.DevLog($"Cancelled fetching collections for {projectName}."),
                    ex => Utilities.DevLog($"Error fetching collections for {projectName}: {ex.Message}"));
            }

            [ExcludeFromCoverage]
            static Task GetCollections(IAssetLibrary assetLibrary, string projectName, Action<IEnumerable<IAssetCollection>> successCallback, CancellationToken token)
            {
                var asyncLoad = new AsyncLoadOperation();
                return asyncLoad.Start(t => assetLibrary.ListCollectionsAsync(Range.All, CancellationTokenSource.CreateLinkedTokenSource(t, token).Token),
                    null,
                    successCallback,
                    null,
                    () => Utilities.DevLog($"Cancelled fetching collections for {projectName}."),
                    ex => Utilities.DevLog($"Error fetching collections for {projectName}: {ex.Message}"));
            }

            [ExcludeFromCoverage]
            static MetadataFieldType GetSelectionFieldType(SelectionFieldDefinitionProperties properties) =>
                properties.Multiselection ? MetadataFieldType.MultiSelection : MetadataFieldType.SingleSelection;
        }
    }
}
