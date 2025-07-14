using DG.Tweening;
using System;
using UnityEngine;

public class GhostDashAnimation : TutorialGhostSegmentAnimation
{
    [SerializeField] private float dashDistance = 11.87f;
    [SerializeField] private SpeedHandler speedHandler;
    [SerializeField] private PlayerContainedData playerContainedData;
    [SerializeField] private int maxCarsCanDestroy = 1;

    private int carsDestroyed;
    private bool isInitialized;

    public override void DoAnimation(Action OnComplete)
    {
        isInitialized = true;
        transform.DOKill();
        float dashSpeed = speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(playerContainedData.PlayerData.PlayerInformation[0].Boosterspeed);
        rb.DOMoveZ(dashDistance, dashSpeed / Time.fixedDeltaTime).SetEase(Ease.Linear).SetUpdate(UpdateType.Fixed).SetSpeedBased(true).SetRelative(true).OnComplete(() => { OnAnimationDone(); OnComplete(); });
    }

    public override void StopAllAnimations()
    {
        base.StopAllAnimations();
        isInitialized = false;
        carsDestroyed = 0;
    }

    public override void OnAnimationDone()
    {
        base.OnAnimationDone();
        carsDestroyed = 0;
        isInitialized = false;

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isInitialized)
            return;

        if (carsDestroyed >= maxCarsCanDestroy)
            return;

        // UnityEngine.Console.Log($"Ghost collided with {other.gameObject}");

        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacles"))
        {
            ObjectDestruction objectDestruction = other.GetComponent<ObjectDestruction>();

            if (objectDestruction == null)
                return;

            if (!objectDestruction.isDestroyableDuringDash)
                return;

            carsDestroyed++;
            objectDestruction.DestroyCar();
        }
    }
}