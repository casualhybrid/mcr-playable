using System.Collections;
using System.Collections.Generic;
using TheKnights.Purchasing;
using TheKnights.SaveFileSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "UnlockCarsIAPItem", menuName = "ScriptableObjects/InAppPurchasing/UnlockCarsIAPItem")]
public class UnlockCarsIAPItem : IAPItem
{
    [SerializeField] private InventorySystem playerInventory;
    [SerializeField] private GameEvent playerBoughtAllCars;

    public override bool ShouldTheInAppOfferBeShown()
    {
        return !playerInventory.AreAllCarsUnlocked();
    }

    public override void ProcessPurchaseCompletion()
    {
        RaiseRightBeforePurchaseCompleted();

        playerInventory.UnlockAllCars();
        MATS_CheckIAP.allCarsUnlocked = true;
        playerBoughtAllCars.RaiseEvent();

        RaisePurchaseCompletionEvent();
    }
}
