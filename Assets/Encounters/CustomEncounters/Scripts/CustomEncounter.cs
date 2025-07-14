using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;

#if (UNITY_EDITOR)

using UnityEditor.SceneManagement;

#endif

[ExecuteInEditMode]
public class CustomEncounter : MonoBehaviour, IValidatable
{

    [ReadOnly] public bool IsSkeleton = false;
    [SerializeField] private int instanceID = 0;
    public int InstanceID => instanceID;

    [SerializeField] private bool canSpawnInLowDifficulty;
    [SerializeField] private float requiredDifficultyInPercent = -1;
    public bool CanSpawnInLowDifficulty => canSpawnInLowDifficulty;

    public float RequiredDifficultyInPercent => requiredDifficultyInPercent;

    [Header("Extention Distances")]
    [SerializeField] private float startExtentionDis;

    [SerializeField] private float endExtentionDis;

    [Header("General")]
    [ReadOnly] [SerializeField] private float encounterSize;

    [Tooltip("Offset required to spawn the encounter at the correct position")] // This counters the situations where the position of the prefab object is not at the encounter start positions
    [ReadOnly] public float encounterOffset;

    public event Action<CustomEncounter> CustomEncounterHasFinished;

    [ReadOnly] [SerializeField] private List<Obstacle> obstaclesList = new List<Obstacle>();
    [ReadOnly] [SerializeField] private List<ObstacleSynchronizer> obstacleSynchronizerList = new List<ObstacleSynchronizer>();
    [ReadOnly] [SerializeField] private List<PreplacedCoinGenerator> preplacedCoinGeneratorList = new List<PreplacedCoinGenerator>();
    [ReadOnly] [SerializeField] private List<ActionTimePreserver> actionTimePreserverList = new List<ActionTimePreserver>();

    private bool isInitializedPostSpawn = false;

    [ShowIf("IsSkeleton")]
    [SerializeField] public CustomEncounterSkeleton CustomEncounterSkeleton;

    [SerializeField] private ObstaclePoolSO obstaclePoolSO;
    [SerializeField] private ObstaclesSafeAreaSO obstaclesSafeAreaSO;

    [SerializeField] private GameEvent obstaclesSafeAreaIsUpdated;
    [SerializeField] private GameEvent playerFinishedBoostingEvent;
    [SerializeField] private GameEvent playerHasRevived;

    [SerializeField] private CustomEncounter Temp;

    public float EncounterSize
    {
        get
        {
            if (isInitializedPostSpawn)
            {
                return encounterSize;
            }

            //if (Temp != null)
            //{
            //    UnityEngine.Console.Log($"Calculated From Original {Temp.CalculateInstantaneousVirtualEncounterSize()}");
            //}

            if (IsSkeleton)
            {
                float fromSkeleton = CalculateInstantaneousVirtualEncounterSizeFromSkeleton();
              //  UnityEngine.Console.Log($"Calculated From Skeleton {fromSkeleton}");
                return fromSkeleton;
            }
            else
            {
                return CalculateInstantaneousVirtualEncounterSize();
            }

         

            
        }
    }

    private bool destructionInProcess;

    [Header("References")]
    [SerializeField] private PlayerSharedData playerSharedData;

    private void Awake()
    {
        if (!Application.isPlaying)
            return;

        if (!IsSkeleton)
            return;

        if (CustomEncounterSkeleton == null)
            throw new System.Exception("Custom encounter skeleton was null. Make sure you are using custom encounters within the skeletons folder");

        CustomEncounterSkeleton.OnObstaclesSpawnedFromMetaData += GetReferencesInPlayMode;
    }

    private void OnEnable()
    {
        if (!Application.isPlaying || !IsSkeleton)
            return;

        obstaclesSafeAreaIsUpdated.TheEvent.AddListener(HandleObstaclesSafeAreaIsUpdated);
        playerFinishedBoostingEvent.TheEvent.AddListener(HandlePlayerBoostingAndReviveEvents);
        playerHasRevived.TheEvent.AddListener(HandlePlayerBoostingAndReviveEvents);
    }

    private void OnDisable()
    {
        if (!Application.isPlaying || !IsSkeleton)
            return;

        obstaclesSafeAreaIsUpdated.TheEvent.RemoveListener(HandleObstaclesSafeAreaIsUpdated);
        playerFinishedBoostingEvent.TheEvent.RemoveListener(HandlePlayerBoostingAndReviveEvents);
        playerHasRevived.TheEvent.RemoveListener(HandlePlayerBoostingAndReviveEvents);
    }

    private void HandleObstaclesSafeAreaIsUpdated(GameEvent gameEvent)
    {
        // For debugging safe area
        //UnityEngine.Console.Log($"Custom encounter transform when safe area is updated {gameObject}, {transform.position.z}");
        
        float startPos = transform.position.z - encounterOffset;
        if (obstaclesSafeAreaSO.CheckIfZRangeIsInsideSafeArea(startPos, startPos + EncounterSize) != -1)
        {
            UnityEngine.Console.Log("Custom encounter is inside safe area.... Disabling it.");
            CustomEncounterHasFinished?.Invoke(this);
        }
        //else
        //{
        //    // For debugging safe area
        //    Debug.DrawRay(new Vector3(transform.position.x, transform.position.y, startPos), Vector3.up * 12f, Color.black, 10f);
        //    Debug.DrawRay(new Vector3(transform.position.x, transform.position.y, startPos + EncounterSize), Vector3.up * 12f, Color.black, 10f);
        //}
    }

    private void HandlePlayerBoostingAndReviveEvents(GameEvent theEvent)
    {
        if ((playerSharedData.CurrentStateName == PlayerState.PlayerAeroplaneState || playerSharedData.WallRunBuilding
            || playerSharedData.CurrentStateName == PlayerState.PlayerThurstState) && theEvent == playerFinishedBoostingEvent)
            return;

        float playerZPos = playerSharedData.PlayerTransform.position.z;
        float customEncounterStartPos = transform.position.z - encounterOffset;
        float playerDistFromStartPos = customEncounterStartPos - playerZPos;

        if ((customEncounterStartPos <= playerZPos && playerZPos <= customEncounterStartPos + EncounterSize)
            || (GameManager.BoostExplosionDistance >= playerDistFromStartPos && playerDistFromStartPos > 0))
        {
            if (theEvent == playerFinishedBoostingEvent)
            {
                foreach (Obstacle _obstacle in obstaclesList)
                {
                    _obstacle.DestroyThisObstacle();
                }

                foreach (ObstacleSynchronizer obstacleSynchronizer in obstacleSynchronizerList)
                {
                    foreach (SyncedObstacleColumn syncedObstacleColumn in obstacleSynchronizer.SynchronizedColumns)
                    {
                        foreach (SyncedObstacle syncedObstacle in syncedObstacleColumn.synchronizedObstacles)
                        {
                            syncedObstacle.FetchedObstacle.GetComponent<Obstacle>()?.DestroyThisObstacle();
                        }
                    }
                }
            }
            else
            {
                CustomEncounterHasFinished?.Invoke(this);
            }
        }
    }

#if UNITY_EDITOR

    [Button("Validate custom encounter")]
    public void ValidateThisCustomEncounter()
    {
        UnityEngine.Console.Log($"Initiating validation of {gameObject.name} custom encounter");

        IValidatable[] validatableScripts = GetComponentsInChildren<IValidatable>();
        foreach (IValidatable validatableScript in validatableScripts)
        {
            validatableScript.ValidateAndInitialize();
        }

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();

        UnityEngine.Console.Log($"Completed validation of {gameObject.name} custom encounter");
    }

#endif

    private void OnDrawGizmos()
    {
        if (IsSkeleton)
            return;

        if (isInitializedPostSpawn)
            return;

        Vector3 rectangleDimensions = new Vector3(0.8f, 0.00001f, EncounterSize);
        for (int i = -1; i < 2; i++)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(new Vector3(i, -rectangleDimensions.y / 2, transform.position.z - encounterOffset + (EncounterSize / 2)), rectangleDimensions);
        }
    }

    public void InitializeCustomEncounter()
    {
        destructionInProcess = false;
        isInitializedPostSpawn = false;

        if (!Application.isPlaying)
            return;

#if (UNITY_EDITOR)
        if (PrefabStageUtility.GetCurrentPrefabStage() != null &&
        PrefabStageUtility.GetCurrentPrefabStage() == PrefabStageUtility.GetPrefabStage(transform.root.gameObject))
            return;
#endif

        bool encounterModified = false;

        ActionTimePreserver[] actionTimePreservers = GetComponentsInChildren<ActionTimePreserver>();
        foreach (ActionTimePreserver actionTimePreserver in actionTimePreservers)
        {
            actionTimePreserver.InitializeActionTimePreserver();
            encounterModified = true;
        }

        ShuffleObstaclesToUniqueColumns[] shuffleObstaclesToUniqueColumnsComponents = GetComponentsInChildren<ShuffleObstaclesToUniqueColumns>();
        foreach (ShuffleObstaclesToUniqueColumns shuffleObstaclesToUniqueColumnsComponent in shuffleObstaclesToUniqueColumnsComponents)
        {
            shuffleObstaclesToUniqueColumnsComponent.InitializeShuffleObstaclesToUniqueColumns();
            encounterModified = true;
        }

        ShuffleObstaclePairsToProvideUniqueEmptyLanes[] shuffleObstaclePairsToProvideUniqueEmptyLanesComponents = GetComponentsInChildren<ShuffleObstaclePairsToProvideUniqueEmptyLanes>();
        foreach (ShuffleObstaclePairsToProvideUniqueEmptyLanes shuffleObstaclePairsToProvideUniqueEmptyLanesComponent in shuffleObstaclePairsToProvideUniqueEmptyLanesComponents)
        {
            shuffleObstaclePairsToProvideUniqueEmptyLanesComponent.InitializeShuffleObstaclePairsToProvideUniqueEmptyLanes();
            encounterModified = true;
        }

        ShuffleObstaclesBySwappingColumns[] shuffleObstaclesBySwappingColumnsComponents = GetComponentsInChildren<ShuffleObstaclesBySwappingColumns>();
        foreach (ShuffleObstaclesBySwappingColumns shuffleObstaclesBySwappingColumnsComponent in shuffleObstaclesBySwappingColumnsComponents)
        {
            shuffleObstaclesBySwappingColumnsComponent.InitializeShuffleObstaclesBySwappingColumns();
            encounterModified = true;
        }

        ShuffleObstaclesBySwappingCustomGroups[] shuffleObstaclesBySwappingCustomGroupsComponents = GetComponentsInChildren<ShuffleObstaclesBySwappingCustomGroups>();
        foreach (ShuffleObstaclesBySwappingCustomGroups shuffleObstaclesBySwappingCustomGroupsComponent in shuffleObstaclesBySwappingCustomGroupsComponents)
        {
            shuffleObstaclesBySwappingCustomGroupsComponent.InitializeShuffleObstaclesBySwappingCustomGroups();
            encounterModified = true;
        }

        if (encounterModified)
        {
            ValidateAndInitialize();
        }

        ObstacleSynchronizer[] obstacleSynchronizerComponents = GetComponentsInChildren<ObstacleSynchronizer>();
        foreach (ObstacleSynchronizer obstacleSynchronizerComponent in obstacleSynchronizerComponents)
        {
            obstacleSynchronizerComponent.InitializeObstacleSynchronizer();
        }

        isInitializedPostSpawn = true;

        PreplacedCoinGenerator[] preplacedCoinSpawners = GetComponentsInChildren<PreplacedCoinGenerator>();
        foreach (PreplacedCoinGenerator preplacedCoinSpawner in preplacedCoinSpawners)
        {
            if (preplacedCoinSpawner.gameObject.GetComponent<SpringRampHandler>())
                continue;
            preplacedCoinSpawner.GenerateCoins();
        }
    }

    public void ResetCustomEncounter()
    {
        foreach (ObstacleSynchronizer obstacleSynchronizer in obstacleSynchronizerList)
        {
            foreach (SyncedObstacleColumn syncedObstacleColumn in obstacleSynchronizer.SynchronizedColumns)
            {
                foreach (SyncedObstacle syncedObstacle in syncedObstacleColumn.synchronizedObstacles)
                {
                    syncedObstacle.ReturnSyncedObstacle();
                }
            }
        }
        foreach (Obstacle obstacle in obstaclesList)
        {
            obstacle.GetComponent<SpringRampHandler>()?.ReturnSpringRamp();
            //obstacle.ResetObstaclePositionAndState();
            obstacle.RestTheObstacle();
            obstaclePoolSO.Return(obstacle);
        }
        foreach (ActionTimePreserver actionTimePreserver in actionTimePreserverList)
        {
            foreach (ActionTimePreserverSubject preservedSubject in actionTimePreserver.PreservedSubjects)
            {
                preservedSubject.ResetPreserverSubjectPosition();
            }
        }
        foreach (PreplacedCoinGenerator preplacedCoinSpawner in preplacedCoinGeneratorList)
        {
            if (preplacedCoinSpawner.gameObject.GetComponent<SpringRampHandler>())
                continue;
            preplacedCoinSpawner.ReturnCoins();
        }
    }

    public void ValidateAndInitialize()
    {
        CalculateInstantaneousEncounterSize();
        CalculateEncounterOffset();
    }

    private void GetReferencesInPlayMode()
    {
        obstaclesList = new List<Obstacle>(GetComponentsInChildren<Obstacle>());
        obstacleSynchronizerList = new List<ObstacleSynchronizer>(GetComponentsInChildren<ObstacleSynchronizer>());
        preplacedCoinGeneratorList = new List<PreplacedCoinGenerator>(GetComponentsInChildren<PreplacedCoinGenerator>());
        actionTimePreserverList = new List<ActionTimePreserver>(GetComponentsInChildren<ActionTimePreserver>());
    }

    private void GetReferencesInEditor()
    {
        if (transform.childCount > 0)
        {
            obstaclesList = new List<Obstacle>(GetComponentsInChildren<Obstacle>());
            obstacleSynchronizerList = new List<ObstacleSynchronizer>(GetComponentsInChildren<ObstacleSynchronizer>());
            preplacedCoinGeneratorList = new List<PreplacedCoinGenerator>(GetComponentsInChildren<PreplacedCoinGenerator>());
            actionTimePreserverList = new List<ActionTimePreserver>(GetComponentsInChildren<ActionTimePreserver>());
        }
    }

    private void CalculateInstantaneousEncounterSize()
    {
        if (transform.childCount > 0)
        {
            float minZPos = Mathf.Infinity;
            float maxZPos = Mathf.NegativeInfinity;

            List<Transform> obstacles = new List<Transform>();
            ObjectDestruction[] objectDestructionComponents = GetComponentsInChildren<ObjectDestruction>();
            SyncedObstacle[] syncedObstacleComponents = GetComponentsInChildren<SyncedObstacle>(); // For unspawned obstacles

            foreach (ObjectDestruction objectDestructionComponent in objectDestructionComponents)
            {
                obstacles.Add(objectDestructionComponent.transform.parent.transform);
            }

            foreach (SyncedObstacle syncedObstacleComponent in syncedObstacleComponents)
            {
                obstacles.Add(syncedObstacleComponent.transform);
            }

            foreach (Transform obstacle in obstacles)
            {
                float obstacleParentZPos = obstacle.position.z;

                if (obstacleParentZPos < minZPos)
                {
                    minZPos = obstacleParentZPos;
                }

                if (obstacleParentZPos > maxZPos)
                {
                    maxZPos = obstacleParentZPos;
                }
            }

            encounterSize = maxZPos - minZPos;

            encounterSize += startExtentionDis;
            encounterSize += endExtentionDis;
        }
        else
        {
            encounterSize = 0;
        }
    }



    /// <summary>
    /// Virtual size takes dynamic distance into account and gives the dynamically modified size of the encounter from the skeleton without needing to instantiate it
    /// </summary>
    /// <returns></returns>
    [Button("CalculateFromSkeleton")]
    public float CalculateInstantaneousVirtualEncounterSizeFromSkeleton()
    {
        if (transform.childCount > 0)
        {
            float minZPos = Mathf.Infinity;
            float maxZPos = Mathf.NegativeInfinity;

            Dictionary<int, float> obstaclesWithZPos = new Dictionary<int, float>();

            foreach (var objectDestructionComponent in CustomEncounterSkeleton.customEncounterObjectDestructionMetaData)
            {
                if (!obstaclesWithZPos.ContainsKey(objectDestructionComponent.ParentCachedInstanceID))
                {
                    obstaclesWithZPos.Add(objectDestructionComponent.ParentCachedInstanceID, objectDestructionComponent.OriginalWordPosition.z);
                }
            }

            foreach (var syncedObstacleComponent in CustomEncounterSkeleton.customEncounterSyncedObstacleMetaData)
            {
                if (!obstaclesWithZPos.ContainsKey(syncedObstacleComponent.ParentCachedInstanceID))
                {
                    obstaclesWithZPos.Add(syncedObstacleComponent.ParentCachedInstanceID, syncedObstacleComponent.OriginalWordPosition.z);
                }
            }

            ActionTimePreserver[] actionTimePreservers = GetComponentsInChildren<ActionTimePreserver>();

          //  UnityEngine.Console.Log($"Action Time Preserver Count {actionTimePreservers.Length}");

            foreach (ActionTimePreserver actionTimePreserver in actionTimePreservers)
            {
              
                if (actionTimePreserver.isActionTimePreservationComplete)
                { 
                  //  UnityEngine.Console.Log("Already Preservation Completed");
                    continue;
                }

                actionTimePreserver.ResetVirtualDeltaPositonsOfSubjects();
                actionTimePreserver.UnchainSubjects();
                actionTimePreserver.ChainPreservedSubjectsTogether();

              //  UnityEngine.Console.Log($"Preserved Subjects List { actionTimePreserver.PreservedSubjects.Count}");

                foreach (ActionTimePreserverSubject actionTimePreserverSubject in actionTimePreserver.PreservedSubjects)
                {
                    var preservedObjectDestructionComponents = CustomEncounterSkeleton.actionTimeSubjectsChildObjectDestructions[actionTimePreserverSubject].actionTimeSubjectsChildObjectDestructions;
                    var preserverdSyncedObstacleComponents = CustomEncounterSkeleton.actionTimePreserversChildSyncedObstacles[actionTimePreserver];
                    List<int> preservedObstacleTransforms = new List<int>();

                    foreach (var preservedObjectDestructionComponent in preservedObjectDestructionComponents)
                    {
                        if (!preservedObstacleTransforms.Contains(preservedObjectDestructionComponent.ParentCachedInstanceID))
                        {
                            preservedObstacleTransforms.Add(preservedObjectDestructionComponent.ParentCachedInstanceID);
                        }
                    }

                    foreach (var preserverdSyncedObstacleComponent in preserverdSyncedObstacleComponents)
                    {
                        if (!preservedObstacleTransforms.Contains(preserverdSyncedObstacleComponent.ParentCachedInstanceID))
                        {
                            preservedObstacleTransforms.Add(preserverdSyncedObstacleComponent.ParentCachedInstanceID);
                        }
                    }

                  //  UnityEngine.Console.Log($"Values {actionTimePreserver.preservationMultiplier} and {actionTimePreserver.calculatedObstacleSpacingMultiplier}");
                    float deltaDisToPreserveTime = actionTimePreserverSubject.GetDeltaDistanceToPreserveActionTime(actionTimePreserver.preservationMultiplier, actionTimePreserver.calculatedObstacleSpacingMultiplier);
                 //   UnityEngine.Console.Log("DeltaDistToPreserveTime " + deltaDisToPreserveTime);

                    actionTimePreserverSubject.Move(Vector3.forward * deltaDisToPreserveTime, true);

                    foreach (int preservedObstacleTransform in preservedObstacleTransforms)
                    {
                        obstaclesWithZPos[preservedObstacleTransform] = obstaclesWithZPos[preservedObstacleTransform] + actionTimePreserverSubject.virtualDeltaPos.z;
                    }
                }
            }

            foreach (float obstacleZPos in obstaclesWithZPos.Values)
            {
                if (obstacleZPos < minZPos)
                {
                    minZPos = obstacleZPos;
                }

                if (obstacleZPos > maxZPos)
                {
                    maxZPos = obstacleZPos;
                }
            }

            float encounterSize = maxZPos - minZPos;

            encounterSize += startExtentionDis;
            encounterSize += endExtentionDis;

            return encounterSize;
        }

        return 0;
    }


    //[Button("CalculateFromOriginal")]
    //public float TempMethod()
    //{
    //    return Temp.CalculateInstantaneousVirtualEncounterSize();
    //}

    /// <summary>
    /// Virtual size takes dynamic distance into account and gives the dynamically modified size of the encounter without needing to instantiate it
    /// </summary>
    /// <returns></returns>
    public float CalculateInstantaneousVirtualEncounterSize()
    {
        if (transform.childCount > 0)
        {
            float minZPos = Mathf.Infinity;
            float maxZPos = Mathf.NegativeInfinity;

            Dictionary<Transform, float> obstaclesWithZPos = new Dictionary<Transform, float>();
            ObjectDestruction[] objectDestructionComponents = GetComponentsInChildren<ObjectDestruction>();

            foreach (ObjectDestruction objectDestructionComponent in objectDestructionComponents)
            {
                if (!obstaclesWithZPos.ContainsKey(objectDestructionComponent.transform.parent.transform))
                {
                    obstaclesWithZPos.Add(objectDestructionComponent.transform.parent.transform, objectDestructionComponent.transform.parent.transform.position.z);
                }
            }

            SyncedObstacle[] syncedObstacleComponents = GetComponentsInChildren<SyncedObstacle>(); // For unspawned obstacles

            foreach (SyncedObstacle syncedObstacleComponent in syncedObstacleComponents)
            {
                if (!obstaclesWithZPos.ContainsKey(syncedObstacleComponent.transform))
                {
                    obstaclesWithZPos.Add(syncedObstacleComponent.transform, syncedObstacleComponent.transform.position.z);
                }
            }

            ActionTimePreserver[] actionTimePreservers = GetComponentsInChildren<ActionTimePreserver>();

            UnityEngine.Console.Log($"Action Time Preserver Count {actionTimePreservers.Length}");

            foreach (ActionTimePreserver actionTimePreserver in actionTimePreservers)
            {
                // temp 
                if (actionTimePreserver.tagToSubjectify == "CoinPoint")
                    continue;

                if (actionTimePreserver.isActionTimePreservationComplete)
                {
                    UnityEngine.Console.Log("Already Preservation Completed");
                    continue;
                }
                

                actionTimePreserver.ResetVirtualDeltaPositonsOfSubjects();
                actionTimePreserver.UnchainSubjects();
                actionTimePreserver.ChainPreservedSubjectsTogether();

                UnityEngine.Console.Log($"Preserved Subjects List { actionTimePreserver.PreservedSubjects.Count}");

                foreach (ActionTimePreserverSubject actionTimePreserverSubject in actionTimePreserver.PreservedSubjects)
                {
                    ObjectDestruction[] preservedObjectDestructionComponents = actionTimePreserverSubject.GetComponentsInChildren<ObjectDestruction>();
                    SyncedObstacle[] preserverdSyncedObstacleComponents = actionTimePreserver.GetComponentsInChildren<SyncedObstacle>();
                    List<Transform> preservedObstacleTransforms = new List<Transform>();

                    foreach (ObjectDestruction preservedObjectDestructionComponent in preservedObjectDestructionComponents)
                    {
                        if (!preservedObstacleTransforms.Contains(preservedObjectDestructionComponent.transform.parent.transform))
                        {
                            preservedObstacleTransforms.Add(preservedObjectDestructionComponent.transform.parent.transform);
                        }
                    }

                    foreach (SyncedObstacle preserverdSyncedObstacleComponent in preserverdSyncedObstacleComponents)
                    {
                        if (!preservedObstacleTransforms.Contains(preserverdSyncedObstacleComponent.transform))
                        {
                            preservedObstacleTransforms.Add(preserverdSyncedObstacleComponent.transform);
                        }
                    }

                    UnityEngine.Console.Log($"Values {actionTimePreserver.preservationMultiplier} and {actionTimePreserver.calculatedObstacleSpacingMultiplier}");
                    float deltaDisToPreserveTime = actionTimePreserverSubject.GetDeltaDistanceToPreserveActionTime(actionTimePreserver.preservationMultiplier, actionTimePreserver.calculatedObstacleSpacingMultiplier);
                    UnityEngine.Console.Log("DeltaDistToPreserveTime " + deltaDisToPreserveTime);

                    actionTimePreserverSubject.Move(Vector3.forward * deltaDisToPreserveTime, true);

                    foreach (Transform preservedObstacleTransform in preservedObstacleTransforms)
                    {
                        obstaclesWithZPos[preservedObstacleTransform] = obstaclesWithZPos[preservedObstacleTransform] + actionTimePreserverSubject.virtualDeltaPos.z;
                    }
                }
            }

            foreach (float obstacleZPos in obstaclesWithZPos.Values)
            {
                if (obstacleZPos < minZPos)
                {
                    minZPos = obstacleZPos;
                }

                if (obstacleZPos > maxZPos)
                {
                    maxZPos = obstacleZPos;
                }
            }

            float encounterSize = maxZPos - minZPos;

            encounterSize += startExtentionDis;
            encounterSize += endExtentionDis;

            return encounterSize;
        }

        return 0;
    }

    private void CalculateEncounterOffset()
    {
        if (transform.childCount > 0)
        {
            float minZPos = Mathf.Infinity;

            List<Transform> obstacles = new List<Transform>();
            ObjectDestruction[] objectDestructionComponents = GetComponentsInChildren<ObjectDestruction>();
            SyncedObstacle[] syncedObstacleComponents = GetComponentsInChildren<SyncedObstacle>();

            foreach (ObjectDestruction objectDestructionComponent in objectDestructionComponents)
            {
                obstacles.Add(objectDestructionComponent.transform.parent.transform);
            }

            foreach (SyncedObstacle syncedObstacleComponent in syncedObstacleComponents)
            {
                obstacles.Add(syncedObstacleComponent.transform);
            }

            foreach (Transform obstacle in obstacles)
            {
                float obstacleParentZPos = obstacle.transform.position.z;

                if (obstacleParentZPos < minZPos)
                {
                    minZPos = obstacleParentZPos;
                }
            }

            encounterOffset = transform.position.z - minZPos;

            encounterOffset += startExtentionDis;
        }
        else
        {
            encounterOffset = 0;
        }
    }

    private void Update()
    {
     //   UnityEngine.Console.Log($"Custom encoutner update called");

        if (!Application.isPlaying)
        {
            if (IsSkeleton)
                return;

            ValidateAndInitialize();
            return;
        }
        else
        {
            float encounterEndPoint = transform.position.z + EncounterSize - (encounterOffset);
            float distFromPlayer = encounterEndPoint - playerSharedData.PlayerTransform.position.z;
            if (distFromPlayer <= -8f && !destructionInProcess)
            {
                StartCoroutine(SendCustomEncounterFinishedEventAfterDuration());
                destructionInProcess = true;
            }
        }
    }

    private IEnumerator SendCustomEncounterFinishedEventAfterDuration()
    {
        yield return new WaitForSeconds(3);

        CustomEncounterHasFinished?.Invoke(this);
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

        if (IsSkeleton)
            return;

        ValidateAndInitialize();
        GetReferencesInEditor();

        if (transform.root == transform && instanceID == 0)
        {
            instanceID = GetInstanceID();
        }
    }

#endif
}