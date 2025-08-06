using System.Collections.Generic;
using UnityEngine;


public class PlayerDistanceCalculator : MonoBehaviour, IFloatingReset
{
    [SerializeField] private GamePlaySessionData gamePlaySessionData;
    [SerializeField] private PlayerSharedData playerSharedData;
    [SerializeField] private GameEvent gameHasStarted;
    [SerializeField] private GameEvent playerCrashed;
    [SerializeField] private GameEvent playerRevived;
    [SerializeField] private GameEvent playerStartedBuildingClimb;
    [SerializeField] private GameEvent playerWallRunBuildingRunOff;
   // [SerializeField] private PlayerLevelingSystem playerLevelingSystem;
   // [SerializeField] private float checkForLevelInvterval = 5;

    private float elapsedCheckForLvlIntervalDuration;

    private float playerLastZPosition;
    private float playerLastYPosition;
    private bool isInitialized;
    private bool isClimbingBuilding; // Haven't used 'playerSharedData.WallClimbDoing' because I need this flag to become false when player reaches the top of the building

    private float timeSpend = 0;
    private float distanceCoverd;
    private float previousDistanceCoverd = 0;


    private void Awake()
    {
        gameHasStarted.TheEvent.AddListener(HandleGameHasStarted);
        playerStartedBuildingClimb.TheEvent.AddListener(HandleStartedBuildingClimb);
        playerWallRunBuildingRunOff.TheEvent.AddListener(HandePlayerWallRunBuildingRunOff);
        playerCrashed.TheEvent.AddListener(HandleOnPlayerCrashed);
        playerRevived.TheEvent.AddListener(HandlePlayerRevived);
      
    }

    private void OnEnable()
    {
        distanceCoverd = 0;
        previousDistanceCoverd = 0;
        timeSpend = 0;
    }

    private void OnDestroy()
    {
        gameHasStarted.TheEvent.RemoveListener(HandleGameHasStarted);
        playerStartedBuildingClimb.TheEvent.RemoveListener(HandleStartedBuildingClimb);
        playerWallRunBuildingRunOff.TheEvent.RemoveListener(HandePlayerWallRunBuildingRunOff);
        playerCrashed.TheEvent.RemoveListener(HandleOnPlayerCrashed);
        playerRevived.TheEvent.RemoveListener(HandlePlayerRevived);
    }

    private void HandleGameHasStarted(GameEvent gameEvent)
    {
        playerLastZPosition = playerSharedData.PlayerTransform.position.z;
        playerLastYPosition = playerSharedData.PlayerTransform.position.y;
        isInitialized = true;
    }

    private void HandlePlayerRevived(GameEvent gameEvent)
    {
        isInitialized = true;
    }

    private void HandleOnPlayerCrashed(GameEvent gameEvent)
    {
        isInitialized = false;
    }

    private float totalDistanceCovered;

    public bool ShoudNotOffsetOnRest { get; set; } = true;

    private void Update()
    {
        if (!isInitialized)
            return;

        UpdateDistanceCovered();

        SendHalfKilometerEvents();

      //  CheckAndUpdatePlayerLevel();
    }

    //private void CheckAndUpdatePlayerLevel()
    //{
    //    elapsedCheckForLvlIntervalDuration += Time.deltaTime;

    //    if (elapsedCheckForLvlIntervalDuration >= checkForLevelInvterval)
    //    {
    //        elapsedCheckForLvlIntervalDuration = 0;
    //        playerLevelingSystem.CalculateDisctanceBasedXP();
    //    }
    //}

    private void UpdateDistanceCovered()
    {
        float deltaZ = playerSharedData.PlayerTransform.position.z - playerLastZPosition;
        playerLastZPosition = playerSharedData.PlayerTransform.position.z;

        float deltaY = isClimbingBuilding ? playerSharedData.PlayerTransform.position.y - playerLastYPosition : 0; // Only taken into account during building climb
        playerLastYPosition = playerSharedData.PlayerTransform.position.y;

        float distanceCoveredThisFrame = Mathf.Abs(deltaZ) + Mathf.Abs(deltaY);
        totalDistanceCovered += distanceCoveredThisFrame;

        gamePlaySessionData.DistanceCoveredInMeters = totalDistanceCovered;
    }

    private void HandleStartedBuildingClimb(GameEvent gameEvent)
    {
        if (!isClimbingBuilding)
        {
            isClimbingBuilding = true;
        }
        else
        {
            UnityEngine.Console.LogWarning("Player was already registered as NOT climbing building. It might've resulted in false score calculation");
        }
    }

    private void HandePlayerWallRunBuildingRunOff(GameEvent gameEvent)
    {
        if (isClimbingBuilding)
        {
            isClimbingBuilding = false;
        }
        else
        {
            UnityEngine.Console.LogWarning("Player was already registered as NOT climbing building. It might've resulted in false score calculation");
        }
    }

    private void SendHalfKilometerEvents()
    {
        timeSpend += Time.deltaTime;

        distanceCoverd = gamePlaySessionData.DistanceCoveredInMeters - previousDistanceCoverd;
        if (distanceCoverd >= 500)
        {
            AnalyticsManager.CustomData("GamePlayScreen_DistanceCovered", new Dictionary<string, object>
                {
                  { "Distance", gamePlaySessionData.DistanceCoveredInMeters},
                    { "TimeTaken", timeSpend }
                });

          

            previousDistanceCoverd += distanceCoverd;
            distanceCoverd = 0;
            timeSpend = 0;
        }
    }

    public void OnBeforeFloatingPointReset()
    {

    }


    public void OnFloatingPointReset(float movedOffset)
    {
        playerLastZPosition -= movedOffset;
    }
}