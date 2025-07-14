using TheKnights.SaveFileSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "PowerUpDurationManager", menuName = "ScriptableObjects/Powerups/PowerUpDurationManager")]
public class PowerUpDurationManager : ScriptableObject
{
    [SerializeField] private CarsDataBase carsDatabase;
    [SerializeField] protected SpecialPickupsEnumSO specialPickupsEnumSO;
    [SerializeField] private SaveManager saveManager;

    public float GetPowerDurationForCar(ScriptableObject power)
    {
        int selectedCar = saveManager.MainSaveFile.currentlySelectedCar;
        CarConfigurationData carData = carsDatabase.GetCarConfigurationData(selectedCar);

        if (power == specialPickupsEnumSO.BoostPickup)
        {
            return carData.GetBoost;
        }
        else if (power == specialPickupsEnumSO.AeroPlanePickup)
        {
            return carData.GetAirplane;
        }
        else if (power == specialPickupsEnumSO.ThrustPickup)
        {
            return carData.GetThrust;
        }

        throw new System.Exception("Invalid power to get duration of. " + power.name);
    }
}