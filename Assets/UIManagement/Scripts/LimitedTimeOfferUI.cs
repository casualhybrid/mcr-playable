using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using deVoid.UIFramework;
using TMPro;
using System;
using TheKnights.Purchasing;
using UnityEngine.Purchasing;

[System.Serializable]
public class LimitedTimeOfferUIProperties : WindowProperties
{
    public readonly TimeSpan RemainingTimeSpan;
    
    public LimitedTimeOfferUIProperties(TimeSpan _remainingTimeSpan)
    {
        RemainingTimeSpan = _remainingTimeSpan;
    }
}


public class LimitedTimeOfferUI : AWindowController<LimitedTimeOfferUIProperties>
{

    [SerializeField] private TextMeshProUGUI remainingTimeText;
    [SerializeField] private LimitedTimeOffersData limitedTimeOffersData;
    [SerializeField] private IAPManager iapManager;
    [SerializeField] private TextMeshProUGUI priceText;

    private IAPItem iapItem;

    private void OnEnable()
    {
        iapItem = limitedTimeOffersData.GetLimitedTimedOffer(ScreenId).GetIAPItem;
        SetLocalizedPriceText(iapItem);

        iapItem.OnRightBeforePurchaseCompleted += UI_Close;
        IAPManager.OnPurchaseFailed.AddListener(CloseTheWindow);
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void UnsubscribeEvents()
    {
        iapItem.OnRightBeforePurchaseCompleted -= UI_Close;
        IAPManager.OnPurchaseFailed.RemoveListener(CloseTheWindow);
    }

    protected override void WhileHiding()
    {
        base.WhileHiding();
        UnsubscribeEvents();
    }

    protected override void OnPropertiesSet()
    {
        base.OnPropertiesSet();

        TimeSpan remainingTime = Properties.RemainingTimeSpan;
        remainingTimeText.text = remainingTime.Days + "D " + remainingTime.Hours + "H " + remainingTime.Minutes + "M";
    }

    public void BuyThisProduct()
    {
       LimitedTimeOffer offer = limitedTimeOffersData.GetLimitedTimedOffer(ScreenId);
       iapManager.BuyTheProduct(offer.GetIAPItem.ProductID);
    }

    public void SetLocalizedPriceText(IAPItem iapItem)
    {
       string priceTxt = iapManager.GetlocalizedPriceString(iapItem);

        if (priceTxt == null)
            return;

        priceText.text = priceTxt;
    }

    private void CloseTheWindow(PurchaseFailureReason reason)
    {
        UI_Close();
    }

}
