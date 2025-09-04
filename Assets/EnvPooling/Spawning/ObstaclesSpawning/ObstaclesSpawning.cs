using System.Collections.Generic;
using TheKnights.SaveFileSystem;
using UnityEngine;

[RequireComponent(typeof(EnvironmentSpawner))]
public class ObstaclesSpawning : MonoBehaviour, IFloatingReset
{
    #region Classes

    private class EncounterRow
    {
        public ObstacleConfiguration[] columnsInRow = new ObstacleConfiguration[3];
        public void Clear()
        {
            columnsInRow[0] = columnsInRow[1] = columnsInRow[2] = null;
        }
    }

    private class ObstacleConfiguration
    {
        public GameObject obstacleToSpawn;
        public ObstacleConfiguration(GameObject obstacleToSpawn) { this.obstacleToSpawn = obstacleToSpawn; }
    }

    #endregion Classes

    #region Enums

    private enum GenerationType
    {
        RowBasedGeneration,
        BlockBasedGeneration
    }

    #endregion Enums

    #region Inspector fields

    [Header("Settings")]
    [Header("General")]
    [SerializeField] private float jumpHorizontalDistanceOnNormalSpeed = 7.3f;
    [SerializeField] private AnimationCurve jumpHorizontalDistanceOnNormalSpeedCurveOffset;

    [SerializeField] private float safeZoneBeforeRampBuilding = 25f;
    [SerializeField] private float safeZoneAfterRampBuilding = 55f;
    [SerializeField] private bool shouldVariableAdditionSafeDistanceBeTakenIntoAccount;
    [SerializeField] private float variableAdditionalSafeZoneAfterRampBuilding = 20f;
    [SerializeField] private float obstacleSpawnAdditionalFogSafeDistance = 5f;

    [Header("Procedural Generation")]
    [SerializeField] private GenerationType obstacleGenerationType;
    [SerializeField] private int rowsInOneBlock = 3;
    [SerializeField] private float spaceBetweenRowBlocksMultiplier = 1.75f;
    [SerializeField] private float firstEncounterOffsetMultiplier = 2f;
    [SerializeField] private float obstacleDistanceScalingMultiplierAtMaxSpeed = 0.75f;
    [SerializeField] private float minProbabiltyOfSpawningTrucksWherePossible = 0.5f;
    [SerializeField] private float chanceToClampDifficultyToMaxDifficultyWithoutTrucksAffectingMinAction = 0.4f;

    [Header("Custom Encounters")]
    [SerializeField] private float customEncounterSpawnProbabilty = 0.75f;

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

    [Header("Events")]
    [SerializeField] private GameEvent playerCrashed;
    [SerializeField] private GameEvent playerRevived;
    [SerializeField] private GameEvent gameHasStarted;
    [SerializeField] private GameEvent cutsceneStarted;
    [SerializeField] private GameEvent obstaclesHasBeenSpawned;

    [Header("Tuning")]
    [SerializeField] private int framesBetweenChecks = 10; // you used 10 previously

    #endregion Inspector fields

    #region Runtime fields

    // buffer of encounter rows to spawn
    private LinkedList<EncounterRow> encounterRowBuffer = new LinkedList<EncounterRow>();

    // state flags
    private bool hasPlayerCrashed = false;
    private bool hasGameStarted = false;
    private bool hasSpawnedFirstObstacleBlock = false;
    private float nextRowGenerationPointZ;
    private bool isGenerationPointAfterRampBuildingSet;

    private float spaceBetweenObstaclesForCurrentBuffer;

    // dependencies
    private EnvironmentSpawner environmentSpawner;
    private ProceduralCoinGenerator proceduralCoinGenerator;

    // Frame throttle
    private int frameCounter = 0;

    // public (matching your original api)
    public bool ShoudNotOffsetOnRest { get; set; } = true;
    [HideInInspector] public float coinSpawnStartPoint;
    [HideInInspector] public float coinSpawnEndPoint;

    // reusable buffers to avoid allocation
    private List<int>[] emptySpaces;
    private List<int>[] filledSpaces;
    private List<int>[] laneSwitchObstacleSpawnPoints;
    private List<int>[] nonActionObstacleSpawnPoints;
    private EncounterRow[] rowsToBeAddedToTheBuffer; // length rowsInOneBlock
    private List<SpringRampHandler> springRampHandlersBuffer = new List<SpringRampHandler>(4);

    #endregion Runtime fields
    public static event System.Action OnSafeAreaCoinSpawnStartEndSet;
    #region Properties

    private float nonOverridenGameTimeScale => speedHandler.isOverriden ? SpeedHandler.GameTimeScaleBeforeOverriden : SpeedHandler.GameTimeScale;

    private float normalizedGameSpeedValue
    {
        get
        {
            float min = speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(gameManager.GetMinimumSpeed);
            float max = speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(gameManager.GetMaximumSpeed);
            float cur = speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(nonOverridenGameTimeScale);
            float denom = max - min;
            return denom == 0f ? 0f : Mathf.Clamp01((cur - min) / denom);
        }
    }

    private float calculatedCurrentSpaceBetweenObstacles
    {
        get
        {
            // replicate your original formula while avoiding repeated expensive calls
            float t = speedHandler.GameTimeScaleRatio;
            float horizontal = jumpHorizontalDistanceOnNormalSpeed + 2f + (jumpHorizontalDistanceOnNormalSpeedCurveOffset.Evaluate(t) * 7f);
            float minSpeedFactor = speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(gameManager.GetMinimumSpeed) / speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(gameManager.GetMinimumSpeed);
            float maxSpeedFactor = speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(gameManager.GetMaximumSpeed) / speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(gameManager.GetMinimumSpeed);
            return horizontal * Mathf.Lerp(minSpeedFactor, maxSpeedFactor, normalizedGameSpeedValue);
        }
    }

    #endregion Properties

    #region Unity lifecycle

    private void Awake()
    {
        environmentSpawner = GetComponent<EnvironmentSpawner>();
        proceduralCoinGenerator = GetComponent<ProceduralCoinGenerator>();

        // subscribe
        if (environmentSpawner != null)
            environmentSpawner.batchOfEnvironmentSpawned += HandleEnvBatchSpawned;

        if (playerCrashed != null) playerCrashed.TheEvent.AddListener(HandlePlayerHasCrashed);
        if (playerRevived != null) playerRevived.TheEvent.AddListener(HandlePlayerRevived);
        if (gameHasStarted != null) gameHasStarted.TheEvent.AddListener(HandleGameHasStarted);
        if (cutsceneStarted != null) cutsceneStarted.TheEvent.AddListener(HandleCutsceneStarted);

        // prepare reusable buffers
        InitializeReusableBuffers();
    }

    private void OnEnable()
    {
        ResetAndInitializeObstaclesSpawnPoint();
    }

    private void OnDestroy()
    {
        if (environmentSpawner != null)
            environmentSpawner.batchOfEnvironmentSpawned -= HandleEnvBatchSpawned;

        if (playerCrashed != null) playerCrashed.TheEvent.RemoveListener(HandlePlayerHasCrashed);
        if (playerRevived != null) playerRevived.TheEvent.RemoveListener(HandlePlayerRevived);
        if (gameHasStarted != null) gameHasStarted.TheEvent.RemoveListener(HandleGameHasStarted);
        if (cutsceneStarted != null) cutsceneStarted.TheEvent.RemoveListener(HandleCutsceneStarted);
    }

    private void Update()
    {
        if (!hasGameStarted || hasPlayerCrashed)
            return;

        frameCounter++;
        if (frameCounter >= framesBetweenChecks)
        {
            frameCounter = 0;
            GenerateObstalcesIfNeeded();
        }
    }

    #endregion Unity lifecycle

    #region Event handlers

    private void HandlePlayerHasCrashed(GameEvent gameEvent) { if (GameManager.IsGameStarted) hasPlayerCrashed = true; }
    private void HandlePlayerRevived(GameEvent gameEvent) { hasPlayerCrashed = false; }
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
        if (patchesBatch == null || patchesBatch.Count == 0)
        {
            UnityEngine.Console.LogWarning("Spawned Batch count was zero. Not spawning any obstacles");
            return;
        }
        // nothing else needed here; you may use patchesBatch in future
    }

    private void HandleCutsceneStarted(GameEvent gameEvent)
    {
        if (!saveManager.MainSaveFile.TutorialHasCompleted) return;

        spaceBetweenObstaclesForCurrentBuffer = calculatedCurrentSpaceBetweenObstacles;
        nextRowGenerationPointZ = playerSharedData.PlayerTransform.position.z + (spaceBetweenObstaclesForCurrentBuffer * firstEncounterOffsetMultiplier);
        coinSpawnStartPoint = nextRowGenerationPointZ - spaceBetweenObstaclesForCurrentBuffer;

        GenerateObstalcesIfNeeded(true);
        hasSpawnedFirstObstacleBlock = true;
    }

    #endregion Event handlers

    #region Public API (coins/procedural)

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

    public void OnFloatingPointReset(float movedOffset)
    {
        if (obstaclesSafeAreaSO != null)
            obstaclesSafeAreaSO.FloatingPointResetObstaclesSafeAreaList(movedOffset);

        nextRowGenerationPointZ -= movedOffset;
        coinSpawnStartPoint -= movedOffset;
        coinSpawnEndPoint -= movedOffset;
    }

    public void OnBeforeFloatingPointReset() { }

    #endregion Public API

    #region Core generation

    private void ResetAndInitializeObstaclesSpawnPoint()
    {
        if (RenderSettings.fog)
        {
            nextRowGenerationPointZ = playerSharedData.PlayerTransform.position.z + (RenderSettings.fogEndDistance + obstacleSpawnAdditionalFogSafeDistance);
            coinSpawnStartPoint = nextRowGenerationPointZ;
        }
        else
        {
            spaceBetweenObstaclesForCurrentBuffer = calculatedCurrentSpaceBetweenObstacles;
            nextRowGenerationPointZ = playerSharedData.PlayerTransform.position.z + (spaceBetweenObstaclesForCurrentBuffer * firstEncounterOffsetMultiplier);
            coinSpawnStartPoint = nextRowGenerationPointZ;
        }
    }

    /// <summary>
    /// Decide if obstacles should be generated (called periodically)
    /// </summary>
    private void GenerateObstalcesIfNeeded(bool forceGenerateObstacle = false)
    {
        float distanceBetweenNextRowGenerationPointAndPlayer = nextRowGenerationPointZ - playerSharedData.PlayerTransform.position.z;
        float totalObstacleSpawnFogSafeDistance = RenderSettings.fogEndDistance + obstacleSpawnAdditionalFogSafeDistance;

        if (!(distanceBetweenNextRowGenerationPointAndPlayer < totalObstacleSpawnFogSafeDistance || forceGenerateObstacle))
            return;

        // decide what kind of encounter to generate
        float randomProbability = Random.Range(0f, 1f);

        bool useProcedural = !customEncountersSO.AreTestingEncountersAvailable &&
                             (!customEncountersSO.AreCustomEncountersAvailable || (randomProbability > customEncounterSpawnProbabilty));

        if (useProcedural)
        {
            if (encounterRowBuffer.Count == 0)
            {
                individualObstaclesSO.RefreshAuthorizedIndividualObstacles();
                RefreshEncounterRowBuffer();
                spaceBetweenObstaclesForCurrentBuffer = calculatedCurrentSpaceBetweenObstacles;
            }

            float pointWhereNextBlockEndsZPosition = nextRowGenerationPointZ + ((encounterRowBuffer.Count - 1) * spaceBetweenObstaclesForCurrentBuffer);
            float distanceBetweenRampBuildingAndBlockStartPoint = environmentData.nextEnvSwitchPatchZPosition - nextRowGenerationPointZ;
            float distanceBetweenRampBuildingAndBlockEndPoint = environmentData.nextEnvSwitchPatchZPosition - pointWhereNextBlockEndsZPosition;
            bool isBlockStartPointBehindRampBuilding = distanceBetweenRampBuildingAndBlockStartPoint > 0f;
            bool doesBlockEndPointExceedSafeDistance = distanceBetweenRampBuildingAndBlockEndPoint < safeZoneBeforeRampBuilding;
            bool isPlayerBehindRampBuilding = environmentData.distanceBetweenCurrentRampBuildingAndPlayer > 0f;

            if (environmentData.firstRampBuildingHasBeenSpawned &&
                ((isBlockStartPointBehindRampBuilding && doesBlockEndPointExceedSafeDistance) ||
                 (isPlayerBehindRampBuilding && !isBlockStartPointBehindRampBuilding)))
            {
                if (!isGenerationPointAfterRampBuildingSet)
                {
                    isGenerationPointAfterRampBuildingSet = true;
                    float normalizedCurrentSpeedValue = Mathf.Clamp01(normalizedGameSpeedValue);
                    float currentAdditionSafeZoneAfterRampBuilding = variableAdditionalSafeZoneAfterRampBuilding * normalizedCurrentSpeedValue;
                    nextRowGenerationPointZ = environmentData.nextEnvSwitchPatchZPosition + (safeZoneAfterRampBuilding + (shouldVariableAdditionSafeDistanceBeTakenIntoAccount ? currentAdditionSafeZoneAfterRampBuilding : 0f));

                    float coinEndPointBefore = coinSpawnEndPoint;
                    coinSpawnStartPoint = coinEndPointBefore;
                    coinSpawnEndPoint = environmentData.nextEnvSwitchPatchZPosition;

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
            // custom encounters block
            float distanceBetweenRampBuildingAndBlockStartPoint = environmentData.nextEnvSwitchPatchZPosition - nextRowGenerationPointZ;
            float buildingSafeZoneStartPosition = environmentData.nextEnvSwitchPatchZPosition - safeZoneBeforeRampBuilding;
            float distanceBetweenRampBuildingSafeZoneAndBlockStartPoint = buildingSafeZoneStartPosition - nextRowGenerationPointZ;
            bool isBlockStartPointBehindRampBuildingSafeZone = distanceBetweenRampBuildingSafeZoneAndBlockStartPoint > 0f;

            float maxCustomEncounterLength = float.PositiveInfinity;
            if (environmentData.firstRampBuildingHasBeenSpawned && isBlockStartPointBehindRampBuildingSafeZone && !customEncountersSO.AreTestingEncountersAvailable)
                maxCustomEncounterLength = distanceBetweenRampBuildingSafeZoneAndBlockStartPoint;

            CustomEncounter customEncounterToSpawn = customEncountersSO.GetCustomEncounter(maxCustomEncounterLength, difficultyScaleSO.currentIntendedPlayerDifficultyRating);

            float pointWhereNextBlockEndsZPosition = nextRowGenerationPointZ + (customEncounterToSpawn != null ? customEncounterToSpawn.EncounterSize : 0f);

            float safeAreaPoint = obstaclesSafeAreaSO.CheckIfZRangeIsInsideSafeArea(nextRowGenerationPointZ, pointWhereNextBlockEndsZPosition);
            if (safeAreaPoint != -1f)
            {
                // found overlap with safe area, push start forward
                nextRowGenerationPointZ = (safeAreaPoint + (ObstaclesSafeAreaSO.safeZoneLength)) + firstEncounterOffsetMultiplier;
                coinSpawnStartPoint = nextRowGenerationPointZ;
                return;
            }

            float distanceBetweenRampBuildingAndBlockEndPoint = environmentData.nextEnvSwitchPatchZPosition - pointWhereNextBlockEndsZPosition;
            bool doesBlockEndPointExceedSafeDistance = distanceBetweenRampBuildingAndBlockEndPoint < safeZoneBeforeRampBuilding;

            if (customEncounterToSpawn == null ||
                (environmentData.firstRampBuildingHasBeenSpawned &&
                 ((distanceBetweenRampBuildingAndBlockStartPoint > 0f && doesBlockEndPointExceedSafeDistance) ||
                  (environmentData.distanceBetweenCurrentRampBuildingAndPlayer > 0f && !(distanceBetweenRampBuildingAndBlockStartPoint > 0f)))))
            {
                if (!isGenerationPointAfterRampBuildingSet)
                {
                    isGenerationPointAfterRampBuildingSet = true;
                    float normalizedCurrentSpeedValue = Mathf.Clamp01(normalizedGameSpeedValue);
                    float currentAdditionSafeZoneAfterRampBuilding = variableAdditionalSafeZoneAfterRampBuilding * normalizedCurrentSpeedValue;
                    nextRowGenerationPointZ = environmentData.nextEnvSwitchPatchZPosition + (safeZoneAfterRampBuilding + (shouldVariableAdditionSafeDistanceBeTakenIntoAccount ? currentAdditionSafeZoneAfterRampBuilding : 0f));
                    coinSpawnStartPoint = nextRowGenerationPointZ - spaceBetweenObstaclesForCurrentBuffer;
                }
            }
            else
            {
                isGenerationPointAfterRampBuildingSet = false;
                TogglePickupGenerationAccourdingToBlockEndPoint(pointWhereNextBlockEndsZPosition);
                SpawnCustomEncounter(customEncounterToSpawn);

                if (customEncountersSO.AreTestingEncountersAvailable)
                    customEncountersSO.IncrementTestingEncounterIndex();
            }
        }
    }

    #endregion Core generation

    #region Procedural & custom spawn helpers

    private void ProcedurallySpawnObstacles()
    {
        // reuse spring ramp buffer
        springRampHandlersBuffer.Clear();

        // iterate rows in buffer until consumed
        while (encounterRowBuffer.Count > 0)
        {
            // if the next row is inside safe area, skip spawning it
            if (obstaclesSafeAreaSO.CheckIfZPositionIsInsideSafeArea(nextRowGenerationPointZ))
            {
                // Don't spawn this row now; we just advance pointers/exit
                break;
            }

            var row = encounterRowBuffer.First.Value;
            // spawn obstacles for 3 lanes (x = -1,0,1)
            for (int currentColumn = 0; currentColumn < 3; currentColumn++)
            {
                var cfg = row.columnsInRow[currentColumn];
                if (cfg == null) continue;

                GameObject obstacleToSpawn = cfg.obstacleToSpawn;
                if (obstacleToSpawn == null) continue;

                Obstacle originalObstacle = obstacleToSpawn.GetComponent<Obstacle>();
                if (originalObstacle == null) continue;

                Obstacle fetchedObstacle = obstaclePoolSO.Request(originalObstacle, originalObstacle.InstanceID);
                if (fetchedObstacle == null) continue;

                fetchedObstacle.ObstacleHasFinished += HandleObstacleFinished;

                float randOffset = Random.Range(-(playerSharedData.Playercollider.size.z / 4f), playerSharedData.Playercollider.size.z / 3.5f);

                // check specific instance IDs safely (protect against missing dict or missing keys)
                try
                {
                    var dict = individualObstaclesSO.allIndividualObstacles;
                    if (dict != null && dict.Count > 0)
                    {
                        Obstacle oneArm = dict.ContainsKey("One-Arm Automatic Barrier") ? dict["One-Arm Automatic Barrier"].GetComponent<Obstacle>() : null;
                        Obstacle threeArm = dict.ContainsKey("Three-Arm Automatic Barrier") ? dict["Three-Arm Automatic Barrier"].GetComponent<Obstacle>() : null;
                        if ((oneArm != null && fetchedObstacle.InstanceID == oneArm.InstanceID) ||
                            (threeArm != null && fetchedObstacle.InstanceID == threeArm.InstanceID))
                        {
                            randOffset = 0f;
                        }
                    }
                }
                catch { /* swallow dictionary read exceptions to be safe */ }

                fetchedObstacle.transform.SetPositionAndRotation(new Vector3(currentColumn - 1, obstacleToSpawn.transform.position.y, nextRowGenerationPointZ + randOffset), Quaternion.identity);

                SpringRampHandler sr = fetchedObstacle.GetComponent<SpringRampHandler>();
                if (sr != null) springRampHandlersBuffer.Add(sr);
            }

            // remove the spawned row
            encounterRowBuffer.RemoveFirst();

            // advance next row pointer
            if (encounterRowBuffer.Count > 0)
            {
                nextRowGenerationPointZ += spaceBetweenObstaclesForCurrentBuffer;
                coinSpawnEndPoint = nextRowGenerationPointZ;
            }
            else
            {
                coinSpawnEndPoint = nextRowGenerationPointZ + ((spaceBetweenObstaclesForCurrentBuffer * spaceBetweenRowBlocksMultiplier) / 2f);
            }

            // For block-based generation continue loop; for row-based generation break after one row
            if (obstacleGenerationType != GenerationType.BlockBasedGeneration)
                break;
        }

        // if any spring ramps found, initialize one of them
        if (springRampHandlersBuffer.Count > 0)
        {
            int idx = Random.Range(0, Mathf.Max(1, springRampHandlersBuffer.Count));
            springRampHandlersBuffer[Mathf.Clamp(idx, 0, springRampHandlersBuffer.Count - 1)].InitializeSpringRamp();
        }

        obstaclesHasBeenSpawned.RaiseEvent();

        coinSpawnStartPoint = coinSpawnEndPoint;

        if (encounterRowBuffer.Count == 0)
        {
            spaceBetweenObstaclesForCurrentBuffer = calculatedCurrentSpaceBetweenObstacles;
            nextRowGenerationPointZ += spaceBetweenObstaclesForCurrentBuffer * spaceBetweenRowBlocksMultiplier;
        }
    }

    private void SpawnCustomEncounter(CustomEncounter customEncounterToSpawn)
    {
        if (customEncounterToSpawn == null || customEncounterPoolSO == null) return;

        CustomEncounter fetchedCustomEncounter = customEncounterPoolSO.Request(customEncounterToSpawn, customEncounterToSpawn.InstanceID);
        if (fetchedCustomEncounter == null) return;

        fetchedCustomEncounter.transform.SetPositionAndRotation(new Vector3(0f, 0f, nextRowGenerationPointZ + customEncounterToSpawn.encounterOffset), Quaternion.identity);
        fetchedCustomEncounter.InitializeCustomEncounter();
        fetchedCustomEncounter.CustomEncounterHasFinished += HandleCustomEncounterFinished;

        coinSpawnEndPoint = nextRowGenerationPointZ + fetchedCustomEncounter.EncounterSize + ((calculatedCurrentSpaceBetweenObstacles * spaceBetweenRowBlocksMultiplier) / 2f);

        if (!(customEncountersSO.pauseCoinAndPickupGenerationInTestingEncounter && customEncountersSO.AreTestingEncountersAvailable))
        {
            obstaclesHasBeenSpawned.RaiseEvent();
        }

        coinSpawnStartPoint = coinSpawnEndPoint;
        spaceBetweenObstaclesForCurrentBuffer = calculatedCurrentSpaceBetweenObstacles;
        nextRowGenerationPointZ += fetchedCustomEncounter.EncounterSize + (calculatedCurrentSpaceBetweenObstacles * spaceBetweenRowBlocksMultiplier);
    }

    private void HandleCustomEncounterFinished(CustomEncounter customEncounter)
    {
        if (customEncounter == null) return;
        customEncounter.CustomEncounterHasFinished -= HandleCustomEncounterFinished;
        customEncounter.ResetCustomEncounter();
        customEncounterPoolSO.Return(customEncounter);
    }

    private void HandleObstacleFinished(Obstacle obstacle)
    {
        if (obstacle == null) return;
        obstacle.ObstacleHasFinished -= HandleObstacleFinished;

        SpringRampHandler springRampHandler = obstacle.GetComponent<SpringRampHandler>();
        springRampHandler?.ReturnSpringRamp();

        obstacle.RestTheObstacle();
        obstaclePoolSO.Return(obstacle);
    }

    #endregion Procedural & custom spawn helpers

    #region Buffer creation (optimized)

    private void InitializeReusableBuffers()
    {
        // init list-of-lists arrays as needed
        emptySpaces = new List<int>[3];
        filledSpaces = new List<int>[3];
        laneSwitchObstacleSpawnPoints = new List<int>[3];
        nonActionObstacleSpawnPoints = new List<int>[3];

        for (int i = 0; i < 3; i++)
        {
            emptySpaces[i] = new List<int>(rowsInOneBlock);
            filledSpaces[i] = new List<int>(rowsInOneBlock);
            laneSwitchObstacleSpawnPoints[i] = new List<int>(rowsInOneBlock);
            nonActionObstacleSpawnPoints[i] = new List<int>(rowsInOneBlock);
        }

        rowsToBeAddedToTheBuffer = new EncounterRow[rowsInOneBlock];
        for (int i = 0; i < rowsInOneBlock; i++)
            rowsToBeAddedToTheBuffer[i] = new EncounterRow();
    }

    /// <summary>
    /// Build encounterRowBuffer in an optimized way with reusable lists
    /// </summary>
    private void RefreshEncounterRowBuffer()
    {
        // Clear structures
        for (int i = 0; i < 3; i++)
        {
            emptySpaces[i].Clear();
            filledSpaces[i].Clear();
            nonActionObstacleSpawnPoints[i].Clear();
            laneSwitchObstacleSpawnPoints[i].Clear();

            // fill empty spaces with row indices
            for (int r = 0; r < rowsInOneBlock; r++)
                emptySpaces[i].Add(r);
        }

        // compute difficulty-derived values
        float currentNormalizedDifficultyValue = (difficultyScaleSO.GetRandomPointOnCurrentDifficultyRange() / 100f);

        int upperBoundOfMinimumActionsRequired = (rowsInOneBlock * 2) + 1;
        int lowerBoundOfMinimumActionsRequired = 1;
        int currentMinimumRequiredActions = (int)(lowerBoundOfMinimumActionsRequired + (((upperBoundOfMinimumActionsRequired - lowerBoundOfMinimumActionsRequired) * currentNormalizedDifficultyValue)));

        if (currentMinimumRequiredActions > 3 && Random.Range(0f, 1f) <= chanceToClampDifficultyToMaxDifficultyWithoutTrucksAffectingMinAction)
            currentMinimumRequiredActions = rowsInOneBlock;

        // avg action calc
        int easiestLane = Random.Range(0, 3);
        int maxExtraObstaclesThatCanBeGenerated = ((rowsInOneBlock - currentMinimumRequiredActions) * 2);
        int currentExtraObstaclesToGenerate = (int)((maxExtraObstaclesThatCanBeGenerated + 1) *
            (((upperBoundOfMinimumActionsRequired - lowerBoundOfMinimumActionsRequired) * currentNormalizedDifficultyValue) % 1f));

        // prepare rowsToBeAddedToTheBuffer
        for (int i = 0; i < rowsInOneBlock; i++)
            rowsToBeAddedToTheBuffer[i].Clear();

        // Fill empty spaces with min actions required
        for (int lane = 0; lane < 3; lane++)
        {
            int minActions = Mathf.Clamp(currentMinimumRequiredActions, 0, rowsInOneBlock);
            for (int j = 0; j < minActions; j++)
            {
                if (emptySpaces[lane].Count == 0) break;
                int randIndex = Random.Range(0, emptySpaces[lane].Count);
                int chosenRow = emptySpaces[lane][randIndex];
                filledSpaces[lane].Add(chosenRow);
                emptySpaces[lane].RemoveAt(randIndex);
            }
        }

        // Fill lanes except easiest with extra obstacles
        List<int> columnsToConsider = new List<int>(3) { 0, 1, 2 };
        columnsToConsider.Remove(easiestLane);

        for (int i = 0; i < currentExtraObstaclesToGenerate + 1; i++)
        {
            if (columnsToConsider.Count == 0) break;
            int randColumnIndex = Random.Range(0, columnsToConsider.Count);
            int randColumn = columnsToConsider[randColumnIndex];
            if (emptySpaces[randColumn].Count == 0)
            {
                columnsToConsider.RemoveAt(randColumnIndex);
                i--; // try again if a column had no empty slots
                continue;
            }
            int randRowIndex = Random.Range(0, emptySpaces[randColumn].Count);
            filledSpaces[randColumn].Add(emptySpaces[randColumn][randRowIndex]);
            emptySpaces[randColumn].RemoveAt(randRowIndex);
            if (emptySpaces[randColumn].Count == 0) columnsToConsider.RemoveAt(randColumnIndex);
        }

        // place at most one transporter (non-action obstacle) in a random column if chance permits
        List<int> randColumnsToPlaceTransporterIn = new List<int>(3) { 0, 1, 2 };
        for (int i = 0; i < 3 && randColumnsToPlaceTransporterIn.Count > 0; i++)
        {
            int pickIndex = Random.Range(0, randColumnsToPlaceTransporterIn.Count);
            int column = randColumnsToPlaceTransporterIn[pickIndex];
            randColumnsToPlaceTransporterIn.RemoveAt(pickIndex);
            if (emptySpaces[column].Count == 0) continue;
            if (Random.Range(0f, 1f) > 0.1f) continue; // transporter spawn prob ~0.1
            int randEmptyRowIndex = Random.Range(0, emptySpaces[column].Count);
            int randObstacleIndex = Random.Range(0, individualObstaclesSO.individualNonActionObstacles.Length);
            rowsToBeAddedToTheBuffer[emptySpaces[column][randEmptyRowIndex]]
                .columnsInRow[column] = new ObstacleConfiguration(individualObstaclesSO.individualNonActionObstacles[randObstacleIndex].obstaclePrefab);
            emptySpaces[column].RemoveAt(randEmptyRowIndex);
            break;
        }

        // handle trucks that increase min actions (keeps your logic but optimized slightly)
        if (currentMinimumRequiredActions > rowsInOneBlock)
        {
            int actionsToIncrease = currentMinimumRequiredActions - rowsInOneBlock;
            int rowCountOccupiedByTrucks = 2 + (int)((actionsToIncrease - 1) / 2);
            int trucksToSpawn = 3 + (actionsToIncrease - 1);

            List<int> unselectedRows = new List<int>(rowsInOneBlock);
            for (int r = 0; r < rowsInOneBlock; r++) unselectedRows.Add(r);

            List<int> rowsOccupiedByTrucks = new List<int>(rowCountOccupiedByTrucks);
            for (int i = 0; i < rowCountOccupiedByTrucks && unselectedRows.Count > 0; i++)
            {
                int ri = Random.Range(0, unselectedRows.Count);
                rowsOccupiedByTrucks.Add(unselectedRows[ri]);
                unselectedRows.RemoveAt(ri);
            }

            // optionally shuffle order (previous code used OrderBy/OrderByDescending)
            if (Random.Range(0, 2) == 0) rowsOccupiedByTrucks.Sort((a, b) => b.CompareTo(a));
            else rowsOccupiedByTrucks.Sort();

            int randColumnBorder = (Random.Range(0, 2) == 1) ? 2 : 0;

            for (int i = 0; i < rowsOccupiedByTrucks.Count && trucksToSpawn > 0; i++)
            {
                laneSwitchObstacleSpawnPoints[randColumnBorder].Add(rowsOccupiedByTrucks[i]);
                trucksToSpawn--;
                if (trucksToSpawn <= 0) break;

                laneSwitchObstacleSpawnPoints[1].Add(i);
                trucksToSpawn--;
                if (trucksToSpawn <= 0) break;

                randColumnBorder = randColumnBorder == 0 ? 2 : 0;
            }
        }

        // spawn obstacles at filled spaces
        int randomDirection = (Random.Range(0, 2) == 0) ? -1 : 1;

        for (int laneLoop = 0; laneLoop < 3; laneLoop++)
        {
            int directionalLoopIndex = randomDirection == 1 ? laneLoop : 2 - laneLoop;
            var filled = filledSpaces[directionalLoopIndex];

            for (int j = 0; j < filled.Count; j++)
            {
                int rowIndex = filled[j];

                // determine whether to place lane switch obstacle (truck) or normal
                bool placeLaneSwitch = laneSwitchObstacleSpawnPoints[directionalLoopIndex].Contains(rowIndex);

                if (placeLaneSwitch)
                {
                    int randomObstacleIndex = Random.Range(0, individualObstaclesSO.individualForceLaneChangeObstacle.Length);
                    rowsToBeAddedToTheBuffer[rowIndex].columnsInRow[directionalLoopIndex] = new ObstacleConfiguration(individualObstaclesSO.individualForceLaneChangeObstacle[randomObstacleIndex].obstaclePrefab);
                }
                else
                {
                    int randomObstacleIndex = Random.Range(0, individualObstaclesSO.CurrentAuthorizedIndividualObstacles.Length);
                    rowsToBeAddedToTheBuffer[rowIndex].columnsInRow[directionalLoopIndex] = new ObstacleConfiguration(individualObstaclesSO.CurrentAuthorizedIndividualObstacles[randomObstacleIndex].obstaclePrefab);
                }
            }
        }

        // add generated rows to encounter buffer (skip empty rows)
        for (int i = 0; i < rowsToBeAddedToTheBuffer.Length; i++)
        {
            bool rowIsEmpty = true;
            for (int c = 0; c < 3; c++)
            {
                if (rowsToBeAddedToTheBuffer[i].columnsInRow[c] != null)
                {
                    rowIsEmpty = false;
                    break;
                }
            }
            if (!rowIsEmpty)
            {
                // create a new small object container (we reuse the EncounterRow instances)
                EncounterRow rr = new EncounterRow();
                rr.columnsInRow[0] = rowsToBeAddedToTheBuffer[i].columnsInRow[0];
                rr.columnsInRow[1] = rowsToBeAddedToTheBuffer[i].columnsInRow[1];
                rr.columnsInRow[2] = rowsToBeAddedToTheBuffer[i].columnsInRow[2];
                encounterRowBuffer.AddLast(rr);
            }
            // clear the rowsToBeAdded for next use
            rowsToBeAddedToTheBuffer[i].Clear();
        }
    }

    #endregion Buffer creation

}
