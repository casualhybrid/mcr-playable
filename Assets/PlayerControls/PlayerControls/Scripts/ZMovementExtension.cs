using Cinemachine;
using UnityEngine;

public class ZMovementExtension : CinemachineExtension
{
    [Tooltip("Accessing mian virtual cam for modyfiying certain values")]
    [SerializeField] private CinemachineVirtualCamera CinemachineVirtualCamera; //Accessing mian virtual cam for modyfiying certain values
    [SerializeField] private CinemachineVirtualCamera PreviousCinemachineVirtualCamera;

    [Tooltip("Player shared Data contains all public variabls which are required in other scripts also")]
    [SerializeField] private PlayerSharedData playerSharedData;

    [SerializeField] private CameraData cameraData;

    [SerializeField] private float playerStumbledDist;
    [SerializeField] private float playerCapturedDist;

    //  [SerializeField] private float timerToRevertStumbledDist;
    [SerializeField] private float dashCameraDist;

    [SerializeField] private float playerStumbledFallBackSpeed;
    [SerializeField] private float playerBeingCapturedFallBackSpeed;
    [SerializeField] private float dashCameraFallBackSpeed;
    [SerializeField] private float dashCameraFOVFallBackSpeed;
    [SerializeField] private float boostingCameraFallBackSpeed;

    [SerializeField] private GameEvent boostFinished;
    [SerializeField] private GameEvent dashStarted;
    [SerializeField] private GameEvent dashFinished;
    [SerializeField] private GameEvent playerHaStumbled;

    private CinemachineFramingTransposer cinemachineFramingTransposer;
    private CinemachineFramingTransposer previousCinemachineFramingTransposer;

    private float initialFOV;
    private float defaultCameraDistance;
  //  private bool MaxFOVAchieved = true;
    private float fallbackSpeed;

    //  private float elapsedStumbledCam;

    protected override void Awake()
    {
        base.Awake();
        cinemachineFramingTransposer = CinemachineVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        previousCinemachineFramingTransposer = PreviousCinemachineVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        defaultCameraDistance = cinemachineFramingTransposer.m_CameraDistance;

        SubscribeEvents();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        DeSubscribeEvents();
    }

    private void SubscribeEvents()
    {
        playerHaStumbled.TheEvent.AddListener(HandlePlayerStumbled);
        boostFinished.TheEvent.AddListener(BoostHasFinishedHandle);
        dashStarted.TheEvent.AddListener(DashHasStartedHandle);
        dashFinished.TheEvent.AddListener(DashHasFinishedHandle);
    }

    private void DeSubscribeEvents()
    {
        playerHaStumbled.TheEvent.RemoveListener(HandlePlayerStumbled);
        boostFinished.TheEvent.RemoveListener(BoostHasFinishedHandle);
        dashStarted.TheEvent.RemoveListener(DashHasStartedHandle);
        dashFinished.TheEvent.RemoveListener(DashHasFinishedHandle);
    }

    private void Start()
    {
        initialFOV = CinemachineVirtualCamera.m_Lens.FieldOfView;
    }

    public void CameraReset()
    {
        GetComponent<Animator>().enabled = false;
        CinemachineVirtualCamera.m_Lens.FieldOfView = initialFOV;
        cinemachineFramingTransposer.m_CameraDistance = previousCinemachineFramingTransposer.m_CameraDistance;
    }

    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (!Application.isPlaying)
            return;

        if (stage != CinemachineCore.Stage.Finalize)
            return;

        float cameraDist;

        if (playerSharedData.IsBoosting)
        {
            cameraDist = Mathf.MoveTowards(cinemachineFramingTransposer.m_CameraDistance, cameraData.targetZOffsetBoost, deltaTime * boostingCameraFallBackSpeed);
        }
        else if (playerSharedData.IsDash && !playerSharedData.HalfDashCompleted)
        {
            cameraDist = defaultCameraDistance;
            //cameraDist = Mathf.MoveTowards(cinemachineFramingTransposer.m_CameraDistance, dashCameraDist, deltaTime * dashCameraFallBackSpeed);
        }
        else if (((ChaserRunner.IsChaserEngagingPlayer) || (playerSharedData.isCrashed && GameManager.IsGameStarted)) && !playerSharedData.isBeingCapturedByChaser)  //  else if (playerSharedData.isStumbled && elapsedStumbledCam > 0)
        {
            cameraDist = Mathf.MoveTowards(cinemachineFramingTransposer.m_CameraDistance, playerStumbledDist, deltaTime * playerStumbledFallBackSpeed);
        }
        else if (playerSharedData.isCrashed && playerSharedData.isBeingCapturedByChaser)  //  else if (playerSharedData.isStumbled && elapsedStumbledCam > 0)
        {
            cameraDist = Mathf.MoveTowards(cinemachineFramingTransposer.m_CameraDistance, playerCapturedDist, deltaTime * playerBeingCapturedFallBackSpeed);
        }
        else
        {
            cameraDist = Mathf.MoveTowards(cinemachineFramingTransposer.m_CameraDistance, defaultCameraDistance, deltaTime * fallbackSpeed);
        }
        //if (MaxFOVAchieved)
        //{
        //    if (CinemachineVirtualCamera.m_Lens.FieldOfView > 80)
        //    {
        //        CinemachineVirtualCamera.m_Lens.FieldOfView = CinemachineVirtualCamera.m_Lens.FieldOfView - deltaTime * dashCameraFOVFallBackSpeed;
        //    }
        //}
        //else
        //{
        //    if (CinemachineVirtualCamera.m_Lens.FieldOfView < 90)
        //    {
        //        CinemachineVirtualCamera.m_Lens.FieldOfView = CinemachineVirtualCamera.m_Lens.FieldOfView + (1.5f * deltaTime * dashCameraFOVFallBackSpeed);
        //    }
        //    else
        //    {
        //        MaxFOVAchieved = true;
        //    }
        //}

       // if(!playerSharedData.IsDash)
            cinemachineFramingTransposer.m_CameraDistance = cameraDist;

        //  elapsedStumbledCam -= Time.deltaTime;
    }

    private void BoostHasFinishedHandle(GameEvent theEvent)
    {
        fallbackSpeed = boostingCameraFallBackSpeed;
    }

    private void HandlePlayerStumbled(GameEvent theEvent)
    {
        //    elapsedStumbledCam = timerToRevertStumbledDist;
        fallbackSpeed = playerStumbledFallBackSpeed;
    }

    private void DashHasStartedHandle(GameEvent theEvent)
    {
        //UnityEngine.Console.Log("Player started dash ZMovement");
        //CinemachineVirtualCamera.m_Lens.FieldOfView = 90;
       // MaxFOVAchieved = false;
    }

    private void DashHasFinishedHandle(GameEvent theEvent)
    {
        //UnityEngine.Console.Log("Player finished dash ZMovement");
        fallbackSpeed = dashCameraFallBackSpeed / 2;
    }
}