using FSM;
using UnityEngine;

[CreateAssetMenu(fileName = PlayerState.PlayerNormalMovementState, menuName = "ScriptableObjects/PlayerNormalMovementState")]
public class PlayerNormalMovementState : StateBase
{
    [SerializeField] private PlayerSharedData PlayerSharedData;
    [SerializeField] private GameEvent cutSceneStarted;
    [SerializeField] private PlayerContainedData PlayerContainedData;
    [SerializeField] private GameEvent pogoPickedUp;
    [SerializeField] private GameEvent AeroplanePickedUp;
    [SerializeField] private GameEvent playerTouchedGround;

    private void OnEnable()
    {
        cutSceneStarted.TheEvent.AddListener(HandleCutSceneStarted);
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        ResetVariable();
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;

        cutSceneStarted.TheEvent.RemoveListener(HandleCutSceneStarted);
    }

    private void ResetVariable()
    {
        UnSubsribeInputEvents();
    }

    //subscribing all events which can occur during normal state
    private void SubsribeInputEvents()
    {
        PlayerContainedData.InputChannel.SwipeRightOccured.AddListener(SwipedRight);
        PlayerContainedData.InputChannel.SwipeLeftOccured.AddListener(SwipedLeft);
        PlayerContainedData.InputChannel.SwipeUpOccured.AddListener(SwipedUp);
        PlayerContainedData.InputChannel.SwipeDownOccured.AddListener(SwipedDown);
        PlayerContainedData.InputChannel.SingleTapOccured.AddListener(SingleTap);
        PlayerContainedData.InputChannel.DoubleTabOccured.AddListener(DoubleTap);
        pogoPickedUp.TheEvent.AddListener(PogoStick);
        AeroplanePickedUp.TheEvent.AddListener(Aeroplane);
        playerTouchedGround.TheEvent.AddListener(PlayerHasTouchedTheGround);
    }

    //unsubscribing all events which can occur during normal state
    public void UnSubsribeInputEvents()
    {
        PlayerContainedData.InputChannel.SwipeRightOccured.RemoveListener(SwipedRight);
        PlayerContainedData.InputChannel.SwipeLeftOccured.RemoveListener(SwipedLeft);
        PlayerContainedData.InputChannel.SwipeUpOccured.RemoveListener(SwipedUp);
        PlayerContainedData.InputChannel.SwipeDownOccured.RemoveListener(SwipedDown);
        PlayerContainedData.InputChannel.SingleTapOccured.RemoveListener(SingleTap);
        PlayerContainedData.InputChannel.DoubleTabOccured.RemoveListener(DoubleTap);
        pogoPickedUp.TheEvent.RemoveListener(PogoStick);
        AeroplanePickedUp.TheEvent.RemoveListener(Aeroplane);
        playerTouchedGround.TheEvent.RemoveListener(PlayerHasTouchedTheGround);
    }

    //onenter is like start wich will be called once when this state starts
    public override void OnEnter()
    {
        base.OnEnter();

        PlayerSharedData.IsIdle = true;

        UnSubsribeInputEvents();
        SubsribeInputEvents();
    }

    private void HandleCutSceneStarted(GameEvent gameEvent)
    {
       // UnSubsribeInputEvents();
        SubsribeInputEvents();
    }

    //onexit will be called once when state exits
    public override void OnExit()
    {
        UnSubsribeInputEvents();
        PlayerSharedData.IsIdle = false;

        base.OnExit();
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

        PlayerContainedData.PlayerBasicMovementShared.movement();
    }

    //when swipe right event has occured
    private void SwipedRight()
    {
        PlayerContainedData.PlayerBasicMovementShared.change_lane(1);
    }

    //when swipe left event has occured
    private void SwipedLeft()
    {
        PlayerContainedData.PlayerBasicMovementShared.change_lane(-1);
    }

    //when swipe up event has occured
    private void SwipedUp()
    {
        StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToJump);
    }

    //when swipe down event has occured
    private void SwipedDown()
    {
        if (PlayerSharedData.IsGrounded)
            StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToSlideState);
        else
            StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToJumpCancel);
    }

    //when single tap (Dash) event has occured
    private void SingleTap()
    {
        if (!PlayerSharedData.IsDash && !PlayerSharedData.IsBoosting && !PlayerSharedData.WallRunBuilding && !PlayerSharedData.RotateOnbuilding)
        {
            PlayerContainedData.PlayerBoostState.StartDash();
        }
    }

    //when Double tap (Boost) event has occured
    private void DoubleTap()
    {
        if (!PlayerSharedData.IsBoosting && !PlayerSharedData.WallRunBuilding && !PlayerSharedData.RotateOnbuilding)
        {
            PlayerContainedData.PlayerDoubleBoostState.StartBoost();
        }
    }

    private void PogoStick(GameEvent theEvent)
    {
        // UnityEngine.Console.Log("pogo");
        StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToThurstState);
    }

    private void Aeroplane(GameEvent theEvent)
    {
        //   UnityEngine.Console.Log(PlayerStateEvent.ToPlayerAeroplaneState);
        StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToPlayerAeroplaneState);
    }

    public override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        PlayerContainedData.PlayerBasicMovementShared.OnTriggerEnter(other);
        if (other.gameObject.tag == ("wallrunbuilding"))
        {
            StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToBuildingRunState);
        }
    }

    public override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        PlayerContainedData.PlayerBasicMovementShared.OnTriggerExit(other);
    }

    private void PlayerHasTouchedTheGround(GameEvent gameEvent)
    {
    }
}