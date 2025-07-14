using FSM;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSharedData", menuName = "ScriptableObjects/PlayerSharedData")]
public class PlayerSharedData : ScriptableObject
{
    public string CurrentStateName;
    public Environment playerCurrentEnvironment;
    public StateMachine PlayerStateMachine;
    public Rigidbody PlayerRigidBody;
    public Animator PlayerAnimator, CharacterAnimator, ChaserAnimator, ChaserCharacterAnimator;
    public Transform PlayerTransform;
    public Transform ChaserTransform;
    public Transform DiamondTargetPoint;
    public GameObject RaycastOriginPosition;
    public BoxCollider Playercollider, PlayerHitCollider;
    public CarSkeleton CarSkeleton;
    public CharacterSkeleton CharacterSkeleton;
    public Transform PivotT;
    public Transform[] RaycastT;
   // public List<Transform> BuildingRunPath = new List<Transform>();
    public BezierCurve bezierBuildingCurve;
    public float Distancecovered;
    public Vector3 defaultHitColliderSize, defaultHitColliderCenter;
    public ObjectDestruction LastObstacleDestroyed { get; set; }
    public bool isInInvincibleZoneDuringDash;

    public Transform WallrunbuildingRay;

    public float LastGroundedYPosition { get; set; }
    public Collider CurrentGroundColliderPlayerIsInContactWith { get; set; }

    [SerializeField] private PlayerContainedData playerContainedData;

    //    [HideInInspector]
    public bool InAir, IsDash, IsBoosting, IsGrounded, HalfDashCompleted, IsIdle, CanWallRun, WallRunBuilding, isStumbled, RotateOnbuilding,
        WallClimbDoing, wallslideDoing, SpringJump, ChaserWasHit, IsArmour, isCrashed, isThursting, isHalfThurstingCompleted, isBeingCapturedByChaser; //Making bools for Dash and Boost instead of State because all movement is same except forward speed.

    public bool isMagnetActive;
    public bool isJumping;
    public bool isJumpingPhase;
    public bool isFalling;
    public bool outsideNormalLane;
    public bool AeroplaneTakingOff;

    // [HideInInspector]
    public Vector3 BoxColliderbounds;

    //  [HideInInspector]
    public float ForwardSpeed, SidewaysSpeed, SlideSpeed, JumpDuration;

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
        isMagnetActive = false;
        isInInvincibleZoneDuringDash = false;
        isHalfThurstingCompleted = true;
        isThursting = false;
        isJumpingPhase = false;
        isJumping = false;
        outsideNormalLane = false;
        AeroplaneTakingOff = false;
        Distancecovered = 0;
        ForwardSpeed = playerContainedData.PlayerData.PlayerInformation[0].ForwardInitialSpeed;
        SidewaysSpeed = playerContainedData.PlayerData.PlayerInformation[0].SideWaysInitalSpeed;
        SlideSpeed = playerContainedData.PlayerData.PlayerInformation[0].SlideInitialSpeed;
        JumpDuration = playerContainedData.PlayerData.PlayerInformation[0].JumpDurationInitialValue;
        InAir = SpringJump = IsArmour = IsDash = IsBoosting = IsIdle = CanWallRun = WallRunBuilding = isStumbled = RotateOnbuilding = ChaserWasHit = WallClimbDoing = wallslideDoing = isCrashed = isBeingCapturedByChaser = false;
        IsGrounded = true;
        HalfDashCompleted = true;
        isFalling = false;
     //   BuildingRunPath.Clear();
    }
}