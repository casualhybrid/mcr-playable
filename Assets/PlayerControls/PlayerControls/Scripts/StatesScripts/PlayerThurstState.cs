using FSM;
using UnityEngine;

[CreateAssetMenu(fileName = PlayerState.PlayerThurstState, menuName = "ScriptableObjects/PlayerThurstState")]
public class PlayerThurstState : StateBase
{
    [SerializeField] private GameEvent playerPickedUPAeroplane;
    [SerializeField] private GameEvent playerHasEndedPogo;
    [SerializeField] private GameEvent PlayerHasStartedPogo;

    public PlayerSharedData PlayerSharedData;
    public PlayerContainedData PlayerContainedData;
    private float nextJumpTimer; //calculating time for next time jump .... if swiped in air timer will start and if timer is greater than 0 at landing it will jump again

    private Vector3 jumpStartPos = Vector3.zero; //getting start pos of obj so that it can move on parabola
    private Vector3 jumpEndPos = Vector3.zero; //getting end pos of obj so that it can move on parabola
    private float elapsedTime;
    private float jumpTime;
    private float Pos; //calculating diiference between my transform and jump vector and adding in to pos ehich is added in upward pos
    private bool fallingFromPogo;

    private void OnEnable()
    {
        playerPickedUPAeroplane.TheEvent.AddListener(Aeroplane);
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        ResetVariable();
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;

        playerPickedUPAeroplane.TheEvent.RemoveListener(Aeroplane);
    }

    private void ResetVariable()
    {
        UnSubsribeInputEvents();
    }

    private void SubsribeInputEvents()
    {
        PlayerContainedData.InputChannel.SwipeRightOccured.AddListener(SwipedRight);
        PlayerContainedData.InputChannel.SwipeLeftOccured.AddListener(SwipedLeft);
        PlayerContainedData.InputChannel.SwipeDownOccured.AddListener(SwipedDown);
        PlayerContainedData.InputChannel.SwipeUpOccured.AddListener(swipeup);
    }

    /// <summary>
    ///unsubscribing all events that can occur during this state
    /// </summary>
    public void UnSubsribeInputEvents()
    {
        PlayerContainedData.InputChannel.SwipeRightOccured.RemoveListener(SwipedRight);
        PlayerContainedData.InputChannel.SwipeLeftOccured.RemoveListener(SwipedLeft);
        PlayerContainedData.InputChannel.SwipeDownOccured.RemoveListener(SwipedDown);
        PlayerContainedData.InputChannel.SwipeUpOccured.RemoveListener(swipeup);
    }

    private void SwipedRight()
    {
        PlayerContainedData.PlayerBasicMovementShared.change_lane(1);
    }

    private void SwipedLeft()
    {
        PlayerContainedData.PlayerBasicMovementShared.change_lane(-1);
    }

    private void SwipedDown()
    {
        PlayerSharedData.InAir = false;
        StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToJumpCancel);
    }

    //onenter is like start wich will be called once when this state starts
    public override void OnEnter()
    {
        base.OnEnter();
        UnSubsribeInputEvents();
        SubsribeInputEvents();
        PlayerContainedData.PlayerBoostState.StopDash(0);
        PlayerContainedData.PlayerDoubleBoostState.StopBoost(0);
        cancelJump();
        startJump();
        PlayerSharedData.isJumpingPhase = false;
        PlayerSharedData.isHalfThurstingCompleted = false;
        PlayerSharedData.isThursting = true;
        nextJumpTimer = 0;
        fallingFromPogo = false;
        Pos = 0;
    }

    //this is called when jump restarts

    //onexit will be called once when state exits
    public override void OnExit()
    {
        base.OnExit();

        // Force quit
        if (!PlayerSharedData.isHalfThurstingCompleted)
        {
            MarkVerticalMotionAsCompleted();
        }

        PlayerSharedData.isThursting = false;

        UnSubsribeInputEvents();
    }

    private void Aeroplane(GameEvent theEvent)
    {
      //  UnityEngine.Console.Log(PlayerStateEvent.ToPlayerAeroplaneState);
        StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToPlayerAeroplaneState);
    }

    //onlogic basically works as update , all work related to update will be done in this method
    public override void OnLogic()
    {
        base.OnLogic();
    }

    //onfixlogic basically works as fixupdate , all work related to fixupdate will be done in this method
    public override void OnFixedLogic()
    {
        base.OnFixedLogic();

        nextJumpTimer -= Time.fixedDeltaTime;

        //if (PlayerSharedData.IsDash)
        //{
        //    StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToNormalMovement);
        //    return;
        //}

        if (jumpTime >= .5f && !fallingFromPogo)
        {
            MarkVerticalMotionAsCompleted();
            StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToNormalMovement);
            return;
        }

        PlayerContainedData.PlayerBasicMovementShared.movement();

   
        //  if (!PlayerSharedData.IsDash)
        Pos = jump().y - PlayerSharedData.PlayerRigidBody.position.y;

        // if (!PlayerSharedData.IsDash)
        PlayerContainedData.PlayerData.PlayerInformation[0].UpwardPos = PlayerSharedData.PlayerRigidBody.position.y + Pos;

        if (!PlayerSharedData.IsGrounded)
            PlayerSharedData.InAir = true;
    }

    //  private float height;

    //start jump is called when jump start
    private void startJump()
    {
        PlayerHasStartedPogo.RaiseEvent();
        jumpStartPos = jumpEndPos = PlayerSharedData.PlayerTransform.position;
        jumpEndPos.z += PlayerContainedData.PlayerData.PlayerInformation[0].ThurstJumpLength;
        PlayerContainedData.AnimationChannel.Jump(AnimatorParameters.jumpspeed, PlayerContainedData.PlayerData.PlayerInformation[0].ThurstDuration, PlayerSharedData.PlayerAnimator);
        PlayerContainedData.AnimationChannel.Thurst(AnimatorParameters.thurst, PlayerContainedData.PlayerData.PlayerInformation[0].ThurstDuration, PlayerSharedData.CharacterAnimator);

        //    height = PlayerContainedData.PlayerData.PlayerInformation[0].jump_height + 10;
    }

    //will return jump vector and thorugh that vector we would move obj to y and z value.
    private Vector3 jump()
    {
        float increaseAmount = !fallingFromPogo ? 1f : 1f;

        jumpTime = Mathf.Clamp01(MyExtensions.elapse_time(ref elapsedTime, PlayerContainedData.PlayerData.PlayerInformation[0].ThurstDuration, increaseAmount));

        if (PlayerSharedData.IsGrounded && PlayerSharedData.InAir && PlayerSharedData.isHalfThurstingCompleted)
        {
            PlayerSharedData.InAir = false;

            if (nextJumpTimer > 0)
            {
                StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToJump);
            }
            else
            {
                StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToNormalMovement);
            }

            elapsedTime = 0;
        }

        Vector3 my = MathParabola.Parabola(jumpStartPos, jumpEndPos, PlayerContainedData.PlayerData.PlayerInformation[0].ThurstJumpHeight, jumpTime);

     

        return my;
    }

    private void MarkVerticalMotionAsCompleted()
    {
        fallingFromPogo = true;
        playerHasEndedPogo.RaiseEvent();
        PlayerSharedData.isHalfThurstingCompleted = true;
    }

    private void swipeup()
    {
        nextJumpTimer = PlayerContainedData.PlayerData.PlayerInformation[0].NextJumpTimer;
    }

    private void cancelJump()
    {
        elapsedTime = 0;
        jumpStartPos = Vector3.zero;
        jumpEndPos = Vector3.zero;
        jumpTime = 0;
        PlayerSharedData.InAir = false;
    }
}