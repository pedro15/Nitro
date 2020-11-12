using UnityEngine;

#if ADDRESSABLES_INSTALLED
using UnityEngine.AddressableAssets;
#endif

namespace Nitro.Pooling.Core
{
    /// <summary>
    /// Defines the Reference type of a Object Pool.
    /// </summary>
    public enum PoolReferenceType : int
    {
        /// <summary>
        /// Use a common Unity Prefab for the object pool instances
        /// </summary>
        PREFAB = 0,
        /// <summary>
        /// Use 'AssetReference' to get the Prefab. It's useful when you have a collection when just one prefab type.
        /// </summary>
        /// <remarks>
        /// Only valid when 'Addressables' package is installed
        /// </remarks>
        ASSET_REFERENCE = 1,
        /// <summary>
        /// Use 'AssetLabelReference' to get the prefab(s). It's usefult when you have a collecion with variations.
        /// </summary>
        /// <remarks>
        /// Only valid when 'Addressables' package is installed
        /// </remarks>
        LABEL_REFERENCE = 2
    }

    [System.Serializable]
    public struct RecycleBinData
    {
        public string Label;
        public int Priority;
        public PoolReferenceType ReferenceType;
        public GameObject[] Prefabs;
#if ADDRESSABLES_INSTALLED
        public AssetReference AssetReference;
        public AssetLabelReference AssetLabelReference;
#endif
        public int PreallocateCount;
        public bool UsePoolParent;
    }
}