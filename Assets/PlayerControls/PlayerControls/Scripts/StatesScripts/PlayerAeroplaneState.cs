using FSM;
using UnityEngine;

[CreateAssetMenu(fileName = PlayerState.PlayerAeroplaneState, menuName = "ScriptableObjects/PlayerAeroplaneState")]
public class PlayerAeroplaneState : StateBase
{
    public PlayerSharedData PlayerSharedData;
    public PlayerContainedData PlayerContainedData;

    [SerializeField] private SpeedHandler speedHandler;
    [SerializeField] private GamePlaySessionData gamePlaySessionData;
    [SerializeField] private GameManager gameManager;

    [SerializeField] private GameEvent PlayerHasCompletedAerplaneState;
    [SerializeField] private GameEvent aeroplaneStateStarted;
    [SerializeField] private GameEvent aeroplaneStateHasReachedMaxHeight;
    [SerializeField] private SpecialPickupsEnumSO specialPickupsEnumSO;
    [SerializeField] private PowerUpDurationManager powerUpDurationManager;

    private Vector3 jumpStartPos = Vector3.zero; //getting start pos of obj so that it can move on parabola
    private Vector3 jumpEndPos = Vector3.zero; //getting end pos of obj so that it can move on parabola
    private float elapsedTime;
    private float jumpTime;
    private float Pos; //calculating diiference between my transform and jump vector and adding in to pos ehich is added in upward pos

    private float stateElapsedTime;
    private bool playerReachedMaxHeightEventSent;
   
    private float timeToReachMaxHeight;

    public float GetAeroplaneLifeTime
    {
        get
        {
            float gameTimeScaleForMaxFlySpeed = speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(PlayerContainedData.PlayerData.PlayerInformation[0].FlyingMaxSpeed);
            float timeToReachMaxHeight = PlayerContainedData.PlayerData.PlayerInformation[0].TimeToReachFlyingTopHeight;
            
            float zDistanceCoveredDuringTakeOff = gameTimeScaleForMaxFlySpeed * (timeToReachMaxHeight / Time.fixedDeltaTime);
            float flyingSpeedIncrement = PlayerContainedData.PlayerData.PlayerInformation[0].FlyingMaxSpeed - gameManager.GetMinimumSpeed;

            // because chunk distance is calculated in minimum speed we multiply it with flyingSpeedIncrement to get distance in this speed
            // divide by 60 because chunk distance is 1 minute gameplay
            return (((GameManager.ChunkDistance * flyingSpeedIncrement) / 60f) * powerUpDurationManager.GetPowerDurationForCar(specialPickupsEnumSO.AeroPlanePickup)) + zDistanceCoveredDuringTakeOff;
        }
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        ResetVariable();
    }

    private void ResetVariable()
    {
        UnSubsribeInputEvents();
    }

    private void SubsribeInputEvents()
    {
        PlayerContainedData.InputChannel.SwipeRightOccured.AddListener(SwipedRight);
        PlayerContainedData.InputChannel.SwipeLeftOccured.AddListener(SwipedLeft);
    }

    /// <summary>
    ///unsubscribing all events that can occur during this state
    /// </summary>
    public void UnSubsribeInputEvents()
    {
        PlayerContainedData.InputChannel.SwipeRightOccured.RemoveListener(SwipedRight);
        PlayerContainedData.InputChannel.SwipeLeftOccured.RemoveListener(SwipedLeft);
    }

    private void SwipedRight()
    {
        PlayerContainedData.PlayerBasicMovementShared.change_lane(1);
    }

    private void SwipedLeft()
    {
        PlayerContainedData.PlayerBasicMovementShared.change_lane(-1);
    }

    //onenter is like start wich will be called once when this state starts
    public override void OnEnter()
    {
        base.OnEnter();
        UnSubsribeInputEvents();
        SubsribeInputEvents();
        cancelJump();
        startJump();
        Pos = 0;

        PlayerContainedData.PlayerBoostState.StopDash(0);
        PlayerContainedData.PlayerDoubleBoostState.StopBoost(0);
        aeroplaneStateStarted.RaiseEvent();
        PowerUpsChannel.RaisePowerActivatedEvent(specialPickupsEnumSO.AeroPlanePickup as InventoryItemSO, powerUpDurationManager.GetPowerDurationForCar(specialPickupsEnumSO.AeroPlanePickup));
        PlayerSharedData.isJumpingPhase = false;
        PlayerSharedData.AeroplaneTakingOff = true;
        speedHandler.ChangeGameTimeScaleInTime(PlayerContainedData.PlayerData.PlayerInformation[0].FlyingMaxSpeed, PlayerContainedData.PlayerData.PlayerInformation[0].TimeToReachFlyingMaxSpeed, true, true);
        PlayerSharedData.CarSkeleton.WingsTransform.SetActive(true);

    }

    //this is called when jump restarts

    //onexit will be called once when state exits
    public override void OnExit()
    {
        PlayerSharedData.AeroplaneTakingOff = false;

        PlayerSharedData.CarSkeleton.WingsTransform.SetActive(false);
        // UnityEngine.Console.LogError("AerpoplaneStateExit");
        PowerUpsChannel.RaisePowerDeactivatedEvent(specialPickupsEnumSO.AeroPlanePickup as InventoryItemSO);
        PlayerHasCompletedAerplaneState.RaiseEvent();
        base.OnExit();
        UnSubsribeInputEvents();

        speedHandler.RemoveOverrideGameTimeScaleMode(PlayerContainedData.PlayerData.PlayerInformation[0].TimeToReachFlyingMaxSpeed);

        PlayerSharedData.PlayerAnimator.enabled = true;
    }

    //onlogic basically works as update , all work related to update will be done in this method
    public override void OnLogic()
    {
        base.OnLogic();

        if (gamePlaySessionData.DistanceCoveredInMeters > stateElapsedTime + GetAeroplaneLifeTime)
        {
            PlayerSharedData.InAir = false;
            StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToNormalMovement);
            elapsedTime = 0;
        }
    }

    //onfixlogic basically works as fixupdate , all work related to fixupdate will be done in this method
    public override void OnFixedLogic()
    {
        base.OnFixedLogic();

        PlayerContainedData.PlayerBasicMovementShared.movement();

        if (!PlayerSharedData.IsDash)
            Pos = jump() - PlayerSharedData.PlayerRigidBody.position.y;

        if (!PlayerSharedData.IsDash)
            PlayerContainedData.PlayerData.PlayerInformation[0].UpwardPos = PlayerSharedData.PlayerRigidBody.position.y + Pos;

        if (!PlayerSharedData.IsGrounded)
            PlayerSharedData.InAir = true;
    }

    //start jump is called when jump start
    private void startJump()
    {
        jumpStartPos = PlayerSharedData.PlayerTransform.position;
        jumpEndPos = new Vector3(PlayerSharedData.PlayerTransform.position.x, PlayerSharedData.PlayerTransform.position.y, PlayerSharedData.PlayerTransform.position.z + PlayerContainedData.PlayerData.PlayerInformation[0].jump_length);

        //  aeroplaneHeight = /*PlayerSharedData.PlayerTransform.position.y +*/ (PlayerContainedData.PlayerData.PlayerInformation[0].FlyingHeight /*- PlayerSharedData.PlayerTransform.position.y*/);

        float aeroplaneHeightCurrent = Mathf.Abs(PlayerContainedData.PlayerData.PlayerInformation[0].FlyingHeight - PlayerSharedData.PlayerTransform.position.y);

        timeToReachMaxHeight = (PlayerContainedData.PlayerData.PlayerInformation[0].TimeToReachFlyingTopHeight / PlayerContainedData.PlayerData.PlayerInformation[0].FlyingHeight) * aeroplaneHeightCurrent;

      //  UnityEngine.Console.Log($"Time to reach max height is {timeToReachMaxHeight}");

        //  PlayerContainedData.AnimationChannel.Jump(AnimatorParameters.jumpspeed, PlayerSharedData.JumpDuration);

        //    PlayerSharedData.PlayerAnimator.Play("Normal");
        //      PlayerSharedData.PlayerAnimator.enabled = false;
    }

    //will return jump vector and thorugh that vector we would move obj to y and z value.
    private float jump()
    {
        //        UnityEngine.Console.Log(jump().y);

        jumpTime = Mathf.Clamp01(MyExtensions.elapse_time(ref elapsedTime, timeToReachMaxHeight));

        // jumpTime = Mathf.Clamp(jumpTime, 0, 0.5f);
        //  Vector3 my = MathParabola.Parabola(jumpStartPos, jumpEndPos, aeroplaneHeight, jumpTime);

        if (jumpTime == 1 && !playerReachedMaxHeightEventSent)
        {
            aeroplaneStateHasReachedMaxHeight.RaiseEvent();
            playerReachedMaxHeightEventSent = true;
        }

        float y = Mathf.Lerp(jumpStartPos.y, PlayerContainedData.PlayerData.PlayerInformation[0].FlyingHeight, jumpTime);

        return y;
    }

    private void cancelJump()
    {
        stateElapsedTime = gamePlaySessionData.DistanceCoveredInMeters;
        elapsedTime = 0;
        jumpStartPos = Vector3.zero;
        jumpEndPos = Vector3.zero;
        jumpTime = 0;
        PlayerSharedData.InAir = false;
        playerReachedMaxHeightEventSent = false;
    }

    public override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        //  PlayerContainedData.PlayerBasicMovementShared.OnTriggerEnter(other);
        if (other.gameObject.tag == ("wallrunbuilding") /*|| other.gameObject.tag == ("MergerGate")*/)
        {
            StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToNormalMovement);
            //     StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToBuildingRunState);
        }
    }

    public override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        PlayerContainedData.PlayerBasicMovementShared.OnTriggerExit(other);
    }
}