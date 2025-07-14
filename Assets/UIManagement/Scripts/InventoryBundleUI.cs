using UnityEngine;
using Knights.UISystem;
using deVoid.UIFramework;
using TheKnights.SaveFileSystem;
using System.Collections.Generic;

public class InventoryBundleUI : MonoBehaviour
{
    [SerializeField] private AWindowController aWindowController;
    [SerializeField] private SaveManager saveManagerObj;
    [SerializeField] private InventorySystem inventoryObj;
    [SerializeField] private ShopPurchaseEvent purchaseEvent;

    /// <summary>
    ///     This function is solo implementation for Shop Panel in Main Menu... and needed to be reset
    /// </summary>
    /// <param name="itemObj"></param>
    public void BuySelectedBundleWithDiamonds(GeneralIAPItem itemObj)
    {
        List<InventoryItem<int>> inventoryItems = new List<InventoryItem<int>>();
        List<string> thingsGot = new List<string>();//z
        List<int> amountsGot = new List<int>();//z

        bool isPurchaseble = false;
        foreach (ItemWithAmount itemWithPrice in itemObj.inventoryItemsPrice)
        {
            if (itemWithPrice.GetAmount > inventoryObj.GetIntKeyValue("AccountDiamonds"))
            {
                isPurchaseble = false;
                break;
            }
            isPurchaseble = true;
        }

        if (isPurchaseble)
        {

            for (int i = 0; i < itemObj.inventoryItemsToGive.Length; i++)
            {
                ItemWithAmount itemWithAmount = itemObj.inventoryItemsToGive[i];
                inventoryItems.Add(new InventoryItem<int>(itemWithAmount.GetItemType.GetKey, itemWithAmount.GetAmount,true));
                thingsGot.Add(itemWithAmount.GetItemType.GetKey);//z
                amountsGot.Add(itemWithAmount.GetAmount);//z
            }


            int decreaseDiamonds = /*inventoryObj.GetIntKeyValue("AccountDiamonds") - */itemObj.inventoryItemsPrice[0].GetAmount;
            inventoryItems.Add(new InventoryItem<int>("AccountDiamonds", -decreaseDiamonds));


            inventoryObj.UpdateKeyValues(inventoryItems);
        }
        else
        {
            print("Item is not purchaseable...");

            aWindowController.OpenTheWindow(ScreenIds.ResourcesNotAvailable);
        }

        purchaseEvent.RaiseEvent(thingsGot, "AccountDiamonds", itemObj.inventoryItemsPrice[0].GetAmount, amountsGot);
    }
   

}
