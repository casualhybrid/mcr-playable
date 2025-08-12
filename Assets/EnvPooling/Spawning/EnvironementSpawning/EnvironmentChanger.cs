using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnvironmentSpawner), typeof(ProceduralCoinGenerator), typeof(ObstaclesSpawning))]
public class EnvironmentChanger : MonoBehaviour
{
    [SerializeField] private EnvironmentSwitchingOrder environmentSwitchingOrder;
    [SerializeField] private EnvironmentAssetsLoader environmentAssetsLoader;
    [SerializeField] private EnviornmentSO switchEnvGeneral;
    [SerializeField] private EnvCategory envCategory;
    [SerializeField] private EnvCategory envChangeCategory;
    [SerializeField] private EnvironmentData environmentData;
    [SerializeField] private EnvironmentChannel environmentChannel;
    [SerializeField] private SpecialPickupsEnumSO specialPickupsEnumSO;
    [SerializeField] private Objectset spawnedSpecialPickupsSet;

    [SerializeField] private GameEvent playerStartedFlying;
    [SerializeField] private GameEvent playerEndedFlying;
    [SerializeField] private EnvFactorySO envFactorySO;
    [SerializeField] private EnvPatchPoolSO envPatchPoolSO;

    private EnvironmentSpawner environmentSpawner;
    //private ProceduralCoinGenerator proceduralCoinGenerator;
    //private ObstaclesSpawning obstaclesSpawning;

    private bool ignoreBatchSpawnEvent = false;
    private int currentEnvIndex = 0;
    private int currentEnvDistIndex = 0;

    private bool pauseSafeZoneGeneration = false;
    private bool isUnPauseSafeZoneGenerationOverriden; // Global Lock
    private bool isLoadingEnvAssets;

    private int currentPausedSwitchEnvGenNumber;

    private void Awake()
    {
        environmentAssetsLoader.onEnvironmentAssetLoaded += HandleEnvironmentAssetLoaded;

        environmentSpawner = GetComponent<EnvironmentSpawner>();

        environmentChannel.OnRequestPauseSwitchEnvironment += PauseSafeZoneGeneration;
        environmentChannel.OnRequestUnPauseSwitchEnvironment += UnPauseSafeZoneGeneration;
        environmentSpawner.batchOfEnvironmentSpawned += CheckAndGenerateSafeZone_MergePatch;
        playerStartedFlying.TheEvent.AddListener(PauseSafeZoneGenerationAndOverrideUnPauseBehaviour);
        playerEndedFlying.TheEvent.AddListener(CheckAndUnPauseSafeZoneGeneration);
    }

    private void OnDestroy()
    {
        environmentAssetsLoader.onEnvironmentAssetLoaded -= HandleEnvironmentAssetLoaded;

        environmentChannel.OnRequestPauseSwitchEnvironment -= PauseSafeZoneGeneration;
        environmentChannel.OnRequestUnPauseSwitchEnvironment -= UnPauseSafeZoneGeneration;
        environmentSpawner.batchOfEnvironmentSpawned -= CheckAndGenerateSafeZone_MergePatch;
        playerStartedFlying.TheEvent.RemoveListener(PauseSafeZoneGenerationAndOverrideUnPauseBehaviour);
        playerEndedFlying.TheEvent.RemoveListener(CheckAndUnPauseSafeZoneGeneration);
    }

    private void PauseSafeZoneGenerationAndOverrideUnPauseBehaviour(GameEvent gameEvent)
    {
        PauseSafeZoneGeneration();
    }

    private void CheckAndUnPauseSafeZoneGeneration(GameEvent gameEvent)
    {
        UnPauseSafeZoneGeneration();
    }

    private void PauseSafeZoneGeneration()
    {
        pauseSafeZoneGeneration = true;
        currentPausedSwitchEnvGenNumber++;
    }

    private void UnPauseSafeZoneGeneration()
    {
        currentPausedSwitchEnvGenNumber--;

        if(currentPausedSwitchEnvGenNumber <= 0)
        {
            pauseSafeZoneGeneration = false;
        }
    }

    private void CheckAndGenerateSafeZone_MergePatch(List<Patch> patchList)
    {
        if (isLoadingEnvAssets)
            return;

        if (pauseSafeZoneGeneration)
            return;

        if (ignoreBatchSpawnEvent)
            return;

        float remainingDistanceBeforeSwitch = environmentSwitchingOrder.DistanceCoveredByEnvBeforeSwitch[currentEnvDistIndex] - environmentData.distanceCoveredByActiveEnvironment;

        if (remainingDistanceBeforeSwitch <= environmentData.minimumSafeZoneLength)
        {
            isLoadingEnvAssets = true;
            Environment envToSpawn = GetNextEnvironmentToSpawn();
            environmentAssetsLoader.LoadEnvironmentAssets(envToSpawn);
        }

        //    StartCoroutine(CheckAndGenerateSafeZone_MergePatchRoutine());
    }

    private void HandleEnvironmentAssetLoaded(EnviornmentSO loadedEnvironment)
    {
        isLoadingEnvAssets = false;
        StartCoroutine(CheckAndGenerateSafeZone_MergePatchRoutine(loadedEnvironment));
    }

    private Environment GetNextEnvironmentToSpawn()
    {
        int nextEnvIndex = currentEnvIndex + 1;
        nextEnvIndex = nextEnvIndex >= environmentSwitchingOrder.EnvironmentToLoopThrough.Length ? 0 : nextEnvIndex;
        return environmentSwitchingOrder.EnvironmentToLoopThrough[nextEnvIndex];
    }
    int currentRampndex=0;
    private IEnumerator CheckAndGenerateSafeZone_MergePatchRoutine(EnviornmentSO loadedEnvironment)
    {
        // envFactorySO.AddEnvironmentForWarmup(loadedEnvironment);
        // yield return envPatchPoolSO.PreWarmWithDelayNoRecord(1);

        // float remainingDistanceBeforeSwitch = distanceCoveredByEnvBeforeSwitch[currentEnvDistIndex] - environmentData.distanceCoveredByActiveEnvironment;

        // if (remainingDistanceBeforeSwitch <= environmentData.minimumSafeZoneLength)
        // {
        ignoreBatchSpawnEvent = true;

        // Should Spawn safe zone now
        //    UnityEngine.Console.Log("Spawning Safe Zone Of Length ");
        yield return environmentSpawner.SpawnSpecificDistancePatches(environmentData.minimumSafeZoneLength);

        // *NOTE* *TEMP* Change the following conditional snippet later on as the design requirement regarding env switching is not clear

        if (currentEnvDistIndex < 4)
        {
            environmentSpawner.ChangeEnvCategory(envChangeCategory);
            environmentSpawner.SpawnOnePatchOnly(false);
        }
        // Looping env
        else if (currentEnvDistIndex == environmentSwitchingOrder.DistanceCoveredByEnvBeforeSwitch.Length - 1)
        {
            if (currentEnvIndex == 2)
            {
                environmentSpawner.ChangeActiveEnv(switchEnvGeneral); // Ramp Building
                environmentSpawner.ChangeEnvCategory(envChangeCategory);
                environmentSpawner.SpawnOnePatchOnly(true);
            }
            else
            {
                environmentSpawner.ChangeEnvCategory(envChangeCategory);
                environmentSpawner.SpawnOnePatchOnly(false);
            }
        }
        else
        {
            if (currentRampndex == 0)
            {
                environmentSpawner.ChangeActiveEnv(switchEnvGeneral); // Ramp Building
            }
            currentRampndex++;
            if (currentRampndex == 3)
                currentRampndex = 0;
            environmentSpawner.ChangeEnvCategory(envChangeCategory);
            environmentSpawner.SpawnOnePatchOnly(true);
        }

        // *NOTE* *TEMP* Change the above conditional snippet later on as the design requirement regarding env switching is not clear

        ++currentEnvIndex;

        currentEnvIndex = currentEnvIndex >= environmentSwitchingOrder.EnvironmentToLoopThrough.Length ? 0 : currentEnvIndex;

        EnviornmentSO enviornmentSO = loadedEnvironment;
        //UnityEngine.Console.Log($"Changing Active Env to {enviornmentSO.name}");

        environmentSpawner.ChangeEnvCategory(envCategory);
        environmentSpawner.ChangeActiveEnv(enviornmentSO, true);

        // UnityEngine.Console.Log($"Chaning Current Env Index To {currentEnvIndex} Lengt hMax {enviornmentToLoopThrough.Length}");

        currentEnvDistIndex = currentEnvDistIndex >= environmentSwitchingOrder.DistanceCoveredByEnvBeforeSwitch.Length - 1 ? environmentSwitchingOrder.DistanceCoveredByEnvBeforeSwitch.Length - 1 : ++currentEnvDistIndex;
        //  UnityEngine.Console.Log($"Chaning Current Env Distance Index To {currentEnvDistIndex} Value {distanceCoveredByEnvBeforeSwitch[currentEnvDistIndex]}");

        ignoreBatchSpawnEvent = false;
        //}
    }
}
