using DG.Tweening;
using System;
using UnityEngine;

public class GhostJumpAnimation : TutorialGhostSegmentAnimation
{
    //  [SerializeField] private float distToCover;
    [SerializeField] private SpeedHandler speedHandler;

    [SerializeField] private PlayerContainedData playerContainedData;
    [SerializeField] private PlayerSharedData playerSharedData;

    private bool isInitialized = false;
    private float elapsedJumpTime;

    //  private float distCovered;
    //  private float xTargetPos = 1;
    private Action onCompleteAction;

    private Vector3 startJumpPos;
    private Vector3 endJumpPos;

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

    public override void StopAllAnimations()
    {
        base.StopAllAnimations();
        elapsedJumpTime = 0;
        //xTargetPos = 1;
        //distCovered = 0;
        isInitialized = false;
    }

    private void FixedUpdate()
    {
        if (!isInitialized)
            return;

        float speed = speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(SpeedHandler.GameTimeScale);
        Vector3 targetPos = transform.position + (Vector3.forward * speed);
        targetPos.y = JumpY();
        //   targetPos.x = xTargetPos;
        rb.MovePosition(targetPos);

        //  distCovered += speed;

        float t = elapsedJumpTime / playerSharedData.JumpDuration;
        t = Mathf.Clamp01(t);
        if (Mathf.Approximately(rb.position.y, GetParabollaValueAtPoint(1)) && Mathf.Approximately(t, 1))
        {
            MarkSpecificAnimationAsCompleted(0);
        }

        // if (distCovered >= distToCover)
        if (HasAllAnimationsCompleted())
        {
            isInitialized = false;

            OnAnimationDone();
            onCompleteAction();
        }
    }

    protected override void AddActionsToQueue()
    {
        base.AddActionsToQueue();
        AddActionToQueue(new GhostAction(CalculateJump));
    }

    private void CalculateJump()
    {
        startJumpPos = transform.position;
        endJumpPos = startJumpPos + (Vector3.forward * playerContainedData.PlayerData.PlayerInformation[0].jump_length);
    }

    public override void OnAnimationDone()
    {
        base.OnAnimationDone();
        elapsedJumpTime = 0;
        isInitialized = false;
    }

    private float JumpY()
    {
        elapsedJumpTime += (Time.fixedDeltaTime * SpeedHandler.GameTimeScale);
        float t = elapsedJumpTime / playerSharedData.JumpDuration;
        t = Mathf.Clamp01(t);

        return GetParabollaValueAtPoint(t);
    }

    private float GetParabollaValueAtPoint(float point)
    {
        return MathParabola.Parabola(startJumpPos, endJumpPos, playerContainedData.PlayerData.PlayerInformation[0].jump_height, point).y;

    }

    //private void DoLeftTurn()
    //{
    //    DOTween.To(() => xTargetPos, (x) => { xTargetPos = x; }, 0, speedHandler.GetSideWaysSpeedBasedOnSpecificGameTimeScale(SpeedHandler.GameTimeScale));
    //}
}