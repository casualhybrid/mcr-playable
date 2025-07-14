using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using System.IO;


[CreateAssetMenu(fileName = "CustomEncounterFactorySO", menuName = "Factory/CustomEncounterFactorySO")]
public class CustomEncounterFactorySO : FactoryVarianceSO<CustomEncounter, int>
{
    [System.Serializable]
    public struct CustomEncounterData
    {
        [ReadOnly] public CustomEncounter Prefab;
        [ReadOnly] public float Probability;

        public CustomEncounterData(CustomEncounter prefab, float probability)
        {
            Prefab = prefab;
            Probability = probability;
        }
    }

    [Header("Main")]
    [SerializeField] private float probabilityOfEachEncounter = 0.2f;
    [SerializeField] private List<CustomEncounterData> customEncountersList;
    public List<CustomEncounterData> CustomEncountersList
    {
        get
        {
            return customEncountersList;
        }
    }

    private void OnEnable()
    {
        ResetCustomEncountersProbabilities();
    }

    public void ResetCustomEncountersProbabilities()
    {
        // Reset probability
        CustomEncounterData customEncounter;
        for (int i = 0; i < customEncountersList.Count; i++)
        {
            customEncounter = customEncountersList[i];
            customEncounter.Probability = probabilityOfEachEncounter;

            customEncountersList[i] = customEncounter;
        }
    }

    private int resettedEncountersCount = 0;
    public void UpdateProbability(int index)
    {
        if (++resettedEncountersCount >= customEncountersList.Count / 2)
        {
            ResetCustomEncountersProbabilities();
            resettedEncountersCount = 0;
        }

       customEncountersList[index] = new CustomEncounterData(customEncountersList[index].Prefab, customEncountersList[index].Probability / 4);
    }

    [Header("Testing - Only testing encounter is spawned, if the value is not null")]
    [SerializeField] private CustomEncounter[] testingEncounters;
    public CustomEncounter[] TestingEncounters
    {
        get
        {
            return testingEncounters;
        }
    }

    public bool pauseCoinAndPickupGenerationInTestingEncounter;

    #region Factory Related Functions
    public override CustomEncounter Create(CustomEncounter key)
    {
        return Instantiate(key);
    }

    public override Dictionary<int, Stack<CustomEncounter>> CreateBatch(int copiesPerItem)
    {
        Dictionary<int, Stack<CustomEncounter>> keyValuePairs = new Dictionary<int, Stack<CustomEncounter>>();

        foreach (CustomEncounterData customEncounter in customEncountersList)
        {
            Stack<CustomEncounter> stack = new Stack<CustomEncounter>();

            for (int k = 0; k < copiesPerItem; k++)
            {
                stack.Push(Instantiate(customEncounter.Prefab));
            }

            keyValuePairs.Add(customEncounter.Prefab.InstanceID, stack);
        }

        return keyValuePairs;
    }
    #endregion

    #region Editor Script for Custom Encounter List Population
#if UNITY_EDITOR
    [Header("Editor - Must refesh after any change in any custom encounter prefab or their scripts")]
    [Tooltip("Custom encounters are refreshed using this path")]
    [SerializeField] private string customEncountersPath = "Assets/Encounters/CustomEncounters/EncounterPrefabs/ShipIt";
    [SerializeField] private string customEncountersSkeletonPath = "Assets/Encounters/CustomEncounters/EncounterPrefabs/Skeletons/Shipit";
    [Button("Refresh and Validate all custom encounters")]
    private void AutoAssignCustomEncounter()
    {
        UnityEngine.Console.Log("Initiating validation of all custom encounters");

        //customEncountersList.Clear();

        List<Object> allAssetsAtPath = LoadAllAssetsAtPath(customEncountersPath);

        foreach (Object asset in allAssetsAtPath)
        {
            if ((asset as GameObject).GetComponent<CustomEncounter>() != null)
            {
                //customEncountersList.Add(new CustomEncounterData((asset as GameObject).GetComponent<CustomEncounter>(), probabilityOfEachEncounter));

                //foreach (IValidatable validatableScript in customEncountersList[^1].Prefab.gameObject.GetComponentsInChildren<IValidatable>())
                foreach (IValidatable validatableScript in (asset as GameObject).GetComponentsInChildren<IValidatable>())
                {
                    validatableScript.ValidateAndInitialize();
                }

                EditorUtility.SetDirty(asset);
            }
        }

        AssetDatabase.SaveAssets();

        UnityEngine.Console.Log("Completed validation of all custom encounters");
    }

    [Button("Assign references of custom encounter skeletons")]
    private void AssignReferencesOfCustomEncounterSkeletons()
    {
        customEncountersList.Clear();

        List<Object> allAssetsAtPath = LoadAllAssetsAtPath(customEncountersSkeletonPath);

        foreach (Object asset in allAssetsAtPath)
        {
            if ((asset as GameObject).GetComponent<CustomEncounter>() != null)
            {
                customEncountersList.Add(new CustomEncounterData((asset as GameObject).GetComponent<CustomEncounter>(), probabilityOfEachEncounter));
            }
        }

        UnityEngine.Console.Log("Skeletons references assignment completed");
    }

    List<Object> LoadAllAssetsAtPath(string path)
    {
        List<Object> objects = new List<Object>();
        if (Directory.Exists(path))
        {
            string[] assets = Directory.GetFiles(path);
            foreach (string assetPath in assets)
            {
                if (assetPath.Contains(".prefab") && !assetPath.Contains(".meta"))
                {
                    objects.Add(AssetDatabase.LoadMainAssetAtPath(assetPath));
                }
            }
        }
        return objects;
    }
#endif
    #endregion
}