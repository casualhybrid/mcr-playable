using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSystemStopped : MonoBehaviour
{
    [SerializeField] private UnityEvent onParticleSystemHasStopped = new UnityEvent();

    public void OnParticleSystemStopped()
    {
        onParticleSystemHasStopped.Invoke();
    }
}
