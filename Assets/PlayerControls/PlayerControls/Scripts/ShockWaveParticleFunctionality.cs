using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ParticleSystem))]
public class ShockWaveParticleFunctionality : MonoBehaviour
{
    //  [SerializeField] private PlayerData playerData;
    // [SerializeField] private PlayerSharedData playerSharedData;
    [SerializeField] private ParticleSystem parentParticleSystem;

    [SerializeField] private UnityEvent OnShockCompleted;
    [SerializeField] private GameEvent shockaveDiedoutEvent;
    [SerializeField] private float shockWaveHeightForLegitDestruction = 0.2f;
    //  [SerializeField] private Vector3 overLapBoxSizeForStackDestruction;

    private ParticleSystem theParticleSystem;
    private float radius = -1;
    private ParticleSystem.Particle[] particles;
    private Vector3 startingStartSizeMultipliers;
    private float startingLifeTime;

    private readonly HashSet<Collider> detectedColliders = new HashSet<Collider>();

    private void Awake()
    {
        theParticleSystem = GetComponent<ParticleSystem>();

        ParticleSystem.MainModule mainModule = theParticleSystem.main;

        startingStartSizeMultipliers.x = mainModule.startSizeXMultiplier;
        startingStartSizeMultipliers.y = mainModule.startSizeYMultiplier;
        startingStartSizeMultipliers.z = mainModule.startSizeZMultiplier;

        startingLifeTime = mainModule.startLifetimeMultiplier;
    }

    private void OnEnable()
    {
        float multiplier = SpeedHandler.GameTimeScale;
        ParticleSystem.MainModule mainModule = theParticleSystem.main;

        // Resetting Values
        radius = -1;
        particles = null;
        detectedColliders.Clear();

        mainModule.startSizeXMultiplier = startingStartSizeMultipliers.x;
        mainModule.startSizeYMultiplier = startingStartSizeMultipliers.y;
        mainModule.startSizeZMultiplier = startingStartSizeMultipliers.z;

        mainModule.startLifetimeMultiplier = startingLifeTime;

        mainModule.startSizeXMultiplier = mainModule.startSizeX.constant * multiplier;
        mainModule.startSizeYMultiplier = mainModule.startSizeY.constant * multiplier;
        mainModule.startSizeZMultiplier = mainModule.startSizeZ.constant * multiplier;

        mainModule.startLifetimeMultiplier = mainModule.startLifetime.constant * multiplier;

        parentParticleSystem.Play();
    }

    private void OnDisable()
    {
        detectedColliders.Clear();
    }

    public void OnParticleSystemStopped()
    {
        parentParticleSystem.Stop();
        shockaveDiedoutEvent.RaiseEvent();
        OnShockCompleted.Invoke();
    }

    private void FixedUpdate()
    {
        if (!theParticleSystem.isPlaying)
        {
            radius = -1;
            return;
        }

        particles = new ParticleSystem.Particle[theParticleSystem.main.maxParticles];
        theParticleSystem.GetParticles(particles);

        if (particles.Length <= 0)
        {
            radius = -1;
            return;
        }

        //// If the car has landed on something that's destructible via shockwave, destroy each car beneath it (like a stack)
        //Collider contactCollider = playerSharedData.CurrentGroundColliderPlayerIsInContactWith;

        //// Caution! Could be unreliable
        //ObjectDestruction objDestruction = contactCollider.transform.parent.GetComponentInChildren<ObjectDestruction>();

        //if (objDestruction != null && objDestruction.destroyDuringShockwave)
        //{
        //    Vector3 upperPos = playerSharedData.PlayerTransform.position;
        //    float groundHeight = playerData.PlayerInformation[0].PlayerStartinPosition.y;

        //    float midY = (groundHeight + upperPos.y) * .5f;
        //    Vector3 boxCenter = new Vector3(upperPos.x, midY, upperPos.z);

        //    overLapBoxSizeForStackDestruction.y = groundHeight + upperPos.y;
        //    //    Gizmos.DrawCube(boxCenter, size);

        //    Collider[] colliders = Physics.OverlapBox(boxCenter, overLapBoxSizeForStackDestruction * .5f, Quaternion.identity, 1 << LayerMask.NameToLayer("Obstacles"), QueryTriggerInteraction.Collide);
        //    var list = colliders.OrderBy((x) => upperPos.y - x.bounds.center.y).TakeWhile((x) =>
        //    {
        //        var od = x.GetComponent<ObjectDestruction>();
        //        return od != null && od.destroyDuringShockwave;
        //    }

        //    );

        //    foreach (var item in list)
        //    {
        //        var od = item.GetComponent<ObjectDestruction>();
        //        od.DestroyCar();
        //    }
        //}

        ParticleSystem.Particle particle = particles[0];
        radius = theParticleSystem.trigger.radiusScale * particle.GetCurrentSize(theParticleSystem);

        //  UnityEngine.Console.Log("Radius For Trigger Is " + radius);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, 1 << LayerMask.NameToLayer("Obstacles"));

        for (int k = 0; k < hitColliders.Length; k++)
        {
            Collider collider = hitColliders[k];

            var destructibleObjects = collider.GetComponents<IDestructibleObstacle>();

            for (int o = 0; o < destructibleObjects.Length; o++)
            {
                var destructibleObject = destructibleObjects[o];

                if (destructibleObject != null && destructibleObject.isDestroyDuringShockwave)
                {
                    if (detectedColliders.Contains(collider))
                        continue;

                    detectedColliders.Add(collider);

                    float y = Mathf.Abs(collider.bounds.min.y - transform.position.y);

                    if (y > shockWaveHeightForLegitDestruction)
                    {
                        UnityEngine.Console.Log($"The GameObject won't be destroyed by shockwave {destructibleObject}");
                        continue;
                    }

                    destructibleObject.HandleGotHitByShockWave();
                }
            }
        }
    }

    //private void OnDrawGizmos()
    //{
    //    if (radius == -1)
    //        return;

    //    Gizmos.DrawSphere(transform.position, radius);
    //}
}