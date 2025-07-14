using System.Collections;
using System.Collections.Generic;
using TheKnights.Purchasing;
using TheKnights.SaveFileSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "UnlockCharactersIAPItem", menuName = "ScriptableObjects/InAppPurchasing/UnlockCharactersIAPItem")]
public class UnlockCharactersIAPItem : IAPItem
{
    [SerializeField] private InventorySystem playerInventory;
    // [SerializeField] private GameEvent playerBoughtAllCharacters;

    public override bool ShouldTheInAppOfferBeShown()
    {
        return !playerInventory.AreAllCharactersUnlocked();
    }

    public override void ProcessPurchaseCompletion()
    {
        RaiseRightBeforePurchaseCompleted();

        playerInventory.UnlockAllCharacters();

        RaisePurchaseCompletionEvent();

        //  playerBoughtAllCharacters.RaiseEvent();
    }
}
