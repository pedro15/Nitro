using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using Nitro.Pooling;
using UnityEngine.TestTools;

namespace Nitro.Tests
{
    [TestFixture]
    public class RecycleBinTests
    {

        public class MyTest : MonoBehaviour, IPoolCallbacks
        {
            public void OnRecycle()
            {
                Debug.Log("Recycle!");
            }

            public void OnSpawn()
            {
                Debug.Log("Spawn");
            }
        }

        private Dictionary<GameObject, IPoolCallbacks[]> callbacks_db = new Dictionary<GameObject, IPoolCallbacks[]>();

        private Stack<GameObject> go_stack = new Stack<GameObject>();

        [OneTimeSetUp]
        public void Setup()
        {
            callbacks_db = new Dictionary<GameObject, IPoolCallbacks[]>();
            go_stack = new Stack<GameObject>();

            for (int i = 0; i < 10; i++)
            {
                GameObject clone = new GameObject("Test-" + i , typeof(MyTest));
                go_stack.Push(clone);
                callbacks_db.Add(clone, clone.GetComponents<IPoolCallbacks>());
            }
        }

        [UnityTest]
        public IEnumerator TestCallbackdb()
        {
            GameObject clone = go_stack.Pop();
            Assert.AreEqual(true, callbacks_db.TryGetValue(clone, out IPoolCallbacks[] cc));
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestCallbackImplementation()
        {
            GameObject go = go_stack.Pop();
            
            if (callbacks_db.TryGetValue(go, out IPoolCallbacks[] callbacks))
            {
                Assert.AreEqual(1, callbacks.Length);

                foreach (IPoolCallbacks poolCallbacks in callbacks)
                {
                    poolCallbacks.OnRecycle();
                    Assert.IsTrue(true);
                }
            }else
            {
                Assert.IsTrue(false);
            }

            yield return null;
        }
    }
}