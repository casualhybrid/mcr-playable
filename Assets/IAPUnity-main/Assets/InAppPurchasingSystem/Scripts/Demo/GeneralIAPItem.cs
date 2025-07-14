using Sirenix.OdinInspector;
using System.Collections.Generic;
using TheKnights.Purchasing;
using UnityEngine;

[System.Serializable]
public struct ItemWithAmount
{
    [SerializeField] private InventoryItemSO itemType;
    [SerializeField] private int amount;

    /// <summary>
    /// The Type Of Inventory Item
    /// </summary>
    public InventoryItemSO GetItemType => itemType;

    /// <summary>
    /// The Value Of Inventory Item
    /// </summary>
    public int GetAmount => amount;
}

[CreateAssetMenu(fileName = "GeneralIAPItem", menuName = "ScriptableObjects/InAppPurchasing/GeneralIAPItem", order = 1)]
public class GeneralIAPItem : IAPItem
{
   // public string inAppIdStr;

    [SerializeField] private InventorySystem playerInventory;

    [ToggleGroup("EnableInventoryItems")]
    [SerializeField] private bool EnableInventoryItems;

    [ToggleGroup("EnableInventoryItems")]
    public ItemWithAmount[] inventoryItemsToGive;

    [ToggleGroup("EnableInventoryItems")]
    public ItemWithAmount[] inventoryItemsPrice;

    [ToggleGroup("EnableCars")]
    [SerializeField] private bool EnableCars;

    [ToggleGroup("EnableCharacters")]
    [SerializeField] private bool EnableCharacters;

    [ToggleGroup("EnableCars")]
    [SerializeField] private CarConfigurationData[] carsToUnlock;

    [ToggleGroup("EnableCharacters")]
    [SerializeField] private CharacterConfigData[] charactersToUnlock;

    public override void ProcessPurchaseCompletion()
    {
        RaiseRightBeforePurchaseCompleted();

        if (EnableInventoryItems)
        {
            List<InventoryItem<int>> inventoryItems = new List<InventoryItem<int>>();

            for (int i = 0; i < inventoryItemsToGive.Length; i++)
            {
                ItemWithAmount itemWithAmount = inventoryItemsToGive[i];
                int amountToAdd = itemWithAmount.GetAmount;

             //   UnityEngine.Console.Log($"Adding {itemWithAmount.GetItemType.name} of amount {amountToAdd}");

                // Add To Inventory

                inventoryItems.Add(new InventoryItem<int>(itemWithAmount.GetItemType.GetKey, amountToAdd,true));
            }

            if (inventoryItems != null && inventoryItems.Count > 0)
            {
                playerInventory.UpdateKeyValues(inventoryItems,true,true,null,false);
            }
       
        }

        if (EnableCars)
        {
            for (int i = 0; i < carsToUnlock.Length; i++)
            {
                // Unlock Any Cars here through inventory or whatever!
                playerInventory.UnlockCar(carsToUnlock[i]);
            }
        }

        if(EnableCharacters)
        {
            for (int i = 0; i < charactersToUnlock.Length; i++)
            {
                // Unlock Any Character here through inventory or whatever!
                playerInventory.UnlockCharacter(charactersToUnlock[i]);
            }
        }

        RaisePurchaseCompletionEvent();
    }

    public override bool ShouldTheInAppOfferBeShown()
    {
        if (productType == UnityEngine.Purchasing.ProductType.Consumable)
            return true;

        bool shouldShowOffer = false;

       if(EnableCars)
        {
            for (int i = 0; i < carsToUnlock.Length; i++)
            {
                CarConfigurationData config = carsToUnlock[i];
                bool isAlreadyUnlocked = playerInventory.isCarUnlocked(config);
                if(!isAlreadyUnlocked)
                {
                    shouldShowOffer = true;
                    break;
                }
            }
        }

        // for characters
        if(EnableCharacters && !shouldShowOffer)
        {
            for (int i = 0; i < charactersToUnlock.Length; i++)
            {
                CharacterConfigData config = charactersToUnlock[i];
                bool isAlreadyUnlocked = playerInventory.isCharacterUnlocked(config);
                if (!isAlreadyUnlocked)
                {
                    shouldShowOffer = true;
                    break;
                }
            }

        }

        Console.Log($"Should the offer {this.name} be shown?: {shouldShowOffer}");
        return shouldShowOffer;
    }
}