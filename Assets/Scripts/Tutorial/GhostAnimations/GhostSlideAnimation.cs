using DG.Tweening;
using System;
using UnityEngine;

public class GhostSlideAnimation : TutorialGhostSegmentAnimation
{
    [SerializeField] private PlayerSharedData playerSharedData;
    [SerializeField] private SpeedHandler speedHandler;
    [SerializeField] private float distToCover;

    private Action onCompleteAction;

    private float distCovered;
    private bool isInitialized;
    private Sequence sequence;

    public override void DoAnimation(Action OnComplete)
    {
        transform.DOKill();

        sequence = DOTween.Sequence();
        sequence.Append(transform.DOScaleY(0.17f, .25f));
        sequence.AppendInterval(playerSharedData.SlideSpeed*.5f);
        sequence.Append(transform.DOScaleY(1f, .25f));

        onCompleteAction = OnComplete;

        isInitialized = true;
    }

    private void FixedUpdate()
    {
        if (!isInitialized)
            return;

        float speed = speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(SpeedHandler.GameTimeScale);
        Vector3 targetPos = transform.position + (Vector3.forward * speed);

        rb.MovePosition(targetPos);

        distCovered += speed;


        bool animationDone = sequence == null || !sequence.IsPlaying();

        if (/*distCovered >= distToCover && */animationDone)
        {
            isInitialized = false;
      
            OnAnimationDone();
            onCompleteAction();
        }
    }

    public override void StopAllAnimations()
    {
        UnityEngine.Console.Log("Stop All Animation");

        base.StopAllAnimations();
       transform.localScale = Vector3.one;
        distCovered = 0;
        isInitialized = false;
    }

    public override void OnAnimationDone()
    {
        UnityEngine.Console.Log("On Animation Done");
        base.OnAnimationDone();
        transform.localScale = Vector3.one;
        sequence.Kill();
        sequence = null;
    }
}