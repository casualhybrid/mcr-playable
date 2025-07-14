using System;
using System.Collections.Generic;
using System.Linq;
using TheKnights.SaveFileSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "InventorySystem", menuName = "PlayerInventorySystem/InventorySystem", order = 1)]
public class InventorySystem : ScriptableObject
{
    public event Action<List<PlayableObjectDataWithIndex>> OnCarsUnlocked;
    public event Action<List<PlayableObjectDataWithIndex>> OnCharactersUnlocked;

    public KeyValuePair<string, int> LastMysteryBoxReward { get; set; }

    public SaveManager saveManagerObj;
    public List<IntegerValueItem> intKeyItems;
    public List<InventoryObject> gameCars;
    public Dictionary<int, int> CharactersFigurinesAvailable = new Dictionary<int, int>();
    public CharactersDataBase CharactersDataBase => charactersDataBase;

    [SerializeField] private GameEvent inventoryHasBeenUpdated;
    [SerializeField] private GameEvent updateUIEvent;
    [SerializeField] private InventoryUpdateEvent updateCelebrationEvent/*, sendCelebrationInfo*/;
    [SerializeField] private InventoryUpdateEvent onAnItemHasBeenAdded;
    [SerializeField] private GameEvent onCarPurchased;
    [SerializeField] private GameEvent CharacterHasBeenUnlocked;
    [SerializeField] private CarsDataBase carsDataBase;
    [SerializeField] private CharactersDataBase charactersDataBase;

    public void SetUpValuesForGame(MainSaveFile saveFileObj)
    {
        for (int i = 0; i < intKeyItems.Count; i++)
        {
            intKeyItems[i].itemValue = saveFileObj.gameIntItems[i].itemValue;
        }

        for (int i = 0; i < gameCars.Count; i++)
        {
            gameCars[i].isObjectUnlocked = saveFileObj.gameVehicles[i].isObjectUnlocked;
        }

        foreach (var pair in saveFileObj.CharactersFigurinesAvailable)
        {
            CharactersFigurinesAvailable.Add(pair.Key, pair.Value);
        }
    }

    #region Figurines

    public int UpdateCharacterFigurine(int characterKey, int value)
    {
        int curFigurines = CharactersFigurinesAvailable[characterKey];
        int reqFigurine = charactersDataBase.GetCharacterConfigurationData(characterKey).FigurinesToUnlock;

        if((reqFigurine - curFigurines == 1) && value >= 1)
        {
            UnlockCharacter(characterKey);
            return reqFigurine;
        }

        int updatedFigurines = Mathf.Clamp(0, curFigurines + value, reqFigurine);
        CharactersFigurinesAvailable[characterKey] = updatedFigurines;

        inventoryHasBeenUpdated.RaiseEvent();

        return updatedFigurines;
    }

    public int GetCharacterFigurines(int characterKey)
    {
        return CharactersFigurinesAvailable[characterKey];
    }

    #endregion


    #region IntItem Value Settings

    private bool UpdateKeyValue(string itemKey, int value, bool isCelebrationAddition, bool showAddEffect = true)
    {
        foreach (IntegerValueItem obj in intKeyItems)
        {
            if (obj.itemName.Equals(itemKey))
            {
                obj.itemValue += value;

                updateUIEvent.RaiseEvent();

                if (value > 0 && showAddEffect)
                {
                    onAnItemHasBeenAdded.RaiseEvent(itemKey, value);
                }

                return true;
            }
        }
        return false;      // Item Not Found
    }

    public void UpdateKeyValues(List<InventoryItem<int>> inventoryItems, bool raiseInventoryUpdateEvent = true, bool showAddEffect = true, string context = null, bool isDoubleRewardPossible = true)
    {
        List<InventoryItem<int>> itemsToCelebrate = new List<InventoryItem<int>>();

        for (int i = 0; i < inventoryItems.Count; i++)
        {
            InventoryItem<int> item = inventoryItems[i];
            UpdateKeyValue(item.itemName, item.itemValue, item.isCelebrationAddition, showAddEffect);

            if(item.isCelebrationAddition)
            {
                itemsToCelebrate.Add(item);
            }
        }

        // Raise inventory update event

        if (raiseInventoryUpdateEvent)
        {
            inventoryHasBeenUpdated.RaiseEvent();
        }

        // We have to show the celebration panel
        if(itemsToCelebrate.Count > 0)
        {
            updateCelebrationEvent.RaiseBatchOfItemsToBeCelebratedAdded(itemsToCelebrate, context, isDoubleRewardPossible);
        }

    }

    public int GetIntKeyValue(string itemKey)
    {
        foreach (IntegerValueItem obj in intKeyItems)
        {
            if (obj.itemName.Equals(itemKey))
            {
                return obj.itemValue;
            }
        }
        return -1;      // Item Not Found
    }

    #endregion IntItem Value Settings

    /// <summary>
    /// Needed to remove this whole lot of section because we have
    /// made independent Vehicale system for our project
    /// </summary>
    /// <param name="carIndex"></param>
    /// <returns></returns>

    #region Cars Related Functionality

    public bool UnlockCar(int carIndex, bool raiseSpecificCarUnlockedEvent = true, bool raiseInventoryHasUpdatedEvent = true)
    {
        UnityEngine.Console.Log("Request to unlock car " + carIndex);

        if (carIndex < gameCars.Count)
        {
            bool isCarUnlockedBefore = isCarUnlocked(carIndex);

            if (isCarUnlockedBefore)
                return false;

            gameCars[carIndex].isObjectUnlocked = true;

            if (raiseInventoryHasUpdatedEvent)
            {
                inventoryHasBeenUpdated.RaiseEvent();
            }

            onCarPurchased.RaiseEvent();

            if(raiseSpecificCarUnlockedEvent)
            {
                CarConfigurationData configData = carsDataBase.GetCarConfigurationData(carIndex);
                OnCarsUnlocked?.Invoke(new List<PlayableObjectDataWithIndex>() {new PlayableObjectDataWithIndex(configData,carIndex) });
            }

            return true;
        }

        return false;
    }

    public void UnlockCars(int[] carsToUnlock)
    {
        List<PlayableObjectDataWithIndex> carsList = new List<PlayableObjectDataWithIndex>();

        for (int i = 0; i < carsToUnlock.Length; i++)
        {
            int carIndex = carsToUnlock[i];
            bool success = UnlockCar(carIndex, false, false);

            if (success)
            {
                carsList.Add(new PlayableObjectDataWithIndex(carsDataBase.GetCarConfigurationData(carIndex), carIndex));
            }
        }

        inventoryHasBeenUpdated.RaiseEvent();
        OnCarsUnlocked?.Invoke(carsList);
    }

    public bool UnlockCar(CarConfigurationData configData, bool raiseSpecificCarUnlockedEvent = true, bool raiseInventoryHasUpdatedEvent = true)
    {
       int index = carsDataBase.GetIndexOfTheCarFromItsConfigData(configData);
       return UnlockCar(index, raiseSpecificCarUnlockedEvent, raiseInventoryHasUpdatedEvent);
    }

    public void UnlockAllCars()
    {
        List<PlayableObjectDataWithIndex> carsList = new List<PlayableObjectDataWithIndex>();

        foreach (var pair in carsDataBase.carsDataBaseDictionary)
        {
            if (!isCarUnlocked(pair.Key))
            {
                carsList.Add(new PlayableObjectDataWithIndex(pair.Value, pair.Key));
            }
        }

        for (int i = 0; i < gameCars.Count; i++)
        {
            InventoryObject car = gameCars[i];
            car.isObjectUnlocked = true;
        }

        inventoryHasBeenUpdated.RaiseEvent();
        OnCarsUnlocked?.Invoke(carsList);
    }

    public bool isCarUnlocked(int carIndex)
    {
        if (carIndex < gameCars.Count)
        {
            return gameCars[carIndex].isObjectUnlocked;
        }

        return false;
    }

    public bool isCarUnlocked(CarConfigurationData configData)
    {
        int index = carsDataBase.GetIndexOfTheCarFromItsConfigData(configData);

        return isCarUnlocked(index);
    }

    public bool AreAllCarsUnlocked()
    {
        bool unlocked = true;

        for (int i = 0; i < gameCars.Count; i++)
        {
            InventoryObject car = gameCars[i];

            if (!car.isObjectUnlocked)
            {
                unlocked = false;
                break;
            }
        }

        return unlocked;
    }


    public bool UpdateObjectPropertyValue(InventoryObject obj, string propertyName, int updatedValue)
    {
        foreach (IntegerValueItem invItem in obj.properties)
        {
            if (invItem.itemName.Equals(propertyName))
            {
                ChangePropertyValue(propertyName, updatedValue);
                return true;
            }
        }
        return false;
    }

    private void ChangePropertyValue(string propertyName, int updatedValue)
    {
        // here we will do binary Manipulations
    }

    #endregion Cars Related Functionality

    #region Characters Related Functionality

    public bool isCharacterUnlocked(int key)
    {
       int figurinesReq = charactersDataBase.GetCharacterConfigurationData(key).FigurinesToUnlock;
       return CharactersFigurinesAvailable[key] >= figurinesReq;
    }

    public bool isCharacterUnlocked(CharacterConfigData configData)
    {
        int index = charactersDataBase.GetIndexOfTheCharacterFromItsConfigData(configData);

        return isCharacterUnlocked(index);
    }

    public bool UnlockCharacter(int characterIndex, bool raiseSpecificCharacterUnlockedEvent = true, bool raiseInventoryHasUpdatedEvent = true)
    {
        if (isCharacterUnlocked(characterIndex))
            return false;

        CharacterConfigData config = charactersDataBase.GetCharacterConfigurationData(characterIndex);
        int reqFigurine = config.FigurinesToUnlock;
        CharactersFigurinesAvailable[characterIndex] = reqFigurine;

        CharacterHasBeenUnlocked?.RaiseEvent();

        if(raiseInventoryHasUpdatedEvent)
        {
            inventoryHasBeenUpdated.RaiseEvent();
        }

        if(raiseSpecificCharacterUnlockedEvent)
        {
            OnCharactersUnlocked?.Invoke(new List<PlayableObjectDataWithIndex>() { new PlayableObjectDataWithIndex(config, characterIndex) });
        }

        return true;
    }

    public bool UnlockCharacter(CharacterConfigData configData, bool raiseSpecificCharacterUnlockedEvent = true, bool raiseInventoryHasUpdatedEvent = true)
    {
        int index = charactersDataBase.GetIndexOfTheCharacterFromItsConfigData(configData);
        return UnlockCharacter(index, raiseSpecificCharacterUnlockedEvent, raiseInventoryHasUpdatedEvent);
    }

    public void UnlockCharacters(int[] charactersToUnlock)
    {
        List<PlayableObjectDataWithIndex> charactersList = new List<PlayableObjectDataWithIndex>();
        
        for (int i = 0; i < charactersToUnlock.Length; i++)
        {
            int characterIndex = charactersToUnlock[i];
            bool success = UnlockCharacter(characterIndex, false, false);

            if (success)
            {
                charactersList.Add(new PlayableObjectDataWithIndex(carsDataBase.GetCarConfigurationData(characterIndex), characterIndex));
            }
        }

        inventoryHasBeenUpdated.RaiseEvent();
        OnCharactersUnlocked?.Invoke(charactersList);
    }

    public void UnlockAllCharacters()
    {
        List<PlayableObjectDataWithIndex> charactersList = new List<PlayableObjectDataWithIndex>();

        foreach (var pair in charactersDataBase.CharactersDataBaseDictionary)
        {
            if (!isCharacterUnlocked(pair.Key))
            {
                charactersList.Add(new PlayableObjectDataWithIndex(pair.Value, pair.Key));
            }
        }

        for (int i = 0; i < CharactersFigurinesAvailable.Count; i++)
        {
            var pair = CharactersFigurinesAvailable.ElementAt(i);
            CharactersFigurinesAvailable[pair.Key] = charactersDataBase.GetCharacterConfigurationData(pair.Key).FigurinesToUnlock;
        }

        inventoryHasBeenUpdated.RaiseEvent();
        OnCharactersUnlocked?.Invoke(charactersList);
    }

    public bool AreAllCharactersUnlocked()
    {
       foreach(var pair in charactersDataBase.CharactersDataBaseDictionary)
        {
            bool isLocked = !isCharacterUnlocked(pair.Key);

            if (isLocked)
                return false;
        }

        return true;
    }


    #endregion
}
