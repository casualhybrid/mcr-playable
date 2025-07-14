using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PickupsContainer", menuName = "ScriptableObjects/Pickups/PickupsContainer")]
public class PickupsContainer : ScriptableObject
{
    public List<InventoryItemSO> possiblePickups;
}
