using System;
using System.Collections.Generic;
using System.Linq;

namespace Unity.AssetManager.Editor
{
    /// <summary>
    /// The base class for creating custom postprocessors for upload assets.
    /// </summary>
    public class AssetManagerPostprocessor
    {
        /// <summary>
        /// The order in which the postprocessor should be executed. Lower values are executed first.
        /// </summary>
        /// <returns>The postprocessor order</returns>
        public virtual int GetPostprocessOrder() => 0;

        /// <summary>
        /// Callback invoked when an asset is about to be uploaded, allowing for modification of the asset's data
        /// </summary>
        /// <param name="asset">The asset to be uploaded</param>
        public virtual void OnPostprocessUploadAsset(UploadAsset asset) { }
    }

    class AssetManagerPostprocessorUtility
    {
        public static AssetManagerPostprocessor[] InstantiateAllAssetManagerPostprocessorsAndOrder()
        {
            var baseType = typeof(AssetManagerPostprocessor);
            var derivedTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t) && t != baseType);

            var instances = new List<AssetManagerPostprocessor>();
            foreach (var type in derivedTypes)
            {
                try
                {
                    var instance = Activator.CreateInstance(type) as AssetManagerPostprocessor;
                    if (instance != null)
                        instances.Add(instance);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"Failed to instantiate {type.FullName}: {ex.Message}");
                }
            }

            return instances.OrderBy(p => p.GetPostprocessOrder()).ToArray();
        }
    }
}
