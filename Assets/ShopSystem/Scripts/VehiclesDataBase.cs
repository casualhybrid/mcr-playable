using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;

[CreateAssetMenu(fileName = "VehiclesDataBase", menuName = "ScriptableObjects/VehiclesDataBase")]
public class VehiclesDataBase : ScriptableObject
{
    [System.Serializable]
    public class VehicleDataDictionary : SerializableDictionaryBase<VehicleSO, VehicleData> { }

    [SerializeField] private VehicleDataDictionary TheVehicleDataDictionary;
}
