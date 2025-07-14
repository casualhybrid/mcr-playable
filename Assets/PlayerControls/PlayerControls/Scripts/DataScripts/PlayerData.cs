using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerInformation
{
    [Header("Forward Speed Values")]
    [Tooltip("speed mulitiplier that will incrase speed over lifetime")]
    [SerializeField] public float ForwarspeedMultiplier;

    [Tooltip("Value From which Forward speed should start")]
    [SerializeField] public float ForwardSpeedInitialValue;

    [Tooltip("maximum value of forward speed")]
    [SerializeField] public float MaxForwardSpeed;

    [Header("Jump Values")]
    [Tooltip("speed with which you want to cancel jump in air")]
    [SerializeField] public float canacelJumpSpeed;

    [Tooltip("length of jump")]
    [SerializeField] public float jump_length;

    [Tooltip("height of jump")]
    [SerializeField] public float jump_height;

    [Tooltip("value with which speed should increase")]
    [SerializeField] public float JumpDurationInitialValue;

    [Tooltip("/calculating time for next time jump , if swiped in air , timer will start and if timer is greater than 0 at landing it will jump again")]
    [SerializeField] public float NextJumpTimer;

    [Tooltip("Height of Jump on spring")]
    [SerializeField] public float SpringJumpHeight;

    [Header("Sideways Movement Values")]
    [Tooltip("speed sidewway of player")]
    [SerializeField] public float SideWaysMultiplier;

    [Tooltip("Initial Speed for Sideway movement")]
    [SerializeField] public float SideWaysInitalSpeed;

    [Tooltip("Maximum speed for sideways")]
    [SerializeField] public float MaxSideWaySpeed;

    [Tooltip("distance to move left and right")]
    [SerializeField] public float xDistToCover;

    [Tooltip("total no of lanes in which onject can move")]
    [SerializeField] public int totalLanes;

    [Header("Sliding  State Values")]
    [Tooltip("speed of slide state")]
    [SerializeField] public float SlideInitialSpeed;

    [Tooltip("speed of slide state")]
    [SerializeField] public float SlideMultiplier;

    [Tooltip("Max speed for slider")]
    [SerializeField] public float MaxSlideSpeed;

    //[Tooltip("Y position of HurtBox when sliding")]
    //[SerializeField] public float HurtBoxYPosDuringSlide;

    [Header("Values For Shockwave")]
    [SerializeField] public float ShockWaveRadius;

    [Header("Rotation Speed Values")]
    [Tooltip("rotation speed on ramp")]
    [SerializeField] public float RampRotationSpeed;

    [Tooltip("rotation speed on ramp")]
    [SerializeField] public float curvedRampRotationSpeed;

    [Tooltip("rotation speed on Building")]
    [SerializeField] public float BuildingClimbRotationSpeed;

    [Tooltip("rotation speed on Building")]
    [SerializeField] public float BuildingFallingRotationSpeed;

    [Header("Animation Curves")]
    [Tooltip("Curve For Falling")]
    [SerializeField] public AnimationCurve FallCurve;

    [Header("Flying State Values")]
    [Tooltip("Max speed for Flying")]
    [SerializeField] public float FlyingMaxSpeed;

    [Tooltip("Time in which you want to reach max speed")]
    [SerializeField] public float TimeToReachFlyingMaxSpeed;

    [Tooltip("Time in which you want to reach Flying Top Height")]
    [SerializeField] public float TimeToReachFlyingTopHeight;

    [Tooltip("Height on which Player Should Fly")]
    [SerializeField] public float FlyingHeight;

    [Header("Raycast Values")]
    [Tooltip("Height of raycast")]
    [SerializeField] public float RaycastHeight;

    [Tooltip("Hover Height basically refers to y position , thata how much height should be added from ground for perfect detection")]
    [SerializeField] public float HoverHeight;

    [Tooltip("Raycast height for adjusting height value according to hit")]
    [SerializeField] public float AdjustmentRaycastHeight;

    [Tooltip("Raycast height during wall run")]
    [SerializeField] public float RayHeightDuringWallRun;

    [Header("Booster State Values")]
    [Tooltip("Target Speed for booster")]
    [SerializeField] public float Boosterspeed;

    [SerializeField] public Vector3 HurtColliderCenterDuringBoost;
    [SerializeField] public Vector3 HurtColliderSizeDuringBoost;

    [Header("Dash State Values")]
    [Tooltip("Duration time for Dash")]
    public float DashDuration;

    [Tooltip("Target Speed for Dash")]
    public float DashSpeed;

    [Header("Thurst State Values")]
    [Tooltip("Duration For Thurst Jump")]
    [SerializeField] public float ThurstDuration;

    [Tooltip("Length For Thurst Jump")]
    [SerializeField] public float ThurstJumpLength;

    [Tooltip("Height For Thurst Jump")]
    [SerializeField] public float ThurstJumpHeight;

    [Header("Other Values")]
    [Tooltip("Max speed of animations")]
    [SerializeField] public float MaxAnimationSpeed;

    [Tooltip("y speed")]
    [SerializeField] public float UpwardPos;

    [Tooltip("Layer on which car can move")]
    [SerializeField] public LayerMask CarLayer;

    [Tooltip("ramp speed")]
    [SerializeField] public Vector3 PlayerStartinPosition;

    [Tooltip("saving forward speed at the time of Boost and Dash")]
    public float ForwardInitialSpeed; // saving inital speed at time of Dash or  Boost

    [Tooltip("Duration time for curved ramp")]
    public float CurvedRampDuration;

    [Tooltip("Maximum step a character can take")]
    public float StepHeight;
}

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData")]
public class PlayerData : ScriptableObject
{
    [SerializeField] public List<PlayerInformation> PlayerInformation = new List<PlayerInformation>();

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

    public void ResetVariable()
    {
        PlayerInformation[0].UpwardPos = 0f;
        PlayerInformation[0].Boosterspeed = 3f;

      //  Animator_controller.LastAniamtion = AnimatorParameters.normal;
    }
}