using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "PowerUpData", menuName = "ScriptableObjects/PowerUpData ")]
public class PowerUpData : ScriptableObject
{
    [SerializeField] private ItemPrice itemPrice;


    public ItemPrice GetItemPrice => itemPrice;
}
