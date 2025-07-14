using System;
using GoogleMobileAds.Api;
using UnityEngine;
public class AppOpenAd : MonoBehaviour
{
    //[SerializeField] private string adID;
    //private GoogleMobileAds.Api.AppOpenAd ad;
    //private bool isShowingAd = false;

    //private bool IsAdAvailable
    //{
    //    get
    //    {
    //        return ad != null;
    //    }
    //}

    //private void Start()
    //{
    //    // Load an app open ad when the scene starts
    //     LoadAd();

    //    // Listen to application foreground and background events.
    //    AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
    //}
    //private void OnAppStateChanged(AppState state)
    //{
    //    // Display the app open ad when the app is foregrounded.
    //    UnityEngine.UnityEngine.Console.Log("App State is " + state);
    //    if (state == AppState.Foreground)
    //    {
    //        AppOpenAdManager.Instance.ShowAdIfAvailable();
    //    }
    //}


    //public void LoadAd()
    //{
    //    AdRequest request = new AdRequest.Builder().Build();

    //    // Load an app open ad for portrait orientation
    //    GoogleMobileAds.Api.AppOpenAd.LoadAd(adID, ScreenOrientation.Portrait, request, ((appOpenAd, error) =>
    //    {
    //        if (error != null)
    //        {
    //            // Handle the error.
    //            Debug.LogFormat("Failed to load the ad. (reason: {0})", error.LoadAdError.GetMessage());
    //            return;
    //        }

    //        // App open ad is loaded.
    //        ad = appOpenAd;
    //    }));
    //}

    //public void ShowAdIfAvailable()
    //{
    //    if (!IsAdAvailable || isShowingAd)
    //    {
    //        return;
    //    }

    //    ad.OnAdDidDismissFullScreenContent += HandleAdDidDismissFullScreenContent;
    //    ad.OnAdFailedToPresentFullScreenContent += HandleAdFailedToPresentFullScreenContent;
    //    ad.OnAdDidPresentFullScreenContent += HandleAdDidPresentFullScreenContent;
    //    ad.OnAdDidRecordImpression += HandleAdDidRecordImpression;
    //    ad.OnPaidEvent += HandlePaidEvent;

    //    ad.Show();
    //}

    //private void HandleAdDidDismissFullScreenContent(object sender, EventArgs args)
    //{
    //    UnityEngine.Console.Log("Closed app open ad");
    //    // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
    //    ad = null;
    //    isShowingAd = false;
    //    LoadAd();
    //}

    //private void HandleAdFailedToPresentFullScreenContent(object sender, AdErrorEventArgs args)
    //{
    //    Debug.LogFormat("Failed to present the ad (reason: {0})", args.AdError.GetMessage());
    //    // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
    //    ad = null;
    //    LoadAd();
    //}

    //private void HandleAdDidPresentFullScreenContent(object sender, EventArgs args)
    //{
    //    UnityEngine.Console.Log("Displayed app open ad");
    //    isShowingAd = true;
    //}

    //private void HandleAdDidRecordImpression(object sender, EventArgs args)
    //{
    //    UnityEngine.Console.Log("Recorded ad impression");
    //}

    //private void HandlePaidEvent(object sender, AdValueEventArgs args)
    //{
    //    Debug.LogFormat("Received paid event. (currency: {0}, value: {1}",
    //            args.AdValue.CurrencyCode, args.AdValue.Value);
    //}
}
