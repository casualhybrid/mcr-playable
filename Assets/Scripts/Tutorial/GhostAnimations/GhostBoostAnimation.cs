using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostBoostAnimation : TutorialGhostSegmentAnimation
{
    [SerializeField] private float boostDistance = 11.5f;
    [SerializeField] private SpeedHandler speedHandler;
    [SerializeField] private PlayerContainedData playerContainedData;
    [SerializeField] private int maxCarsCanDestroy = 1;

    private int carsDestroyed;
    private bool isInitialized;

    private Renderer[] childRenders;


    public override void RewindGhost(Vector3 targetPosition, Action RewindCompleteCallBack, bool isLocal = false)
    {
        base.RewindGhost(targetPosition, RewindCompleteCallBack, isLocal);
        ToggleChildRenders(true);
    }

    public override void DoAnimation(Action OnComplete)
    {
        isInitialized = true;
        transform.DOKill();
        float boostSpeed = speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(playerContainedData.PlayerData.PlayerInformation[0].Boosterspeed);
        rb.DOMoveZ(boostDistance, boostSpeed / Time.fixedDeltaTime).SetEase(Ease.Linear).SetUpdate(UpdateType.Fixed).SetSpeedBased(true).SetRelative(true).OnComplete(() => { OnComplete(); OnAnimationDone(); });
    }

    public override void StopAllAnimations()
    {
        base.StopAllAnimations();
        isInitialized = false;
        carsDestroyed = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isInitialized)
            return;

        if (carsDestroyed >= maxCarsCanDestroy)
            return;

    //    UnityEngine.Console.Log($"Ghost collided with {other.gameObject}");

        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacles"))
        {
            ObjectDestruction objectDestruction = other.GetComponent<ObjectDestruction>();

            if (objectDestruction == null)
                return;

            if (!objectDestruction.isDestroyingDuringBoost)
                return;

            carsDestroyed++;
            objectDestruction.DestroyCar();
        }
    }

    public override void OnAnimationDone()
    {
        base.OnAnimationDone();
        ToggleChildRenders(false);

    }

    private void ToggleChildRenders(bool status)
    {
        if (childRenders == null)
        {
            childRenders = GetComponentsInChildren<Renderer>();
        }

        foreach (var item in childRenders)
        {
            item.enabled = status;
        }
    }
}
