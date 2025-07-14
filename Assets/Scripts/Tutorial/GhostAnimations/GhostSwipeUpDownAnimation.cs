using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class GhostSwipeUpDownAnimation : TutorialGhostSegmentAnimation
{
    [SerializeField] private EditorPathScript editorPathScript;
    [SerializeField] private SpeedHandler speedHandler;
    [SerializeField] private PlayerContainedData playerContainedData;
    [SerializeField] private PlayerSharedData playerSharedData;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject shockObject;
 //   [SerializeField] private float distanceToCover;
    [SerializeField] private float jumpoffsetFromPlayersJump;
    [SerializeField] private float groundHeight;

    private Action onCompleteAction;

    private bool isInitialized;

   // private float xTargetPos = 1;
  //  private float distanceCovered;
    private float elapsedJumpTime;
    private Vector3 startJumpPos;
    private Vector3 endJumpPos;
    private bool isJumping;
    private bool isCanceledJump;
    private float initialHeight;

    private void OnDisable()
    {
        TimeScaleEffectsHandler.IsPaused = false;
    }

    public override void DoAnimation(Action OnComplete)
    {
        onCompleteAction = OnComplete;

        GhostAction ghostAction;

        if(!actionsToPerform.TryDequeue(out ghostAction))
        {
            throw new System.Exception($"Failed to deque tutorial ghost action");
        }

        ghostAction.actionToPerform();

    }

    private void GoUpTheRamp()
    {
        Vector3 groundPosition = transform.TransformPoint(new Vector3(0, groundHeight, 0));
        initialHeight = groundPosition.y;

        transform.DOKill();

        float speed = speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(SpeedHandler.GameTimeScale) / Time.fixedDeltaTime;

        Tween pathTween = rb.DOPath(editorPathScript.GetPath(), speed, PathType.Linear, PathMode.Full3D, 10).SetSpeedBased(true).SetUpdate(UpdateType.Fixed).SetEase(Ease.Linear).OnComplete(() =>
        {
            MarkSpecificAnimationAsCompleted(0);
            isInitialized = true;
        });

        pathTween.OnUpdate(() =>
        {
            float s = speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(SpeedHandler.GameTimeScale) / Time.fixedDeltaTime;
            float relativeSpeed = s / speed;
            pathTween.timeScale = relativeSpeed;
        });
    }


    private void FixedUpdate()
    {
        if (!isInitialized)
            return;

        float speed = speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(SpeedHandler.GameTimeScale);
        Vector3 targetPos = transform.position + (Vector3.forward * speed);

        if (isJumping)
        {
            targetPos.y = JumpY();
        }
        else if (isCanceledJump)
        {
            targetPos.y = ThrowDownWards();
        }

     //   targetPos.x = xTargetPos;
        rb.MovePosition(targetPos);

     //   distanceCovered += speed;

        if (/*distanceCovered >= distanceToCover && */HasAllAnimationsCompleted())
        {
            isInitialized = false;
            OnAnimationDone();
            onCompleteAction();
        }
    }

    public override void StopAllAnimations()
    {
        base.StopAllAnimations();
     //   xTargetPos = 1;
        isInitialized = false;
        elapsedJumpTime = 0;
        isJumping = false;
        isCanceledJump = false;
      //  distanceCovered = 0;
    }

    public override void OnAnimationDone()
    {
        base.OnAnimationDone();
        isInitialized = false;
        elapsedJumpTime = 0;
        isJumping = false;
        isCanceledJump = false;
      //  distanceCovered = 0;
}

    private void StartJump()
    {
        isJumping = true;
        startJumpPos = transform.position;
        endJumpPos = new Vector3(startJumpPos.x, initialHeight, playerContainedData.PlayerData.PlayerInformation[0].jump_length);
    }

    private float JumpY()
    {
        elapsedJumpTime += Time.fixedDeltaTime;
        float t = elapsedJumpTime / playerSharedData.JumpDuration;
        t = Mathf.Clamp01(t);

        return MathParabola.Parabola(startJumpPos, endJumpPos, playerContainedData.PlayerData.PlayerInformation[0].jump_height + jumpoffsetFromPlayersJump, t).y;
    }

    private float ThrowDownWards()
    {
        TimeScaleEffectsHandler.IsPaused = true;

        float vel = LerpExtensions.EaseInSine(1) * gameManager.GetTerminalVelocity * Time.fixedDeltaTime * SpeedHandler.GameTimeScale;
        if (transform.position.y + vel <= initialHeight)
        {
            isCanceledJump = false;
            MarkSpecificAnimationAsCompleted(2);
            var shockWave = Instantiate(shockObject, transform);
            shockWave.GetComponent<LockMovementOnPlayer>().enabled = false;
            shockWave.transform.position = transform.position;
            StartCoroutine(WaitAndReEnableTimeScaleEffects());
            return initialHeight;
        }
        else
        {
            return transform.position.y + vel;
        }
    }

    private IEnumerator WaitAndReEnableTimeScaleEffects()
    {
        yield return new WaitForSeconds(1);
        UnityEngine.Console.Log("Is UnPaused"); TimeScaleEffectsHandler.IsPaused = false;
    }

    private void CancelJump()
    {
        isJumping = false;
        MarkSpecificAnimationAsCompleted(1);
        isCanceledJump = true;
    }

    //private void DoLeftTurn()
    //{
    //    DOTween.To(() => xTargetPos, (x) => { xTargetPos = x; }, 0, speedHandler.GetSideWaysSpeedBasedOnSpecificGameTimeScale(SpeedHandler.GameTimeScale)).OnComplete(() => { MarkSpecificAnimationAsCompleted(3); });
    //}


    protected override void AddActionsToQueue()
    {
        base.AddActionsToQueue();

        AddActionToQueue(new GhostAction(GoUpTheRamp));
        AddActionToQueue(new GhostAction(StartJump));
        AddActionToQueue(new GhostAction(CancelJump));
      //  AddActionToQueue(new GhostAction(DoLeftTurn));
    }
}