using System.Collections.Generic;
using UnityEngine;

public class ParticleInstantiateInfo
{
    public GameObject TheParticleObject;
    public Transform TheParentT;
    public bool isValid = true;

    public ParticleInstantiateInfo(GameObject _theParticleObject, Transform _theParentT)
    {
        TheParticleObject = _theParticleObject;
        TheParentT = _theParentT;
    }
}

public class ParticleInstantiationManager : MonoBehaviour
{
    [SerializeField] private ParticleInstantiationChannel particleInstantiationChannel;

    private readonly Queue<ParticleInstantiateInfo> particleInstantiationQueue = new Queue<ParticleInstantiateInfo>();

    private void Awake()
    {
        particleInstantiationChannel.OnParticleSpawnRequest.AddListener(EnqueueParticleInstantiateRequest);
    }

    private void OnDestroy()
    {
        particleInstantiationChannel.OnParticleSpawnRequest.RemoveListener(EnqueueParticleInstantiateRequest);
    }

    public void EnqueueParticleInstantiateRequest(ParticleInstantiateInfo particleInstantiateInfo)
    {
        particleInstantiationQueue.Enqueue(particleInstantiateInfo);
    }

    private void Update()
    {
        if (particleInstantiationQueue.Count == 0)
            return;

        var particleSpawnInfo = particleInstantiationQueue.Dequeue();

        if (!particleSpawnInfo.isValid)
            return;

        GameObject particle = Instantiate(particleSpawnInfo.TheParticleObject, particleSpawnInfo.TheParentT);
        particle.transform.localPosition = Vector3.zero;
        particle.transform.localRotation = Quaternion.identity;
    }
}