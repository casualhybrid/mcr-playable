using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;

[CreateAssetMenu(fileName = "PowerUpDataBase", menuName = "ScriptableObjects/PowerUpDataBase", order = 1)]
public class PowerUpDataBase : ScriptableObject
{
    [System.Serializable]
    public class PowerUpDictionary : SerializableDictionaryBase<PowerUpSO, PowerUpData> { }

    [SerializeField] private PowerUpDictionary ThePowerUpDictionary;

}