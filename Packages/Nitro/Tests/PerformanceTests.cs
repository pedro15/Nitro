using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Nitro.Tests
{
    [TestFixture]
    public class PerformanceTests
    {
        private Dictionary<GameObject, int> GoDict = new Dictionary<GameObject, int>();

        private Dictionary<int, int> IdDict = new Dictionary<int, int>();

        [OneTimeSetUp]
        public void Setup()
        {
            for (int i = 0; i < 1200; i++)
            {
                GoDict.Add(new GameObject(), Random.Range(-1000, 1000));
            }

            for (int i = 0; i < 1200; i++)
            {
                IdDict.Add(new GameObject().GetInstanceID(), Random.Range(-1000, 1000));
            }
        }

        [UnityTest]
        public IEnumerator TestPerformance()
        {
            GameObject test_gameObject = new GameObject("Test!");
            GoDict.Add(test_gameObject, Random.Range(-1000, 1000));
            
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
            if (GoDict.TryGetValue(test_gameObject , out int myval))
            {
                stopwatch.Stop();
                Debug.Log("gameObject Dictionary Delay: " + stopwatch.ElapsedTicks + " ticks ");
            }
            else
            {
                Assert.IsTrue(false);
            }

            stopwatch.Reset();
            IdDict.Add(test_gameObject.GetInstanceID(), Random.Range(-1000, 1000));

            stopwatch.Start();

            if (IdDict.TryGetValue(test_gameObject.GetInstanceID() , out int myyval))
            {
                stopwatch.Stop();
                Debug.Log("ID Dictionary Delay: " + stopwatch.ElapsedTicks + " ticks ");
            }else
            {
                Assert.IsTrue(false);
            }

            Assert.IsTrue(true);
            stopwatch.Reset();

            yield return null;
        }

        [UnityTest]
        public IEnumerator TestPerformance2()
        {
            GameObject obj = new GameObject();
            Stopwatch sw = new Stopwatch();

            sw.Start();

            bool is_null =  ReferenceEquals(obj, null);
            
            sw.Stop();

            if (is_null) Assert.IsFalse(is_null);

            Debug.Log("| ReferenceEquals(obj, null); | ->  Delay: " + sw.ElapsedTicks + " Ticks");

            sw.Start();

            bool is_null2 = !obj;

            sw.Stop();

            if (is_null2) Assert.IsFalse(is_null2);

            Debug.Log("| !obj | -> Delay: " + sw.ElapsedTicks + " Ticks");

            yield return null;
        }
    }
}