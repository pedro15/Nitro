using UnityEngine;
using System.Reflection;

namespace Nitro.Utility
{
    /// <summary>
    /// Singleton Desing pattern implementation
    /// </summary>
    /// <typeparam name="T">Class</typeparam>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T eInstance = null;

        /// <summary>
        /// Should this singleton be persistent Between scenes?
        /// </summary>
        protected virtual bool Persistent
        {
            get { return false; }
        }

        public static T Instance
        {
            get
            {
                if (!eInstance)
                {
                    eInstance = FindObjectOfType<T>();

                    if (!eInstance)
                    {
                        SingletonPrefabAttribute m_prefab = typeof(T).GetCustomAttribute<SingletonPrefabAttribute>(true);
                        if (m_prefab != null && !string.IsNullOrEmpty(m_prefab.PrefabPath))
                        {
                            try
                            {
                                eInstance = Instantiate(Resources.Load<GameObject>(m_prefab.PrefabPath)).GetComponent<T>();
                            }
                            catch { }
                        }
                    }
                }
                return eInstance;
            }
        }

        /// <summary>
        /// Validate and initializes singleton Instance
        /// </summary>
        /// <returns>True if the singleton instance is valid</returns>
        protected virtual bool ValidateSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return false;
            }

            if (Persistent)
            {
                DontDestroyOnLoad(gameObject);
            }

            eInstance = this as T;

            return true;
        }
    }
}