using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using Nitro.Pooling;

namespace Nitro.Demo.ObjectPool
{
    public class BallGenerator : MonoBehaviour
    {
        [SerializeField]
        private Transform Origin = default;
        [SerializeField]
        private float RadiusMin = 1f;
        [SerializeField]
        private float RadiusMax = 2f;
        [SerializeField]
        private Vector3 Offset = Vector3.zero;
        [SerializeField]
        private float SpawnTime = 0.15f;
        [SerializeField]
        private string PoolKey = default;

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => PoolManager.Instance.IsInitialized);

            while(Application.isPlaying)
            {
                Vector2 circle = Random.insideUnitCircle;
                Vector3 spawnpoint = Origin.position + new Vector3(circle.x * Random.Range(RadiusMin, RadiusMax), 0, circle.y
                    * Random.Range(RadiusMin, RadiusMax)) + Offset;

                PoolManager.Instance.Spawn(PoolKey, spawnpoint, Quaternion.Euler(Random.insideUnitSphere * Random.Range(0, 360f)));

                yield return new WaitForSeconds(SpawnTime);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (Origin != null )
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(Origin.position + Offset, RadiusMin);
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(Origin.position + Offset, RadiusMax);
            }
        }
    }
}