using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePlaceHolder : MonoBehaviour
{
    [SerializeField] private ParticleInstantiationChannel particleInstantiationChannel;
    [SerializeField] private GameObject particleToSpawn;

    //private readonly List<ParticleSystem> particleSystems = new List<ParticleSystem>();

    ParticleInstantiateInfo instantiateInfo;

    private void OnEnable()
    {
        instantiateInfo = new ParticleInstantiateInfo(particleToSpawn, transform);
        particleInstantiationChannel.SendParticleSpawnRequest(instantiateInfo);
        //StartCoroutine(GetChildParticleReferences());
    }

    //private IEnumerator GetChildParticleReferences()
    //{
    //    yield return null;
    //    yield return null;

    //    GetRootParticleSystems();
    //}


    private void OnDisable()
    {
       if(instantiateInfo != null)
        {
            instantiateInfo.isValid = false;
            instantiateInfo = null;
        }

        for (int i = 0; i < transform.childCount; i++)
        {
           Destroy(transform.GetChild(i).gameObject);
        }

        //particleSystems.Clear();
    }

    //public void PlayAllChildParticles()
    //{
    //    if (!GetChildParticles())
    //        return;

    //    for (int i = 0; i < particleSystems.Count; i++)
    //    {
    //        particleSystems[i].Play(true);
    //    }

    //}


    //public void StopAllChildParticles()
    //{
    //    if (!GetChildParticles())
    //        return;

    //    for (int i = 0; i < particleSystems.Count; i++)
    //    {
    //        particleSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
    //    }

    //}

    //private void GetRootParticleSystems()
    //{
    //    for (int i = 0; i < transform.childCount; i++)
    //    {
    //        ParticleSystem pSystem = transform.GetChild(i).GetComponent<ParticleSystem>();

    //        if (pSystem != null)
    //        {
    //            particleSystems.Add(pSystem);
    //        }
    //    }
    //}

    //private bool GetChildParticles()
    //{
    //    if (particleSystems == null || particleSystems.Count == 0)
    //    {
    //        UnityEngine.Console.LogWarning($"Unable to obtain child particles. Trying to find it in the transform {transform.parent.name}");
    //        GetRootParticleSystems();
    //    }
    //    else
    //    {
    //        return true;
    //    }

    //    if (particleSystems == null || particleSystems.Count == 0)
    //    {
    //        UnityEngine.Console.LogWarning($"Failed to find child particles. Object {transform.parent.name}");
    //        return false;
    //    }

    //    return true;
    //}
}
