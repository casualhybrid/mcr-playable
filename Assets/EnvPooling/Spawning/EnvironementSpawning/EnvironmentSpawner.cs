using System;
using System.Collections;
using System.Collections.Generic;
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

    [Header("References (keep as in original)")]
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private Transform[] cutSceneEnv;
    [SerializeField] private GameEvent cutSceneStarted;
    [Tooltip("Increase this to maintain more patches in front of player")]
    [SerializeField] private int minPatchesDistanceInFrontOfPlayer;
    [SerializeField] private int minForwardDistThresholdToInvokeSpawn;
    [SerializeField] private string nextEnvironmentKey;
    [SerializeField] private PlayerSharedData playerRunTimeData;
    [SerializeField] private EnvironmentsEnumSO environmentsEnumSO;
    [SerializeField] private EnvironmentChannel environmentChannel;
    [SerializeField] private EnviornmentSO tempSO;
    [SerializeField] private TutorialTypesData tutorialTypesData;
    [SerializeField] private EnvPatchPoolSO envPatchPoolSO;
    [SerializeField] private EnvFactorySO envFactorySO;
    [SerializeField] private EnvironmentData _environmentData;
    [SerializeField] private int initialCopiesPerPatch = 1;
    [SerializeField] private EnvCategory envCategory;
    [SerializeField] private GameObject newEnvEnteredGameObject;

    [Header("Performance / Tuning")]
    [Tooltip("Delay (seconds) between spawning individual patches. Small value (0.01 - 0.03) smooths spikes.")]
    [SerializeField] private float spawnDelay = 0.01f;
    [Tooltip("Max number of patches spawned in a single iteration of the spawn loop. Helps bound CPU cost.")]
    [SerializeField] private int maxPatchesPerIteration = 2;
    [Tooltip("If true, Resources.UnloadUnusedAssets() will run when destroying previous env (not recommended).")]
    [SerializeField] private bool callUnloadUnusedAssetsOnDestroy = false;

    public event Action<List<Patch>> batchOfEnvironmentSpawned;

    // ----- Internal runtime state (same semantics as your original script) -----
    private readonly Dictionary<Patch, float> suspendedPatches = new Dictionary<Patch, float>(); // patch -> playerZ_when_suspended
    private Patch lastSpawnedPatch;
    private float zDistanceWhereLastPatchEnded;
    private float nextPatchStarting = 0f;
    public static EnviornmentSO currentlyActiveEnv { get; private set; }
    public static EnviornmentSO previouslyActiveEnv { get; private set; }

    public bool ShoudNotOffsetOnRest { get; set; } = true;

    private EnvCategory currentlyActiveCategory;
    private bool initialized;
    private bool isAmbianceChanged = false;
    private GameObject rootEnvObject;
    private Patch currentEndPatch; // last spawned in batch
    private Patch tutorialPatch;
    private EnviornmentSO tutorialSO;
    private Queue<NewEnvironmentInfo> NewEnvironmentInfoQueue = new Queue<NewEnvironmentInfo>();

    // Reused collections to avoid allocations during spawning
    private readonly List<Patch> tmpReducedPatches = new List<Patch>(16);
    private readonly List<Patch> spawnedBatch = new List<Patch>(8);
    private readonly List<NewEnvironmentInfo> tmpEnvInfoList = new List<NewEnvironmentInfo>(4);

    // Single persistent spawn coroutine
    private Coroutine spawnCoroutine;
    private bool spawnLoopRequested = false; // trigger to run spawn loop once

    private void Awake()
    {
        environmentChannel.OnPlayerEnvironmentChanged += DestroyPreviousEnvironment;
        environmentChannel.OnPlayerEnvironmentChanged += CheckAndSpawnPendingNewEnvTriggers;
        cutSceneStarted.TheEvent.AddListener(HandleCutSceneStarted);

        rootEnvObject = new GameObject("rootEnvObject");
        rootEnvObject.transform.SetParent(this.transform, false);

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

        currentlyActiveEnv = saveManager.MainSaveFile.TutorialHasCompleted ? tempSO : tutorialSO;
        envPatchPoolSO.SetParent(rootEnvObject.transform);

        // Spread initial spawns across frames to avoid first-frame spike
        yield return null;

        // Spawn 2 patches at start (same as original requirement)
        for (int i = 0; i < 2; i++)
        {
            // direct spawn request using same coroutine path
            RequestSpawnLoop(); // set flag
            yield return null; // let spawn coroutine run on next frame
        }

        // Prepare tutorial patch if needed (same behavior)
        if (!saveManager.MainSaveFile.TutorialHasCompleted && tutorialSO != null)
        {
            Patch tutorialOriginalPatch = tutorialSO.GetMergedPatchesCollectionIrrelevantToCategories()[0];
            tutorialPatch = envPatchPoolSO.Request(tutorialOriginalPatch, tutorialOriginalPatch.InstanceID);
            if (tutorialPatch != null)
            {
                var inst = tutorialPatch.GetComponent<InstantiateSubPrefabsInMultipleFrames>();
                if (inst != null)
                    yield return inst.SpawnSubPrefabs();
            }
        }

        initialized = true;

        // start the persistent spawn coroutine (it will sleep until requested)
        spawnCoroutine = StartCoroutine(SpawnLoopCoroutine());
    }

    private void HandleCutSceneStarted(GameEvent gameEvent)
    {
        if (cutSceneEnv == null || cutSceneEnv.Length == 0) return;

        Transform farthest = cutSceneEnv[0];
        for (int i = 1; i < cutSceneEnv.Length; i++)
        {
            if (cutSceneEnv[i].position.z > farthest.position.z) farthest = cutSceneEnv[i];
        }

        BoxCollider boxCollider = farthest.GetComponent<BoxCollider>();
        float startingZ = farthest.position.z + boxCollider.bounds.size.z / 2f;
        zDistanceWhereLastPatchEnded = startingZ;

        if (!saveManager.MainSaveFile.TutorialHasCompleted)
        {
            SpawnOnePatchOnly();
            currentlyActiveEnv = tempSO;
        }

        playerRunTimeData.playerCurrentEnvironment = currentlyActiveEnv.GetEnvironmentType;
        initialized = true;

        tempSO = null;
        tutorialSO = null;
    }

    private void Update()
    {
        if (!initialized) return;

        if (!isAmbianceChanged && playerRunTimeData.PlayerTransform.position.z > nextPatchStarting)
            isAmbianceChanged = true;

        float zDiff = zDistanceWhereLastPatchEnded - playerRunTimeData.PlayerTransform.position.z;

        // If player is approaching the end distance, signal the spawn loop to run
        if (zDiff < minForwardDistThresholdToInvokeSpawn)
            RequestSpawnLoop();
    }

    // Mark that we want the spawn loop to run (safe & cheap)
    private void RequestSpawnLoop()
    {
        spawnLoopRequested = true;
    }

    public void SpawnOnePatchOnly(bool isSafeZonePatch = false)
    {
        // request spawn for a single patch; spawn loop will handle singlePatchOnly param
        StartCoroutine(SpawnPatchesInFrontOfPlayer(currentlyActiveEnv, true, -1f, -1f, true, isSafeZonePatch));
    }

    public Coroutine SpawnSpecificDistancePatches(float lengthToSpawnTill)
    {
        return StartCoroutine(SpawnPatchesInFrontOfPlayer(currentlyActiveEnv, false, zDistanceWhereLastPatchEnded, lengthToSpawnTill));
    }

    // Persistent loop that only runs when spawnLoopRequested is true.
    // This avoids starting many coroutines from Update.
    private IEnumerator SpawnLoopCoroutine()
    {
        while (true)
        {
            if (!spawnLoopRequested)
            {
                // sleep a frame; trivial cost
                yield return null;
                continue;
            }

            // Reset the flag; SpawnPatchesInFrontOfPlayer will set it again if more spawning is needed
            spawnLoopRequested = false;

            // Run the actual spawn routine (non-reentrant)
            yield return StartCoroutine(SpawnPatchesInFrontOfPlayer(currentlyActiveEnv, false, -1f, -1f, false, false));
        }
    }

    // The main spawner: reused lists, limited per-iteration work, minimal allocations.
    private IEnumerator SpawnPatchesInFrontOfPlayer(EnviornmentSO enviornmentSO,
        bool singlePatchOnly = false,
        float playerPosiitonOverride = -1f,
        float lengthToSpawnTill = -1f,
        bool dontRaiseSpawnEvent = false,
        bool isSafeZonePatch = false)
    {
        if (enviornmentSO == null)
            yield break;

        float zPosToMeasureFrom = (playerPosiitonOverride == -1f) ? playerRunTimeData.PlayerTransform.position.z : playerPosiitonOverride;
        float distanceToSpawnTill = (lengthToSpawnTill == -1f) ? minPatchesDistanceInFrontOfPlayer : lengthToSpawnTill;

        spawnedBatch.Clear();

        // Keep spawning while we need patches in front
        while ((zDistanceWhereLastPatchEnded - zPosToMeasureFrom) < distanceToSpawnTill || singlePatchOnly)
        {
            // get possible patches reference (no copy)
            List<Patch> possiblePatches = (currentlyActiveCategory == null)
                ? enviornmentSO.GetMergedPatchesCollectionIrrelevantToCategories()
                : enviornmentSO.GetCollectionOfPossiblePatchesForCategory(currentlyActiveCategory);

            if (possiblePatches == null || possiblePatches.Count == 0)
                break;

            // Clean expired suspended patches using snapshot to avoid modifying dictionary while iterating
            if (suspendedPatches.Count > 0)
            {
                Patch[] suspendedKeys = new Patch[suspendedPatches.Count];
                suspendedPatches.Keys.CopyTo(suspendedKeys, 0);
                for (int si = 0; si < suspendedKeys.Length; si++)
                {
                    Patch pkey = suspendedKeys[si];
                    float storedZ = suspendedPatches[pkey];
                    if (playerRunTimeData.PlayerTransform.position.z - storedZ >= pkey.DistanceBeforeItCanReAppear)
                    {
                        suspendedPatches.Remove(pkey);
                    }
                }
            }

            // Build reduced list without allocations (reuse tmpReducedPatches)
            tmpReducedPatches.Clear();
            for (int i = 0; i < possiblePatches.Count; i++)
            {
                Patch p = possiblePatches[i];
                if (!suspendedPatches.ContainsKey(p))
                    tmpReducedPatches.Add(p);
            }

            if (tmpReducedPatches.Count == 0)
                break;

            // Pick random patch from reduced list
            int randIndex = UnityEngine.Random.Range(0, tmpReducedPatches.Count);
            Patch originalPatch = tmpReducedPatches[randIndex];

            // Request from pool or tutorial handling
            Patch patchRequested;
            if (enviornmentSO == tutorialSO)
            {
                patchRequested = tutorialPatch;
                if (patchRequested == null) break;

                patchRequested.gameObject.SetActive(true);
                patchRequested.PatchHasFinished += HandleTutorialPatchHasFinished;
            }
            else
            {
                patchRequested = envPatchPoolSO.Request(originalPatch, originalPatch.InstanceID);
                if (patchRequested == null) break;
                patchRequested.PatchHasFinished += HandlePatchHasFinished;
            }

            // Ensure proper parent (avoid repeated SetParent if already correct)
            if (patchRequested.transform.parent != rootEnvObject.transform)
                patchRequested.transform.SetParent(rootEnvObject.transform, false);

            // Place patch using precomputed diffs (cheap Transform ops)
            Vector3 patchNewPosition = new Vector3(originalPatch.transform.position.x,
                                                   originalPatch.transform.position.y,
                                                   zDistanceWhereLastPatchEnded - patchRequested.DiffBWPivotAndColliderMinBoundPoint);
            patchRequested.transform.localPosition = patchNewPosition;

            lastSpawnedPatch = patchRequested;
            zDistanceWhereLastPatchEnded = patchNewPosition.z + patchRequested.DiffBWPivotAndColliderMaxBoundPoint;

            _environmentData.distanceCoveredByActiveEnvironment += patchRequested.GetLengthOfPatch;

            if (patchRequested.isLimitedSpawningByDistance)
                suspendedPatches[originalPatch] = playerRunTimeData.PlayerTransform.position.z;

            spawnedBatch.Add(patchRequested);

            if (isSafeZonePatch)
            {
                _environmentData.nextEnvSwitchPatchZPosition = patchNewPosition.z;
                _environmentData.firstRampBuildingHasBeenSpawned = true;
            }

            // if only one patch requested, break
            if (singlePatchOnly)
                break;

            // Bound patches per iteration to avoid long single-frame work
            int spawnedThisIteration = 1;
            while (spawnedThisIteration < maxPatchesPerIteration &&
                   (zDistanceWhereLastPatchEnded - zPosToMeasureFrom) < distanceToSpawnTill)
            {
                // Try spawn more within same iteration using same logic
                // reuse tmpReducedPatches: rebuild reduced list (fast) and spawn up to maxPatchesPerIteration
                tmpReducedPatches.Clear();
                for (int i = 0; i < possiblePatches.Count; i++)
                {
                    Patch p = possiblePatches[i];
                    if (!suspendedPatches.ContainsKey(p))
                        tmpReducedPatches.Add(p);
                }
                if (tmpReducedPatches.Count == 0) break;

                randIndex = UnityEngine.Random.Range(0, tmpReducedPatches.Count);
                originalPatch = tmpReducedPatches[randIndex];

                Patch nextPatch = envPatchPoolSO.Request(originalPatch, originalPatch.InstanceID);
                if (nextPatch == null) break;
                nextPatch.PatchHasFinished += HandlePatchHasFinished;

                if (nextPatch.transform.parent != rootEnvObject.transform)
                    nextPatch.transform.SetParent(rootEnvObject.transform, false);

                Vector3 nextPos = new Vector3(originalPatch.transform.position.x,
                                              originalPatch.transform.position.y,
                                              zDistanceWhereLastPatchEnded - nextPatch.DiffBWPivotAndColliderMinBoundPoint);
                nextPatch.transform.localPosition = nextPos;

                lastSpawnedPatch = nextPatch;
                zDistanceWhereLastPatchEnded = nextPos.z + nextPatch.DiffBWPivotAndColliderMaxBoundPoint;
                _environmentData.distanceCoveredByActiveEnvironment += nextPatch.GetLengthOfPatch;

                if (nextPatch.isLimitedSpawningByDistance)
                    suspendedPatches[originalPatch] = playerRunTimeData.PlayerTransform.position.z;

                spawnedBatch.Add(nextPatch);
                spawnedThisIteration++;
            }

            // After spawning up to maxPatchesPerIteration, yield according to spawnDelay to spread load
            if (spawnDelay > 0f)
                yield return new WaitForSeconds(spawnDelay);
            else
                yield return null;

            // Loop condition will be re-evaluated
        }

        // Notify listeners if any
        if (!dontRaiseSpawnEvent && spawnedBatch.Count > 0)
            batchOfEnvironmentSpawned?.Invoke(spawnedBatch);

        if (spawnedBatch.Count > 0)
            currentEndPatch = spawnedBatch[spawnedBatch.Count - 1];

        // If after this run the distance still needs patches, request the spawn loop again
        float currentZPos = playerRunTimeData.PlayerTransform.position.z;
        if ((zDistanceWhereLastPatchEnded - currentZPos) < minPatchesDistanceInFrontOfPlayer)
        {
            // schedule another iteration of the spawn loop
            spawnLoopRequested = true;
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
        Destroy(patch.gameObject, 2); // as originally requested
    }

    public void ChangeEnvCategory(EnvCategory envCategory)
    {
        currentlyActiveCategory = envCategory;
    }

    public void ChangeActiveEnv(EnviornmentSO enviornmentSO, bool markStartOfNewEnv = false)
    {
        _environmentData.distanceCoveredByActiveEnvironment = 0;
        previouslyActiveEnv = currentlyActiveEnv != null && currentlyActiveEnv.GetEnvironmentType != environmentsEnumSO.GeneralSwitchEnvironment ? currentlyActiveEnv : previouslyActiveEnv;
        currentlyActiveEnv = enviornmentSO;
        nextPatchStarting = zDistanceWhereLastPatchEnded;
        isAmbianceChanged = false;

        if (markStartOfNewEnv)
        {
            Vector3 pos = new Vector3(0, 0, nextPatchStarting);
            NewEnvironmentInfoQueue.Enqueue(new NewEnvironmentInfo(pos, enviornmentSO.GetEnvironmentType));
            if (NewEnvironmentInfoQueue.Count == 1)
                SpawnNewEnvTrigger(pos, enviornmentSO.GetEnvironmentType);
        }
    }

    private void CheckAndSpawnPendingNewEnvTriggers(Environment env)
    {
        if (NewEnvironmentInfoQueue.Count == 0) return;
        NewEnvironmentInfoQueue.Dequeue();
        if (NewEnvironmentInfoQueue.Count == 0) return;
        NewEnvironmentInfo newEnvironmentInfo = NewEnvironmentInfoQueue.Peek();
        SpawnNewEnvTrigger(newEnvironmentInfo.Position, newEnvironmentInfo.Environment);
    }

    private void SpawnNewEnvTrigger(Vector3 pos, Environment enviornment)
    {
        if (newEnvEnteredGameObject == null) return;
        newEnvEnteredGameObject.transform.position = pos;
        var trigger = newEnvEnteredGameObject.GetComponent<NewEnvironmentEnteredTrigger>();
        if (trigger != null) trigger.enviornment = enviornment;
        newEnvEnteredGameObject.SetActive(true);
    }

    private void DestroyPreviousEnvironment(Environment environment)
    {
        StartCoroutine(DestroyPreviousEnvironmentRoutine());
    }

    private IEnumerator DestroyPreviousEnvironmentRoutine()
    {
        yield return new WaitForSeconds(3f);

        if (previouslyActiveEnv == null) yield break;

        List<Patch> allPatches = previouslyActiveEnv.GetMergedPatchesCollectionIrrelevantToCategories();
        for (int i = 0; i < allPatches.Count; i++)
        {
            envPatchPoolSO.DestroyPooledObjectsForID(allPatches[i].InstanceID);
            // spread destruction across frames to avoid spikes
            yield return null;
        }

        if (callUnloadUnusedAssetsOnDestroy)
        {
            yield return Resources.UnloadUnusedAssets();
        }
    }

    public void OnBeforeFloatingPointReset() { }

    public void OnFloatingPointReset(float movedOffset)
    {
        if (suspendedPatches.Count > 0)
        {
            Patch[] keys = new Patch[suspendedPatches.Count];
            suspendedPatches.Keys.CopyTo(keys, 0);
            for (int i = 0; i < keys.Length; i++)
            {
                Patch k = keys[i];
                suspendedPatches[k] = suspendedPatches[k] - movedOffset;
            }
        }

        if (newEnvEnteredGameObject != null)
            newEnvEnteredGameObject.transform.position -= new Vector3(0, 0, movedOffset);

        if (NewEnvironmentInfoQueue.Count > 0)
        {
            tmpEnvInfoList.Clear();
            foreach (var item in NewEnvironmentInfoQueue)
            {
                tmpEnvInfoList.Add(new NewEnvironmentInfo(new Vector3(item.Position.x, item.Position.y, item.Position.z - movedOffset), item.Environment));
            }

            NewEnvironmentInfoQueue.Clear();
            for (int i = 0; i < tmpEnvInfoList.Count; i++)
                NewEnvironmentInfoQueue.Enqueue(tmpEnvInfoList[i]);
        }

        zDistanceWhereLastPatchEnded -= movedOffset;
        nextPatchStarting -= movedOffset;
        _environmentData.nextEnvSwitchPatchZPosition -= movedOffset;
    }
}
