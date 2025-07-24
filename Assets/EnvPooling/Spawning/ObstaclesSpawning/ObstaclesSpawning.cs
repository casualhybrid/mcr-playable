using System.Collections.Generic;
using System.Linq;
using TheKnights.SaveFileSystem;
using UnityEngine;

[RequireComponent(typeof(EnvironmentSpawner))]
public class ObstaclesSpawning : MonoBehaviour, IFloatingReset
{
    #region Classes

    private class EncounterRow
    {
        public ObstacleConfiguration[] columnsInRow = new ObstacleConfiguration[3];
    }

    private class ObstacleConfiguration
    {
        public GameObject obstacleToSpawn;

        public ObstacleConfiguration(GameObject obstacleToSpawn)
        {
            this.obstacleToSpawn = obstacleToSpawn;
        }
    }

    #endregion Classes

    #region Enums

    private enum GenerationType
    {
        RowBasedGeneration,
        BlockBasedGeneration
    }

    #endregion Enums

    #region Variables

    [Header("Settings")]
    [Header("General")]
    [SerializeField] private float jumpHorizontalDistanceOnNormalSpeed = 7.3f;
    [SerializeField] private AnimationCurve jumpHorizontalDistanceOnNormalSpeedCurveOffset;

    [SerializeField] private float safeZoneBeforeRampBuilding = 25f;
    [SerializeField] private float safeZoneAfterRampBuilding = 55f;
    [SerializeField] private bool shouldVariableAdditionSafeDistanceBeTakenIntoAccount;
    [SerializeField] private float variableAdditionalSafeZoneAfterRampBuilding = 20f; // How much of this is added depends on player speed
    [SerializeField] private float obstacleSpawnAdditionalFogSafeDistance = 5f; // To make sure the obstacle spawned is outside the fog. Obstacle spawning inside the fog can cause when obstacle length is a lot AND the pivot of the obstacle spawned is not at the back z bounds of the obstacle

    [Header("Procedural Generation")]
    [SerializeField] private GenerationType obstacleGenerationType;

    [SerializeField] private int rowsInOneBlock = 3;
    [SerializeField] private float spaceBetweenRowBlocksMultiplier = 1.75f;
    [SerializeField] private float firstEncounterOffsetMultiplier = 2f;
    [SerializeField] private float obstacleDistanceScalingMultiplierAtMaxSpeed = 0.75f; // The weightage of this multiplier increases as speed gets closer to maximum speed. The weightage of this multiplier is NONE at minimum speed.
    [SerializeField] private float minProbabiltyOfSpawningTrucksWherePossible = 0.5f;
    [SerializeField] private float chanceToClampDifficultyToMaxDifficultyWithoutTrucksAffectingMinAction = 0.4f;

    [Header("Custom Encounters")]
    [SerializeField] private float customEncounterSpawnProbabilty = 0.75f;

    [Header("General")]
    private LinkedList<EncounterRow> encounterRowBuffer = new LinkedList<EncounterRow>();

    private bool hasPlayerCrashed = false;
    private bool hasGameStarted = false;
    private bool hasSpawnedFirstObstacleBlock = false;
    private float nextRowGenerationPointZ;
    private bool isGenerationPointAfterRampBuildingSet;

    private float nonOverridenGameTimeScale // AKA current game speed
    {
        get
        {
            return speedHandler.isOverriden ? SpeedHandler.GameTimeScaleBeforeOverriden : SpeedHandler.GameTimeScale;
        }
    }

    private float spaceBetweenObstaclesForCurrentBuffer;

    private float normalizedGameSpeedValue
    {
        get
        {
            return (speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(nonOverridenGameTimeScale) - speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(gameManager.GetMinimumSpeed)) /
            (speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(gameManager.GetMaximumSpeed) - speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(gameManager.GetMinimumSpeed));
        }
    }

    private float calculatedCurrentSpaceBetweenObstacles
    {
        get
        {
            return (jumpHorizontalDistanceOnNormalSpeed + 2f + (jumpHorizontalDistanceOnNormalSpeedCurveOffset.Evaluate(speedHandler.GameTimeScaleRatio) * 7f)    /*safe distance*/) *
          Mathf.Lerp(speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(gameManager.GetMinimumSpeed) / speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(gameManager.GetMinimumSpeed),
            speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(gameManager.GetMaximumSpeed) / speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(gameManager.GetMinimumSpeed), normalizedGameSpeedValue); ;
        }
    }

    public bool ShoudNotOffsetOnRest { get; set; } = true;

    [HideInInspector] public float coinSpawnStartPoint;
    [HideInInspector] public float coinSpawnEndPoint;
    //    private List<Patch> currentPatchesBatch = new List<Patch>();

    [Header("References")]
    [SerializeField] private ObstaclePoolSO obstaclePoolSO;

    [SerializeField] private CustomEncounterPoolSO customEncounterPoolSO;
    [SerializeField] private SpeedHandler speedHandler;
    [SerializeField] private PlayerSharedData playerSharedData;
    [SerializeField] private EnvironmentData environmentData;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private DifficultyScaleSO difficultyScaleSO;
    [SerializeField] private IndividualObstaclesSO individualObstaclesSO;
    [SerializeField] private CustomEncountersSO customEncountersSO;
    [SerializeField] private ObstaclesSafeAreaSO obstaclesSafeAreaSO;
    [SerializeField] private SaveManager saveManager;
    private EnvironmentSpawner environmentSpawner;
    private ProceduralCoinGenerator proceduralCoinGenerator;

    [Header("Events")]
    [SerializeField] private GameEvent playerCrashed;

    [SerializeField] private GameEvent playerRevived;
    [SerializeField] private GameEvent gameHasStarted;
    [SerializeField] private GameEvent cutsceneStarted;
    [SerializeField] private GameEvent obstaclesHasBeenSpawned;

    public event System.Action OnSafeAreaCoinSpawnStartEndSet;

    #endregion Variables

    #region Unity Callbacks

    private void Awake()
    {
        environmentSpawner = GetComponent<EnvironmentSpawner>();
        proceduralCoinGenerator = GetComponent<ProceduralCoinGenerator>();

        environmentSpawner.batchOfEnvironmentSpawned += HandleEnvBatchSpawned;

        playerCrashed.TheEvent.AddListener(HandlePlayerHasCrashed);
        playerRevived.TheEvent.AddListener(HandlePlayerRevived);
        gameHasStarted.TheEvent.AddListener(HandleGameHasStarted);
        cutsceneStarted.TheEvent.AddListener(HandleCutsceneStarted);

    }

    private void OnEnable()
    {
        ResetAndInitializeObstaclesSpawnPoint();
    }

    private void OnDestroy()
    {
        environmentSpawner.batchOfEnvironmentSpawned -= HandleEnvBatchSpawned;

        playerCrashed.TheEvent.RemoveListener(HandlePlayerHasCrashed);
        playerRevived.TheEvent.RemoveListener(HandlePlayerRevived);
        gameHasStarted.TheEvent.RemoveListener(HandleGameHasStarted);
        cutsceneStarted.TheEvent.RemoveListener(HandleCutsceneStarted);
    }

    private void Update()
    {
        if (!hasGameStarted)
            return;

        if (hasPlayerCrashed)
            return;

        //  UnityEngine.Console.Log($"Current Diff Rating {difficultyScaleSO.currentPlayerDifficultyRating}");
        //UnityEngine.Console.Log($"Is Skill Placement {saveManager.MainSaveFile.isSkillPlacement}");
        GenerateObstalcesIfNeeded();
    }

    #endregion Unity Callbacks

    #region Event Handling

    private void HandlePlayerHasCrashed(GameEvent gameEvent)
    {
        if (!GameManager.IsGameStarted)
            return;

        hasPlayerCrashed = true;
    }

    private void HandlePlayerRevived(GameEvent gameEvent)
    {
        hasPlayerCrashed = false;
    }

    private void HandleGameHasStarted(GameEvent gameEvent)
    {
        if (!hasSpawnedFirstObstacleBlock)
        {
            ResetAndInitializeObstaclesSpawnPoint();

            hasSpawnedFirstObstacleBlock = true;
        }

        hasGameStarted = true;
    }

    private void HandleEnvBatchSpawned(List<Patch> patchesBatch)
    {
        if (patchesBatch.Count == 0)
        {
            UnityEngine.Console.LogWarning("Spawned Batch count was zero. Not spawning any obstacles");
            return;
        }

        // currentPatchesBatch = patchesBatch;
    }

    private void HandleCutsceneStarted(GameEvent gameEvent)
    {
        if (!saveManager.MainSaveFile.TutorialHasCompleted)
            return;

        // Initialize Space Between Obstacles
        spaceBetweenObstaclesForCurrentBuffer = calculatedCurrentSpaceBetweenObstacles;

        nextRowGenerationPointZ = playerSharedData.PlayerTransform.position.z + (spaceBetweenObstaclesForCurrentBuffer * firstEncounterOffsetMultiplier);
        coinSpawnStartPoint = nextRowGenerationPointZ - spaceBetweenObstaclesForCurrentBuffer;

        GenerateObstalcesIfNeeded(true);

        hasSpawnedFirstObstacleBlock = true;
    }

    private void HandleObstacleFinished(Obstacle obstacle)
    {
        obstacle.ObstacleHasFinished -= HandleObstacleFinished;

        SpringRampHandler springRampHandler = obstacle.GetComponent<SpringRampHandler>();
        springRampHandler?.ReturnSpringRamp();

        obstacle.RestTheObstacle();
        obstaclePoolSO.Return(obstacle);
    }

    #endregion Event Handling

    #region Obstacle Generation

    private void ResetAndInitializeObstaclesSpawnPoint()
    {
        if (RenderSettings.fog)
        {
            nextRowGenerationPointZ = playerSharedData.PlayerTransform.position.z + (RenderSettings.fogEndDistance + obstacleSpawnAdditionalFogSafeDistance);
            coinSpawnStartPoint = playerSharedData.PlayerTransform.position.z + (RenderSettings.fogEndDistance + obstacleSpawnAdditionalFogSafeDistance);

            //    Debug.DrawRay(new Vector3(0, 0, coinSpawnStartPoint), Vector3.up * 100f, Color.white,5);
        }
        else
        {
            nextRowGenerationPointZ = playerSharedData.PlayerTransform.position.z + (spaceBetweenObstaclesForCurrentBuffer * firstEncounterOffsetMultiplier);
            coinSpawnStartPoint = playerSharedData.PlayerTransform.position.z + (spaceBetweenObstaclesForCurrentBuffer * firstEncounterOffsetMultiplier);

            //  Debug.DrawRay(new Vector3(0, 0, coinSpawnStartPoint), Vector3.up * 100f, Color.white,5);
        }
    }

    private void GenerateObstalcesIfNeeded(bool forceGenerateObstacle = false)
    {
        Debug.LogError("LogsMohsin");
        float distanceBetweenNextRowGenerationPointAndPlayer = nextRowGenerationPointZ - playerSharedData.PlayerTransform.position.z;
        float totalObstacleSpawnFogSafeDistance = RenderSettings.fogEndDistance + obstacleSpawnAdditionalFogSafeDistance;

        if (distanceBetweenNextRowGenerationPointAndPlayer < totalObstacleSpawnFogSafeDistance ||
        forceGenerateObstacle)
        {
            // Process Encounter Type
            float randomProbability = Random.Range(0, 1f);

            /*&& (obstacleGenerationType == GenerationType.RowBasedGeneration && encounterRowBuffer.Count > 0)*/

            if (!customEncountersSO.AreTestingEncountersAvailable && (!customEncountersSO.AreCustomEncountersAvailable || (randomProbability > customEncounterSpawnProbabilty))) // If generation is row-based and the buffer isn't empty, continue row-based generation and ignore custom encounters
            {
                // Generate obstacles procedurally

                if (encounterRowBuffer.Count == 0)
                {
                    individualObstaclesSO.RefreshAuthorizedIndividualObstacles();
                    RefreshEncounterRowBuffer();
                    spaceBetweenObstaclesForCurrentBuffer = calculatedCurrentSpaceBetweenObstacles;
                }

                float pointWhereNextBlockEndsZPosition = nextRowGenerationPointZ + ((encounterRowBuffer.Count - 1) * spaceBetweenObstaclesForCurrentBuffer); // We subtracted '1' from row blocks because we already have added distance between rows in 'nextRowGenerationPointZ'
                float distanceBetweenRampBuildingAndBlockStartPoint = environmentData.nextEnvSwitchPatchZPosition - nextRowGenerationPointZ;
                float distanceBetweenRampBuildingAndBlockEndPoint = environmentData.nextEnvSwitchPatchZPosition - pointWhereNextBlockEndsZPosition; // Negative means the point is ahead of the building and positive means it is behind
                bool isBlockStartPointBehindRampBuilding = distanceBetweenRampBuildingAndBlockStartPoint > 0;
                bool doesBlockEndPointExceedSafeDistance = distanceBetweenRampBuildingAndBlockEndPoint < safeZoneBeforeRampBuilding;
                bool isPlayerBehindRampBuilding = environmentData.distanceBetweenCurrentRampBuildingAndPlayer > 0;

                if (environmentData.firstRampBuildingHasBeenSpawned &&
                ((isBlockStartPointBehindRampBuilding && doesBlockEndPointExceedSafeDistance) ||
                (isPlayerBehindRampBuilding && !isBlockStartPointBehindRampBuilding))) // This ensures that the first combination spawned after ramp building spawned at the set generation point in this condition
                {
                //    UnityEngine.Console.LogError("SafeZone Condition");

                    if (!isGenerationPointAfterRampBuildingSet)
                    {
                     //   UnityEngine.Console.LogError("Setting IsGenerationPointafterrampbuilding set = true");

                        isGenerationPointAfterRampBuildingSet = true;

                        float normalizedCurrentSpeedValue = Mathf.Clamp01(normalizedGameSpeedValue);
                        float currentAdditionSafeZoneAfterRampBuilding = variableAdditionalSafeZoneAfterRampBuilding * normalizedCurrentSpeedValue;

                        nextRowGenerationPointZ = environmentData.nextEnvSwitchPatchZPosition + (safeZoneAfterRampBuilding + (shouldVariableAdditionSafeDistanceBeTakenIntoAccount ? currentAdditionSafeZoneAfterRampBuilding : 0));

                        float coinEndPointBefore = coinSpawnEndPoint;
                        coinSpawnStartPoint = /*nextRowGenerationPointZ - spaceBetweenObstaclesForCurrentBuffer*/ coinSpawnEndPoint;
                        UnityEngine.Console.Log($"coinSpawnStartPoint {coinSpawnStartPoint}");

                        coinSpawnEndPoint = environmentData.nextEnvSwitchPatchZPosition;

                        UnityEngine.Console.Log($"End Point Was {coinSpawnEndPoint}");
                        Debug.DrawRay(new Vector3(0, 0, coinSpawnStartPoint), Vector3.up * 100f, Color.black, 5);

                        bool previousPickupGenEnableState = proceduralCoinGenerator.pauseProceduralPickupGeneration;
                        proceduralCoinGenerator.pauseProceduralPickupGeneration = true;

                        OnSafeAreaCoinSpawnStartEndSet?.Invoke();

                        proceduralCoinGenerator.pauseProceduralPickupGeneration = previousPickupGenEnableState;

                        coinSpawnStartPoint = nextRowGenerationPointZ - spaceBetweenObstaclesForCurrentBuffer;
                        coinSpawnEndPoint = coinEndPointBefore;
                    }
                }
                else
                {
                    isGenerationPointAfterRampBuildingSet = false;

                    TogglePickupGenerationAccourdingToBlockEndPoint(pointWhereNextBlockEndsZPosition);
                    ProcedurallySpawnObstacles();
                }
            }
            else
            {
                // Generate custom encounters

                float distanceBetweenRampBuildingAndBlockStartPoint = environmentData.nextEnvSwitchPatchZPosition - nextRowGenerationPointZ;
                bool isBlockStartPointBehindRampBuilding = distanceBetweenRampBuildingAndBlockStartPoint > 0;
                bool isPlayerBehindRampBuilding = environmentData.distanceBetweenCurrentRampBuildingAndPlayer > 0;

                float maxCustomEncounterLength = Mathf.Infinity;
                float buildingSafeZoneStartPosition = environmentData.nextEnvSwitchPatchZPosition - safeZoneBeforeRampBuilding;
                float distanceBetweenRampBuildingSafeZoneAndBlockStartPoint = buildingSafeZoneStartPosition - nextRowGenerationPointZ;
                bool isBlockStartPointBehindRampBuildingSafeZone = distanceBetweenRampBuildingSafeZoneAndBlockStartPoint > 0;

                if (environmentData.firstRampBuildingHasBeenSpawned && isBlockStartPointBehindRampBuildingSafeZone && !customEncountersSO.AreTestingEncountersAvailable)
                {
                    maxCustomEncounterLength = distanceBetweenRampBuildingSafeZoneAndBlockStartPoint;
                }

                CustomEncounter customEncounterToSpawn = customEncountersSO.GetCustomEncounter(maxCustomEncounterLength, difficultyScaleSO.currentIntendedPlayerDifficultyRating);

                float pointWhereNextBlockEndsZPosition = nextRowGenerationPointZ + (customEncounterToSpawn != null ? customEncounterToSpawn.EncounterSize : 0); // We subtracted '1' from row blocks because we already have added distance between rows in 'nextRowGenerationPointZ'

                float safeAreaPoint = obstaclesSafeAreaSO.CheckIfZRangeIsInsideSafeArea(nextRowGenerationPointZ, pointWhereNextBlockEndsZPosition);
                if (safeAreaPoint != -1)
                {
                    UnityEngine.Console.Log("Custom encounter is trying to generate inside safe area");
                    nextRowGenerationPointZ = (safeAreaPoint + (ObstaclesSafeAreaSO.safeZoneLength)) + firstEncounterOffsetMultiplier;
                    coinSpawnStartPoint = nextRowGenerationPointZ;
                    return;
                }

                float distanceBetweenRampBuildingAndBlockEndPoint = environmentData.nextEnvSwitchPatchZPosition - pointWhereNextBlockEndsZPosition; // Negative means the point is ahead of the building and positive means it is behind
                bool doesBlockEndPointExceedSafeDistance = distanceBetweenRampBuildingAndBlockEndPoint < safeZoneBeforeRampBuilding;

                if (customEncounterToSpawn == null ||
                (environmentData.firstRampBuildingHasBeenSpawned &&
                ((isBlockStartPointBehindRampBuilding && doesBlockEndPointExceedSafeDistance) ||
                (isPlayerBehindRampBuilding && !isBlockStartPointBehindRampBuilding)))) // This ensures that the first combination spawned after ramp building spawned at the set generation point in this condition
                {
                    if (!isGenerationPointAfterRampBuildingSet)
                    {
                        isGenerationPointAfterRampBuildingSet = true;

                        float normalizedCurrentSpeedValue = Mathf.Clamp01(normalizedGameSpeedValue);
                        float currentAdditionSafeZoneAfterRampBuilding = variableAdditionalSafeZoneAfterRampBuilding * normalizedCurrentSpeedValue;

                        nextRowGenerationPointZ = environmentData.nextEnvSwitchPatchZPosition + (safeZoneAfterRampBuilding + (shouldVariableAdditionSafeDistanceBeTakenIntoAccount ? currentAdditionSafeZoneAfterRampBuilding : 0));
                        coinSpawnStartPoint = nextRowGenerationPointZ - spaceBetweenObstaclesForCurrentBuffer;
                    }
                }
                else
                {
                    isGenerationPointAfterRampBuildingSet = false;

                    TogglePickupGenerationAccourdingToBlockEndPoint(pointWhereNextBlockEndsZPosition);
                    SpawnCustomEncounter(customEncounterToSpawn);

                    // Increment testing encounter index once the encounter has successfully spawned
                    if (customEncountersSO.AreTestingEncountersAvailable)
                    {
                        customEncountersSO.IncrementTestingEncounterIndex();
                    }
                }
            }
        }
    }

    #endregion Obstacle Generation

    #region Procedural Generation

    private void ProcedurallySpawnObstacles()
    {
        // Request obstacles from pool:
        // Obstacle spawnedObstacle = obstaclePoolSO.Request(obstacle, obstacle.InstanceID);

        // Return obstacles to pool:
        // spawnedObstacle.ObstacleHasFinished += HandleObstacleHasFinished;
        float randomDistanceOffset = 0;
        List<SpringRampHandler> springRampHandlersList = new List<SpringRampHandler>();
        do
        {
            if (!obstaclesSafeAreaSO.CheckIfZPositionIsInsideSafeArea(nextRowGenerationPointZ))
            {
                for (int currentColumn = 0; currentColumn < 3; currentColumn++)
                {
                    if (encounterRowBuffer.First.Value.columnsInRow[currentColumn] == null)
                        continue;

                    GameObject obstacleToSpawn = encounterRowBuffer.First.Value.columnsInRow[currentColumn].obstacleToSpawn;

                    Obstacle originalObstacle = obstacleToSpawn.GetComponent<Obstacle>();
                    Obstacle fetchedObstacle = obstaclePoolSO.Request(originalObstacle, originalObstacle.InstanceID);

                    fetchedObstacle.ObstacleHasFinished += HandleObstacleFinished;

                    randomDistanceOffset = Random.Range(-(playerSharedData.Playercollider.size.z / 4f), playerSharedData.Playercollider.size.z / 3.5f);

                    if (fetchedObstacle.InstanceID == individualObstaclesSO.allIndividualObstacles["One-Arm Automatic Barrier"].GetComponent<Obstacle>().InstanceID
                        || fetchedObstacle.InstanceID == individualObstaclesSO.allIndividualObstacles["Three-Arm Automatic Barrier"].GetComponent<Obstacle>().InstanceID)
                    {
                        randomDistanceOffset = 0;
                    }

                    fetchedObstacle.transform.SetPositionAndRotation(new Vector3(currentColumn - 1, obstacleToSpawn.transform.position.y, nextRowGenerationPointZ + randomDistanceOffset), Quaternion.identity);
                    // Instantiate(obstacleToSpawn, new Vector3(currentColumn - 1, obstacleToSpawn.transform.position.y, nextRowGenerationPointZ), Quaternion.identity); // 'x position = currentColumn - 1' because the columns are at (-1, 0, 1)

                    SpringRampHandler springRampHandler = fetchedObstacle.GetComponent<SpringRampHandler>();
                    if (springRampHandler != null)
                    {
                        springRampHandlersList.Add(springRampHandler);
                    }
                }

                encounterRowBuffer.RemoveFirst();
            }
            if (encounterRowBuffer.Count > 0)
            {
                nextRowGenerationPointZ += spaceBetweenObstaclesForCurrentBuffer;
                coinSpawnEndPoint = nextRowGenerationPointZ /*- (spaceBetweenObstaclesForCurrentBuffer) /2f*/;
                //  Debug.DrawRay(new Vector3(0, 0, coinSpawnEndPoint), Vector3.up * 100f, Color.red,5);
            }
            else
            {
                coinSpawnEndPoint = nextRowGenerationPointZ + ((spaceBetweenObstaclesForCurrentBuffer * spaceBetweenRowBlocksMultiplier) / 2);
                //   Debug.DrawRay(new Vector3(0, 0, coinSpawnEndPoint), Vector3.up * 100f, Color.red,5);
            }
        } while (obstacleGenerationType == GenerationType.BlockBasedGeneration && encounterRowBuffer.Count > 0);

        if (springRampHandlersList.Count > 0)
        {
            springRampHandlersList[Random.Range(0, springRampHandlersList.Count - 1)].InitializeSpringRamp();
        }

        // Handles coin generation as well
        obstaclesHasBeenSpawned.RaiseEvent();

        coinSpawnStartPoint = coinSpawnEndPoint;
        //   Debug.DrawRay(new Vector3(0, 0, coinSpawnStartPoint), Vector3.up * 100f, Color.white,5);

        if (encounterRowBuffer.Count == 0)
        {
            spaceBetweenObstaclesForCurrentBuffer = calculatedCurrentSpaceBetweenObstacles;

            // Add block safe distance
            nextRowGenerationPointZ += spaceBetweenObstaclesForCurrentBuffer * spaceBetweenRowBlocksMultiplier;
            //coinSpawnStartPoint = nextRowGenerationPointZ - ((spaceBetweenObstaclesForCurrentBuffer * spaceBetweenRowBlocksMultiplier) / 2);
        }
    }

    /// <summary>
    // This function also updates spaceBetweenObstaclesForCurrentBuffer
    /// </summary>
    private void RefreshEncounterRowBuffer()
    {
        // *** UNCLEAN CODE ***
        float currentNormalizedDifficultyValue = (difficultyScaleSO.GetRandomPointOnCurrentDifficultyRange() / 100) /*Max difficulty rating value*/;

        // MIN ACTION CALCULATION
        int upperBoundOfMinimumActionsRequired = (rowsInOneBlock * 2) + 1; // where '3' represents columns
        int lowerBoundOfMinimumActionsRequired = 1;

        int currentMinimumRequiredActions = (int)(lowerBoundOfMinimumActionsRequired + (((upperBoundOfMinimumActionsRequired - lowerBoundOfMinimumActionsRequired) * currentNormalizedDifficultyValue)));

        if (currentMinimumRequiredActions > 3 && Random.Range(0f, 1f) <= chanceToClampDifficultyToMaxDifficultyWithoutTrucksAffectingMinAction)
        {
            currentMinimumRequiredActions = rowsInOneBlock;
        }
        // END MIN ACTION CALCULATION

        // AVG ACTION CALCULATION
        int easiestLane = Random.Range(0, 3);
        int maxExtraObstaclesThatCanBeGenerated = ((rowsInOneBlock - currentMinimumRequiredActions) * 2);
        int currentExtraObstaclesToGenerate = (int)((maxExtraObstaclesThatCanBeGenerated + 1) * (((upperBoundOfMinimumActionsRequired - lowerBoundOfMinimumActionsRequired) * currentNormalizedDifficultyValue) % 1)); // 'maxExtraObstaclesToGenerate + 1' because we want max extra obstacles to hold it's max value. Otherwise, it never spawns max extra obstacles
        // END AVG ACTION CALCULATION

        // TRANSPORTER SPAWN RATE
        float transporterSpawnProbability = 0.1f;
        // END TRANSPORTER SPAWN RATE

        EncounterRow[] rowsToBeAddedToTheBuffer = new EncounterRow[rowsInOneBlock];

        for (int i = 0; i < rowsToBeAddedToTheBuffer.Length; i++)
        {
            rowsToBeAddedToTheBuffer[i] = new EncounterRow();
        }

        List<List<int>> emptySpaces = new List<List<int>>(); // Outer index represents column and inner index represents rows
        List<List<int>> filledSpaces = new List<List<int>>();
        List<List<int>> nonActionObstacleSpawnPoints = new List<List<int>>();
        List<List<int>> laneSwitchObstacleSpawnPoints = new List<List<int>>();

        // Put all empty spaces in a list and initialize both lists
        for (int i = 0; i < 3; i++)
        {
            emptySpaces.Add(new List<int>());
            filledSpaces.Add(new List<int>());
            nonActionObstacleSpawnPoints.Add(new List<int>());
            laneSwitchObstacleSpawnPoints.Add(new List<int>());

            for (int j = 0; j < rowsInOneBlock; j++)
            {
                emptySpaces[i].Add(j);
            }
        }

        // Fill empty spaces with min actions required
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < Mathf.Clamp(currentMinimumRequiredActions, 0, rowsInOneBlock); j++)
            {
                int randEmptyRowIndex = Random.Range(0, emptySpaces[i].Count);

                if (emptySpaces[i].Count == 0)
                {
                    continue;
                }

                filledSpaces[i].Add(emptySpaces[i][randEmptyRowIndex]);
                //rowsToBeAddedToTheBuffer[emptySpaces[i][randEmptyRowIndex]].columnsInRow[i] = new ObstacleConfiguration(individualObstaclesSO.individualObstacles[randomObstacleIndex].obstaclePrefab);

                emptySpaces[i].RemoveAt(randEmptyRowIndex);
            }
        }

        // Fill all lanes except easiest lane with calculated extra obstacles
        List<int> columnsToBeConsidered = new List<int>() { 0, 1, 2 };
        columnsToBeConsidered.Remove(easiestLane);

        for (int i = 0; i < currentExtraObstaclesToGenerate + 1; i++)
        {
            if (columnsToBeConsidered.Count == 0)
            {
                continue;
            }

            int randColumnIndex = Random.Range(0, columnsToBeConsidered.Count);
            int randColumn = columnsToBeConsidered[randColumnIndex];
            int randRowIndex = Random.Range(0, emptySpaces[randColumn].Count);

            if (emptySpaces[randColumn].Count == 0)
            {
                continue;
            }

            filledSpaces[randColumn].Add(emptySpaces[randColumn][randRowIndex]);
            //rowsToBeAddedToTheBuffer[emptySpaces[randColumn][randRowIndex]].columnsInRow[randColumn] = new ObstacleConfiguration(individualObstaclesSO.individualObstacles[randomObstacleIndex].obstaclePrefab);

            emptySpaces[randColumn].RemoveAt(randRowIndex);

            if (emptySpaces[randColumn].Count == 0)
                columnsToBeConsidered.Remove(randColumn);
        }

        List<int> randColumnsToPlaceTransporterIn = new List<int>() { 0, 1, 2 };
        bool oneTransporterPlaced = false;

        // Place transporters in empty spaces and spawns them
        for (int i = 0; i < 3; i++)
        {
            int randColumnToPlaceTransporterInRandIndex = Random.Range(0, randColumnsToPlaceTransporterIn.Count);
            int currentColumn = randColumnsToPlaceTransporterIn[randColumnToPlaceTransporterInRandIndex];

            randColumnsToPlaceTransporterIn.RemoveAt(randColumnToPlaceTransporterInRandIndex);

            if (emptySpaces[currentColumn].Count == 0)
                continue;

            float randomProbabiltiy = Random.Range(0f, 1f);

            if (randomProbabiltiy > transporterSpawnProbability)
                continue;

            int randEmptyRowIndex = Random.Range(0, emptySpaces[currentColumn].Count);

            //nonActionObstacleSpawnPoints[i].Add(emptySpaces[i][randEmptyRowIndex]);
            //rowsToBeAddedToTheBuffer[emptySpaces[i][randEmptyRowIndex]].columnsInRow[i] = new ObstacleConfiguration(individualObstaclesSO.individualNonActionObstacles[randomNonActionObstacleIndex].obstaclePrefab);
            int randomObstacleIndex = Random.Range(0, individualObstaclesSO.individualNonActionObstacles.Length);
            rowsToBeAddedToTheBuffer[emptySpaces[currentColumn][randEmptyRowIndex]].columnsInRow[currentColumn] = new ObstacleConfiguration(individualObstaclesSO.individualNonActionObstacles[randomObstacleIndex].obstaclePrefab);

            emptySpaces[currentColumn].RemoveAt(randEmptyRowIndex);

            oneTransporterPlaced = true;
            break;
        }

        // Spawn trucks to increase min actions
        if (currentMinimumRequiredActions > rowsInOneBlock)
        {
            int actionsToIncrease = currentMinimumRequiredActions - rowsInOneBlock;

            // First way of spawning trucks that increase minimum actions
            int rowCountOccupiedByTrucks = 2 + (int)((actionsToIncrease - 1) / 2);
            int trucksToSpawn = 3 + (actionsToIncrease - 1);

            List<int> rowsOccupiedByTrucks = new List<int>();
            List<int> unselectedRows = new List<int>();

            for (int i = 0; i < rowsInOneBlock; i++)
            {
                unselectedRows.Add(i);
            }

            for (int i = 0; i < rowCountOccupiedByTrucks; i++)
            {
                int randUnselectedRowIndex = Random.Range(0, unselectedRows.Count);

                rowsOccupiedByTrucks.Add(unselectedRows[randUnselectedRowIndex]);
                unselectedRows.RemoveAt(randUnselectedRowIndex);
            }

            if (Random.Range(0, 2) == 0)
            {
                rowsOccupiedByTrucks = rowsOccupiedByTrucks.OrderByDescending(x => x).ToList();
            }
            else
            {
                rowsOccupiedByTrucks = rowsOccupiedByTrucks.OrderBy(x => x).ToList();
            }

            int randColumnBorder = Random.Range(0, 2) == 1 ? 2 : 0;

            for (int i = 0; i < rowsOccupiedByTrucks.Count; i++)
            {
                laneSwitchObstacleSpawnPoints[randColumnBorder].Add(rowsOccupiedByTrucks[i]);
                trucksToSpawn--;

                if (trucksToSpawn == 0)
                    break;

                laneSwitchObstacleSpawnPoints[1].Add(i);
                trucksToSpawn--;

                if (trucksToSpawn == 0)
                    break;

                randColumnBorder = randColumnBorder == 0 ? 2 : 0;
            }
        }

        // Spawn obstacles at filled spaces
        int randomDirection = Random.Range(0, 2) == 0 ? -1 : 1;

        for (int i = 0; i < 3; i++)
        {
            int directionalLoopIndex = randomDirection == 1 ? i : 2 - i;

            for (int j = 0; j < filledSpaces[directionalLoopIndex].Count; j++)
            {
                // Truck spawn conditions without affecting minimum actions
                if (directionalLoopIndex == 0 || directionalLoopIndex == 2) // If this lane is left-most or right-most
                {
                    if (!filledSpaces[1].Contains(filledSpaces[directionalLoopIndex][j]))
                    {
                        // if (i == easiestLane)
                        // {
                        int obstalcesInCurrentLaneProceedingThisObstacle = 0;

                        for (int obstacleIndex = 0; obstacleIndex < filledSpaces[directionalLoopIndex].Count; obstacleIndex++)
                        {
                            if (filledSpaces[directionalLoopIndex][j] > filledSpaces[directionalLoopIndex][obstacleIndex])
                            {
                                obstalcesInCurrentLaneProceedingThisObstacle++;
                            }
                        }

                        int obstalcesInAdjacentLane = 0;

                        for (int obstacleIndex = 0; obstacleIndex < filledSpaces[1].Count; obstacleIndex++) // Middle Lane
                        {
                            if (filledSpaces[directionalLoopIndex][j] >= filledSpaces[1][obstacleIndex])
                            {
                                obstalcesInAdjacentLane++;
                            }
                        }

                        if (obstalcesInAdjacentLane <= obstalcesInCurrentLaneProceedingThisObstacle && laneSwitchObstacleSpawnPoints[directionalLoopIndex].Count == 0)
                        {
                            // Can spawn. Should it?
                            if (Random.Range(0f, 1f) <= minProbabiltyOfSpawningTrucksWherePossible)
                            {
                                laneSwitchObstacleSpawnPoints[directionalLoopIndex].Add(filledSpaces[directionalLoopIndex][j]);
                            }
                        }
                        // }
                        // else
                        // {
                        //     // Write code
                        // }
                    }
                }
                else if (directionalLoopIndex == 1) // If this is middle lane
                {
                    if (!(filledSpaces[0].Contains(filledSpaces[directionalLoopIndex][j]) &&
                    filledSpaces[2].Contains(filledSpaces[directionalLoopIndex][j])))
                    {
                        int obstalcesInLaneProceedingThisObstacle = 0;

                        for (int obstacleIndex = 0; obstacleIndex < filledSpaces[directionalLoopIndex].Count; obstacleIndex++)
                        {
                            if (filledSpaces[directionalLoopIndex][j] > filledSpaces[directionalLoopIndex][obstacleIndex])
                            {
                                obstalcesInLaneProceedingThisObstacle++;
                            }
                        }

                        for (int laneIndex = 0; laneIndex < 3; laneIndex += 2)
                        {
                            int obstalcesInAdjacentLane = 0;

                            for (int obstacleIndex = 0; obstacleIndex < filledSpaces[laneIndex].Count; obstacleIndex++) // Middle Lane
                            {
                                if (filledSpaces[directionalLoopIndex][j] >= filledSpaces[laneIndex][obstacleIndex])
                                {
                                    obstalcesInAdjacentLane++;
                                }
                            }

                            if (obstalcesInAdjacentLane <= obstalcesInLaneProceedingThisObstacle && laneSwitchObstacleSpawnPoints[directionalLoopIndex].Count == 0)
                            {
                                // Can spawn. Should it?
                                if (Random.Range(0f, 1f) <= minProbabiltyOfSpawningTrucksWherePossible)
                                {
                                    laneSwitchObstacleSpawnPoints[directionalLoopIndex].Add(filledSpaces[directionalLoopIndex][j]);
                                }
                            }
                        }
                    }
                }

                if (laneSwitchObstacleSpawnPoints[directionalLoopIndex].Contains(filledSpaces[directionalLoopIndex][j]))
                {
                    int randomObstacleIndex = Random.Range(0, individualObstaclesSO.individualForceLaneChangeObstacle.Length);
                    rowsToBeAddedToTheBuffer[filledSpaces[directionalLoopIndex][j]].columnsInRow[directionalLoopIndex] = new ObstacleConfiguration(individualObstaclesSO.individualForceLaneChangeObstacle[randomObstacleIndex].obstaclePrefab);
                }
                else
                {
                    int randomObstacleIndex = Random.Range(0, individualObstaclesSO.CurrentAuthorizedIndividualObstacles.Length);
                    rowsToBeAddedToTheBuffer[filledSpaces[directionalLoopIndex][j]].columnsInRow[directionalLoopIndex] = new ObstacleConfiguration(individualObstaclesSO.CurrentAuthorizedIndividualObstacles[randomObstacleIndex].obstaclePrefab);
                }
            }
        }

        // Add all generated obstacles rows to row buffer and exclude empty rows (with min actions being one, all rows can never be empty)
        for (int i = 0; i < rowsToBeAddedToTheBuffer.Length; i++)
        {
            bool rowIsEmpty = true;

            for (int j = 0; j < 3; j++)
            {
                if (rowsToBeAddedToTheBuffer[i].columnsInRow[j] != null)
                {
                    rowIsEmpty = false;
                    break;
                }
            }

            if (!rowIsEmpty)
                encounterRowBuffer.AddLast(rowsToBeAddedToTheBuffer[i]);
        }
    }

    #endregion Procedural Generation

    #region Custom Encounters

    private void SpawnCustomEncounter(CustomEncounter customEncounterToSpawn)
    {
        CustomEncounter fetchedCustomEncounter = customEncounterPoolSO.Request(customEncounterToSpawn, customEncounterToSpawn.InstanceID);

        fetchedCustomEncounter.transform.SetPositionAndRotation(new Vector3(0, 0, nextRowGenerationPointZ + customEncounterToSpawn.encounterOffset), Quaternion.identity);

        fetchedCustomEncounter.InitializeCustomEncounter();

        fetchedCustomEncounter.CustomEncounterHasFinished += HandleCustomEncounterFinished;

        coinSpawnEndPoint = nextRowGenerationPointZ + fetchedCustomEncounter.EncounterSize + ((calculatedCurrentSpaceBetweenObstacles * spaceBetweenRowBlocksMultiplier) / 2);

        if (!(customEncountersSO.pauseCoinAndPickupGenerationInTestingEncounter && customEncountersSO.AreTestingEncountersAvailable))
        {
            obstaclesHasBeenSpawned.RaiseEvent();
        }

        coinSpawnStartPoint = coinSpawnEndPoint;
        //  Debug.DrawRay(new Vector3(0, 0, coinSpawnStartPoint), Vector3.up * 100f, Color.white,5);

        spaceBetweenObstaclesForCurrentBuffer = calculatedCurrentSpaceBetweenObstacles;

        nextRowGenerationPointZ += fetchedCustomEncounter.EncounterSize + (calculatedCurrentSpaceBetweenObstacles * spaceBetweenRowBlocksMultiplier);
        //coinSpawnStartPoint = nextRowGenerationPointZ - ((calculatedCurrentSpaceBetweenObstacles * spaceBetweenRowBlocksMultiplier) / 2);
    }

    private void HandleCustomEncounterFinished(CustomEncounter customEncounter)
    {
        customEncounter.CustomEncounterHasFinished -= HandleCustomEncounterFinished;
        customEncounter.ResetCustomEncounter();
        customEncounterPoolSO.Return(customEncounter);
    }

    #endregion Custom Encounters

    #region Coin Generation

    public void TogglePickupGenerationAccourdingToBlockEndPoint(float pointWhereNextBlockEndsZPosition)
    {
        if (environmentData.firstRampBuildingHasBeenSpawned &&
        pointWhereNextBlockEndsZPosition > environmentData.nextEnvSwitchPatchZPosition - environmentData.minimumSafeZoneLength &&
        pointWhereNextBlockEndsZPosition <= environmentData.nextEnvSwitchPatchZPosition)
        {
            proceduralCoinGenerator.pauseProceduralPickupGeneration = true;
        }
        else
        {
            proceduralCoinGenerator.pauseProceduralPickupGeneration = false;
        }
    }

    #endregion Coin Generation

    public void OnFloatingPointReset(float movedOffset)
    {
        obstaclesSafeAreaSO.FloatingPointResetObstaclesSafeAreaList(movedOffset);
        nextRowGenerationPointZ -= movedOffset;
        coinSpawnStartPoint -= movedOffset;
        coinSpawnEndPoint -= movedOffset;
    }

    public void OnBeforeFloatingPointReset()
    {
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(new Vector3(0, 0, nextRowGenerationPointZ), Vector3.one * 10);
    }
}