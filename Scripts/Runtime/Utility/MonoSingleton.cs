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
                            eInstance = Instantiate(Resources.Load<GameObject>(m_prefab.PrefabPath)).GetComponent<T>();
                        }
                    }
                }
                return eInstance;
            }
        }

        protected virtual bool RegisterSingleton()
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