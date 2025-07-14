using UnityEngine;
using Knights.UISystem;
using deVoid.UIFramework;
using TheKnights.SaveFileSystem;
using System.Collections.Generic;

public class BuyBoostOnPausePanel : MonoBehaviour
{
    [SerializeField] private AWindowController aWindowController;
    [SerializeField] private SaveManager saveManagerObj;
    [SerializeField] private InventorySystem inventoryObj;
    [SerializeField] private ShopPurchaseEvent purchaseEvent;

    public void BuyBoostWithCoins()
    {
        bool isPurchaseble = inventoryObj.GetIntKeyValue("AccountCoins") >= 100;
        List<string> thingsGot = new List<string>();//z
        List<int> amountsGot = new List<int>();//z
        if (isPurchaseble)
        {
            List<InventoryItem<int>> inventoryItems = new List<InventoryItem<int>>();


            inventoryItems.Add(new InventoryItem<int>("AccountCoins", -100));

            inventoryItems.Add(new InventoryItem<int>("GameBoost", 1, true));

            thingsGot.Add("GameBoost");
            amountsGot.Add(1);

            inventoryObj.UpdateKeyValues(inventoryItems);
        }
        else
        {
            print("Item is not purchaseable...");

            aWindowController.OpenTheWindow(ScreenIds.ResourcesNotAvailable);
        }

        purchaseEvent.RaiseEvent(thingsGot, "AccountCoins", 100, amountsGot);

    }

        public void BuyBoostWithAD()
        {

                List<InventoryItem<int>> inventoryItems = new List<InventoryItem<int>>();

                inventoryItems.Add(new InventoryItem<int>("GameBoost", 1));

                inventoryObj.UpdateKeyValues(inventoryItems);

        }

    }
