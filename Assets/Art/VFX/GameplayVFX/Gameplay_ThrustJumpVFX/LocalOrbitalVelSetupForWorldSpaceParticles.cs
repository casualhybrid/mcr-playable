using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalOrbitalVelSetupForWorldSpaceParticles : MonoBehaviour
{
    #region Variables
    private ParticleSystem _selfParticleSystem;
    private ParticleSystem.VelocityOverLifetimeModule velOverLifetimeModule;
    private Coroutine _updateVelocityOverLifetimeOffsetRoutine;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        _selfParticleSystem = GetComponent<ParticleSystem>();
        velOverLifetimeModule = _selfParticleSystem.velocityOverLifetime;
    }
    #endregion

    #region Functionality
    /// <summary>
    /// Only compatable with Particle Systems which use (0, 0, 0) in the Velocity Over Lifetime Offset.
    /// This function needs to be called when the particle system is played.
    /// </summary>
    public void SetLocalOrbitalVel()
    {
        velOverLifetimeModule.orbitalOffsetX = 0;
        velOverLifetimeModule.orbitalOffsetY = 0;
        velOverLifetimeModule.orbitalOffsetZ = 0;

        if (_updateVelocityOverLifetimeOffsetRoutine != null)
            StopCoroutine(_updateVelocityOverLifetimeOffsetRoutine);

        _updateVelocityOverLifetimeOffsetRoutine = StartCoroutine(UpdateVelocityOverLifetimeOffset());
    }

    IEnumerator UpdateVelocityOverLifetimeOffset()
    {
        float updateDuration = _selfParticleSystem.main.duration;
        float initTime = Time.time;
        float currentTime = Time.time - initTime;
        Vector3 psPlayPos = _selfParticleSystem.transform.position;
        Vector3 psOffsetValue = psPlayPos - _selfParticleSystem.transform.position;

        while (currentTime < updateDuration)
        {
            velOverLifetimeModule.orbitalOffsetX = psOffsetValue.x;
            velOverLifetimeModule.orbitalOffsetY = psOffsetValue.y;
            velOverLifetimeModule.orbitalOffsetZ = psOffsetValue.z;

            yield return null;

            currentTime = Time.time - initTime;
            psOffsetValue = psPlayPos - _selfParticleSystem.transform.position;
        }
    }
    #endregion
}
