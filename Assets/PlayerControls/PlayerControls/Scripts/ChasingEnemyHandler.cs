using UnityEngine;

public class ChasingEnemyHandler : MonoBehaviour, IFloatingReset
{
    public bool ShoudNotOffsetOnRest { get; set; }

    [SerializeField] private TutorialSegmentStateChannel tutorialSegmentStateChannel;
    [SerializeField] private PlayerSharedData playerSharedData;
    [SerializeField] private ChaserRunner chaserRunner;
    [SerializeField] private GameObject chaserGameObject;
    [SerializeField] private Transform chaserCharacterParentT;
    [SerializeField] private GameEvent playerHasStumbled;
    [SerializeField] private GameEvent playerEnteredPogoStick;
    [SerializeField] private GameEvent playerEnteredAeroplane;
    [SerializeField] private GameEvent playerStartedClimbingBuilding;
    [SerializeField] private GameEvent gameOverEvent;
    [SerializeField] private GameEvent playerHasRevived;
    [SerializeField] private GameEvent playerTouchedGroundAfterBuildingRun;
    [SerializeField] private GameEvent playerStartedBoosting;
    [SerializeField] private GameEvent playerStartedDash;

    [SerializeField] private float lifeTime;

    public static bool isHitFirstTime;

    private float elapsedTimeChaserLifeTime;

    private void Awake()
    {
        playerTouchedGroundAfterBuildingRun.TheEvent.AddListener(HandlePlayerTouchedGroundAfterBuildingRun);
        gameOverEvent.TheEvent.AddListener(ThePlayerStumbled);
        playerHasStumbled.TheEvent.AddListener(ThePlayerStumbled);
        playerEnteredPogoStick.TheEvent.AddListener(TurnOffPlayerStumbled);
        playerEnteredAeroplane.TheEvent.AddListener(TurnOffPlayerStumbled);
        playerStartedDash.TheEvent.AddListener(TurnOffPlayerStumbled);
        playerStartedBoosting.TheEvent.AddListener(TurnOffPlayerStumbled);
        playerStartedClimbingBuilding.TheEvent.AddListener(TurnOffPlayerStumbled);
        playerHasRevived.TheEvent.AddListener(InstantlyDisappearTheChaser);
        tutorialSegmentStateChannel.OnRewind += TurnOffPlayerStumbled;
        isHitFirstTime = false;
    }

    private void OnDestroy()
    {
        playerTouchedGroundAfterBuildingRun.TheEvent.RemoveListener(HandlePlayerTouchedGroundAfterBuildingRun);
        gameOverEvent.TheEvent.RemoveListener(ThePlayerStumbled);
        playerHasStumbled.TheEvent.RemoveListener(ThePlayerStumbled);
        playerEnteredPogoStick.TheEvent.RemoveListener(TurnOffPlayerStumbled);
        playerEnteredAeroplane.TheEvent.RemoveListener(TurnOffPlayerStumbled);
        playerStartedDash.TheEvent.RemoveListener(TurnOffPlayerStumbled);
        playerStartedBoosting.TheEvent.RemoveListener(TurnOffPlayerStumbled);
        playerStartedClimbingBuilding.TheEvent.RemoveListener(TurnOffPlayerStumbled);
        playerHasRevived.TheEvent.AddListener(InstantlyDisappearTheChaser);
        tutorialSegmentStateChannel.OnRewind -= TurnOffPlayerStumbled;
    }

    private void Update()
    {
        if (!playerSharedData.isStumbled)
            return;

        elapsedTimeChaserLifeTime += Time.deltaTime;

        if (elapsedTimeChaserLifeTime >= lifeTime)
        {
             chaserRunner.SwitchToExitState(null);
        }
    }

    private void ThePlayerStumbled(GameEvent theEvent)
    {
        elapsedTimeChaserLifeTime = 0;

        if (!GameManager.IsGameStarted)
            return;

        if (!playerSharedData.WallRunBuilding)
        {
            chaserGameObject.SetActive(true);
           
            if (!isHitFirstTime)
            { 
                PersistentAudioPlayer.Instance.InstantlyPlayTungTung();
                Debug.LogError("Hit");
            }
        }
    }

    private void InstantlyDisappearTheChaser(GameEvent gameEvent)
    {
        chaserRunner.InstantlyDisableTheChaser();
    }

    private void TurnOffPlayerStumbled(GameEvent theEvent = null)
    {
        chaserRunner.SwitchToExitState(null);
    }

    private void TurnOffPlayerStumbled()
    {
        TurnOffPlayerStumbled(null);
    }

    public void ResetChaserParentTransformRotation()
    {
        chaserCharacterParentT.localRotation = Quaternion.identity;

        if (TutorialManager.IsTutorialActive)
        {
            chaserRunner.InstantlyDisableTheChaser();
        }
    }

    private void HandlePlayerTouchedGroundAfterBuildingRun(GameEvent theEvent)
    {
        if(playerSharedData.isStumbled)
        {
            
            chaserGameObject.SetActive(true);
        }
    }

    public void OnFloatingPointReset(float movedOffset)
    {
        
    }

    public void OnBeforeFloatingPointReset()
    {

    }
}