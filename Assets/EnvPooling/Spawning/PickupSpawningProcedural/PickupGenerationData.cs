using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;
using Sirenix.OdinInspector;

public enum SpawnType
{
    CommonSpawn = 1,
    UncommonSpawn = 2,
    RareSpawn = 4
}

[System.Serializable]
public struct PickupSpawnData
{
    [SerializeField] private InventoryItemSO pickupType;
    [SerializeField] private GameObject prefab;
    [EnumToggleButtons] public SpawnType pickupSpawnType;
    [Range(0, 1)]
    [Tooltip("Spawn probability weight. Characters should have higher values.")]
    [LabelText("Spawn Probability")]
    public float spawnProbability;
    public InventoryItemSO GetPickupType => pickupType;
    public float GetMinimumDistanceBeforeSpawn => (float)pickupSpawnType * GameManager.ChunkDistance;

    public float GetInitialMinimumDistanceBeforeSpawn => ((float)pickupSpawnType - 1) * GameManager.ChunkDistance;
    public GameObject GetPrefab => prefab;
    public float GetSpawnProbability => spawnProbability;
}

[CreateAssetMenu(fileName = "PickupGenerationData", menuName = "ScriptableObjects/PickupGenerationData")]
public class PickupGenerationData : ScriptableObject
{
    [System.Serializable] public class PickupToGenerationDictionary : SerializableDictionaryBase<InventoryItemSO, PickupSpawnData> { }

    [SerializeField] private PickupToGenerationDictionary pickupSpawnData;
    public PickupToGenerationDictionary GetPickupSpawnData => pickupSpawnData;

    [SerializeField] private PickupCurrentStateData pickupCurrentStateData;
    public PickupCurrentStateData PickupCurrentStateData => pickupCurrentStateData;

    public PickupSpawnData GetSpawnDataForPickup(InventoryItemSO inventoryItemSO)
    {
        return pickupSpawnData[inventoryItemSO];
    }
}