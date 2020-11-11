using Nitro.Pooling;
using UnityEngine;

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

        public void OnSpawn() { }

        public void OnRecycle()
        {
            rb.velocity = Vector3.zero;
        }
    }
}