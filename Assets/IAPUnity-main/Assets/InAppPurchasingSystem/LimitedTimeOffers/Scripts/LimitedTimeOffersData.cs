using System;
using System.Collections;
using System.Collections.Generic;
using TheKnights.Purchasing;
using UnityEngine;
using deVoid.UIFramework;
using Firebase.Firestore;

[System.Serializable]
public struct LimitedTimeOffer
{
    [SerializeField] private IAPItem iapItem;
    [SerializeField] private string screenName;
    [SerializeField] private int offerDays;
    [SerializeField] private int offerHours;
    [SerializeField] private int offerMint;

    public string ScreenName => screenName;
    public TimeSpan GetOfferTime => new TimeSpan(offerDays, offerHours, offerMint, 0);
    public IAPItem GetIAPItem => iapItem;
}

    [CreateAssetMenu(fileName = "LimitedTimeOffersData", menuName = "ScriptableObjects/InAppPurchasing/LimitedTimeOffersData", order = 1)]
public class LimitedTimeOffersData : ScriptableObject
{
    [SerializeField] private LimitedTimeOffer[] limitedTimeOffers;

    [SerializeField] private UISettings additiveUISettings;
    [SerializeField] private UISettings gameplayUISettings;

    public LimitedTimeOffer[] LimitedTimeOffers => limitedTimeOffers;

    public LimitedTimeOffer GetLimitedTimedOffer(string screenID)
    {
        for (int i = 0; i < limitedTimeOffers.Length; i++)
        {
            LimitedTimeOffer limitedTimeOffer = limitedTimeOffers[i];

            if(limitedTimeOffer.ScreenName == screenID)
                return limitedTimeOffer;
        }

        throw new Exception($"Failed to find limited timed offer for screen ID {screenID}");
    }

    private void OnValidate()
    {
        foreach (var limitedOffer in limitedTimeOffers)
        {
           bool exisits = additiveUISettings.DoesThisScreenExists(limitedOffer.ScreenName) ||
               gameplayUISettings.DoesThisScreenExists(limitedOffer.ScreenName);

            if(!exisits) {

                Debug.LogError($"Limited time offer {limitedOffer.ScreenName} screen does not exist in either additive or gameplay ui settings");
            }
        }
    }
}
