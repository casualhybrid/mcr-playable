using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "CarConfigurationData", menuName = "ScriptableObjects/StaticData/Cars/CarConfigurationData")]
public class CarConfigurationData : SerializedScriptableObject, IBasicAssetInfo
{

    [SerializeField] private Sprite carImage;
    [SerializeField] private string carName;
    [SerializeField] private string carKey;
    [SerializeField] private ItemPrice carPrice;
    [SerializeField] private int unlockLevel;
    [SerializeField] private string carDisplayKey;
    [SerializeField] private Dictionary<int, Vector3> charactersSittingPosition;

    [SerializeField] private float thrust;

    [SerializeField] private float airplane;

    [SerializeField] private float boost;

    public int IndexKey { get; set; } = -1;

    public string GetName => carName;

    public string GetLoadingKeyGamePlay => carKey;

    public string GetLoadingKeyDisplay => carDisplayKey;

    public float GetThrust => thrust;

    public float GetAirplane => airplane;

    public float GetBoost => boost;

    public int GetUnlockLevel => unlockLevel;

    public Sprite GetCarSprite => carImage;

    public int GetPrice

    {
        get
        {
            if (carPrice.GetCurrencyMode == ItemPrice.CurrencyMode.Single)
            {
                return carPrice.GetPricesList.First().GetAmount;
            }
            else
            {
                UnityEngine.Console.LogWarning("Multiple currency mode hasn't been implemented YET!");
                return 0;
            }
        }
    }

    public Vector3 GetCharacterSittingPosition(int index)
    {
        /*if (!charactersSittingPosition.ContainsKey(index))
        {
            throw new System.Exception($"Invalid character sitting position requested for character ID {index}");
        }*/

        return charactersSittingPosition[index];
    }
}