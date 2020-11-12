using System.Collections;
using UnityEngine;
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
        private string[] keys = default;
        private IEnumerator Start()
        {
            bool isDone = false;
            PoolManager.d_OnPoolDefinitionLoaded onDone = null;
            onDone = () =>
            {
                isDone = true;
                PoolManager.OnPoolDefinitionLoaded -= onDone;
            };

            PoolManager.OnPoolDefinitionLoaded += onDone;

            yield return new WaitUntil(() => isDone);

            while(Application.isPlaying)
            {
                Vector2 circle = Random.insideUnitCircle;
                Vector3 spawnpoint = Origin.position + new Vector3(circle.x * Random.Range(RadiusMin, RadiusMax), 0, circle.y
                    * Random.Range(RadiusMin, RadiusMax)) + Offset;

                Quaternion rot = Quaternion.Euler(Random.insideUnitSphere * Random.Range(0, 360f));

                //  PoolManager.Instance.Spawn(PoolKey, spawnpoint, rot );

                PoolManager.Instance.SpawnWeighted(spawnpoint, rot, keys);
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