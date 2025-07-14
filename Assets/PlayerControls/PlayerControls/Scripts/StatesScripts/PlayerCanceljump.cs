using FSM;
using UnityEngine;

[CreateAssetMenu(fileName = PlayerState.PlayerCanceljump, menuName = "ScriptableObjects/PlayerCanceljump")]
public class PlayerCanceljump : StateBase
{
    [SerializeField] private PlayerSharedData PlayerSharedData;
    [SerializeField] private PlayerContainedData PlayerContainedData;
    [SerializeField] private GameEvent PlayerCancelJump;
    [SerializeField] private GameEvent PlayerHasDoneShockwave;

    [SerializeField] private GameEvent pogoPickedUp;
    [SerializeField] private GameEvent AeroplanePickedUp;
    // private float nextJumpTimer; //calculating time for next time jump .... if swiped in air timer will start and if timer is greater than 0 at landing it will jump again

    //onenter is like start wich will be called once when this state start
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
        PlayerContainedData.InputChannel.SwipeUpOccured.AddListener(SwipedUp);
        PlayerContainedData.InputChannel.SwipeDownOccured.AddListener(SwipedDown);
        PlayerContainedData.InputChannel.SingleTapOccured.AddListener(SingleTap);
        PlayerContainedData.InputChannel.DoubleTabOccured.AddListener(DoubleTap);
        pogoPickedUp.TheEvent.AddListener(PogoStick);
        AeroplanePickedUp.TheEvent.AddListener(Aeroplane);
        // PlayerContainedData.InputChannel.SwipeUpOccured.AddListener(Jump);
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
        // PlayerContainedData.InputChannel.SwipeUpOccured.RemoveListener(Jump);
    }

    public override void OnEnter()
    {
        base.OnEnter();
        SubsribeInputEvents();
        PlayerContainedData.Animator_controller.ResetTrigger(AnimatorParameters.normal);
        PlayerContainedData.Animator_controller.ResetTrigger(AnimatorParameters.jump);
        PlayerContainedData.Animator_controller.ResetTrigger(AnimatorParameters.left);
        PlayerContainedData.Animator_controller.ResetTrigger(AnimatorParameters.right);
        PlayerContainedData.AnimationChannel.CancelJump();
        PlayerCancelJump.RaiseEvent();
        //  nextJumpTimer = 0;
        PlayerContainedData.Animator_controller.ResetTrigger(AnimatorParameters.normal);
        PlayerContainedData.Animator_controller.ResetTrigger(AnimatorParameters.jump);
        PlayerContainedData.Animator_controller.ResetTrigger(AnimatorParameters.left);
        PlayerContainedData.Animator_controller.ResetTrigger(AnimatorParameters.right);
    }

    public override void OnExit()
    {
        PlayerContainedData.Animator_controller.ResetTrigger(AnimatorParameters.canceljump);
        UnSubsribeInputEvents();
        base.OnExit();
    }

    //onlogic basically works as update , all work related to update will be done in this method
    public override void OnLogic()
    {
        base.OnLogic();
        if (PlayerSharedData.IsGrounded)
        {
            PlayerContainedData.Animator_controller.ResetTrigger(AnimatorParameters.normal);
            PlayerContainedData.Animator_controller.ResetTrigger(AnimatorParameters.jump);
            PlayerContainedData.Animator_controller.ResetTrigger(AnimatorParameters.left);
            PlayerContainedData.Animator_controller.ResetTrigger(AnimatorParameters.right);
            ExplosionDamage(PlayerSharedData.PlayerTransform.position, PlayerContainedData.PlayerData.PlayerInformation[0].ShockWaveRadius);
            //  if (nextJumpTimer > 0)
            //  {
            //  StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToJump);
            //  }
            //  else
            //   {
            StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToSlideState);
            //  }
        }
        // nextJumpTimer -= Time.deltaTime;
    }

    public override void OnFixedLogic()
    {
        base.OnFixedLogic();
        PlayerContainedData.PlayerBasicMovementShared.movement();
    }

    private void ExplosionDamage(Vector3 center, float radius)
    {
        PlayerHasDoneShockwave.RaiseEvent();
    }

    public override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        PlayerContainedData.PlayerBasicMovementShared.OnTriggerEnter(other);
        if (other.gameObject.tag == ("wallrunbuilding"))
        {
            UnityEngine.Console.LogError("canceljumpstatebuilding");

            StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToBuildingRunState);
        }
    }

    private void PogoStick(GameEvent theEvent)
    {
        StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToThurstState);
    }

    private void Aeroplane(GameEvent theEvent)
    {
        StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToPlayerAeroplaneState);
    }

    //private void Jump()
    //{
    //    nextJumpTimer = 0.2f;
    //}

    public override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        PlayerContainedData.PlayerBasicMovementShared.OnTriggerExit(other);
    }

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
}