using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Nitro.Utility;

namespace Nitro.Pooling
{
    [AddComponentMenu("Nitro/Object Pool/Pool Manager")]
    public class PoolManager : MonoSingleton<PoolManager>
    {
        public delegate void OnInitDelegate();

        public static event OnInitDelegate OnInit;

        [SerializeField]
        private bool persistent = false;

        protected override bool Persistent => persistent;

        [SerializeField]
        private PoolDefinition predefinedPool = default;

        public bool IsInitialized { get; private set; } = false;

        private Dictionary<string, RecycleBin> runtimeRecycleBins = new Dictionary<string, RecycleBin>();

        private Dictionary<GameObject, string> ObjectPoolData = new Dictionary<GameObject, string>();

        private IEnumerator Start()
        {
            IsInitialized = false;
            if (RegisterSingleton())
            {
                Debug.Log("INIT! POOL !", this);
                if (predefinedPool != null && predefinedPool.poolData.Length > 0)
                {
                    for (int i = 0; i < predefinedPool.poolData.Length; i++)
                    {
                        var current_data = predefinedPool.poolData[i];

                        if (string.IsNullOrEmpty(current_data.Label))
                        {
                            Debug.LogWarning($"[{GetType().Name}] Pool With null label detected at position {i}. Ignoring...");
                            continue;
                        }

                        if (runtimeRecycleBins.ContainsKey(current_data.Label))
                        {
                            Debug.LogWarning($"[{GetType().Name}] A Pool already Exists with label '{current_data.Label}'. Ignoring...'");
                            continue;
                        }

                        RecycleBin current = null;

                        switch (current_data.referenceType)
                        {
                            case PoolReferenceType.PREFAB:
                                current = new RecycleBin(current_data.Label, current_data.Prefab, current_data.MaxItems,
                                    current_data.PreallocateCount, null, current_data.UsePoolParent);
                                break;
#if ADDRESSABLES_INSTALLED
                            case PoolReferenceType.ASSET_REFERENCE:
                                current = new RecycleBin(current_data.Label, current_data.assetReference,
                                    current_data.MaxItems, current_data.PreallocateCount, null, current_data.UsePoolParent);
                                break;

                            case PoolReferenceType.LABEL_REFERENCE:
                                current = new RecycleBin(current_data.Label, current_data.assetlabelReference,
                                    current_data.MaxItems, current_data.PreallocateCount, null, current_data.UsePoolParent);
                                break;
#endif
                            default: goto case PoolReferenceType.PREFAB;
                        }

                        runtimeRecycleBins.Add(current.Label, current);
                        Debug.Log("Init pool " + i);
                        yield return current.FillPool();
                    }
                }
                yield return new WaitForEndOfFrame();
                Debug.Log($"[{GetType().Name}] Initialization finished");
                IsInitialized = true;
                if (OnInit != null) OnInit.Invoke();
            }
        }

        public GameObject Spawn(string key, Vector3 position, Quaternion rotation)
        {
            RecycleBin recycleBin = GetRecycleBin(key);
            if (recycleBin != null)
            {
                GameObject clone = recycleBin.Spawn(position, rotation);

                if (clone != null && !ObjectPoolData.ContainsKey(clone))
                    ObjectPoolData.Add(clone, key);

                return clone;
            }else
            {
                Debug.LogError($"[{GetType().Name}] key not found: " + key);
            }
            return null;
        }

        public void RecycleGameObject(GameObject obj)
        {
            string recycleBinIndex;
            if (ObjectPoolData.TryGetValue(obj , out recycleBinIndex))
            {
                runtimeRecycleBins[recycleBinIndex].Recycle(obj);
            }
        }

        public bool IsOnObjectPool(GameObject obj)
        {
            return ObjectPoolData.ContainsKey(obj);
        }

        public RecycleBin GetRecycleBin(GameObject obj)
        {
            string rid;
            if (ObjectPoolData.TryGetValue(obj, out rid))
                return runtimeRecycleBins[rid];

            return null;
        }

        public RecycleBin GetRecycleBin(string key)
        {
            RecycleBin recycleBin;
            if (runtimeRecycleBins.TryGetValue(key, out recycleBin))
            {
                return recycleBin;
            }
            return null;
        }

        public void CreateObjectPool(GameObject prefab, string objectPoolKey, int MaxItems , int preallocateCount = 0, Transform parent = null,
            bool forcePoolParent = true)
        {
            if (!ObjectPoolExists(objectPoolKey))
            {
                RecycleBin recycleBin = new RecycleBin(objectPoolKey, prefab, MaxItems , preallocateCount , parent , forcePoolParent);
                
                if(preallocateCount > 0)
                {
                    recycleBin.FillPool();
                }

                runtimeRecycleBins.Add(objectPoolKey, recycleBin);
            }
            else
            {
                Debug.LogError($"[{GetType().Name}] Can't create object pool: the key already exists");
            }
        }

        public bool ObjectPoolExists(string poolkey)
        {
            return runtimeRecycleBins.ContainsKey(poolkey);
        }
    }
}