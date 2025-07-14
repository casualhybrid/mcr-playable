using System.Collections.Generic;
using UnityEngine;

public class ParticlesManager : MonoBehaviour
{
    [Header("Listening on channels")]
    [Tooltip("The ParticleManager listens to this event, fired by objects in any scene, to play Particles")]
    [SerializeField] private ParticleChannelEventSO _ParticleEventChannel = default;

    [SerializeField] private GeneralGameObjectVariancePool particlesPool;
    [SerializeField] private int warmpUpCount;

    private void Awake()
    {
        //TODO: Get the initial volume levels from the settings
        _ParticleEventChannel.OnParticleCueRequested += PlayParticleCue;
    }

    private void Start()
    {
        // Warmup
        particlesPool.Prewarm(warmpUpCount);
    }

    private void OnDestroy()
    {
        _ParticleEventChannel.OnParticleCueRequested -= PlayParticleCue;
    }

    public void PlayParticleCue(List<InstanceIDUnique> _particlesSpawned, ParticleCueSO particleCue, Transform theTransform, Vector3 position = default, bool onlyAssignParentPos = false)
    {
        GameObject[] particlesToPlay = particleCue.GetParticles();

        for (int i = 0; i < particlesToPlay.Length; i++)
        {
            Transform transformToSpawn = onlyAssignParentPos ? null : theTransform;
            Vector3 posToSpawn = onlyAssignParentPos && theTransform ? theTransform.TransformPoint(position) : position;

            InstanceIDUnique originalObjectInstaneIDComponent = particlesToPlay[i].GetComponent<InstanceIDUnique>();
            InstanceIDUnique requestedParticleInstanceIDComponent = particlesPool.Request(originalObjectInstaneIDComponent, originalObjectInstaneIDComponent.InstanceID);
            requestedParticleInstanceIDComponent.OnObjectDestroyed += ReturnParticleBackToPool;
            GameObject theParticle = requestedParticleInstanceIDComponent.gameObject;
            theParticle.transform.SetParent(transformToSpawn);

            //  GameObject theParticle = Instantiate(particlesToPlay[i], transformToSpawn);
            theParticle.transform.localPosition = posToSpawn;

            ParticleSystem particleSystem = theParticle.GetComponent<ParticleSystem>();
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particleSystem.Play(true);

            _particlesSpawned.Add(requestedParticleInstanceIDComponent);
        }
    }


    private void ReturnParticleBackToPool(InstanceIDUnique instanceIDUnique)
    {
        instanceIDUnique.OnObjectDestroyed -= ReturnParticleBackToPool;
        particlesPool.Return(instanceIDUnique);
    }
}