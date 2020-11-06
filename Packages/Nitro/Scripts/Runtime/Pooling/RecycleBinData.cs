using UnityEngine;

#if ADDRESSABLES_INSTALLED
using UnityEngine.AddressableAssets;
#endif

namespace Nitro.Pooling
{
    /// <summary>
    /// Defines the Reference type of a Object Pool.
    /// </summary>
    public enum PoolReferenceType : int
    {
        /// <summary>
        /// Use a common Unity Prefab for the object pool instances
        /// </summary>
        PREFAB = 1,
        /// <summary>
        /// Use 'AssetReference' to get the Prefab. It's useful when you have a collection when just one prefab type.
        /// </summary>
        /// <remarks>
        /// Only valid when 'Addressables' package is installed
        /// </remarks>
        ASSET_REFERENCE = 2,
        /// <summary>
        /// Use 'AssetLabelReference' to get the prefab(s). It's usefult when you have a collecion with variations.
        /// </summary>
        /// <remarks>
        /// Only valid when 'Addressables' package is installed
        /// </remarks>
        LABEL_REFERENCE = 3
    }

    [System.Serializable]
    public struct RecycleBinData
    {
        public string Label;
        public int Priority;
        public PoolReferenceType referenceType;
        [Header("Reference")]
        public GameObject Prefab;
#if ADDRESSABLES_INSTALLED
        public AssetReference assetReference;
        public AssetLabelReference assetlabelReference;
#endif
        [Header("General Settings")]
        public int PreallocateCount;
        public bool UsePoolParent;
    }
}