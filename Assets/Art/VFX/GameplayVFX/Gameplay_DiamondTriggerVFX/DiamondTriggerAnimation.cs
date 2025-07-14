using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

public class DiamondTriggerAnimation : MonoBehaviour
{
    [SerializeField] private UnityEvent OnAnimationFinished;

    #region Variables
    [Header("Rotation Animation")]
    [SerializeField] private float _rotationAmountY = 360f;
    [SerializeField] private float _rotationDuration = 5f;
    [SerializeField] private float _easingPowerForRotation = 4f;

    [Header("Movement Animation")]
    [SerializeField] private GameObject _movementTargetPoint;
    [SerializeField] private float _movementDuration = 1f;
    [SerializeField] private float _easingPowerForMovement = 3f;

    [Header("Scale Out Animation")]
    [SerializeField] private float _bounceScaleInDuration = 0.1f;
    [SerializeField] private float _bounceScaleOutDuration = 0.2f;
    [SerializeField] private float _bounceScaleMultiplier = 1.2f;
    [SerializeField] private float _easingPowerForScaleOut = 3f;

    [Header("Particle Explosion")]
    [SerializeField] private Transform _explosionParticleSystemParent;
    [SerializeField] private float _explosionDelay = 0f;

    [Header("General")]
    [SerializeField] PlayerSharedData PlayerSharedData;

    private ParticleSystem _explosionParticleSystem;
    #endregion

    #region Trigger
    public void PlayAnimation()
    {
        if (PlayerSharedData.DiamondTargetPoint != null)
        {
            _movementTargetPoint = PlayerSharedData.DiamondTargetPoint.gameObject;
        }
        else
        {
            UnityEngine.Console.LogWarning("_movementTargetPoint is null");
        }

        StartCoroutine(PlayExplosionParticles());
        StartCoroutine(RotateToTargetRotation());
        StartCoroutine(MoveToTargetPosition());
        StartCoroutine(ScaleOut());
        StartCoroutine(OnAnimationComplete());

        //SendCollectionEvent();
    }
    #endregion

    #region Particles
    IEnumerator PlayExplosionParticles()
    {
        yield return new WaitForSeconds(_explosionDelay + (_movementDuration + _bounceScaleInDuration + _bounceScaleOutDuration));

        if(_explosionParticleSystem == null)
        {
            _explosionParticleSystem = _explosionParticleSystemParent.GetComponentInChildren<ParticleSystem>();
        }

        if (_explosionParticleSystem != null)
        {
            _explosionParticleSystem.transform.SetParent(PlayerSharedData.DiamondTargetPoint);
            _explosionParticleSystem.transform.position = transform.position;
            _explosionParticleSystem.Play();
        }
    }
    #endregion

    #region Animations
    IEnumerator RotateToTargetRotation()
    {
        float lerp = 0;
        Vector3 initialEulerAngles = transform.localEulerAngles;
        Vector3 targetEulerAngles = initialEulerAngles + (transform.up * _rotationAmountY);

        while (transform.localEulerAngles != targetEulerAngles)
        {
            transform.localEulerAngles = Vector3.Lerp(initialEulerAngles, targetEulerAngles, 1 - Mathf.Pow((1 - lerp), _easingPowerForRotation));

            lerp += Time.deltaTime / _rotationDuration;

            if (lerp > 1)
                lerp = 1;

            yield return null;
        }
    }

    IEnumerator MoveToTargetPosition()
    {
        float lerp = 0;
        Vector3 initialPos = transform.position;
        Vector3 initialScale = transform.localScale;

        while (transform.position != _movementTargetPoint.transform.position)
        {
            transform.position = Vector3.Lerp(initialPos, _movementTargetPoint.transform.position, 1 - Mathf.Pow((1 - lerp), _easingPowerForMovement));

            lerp += Time.deltaTime / _movementDuration;

            if (lerp > 1)
                lerp = 1;

            yield return null;
        }
    }

    IEnumerator ScaleOut()
    {
        yield return new WaitForSeconds(_movementDuration);

        float lerp = 0;
        Vector3 initialScale = transform.localScale;
        Vector3 targetScale = initialScale * _bounceScaleMultiplier;

        while (transform.localScale != targetScale)
        {
            transform.localScale = Vector3.Lerp(initialScale, targetScale, 1 - Mathf.Pow((1 - lerp), _easingPowerForScaleOut));

            lerp += Time.deltaTime / _bounceScaleInDuration;

            if (lerp > 1)
                lerp = 1;

            yield return null;
        }

        lerp = 0;
        initialScale = transform.localScale;

        while (transform.localScale != Vector3.zero)
        {
            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, Mathf.Pow(lerp, _easingPowerForScaleOut));

            lerp += Time.deltaTime / _bounceScaleOutDuration;

            if (lerp > 1)
                lerp = 1;

            yield return null;
        }
    }
    #endregion

    #region Post-Animation
    IEnumerator OnAnimationComplete()
    {
        yield return new WaitForSeconds(_explosionDelay + (_movementDuration + _bounceScaleInDuration + _bounceScaleOutDuration) + 3 /* Particle System Duration */);

        OnAnimationFinished.Invoke();
    }
    #endregion
}
