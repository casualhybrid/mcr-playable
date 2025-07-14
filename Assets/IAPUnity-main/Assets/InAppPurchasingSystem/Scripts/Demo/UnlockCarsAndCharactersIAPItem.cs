using System.Collections;
using System.Collections.Generic;
using TheKnights.Purchasing;
using TheKnights.SaveFileSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "UnlockCarsAndCharactersIAPItem", menuName = "ScriptableObjects/InAppPurchasing/UnlockCarsAndCharactersIAPItem")]
public class UnlockCarsAndCharactersIAPItem : IAPItem
{
    [SerializeField] private InventorySystem playerInventory;
    [SerializeField] private GameEvent playerBoughtAllCars;

    public override bool ShouldTheInAppOfferBeShown()
    {
        return !playerInventory.AreAllCarsUnlocked() || !playerInventory.AreAllCharactersUnlocked();
    }

    public override void ProcessPurchaseCompletion()
    {
        RaiseRightBeforePurchaseCompleted();

        playerInventory.UnlockAllCars();
        playerBoughtAllCars.RaiseEvent();
        playerInventory.UnlockAllCharacters();
        RaisePurchaseCompletionEvent();
    }
}
