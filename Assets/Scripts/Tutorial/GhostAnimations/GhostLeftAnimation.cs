using DG.Tweening;
using System;
using UnityEngine;

public class GhostLeftAnimation : TutorialGhostSegmentAnimation
{
    [SerializeField] private SpeedHandler speedHandler;
    [SerializeField] private PlayerContainedData playerContainedData;

    private float xTargetPos;
    private float xCurPos;
    private bool isInitialized = false;

    private Action onCompleteAction;
    private Tween xMoveTween;

    public override void DoAnimation(Action OnComplete)
    {
        transform.DOKill();
        onCompleteAction = OnComplete;
        isInitialized = true;

        GhostAction ghostAction;

        if (!actionsToPerform.TryDequeue(out ghostAction))
        {
            throw new System.Exception($"Failed to deque tutorial ghost action");
        }

        ghostAction.actionToPerform();
    }

    private void FixedUpdate()
    {
        if (!isInitialized)
            return;

        float speed = speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(SpeedHandler.GameTimeScale);
        Vector3 targetPos = transform.position + (Vector3.forward * speed);
        targetPos.x = xCurPos;
        rb.MovePosition(targetPos);

        if (Mathf.Approximately(rb.position.x, xTargetPos))
        {
            MarkSpecificAnimationAsCompleted(0);
        }

        if (HasAllAnimationsCompleted())
        {
            isInitialized = false;
            OnAnimationDone();
            onCompleteAction();
        }
    }

    private void DoLeftTurn()
    {
        float sideWaySpeed = speedHandler.GetSideWaysSpeedBasedOnSpecificGameTimeScale(SpeedHandler.GameTimeScale);
        //    UnityEngine.Console.Log("SideWaysSpeed" + sideWaySpeed);
        xTargetPos = rb.position.x - 1;
        xCurPos = rb.position.x;
        float startingVal = xCurPos;
        xMoveTween = DOTween.To(() => startingVal, (x) => { xCurPos = x; }, xTargetPos, sideWaySpeed).SetEase(Ease.InOutCubic);
    }

    public override void StopAllAnimations()
    {
        //  UnityEngine.Console.Log("Stop IT?");

        base.StopAllAnimations();

        if (xMoveTween != null)
        {
            xMoveTween.Kill();
        }

        isInitialized = false;
        xTargetPos = 0;
    }

    protected override void AddActionsToQueue()
    {
        base.AddActionsToQueue();
        AddActionToQueue(new GhostAction(DoLeftTurn));
    }

    public override void OnAnimationDone()
    {
        base.OnAnimationDone();

        if (xMoveTween != null)
        {
            xMoveTween.Kill();
        }

        xTargetPos = 0;
        isInitialized = false;
    }
}