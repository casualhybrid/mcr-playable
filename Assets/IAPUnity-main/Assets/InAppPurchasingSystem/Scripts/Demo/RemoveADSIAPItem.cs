using System.Collections;
using System.Collections.Generic;
using TheKnights.Purchasing;
using TheKnights.SaveFileSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "RemoveADSIAPItem", menuName = "ScriptableObjects/InAppPurchasing/RemoveADSIAPItem")]
public class RemoveADSIAPItem : IAPItem
{
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private GameEvent onPlayerBoughtAds;

    public override bool ShouldTheInAppOfferBeShown()
    {
        return !saveManager.MainSaveFile.isAdsPurchased;
    }

    public override void ProcessPurchaseCompletion()
    {
        RaiseRightBeforePurchaseCompleted();
        // saveManager.MainSaveFile.isAdsPurchased = true;
        PlayerPrefs.SetInt("IsAdsRemoved", 1);
        
        onPlayerBoughtAds.RaiseEvent();
        RaisePurchaseCompletionEvent();
    }
}
