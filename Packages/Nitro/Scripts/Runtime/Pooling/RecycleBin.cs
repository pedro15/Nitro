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
        #region Fields

        private int priority = 0;

        public int Priority => priority;

        public string Label { get { return label; } }

        /// <summary>
        /// Object pool label
        /// </summary>
        private string label = default;

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
        private GameObject[] Prefabs = default;
        /// <summary>
        /// Object Pool Parent
        /// </summary>
        private Transform PoolParent = default;
        /// <summary>
        /// Should a Object Pool Parent needs to be created if the default pool parent does not exists?
        /// </summary>
        private bool ForcePoolParent = true;

        public int PreAllocateCount => preAllocateCount;
        public PoolReferenceType referenceType { get; private set; }

        [System.NonSerialized]
        private Stack<GameObject> PooledObjects = new Stack<GameObject>();

        private Dictionary<GameObject, IPoolCallbacks[]> Callbacksdb = new Dictionary<GameObject, IPoolCallbacks[]>();

        private int _objectCount = 0;

        #endregion

#if ADDRESSABLES_INSTALLED
        public RecycleBin(string _label, AssetReference _prefab, int _preallocateCount = 0,
           Transform _parent = null, bool _forcePoolParent = true , int _priority = 0)
        {
            label = _label;
            Prefab_ref = _prefab;
            PoolParent = _parent;
            preAllocateCount = _preallocateCount;
            ForcePoolParent = _forcePoolParent;
            priority = _priority;

            referenceType = PoolReferenceType.ASSET_REFERENCE;
            Dispose();
        }

        public RecycleBin(string _label, AssetLabelReference _prefab, int _preallocateCount = 0,
           Transform _parent = null, bool _forcePoolParent = true, int _priority = 0)
        {
            label = _label;
            Prefabs_label = _prefab;
            PoolParent = _parent;
            preAllocateCount = _preallocateCount;
            ForcePoolParent = _forcePoolParent;
            priority = _priority;

            referenceType = PoolReferenceType.LABEL_REFERENCE;
            Dispose();
        }
#endif
        public RecycleBin(string _label, GameObject[] _prefabs, int _preallocateCount = 0,
            Transform _parent = null, bool _forcePoolParent = true , int _priority = 0)
        {
            label = _label;
            Prefabs = _prefabs.Where((GameObject obj) => obj != null ).ToArray();
            PoolParent = _parent;
            preAllocateCount = _preallocateCount;
            ForcePoolParent = _forcePoolParent;
            priority = _priority;

            referenceType = PoolReferenceType.PREFAB;
            Dispose();
        }

        #region Public API 


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
                  yield return new WaitForEndOfFrame();
                } while (!async_process.IsCompleted);
#else
                RegisterPrefab();
                yield return new WaitForEndOfFrame();
#endif
            }
        }

        /// <summary>
        /// Allocates the object pool in memory
        /// </summary>
        public IEnumerator Allocate(int count, System.Action OnFinish)
        {
            if (count > 0)
            {
#if ADDRESSABLES_INSTALLED
                bool done = false;
                if (referenceType == PoolReferenceType.LABEL_REFERENCE)
                {
                    var load_resouces = Addressables.LoadResourceLocationsAsync(Prefabs_label.labelString, typeof(GameObject));

                    yield return new WaitUntil(() => load_resouces.IsDone);

                    prefab_locations = load_resouces.Result.ToList();

                    var load_process = Addressables.LoadAssetsAsync<GameObject>(prefab_locations, null);

                    yield return new WaitUntil(() => load_process.IsDone);

                    done = true;
                }
                else if (referenceType == PoolReferenceType.ASSET_REFERENCE)
                {
                    var load_prefab = Addressables.LoadAssetAsync<GameObject>(Prefab_ref);

                    yield return new WaitUntil(() => load_prefab.IsDone);

                    done = true;
                }
                else
                {
                    done = true;
                }

                yield return new WaitUntil(() => done);

                yield return I_FillPool(count);
#else
                yield return I_FillPool(count);
#endif

                if (OnFinish != null) OnFinish.Invoke();

                yield break;
            }
        }

        public IEnumerator Allocate(System.Action OnFinish = null)
        {
            yield return Allocate(PreAllocateCount, OnFinish);
        }

        /// <summary>
        /// Recycles an object to the object pool
        /// </summary>
        /// <param name="InstanceId">Instance id of the object</param>

        public void Recycle(GameObject go)
        {
            if (!go)
            {
                Debug.LogWarning($"[{GetType().Name}] Trying to recycle a null object. ignoring...");
                return;
            }

            if (!PooledObjects.Contains(go))
            {
                InvokeCallbacks(go, (IPoolCallbacks cc) => cc.OnRecycle());
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
            if (PooledObjects.Count <= 0)
            {
                Task<GameObject> task = RegisterPrefabAsync();
                await task;
                GameObject other = task.Result;

                if (other != null)
                {
                    other.transform.SetPositionAndRotation(Position, Rotation);

                    InvokeCallbacks(other, (IPoolCallbacks cc) => cc.OnSpawn());

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
            if (PooledObjects.Count <= 0)
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

                    InvokeCallbacks(other, (IPoolCallbacks cc) => cc.OnSpawn());

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
        /// Clears and resets the recycleBin to it's default values
        /// </summary>
        /// <param name="destroyObjects"></param>
        public void Dispose(bool destroyObjects = false)
        {
            while (PooledObjects != null && PooledObjects.Count > 0)
            {
                GameObject obj = PooledObjects.Pop();
                if (destroyObjects)
                {
#if ADDRESSABLES_INSTALLED
                    Addressables.ReleaseInstance(obj);
#else
                    Object.Destroy(obj);
#endif
                }
            }
            
            _objectCount = 0;
            if (PooledObjects != null)
                PooledObjects.Clear();
            else PooledObjects = new Stack<GameObject>();
            
            Callbacksdb.Clear();

#if ADDRESSABLES_INSTALLED
            if(prefab_locations.Count > 0)
            {
                for (int i = 0; i < prefab_locations.Count; i++)
                {
                    Addressables.Release(prefab_locations[i]);
                }
                prefab_locations.Clear();
            }
#endif
        }

#endregion

#region Private API 

        private void InvokeCallbacks(GameObject other, System.Action<IPoolCallbacks> invocation)
        {
            if (Callbacksdb.TryGetValue(other, out IPoolCallbacks[] callbacks))
            {
                foreach (IPoolCallbacks poolCallbacks in callbacks)
                {
                    if (invocation != null) invocation.Invoke(poolCallbacks);
                }
            }
        }

        private void AddToCallbacksIfQualifyed(GameObject obj)
        {
            if (!obj) return;

            IPoolCallbacks[] callbacks = obj.GetComponents<IPoolCallbacks>();
            if (callbacks != null && callbacks.Length > 0 && !Callbacksdb.ContainsKey(obj))
                Callbacksdb.Add(obj, callbacks);
        }

        /// <summary>
        /// Returns a GameObject from object pool and enables it
        /// </summary>
        /// <param name="pos">New Position</param>
        /// <param name="rot">New Rotation</param>
        private GameObject GetFromPool(Vector3 pos, Quaternion rot)
        {
            if (PooledObjects.Count > 0)
            {
                GameObject g = PooledObjects.Pop();
                g.transform.position = pos;
                g.transform.rotation = rot;
                g.SetActive(true);

                InvokeCallbacks(g, (IPoolCallbacks cc) => cc.OnSpawn());

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
        internal async Task<GameObject> RegisterPrefabAsync(bool recycle = true)
        {
            void RegistrationPostProcess(GameObject clone)
            {
                clone.transform.SetParent(PoolParent);
                if (recycle) Recycle(clone);
                _objectCount++;
                AddToCallbacksIfQualifyed(clone);
            }

            switch (referenceType)
            {
                case PoolReferenceType.PREFAB:

                    if (Prefabs == null || Prefabs.Length == 0)
                    {
                        Debug.LogError($"[{GetType().Name}] Prefab is null on pool: '{Label}'");
                        return null;
                    }
                    break;
                case PoolReferenceType.ASSET_REFERENCE:
                    if (!Prefab_ref.RuntimeKeyIsValid())
                    {
                        Debug.LogError($"[{GetType().Name}] Asset reference does not contain any valid key on pool: '{Label}'");
                        return null;
                    }
                    break;

                case PoolReferenceType.LABEL_REFERENCE:

                    if (!Prefabs_label.RuntimeKeyIsValid())
                    {
                        Debug.LogError($"[{GetType().Name}] Asset label reference does not cotain any valid key on pool: {label}");
                        return null;
                    }
                    break;
                default: goto case PoolReferenceType.PREFAB;
            }

            if (!PoolParent && ForcePoolParent)
            {
                PoolParent = new GameObject($"Pool :: {Label}").transform;
                PoolParent.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                PoolParent.SetParent(PoolManager.Instance.transform);
            }

            AsyncOperationHandle<GameObject> _process = default;

            switch (referenceType)
            {
                case PoolReferenceType.PREFAB:
                    GameObject clone = Object.Instantiate(Prefabs[Random.Range(0,Prefabs.Length)], Vector3.zero, Quaternion.identity);
                    RegistrationPostProcess(clone);
                    return clone;

                case PoolReferenceType.ASSET_REFERENCE:

                    _process = Addressables.InstantiateAsync(Prefab_ref, Vector3.zero, Quaternion.identity);

                    break;

                case PoolReferenceType.LABEL_REFERENCE:

                    IResourceLocation key = prefab_locations[Random.Range(0, prefab_locations.Count)];

                    _process = Addressables.InstantiateAsync(key, Vector3.zero, Quaternion.identity);

                    break;
                default: goto case PoolReferenceType.PREFAB;
            }

            System.Action<AsyncOperationHandle<GameObject>> Handler = null;

            Handler = (AsyncOperationHandle<GameObject> handle) =>
            {
                GameObject clone = handle.Result;
                RegistrationPostProcess(clone);
                _process.Completed -= Handler;
            };

            _process.Completed += Handler;
            return await _process.Task;
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
            return label.GetHashCode() + PreAllocateCount.GetHashCode() + 4;
        }
    }
}