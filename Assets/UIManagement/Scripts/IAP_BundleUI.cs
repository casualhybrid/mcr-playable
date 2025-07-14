using deVoid.UIFramework;
using Knights.UISystem;
using System;
using System.Collections.Generic;
using TheKnights.Purchasing;
using TheKnights.SaveFileSystem;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAP_BundleUI : MonoBehaviour
{
    [SerializeField] private AWindowController aWindowController;
    [SerializeField] private SaveManager saveManagerObj;
    [SerializeField] private IAPManager iAPManagerObj;
    [SerializeField] private InventorySystem inventoryObj;
  //  [SerializeField] private string inAppIdStr = "com_minicarrush_iapbundle_1";
    [SerializeField] private TextMeshProUGUI textObj;
    [SerializeField] private ShopPurchaseEvent purchaseEvent;

    private Action pendingPurchasedItemResult;

    private void OnEnable()
    {
        IAPManager.OnIAPInitializationFailed.AddListener(ChangeFeedBackText);
        IAPManager.OnIAPInitialized.AddListener(InAppInitialization);
    }

    private void OnDisable()
    {
        IAPManager.OnIAPInitializationFailed.RemoveListener(ChangeFeedBackText);
        IAPManager.OnIAPInitialized.RemoveListener(InAppInitialization);
        DeSubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        IAPManager.OnPurchaseFailed.AddListener(PurchaseFailed);
        IAPManager.OnPurchaseSuccessfull.AddListener(PurchaseSuccessful);
    }

    private void DeSubscribeToEvents()
    {
        IAPManager.OnPurchaseFailed.RemoveListener(PurchaseFailed);
        IAPManager.OnPurchaseSuccessfull.RemoveListener(PurchaseSuccessful);
    }

    /// <summary>
    ///     This function is solo implementation for Shop Panel in Main Menu... and needed to be reset
    /// </summary>
    /// <param name="itemObj"></param>
    public void BuySelectedBundleWithMoney(GeneralIAPItem itemObj)
    {
        SubscribeToEvents();

        List<string> thingsGot = new List<string>();//z
        List<int> amountsGot = new List<int>();//z

        for (int i = 0; i < itemObj.inventoryItemsToGive.Length; i++)
        {
            ItemWithAmount itemWithAmount = itemObj.inventoryItemsToGive[i];
            thingsGot.Add(itemWithAmount.GetItemType.GetKey);//z
            amountsGot.Add(itemWithAmount.GetAmount);//z
        }

        pendingPurchasedItemResult = () =>
        {
            purchaseEvent.RaiseEvent(thingsGot, "Dollars", itemObj.ProductID[itemObj.ProductID.Length - 1], amountsGot);
        };

        iAPManagerObj.BuyTheProduct(itemObj.ProductID);

    }

    private void PurchaseSuccessful()
    {
        DeSubscribeToEvents();

        pendingPurchasedItemResult?.Invoke();
    }

    private void PurchaseFailed(PurchaseFailureReason reason)
    {
        DeSubscribeToEvents();
       // aWindowController.OpenTheWindow(ScreenIds.ResourcesNotAvailable);

       // pendingPurchasedItemResult?.Invoke();
    }

    private void InAppInitialization()
    {
        //  textObj.text = iAPManagerObj.GetlocalizedPriceString(inAppIdStr);
    }

    private void ChangeFeedBackText()
    {
       // print("IAP Status = " + "Initialization Failed");
    }
}