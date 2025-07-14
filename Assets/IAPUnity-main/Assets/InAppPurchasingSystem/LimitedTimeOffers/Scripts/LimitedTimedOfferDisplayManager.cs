using System;
using System.Collections.Generic;
using TheKnights.SaveFileSystem;
using UnityEngine;

public class LimitedTimedOfferDisplayManager : MonoBehaviour
{
    [SerializeField] private LimitedTimeOffersData limitedTimeOffersData;
    [SerializeField] private UIAdditiveSceneUIController uIAdditiveSceneUIController;
    [SerializeField] private SaveManager saveManager;

    private static bool shownOnceThisSession;

    private void Start()
    {
        if (shownOnceThisSession)
            return;

        if (!saveManager.MainSaveFile.TutorialHasCompleted)
            return;

      //  ShowRandomEligibleOffer();
    }

    //private void DisplayTheOfferIfEligible(LimitedTimeOffer offer, DateTime currentTime)
    //{
    //    bool isEligible = offer.GetIAPItem.ShouldTheInAppOfferBeShown();

    //    if (!isEligible)
    //        return;

    //    bool hasOpenedBefore = PlayerPrefs.HasKey(offer.ScreenName);

    //    if (!hasOpenedBefore)
    //    {
    //        SetOfferViewerAtTimeAndShowScreen(offer);
    //        return;
    //    }

    //    string firstOpenedTimeStr = PlayerPrefs.GetString(offer.ScreenName);
    //    DateTime firstOpenedDateTime;

    //    bool success = DateTime.TryParse(firstOpenedTimeStr, out firstOpenedDateTime);

    //    if (success)
    //    {
    //        TimeSpan remainingTimeSpan = currentTime.Subtract(firstOpenedDateTime);

    //        if (remainingTimeSpan >= offer.GetOfferTime)
    //            return;

    //        uIAdditiveSceneUIController.ShowWindowScreen(offer.ScreenName, ScreenOperation.Open, new LimitedTimeOfferUIProperties(offer.GetOfferTime - remainingTimeSpan));
    //    }
    //    else
    //    {
    //        UnityEngine.Console.LogWarning($"Failed to parse the viewed date for limited timed offer {offer.ScreenName}");
    //        SetOfferViewerAtTimeAndShowScreen(offer);
    //    }
    //}

    private void ShowRandomEligibleOffer()
    {
        LimitedTimeOffer[] offers = limitedTimeOffersData.LimitedTimeOffers;

        if (offers.Length == 0)
            return;

        DateTime currentTime = DateTime.Now;
        List<LimitedTimeOffer> eligibleOffers = new List<LimitedTimeOffer>();

        for (int i = 0; i < offers.Length; i++)
        {
            LimitedTimeOffer offer = offers[i];
            bool isEligible = offer.GetIAPItem.ShouldTheInAppOfferBeShown() && GetTheRemainingOfferTime(offer, currentTime) > TimeSpan.Zero;

            if (isEligible)
            {
                eligibleOffers.Add(offer);
            }
        }

        if (eligibleOffers.Count == 0)
            return;

        int rand = UnityEngine.Random.Range(0, eligibleOffers.Count);

        LimitedTimeOffer offerToDisplay = eligibleOffers[rand];
        DisplayTheOffer(offerToDisplay, currentTime);

        shownOnceThisSession = true;
    }

    private void DisplayTheOffer(LimitedTimeOffer offer, DateTime currentTime)
    {
        bool hasOpenedBefore = PlayerPrefs.HasKey(offer.ScreenName);

        if (!hasOpenedBefore)
        {
            PlayerPrefs.SetString(offer.ScreenName, System.DateTime.Now.ToString());
        }

        uIAdditiveSceneUIController.ShowWindowScreen(offer.ScreenName, ScreenOperation.Open, new LimitedTimeOfferUIProperties(GetTheRemainingOfferTime(offer, currentTime)));
    }

    private TimeSpan GetTheRemainingOfferTime(LimitedTimeOffer offer, DateTime currentTime)
    {
        bool hasOpenedBefore = PlayerPrefs.HasKey(offer.ScreenName);

        if (!hasOpenedBefore)
        {
            return offer.GetOfferTime;
        }

        string firstOpenedTimeStr = PlayerPrefs.GetString(offer.ScreenName);
        DateTime firstOpenedDateTime;

        bool success = DateTime.TryParse(firstOpenedTimeStr, out firstOpenedDateTime);

        if (success)
        {
            TimeSpan remainingTimeSpan = currentTime.Subtract(firstOpenedDateTime);
            return offer.GetOfferTime.Subtract(remainingTimeSpan);
        }
        else
        {
            UnityEngine.Console.LogWarning($"Failed to parse the viewed date for limited timed offer {offer.ScreenName}");
            return offer.GetOfferTime;
        }
    }

}