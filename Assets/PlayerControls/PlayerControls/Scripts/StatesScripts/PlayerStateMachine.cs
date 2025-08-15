using FSM;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TheKnights.SaveFileSystem;
using System.Collections;

public static class PlayerState
{
    public const string PlayerNormalMovementState = "PlayerNormalMovementState";
    public const string PlayerJumpState = "PlayerJumpState";
    public const string PlayerCanceljump = "PlayerCanceljump";
    public const string PlayerSlideState = "PlayerSlideState";
    public const string PlayerBoostState = "PlayerBoostState";
    public const string PlayerThurstState = "PlayerThurstState";
    public const string PlayerAeroplaneState = "PlayerAeroplaneState";
    public const string PlayerDeathState = "PlayerDeathState";
    public const string PlayerBuildingRunState = "PlayerBuildingRunState";
}

public static class PlayerStateEvent
{
    public const string ToJump = "ToJump";
    public const string ToNormalMovement = "ToNormalMovement";
    public const string ToJumpCancel = "ToJumpCancel";
    public const string ToSlideState = "ToSlideState";
    public const string ToThurstState = "ToThurstState";
    public const string ToBuildingRunState = "ToBuildingRunState";
    public const string ToPlayerAeroplaneState = "ToPlayerAeroplaneState";
    public const string ToDead = "ToDead";
}


public class PlayerStateMachine : MonoBehaviour, IFloatingReset
{
    public bool ShoudNotOffsetOnRest { get; set; }

    [SerializeField] private GameManager gameManager;
    [SerializeField] private PlayerSharedData PlayerSharedData;
    [SerializeField] private PlayerContainedData PlayerContainedData;

    [SerializeField] private Text currentstate;
    [SerializeField] private Rigidbody MyRigidbody;
    [SerializeField] private BoxCollider boxCollider;
    [SerializeField] private BoxCollider HitCollider;

    [SerializeField] private GameObject raycastOriginPosition;

    [SerializeField] private Transform[] RayT;
    [SerializeField] private Transform pivotT;

    [SerializeField] private Transform WallRunbuildingRay;

    public StateMachine MyStateMachine { get; set; }

    // GameEvents
    [SerializeField] private GameEvent gameHasStarted;
    [SerializeField] private GameEvent playerHasSpawned;
    [SerializeField] private GameEvent playerHasCrashed;
    [SerializeField] private GameEvent headStartTapped;
    [SerializeField] private GameEvent playerHasRevived;
   

    [Space(10)]
    [SerializeField] private SaveManager saveManager;
    public bool isOneTime = true;

    public UnityEvent tempEvent;

    private bool isInitialized;

    //    private string activestate;

    private void OnEnable()
    {
        gameHasStarted.TheEvent.AddListener(HandleGameStarted);
        playerHasCrashed.TheEvent.AddListener(HandlePlayerCrashed);
        headStartTapped.TheEvent.AddListener(HandleHeadStartTapped);
        playerHasRevived.TheEvent.AddListener(HandleplayerHasRevived);

        PlayerContainedData.Animator_controller.SubscribeEvents();
    }

    private void OnDisable()
    {
        gameHasStarted.TheEvent.RemoveListener(HandleGameStarted);
        playerHasCrashed.TheEvent.RemoveListener(HandlePlayerCrashed);
        headStartTapped.TheEvent.RemoveListener(HandleHeadStartTapped);
        playerHasRevived.TheEvent.RemoveListener(HandleplayerHasRevived);

        PlayerContainedData.Animator_controller.UnSubscribeEvents();
    }

    private void HandleHeadStartTapped(GameEvent arg0)
    {
        StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToPlayerAeroplaneState);

        tempEvent.Invoke();
    }

    private void Awake()
    {
        MyStateMachine = ScriptableObject.CreateInstance("StateMachine") as StateMachine;
        MyStateMachine.StateChanged += ActiveStateChanged;

        PlayerSharedData.PlayerStateMachine = MyStateMachine;
        PlayerSharedData.PlayerRigidBody = MyRigidbody;
        PlayerSharedData.PlayerTransform = GetComponent<Transform>();
        PlayerSharedData.RaycastT = RayT;
        PlayerSharedData.PivotT = pivotT;
        PlayerSharedData.Playercollider = boxCollider;
        PlayerSharedData.PlayerHitCollider = HitCollider;
        PlayerSharedData.defaultHitColliderCenter = HitCollider.center;
        PlayerSharedData.defaultHitColliderSize = HitCollider.size;

        PlayerSharedData.WallrunbuildingRay = WallRunbuildingRay;
        //   PlayerSharedData.BoxColliderbounds = PlayerSharedData.Playercollider.bounds.size / 2;
        PlayerSharedData.RaycastOriginPosition = raycastOriginPosition;
        PlayerSharedData.InAir = PlayerSharedData.IsDash = PlayerSharedData.IsBoosting = PlayerSharedData.isStumbled = false;
        //  PlayerSharedData.PlayerTransform.position = PlayerContainedData.PlayerData.PlayerInformation[0].PlayerStartinPosition;
        PlayerContainedData.PlayerBasicMovementShared.distForRayCast = boxCollider.bounds.extents.y;


    
    }

    private void OnDestroy()
    {
        if (MyStateMachine != null)
        {
            MyStateMachine.StateChanged -= ActiveStateChanged;
            Destroy(MyStateMachine);
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        MyStateMachine.AddState(PlayerState.PlayerNormalMovementState, PlayerContainedData.PlayerNormalMovementState);
        MyStateMachine.AddState(PlayerState.PlayerJumpState, PlayerContainedData.PlayerJumpState);
        MyStateMachine.AddState(PlayerState.PlayerCanceljump, PlayerContainedData.PlayerCanceljump);
        MyStateMachine.AddState(PlayerState.PlayerSlideState, PlayerContainedData.PlayerSlideState);
        MyStateMachine.AddState(PlayerState.PlayerBoostState, PlayerContainedData.PlayerBoostState);
        MyStateMachine.AddState(PlayerState.PlayerThurstState, PlayerContainedData.PlayerThurstState);
        MyStateMachine.AddState(PlayerState.PlayerAeroplaneState, PlayerContainedData.PlayerAeroplaneState);
        MyStateMachine.AddState(PlayerState.PlayerDeathState, PlayerContainedData.PlayerDeathState);
        MyStateMachine.AddState(PlayerState.PlayerBuildingRunState, PlayerContainedData.PlayerBuildingRunState);

        MyStateMachine.SetStartState(PlayerState.PlayerNormalMovementState);
        //if (PlayerPrefs.GetInt("SelectedCharacter") == 1)
        //{
        //    UnityEngine.Console.LogError("I am Ari");
        //    MyStateMachine.AddTransition(new Transition(PlayerState.PlayerNormalMovementState, PlayerState.PlayerJumpState, PlayerStateEvent.ToJump, (transition) => !PlayerSharedData.WallRunBuilding && !PlayerSharedData.RotateOnbuilding /*&& (*//*PlayerSharedData.IsGrounded ||*/ /*(PlayerContainedData.elapsedTimeRelaxationAfterFall <= gameManager.GetRelaxTimerForJump)*/ /*&& !PlayerSharedData.isJumpingPhase)*/&& (PlayerPrefs.GetInt("doublejump") < 1) && !PlayerSharedData.wallslideDoing));
        //}
        //else
        //{
        //    UnityEngine.Console.LogError("I am character no:" + PlayerPrefs.GetInt("SelectedCharacter"));
        //    MyStateMachine.AddTransition(new Transition(PlayerState.PlayerNormalMovementState, PlayerState.PlayerJumpState, PlayerStateEvent.ToJump, (transition) => !PlayerSharedData.WallRunBuilding && !PlayerSharedData.RotateOnbuilding && (PlayerSharedData.IsGrounded || (PlayerContainedData.elapsedTimeRelaxationAfterFall <= gameManager.GetRelaxTimerForJump) && !PlayerSharedData.isJumpingPhase) && !PlayerSharedData.wallslideDoing));
        //}

        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerNormalMovementState, PlayerState.PlayerJumpState, PlayerStateEvent.ToJump, (transition) => HandleCharactersTransition()));

       // if (saveManager.MainSaveFile.currentlySelectedCharacter == 1)
       // {
            MyStateMachine.AddTransition(new Transition(PlayerState.PlayerJumpState, PlayerState.PlayerJumpState, PlayerStateEvent.ToJump, (transition) => HandleJumpToJumpTransition()));
    //    }

        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerJumpState, PlayerState.PlayerNormalMovementState, PlayerStateEvent.ToNormalMovement));
        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerJumpState, PlayerState.PlayerCanceljump, PlayerStateEvent.ToJumpCancel, (transition) => !PlayerSharedData.WallClimbDoing));
        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerNormalMovementState, PlayerState.PlayerCanceljump, PlayerStateEvent.ToJumpCancel, (transition) => !PlayerSharedData.IsGrounded && !PlayerSharedData.WallClimbDoing));
        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerCanceljump, PlayerState.PlayerNormalMovementState, PlayerStateEvent.ToNormalMovement));
        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerNormalMovementState, PlayerState.PlayerSlideState, PlayerStateEvent.ToSlideState, (transition) => PlayerSharedData.IsGrounded && !PlayerSharedData.WallRunBuilding && !PlayerSharedData.RotateOnbuilding));
        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerSlideState, PlayerState.PlayerNormalMovementState, PlayerStateEvent.ToNormalMovement));
        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerSlideState, PlayerState.PlayerJumpState, PlayerStateEvent.ToJump, (transition) => !PlayerSharedData.WallRunBuilding && !PlayerSharedData.RotateOnbuilding && (PlayerSharedData.IsGrounded || (PlayerContainedData.elapsedTimeRelaxationAfterFall <= gameManager.GetRelaxTimerForJump) && !PlayerSharedData.isJumpingPhase) && !PlayerSharedData.wallslideDoing));
        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerCanceljump, PlayerState.PlayerSlideState, PlayerStateEvent.ToSlideState));
        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerBoostState, PlayerState.PlayerNormalMovementState, PlayerStateEvent.ToNormalMovement));
        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerNormalMovementState, PlayerState.PlayerThurstState, PlayerStateEvent.ToThurstState));
        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerNormalMovementState, PlayerState.PlayerCanceljump, PlayerStateEvent.ToJumpCancel, (transition) => !PlayerSharedData.WallClimbDoing));

        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerCanceljump, PlayerState.PlayerThurstState, PlayerStateEvent.ToThurstState));
        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerSlideState, PlayerState.PlayerThurstState, PlayerStateEvent.ToThurstState));
        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerJumpState, PlayerState.PlayerThurstState, PlayerStateEvent.ToThurstState));
        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerNormalMovementState, PlayerState.PlayerBuildingRunState, PlayerStateEvent.ToBuildingRunState));
        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerJumpState, PlayerState.PlayerBuildingRunState, PlayerStateEvent.ToBuildingRunState));
        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerSlideState, PlayerState.PlayerBuildingRunState, PlayerStateEvent.ToBuildingRunState));
        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerCanceljump, PlayerState.PlayerBuildingRunState, PlayerStateEvent.ToBuildingRunState));

        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerBuildingRunState, PlayerState.PlayerNormalMovementState, PlayerStateEvent.ToNormalMovement));

        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerSlideState, PlayerState.PlayerAeroplaneState, PlayerStateEvent.ToPlayerAeroplaneState));
        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerCanceljump, PlayerState.PlayerAeroplaneState, PlayerStateEvent.ToPlayerAeroplaneState));
        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerJumpState, PlayerState.PlayerAeroplaneState, PlayerStateEvent.ToPlayerAeroplaneState));
        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerThurstState, PlayerState.PlayerAeroplaneState, PlayerStateEvent.ToPlayerAeroplaneState));
        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerThurstState, PlayerState.PlayerCanceljump, PlayerStateEvent.ToJumpCancel, (transition) => !PlayerSharedData.WallClimbDoing));
        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerCanceljump, PlayerState.PlayerJumpState, PlayerStateEvent.ToJump, (transition) => !PlayerSharedData.WallRunBuilding && !PlayerSharedData.RotateOnbuilding && (PlayerSharedData.IsGrounded || (PlayerContainedData.elapsedTimeRelaxationAfterFall <= gameManager.GetRelaxTimerForJump) && !PlayerSharedData.isJumpingPhase)));

        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerThurstState, PlayerState.PlayerNormalMovementState, PlayerStateEvent.ToNormalMovement));
        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerThurstState, PlayerState.PlayerJumpState, PlayerStateEvent.ToJump, (transition) => !PlayerSharedData.WallRunBuilding && !PlayerSharedData.RotateOnbuilding && (PlayerSharedData.IsGrounded || (PlayerContainedData.elapsedTimeRelaxationAfterFall <= gameManager.GetRelaxTimerForJump) && !PlayerSharedData.isJumpingPhase)));

        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerNormalMovementState, PlayerState.PlayerAeroplaneState, PlayerStateEvent.ToPlayerAeroplaneState));
        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerAeroplaneState, PlayerState.PlayerNormalMovementState, PlayerStateEvent.ToNormalMovement));
        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerAeroplaneState, PlayerState.PlayerBuildingRunState, PlayerStateEvent.ToBuildingRunState));

        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerNormalMovementState, PlayerState.PlayerDeathState, PlayerStateEvent.ToDead));
        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerJumpState, PlayerState.PlayerDeathState, PlayerStateEvent.ToDead));
        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerCanceljump, PlayerState.PlayerDeathState, PlayerStateEvent.ToDead));
        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerSlideState, PlayerState.PlayerDeathState, PlayerStateEvent.ToDead));
        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerBoostState, PlayerState.PlayerDeathState, PlayerStateEvent.ToDead));
        //MyStateMachine.AddTransition(new Transition(PlayerState.PlayerThurstState, PlayerState.PlayerDeathState, PlayerStateEvent.ToDead));
        //  MyStateMachine.AddTransition(new Transition(PlayerState.PlayerAeroplaneState, PlayerState.PlayerDeathState, PlayerStateEvent.ToDead));

        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerDeathState, PlayerState.PlayerNormalMovementState, PlayerStateEvent.ToNormalMovement));
        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerDeathState, PlayerState.PlayerBuildingRunState, PlayerStateEvent.ToBuildingRunState));

        MyStateMachine.AddTransition(new Transition(PlayerState.PlayerBuildingRunState, PlayerState.PlayerDeathState, PlayerStateEvent.ToDead));

        MyStateMachine.DeSubscribeEvents();
        //MyStateMachine.OnEnter();
    }

    private void ActiveStateChanged(StateBase state)
    {
        PlayerSharedData.CurrentStateName = state.name;
    }

    private void HandleGameStarted(GameEvent theEvent)
    {
        isInitialized = true;
        MyStateMachine.OnEnter();
    }

    private void HandlePlayerCrashed(GameEvent theEvent)
    {
        //  if (PlayerSharedData.CurrentStateName != PlayerState.PlayerBuildingRunState)

        string curState = PlayerSharedData.CurrentStateName;

        //if(curState == PlayerState.PlayerSlideState)
        //{
        //    UnityEngine.Console.LogError("PLAYER COLLIDED DURING SLIDE STATE");
        //}

        StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToDead);
      
            
    }

    private bool HandleJumpToJumpTransition()
    {
        return  saveManager.MainSaveFile.currentlySelectedCharacter == 1 && !PlayerSharedData.WallRunBuilding && !PlayerSharedData.RotateOnbuilding &&  (PlayerPrefs.GetInt("doublejump") < 1) && !PlayerSharedData.wallslideDoing;

    }

    private bool HandleCharactersTransition()
    {
        if (saveManager.MainSaveFile.currentlySelectedCharacter == 1 && PlayerSharedData.isJumpingPhase)
        {
          return !PlayerSharedData.WallRunBuilding && !PlayerSharedData.RotateOnbuilding &&  PlayerPrefs.GetInt("doublejump") < 1 && !PlayerSharedData.wallslideDoing;
        }
        else
        {
            return !PlayerSharedData.WallRunBuilding && !PlayerSharedData.RotateOnbuilding && (PlayerSharedData.IsGrounded || (PlayerContainedData.elapsedTimeRelaxationAfterFall <= gameManager.GetRelaxTimerForJump) && !PlayerSharedData.isJumpingPhase) && !PlayerSharedData.wallslideDoing;
        }
    }

    public bool IsJumpAllowedIfTransitionExists()
    {
        return !PlayerSharedData.WallRunBuilding && !PlayerSharedData.RotateOnbuilding && (PlayerSharedData.IsGrounded || (PlayerContainedData.elapsedTimeRelaxationAfterFall <= gameManager.GetRelaxTimerForJump) && !PlayerSharedData.isJumpingPhase) && !PlayerSharedData.wallslideDoing;
    }

    private void Update()
    {
      
 

        if (!isInitialized)
        {
            return;
        }

        MyStateMachine.OnLogic();

        PlayerContainedData.SpeedHandler.IncreaseSpeed();

        if (PlayerSharedData.CanWallRun)
        {
            PlayerContainedData.PlayerData.PlayerInformation[0].totalLanes = 2;
        }
        else if (!PlayerSharedData.CanWallRun)
        {
            PlayerContainedData.PlayerData.PlayerInformation[0].totalLanes = 1;
        }

        if (GameManager.IsGameStarted && !PlayerSharedData.isCrashed)
        {
            GameManager.gameplaySessionTimeInSeconds += Time.deltaTime;
        }
        
    }
    public float temp;
    public bool isPlayerRest = false;
    private void FixedUpdate()
    {
        if (!isInitialized)
        {
            return;
        }

        temp = gameObject.transform.position.z ;

        if (temp >= 1995)
        {
            isPlayerRest = false;
            if (isOneTime)
            {
            Debug.LogError("Challa hai");
                isOneTime = false;
                StartCoroutine(Delay(true));

               
            }
        }
       /* if (isOneTime == false)
        {
            if (temp >= 50 && temp <= 60 && isPlayerRest == false)
            {
                isOneTime = true;
                isPlayerRest = true;
                MyRigidbody.interpolation = RigidbodyInterpolation.None;  //zzzn
                MyRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            }
        }*/

        MyStateMachine.OnFixedLogic();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isInitialized)
        {
            return;
        }

        MyStateMachine.OnTriggerEnter(other);
        Debug.LogError("TrigerHere:");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isInitialized)
        {
            return;
        }

        MyStateMachine.OnTriggerExit(other);
    }

    private void HandleplayerHasRevived(GameEvent gameEvent)
    {
        if (PlayerSharedData.WallRunBuilding)
        {
            MyStateMachine.RequestStateChange(PlayerState.PlayerBuildingRunState);
        }
        else
        {
            MyStateMachine.RequestStateChange(PlayerState.PlayerNormalMovementState);
        }
    }

    public void OnFloatingPointReset(float movedOffset)
    {

     //   StartCoroutine(Delay(false));
    }

    public void OnBeforeFloatingPointReset()
    {
        // MyRigidbody.interpolation = RigidbodyInterpolation.None;  //zzzn
       // StartCoroutine(Delay(true));
    }

   
    IEnumerator Delay(bool isBefore)
    {
        if (isBefore)
        {

            MyRigidbody.interpolation = RigidbodyInterpolation.None;  //zzzn
            MyRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;  //zzzn
            yield return new WaitForSecondsRealtime(10);
            MyRigidbody.interpolation = RigidbodyInterpolation.Interpolate;  //zzzn
            MyRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

        }
        else
        {
            MyRigidbody.interpolation = RigidbodyInterpolation.Interpolate;  //zzzn
            MyRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            yield return new WaitForSecondsRealtime(3);
            MyRigidbody.interpolation = RigidbodyInterpolation.None;  //zzzn
            MyRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;  //zzzn
        }
    }
}