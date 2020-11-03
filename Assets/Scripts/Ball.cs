using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nitro.Pooling;

namespace Nitro.Demo.ObjectPool
{
    public class Ball : MonoBehaviour
    {
        [SerializeField]
        private int HitsToRecycle = 2;

        public int _hits = 0;
        private void OnEnable()
        {
            _hits = 0;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Finish"))
                _hits++;

            if (_hits >= HitsToRecycle) gameObject.Recycle();
        }
    }
}