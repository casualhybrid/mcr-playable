using System;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System.Collections;
using Firebase.Analytics;
using System.Collections.Generic;
using AppsFlyerSDK;

public class AppOpenAdController : MonoBehaviour
{
    public static AppOpenAdController instance;
    public  Action AdmobSdkInit;
#if UNITY_ANDROID
    private string _adUnitId = "ca-app-pub-1919652342336147/5359299507";
#elif UNITY_IPHONE
        private const string _adUnitId = "ca-app-pub-3940256099942544/5575463023";
#else
        private const string _adUnitId = "unused";
#endif

    private readonly TimeSpan TIMEOUT = TimeSpan.FromHours(4);
    private DateTime _expireTime;
    private AppOpenAd _appOpenAd;

    
    public void Awake()
    {
        instance = this;
        AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
      //  AdmobSdkInit += InitSdk;
    }
    public void Start()
    {
        Invoke(nameof(InitSdk), 0.3f);
    }
    public void InitSdk()
    {
        MobileAds.Initialize(HandleInitCompleteAction);
    }

    private void HandleInitCompleteAction(InitializationStatus initstatus)
    {
        try
        {
            MobileAdsEventExecutor.ExecuteInUpdate(() =>
            {
                Debug.LogError("ADmobSDKInit.................");

                StartCoroutine(InitializeFullScreenAds());

            });
        }
        catch (Exception e)
        {
            Debug.LogError(e + "CatchInit.........");
        }
    }
    public IEnumerator InitializeFullScreenAds()
    {
        yield return new WaitForSeconds(0.1f);
        LoadAd();
        yield return new WaitForSeconds(6);
        CheckAppopenStatus();

    }
    public void CheckAppopenStatus()
    {
        if (RemoteConfigManager.ShowOpenAds == false)
        {
            return;
        }
        ShowAd();
    }

    public void LoadAd()
    {
        if (_appOpenAd != null)
        {
            DestroyAd();
        }

        Debug.Log("Loading app open ad.");

        var adRequest = new AdRequest();

        AppOpenAd.Load(_adUnitId, adRequest, (AppOpenAd ad, LoadAdError error) =>
        {
            RegisterEventHandlers(ad);
            if (error != null)
            {
                Debug.LogError("App open ad failed to load an ad with error : "
                                + error);
                return;
            }
            if (ad == null)
            {
                Debug.LogError("Unexpected error: App open ad load event fired with " +
                               " null ad and null error.");
                return;
            }
            Debug.Log("App open ad loaded with response : " + ad.GetResponseInfo());
            _appOpenAd = ad;

            _expireTime = DateTime.Now + TIMEOUT;

        });
    }
    int index = 0;
    public void ShowAd()
    {
        bool isFirstOpen = !PlayerPrefs.HasKey("first_open_done");

        if (isFirstOpen)
        {
            PlayerPrefs.SetInt("first_open_done", 1); // Mark that we've opened the app at least once
            PlayerPrefs.Save();

            if (!RemoteConfigManager.ShowOpenAdsFirstOpen)
            {
                Debug.Log("[AppOpenAd] Skipping ad on first open (per Remote Config).");
                return;
            }
            else
            {
                Debug.Log("[AppOpenAd] Showing ad on first open (per Remote Config).");
            }
        }
        else
        {
            Debug.Log("[AppOpenAd] Not first open — always show.");
        }

        // Show ad if ready
        if (_appOpenAd != null && _appOpenAd.CanShowAd())
        {
            Debug.Log("[AppOpenAd] Showing app open ad.");
            _appOpenAd.Show();
        }
        else
        {
            Debug.LogWarning("[AppOpenAd] Ad is not ready yet.");
        }

    }

    public void DestroyAd()
    {
        if (_appOpenAd != null)
        {
            Debug.Log("Destroying app open ad.");
            _appOpenAd.Destroy();
            _appOpenAd = null;
        }

    }

    public void LogResponseInfo()
    {
        if (_appOpenAd != null)
        {
            var responseInfo = _appOpenAd.GetResponseInfo();
            UnityEngine.Debug.Log(responseInfo);
        }
    }
    int Index = 0;
    private void OnAppStateChanged(AppState state)
    {
        Index++;
        Debug.Log("App State changed to : " + state);

        if (state == AppState.Foreground)
        {
            ShowAd();
        }
    }

    private void RegisterEventHandlers(AppOpenAd ad)
    {
        ad.OnAdPaid += (AdValue adValue) =>
        {
           
            double revenue = adValue.Value / 1000000.0;
            FirebaseAnalytics.LogEvent("appopen_ad_revenue", new Parameter[] {
                new Parameter("revenue", revenue),
                new Parameter("currency_code", adValue.CurrencyCode)
            });

            Dictionary<string, string> additionalParams = new Dictionary<string, string>();
            additionalParams.Add(AdRevenueScheme.AD_TYPE, "appopen");
            var logRevenue = new AFAdRevenueData("appopen_ad_revenue", MediationNetwork.GoogleAdMob, "USD", revenue);
          AppsFlyer.logAdRevenue(logRevenue, additionalParams);

        };
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("App open ad recorded an impression.");
        };
        ad.OnAdClicked += () =>
        {
            Debug.Log("App open ad was clicked.");
        };
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("App open ad full screen content opened.");
        };

        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("App open ad full screen content closed."); 

            LoadAd();
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            LoadAd();
            Debug.LogError("App open ad failed to open full screen content with error : "
                            + error);
        };
    }
}

