using FSM;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = PlayerState.PlayerSlideState, menuName = "ScriptableObjects/PlayerSlideState")]
public class PlayerSlideState : StateBase
{
    // GameEvents
    [SerializeField] private GameEvent playerHasSlid;
    [SerializeField] private GameEvent playerHasStoppedSliding;

    [SerializeField] private PlayerSharedData PlayerSharedData;
    [SerializeField] private PlayerContainedData PlayerContainedData;
    [SerializeField] private GameEvent pogoPickedUp;
    [SerializeField] private GameEvent AeroplanePickedUp;
    [SerializeField] private float slideInputRetentionPercent;
    [SerializeField] private Vector2 slideColliderProperties;
    [SerializeField] private GameEvent dependenciesLoaded;

    private Vector2 defaultColliderProperties;
    private float slideDurationAtStartOfSlide;
    private float slideDuration;  //Duration in which slide sould perform.
    private float slideElapsedRetentionWindow;

  //  private float enteredZ;


    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        dependenciesLoaded.TheEvent.AddListener(SetDefaultColliderProperties);
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        dependenciesLoaded.TheEvent.RemoveListener(SetDefaultColliderProperties);
    }

    private void SetDefaultColliderProperties(GameEvent gameEvent = null)
    {
        defaultColliderProperties = new Vector2(PlayerSharedData.PlayerHitCollider.size.y, PlayerSharedData.PlayerHitCollider.center.y);
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        ResetVariable();
    }

    private void ResetVariable()
    {
        slideElapsedRetentionWindow = 0;
        UnSubsribeInputEvents();
    }

    //subscribing all events that can occur during this state
    private void SubsribeInputEvents()
    {
        PlayerContainedData.InputChannel.SwipeRightOccured.AddListener(SwipedRight);
        PlayerContainedData.InputChannel.SwipeLeftOccured.AddListener(SwipedLeft);
        PlayerContainedData.InputChannel.SwipeUpOccured.AddListener(SwipeUp);
        PlayerContainedData.InputChannel.SingleTapOccured.AddListener(Dash);
        PlayerContainedData.InputChannel.DoubleTabOccured.AddListener(Boost);
        PlayerContainedData.InputChannel.SwipeDownOccured.AddListener(StartSwipeDownRetentionWindow);
        pogoPickedUp.TheEvent.AddListener(PogoStick);
        AeroplanePickedUp.TheEvent.AddListener(Aeroplane);
    }

    /// <summary>
    ///unsubscribing all events that can occur during this state
    /// </summary>
    public void UnSubsribeInputEvents()
    {
        PlayerContainedData.InputChannel.SwipeRightOccured.RemoveListener(SwipedRight);
        PlayerContainedData.InputChannel.SwipeLeftOccured.RemoveListener(SwipedLeft);
        PlayerContainedData.InputChannel.SwipeUpOccured.RemoveListener(SwipeUp);
        PlayerContainedData.InputChannel.SingleTapOccured.RemoveListener(Dash);
        PlayerContainedData.InputChannel.DoubleTabOccured.RemoveListener(Boost);
        PlayerContainedData.InputChannel.SwipeDownOccured.RemoveListener(StartSwipeDownRetentionWindow);

        pogoPickedUp.TheEvent.RemoveListener(PogoStick);
        AeroplanePickedUp.TheEvent.RemoveListener(Aeroplane);
    }

    //onenter is like start wich will be called once when this state starts
    public override void OnEnter()
    {
        base.OnEnter();

        InitializeSliding();
    }

    //onexit will be called once when state exits
    public override void OnExit()
    {
        SetColliderYPos(false);
        base.OnExit();
        UnSubsribeInputEvents();

        playerHasStoppedSliding.RaiseEvent();
    }

    private void InitializeSliding()
    {
        SetColliderYPos(true);

        playerHasSlid.RaiseEvent();

        UnSubsribeInputEvents();

        SubsribeInputEvents();
        SlideAnimation();

        slideElapsedRetentionWindow = 0;

        slideDurationAtStartOfSlide = PlayerSharedData.SlideSpeed;
        slideDuration = PlayerSharedData.SlideSpeed;
    }

    private void SetColliderYPos(bool slide)
    {
        var colliderSize = PlayerSharedData.PlayerHitCollider.size;
        var center = PlayerSharedData.PlayerHitCollider.center;

        if (slide)
        {
            colliderSize.y = slideColliderProperties.x;
            center.y = slideColliderProperties.y;
        }
        else
        {
            colliderSize.y = defaultColliderProperties.x;
            center.y = defaultColliderProperties.y;

        }


        PlayerSharedData.PlayerHitCollider.size = colliderSize;
        PlayerSharedData.PlayerHitCollider.center = center;

    }

    public override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        PlayerContainedData.PlayerBasicMovementShared.OnTriggerEnter(other);
        if (other.gameObject.tag == ("wallrunbuilding"))
        {
            PlayerContainedData.AnimationChannel.Normal();
            StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToBuildingRunState);
        }
    }

    public override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        PlayerContainedData.PlayerBasicMovementShared.OnTriggerExit(other);
    }

    //onlogic basically works as update , all work related to update will be done in this method
    public override void OnLogic()
    {
        base.OnLogic();
        if (PlayerSharedData.IsBoosting)
        {
            FinishSlide();
            return;
        }
    }

    //onfixlogic basically works as fixupdate , all work related to fixupdate will be done in this method
    public override void OnFixedLogic()
    {
        base.OnFixedLogic();

        PlayerContainedData.PlayerBasicMovementShared.movement();

        if (CalculateSlideDuration() <= 0)
        {
            if(slideElapsedRetentionWindow > 0)
            {
                InitializeSliding();
            }
            else
            {
                FinishSlide();
            }

           
        }
    }

    //calls animation on slide functionality
    private void SlideAnimation()
    {
        // if (Animator_controller.LastAniamtion != AnimatorParameters.slide)
        //  {
        PlayerContainedData.Animator_controller.ResetTrigger(AnimatorParameters.normal);
        PlayerContainedData.Animator_controller.ResetTrigger(AnimatorParameters.jump);
        PlayerContainedData.Animator_controller.ResetTrigger(AnimatorParameters.left);
        PlayerContainedData.Animator_controller.ResetTrigger(AnimatorParameters.right);
        PlayerContainedData.AnimationChannel.SlideDown();
        PlayerSharedData.CharacterAnimator.SetTrigger(AnimatorParameters.slide);

        // }
    }

    //calculating duration for slide
    private float CalculateSlideDuration()
    {
        // subtractedTimes++;
        slideDuration -= Time.fixedDeltaTime;

        if (slideElapsedRetentionWindow > 0)
        {
            slideElapsedRetentionWindow -= Time.fixedDeltaTime;
        }

        return slideDuration;
    }

    //finish the slide when duration for slide is completed
    private void FinishSlide()
    {
        //  UnityEngine.Console.Log("Finish Slide");

        PlayerContainedData.AnimationChannel.Normal();
        //  PlayerSharedData.CharacterAnimator.SetTrigger(AnimatorParameters.normal);

        StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToNormalMovement);
    }

    private void SwipedRight()
    {
        PlayerContainedData.PlayerBasicMovementShared.change_lane(1);
        StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToNormalMovement);
    }

    private void SwipedLeft()
    {
        PlayerContainedData.PlayerBasicMovementShared.change_lane(-1);
        StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToNormalMovement);
    }

    private void SwipeUp()
    {
        if (PlayerSharedData.IsGrounded)
        {
            PlayerContainedData.AnimationChannel.Normal();
            StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToJump);
        }
    }

    private void Dash()
    {
        if (!PlayerSharedData.IsDash && !PlayerSharedData.IsBoosting && !PlayerSharedData.WallRunBuilding && !PlayerSharedData.RotateOnbuilding)
        {
            PlayerContainedData.PlayerBoostState.StartDash();
        }
    }

    private void Boost()
    {
        if (!PlayerSharedData.IsBoosting && !PlayerSharedData.WallRunBuilding && !PlayerSharedData.RotateOnbuilding)
        {
            PlayerContainedData.PlayerDoubleBoostState.StartBoost();
        }
    }

    private void StartSwipeDownRetentionWindow()
    {
        slideElapsedRetentionWindow = slideDurationAtStartOfSlide * slideInputRetentionPercent;
    }

    private void PogoStick(GameEvent theEvent)
    {
        StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToThurstState);
    }

    private void Aeroplane(GameEvent theEvent)
    {
        StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToPlayerAeroplaneState);
    }
}