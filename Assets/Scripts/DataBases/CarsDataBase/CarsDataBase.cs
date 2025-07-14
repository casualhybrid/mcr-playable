using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CarsDataBase", menuName = "ScriptableObjects/StaticData/Cars/CarsDataBase")]
public class CarsDataBase : SerializedScriptableObject
{
    [ReadOnly] [SerializeField] public float MaxAeroplaneDuration;
    [ReadOnly] [SerializeField] public float MaxThrustDuration;
    [ReadOnly] [SerializeField] public float MaxBoostDuration;

    public Dictionary<int, CarConfigurationData> carsDataBaseDictionary;

    private void OnEnable()
    {
        foreach (var pair in carsDataBaseDictionary)
        {
            pair.Value.IndexKey = pair.Key;
        }
    }

    private void OnValidate()
    {
        float _MaxAeroplaneDuration = 0;
        float _MaxThrustDuration = 0;
        float _MaxBoostDuration = 0;

        foreach (CarConfigurationData config in carsDataBaseDictionary.Values)
        {
            if (config.GetAirplane > _MaxAeroplaneDuration)
                _MaxAeroplaneDuration = config.GetAirplane;

            if (config.GetThrust > _MaxThrustDuration)
                _MaxThrustDuration = config.GetThrust;

            if (config.GetBoost > _MaxBoostDuration)
                _MaxBoostDuration = config.GetBoost;
        }

         MaxAeroplaneDuration = _MaxAeroplaneDuration;
         MaxThrustDuration = _MaxThrustDuration;
         MaxBoostDuration = _MaxBoostDuration;
    }

    public CarConfigurationData GetCarConfigurationData(int carIndexKey)
    {
        CarConfigurationData carConfigurationData;

        carsDataBaseDictionary.TryGetValue(carIndexKey, out carConfigurationData);

        if (carConfigurationData == null)
            throw new System.Exception($"Failed to find car configuration data for the requested keyIndex {carIndexKey}");

        return carConfigurationData;
    }

    public ICollection<CarConfigurationData> GetAllCarConfigurations()
    {
        return carsDataBaseDictionary.Values;
    }

    public int GetIndexOfTheCarFromItsConfigData(CarConfigurationData config)
    {
        if (config.IndexKey == -1)
            throw new System.Exception("index key for the supplied car config is not set: " + config.GetName);

        return config.IndexKey;

        //foreach (var pair in carsDataBaseDictionary)
        //{
        //    if (pair.Value != config)
        //        continue;

        //    return pair.Key;
        //}

        //throw new System.Exception("Failed to find index for the supplied car config " + config.GetName);
    }

    public CarConfigurationData GetMaximumSupportedCarForThePlayerLevel(int level)
    {
        CarConfigurationData config = null;
        int levelFound = -1;

        foreach (var item in carsDataBaseDictionary)
        {
            int itemUnlockLevel = item.Value.GetUnlockLevel;

            if (itemUnlockLevel > levelFound && itemUnlockLevel <= level)
            {
                config = item.Value;
                levelFound = itemUnlockLevel;
            }
        }

        return config;
    }
}