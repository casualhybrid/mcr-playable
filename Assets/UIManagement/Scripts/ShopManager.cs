using TheKnights.SaveFileSystem;
using UnityEngine;
using System.Linq;

public enum InventoryToItemPriceRelation
{
    Twice = 2, Fifth = 5
}

public struct PlayableObjectDataWithIndex
{
    public IBasicAssetInfo basicAssetInfo;
    public int index;

    public bool isValid;

    public PlayableObjectDataWithIndex(IBasicAssetInfo _basicAssetInfo, int _index)
    {
        basicAssetInfo = _basicAssetInfo;
        index = _index;

        isValid = true;
    }
}


[CreateAssetMenu(fileName = "ShopManager", menuName = "ShopSystem/ShopManager", order = 1)]
public class ShopManager : ScriptableObject
{
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private CarsDataBase carsDataBase;
    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private GamePlaySessionInventory gamePlaySessionInventory;
    [SerializeField] private PlayerLevelingSystem playerLevelingSystem;

    public PlayableObjectDataWithIndex GetAnyAvailableToBuyCar()
    {
        foreach (var item in carsDataBase.carsDataBaseDictionary)
        {
            int index = item.Key;
            bool isUnlocked = inventorySystem.isCarUnlocked(index);

            if (isUnlocked)
                continue;

            bool hasAlreadyShown = PlayerPrefs.GetInt("ShownCarAvailable_1x" + index, 0) == 1;

            if (hasAlreadyShown)
                continue;

            var configData = carsDataBase.GetCarConfigurationData(index);

            int price = configData.GetPrice;

          //  UnityEngine.Console.Log($"Price for car index {index} is {price}");

            if ((inventorySystem.GetIntKeyValue("AccountCoins") + gamePlaySessionInventory.GetIntKeyValue("AccountCoins")) < price)
                continue;

            int levelRequired = configData.GetUnlockLevel;

            int playerLevel = playerLevelingSystem.GetCurrentPlayerLevelForDistanceCovered();

          //  UnityEngine.Console.Log("Player LEVEL ISSSS " + playerLevel);

            if (playerLevel < levelRequired)
                continue;

            PlayerPrefs.SetInt("ShownCarAvailable_1x" + index, 1);

            return new PlayableObjectDataWithIndex(configData, index);
        }

        return new PlayableObjectDataWithIndex();
    }


    public PlayableObjectDataWithIndex CheckIfCarIsAvailableWithPlayerCoinsBeingMoreThanRequired(InventoryToItemPriceRelation inventoryToItemPriceRelation)
    {
        var sortedCars = carsDataBase.carsDataBaseDictionary.OrderByDescending((x) => x.Value.GetPrice);

        foreach (var item in sortedCars)
        {
            int index = item.Key;
            bool isUnlocked = inventorySystem.isCarUnlocked(index);

            if (isUnlocked)
                continue;

            var configData = carsDataBase.GetCarConfigurationData(index);

            int price = configData.GetPrice;

            if (inventorySystem.GetIntKeyValue("AccountCoins") < price * (int)inventoryToItemPriceRelation)
                continue;

            int playerLevel = saveManager.MainSaveFile.PlayerCurrentLevel;

            int levelRequired = configData.GetUnlockLevel;

            if (playerLevel < levelRequired)
                continue;

            return new PlayableObjectDataWithIndex(configData,index);
        }

        return new PlayableObjectDataWithIndex();
    }
}