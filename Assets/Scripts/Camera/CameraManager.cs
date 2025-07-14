using UnityEngine;

using TheKnights.SaveFileSystem;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private GameObject BuildingRunCamera;
    [SerializeField] private GameObject BuildingFallCamera;
    [SerializeField] private GameObject playerCarCamera;
    [SerializeField] private GameObject pogostickCamera;
    [SerializeField] private GameObject springJumpCamera;
    [SerializeField] private GameObject aeroPlaneCamera;
    [SerializeField] private GameObject fallingCamera;
    [SerializeField] private GameObject doubleJumpCamera;
 // [SerializeField] private GameObject jumpCamera;

    [SerializeField] private GameEvent playerTouchedTheGround;
    [SerializeField] private GameEvent playerHasStoppedVerticalClimb;
    [SerializeField] private GameEvent playerStartedBuildingRun;
    [SerializeField] private GameEvent playerCompletedWallClimbTouchedGround;
    [SerializeField] private GameEvent gameHasStarted;
    [SerializeField] private GameEvent playerHasPickedPogoStick;
    [SerializeField] private GameEvent playerHasPickedAeroPlanePower;
    [SerializeField] private GameEvent aeroPlaneHasFinished;
    [SerializeField] private GameEvent pogoStickHasEnded;
    [SerializeField] private GameEvent playerDoubleJumping;
  //  [SerializeField] private GameEvent playerHasJumped;
    [SerializeField] private GameEvent playerSpringJumped;


    [SerializeField] private CameraShakeVariations cameraShakeVariations;
    [SerializeField] private HapticsHandler HapticsHandler;
    [SerializeField] private CameraResolutionDownScaleSO cameraResolutionDownScale;

    [SerializeField] private PlayerSharedData playerSharedData;

    public static CameraManager Instance;
    public GameObject player;
   /* public GameObject camera;
    public GameObject cameraParent;*/
    public GameObject parent;


    private float shakeTimer;

    private void Awake()
    {
        Instance = this;
        SubscribeEvents();
        cameraResolutionDownScale.CalculateRenderScaleValue();
    }

    private void OnDestroy()
    {
        UnSubscribeEvents();
    }

    private void SubscribeEvents()
    {
        aeroPlaneHasFinished.TheEvent.AddListener(HandlePlayerStoppedPogo);
        cameraShakeVariations.OnShakeTheCamera.AddListener(HandleShakeTheCamera);
        cameraShakeVariations.OnShakeEnded.AddListener(HandleShakeEnded);
        playerHasPickedAeroPlanePower.TheEvent.AddListener(HandlePlayerPickedAeroPlane);
        playerTouchedTheGround.TheEvent.AddListener(HandlePlayerTouchedTheGround);
        pogoStickHasEnded.TheEvent.AddListener(HandlePlayerStoppedPogo);
        playerHasPickedPogoStick.TheEvent.AddListener(HandlePlayerPickedPogoStick);
        playerStartedBuildingRun.TheEvent.AddListener(HandlePlayerStartedBuildingRun);
        playerHasStoppedVerticalClimb.TheEvent.AddListener(HandlePlayerStoppedVerticalClimb);
        gameHasStarted.TheEvent.AddListener(HandleGameHasStarted);
        playerDoubleJumping.TheEvent.AddListener(HandlePlayerDoubleJumped);
      //  playerHasJumped.TheEvent.AddListener(HandlePlayerJumped);
        playerSpringJumped.TheEvent.AddListener(HandleSpringJumped);
    }

    private void UnSubscribeEvents()
    {
        aeroPlaneHasFinished.TheEvent.RemoveListener(HandlePlayerStoppedPogo);
        cameraShakeVariations.OnShakeTheCamera.RemoveListener(HandleShakeTheCamera);
        cameraShakeVariations.OnShakeEnded.RemoveListener(HandleShakeEnded);
        playerHasPickedAeroPlanePower.TheEvent.RemoveListener(HandlePlayerPickedAeroPlane);
        playerTouchedTheGround.TheEvent.RemoveListener(HandlePlayerTouchedTheGround);
        pogoStickHasEnded.TheEvent.RemoveListener(HandlePlayerStoppedPogo);
        playerHasPickedPogoStick.TheEvent.RemoveListener(HandlePlayerPickedPogoStick);
        playerStartedBuildingRun.TheEvent.RemoveListener(HandlePlayerStartedBuildingRun);
        playerHasStoppedVerticalClimb.TheEvent.RemoveListener(HandlePlayerStoppedVerticalClimb);
        gameHasStarted.TheEvent.RemoveListener(HandleGameHasStarted);
        playerDoubleJumping.TheEvent.RemoveListener(HandlePlayerDoubleJumped);
     //   playerHasJumped.TheEvent.RemoveListener(HandlePlayerJumped);
        playerSpringJumped.TheEvent.RemoveListener(HandleSpringJumped);

    }

    private void HandleShakeTheCamera(CameraShakeConfig cameraShakeConfig)
    {
        shakeTimer = cameraShakeConfig.GetShakeTime;
    }

    private void HandleShakeEnded()
    {
        shakeTimer = 0;
    }

    private void Update()
    {
        if (shakeTimer <= 0)
        {
            return;
        }

        shakeTimer -= Time.deltaTime;

        if (shakeTimer <= 0)
        {
            cameraShakeVariations.RaiseShakeEndedEvent();
        }
    }

    // private void HandlePlayerJumped(GameEvent gameEvent)
    //  {
    //  jumpCamera.SetActive(true);
    // }
    private void HandleSpringJumped(GameEvent gameEvent)
    {
        doubleJumpCamera.SetActive(false);
        pogostickCamera.SetActive(true);
    }
    private void HandlePlayerDoubleJumped(GameEvent gameEvent)
    {
        doubleJumpCamera.SetActive(true);
    }


    private void HandlePlayerTouchedTheGround(GameEvent theEvent)
    {
      //  jumpCamera.SetActive(false);
        doubleJumpCamera.SetActive(false);
        BuildingFallCamera.SetActive(false);
        fallingCamera.SetActive(false);
        pogostickCamera.SetActive(false);
        aeroPlaneCamera.SetActive(false);
    }

    private void HandlePlayerStoppedPogo(GameEvent theEvent)
    {
        fallingCamera.SetActive(true);
    }

    private void HandlePlayerPickedPogoStick(GameEvent theEvent)
    {
       // jumpCamera.SetActive(false);
        doubleJumpCamera.SetActive(false);
        pogostickCamera.SetActive(true);
    }

    private void HandlePlayerPickedAeroPlane(GameEvent theEvent)
    {
      //  jumpCamera.SetActive(false);
        doubleJumpCamera.SetActive(false);

        if (playerSharedData.isThursting)
        {
            pogostickCamera.SetActive(false);
        }

        fallingCamera.SetActive(false);
        aeroPlaneCamera.SetActive(true);
    }

    //private void HandlePlayerCompletedWallClimb(GameEvent theEvent)
    //{
    //    BuildingFallCamera.SetActive(false);
    //}

    private void HandlePlayerStartedBuildingRun(GameEvent theEvent)
    {
        BuildingRunCamera.SetActive(true);
    }

    private void HandlePlayerStoppedVerticalClimb(GameEvent theEvent)
    {
        BuildingRunCamera.SetActive(false);
        BuildingFallCamera.SetActive(true);
    }

    private void HandleGameHasStarted(GameEvent theEvent)
    {
        playerCarCamera.SetActive(true);
    }
}