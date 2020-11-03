using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if ADDRESSABLES_INSTALLED
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

namespace Nitro.Pooling
{
    [System.Serializable]
    public sealed class RecycleBin
    {
        public delegate void RecycleBinDelegate(GameObject obj, string recyclebinLabel);

        public static event RecycleBinDelegate OnSpawn;

        public static event RecycleBinDelegate OnRecycle;

        #region Fields

        public string Label { get { return label; } }

        /// <summary>
        /// Object pool label
        /// </summary>
        [SerializeField]
        private string label = default;

        /// <summary>
        /// Pool Max. Items. 0 or less means Dynamic Pool. 
        /// </summary>
        public int MaxItems = 0;

        /// <summary>
        /// How much elements must be allocated on initialization ?
        /// </summary>
        [SerializeField]
        private int preAllocateCount = 5;

#if ADDRESSABLES_INSTALLED
        [SerializeField]
        private AssetReference Prefab_ref = default;
        [SerializeField]
        private AssetLabelReference Prefabs_label = default;
        private List<IResourceLocation> prefab_locations = new List<IResourceLocation>();
#endif
        /// <summary>
        /// Object Pool Prefab
        /// </summary>
        private GameObject Prefab = default;
        /// <summary>
        /// Object Pool Parent
        /// </summary>
        [HideInInspector]
        private Transform PoolParent = default;
        /// <summary>
        /// Should a Object Pool Parent needs to be created if the default pool parent does not exists?
        /// </summary>
        [SerializeField]
        private bool ForcePoolParent = true;

        public int PreAllocateCount => preAllocateCount;
        public PoolReferenceType referenceType { get; private set; }

        [System.NonSerialized]
        private Stack<GameObject> PooledObjects = new Stack<GameObject>();

        #endregion

#if ADDRESSABLES_INSTALLED
        public RecycleBin(string _label, AssetReference _prefab, int _MaxItems, int _preallocateCount = 0,
           Transform _parent = null, bool _forcePoolParent = true)
        {
            label = _label;
            Prefab_ref = _prefab;
            PoolParent = _parent;
            MaxItems = _MaxItems;
            preAllocateCount = _preallocateCount;
            ForcePoolParent = _forcePoolParent;

            referenceType = PoolReferenceType.ASSET_REFERENCE;
            Reset();
        }

        public RecycleBin(string _label, AssetLabelReference _prefab, int _MaxItems, int _preallocateCount = 0,
           Transform _parent = null, bool _forcePoolParent = true)
        {
            label = _label;
            Prefabs_label = _prefab;
            PoolParent = _parent;
            MaxItems = _MaxItems;
            preAllocateCount = _preallocateCount;
            ForcePoolParent = _forcePoolParent;

            referenceType = PoolReferenceType.LABEL_REFERENCE;
            Reset();
        }
#endif
        public RecycleBin(string _label , GameObject _prefab ,int _MaxItems ,int _preallocateCount = 0 ,
            Transform _parent = null, bool _forcePoolParent = true)
        {
            label = _label;
            Prefab = _prefab;
            PoolParent = _parent;
            MaxItems = _MaxItems;
            preAllocateCount = _preallocateCount;
            ForcePoolParent = _forcePoolParent;

            referenceType = PoolReferenceType.PREFAB;
            Reset();
        }

        #region Public API 

        private int _objectCount = 0;

        /// <summary>
        /// Returns the object pool size
        /// </summary>
        public int ObjectCount
        {
            get
            {
                return _objectCount;
            }
        }

        /// <summary>
        /// Iterator for fill the object pool
        /// </summary>
        /// <returns></returns>
        private IEnumerator I_FillPool(int count)
        {
            for (int i = 0; i < count; i++)
            {
#if ADDRESSABLES_INSTALLED
                var async_process = RegisterPrefabAsync();
                do
                {
                    Debug.Log($"Prefab Created[{i}]: " + async_process.IsCompleted);
                    yield return new WaitForEndOfFrame();
                } while (!async_process.IsCompleted);

                Debug.Log("Count: " + ObjectCount);
#else
                RegisterPrefab();
                yield return new WaitForEndOfFrame();
#endif
            }
        }

        /// <summary>
        /// Fills The objectPool
        /// </summary>
        public IEnumerator FillPool(int count)
        {
            if (PreAllocateCount > 0)
            {
#if ADDRESSABLES_INSTALLED
                bool done = false;
                if (referenceType == PoolReferenceType.LABEL_REFERENCE)
                {
                    Addressables.LoadResourceLocationsAsync(Prefabs_label.labelString).Completed +=
                        (AsyncOperationHandle<IList<IResourceLocation>> locations) =>
                        {
                            prefab_locations = locations.Result.ToList();
                            Addressables.LoadAssetsAsync<GameObject>(locations , null).Completed +=
                                (AsyncOperationHandle<IList<GameObject>> handle) =>
                                {
                                    done = true;
                                };
                        };
                }else if (referenceType == PoolReferenceType.ASSET_REFERENCE)
                {
                    Addressables.LoadAssetAsync<GameObject>(Prefab_ref).Completed +=
                        (AsyncOperationHandle<GameObject> handle) =>
                        {
                            done = true;
                        };
                }
                else
                {
                    done = true;
                }

                yield return new WaitUntil(() => done);

                yield return I_FillPool(count);
#else
                yield return PoolManager.Instance.StartCoroutine(I_FillPool(count));
#endif
            }
        }

        public IEnumerator FillPool()
        {
            yield return FillPool(PreAllocateCount);
        }

        /// <summary>
        /// Resets the recycle bin to it's default values and Destroys the object pool
        /// </summary>
        public void Reset()
        {
            _objectCount = 0;
            ClearRecycleBin(true);
#if ADDRESSABLES_INSTALLED
            prefab_locations.Clear();
#endif
            if (PooledObjects != null)
                PooledObjects.Clear();
            else PooledObjects = new Stack<GameObject>();
        }
        
        /// <summary>
        /// Recycles an object to the object pool
        /// </summary>
        /// <param name="InstanceId">Instance id of the object</param>
        
        public void Recycle(GameObject go )
        {
            if (!go)
            {
                Debug.LogWarning($"[{GetType().Name}] Trying to recycle an null object. ignoring...");
                return;
            }

            if (go != null && !PooledObjects.Contains(go))
            {
                if (OnRecycle != null) OnRecycle.Invoke(go, Label);
                go.SetActive(false);
                go.transform.position = Vector3.zero;
                PooledObjects.Push(go);
            }
        }

#if ADDRESSABLES_INSTALLED
        /// <summary>
        /// Returns a GameObject from the Object pool (asynchronous process for new Object Pool Instances)
        /// </summary>
        /// <param name="Position">Target Position</param>
        /// <param name="Rotation">Target Rotation</param>
        /// <returns>Returns a Gameobject if it is aviable, otherwise returns null</returns>
        public async Task<GameObject> SpawnAsync(Vector3 Position, Quaternion Rotation)
        {
            if (PooledObjects.Count <= 0 && (MaxItems <= 0 || ObjectCount < MaxItems))
            {
                Task<GameObject> task = RegisterPrefabAsync();
                await task;
                GameObject other = task.Result;

                if (other != null)
                {
                    other.transform.SetPositionAndRotation(Position, Rotation);
                    if (OnSpawn != null) OnSpawn.Invoke(other, label);
                    return other;
                }
                else return null;
            }
            else
            {
                return GetFromPool(Position, Rotation);
            }
        }
#endif
        /// <summary>
        /// Returns a GameObject from the Object pool
        /// </summary>
        /// <param name="Position">Target Position</param>
        /// <param name="Rotation">Target Rotation</param>
        /// <returns>Returns a Gameobject if it is aviable, otherwise returns null</returns>
        public GameObject Spawn(Vector3 Position, Quaternion Rotation)
        {
            if (PooledObjects.Count <= 0 && (MaxItems <= 0 || ObjectCount < MaxItems))
            {
#if ADDRESSABLES_INSTALLED
                Task<GameObject> go_process = Task.Run(() => RegisterPrefabAsync(false));

                go_process.Wait();
                GameObject other = go_process.Result;
#else 
                GameObject other = RegisterPrefab(false);
#endif
                if (other != null)
                {
                    other.transform.SetPositionAndRotation(Position, Rotation);
                    if (OnSpawn != null) OnSpawn.Invoke(other, label);
                    return other;
                }
                else return null;
            }
            else
            {
                return GetFromPool(Position, Rotation);
            }
        }

        /// <summary>
        /// Clears the entrie recycle bin
        /// </summary>
        /// <param name="destroyObjects"></param>
        public void ClearRecycleBin(bool destroyObjects = false)
        {
            while (PooledObjects != null  && PooledObjects.Count > 0 )
            {
                GameObject obj = PooledObjects.Pop();
                if (destroyObjects)
                {
                    Object.Destroy(obj);
                    _objectCount--;
                }
            }
        }

#endregion

#region Private API 
        
        /// <summary>
        /// Returns a GameObject from object pool and enables it
        /// </summary>
        /// <param name="pos">New Position</param>
        /// <param name="rot">New Rotation</param>
        private GameObject GetFromPool(Vector3 pos , Quaternion rot )
        {
            if (PooledObjects.Count > 0)
            {
                GameObject g = PooledObjects.Pop();
                g.transform.position = pos;
                g.transform.rotation = rot;
                g.SetActive(true);

                if (OnSpawn != null) OnSpawn.Invoke(g,Label);
                return g;
            }
            return null; 
        }

#if !ADDRESSABLES_INSTALLED
        /// <summary>
        /// Creates the prefab clone and adds it to the Object Pool
        /// </summary>
        /// <returns>Prefab clone</returns>
        internal GameObject RegisterPrefab(bool recycle = true)
        {
            if (Prefab != null && (ObjectCount < MaxItems || MaxItems <= 0) )
            {
                GameObject Clone = Object.Instantiate(Prefab, Vector3.zero, Quaternion.identity);

                if (PoolParent != null && PoolParent.gameObject.scene.IsValid())
                {
                    Clone.transform.SetParent(PoolParent);
                }
                else
                {
                    PoolParent = (new GameObject(label + "-PoolParent")).transform;
                    PoolParent.transform.SetParent(PoolManager.Instance.transform);
                    Clone.transform.SetParent(PoolParent);
                }
                if (recycle) Recycle(Clone);
                _objectCount++;
                return Clone;
            }
            return null;
        }
#else
        /// <summary>
        /// Creates the prefab clone and adds it to the Object Pool
        /// </summary>
        /// <returns>Prefab clone Task</returns>
        internal async Task<GameObject> RegisterPrefabAsync (bool recycle = true)
        {
            switch (referenceType)
            {
                case PoolReferenceType.PREFAB:
                    
                    if (!Prefab)
                    {
                        Debug.LogError($"[{GetType().Name}] Prefab is null on pool: '{Label}'");
                        return await new Task<GameObject>(() => null);
                    }
                    break;
                case PoolReferenceType.ASSET_REFERENCE:
                    if (!Prefab_ref.RuntimeKeyIsValid())
                    {
                        Debug.LogError($"[{GetType().Name}] Asset reference does not contain any valid key on pool: '{Label}'");
                        return await new Task<GameObject>(() => null);
                    }
                    break;

                case PoolReferenceType.LABEL_REFERENCE:

                    if (!Prefabs_label.RuntimeKeyIsValid())
                    {
                        Debug.LogError($"[{GetType().Name}] Asset label reference does not cotain any valid key on pool: {label}");
                        return await new Task<GameObject>(() => null);
                    }
                    break;
                default: goto case PoolReferenceType.PREFAB;
            }

            if (ObjectCount < MaxItems || MaxItems <= 0)
            {
                if (!PoolParent)
                {
                    PoolParent = new GameObject($"Pool :: {Label}").transform;
                    PoolParent.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                }

                object key = null;

                switch (referenceType)
                {
                    case PoolReferenceType.PREFAB:

                        key = Prefab;

                        break;
                    case PoolReferenceType.ASSET_REFERENCE:

                        key = Prefab_ref;

                        break;

                    case PoolReferenceType.LABEL_REFERENCE:

                        key = prefab_locations[Random.Range(0, prefab_locations.Count)];

                        break;
                    default: goto case PoolReferenceType.PREFAB;
                }

                if (key == null)
                {
                    Debug.LogError($"[{GetType().Name}] - {Label} | Null Object Pool Key. Prefab Missing?");
                    throw new System.ArgumentNullException($"[{GetType().Name}] - {Label} | Null key");
                }

                var _process = Addressables.InstantiateAsync(key, Vector3.zero, Quaternion.identity, PoolParent);

                System.Action<AsyncOperationHandle<GameObject>> Handler = null;

                Handler = (AsyncOperationHandle<GameObject> handle) =>
                {
                    GameObject clone = handle.Result;

                    if (PoolParent != null && PoolParent.gameObject.scene.IsValid())
                    {
                        clone.transform.SetParent(PoolParent);
                    }else
                    {
                        PoolParent = (new GameObject(label + "-PoolParent")).transform;
                        PoolParent.transform.SetParent(PoolManager.Instance.transform);
                        clone.transform.SetParent(PoolParent);
                    }

                    if (recycle) Recycle(clone);
                    _objectCount++;
                    _process.Completed -= Handler;
                };

                _process.Completed += Handler;

                return await _process.Task;
            }else
            {
                Debug.LogWarning($"[{GetType().Name}] Pre-allocate limit reached! Please enable Dynamic pool if you want to keep creating instances even when limit is reached.");
                return null;
            }
        }
#endif
        #endregion

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj is RecycleBin)
            {
                RecycleBin other = (RecycleBin)obj;
                return other.label.Equals(label) && other.PreAllocateCount.Equals(PreAllocateCount);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return label.GetHashCode() + PreAllocateCount.GetHashCode() + 2;
        }
    }
}