using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct InventoryItemAnimData
{
    public bool isValid { get; }
    public Vector2 screenLocation { get; }
    public DOTweenAnimation[] animations { get; }

    public InventoryItemAnimData(Vector2 _screenLocation, DOTweenAnimation[] _animations, bool _isValid = true)
    {
        screenLocation = _screenLocation;
        animations = _animations;
        isValid = _isValid;
    }
}

public static class InventoryPositions 
{
    private static readonly Dictionary<string, InventoryItemAnimData> inventoryItemAnimationDataCollection = new Dictionary<string, InventoryItemAnimData>();

    public static InventoryItemAnimData GetAnimDataForItem(string itemName)
    {
        InventoryItemAnimData data;
        inventoryItemAnimationDataCollection.TryGetValue(itemName, out data);
        return data;
    }

    public static void UpdateInventoryLocation(InventoryItemAnimData data, string inventoryItemType)
    {
        if(inventoryItemAnimationDataCollection.ContainsKey(inventoryItemType))
        {
            inventoryItemAnimationDataCollection[inventoryItemType] = data;
        }
        else
        {
            inventoryItemAnimationDataCollection.Add(inventoryItemType, data);
        }
    }

    //public static void UpdateInventoryLocation(Dictionary<string, RectTransform> keyValuePairs, Camera cam)
    //{

    //    foreach (var item in keyValuePairs)
    //    {
    //        RectTransform rt = item.Value;
    //        Vector2 screenPos = cam.WorldToScreenPoint(rt.position);

    //        if (inventoryScreenSpaceLocations.ContainsKey(item.Key))
    //        {
    //            inventoryScreenSpaceLocations[item.Key] = screenPos;
    //        }
    //        else
    //        {
    //            inventoryScreenSpaceLocations.Add(item.Key, screenPos);
    //        }
    //    }

       
    //}
}
