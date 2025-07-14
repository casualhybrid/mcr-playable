using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MysteryBoxTriggerAnimation : MonoBehaviour
{
    [SerializeField] private UnityEvent OnAnimationFinished;

    #region Variables
    [Header("Trigger Particles")]
    [SerializeField] private Transform _triggerParticleSystemParentT;

    [Header("Rotation Animation")]
    [SerializeField] private float _rotationAmountY = 270f;
    [SerializeField] private float _rotationDuration = 0.375f;
    [SerializeField] private float _easingPowerForRotation = 1.5f;

    [Header("Movement Animation")]
    [SerializeField] private float movementAmountY = 0.5f;
    [SerializeField] private float _movementDuration = 0.375f;
    [SerializeField] private float _easingPowerForMovement = 1.5f;

    [Header("Scale Out Animation")]
    [SerializeField] private float _bounceScaleInDuration = 0.075f;
    [SerializeField] private float _bounceScaleOutDuration = 0.3f;
    [SerializeField] private float _bounceScaleMultiplier = 1.2f;
    [SerializeField] private float _easingPowerForScaleOut = 1.5f;
    #endregion

    private ParticleSystem _triggerParticleSystem;

    #region Trigger
    public void PlayAnimation()
    {
        PlayTriggerParticles();
        StartCoroutine(RotateToTargetRotation());
        StartCoroutine(MoveToTargetPosition());
        StartCoroutine(ScaleOut());
    }
    #endregion

    #region Particles
    private void PlayTriggerParticles()
    {
        if(_triggerParticleSystem == null)
        {
            _triggerParticleSystem = _triggerParticleSystemParentT.GetComponentInChildren<ParticleSystem>();
        }

        if (_triggerParticleSystem != null)
        {
            _triggerParticleSystem.transform.position = transform.position;
            _triggerParticleSystem.Play();
        }
    }
    #endregion

    #region Animations
    IEnumerator RotateToTargetRotation()
    {
        float lerp = 0;
        Vector3 initialEulerAngles = transform.localEulerAngles;
        Vector3 targetEulerAngles = transform.localEulerAngles + (Vector3.up * _rotationAmountY);

        while (transform.localEulerAngles != targetEulerAngles)
        {
            transform.localEulerAngles = Vector3.Lerp(initialEulerAngles, targetEulerAngles, Mathf.Pow(lerp, _easingPowerForRotation));

            lerp += Time.deltaTime / _rotationDuration;

            if (lerp > 1)
                lerp = 1;

            yield return null;
        }
    }

    IEnumerator MoveToTargetPosition()
    {
        float lerp = 0;
        Vector3 initialPos = transform.localPosition;
        Vector3 targetPos = transform.localPosition + (Vector3.up * movementAmountY);

        while (transform.localPosition != targetPos)
        {
            transform.localPosition = Vector3.Lerp(initialPos, targetPos, Mathf.Pow(lerp, _easingPowerForMovement));

            lerp += Time.deltaTime / _movementDuration;

            if (lerp > 1)
                lerp = 1;

            yield return null;
        }
    }

    IEnumerator ScaleOut()
    {
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

        while (initialScale != Vector3.zero)
        {
            initialScale = Vector3.Lerp(initialScale, Vector3.zero, Mathf.Pow(lerp, _easingPowerForScaleOut));
            transform.localScale = initialScale;

            lerp += Time.deltaTime / _bounceScaleOutDuration;

            if (lerp > 1)
                lerp = 1;

            yield return null;
        }

       
        OnAnimationFinished.Invoke();
    }
    #endregion
}
