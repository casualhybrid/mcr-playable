using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using CustomEncounterData = CustomEncounterFactorySO.CustomEncounterData;

[CreateAssetMenu(fileName = "CustomEncountersSO", menuName = "ScriptableObjects/Encounters/CustomEncountersSO")]
public class CustomEncountersSO : ScriptableObject
{
    [SerializeField] private CustomEncounterPoolSO customEncounterPoolSO;
    [SerializeField] private DifficultyScaleSO difficultyScaleSO;

    private CustomEncounterData LastSpawnedCustomEncounter = new(null, 0);

    private int TestingEncounterIndex {get; set;}
    public bool pauseCoinAndPickupGenerationInTestingEncounter
    {
        get
        {
            CustomEncounterFactorySO customEncounterFactorySO = customEncounterPoolSO.Factory as CustomEncounterFactorySO;
            return customEncounterFactorySO.pauseCoinAndPickupGenerationInTestingEncounter;
        }
    }
    public bool AreTestingEncountersAvailable
    {
        get
        {
            CustomEncounterFactorySO customEncounterFactorySO = customEncounterPoolSO.Factory as CustomEncounterFactorySO;
            return customEncounterFactorySO.TestingEncounters.Length > 0;
        }
    }

    public bool AreCustomEncountersAvailable
    {
        get
        {
            CustomEncounterFactorySO customEncounterFactorySO = customEncounterPoolSO.Factory as CustomEncounterFactorySO;
            return customEncounterFactorySO.CustomEncountersList.Count > 0;
        }
    }

    #region Unity Callbacks
    private void OnEnable()
    {
        SceneManager.activeSceneChanged += HandleActiveSceneChanged;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
    }
#endregion

#region Event Handling
    private void HandleActiveSceneChanged(Scene replacedScene, Scene nextScene)
    {
        ResetLastSpawnedCustomEncounter();
        ResetTestingEncounterIndex();
    }
    #endregion

    #region Custom Encounters
    public CustomEncounterData GetRandomCustomEncounter(IEnumerable<CustomEncounterData> pool)
    {
        // get universal probability 
        double u = pool.Sum(p => p.Probability);

        // pick a random number between 0 and u
        double r = (new System.Random()).NextDouble() * u;

        double sum = 0;
        foreach (CustomEncounterData n in pool)
        {
            // loop until the random number is less than our cumulative probability
            if (r <= (sum += n.Probability))
            {
                return n;
            }
        }
        // should never get here
        return new(null, 0);
    }

    public CustomEncounter GetCustomEncounter(float maxEncounterLength = Mathf.Infinity, float currentIntendedPlayerDifficultyRating = Mathf.Infinity)
    {
        CustomEncounterFactorySO customEncounterFactorySO = customEncounterPoolSO.Factory as CustomEncounterFactorySO;

        CustomEncounter customEncounterToReturn;
        if (customEncounterFactorySO.TestingEncounters.Length == 0)
        {
            List<CustomEncounterData> eligibleCustomEncounters = new List<CustomEncounterData>(customEncounterFactorySO.CustomEncountersList);


            if (LastSpawnedCustomEncounter.Prefab != null)
                eligibleCustomEncounters = RemoveLastSpawnedCustomEncounter(ref eligibleCustomEncounters);

            eligibleCustomEncounters = MapCustomEncountersOnDifficultyScale(ref eligibleCustomEncounters, currentIntendedPlayerDifficultyRating < 35);

            eligibleCustomEncounters = RemoveCustomEncountersLongerThanSpecifiedLength(ref eligibleCustomEncounters, maxEncounterLength);


            if (eligibleCustomEncounters.Count > 0)
            {
                LastSpawnedCustomEncounter = GetRandomCustomEncounter(eligibleCustomEncounters);
                for (int i = 0; i < customEncounterFactorySO.CustomEncountersList.Count; i++)
                {
                    if (customEncounterFactorySO.CustomEncountersList[i].Prefab == LastSpawnedCustomEncounter.Prefab)
                    {
                        customEncounterFactorySO.UpdateProbability(i);
                        break;
                    }
                }
                customEncounterToReturn = LastSpawnedCustomEncounter.Prefab;
            }
            else
            {
                // The place where this function is called will disregard this returned encounter automatically
                customEncounterToReturn = GetRandomCustomEncounter(eligibleCustomEncounters).Prefab;
            }
        }
        else
        {
            customEncounterToReturn = customEncounterFactorySO.TestingEncounters[TestingEncounterIndex];
        }

        if (customEncounterToReturn == null)
            UnityEngine.Console.LogError("Returning Null WTH");

        return customEncounterToReturn;
    }

    private List<CustomEncounterData> RemoveLastSpawnedCustomEncounter(ref List<CustomEncounterData> customEncounters)
    {
        customEncounters.Remove(LastSpawnedCustomEncounter);
        return customEncounters;
    }

    private List<CustomEncounterData> MapCustomEncountersOnDifficultyScale(ref List<CustomEncounterData> customEncounters, bool isLowDifficultyEncounters)
    {
        for (int i = 0; i < customEncounters.Count; i++)
        {
            if (customEncounters[i].Prefab.CanSpawnInLowDifficulty != isLowDifficultyEncounters ||
                customEncounters[i].Prefab.RequiredDifficultyInPercent > difficultyScaleSO.CurrentIntendedDifficultyToMaxRatioPercent)
            {
                customEncounters.RemoveAt(i);
                i--;
            }
        }
        return customEncounters;
    }

    private List<CustomEncounterData> RemoveCustomEncountersLongerThanSpecifiedLength(ref List<CustomEncounterData> customEncounters, float maxEncounterLength)
    {
        for (int i = 0; i < customEncounters.Count; i++)
        {
            if (customEncounters[i].Prefab.EncounterSize > maxEncounterLength)
            {
                customEncounters.RemoveAt(i);
                i--;
            }
        }

        return customEncounters;
    }
    private void ResetLastSpawnedCustomEncounter()
    {
        LastSpawnedCustomEncounter = new(null, 0);
    }
#endregion

#region Testing Encounters
    public void IncrementTestingEncounterIndex()
    {
        TestingEncounterIndex++;
        CustomEncounterFactorySO customEncounterFactorySO = customEncounterPoolSO.Factory as CustomEncounterFactorySO;

        if (TestingEncounterIndex == customEncounterFactorySO.TestingEncounters.Length)
        {
            TestingEncounterIndex = 0;
        } 
    }

    private void ResetTestingEncounterIndex()
    {
        TestingEncounterIndex = 0;
    }
#endregion
}
