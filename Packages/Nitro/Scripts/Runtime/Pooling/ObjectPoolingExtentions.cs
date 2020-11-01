using UnityEngine;

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
            PoolManager.Instance.RecycleGameObject(go);

            

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