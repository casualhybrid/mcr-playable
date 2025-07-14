using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VehicleData", menuName = "ScriptableObjects/VehicleData")]
public class VehicleData : ScriptableObject
{
    [SerializeField] private ItemPrice itemPrice;
    [SerializeField] private bool isLockedAtStart;
    [SerializeField] private int playerLevelRequirement;

    public ItemPrice GetItemPrice => itemPrice;
}
