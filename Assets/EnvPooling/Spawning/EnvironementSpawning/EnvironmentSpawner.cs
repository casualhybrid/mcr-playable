using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TheKnights.SaveFileSystem;
using UnityEngine;

public class EnvironmentSpawner : MonoBehaviour, IFloatingReset
{
    private struct NewEnvironmentInfo
    {
        public Vector3 Position { readonly get; set; }
        public readonly Environment Environment { get; }

        public NewEnvironmentInfo(Vector3 position, Environment environment)
        {
            Position = position;
            Environment = environment;
        }
    }


    [SerializeField] private SaveManager saveManager;
    [SerializeField] private Transform[] cutSceneEnv;
    [SerializeField] private GameEvent cutSceneStarted;

    [Tooltip("Increase this to maintain more patches in front of player")]
    [SerializeField] private int minPatchesDistanceInFrontOfPlayer;

    [SerializeField] private int minForwardDistThresholdToInvokeSpawn;

    [SerializeField] private string nextEnvironmentKey;

    [SerializeField] private PlayerSharedData playerRunTimeData;

    //  [SerializeField] private GameEvent initialEnvHasSpawned;

    [SerializeField] private EnvironmentsEnumSO environmentsEnumSO;
    [SerializeField] private EnvironmentChannel environmentChannel;
    [SerializeField] private EnviornmentSO tempSO;

    // Tutorial Types Data
    [SerializeField] private TutorialTypesData tutorialTypesData;

    [SerializeField] private EnvPatchPoolSO envPatchPoolSO;
    [SerializeField] private EnvFactorySO envFactorySO;

    [SerializeField] private EnvironmentData _environmentData;

    [SerializeField] private int initialCopiesPerPatch = 1;

    [SerializeField] private EnvCategory envCategory;

    [SerializeField] private GameObject newEnvEnteredGameObject;

    public event Action<List<Patch>> batchOfEnvironmentSpawned;

    private Dictionary<Patch, float> suspendedPatches = new Dictionary<Patch, float>();

    private Patch lastSpawnedPatch;
    private float zDistanceWhereLastPatchEnded, nextPatchStarting = 0f;
    public static EnviornmentSO currentlyActiveEnv { get; private set; }
    public static EnviornmentSO previouslyActiveEnv { get; private set; }

    public bool ShoudNotOffsetOnRest { get; set; } = true;

    private EnvCategory currentlyActiveCategory;

    private bool initialized, isAmbianceChanged = false;
    private GameObject rootEnvObject;
    private Patch currentEndPatch; // The patch at the very end
    private Patch tutorialPatch;

    private EnviornmentSO tutorialSO;
    private Queue<NewEnvironmentInfo> NewEnvironmentInfoQueue = new Queue<NewEnvironmentInfo>();

    private void Awake()
    {
        environmentChannel.OnPlayerEnvironmentChanged += DestroyPreviousEnvironment;
        environmentChannel.OnPlayerEnvironmentChanged += CheckAndSpawnPendingNewEnvTriggers;
        cutSceneStarted.TheEvent.AddListener(HandleCutSceneStarted);

        rootEnvObject = new GameObject("rootEnvObject");
        rootEnvObject.transform.SetParent(this.transform);
        // lastSpawnedPatch = GetComponentInChildren<Patch>();
        // zDistanceWhereLastPatchEnded += (lastSpawnedPatch.transform.position.z + lastSpawnedPatch.GetLengthOfPatch);
        ChangeEnvCategory(envCategory);

        
    }

    private void OnDestroy()
    {
        environmentChannel.OnPlayerEnvironmentChanged -= DestroyPreviousEnvironment;
        environmentChannel.OnPlayerEnvironmentChanged -= CheckAndSpawnPendingNewEnvTriggers;
        cutSceneStarted.TheEvent.RemoveListener(HandleCutSceneStarted);
    }

    private IEnumerator Start()
    {
        tutorialSO = tutorialTypesData.GetTutorialInstanceData(TutorialManager.ActiveTutorialType).TutorialEnvironment;

        #region AddressablesInstantiation

        //var operationHandle = AddressableLoader.LoadTheAsset<EnviornmentSO>(nextEnvironmentKey);
        //operationHandle.Completed += (handle) =>
        //{
        //    EnviornmentSO enviornmentSO = handle.Result;
        //    currentlyActiveEnv = enviornmentSO;
        //    SpawnPatchesInFrontOfPlayer(enviornmentSO);
        //    initialized = true;
        //    initialEnvHasSpawned.RaiseEvent();
        //};

        #endregion AddressablesInstantiation


        currentlyActiveEnv = saveManager.MainSaveFile.TutorialHasCompleted ? tempSO : tutorialSO;
        envPatchPoolSO.SetParent(rootEnvObject.transform);

       // envFactorySO.AddEnvironmentForWarmup(tempSO);
    //    envPatchPoolSO.Prewarm(initialCopiesPerPatch);

        yield return null;

        if (!saveManager.MainSaveFile.TutorialHasCompleted)
        {
         //   UnityEngine.Profiling.Profiler.BeginSample("TutorialGenerated");
            // Cache tutorial patch
            Patch tutorialOriginalPatch = tutorialSO.GetMergedPatchesCollectionIrrelevantToCategories()[0];
            tutorialPatch = envPatchPoolSO.Request(tutorialOriginalPatch, tutorialOriginalPatch.InstanceID);
            yield return tutorialPatch.GetComponent<InstantiateSubPrefabsInMultipleFrames>().SpawnSubPrefabs();
            //   tutorialPatch.gameObject.SetActive(false);
           // UnityEngine.Profiling.Profiler.EndSample();

           // Debug.Break();
        }
    }

    private void HandleCutSceneStarted(GameEvent gameEvent)
    {
        Transform[] array = cutSceneEnv.OrderByDescending(x => x.transform.position.z).ToArray();

        Transform env = array[0];

        BoxCollider boxCollider = env.GetComponent<BoxCollider>();

        float startingZ = env.position.z + boxCollider.bounds.size.z / 2f;

        zDistanceWhereLastPatchEnded = startingZ;

        //lastSpawnedPatch.transform.position = new Vector3(lastSpawnedPatch.transform.position.x, lastSpawnedPatch.transform.position.y, zDistanceWhereLastPatchEnded);

        //  zDistanceWhereLastPatchEnded = (lastSpawnedPatch.transform.position.z + lastSpawnedPatch.GetLengthOfPatch);

        if (!saveManager.MainSaveFile.TutorialHasCompleted)
        {
            SpawnOnePatchOnly();

            // Switch back from tutorial environment
            currentlyActiveEnv = tempSO;
        }

        playerRunTimeData.playerCurrentEnvironment = currentlyActiveEnv.GetEnvironmentType;

        // StartCoroutine(SpawnPatchesInFrontOfPlayer(tempSO));
        initialized = true;

        // Null References To Clean Memory La[ter
        tempSO = null;
        tutorialSO = null;
    }

    private void Update()
    {
        if (!initialized)
            return;

        if (!isAmbianceChanged)
        {
            if (playerRunTimeData.PlayerTransform.position.z > nextPatchStarting)
            {
                //  currentlyActiveEnv.typeOfEnviorment.RaiseEvent();
                isAmbianceChanged = true;
                //    UnityEngine.Console.Log($"Ambiance Changed to {currentlyActiveEnv.name}");
            }
        }

        float zDiff = zDistanceWhereLastPatchEnded - playerRunTimeData.PlayerTransform.position.z;

        if (zDiff < minForwardDistThresholdToInvokeSpawn)
        {
            StartCoroutine(SpawnPatchesInFrontOfPlayer(currentlyActiveEnv));
        }
    }

    public void SpawnOnePatchOnly(bool isSafeZonePatch = false)
    {
        StartCoroutine(SpawnPatchesInFrontOfPlayer(currentlyActiveEnv, true, -1, -1, true, isSafeZonePatch));
    }

    public Coroutine SpawnSpecificDistancePatches(float lengthToSpawnTill)
    {
        return StartCoroutine(SpawnPatchesInFrontOfPlayer(currentlyActiveEnv, false, zDistanceWhereLastPatchEnded, lengthToSpawnTill));
    }

    private IEnumerator SpawnPatchesInFrontOfPlayer(EnviornmentSO enviornmentSO, bool singlePatchOnly = false, float playerPosiitonOverride = -1f, float lengthToSpawnTill = -1f, bool dontRaiseSpawnEvent = false, bool isSafeZonePatch = false)
    {
       
        float zPosToMeasureFrom = playerPosiitonOverride == -1 ? playerRunTimeData.PlayerTransform.position.z : playerPosiitonOverride;
        float distBetweenLastPatchZandPlayer = zDistanceWhereLastPatchEnded - zPosToMeasureFrom;
        float distanceToSpawnTill = lengthToSpawnTill == -1 ? minPatchesDistanceInFrontOfPlayer : lengthToSpawnTill;

        List<Patch> spawnewdBatch = new List<Patch>();

        while (distBetweenLastPatchZandPlayer < distanceToSpawnTill || singlePatchOnly)
        {
            distBetweenLastPatchZandPlayer = zDistanceWhereLastPatchEnded - zPosToMeasureFrom;

            List<Patch> possiblePatches;

            if (currentlyActiveCategory == null)
            {
                possiblePatches = enviornmentSO.GetMergedPatchesCollectionIrrelevantToCategories();
            }
            else
            {
                possiblePatches = enviornmentSO.GetCollectionOfPossiblePatchesForCategory(currentlyActiveCategory);
            }

            List<Patch> reducedPossiblePatches;

            if (suspendedPatches.Count > 0)
            {
                List<Patch> keysList = suspendedPatches.Keys.ToList();

                for (int i = 0; i < keysList.Count; i++)
                {
                    Patch p = keysList[i];
                    float diff = playerRunTimeData.PlayerTransform.position.z - suspendedPatches[p];

                    if (diff >= p.DistanceBeforeItCanReAppear)
                    {
                        UnityEngine.Console.Log($"Removed from suspended patches {p.name}");
                        suspendedPatches.Remove(p);
                    }
                }
            }

            if (suspendedPatches.Count > 0)
            {
                reducedPossiblePatches = new List<Patch>();
                for (int i = 0; i < possiblePatches.Count; i++)
                {
                    Patch p = possiblePatches[i];
                    if (!suspendedPatches.ContainsKey(p))
                    {
                        reducedPossiblePatches.Add(p);
                    }
                }
            }
            else
            {
                reducedPossiblePatches = possiblePatches;
            }

            int rand = UnityEngine.Random.Range(0, reducedPossiblePatches.Count);
            Patch originalPatch = reducedPossiblePatches[rand];
            GameObject originalPatchObject = originalPatch.gameObject;
            Patch patchRequested;
            // Is this tutorial env
            if (enviornmentSO == tutorialSO)
            {
                patchRequested = tutorialPatch;
                patchRequested.gameObject.SetActive(true);
                patchRequested.PatchHasFinished += HandleTutorialPatchHasFinished;
            }
            else
            {
                patchRequested = envPatchPoolSO.Request(originalPatch, originalPatch.InstanceID);
                patchRequested.PatchHasFinished += HandlePatchHasFinished;
            }

            GameObject patchRequestedGameObject = patchRequested.gameObject;

            //    UnityEngine.Console.Log($"Spawning Patch {patchRequestedGameObject} at position {zDistanceWhereLastPatchEnded} and name {patchRequestedGameObject.name}");
            Vector3 patchNewPosition = new Vector3(originalPatchObject.transform.position.x, originalPatchObject.transform.position.y, zDistanceWhereLastPatchEnded - patchRequested.DiffBWPivotAndColliderMinBoundPoint);
          //  UnityEngine.Console.Log("Spawning Patch at " + patchNewPosition.z);
            patchRequestedGameObject.transform.localPosition = patchNewPosition;

            lastSpawnedPatch = patchRequested;

            //  zDistanceWhereLastPatchEnded += patchRequested.GetLengthOfPatch;

         //   UnityEngine.Console.Log("DIFF POINT IS " + patchRequested.DiffBWPivotAndColliderMaxBoundPoint);
            zDistanceWhereLastPatchEnded = patchNewPosition.z + patchRequested.DiffBWPivotAndColliderMaxBoundPoint;

            // zDistanceWhereLastPatchEnded = patchRequested.EndingZPoint;

            _environmentData.distanceCoveredByActiveEnvironment += patchRequested.GetLengthOfPatch;

            //  UnityEngine.Console.Log("Will spawn next patch at " + zDistanceWhereLastPatchEnded);

            if (patchRequested.isLimitedSpawningByDistance)
            {
                //   UnityEngine.Console.Log($"Added to suspended patches {patchRequested.name}");
                suspendedPatches.Add(originalPatch, playerRunTimeData.PlayerTransform.position.z);
            }

            spawnewdBatch.Add(patchRequested);

            if (isSafeZonePatch)
            {
                _environmentData.nextEnvSwitchPatchZPosition = patchNewPosition.z;
                _environmentData.firstRampBuildingHasBeenSpawned = true;
            }

            if (singlePatchOnly)
            {
                break;
            }

            yield return null;

    

        }

   

        if (!dontRaiseSpawnEvent)
        {
            // UnityEngine.Console.Log("SpawningBatchPatchEvent");

            batchOfEnvironmentSpawned?.Invoke(spawnewdBatch);

            //enviornmentSO.typeOfEnviorment.RaiseEvent();
        }

        if (spawnewdBatch.Count > 0)
        {
            // Last Patch
            currentEndPatch = spawnewdBatch.Last();
        }
    }

    private void HandlePatchHasFinished(Patch patch)
    {
        patch.PatchHasFinished -= HandlePatchHasFinished;
        envPatchPoolSO.Return(patch);
    }

    private void HandleTutorialPatchHasFinished(Patch patch)
    {
        patch.PatchHasFinished -= HandleTutorialPatchHasFinished;
        Destroy(patch.gameObject, 2);
    }

    public void ChangeEnvCategory(EnvCategory envCategory)
    {
        currentlyActiveCategory = envCategory;
    }

    public void ChangeActiveEnv(EnviornmentSO enviornmentSO, bool markStartOfNewEnv = false)
    {
        _environmentData.distanceCoveredByActiveEnvironment = 0;
        previouslyActiveEnv = currentlyActiveEnv.GetEnvironmentType != environmentsEnumSO.GeneralSwitchEnvironment ? currentlyActiveEnv : previouslyActiveEnv;
        currentlyActiveEnv = enviornmentSO;
        nextPatchStarting = zDistanceWhereLastPatchEnded;
        isAmbianceChanged = false;

        // With the assumption that atleast one patch on the changed env will spawn :)
        if (markStartOfNewEnv)
        {
            //  UnityEngine.Console.Log("Enabled and positioned new env trigger for env: " + enviornmentSO.name);

            Vector3 pos = new Vector3(0, 0, nextPatchStarting);

            NewEnvironmentInfo newEnvironmentInfo = new NewEnvironmentInfo(pos, enviornmentSO.GetEnvironmentType);
            NewEnvironmentInfoQueue.Enqueue(newEnvironmentInfo);

            if (NewEnvironmentInfoQueue.Count == 1)
            {
                SpawnNewEnvTrigger(pos, enviornmentSO.GetEnvironmentType);
            }
        }
    }

    private void CheckAndSpawnPendingNewEnvTriggers(Environment env)
    {
        NewEnvironmentInfoQueue.Dequeue();

        if (NewEnvironmentInfoQueue.Count == 0)
            return;

        NewEnvironmentInfo newEnvironmentInfo = NewEnvironmentInfoQueue.Peek();
        SpawnNewEnvTrigger(newEnvironmentInfo.Position, newEnvironmentInfo.Environment);
    }

    private void SpawnNewEnvTrigger(Vector3 pos, Environment enviornment)
    {
        newEnvEnteredGameObject.transform.position = pos;
        newEnvEnteredGameObject.GetComponent<NewEnvironmentEnteredTrigger>().enviornment = enviornment;
        newEnvEnteredGameObject.SetActive(true);
    }

    private void DestroyPreviousEnvironment(Environment environment)
    {
      //  UnityEngine.Console.Log("Environment Changed");

        StartCoroutine(DestroyPreviousEnvironmentRoutine());
    }

    private IEnumerator DestroyPreviousEnvironmentRoutine()
    {
        yield return new WaitForSeconds(3);

       List<Patch> allPatches = previouslyActiveEnv.GetMergedPatchesCollectionIrrelevantToCategories();

        for (int i = 0; i < allPatches.Count; i++)
        {
        //    UnityEngine.Console.Log("Request To Destroy Patch " + allPatches[i]);
            envPatchPoolSO.DestroyPooledObjectsForID(allPatches[i].InstanceID);

            yield return null;
        }

        yield return null;

        Resources.UnloadUnusedAssets();

    }

    public void OnBeforeFloatingPointReset()
    {

    }


    public void OnFloatingPointReset(float movedOffset)
    {
        foreach (var item in suspendedPatches)
        {
            suspendedPatches[item.Key] = item.Value - movedOffset;

        }

        Transform newEnvT = newEnvEnteredGameObject.transform;
        Vector3 newEnvObjectPos = newEnvT.position;
        newEnvObjectPos.z -= movedOffset;
        newEnvT.position = newEnvObjectPos;


        if (NewEnvironmentInfoQueue.Count != 0)
        {

            Queue<NewEnvironmentInfo> tempQueue = new Queue<NewEnvironmentInfo>();

            foreach (NewEnvironmentInfo item in NewEnvironmentInfoQueue)
            {
                Vector3 newPos = item.Position;
                newPos.z -= movedOffset;

                tempQueue.Enqueue(new NewEnvironmentInfo(newPos, item.Environment));
            }

            NewEnvironmentInfoQueue = tempQueue;
        }

        zDistanceWhereLastPatchEnded -= movedOffset;
        nextPatchStarting -= movedOffset;
        _environmentData.nextEnvSwitchPatchZPosition -= movedOffset;
    }
}