using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nitro.Pooling;

namespace Nitro.Demo.ObjectPool
{
    public class Ball : MonoBehaviour, IPoolCallbacks
    {
        [SerializeField]
        private int HitsToRecycle = 2;

        public int _hits = 0;

        private Rigidbody rb = default;

        private void OnEnable()
        {
            _hits = 0;
            if (!rb) rb = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Finish"))
                _hits++;

            if (_hits >= HitsToRecycle) gameObject.Recycle();
        }

        public void OnSpawn()
        {
            Vector3 scale = Vector3.Scale(transform.localScale, Vector3.one * 1.07f);            
            transform.localScale = scale;
        }

        public void OnRecycle()
        {
            rb.velocity = Vector3.zero;
        }
    }
}