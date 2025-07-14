using Cinemachine;
using UnityEngine;
using DG.Tweening;

public class CameraMovement : CinemachineExtension, IFloatingReset
{
    [Tooltip("Accessing mian virtual cam for modyfiying certain values")]
    [SerializeField] private CinemachineVirtualCamera CinemachineVirtualCamera; //Accessing mian virtual cam for modyfiying certain values

    [Tooltip("Player shared Data contains all public variabls which are required in other scripts also")]
    [SerializeField] private PlayerSharedData PlayerSharedData;

    [SerializeField] private PlayerContainedData PlayerContainedData;
    [SerializeField] private GameEvent playerHasJumped;
    [SerializeField] private GameEvent playerHasTouchedGround;
  //  private CinemachineFramingTransposer CinemachineFramingTransposer; // For changing values for transposer

    [SerializeField] private float offset;
    [SerializeField] private float jumpOffset;
    [SerializeField] Ease jumpOffsetEase;
    [SerializeField] Ease nomralOffsetEase;
    [SerializeField] float offsetSwitchTime;


    [SerializeField] private float yFollowSpeed;
    [SerializeField] private float yFollowSpeedCatchupSpeed;
    [SerializeField] private float yFollowSpeedDistIncreased;
    [SerializeField] private float inAirSpeed;
    [SerializeField] private float resettingSpeed;


    private float currentlyUsedOffset;
    private float currentYFollowSpeed;

    private bool isResetting;
    [SerializeField] private float yFollowSpeedDistDecreased;
    [SerializeField] private float yFollowSpeedDistDecreasedMin;
    [SerializeField] private float offsetAbovePlayerIfminimumYDist;
    [SerializeField] private float minimumYDistToCamera;

    public bool ShoudNotOffsetOnRest { get; set; } = true;

    private Tween offsetTween;

    protected override void Awake()
    {
        base.Awake();

        currentlyUsedOffset = offset;
        SubscribeEvents();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    private void SubscribeEvents()
    {
      
        playerHasJumped.TheEvent.AddListener(HandlePlayerJumped);
        playerHasTouchedGround.TheEvent.AddListener(HandlePlayerTouchedGround);
    }

    private void UnSubscribeEvents()
    {
        playerHasJumped.TheEvent.RemoveListener(HandlePlayerJumped);
        playerHasTouchedGround.TheEvent.RemoveListener(HandlePlayerTouchedGround);
    }

    private void HandlePlayerJumped(GameEvent gameEvent)
    {
        TweenTheCurrentOffset(jumpOffset, jumpOffsetEase);
    }

    private void HandlePlayerTouchedGround(GameEvent gameEvents)
    {
        TweenTheCurrentOffset(offset, nomralOffsetEase);

    }

    private void TweenTheCurrentOffset(float endVal, Ease ease)
    {
       offsetTween?.Kill();

       float curVal = currentlyUsedOffset;
       offsetTween = DOTween.To(() => curVal, (x) => { currentlyUsedOffset = x; }, endVal, offsetSwitchTime).SetEase(ease);

    }
    //private void Update()
    //{
    //    if (CinemachineFramingTransposer != null)
    //    {
    //        float targetX = PlayerSharedData.PlayerTransform.position.x == 0 ? PlayerContainedData.CameraData.xCenterOffset : PlayerSharedData.PlayerTransform.position.x >= 0.7f ? -PlayerContainedData.CameraData.xLanesOffset : PlayerSharedData.PlayerTransform.position.x <= -0.7f ? PlayerContainedData.CameraData.xLanesOffset : CinemachineFramingTransposer.m_TrackedObjectOffset.x;
    //        CinemachineFramingTransposer.m_TrackedObjectOffset.x = Mathf.Lerp(CinemachineFramingTransposer.m_TrackedObjectOffset.x, targetX, PlayerSharedData.SidewaysSpeed);

    //    }
    //}

    private void OnDisable()
    {
        UnSubscribeEvents();
    }

    private void Start()
    {
        SubscribeEvents();
      //  CinemachineFramingTransposer = CinemachineVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
    }

    public void CameraReset()
    {
        // transform.position = Camera.main.transform.position;
        isResetting = true;
    }

    protected override void PostPipelineStageCallback(
    CinemachineVirtualCameraBase vcam,
    CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (!GameManager.IsGamePlayStarted)
            return;

        if (isResetting)
        {
            //UnityEngine.Console.Log("Resetting");

            Vector3 rawPosition = state.RawPosition;
            float y = PlayerSharedData.LastGroundedYPosition + currentlyUsedOffset;
            y = Mathf.MoveTowards(vcam.transform.position.y, y, deltaTime * resettingSpeed);
            rawPosition.y = y;
            state.RawPosition = rawPosition;

            if (transform.position.y - y <= Mathf.Epsilon)
            {
                isResetting = false;
            }

            return;
        }

        if (stage == CinemachineCore.Stage.Finalize)
        {
            float differenceY = vcam.transform.position.y - PlayerSharedData.PlayerTransform.position.y;

            if (differenceY - currentlyUsedOffset >= Mathf.Epsilon && !PlayerSharedData.IsGrounded)
            {
                Vector3 rawPosition = state.RawPosition;
                float y = PlayerSharedData.PlayerTransform.position.y + currentlyUsedOffset;

                y = Mathf.MoveTowards(vcam.transform.position.y, y, deltaTime * yFollowSpeedDistIncreased * Mathf.Pow(differenceY, 2)); //* PlayerSharedData.ForwardSpeed
                rawPosition.y = y;

                state.RawPosition = rawPosition;

                currentYFollowSpeed = 0;
            }
            else
            {
                //   UnityEngine.Console.Log("IsGroundedCamera");

                Vector3 rawPosition = state.RawPosition;

                float y = PlayerSharedData.LastGroundedYPosition + currentlyUsedOffset;
                float speed;

                //if (y < PlayerSharedData.PlayerTransform.position.y || differenceY <= minimumYDistToCamera)
                //{
                //    //UnityEngine.Console.Log("OffsettingCamera");
                //    y = PlayerSharedData.PlayerTransform.position.y + offsetAbovePlayerIfminimumYDist;

                //    //     speed = deltaTime * yFollowSpeedDistDecreased * (1f / Mathf.Pow(differenceY, 4)); //TimeScale
                //    speed = ((yFollowSpeedDistDecreased * Mathf.Clamp(differenceY, 0, differenceY)) + yFollowSpeedDistDecreasedMin) * deltaTime;

                //    currentYFollowSpeed = 0;
                //}
                //  else
                //  {
                //  UnityEngine.Console.Log("GroundedCamera");
                currentYFollowSpeed += yFollowSpeedCatchupSpeed * deltaTime * PlayerSharedData.ForwardSpeed;
                currentYFollowSpeed = Mathf.Clamp(currentYFollowSpeed, 0, yFollowSpeed);
                speed = deltaTime * currentYFollowSpeed * PlayerSharedData.ForwardSpeed;
                //  }

                y = Mathf.MoveTowards(vcam.transform.position.y, y, speed);

                rawPosition.y = y;

                state.RawPosition = rawPosition;
            }

            //if (!PlayerSharedData.IsGrounded)
            //{
            //    float cur = currentYFollowSpeed;
            //    currentYFollowSpeed = Mathf.Clamp(currentYFollowSpeed/1.2f, 0, cur);
            //}
        }
    }

   

    public void OnFloatingPointReset(float movedOffset)
    {
        CinemachineFramingTransposer transposer = CinemachineVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        var pos = transform.position;
        pos.z -= movedOffset;

        transposer.ForceCameraPosition(pos, transform.rotation);
        // CinemachineVirtualCamera.enabled = true;
    }

    public void OnBeforeFloatingPointReset()
    {
        //  CinemachineVirtualCamera.enabled = false;
    }
}