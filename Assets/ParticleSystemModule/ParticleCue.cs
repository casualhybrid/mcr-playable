using System.Collections.Generic;
using UnityEngine;

public class ParticleCue : MonoBehaviour
{
    [Header("Particle definition")]
    [SerializeField] private ParticleCueSO _particleCue = default;

    [SerializeField] private bool _playOnStart = false;

    [SerializeField] private Transform transformToPlayIn;
    [SerializeField] private bool onlyAssignParentPos;

    [SerializeField] private Vector3 position = Vector3.zero;

    [Header("Configuration")]
    [SerializeField] private ParticleChannelEventSO _particleCueEventChannel = default;

    [SerializeField] private GameEvent[] gameEvents;

    private readonly List<InstanceIDUnique> particlesSpawned = new List<InstanceIDUnique>();

    private void Awake()
    {
        //if (transformToPlayIn == null)
        //{
        //    transformToPlayIn = this.transform;
        //}

        for (int i = 0; i < gameEvents.Length; i++)
        {
            gameEvents[i].TheEvent.AddListener(PlayParticleCue);
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < gameEvents.Length; i++)
        {
            gameEvents[i].TheEvent.RemoveListener(PlayParticleCue);
        }
    }

    private void Start()
    {
        if (_playOnStart)
            PlayParticleCue(null);
    }

    public void PlayParticleCue()
    {
      //  UnityEngine.Console.Log("Playng Particle Explosion");
        _particleCueEventChannel.RaiseEvent(particlesSpawned, _particleCue, transformToPlayIn, position, onlyAssignParentPos);
        particlesSpawned[^1].OnObjectDestroyed += RemoveParticleFromParticlesSpawnedList;
    }

    public void PlayParticleCue(GameEvent theEvent)
    {
        _particleCueEventChannel.RaiseEvent(particlesSpawned, _particleCue, transformToPlayIn, position, onlyAssignParentPos);
        particlesSpawned[^1].OnObjectDestroyed += RemoveParticleFromParticlesSpawnedList;
    }

    private void RemoveParticleFromParticlesSpawnedList(InstanceIDUnique instanceIDUnique)
    {
        particlesSpawned.Remove(instanceIDUnique);
    }

    public void StopAllParticlesInCue()
    {
        for (int i = particlesSpawned.Count - 1; i >= 0; i--)
        {
            particlesSpawned[i].SendCompletionEvent();
        }
    }
}