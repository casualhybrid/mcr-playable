using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "ParticleInstantiationChannel", menuName = "ScriptableObjects/ParticleSystem/ParticleInstantiationChannel")]
public class ParticleInstantiationChannel : ScriptableObject
{
    public readonly UnityEvent<ParticleInstantiateInfo> OnParticleSpawnRequest = new UnityEvent<ParticleInstantiateInfo>();

    public void SendParticleSpawnRequest(ParticleInstantiateInfo instantiateInfo)
    {
        OnParticleSpawnRequest.Invoke(instantiateInfo);
    }
}
