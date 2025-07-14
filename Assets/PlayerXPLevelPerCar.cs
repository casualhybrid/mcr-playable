using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;

[System.Serializable]
public class Values
{
    [SerializeField]public float distanceInUnits;
    [SerializeField]public float xp;
    [SerializeField] public float xpRequiredInitial;
}

[System.Serializable]
public class XPLevelPerKm : SerializableDictionaryBase<string, Values>
{

}
[CreateAssetMenu(fileName = "PlayerXPLevelPerCar", menuName = "ScriptableObjects/PlayerXPLevelPerCar")]
public class PlayerXPLevelPerCar : ScriptableObject
{
    [SerializeField]public XPLevelPerKm XPLevelPerKm;

}
