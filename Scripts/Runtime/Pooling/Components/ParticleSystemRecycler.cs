using System.Collections;
using UnityEngine;

namespace Nitro.Pooling
{
    /// <summary>
    /// Component for Auto Stopping/Playing ParticleSystems when is on a Object Pool
    /// </summary>
    [AddComponentMenu("GoRecycler/ParticleSystem Recycler")]
    public class ParticleSystemRecycler : MonoBehaviour,IPoolCallbacks
    {
        /// <summary>
        /// Should Autoplay on Spawn ?
        /// </summary>
        [SerializeField]
        private bool AutoPlay = true;

        [SerializeField,Tooltip("0 = no TimeOut")]
        public float TimeOut = 0;

        private ParticleSystem particles = null;

        private void Awake()
        {
            particles = GetComponent<ParticleSystem>();
        }

        private void OnEnable()
        {
            if (TimeOut > 0)
            {
                StartCoroutine(KillParticles());
            }
        }

        IEnumerator KillParticles()
        {
            yield return new WaitForSeconds(TimeOut);
            gameObject.Recycle();
            yield break;
        }

        public void OnSpawn()
        {
            if (AutoPlay) particles.Play();
            ParticleSystem[] childparticles = particles.GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < childparticles.Length; i++) childparticles[i].Play();
        }

        public void OnRecycle()
        {
            if (particles.isPlaying) particles.Stop();
            ParticleSystem[] childparticles = particles.GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < childparticles.Length; i++) childparticles[i].Stop();
        }
    }
}