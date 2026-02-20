using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.AssetManager.Core.Editor;
using Unity.Cloud.CommonEmbedded;
using UnityEngine;
using AssetUpdate = Unity.AssetManager.Core.Editor.AssetUpdate;
using Object = UnityEngine.Object;
using Utilities = Unity.AssetManager.Core.Editor.Utilities;

namespace Unity.AssetManager.Upload.Editor
{
    class UploadOperation : AssetDataOperation, IProgress<HttpProgress>
    {
        List<AssetIdentifier> m_Dependencies = new();
        readonly  List<AssetDependency> m_ExistingDependencies = new();
        readonly HashSet<HttpProgress> m_HttpProgresses = new();
        readonly IUploadAsset m_UploadAsset;

        string m_Description;
        float m_Progress;

        public override AssetIdentifier Identifier => m_UploadAsset.LocalIdentifier;
        public override float Progress => m_Progress;
        public override string OperationName => $"Uploading {Path.GetFileName(m_UploadAsset.Name)}";
        public override string Description => m_Description;
        public override bool StartIndefinite => true;
        public override bool IsSticky => true;

        public UploadOperation(IUploadAsset uploadAsset)
        {
            m_UploadAsset = uploadAsset;
        }

        public void Report(HttpProgress value)
        {
            m_HttpProgresses.Add(value);

            var totalProgress = m_HttpProgresses.Where(httpProgress => httpProgress.UploadProgress != null)
                .Sum(httpProgress => httpProgress.UploadProgress.Value);

            totalProgress /= m_HttpProgresses.Count;

            ReportStep(totalProgress);
        }

        public void Report(string description, float? progress = null)
        {
            m_Description = description;
            if(progress.HasValue)
                m_Progress = progress.Value;
            Report();
        }

        public async Task FetchAssetDependenciesAsync(AssetData targetAssetData, IDictionary<AssetIdentifier, AssetData> identifierToAssetLookup,
            CancellationToken token = default)
        {
            var assetsProvider = ServicesContainer.instance.Resolve<IAssetsProvider>();

            ReportStep("Preparing manifest...");

            var dependencies = new List<AssetIdentifier>();

            // Dependency manifest
            foreach (var dependency in m_UploadAsset.Dependencies)
            {
                if (dependency.IsLocal())
                {
                    // If the dependency is pointing to a local asset, we need to resolve its target asset data

                    var identifier = identifierToAssetLookup[dependency].Identifier.Clone();
                    identifier.VersionLabel = dependency.VersionLabel;
                    identifier.Version = dependency.Version != AssetManagerCoreConstants.NewVersionId ? dependency.Version : identifier.Version;
                    dependencies.Add(identifier);
                }
                else
                {
                    // Otherwise, the dependency is already pointing to a cloud asset
                    dependencies.Add(dependency);
                }
            }
            m_Dependencies = dependencies;

            m_ExistingDependencies.Clear();
            var cloudDependenciesAsync = assetsProvider.GetDependenciesAsync(targetAssetData.Identifier, Range.All, token);
            await foreach (var dependency in cloudDependenciesAsync)
            {
                m_ExistingDependencies.Add(dependency);
            }
        }

        public async Task UpdateDependenciesAsync(AssetData targetAssetData, CancellationToken token = default)
        {
            ReportStep("Updating dependencies");
            ReportStep(-1);

            var assetsProvider = ServicesContainer.instance.Resolve<IAssetsProvider>();
            await assetsProvider.UpdateDependenciesAsync(targetAssetData.Identifier, m_Dependencies, m_ExistingDependencies, token);

            ReportStep("Done updating dependencies. Waiting on others to finish..");
            ReportStep(0.85f);
        }

        public async Task UploadAsync(AssetData targetAssetData, CancellationToken token = default)
        {
            const ComparisonResults filesChanged = ComparisonResults.FilesAdded | ComparisonResults.FilesRemoved | ComparisonResults.FilesModified;

            ReportStep("Preparing for upload");

            var assetsProvider = ServicesContainer.instance.Resolve<IAssetsProvider>();
            var ioProxy = ServicesContainer.instance.Resolve<IIOProxy>();

            // Create a thumbnail

            GetDatabaseAssetInfo(m_UploadAsset, out var assetPath, out var assetInstance);
            var thumbnailFile = await UploadThumbnailAsync(assetsProvider, assetInstance, assetPath, targetAssetData, token);

            // Upload files tasks

            // Only upload files if they were considered modified in some way
            if ((m_UploadAsset.ComparisonResults & filesChanged) != 0)
            {
                var fileNumber = 0;
                await TaskUtils.RunAllTasks(m_UploadAsset.Files.Where(file => !string.IsNullOrEmpty(file.SourcePath)),
                    file =>
                    {
                        Interlocked.Increment(ref fileNumber);
                        ReportStep($"Preparing file {Path.GetFileName(file.SourcePath)} ({fileNumber} of {m_UploadAsset.Files.Count})");
                        return UploadFile(file.DestinationPath, file.SourcePath);
                    });
            }

            // Finalize asset

            await ApplyThumbnailAsync(assetsProvider, targetAssetData, thumbnailFile, token);
            await UpdateStatusAsync(assetsProvider, targetAssetData, m_UploadAsset.Status, token);
            await FreezeAssetAsync(assetsProvider, targetAssetData, token);


            async Task UploadFile(string destinationPath, string sourcePath)
            {
                await using var stream = ioProxy.FileOpenRead(sourcePath);
                var file = await assetsProvider.UploadFile(targetAssetData, destinationPath, stream, this, token);
                // If the thumbnail has not been set, select a file that is supported for preview
                thumbnailFile ??= AssetDataTypeHelper.IsSupportingPreviewGeneration(Path.GetExtension(destinationPath)) ? file : null;
            }

            ReportStep("Done uploading. Waiting on others to finish..");
            ReportStep(0.75f);
        }

        void ReportStep(string description)
        {
            m_Description = description;
            Report();
        }

        void ReportStep(float progress = 0.0f)
        {
            m_Progress = progress;
            Report();
        }

        static void GetDatabaseAssetInfo(IUploadAsset uploadAsset, out string assetPath, out Object assetInstance)
        {
            var assetDatabaseProxy = ServicesContainer.instance.Resolve<IAssetDatabaseProxy>();
            assetPath = assetDatabaseProxy.GuidToAssetPath(uploadAsset.PreviewGuid);
            assetInstance = assetDatabaseProxy.LoadAssetAtPath(assetPath);
        }

        async Task<AssetDataFile> UploadThumbnailAsync(IAssetsProvider assetsProvider, Object assetInstance, string assetPath, AssetData targetAssetData,
            CancellationToken token)
        {
            if (!RequiresThumbnail(assetInstance, assetPath))
                return null;

            ReportStep("Preparing thumbnail");

            AssetDataFile thumbnailFile = null;
            var texture = await AssetPreviewer.GenerateAdvancedPreview(assetInstance, assetPath, 512);
            if (texture != null)
            {
                thumbnailFile = await assetsProvider.UploadThumbnail(targetAssetData, texture, this, token);
            }

            return thumbnailFile;
        }

        static bool RequiresThumbnail(Object assetInstance, string assetPath)
        {
            return assetInstance is not Texture2D ||
                   !AssetDataTypeHelper.IsSupportingPreviewGeneration(Path.GetExtension(assetPath));
        }

        async Task ApplyThumbnailAsync(IAssetsProvider assetsProvider, AssetData targetAssetData, AssetDataFile thumbnailFile, CancellationToken token)
        {
            if (thumbnailFile != null && !string.IsNullOrEmpty(thumbnailFile.Path))
            {
                ReportStep("Applying thumbnail");

                var existingTags = targetAssetData.Tags ?? new List<string>();
                var assetUpdate = new AssetUpdate
                {
                    // Bubble up the generated tags from the thumbnail to the asset
                    Tags = existingTags.Union(thumbnailFile.Tags ?? Array.Empty<string>()).ToList(),
                    PreviewFile = thumbnailFile.Path
                };

                await assetsProvider.UpdateAsync(targetAssetData, assetUpdate, token);
            }
        }

        async Task UpdateStatusAsync(IAssetsProvider assetsProvider, AssetData targetAssetData, string targetStatus, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(targetStatus))
                return;

            var warningMessage = $"Unable to update the status to {targetStatus} for asset '{targetAssetData.Name}'. Asset will stay in {targetAssetData.Status} status.";

            try
            {
                var projectOrganizationProvider = ServicesContainer.instance.Resolve<IProjectOrganizationProvider>();
                var statusFlowInfo = await projectOrganizationProvider.SelectedOrganization.GetStatusFlowInfoAsync(targetAssetData, token);

                if (statusFlowInfo == null)
                {
                    Debug.LogWarning(warningMessage);
                    return;
                }

                var currentStatus = targetAssetData.Status;
                if (string.Equals(currentStatus, targetStatus, StringComparison.Ordinal))
                    return;

                // Get the transition path from the current status to the target status
                var path = statusFlowInfo.GetTransitionPath(currentStatus, targetStatus);
                if (path == null || path.Length == 0)
                {
                    Debug.LogWarning(warningMessage);
                    return;
                }

                // Sequentially update the status of the asset through that path (skip the first, which is the current status)
                ReportStep("Updating status");
                for (var i = 1; i < path.Length; i++)
                {
                    await assetsProvider.UpdateStatusAsync(targetAssetData, path[i], token);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Debug.LogWarning(warningMessage);
                Utilities.DevLogException(e);
            }
        }

        static async Task FreezeAssetAsync(IAssetsProvider assetsProvider, AssetData targetAssetData, CancellationToken token)
        {
            try
            {
                await assetsProvider.FreezeAsync(targetAssetData, null, token);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Unable to commit asset version for asset {targetAssetData?.Name}. Asset version will remain unfrozen.");
                Utilities.DevLogException(e);
            }
        }
    }
}
