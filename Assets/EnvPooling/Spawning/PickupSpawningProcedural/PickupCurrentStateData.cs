using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class PickupCondition
{
    public float initialOffset = -1;
    public float lastSpawnedZPosition = -1;
    public float currentProbability;
}

[CreateAssetMenu(fileName = "PickupCurrentStateData", menuName = "ScriptableObjects/PickupCurrentStateData")]
public class PickupCurrentStateData : ScriptableObject
{
    [SerializeField] private PickupGenerationData pickupGenerationData;
    
    // magnet safe area for airplane
    public float magnetPickupSafeArea { get; set; } = -1f;

    public InventoryItemSO lastSpawnedPickup { get; set; }

    public float lastPickupSpawnedDistanceZPoint { get; set; } = -1f;

    // pickup current state e.g. last spawned etc.

    public readonly Dictionary<InventoryItemSO, PickupCondition> GetPickupCurrentStateDictionary /*{ get; private set; } = new Dictionary<InventoryItemSO, PickupCondition>();*/ = new Dictionary<InventoryItemSO, PickupCondition>();

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        ResetVariable();
    }

    private void ResetVariable()
    {
        magnetPickupSafeArea = -1f;
        lastPickupSpawnedDistanceZPoint = -1f;
        lastSpawnedPickup = null;

     //  GetPickupCurrentStateDictionary.Clear();
    }

    public void InitializePickupCurrentStateDictionary()
    {
        GetPickupCurrentStateDictionary.Clear();

        PickupSpawnData[] spawnData = pickupGenerationData.GetPickupSpawnData.Values.ToArray();

        if (GetPickupCurrentStateDictionary.Count <= 0)
        {
            for (int i = 0; i < spawnData.Length; i++)
            {
                PickupSpawnData data = spawnData[i];
                PickupCondition pickupCondition = new PickupCondition();
                pickupCondition.initialOffset = /*data.GetInitialMinimumDistanceBeforeSpawn*/ data.GetMinimumDistanceBeforeSpawn ;
                pickupCondition.currentProbability = 1f / (float)spawnData.Length;
                GetPickupCurrentStateDictionary.Add(data.GetPickupType, pickupCondition);
            }
        }
    }

    public void UpdateLastSpawnedDistanceForAPickup(InventoryItemSO pickup, float distanceToUpdate)
    {
        PickupCondition pickupCondition;
        GetPickupCurrentStateDictionary.TryGetValue(pickup, out pickupCondition);

        if (pickupCondition == null)
        {
            throw new System.Exception($"Failed to update last spawned Z position for a pickup {pickup.name}");
        }

        pickupCondition.lastSpawnedZPosition = distanceToUpdate;
      


    }
}
