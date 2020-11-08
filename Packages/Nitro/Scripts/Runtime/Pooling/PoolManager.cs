using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Nitro.Utility;
using System.Linq;

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

        private Dictionary<int, string> ObjectPoolData = new Dictionary<int, string>();

        private IEnumerator Start()
        {
            if (RegisterSingleton())
            {
                Debug.Log("INIT! POOL !", this);
                if (predefinedPool != null && predefinedPool.poolData.Count > 0)
                {
                    for (int i = 0; i < predefinedPool.poolData.Count; i++)
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
                                current = new RecycleBin(current_data.Label, current_data.Prefabs,
                                    current_data.PreallocateCount, null, current_data.UsePoolParent, current_data.Priority);
                                break;
#if ADDRESSABLES_INSTALLED
                            case PoolReferenceType.ASSET_REFERENCE:
                                current = new RecycleBin(current_data.Label, current_data.assetReference,current_data.PreallocateCount, 
                                    null, current_data.UsePoolParent , current_data.Priority);
                                break;

                            case PoolReferenceType.LABEL_REFERENCE:
                                current = new RecycleBin(current_data.Label, current_data.assetlabelReference, 
                                    current_data.PreallocateCount, null, current_data.UsePoolParent,current_data.Priority);
                                break;
#endif
                            default: goto case PoolReferenceType.PREFAB;
                        }

                        runtimeRecycleBins.Add(current.Label, current);

                        yield return current.Allocate();
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
            if (runtimeRecycleBins.ContainsKey(key))
            {
                RecycleBin Rb;
                if (runtimeRecycleBins.TryGetValue(key , out Rb ))
                {
                    GameObject clone = Rb.Spawn(position, rotation);
                    int _id = clone.GetInstanceID();
                    if (clone != null && !ObjectPoolData.ContainsKey(_id))
                        ObjectPoolData.Add(_id, key);

                    return clone;
                }
                return null;
            }else
            {
                Debug.LogError($"[{GetType().Name}] key not found: " + key);
            }
            return null;
        }

        public GameObject SpawnWeighted(Vector3 position, Quaternion rotation , params string[] ValidKeys)
        {
            int ChooseIndex(float[] probs)
            {
                float total = 0;

                foreach (float elem in probs)
                {
                    total += elem;
                }

                float randomPoint = Random.value * total;

                for (int i = 0; i < probs.Length; i++)
                {
                    if (randomPoint < probs[i])
                    {
                        return i;
                    }
                    else
                    {
                        randomPoint -= probs[i];
                    }
                }
                return probs.Length - 1;
            }

            RecycleBin[] validBins = runtimeRecycleBins.Where((KeyValuePair<string, RecycleBin> recycleBinItem)
                => ValidKeys.Any((string key) => key.Equals(recycleBinItem.Key))).
                Select((KeyValuePair<string, RecycleBin> recycleBinItem) => recycleBinItem.Value).ToArray();

            if (validBins == null || validBins.Length <= 0)
            {
                Debug.LogError($"[{GetType().Name}] No Valid ObjectPool found with the given keys.");
                return null;
            }

            int[] priorities = validBins.Select((RecycleBin bb) => bb.Priority).ToArray();
            int total_value = priorities.Sum();
            float[] probabilities = priorities.Select((int priority) => (priority / (float)total_value)).ToArray();

            RecycleBin selected_bin = validBins[ChooseIndex(probabilities)];

            GameObject clone = selected_bin.Spawn(position, rotation);

            int _id = clone.GetInstanceID();

            if (clone != null && !ObjectPoolData.ContainsKey(_id))
                ObjectPoolData.Add(_id, selected_bin.Label);

            return clone;
        }

        public void RecycleGameObject(GameObject obj)
        {
            string recycleBinIndex;
            if (ObjectPoolData.TryGetValue(obj.GetInstanceID() , out recycleBinIndex))
            {
                runtimeRecycleBins[recycleBinIndex].Recycle(obj);
            }
        }

        public bool IsOnObjectPool(GameObject obj)
        {
            return ObjectPoolData.ContainsKey(obj.GetInstanceID());
        }

        public RecycleBin GetRecycleBin(GameObject obj)
        {
            if (!obj) return null;

            string rid;
            if (ObjectPoolData.TryGetValue(obj.GetInstanceID(), out rid))
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

        public void CreateObjectPool(GameObject[] prefabs, string objectPoolKey, int preallocateCount = 0, Transform parent = null,
            bool forcePoolParent = true)
        {
            if (!ObjectPoolExists(objectPoolKey))
            {
                RecycleBin recycleBin = new RecycleBin(objectPoolKey, prefabs , preallocateCount , parent , forcePoolParent);
                
                if(preallocateCount > 0)
                {
                   StartCoroutine(recycleBin.Allocate());
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