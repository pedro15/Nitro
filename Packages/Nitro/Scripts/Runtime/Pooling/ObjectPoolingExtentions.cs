using UnityEngine;
using Nitro.Pooling.Core;

namespace Nitro.Pooling
{
    /// <summary>
    /// Extention methods for gameObject for easy use API
    /// </summary>
    public static class ObjectPoolExtentions
    {
        /// <summary>
        /// Recycle the gameObject to the Object Pool if available
        /// </summary>
        public static void Recycle(this GameObject go)
        {
            if (!go) return;

            if (PoolManager.Instance != null)
                PoolManager.Instance.RecycleGameObject(go);
            else
            {
                Debug.LogWarning($"[Nitro] '{typeof(PoolManager).Name}' is Null. Destroying the object instead of recycling it.. " + 
                    $"Please Make sure that you have an instance of '{typeof(PoolManager).Name}' in your scene");
                Object.Destroy(go);
            }
        }
        
        /// <summary>
        /// Is the gameObject asigned on an Object Pool ?
        /// </summary>
        public static bool IsOnPool( this GameObject go)
        {
            return PoolManager.Instance.IsOnObjectPool(go);
        }

        /// <summary>
        /// Gets the associated recyclebin to this gameObject
        /// </summary>
        /// <returns>returns null if the gameObject is not associated to any ObjectPool</returns>
        public static RecycleBin GetRecycleBin(this GameObject go)
        {
            return PoolManager.Instance.GetRecycleBin(go);
        }
    }
}