using FSM;
using TheKnights.SaveFileSystem;
using UnityEngine;

[CreateAssetMenu(fileName = PlayerState.PlayerJumpState, menuName = "ScriptableObjects/PlayerJumpState")]
public class PlayerJumpState : StateBase
{
    // GameEvents
    [SerializeField] private GameEvent playerIsJumping;
    [SerializeField] private GameEvent playerIsDoubleJumping;

    [SerializeField] private GameEvent playerIsSpringJumping;

    [SerializeField] private GameEvent pogoPickedUp;
    [SerializeField] private GameEvent AeroplanePickedUp;

    [SerializeField] private PlayerSharedData PlayerSharedData;
    [SerializeField] private PlayerContainedData PlayerContainedData;
    [SerializeField] private SaveManager saveManager;

    private float speedTimeToJump; // speed to jump left and right . depends on movement speed .
    private Vector3 jumpStartPos = Vector3.zero; //getting start pos of obj so that it can move on parabola
    private Vector3 jumpEndPos = Vector3.zero; //getting end pos of obj so that it can move on parabola
    private float jumpDistance; //distance between jump start and jump end.
    private float elapsedTime;
    private float nextJumpTimer; //calculating time for next time jump .... if swiped in air timer will start and if timer is greater than 0 at landing it will jump again
    private float jumpTime;
    private float Pos; //calculating diiference between my transform and jump vector and adding in to pos ehich is added in upward pos
    private float height;

    private bool stateExit;

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

    //subscribing all events that can occur during this state
    private void SubsribeInputEvents()
    {
        PlayerContainedData.InputChannel.SwipeRightOccured.AddListener(SwipedRight);
        PlayerContainedData.InputChannel.SwipeLeftOccured.AddListener(SwipedLeft);
        PlayerContainedData.InputChannel.SwipeDownOccured.AddListener(SwipedDown);
        PlayerContainedData.InputChannel.SwipeUpOccured.AddListener(SwipeUp);
        PlayerContainedData.InputChannel.SingleTapOccured.AddListener(Dash);
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
        PlayerContainedData.InputChannel.SwipeDownOccured.RemoveListener(SwipedDown);
        PlayerContainedData.InputChannel.SwipeUpOccured.RemoveListener(SwipeUp);
        PlayerContainedData.InputChannel.SingleTapOccured.RemoveListener(Dash);

        pogoPickedUp.TheEvent.RemoveListener(PogoStick);
        AeroplanePickedUp.TheEvent.RemoveListener(Aeroplane);
    }

    //onenter is like start wich will be called once when this state starts
    public override void OnEnter()
    {
        //  UnityEngine.Console.Log("When started Jump isGrounded ? " + PlayerSharedData.IsGrounded);

        base.OnEnter();
         


        if(PlayerSharedData.isJumpingPhase)
        {
            playerIsDoubleJumping.RaiseEvent();
            AnalyticsManager.CustomData("PlayerHasDoubleJumped");
        }

        if (PlayerSharedData.SpringJump)
        {
            playerIsSpringJumping.RaiseEvent();
            AnalyticsManager.CustomData("PlayerHasSpringJumped");
        }
        else
        {
            playerIsJumping.RaiseEvent();
        }

        UnSubsribeInputEvents();
        SubsribeInputEvents();
        RestartJump();

        PlayerContainedData.elapsedTimeRelaxationAfterFall = 0;
        PlayerSharedData.isJumping = true;
        PlayerSharedData.isJumpingPhase = true;
        Pos = 0;
    }

    //this is called when jump restarts
    private void RestartJump()
    {
        cancelJump();
        startJump();
    }

    //onexit will be called once when state exits
    public override void OnExit()
    {
        base.OnExit();
        //     UnityEngine.Console.LogError("JumpState EXIT");
        stateExit = true;
        PlayerSharedData.isJumping = false;
        UnSubsribeInputEvents();
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

        Pos = jump().y;

        if (stateExit)
            return;

        float posYToReach = Pos;

        #region DiffentApproachOfKeepingCarAboveTransporter

        RaycastHit groundedHit = GetHoverPositionIfGrounded();
        if (groundedHit.collider != null && groundedHit.collider.CompareTag("AdjustHeightDuringJump"))
        {
            float YPosIfGrounded = groundedHit.point.y + PlayerContainedData.PlayerData.PlayerInformation[0].HoverHeight;

            if (YPosIfGrounded > posYToReach)
            {
                PlayerSharedData.InAir = true;
            }

            posYToReach = Mathf.Max(posYToReach, YPosIfGrounded);
        }

        #endregion DiffentApproachOfKeepingCarAboveTransporter

        PlayerContainedData.PlayerData.PlayerInformation[0].UpwardPos = posYToReach;

        PlayerContainedData.PlayerBasicMovementShared.movement();

        nextJumpTimer -= Time.deltaTime;

        if (!PlayerSharedData.IsGrounded)
        {
            PlayerSharedData.InAir = true;
        }
    }

    //private void Rotation(Vector3 rotationTarget, float rotationSpeed)
    //{
    //    Vector3 forwardProjection = Vector3.ProjectOnPlane(Vector3.forward, rotationTarget);
    //    Quaternion rotaiton = Quaternion.LookRotation(forwardProjection, rotationTarget);
    //    rotaiton = Quaternion.RotateTowards(PlayerSharedData.PlayerTransform.rotation, rotaiton, Time.fixedDeltaTime * rotationSpeed);
    //    PlayerSharedData.PlayerRigidBody.MoveRotation(rotaiton);
    //}

    private RaycastHit GetHoverPositionIfGrounded()
    {
        RaycastHit thehit;

        Vector3 origin = PlayerSharedData.RaycastOriginPosition.transform.position;

        Physics.BoxCast(origin, PlayerSharedData.BoxColliderbounds, -PlayerSharedData.PlayerTransform.up, out thehit, Quaternion.identity, PlayerSharedData.wallslideDoing ? PlayerContainedData.PlayerData.PlayerInformation[0].RayHeightDuringWallRun : PlayerContainedData.PlayerData.PlayerInformation[0].AdjustmentRaycastHeight, PlayerContainedData.PlayerData.PlayerInformation[0].CarLayer, QueryTriggerInteraction.Ignore);

        return thehit;
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

    //start jump is called when jump start
    private void startJump()
    {
        PlayerPrefs.SetInt("doublejump", PlayerPrefs.GetInt("doublejump") + 1);
        if (PlayerPrefs.GetInt("doublejump") >= 2)
        {
            return;
        }

        jumpStartPos = PlayerSharedData.PlayerTransform.position;
        nextJumpTimer = 0;

        height = PlayerSharedData.SpringJump ? PlayerContainedData.PlayerData.PlayerInformation[0].SpringJumpHeight : PlayerContainedData.PlayerData.PlayerInformation[0].jump_height;
        jumpEndPos = new Vector3(PlayerSharedData.PlayerTransform.position.x, PlayerSharedData.PlayerTransform.position.y, PlayerSharedData.PlayerTransform.position.z + PlayerContainedData.PlayerData.PlayerInformation[0].jump_length);
        //PlayerContainedData.AnimationChannel.Jump(AnimatorParameters.jumpspeed, PlayerSharedData.JumpDuration, PlayerSharedData.PlayerAnimator);
    }

    //will return jump vector and thorugh that vector we would move obj to y and z value.
    private Vector3 jump()
    {
        jumpTime = Mathf.Clamp01(MyExtensions.elapse_time(ref elapsedTime, PlayerSharedData.JumpDuration));
        if ((PlayerSharedData.IsGrounded && PlayerSharedData.CurrentGroundColliderPlayerIsInContactWith != null && PlayerSharedData.CurrentGroundColliderPlayerIsInContactWith.CompareTag("AdjustHeightDuringJump") && PlayerSharedData.InAir) || jumpTime > .5f)
        {
            // UnityEngine.Console.Log($"Took {elapsedTime} to jump");

            PlayerSharedData.InAir = false;

            StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToNormalMovement);

            elapsedTime = 0;
        }

        Vector3 my = MathParabola.Parabola(jumpStartPos, jumpEndPos, height, jumpTime);

        return my;
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

    private void SwipeUp()
    {
        nextJumpTimer = PlayerContainedData.PlayerData.PlayerInformation[0].NextJumpTimer;
        //if (PlayerPrefs.GetInt("doublejump") < 1 && saveManager.MainSaveFile.currentlySelectedCharacter == 1)
        //{
        //    playerIsDoubleJumping.RaiseEvent();
        //    OnEnter();
        //}

        StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToJump);
    }

    private void cancelJump()
    {
        stateExit = false;
        elapsedTime = 0;
        jumpStartPos = Vector3.zero;
        jumpEndPos = Vector3.zero;
        jumpDistance = 0;
        speedTimeToJump = 0;
        jumpTime = 0;
        PlayerSharedData.InAir = false;
    }

    private void Dash()
    {
        if (!PlayerSharedData.IsDash && !PlayerSharedData.IsBoosting && !PlayerSharedData.WallRunBuilding && !PlayerSharedData.RotateOnbuilding)
        {
            PlayerContainedData.PlayerBoostState.StartDash();
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
}