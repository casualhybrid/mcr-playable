using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.Serialization;
using Sirenix.OdinInspector;

[System.Serializable]
public struct InventoryItemMeta
{
    public string Name;
    public Sprite Sprite;
}

[CreateAssetMenu(fileName = "InventoryItemsMetaData", menuName = "PlayerInventorySystem/InventoryItemsMetaData")]
public class InventoryItemsMetaData : SerializedScriptableObject
{
    [OdinSerialize] private Dictionary<InventoryItemSO, InventoryItemMeta> inventoryItemsMetaDictionary;

    public InventoryItemMeta GetInventoryItemMeta(InventoryItemSO inventoryItemSO)
    {
        InventoryItemMeta meta = default;
        bool success = inventoryItemsMetaDictionary.TryGetValue(inventoryItemSO, out meta);

        if(!success)
        {
            throw new System.Exception("Failed to get metaData for inventory item " + inventoryItemSO.name);
        }

        return meta;
    }
}
