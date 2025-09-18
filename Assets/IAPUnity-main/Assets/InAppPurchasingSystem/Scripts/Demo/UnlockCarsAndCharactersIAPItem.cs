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
        MATS_CheckIAP.allCarsUnlocked = true;
        MATS_CheckIAP.allCharactersUnlocked = true;
        MATS_CheckIAP.allGame = true;
        PlayerPrefs.SetInt("IsAdsRemoved", 1);
        playerBoughtAllCars.RaiseEvent();
        playerInventory.UnlockAllCharacters();
        RaisePurchaseCompletionEvent();
    }
}
