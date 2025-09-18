using AppsFlyerSDK;
using deVoid.UIFramework;
using GoogleMobileAds.Api;
using Knights.UISystem;
using System;
using System.Collections;
using System.Collections.Generic;
using TheKnights.AdsSystem;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Purchasing;
using UnityEngine.UI;
using static MaxSdkBase;

public class MaxAdMobController : MonoBehaviour
{
    public string MaxSdkKey = "ENTER_MAX_SDK_KEY_HERE";
    public string InterstitialAdUnitId = "ENTER_INTERSTITIAL_AD_UNIT_ID_HERE";
    public string RewardedAdUnitId = "ENTER_REWARD_AD_UNIT_ID_HERE";
    public string RewardedInterstitialAdUnitId = "ENTER_REWARD_INTER_AD_UNIT_ID_HERE";
    public string BannerAdUnitId = "ENTER_BANNER_AD_UNIT_ID_HERE";
    public string MRecAdUnitId = "ENTER_MREC_AD_UNIT_ID_HERE";
    public string AppOpenAdUnitId = "«Android-ad-unit-ID»";
    public static MaxAdMobController Instance;
    private bool isBannerShowing;
    private bool isMRecShowing;
    public bool firebaseReady;
    private int interstitialRetryAttempt;
    private int rewardedRetryAttempt;
    private int rewardedInterstitialRetryAttempt;
    public bool isComPauseAds, isComMainMenuAd, isSurPauseAds, isSurMainMenuAd, isMultiPauseAds, isMultiMainMenuAd;
    public bool isOption, isbacktoModeSelection, isBacktoWeapontoMainmenu;
    [HideInInspector] public bool DailyRewardScreen;
    public int AdCounter { get; internal set; }
    public UnityEvent OnUserEarnedRewardEvent;
    public bool islevelNext { get; internal set; }
    public int MedCount;
    public static int Adcount;
    public static int RemoveAdsCount;
    public bool RandomLevel = false;
    public int InAppsCount;
    public bool Backfromhome, modesads;
    private RewardedAdSO _currentRequester;
    string RewardInfo = "";

    public GameObject adsNotAvailibleNoti;

    private float lastInterstitialTime = -999f;


    bool AdsCompability => SystemInfo.systemMemorySize > 1500;
    public bool LowMemoryDevice => SystemInfo.systemMemorySize <= 1500;
    public int Option { get; internal set; }


    public static Action OnVideoAdCompleteReward;


    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        DontDestroyOnLoad(this.gameObject);
    }
    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        MaxSdk.SetHasUserConsent(true);
        MaxSdk.SetDoNotSell(false);
        //Invoke(nameof(PuaseDelay), 0.5f);
        if (PlayerPrefs.GetInt("MaxPrivacyShown", 0) == 0)
        {
            Invoke(nameof(Init), 1.0f);
            Invoke(nameof(DelayToPause), 2f);
        }
        else
        {
            // Just init directly, no pause
            Invoke(nameof(Init), 1.0f);
        }
    }
    public bool IsInternetConnection()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
            return true;
        else
            return false;
    }
    public void PuaseDelay()
    {
        Time.timeScale = 0;
        AudioListener.pause = true;
    }
    public void Init()
    {
        if (!AdsCompability)
        {
            return;
        }
        MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
        {
            // Show Mediation Debugger
            // MaxSdk.ShowMediationDebugger();
        };
        MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration =>
        {
            Time.timeScale = 1;
            AudioListener.pause = false;
            PlayerPrefs.SetInt("PrivacyShownKey", 1);

            StartCoroutine(InitializeFullScreenAds());
            Debug.LogError("MaxSdkInit....................................");

        };
        try
        {
            MaxSdk.InitializeSdk();
        }
        catch (Exception ex)
        {
            Debug.LogError("An error occurred during SDK Init : " + ex.Message);
        }


    }
    public void DelayToPause()
    {

        if (PlayerPrefs.GetInt("PrivacyShownKey") == 1)
        {
            return;
        }
        Time.timeScale = 0f;
        AudioListener.pause = true;
        Debug.LogError("timescale testing");
        PlayerPrefs.SetInt("PrivacyShownKey", 1);
        Debug.LogError("Pref value check " + PlayerPrefs.GetInt("PrivacyShownKey"));
    }
    public IEnumerator InitializeFullScreenAds()
    {
        if (PlayerPrefs.GetInt("IsAdsRemoved") != 1)
        {
            yield return new WaitForSeconds(0.3f);
            RequestBannerAd();
            yield return new WaitForEndOfFrame();
            InitializeInterstitialAds();
        }
        yield return new WaitForEndOfFrame();
        InitializeRewardedAds();

    }

    public void ShowAdIfReady()
    {
        if (MaxSdk.IsAppOpenAdReady(AppOpenAdUnitId))
        {
            MaxSdk.ShowAppOpenAd(AppOpenAdUnitId);
        }
        else
        {
            MaxSdk.LoadAppOpenAd(AppOpenAdUnitId);
        }
    }

    #region Interstitial Ad Methods

    private void InitializeInterstitialAds()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            return;
        }
        // Attach callbacks
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += InterstitialFailedToDisplayEvent;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialDismissedEvent;
        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnInterstitialRevenuePaidEvent;

        // Load the first interstitial
        LoadInterstitial();
    }


    void LoadInterstitial()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            return;
        }
        // interstitialStatusText.text = "Loading...";
        MaxSdk.LoadInterstitial(InterstitialAdUnitId);
    }

    public void ShowInterstitialAd()
    {
        if (PlayerPrefs.GetInt("IsAdsRemoved") == 1 || RemoteConfigManager.HienQc == false)
        {
            return;
        }

        // Check cooldown using Remote Config value
        float interval = RemoteConfigManager.AdsInterval; // default 25 from remote config
        if (Time.time - lastInterstitialTime < interval)
        {
            float waitTime = interval - (Time.time - lastInterstitialTime);
            Debug.Log($"[Interstitial] Cooldown active. Wait {waitTime:F1} seconds before showing next ad.");
            return;
        }

        // Show if ready
        if (MaxSdk.IsInterstitialReady(InterstitialAdUnitId))
        {
            AppOpenAdController.instance.isAdShowing = true;
            MaxSdk.ShowInterstitial(InterstitialAdUnitId);
            lastInterstitialTime = Time.time; // mark the time ad was shown
            Debug.Log($"[Interstitial] Ad shown. Next ad allowed after {interval} seconds.");
        }
        else
        {
            Debug.Log("[Interstitial] Ad not ready, loading new one...");
            LoadInterstitial();
        }
    }

    private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {

        Debug.Log("Interstitial loaded");

        // Reset retry attempt
        interstitialRetryAttempt = 0;
    }

    private void OnInterstitialFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Interstitial ad failed to load. We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds).
        interstitialRetryAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(6, interstitialRetryAttempt));

        // interstitialStatusText.text = "Load failed: " + errorInfo.Code + "\nRetrying in " + retryDelay + "s...";
        Debug.Log("Interstitial failed to load with error code: " + errorInfo.Code);

        Invoke(nameof(LoadInterstitial), (float)retryDelay);
    }

    private void InterstitialFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad failed to display. We recommend loading the next ad
        Debug.Log("Interstitial failed to display with error code: " + errorInfo.Code);
        LoadInterstitial();
    }

    private void OnInterstitialDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad is hidden. Pre-load the next ad
        Debug.Log("Interstitial dismissed");
        AppOpenAdController.instance.isAdShowing = false;
        LoadInterstitial();
    }

    private void OnInterstitialRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad revenue paid. Use this callback to track user revenue.
        Debug.Log("Interstitial revenue paid");

        // Ad revenue
        double revenue = adInfo.Revenue;

        // Miscellaneous data
        string countryCode = MaxSdk.GetSdkConfiguration().CountryCode; // "US" for the United States, etc - Note: Do not confuse this with currency code which is "USD" in most cases!
        string networkName = adInfo.NetworkName; // Display name of the network that showed the ad (e.g. "AdColony")
        string adUnitIdentifier = adInfo.AdUnitIdentifier; // The MAX Ad Unit ID
        string placement = adInfo.Placement; // The placement this ad's postbacks are tied to



        var parameters = new Dictionary<string, string>()
  {
    { "ad_platform", "applovin_max" },
    { "ad_source", adInfo.NetworkName },
    { "ad_unit_name", adInfo.AdUnitIdentifier },
    { "ad_format", adInfo.AdFormat },
    { "currency", "USD" },
    { "value", adInfo.Revenue.ToString(System.Globalization.CultureInfo.InvariantCulture) } // Convert double to string with "." decimal
  };
        var logRevenue = new AFAdRevenueData("applovin", MediationNetwork.IronSource, "USD", revenue);
        AppsFlyer.logAdRevenue(logRevenue, parameters);

        Debug.Log(
       $"[AppsFlyer] logAdRevenue Called\n" +
       $"Monetization Network: {logRevenue.monetizationNetwork}\n" +
       $"Mediation Network: {logRevenue.mediationNetwork}\n" +
       $"Currency: {logRevenue.currencyIso4217Code} | Revenue: {logRevenue.eventRevenue}\n" +
       $"ad_platform: {parameters["ad_platform"]}\n" +
       $"ad_source: {parameters["ad_source"]}\n" +
       $"ad_unit_name: {parameters["ad_unit_name"]}\n" +
       $"ad_format: {parameters["ad_format"]}\n" +
       $"currency: {parameters["currency"]}\n" +
       $"value: {parameters["value"]}"
   );


        var impressionParameters = new[] {
  new Firebase.Analytics.Parameter("ad_platform", "AppLovin"),
  new Firebase.Analytics.Parameter("ad_source", adInfo.NetworkName),
  new Firebase.Analytics.Parameter("ad_unit_name", adInfo.AdUnitIdentifier),
  new Firebase.Analytics.Parameter("ad_format", adInfo.AdFormat),
  new Firebase.Analytics.Parameter("value", revenue),
  new Firebase.Analytics.Parameter("currency", "USD"), // All AppLovin revenue is sent in USD
};
        Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);


        var eventValues = new Dictionary<string, string>()
    {
        { "ad_platform", "applovin_max" },
        { "ad_source", adInfo.NetworkName },
        { "ad_unit_name", adInfo.AdUnitIdentifier },
        { "ad_format", adInfo.AdFormat },
        { "placement", adInfo.Placement }
    };

        // Send AppsFlyer event
        AppsFlyer.sendEvent("af_inters_displayed", eventValues);
    }

    #endregion

    #region Rewarded Ad Methods

    private void InitializeRewardedAds()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            return;
        }
        // Attach callbacks
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
        MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdDismissedEvent;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;

        // Load the first RewardedAd
        LoadRewardedAd();
    }

    private void LoadRewardedAd()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            return;
        }
        //rewardedStatusText.text = "Loading...";
        MaxSdk.LoadRewardedAd(RewardedAdUnitId);
    }

    public void ShowRewardedVideoAd(RewardedAdSO requester)
    {
        //  RewardInfo = Info;
        _currentRequester = requester;
        if (Application.internetReachability == NetworkReachability.NotReachable || MaxSdk.IsRewardedAdReady(RewardedAdUnitId) == false)
        {
            if (once == false)
            {
                once = true;
                adsNotAvailibleNoti.SetActive(true);
                Invoke(nameof(DelayFalseOnce), 1.5f);
            }

            return;
        }
        if (MaxSdk.IsRewardedAdReady(RewardedAdUnitId))
        {
            MaxSdk.ShowRewardedAd(RewardedAdUnitId);
        }
        else
        {
            //rewardedStatusText.text = "Ad not ready";
        }
    }

    bool once = false;
    public void ShowRewardedVideoAd()
    {
        //  RewardInfo = Info;
        //  _currentRequester = requester;
        if (Application.internetReachability == NetworkReachability.NotReachable || MaxSdk.IsRewardedAdReady(RewardedAdUnitId) == false)
        {
            if (once == false)
            {
                once = true;
                adsNotAvailibleNoti.SetActive(true);
                Invoke(nameof(DelayFalseOnce), 1.5f);
            }

            return;
        }
        if (MaxSdk.IsRewardedAdReady(RewardedAdUnitId))
        {
            AppOpenAdController.instance.isAdShowing = true;
            MaxSdk.ShowRewardedAd(RewardedAdUnitId);
        }
        else
        {
            //rewardedStatusText.text = "Ad not ready";
        }
    }


    void DelayFalseOnce()
    {
        once = false;
        adsNotAvailibleNoti.SetActive(false);
    }

    private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad is ready to be shown. MaxSdk.IsRewardedAdReady(rewardedAdUnitId) will now return 'true'
        // rewardedStatusText.text = "Loaded";
        Debug.Log("Rewarded ad loaded");

        // Reset retry attempt
        rewardedRetryAttempt = 0;
    }

    private void OnRewardedAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Rewarded ad failed to load. We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds).
        rewardedRetryAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(6, rewardedRetryAttempt));

        // rewardedStatusText.text = "Load failed: " + errorInfo.Code + "\nRetrying in " + retryDelay + "s...";
        Debug.Log("Rewarded ad failed to load with error code: " + errorInfo.Code);

        Invoke(nameof(LoadRewardedAd), (float)retryDelay);
    }

    private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad failed to display. We recommend loading the next ad
        Debug.Log("Rewarded ad failed to display with error code: " + errorInfo.Code);
        LoadRewardedAd();
    }

    private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Rewarded ad displayed");
    }

    private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Rewarded ad clicked");
    }

    private void OnRewardedAdDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad is hidden. Pre-load the next ad
        Debug.Log("Rewarded ad dismissed");
        LoadRewardedAd();
    }

    private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad was displayed and user should receive the reward
        Debug.Log("Rewarded ad received reward");
        GiveReward();
        AppOpenAdController.instance.isAdShowing = false;
        OnUserEarnedRewardEvent.Invoke();
        OnVideoAdCompleteReward?.Invoke();
        _currentRequester?.CompletionCallBack(Status.Succeded, new ADMeta());
    }

    private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad revenue paid. Use this callback to track user revenue.
        Debug.Log("Rewarded ad revenue paid");

        // Ad revenue
        double revenue = adInfo.Revenue;

        // Miscellaneous data
        string countryCode = MaxSdk.GetSdkConfiguration().CountryCode; // "US" for the United States, etc - Note: Do not confuse this with currency code which is "USD" in most cases!
        string networkName = adInfo.NetworkName; // Display name of the network that showed the ad (e.g. "AdColony")
        string adUnitIdentifier = adInfo.AdUnitIdentifier; // The MAX Ad Unit ID
        string placement = adInfo.Placement; // The placement this ad's postbacks are tied to
        var parameters = new Dictionary<string, string>()
  {
    { "ad_platform", "applovin_max" },
    { "ad_source", adInfo.NetworkName },
    { "ad_unit_name", adInfo.AdUnitIdentifier },
    { "ad_format", adInfo.AdFormat },
    { "currency", "USD" },
    { "value", adInfo.Revenue.ToString(System.Globalization.CultureInfo.InvariantCulture) } // Convert double to string with "." decimal
  };
        var logRevenue = new AFAdRevenueData("applovin", MediationNetwork.IronSource, "USD", revenue);
        AppsFlyer.logAdRevenue(logRevenue, parameters);



        var impressionParameters = new[] {
  new Firebase.Analytics.Parameter("ad_platform", "AppLovin"),
  new Firebase.Analytics.Parameter("ad_source", adInfo.NetworkName),
  new Firebase.Analytics.Parameter("ad_unit_name", adInfo.AdUnitIdentifier),
  new Firebase.Analytics.Parameter("ad_format", adInfo.AdFormat),
  new Firebase.Analytics.Parameter("value", revenue),
  new Firebase.Analytics.Parameter("currency", "USD"), // All AppLovin revenue is sent in USD
};
        Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);


        Debug.Log(
       $"[AppsFlyer] logAdRevenue Called\n" +
       $"Monetization Network: {logRevenue.monetizationNetwork}\n" +
       $"Mediation Network: {logRevenue.mediationNetwork}\n" +
       $"Currency: {logRevenue.currencyIso4217Code} | Revenue: {logRevenue.eventRevenue}\n" +
       $"ad_platform: {parameters["ad_platform"]}\n" +
       $"ad_source: {parameters["ad_source"]}\n" +
       $"ad_unit_name: {parameters["ad_unit_name"]}\n" +
       $"ad_format: {parameters["ad_format"]}\n" +
       $"currency: {parameters["currency"]}\n" +
       $"value: {parameters["value"]}"

   );






        var eventValues = new Dictionary<string, string>()
    {
        { "ad_platform", "applovin_max" },
        { "ad_source", adInfo.NetworkName },
        { "ad_unit_name", adInfo.AdUnitIdentifier },
        { "ad_format", adInfo.AdFormat },
        { "placement", adInfo.Placement }
    };

        // Send AppsFlyer event
        AppsFlyer.sendEvent("af_rewarded_ad_displayed", eventValues);

    }



    #endregion

    #region Banner Ad Methods

    public void RequestBannerAd()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            return;
        }
        // Attach Callbacks
        MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdFailedEvent;
        MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;

        // Banners are automatically sized to 320x50 on phones and 728x90 on tablets.
        // You may use the utility method `MaxSdkUtils.isTablet()` to help with view sizing adjustments.
        var adViewConfiguration = new MaxSdk.AdViewConfiguration(MaxSdk.AdViewPosition.BottomCenter);
        MaxSdk.CreateBanner(BannerAdUnitId, adViewConfiguration);
        // MaxSdk.SetBannerExtraParameter(BannerAdUnitId, "adaptive_banner", "false");
        MaxSdkUtils.IsTablet();
        // Set background or background color for banners to be fully functional.
        MaxSdk.SetBannerBackgroundColor(BannerAdUnitId, Color.black);
    }
    public void ShowAdmobBanner()
    {
        if (PlayerPrefs.GetInt("IsAdsRemoved") == 1 || RemoteConfigManager.HienQc == false)
        {
            return;
        }

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            return;
        }
        MaxSdk.ShowBanner(BannerAdUnitId);
    }
    public void HideAdmobBanner()
    {
        MaxSdk.HideBanner(BannerAdUnitId);
    }



    private void ToggleBannerVisibility()
    {
        if (!isBannerShowing)
        {
            MaxSdk.ShowBanner(BannerAdUnitId);
            //showBannerButton.GetComponentInChildren<Text>().text = "Hide Banner";
        }
        else
        {
            MaxSdk.HideBanner(BannerAdUnitId);
            // showBannerButton.GetComponentInChildren<Text>().text = "Show Banner";
        }

        isBannerShowing = !isBannerShowing;
    }

    private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Banner ad is ready to be shown.
        // If you have already called MaxSdk.ShowBanner(BannerAdUnitId) it will automatically be shown on the next ad refresh.
        Debug.Log("Banner ad loaded");
    }

    private void OnBannerAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Banner ad failed to load. MAX will automatically try loading a new ad internally.
        Debug.Log("Banner ad failed to load with error code: " + errorInfo.Code);
    }

    private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Banner ad clicked");
    }

    private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Banner ad revenue paid. Use this callback to track user revenue.
        Debug.Log("Banner ad revenue paid");

        // Ad revenue
        double revenue = adInfo.Revenue;

        // Miscellaneous data
        string countryCode = MaxSdk.GetSdkConfiguration().CountryCode; // "US" for the United States, etc - Note: Do not confuse this with currency code which is "USD" in most cases!
        string networkName = adInfo.NetworkName; // Display name of the network that showed the ad (e.g. "AdColony")
        string adUnitIdentifier = adInfo.AdUnitIdentifier; // The MAX Ad Unit ID
        string placement = adInfo.Placement; // The placement this ad's postbacks are tied to


        var impressionParameters = new[] {
  new Firebase.Analytics.Parameter("ad_platform", "AppLovin"),
  new Firebase.Analytics.Parameter("ad_source", adInfo.NetworkName),
  new Firebase.Analytics.Parameter("ad_unit_name", adInfo.AdUnitIdentifier),
  new Firebase.Analytics.Parameter("ad_format", adInfo.AdFormat),
  new Firebase.Analytics.Parameter("value", revenue),
  new Firebase.Analytics.Parameter("currency", "USD"), // All AppLovin revenue is sent in USD
};
        Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);
        var parameters = new Dictionary<string, string>()
  {
    { "ad_platform", "applovin_max" },
    { "ad_source", adInfo.NetworkName },
    { "ad_unit_name", adInfo.AdUnitIdentifier },
    { "ad_format", adInfo.AdFormat },
    { "currency", "USD" },
    { "value", adInfo.Revenue.ToString(System.Globalization.CultureInfo.InvariantCulture) } // Convert double to string with "." decimal
  };
        var logRevenue = new AFAdRevenueData("applovin", MediationNetwork.IronSource, "USD", revenue);
        AppsFlyer.logAdRevenue(logRevenue, parameters);

        Debug.Log(
       $"[AppsFlyer] logAdRevenue Called\n" +
       $"Monetization Network: {logRevenue.monetizationNetwork}\n" +
       $"Mediation Network: {logRevenue.mediationNetwork}\n" +
       $"Currency: {logRevenue.currencyIso4217Code} | Revenue: {logRevenue.eventRevenue}\n" +
       $"ad_platform: {parameters["ad_platform"]}\n" +
       $"ad_source: {parameters["ad_source"]}\n" +
       $"ad_unit_name: {parameters["ad_unit_name"]}\n" +
       $"ad_format: {parameters["ad_format"]}\n" +
       $"currency: {parameters["currency"]}\n" +
       $"value: {parameters["value"]}"
   );

    }

    #endregion

    #region MREC Ad Methods

    private void InitializeMRecAds()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            return;
        }
        // Attach Callbacks
        MaxSdkCallbacks.MRec.OnAdLoadedEvent += OnMRecAdLoadedEvent;
        MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += OnMRecAdFailedEvent;
        MaxSdkCallbacks.MRec.OnAdClickedEvent += OnMRecAdClickedEvent;
        MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnMRecAdRevenuePaidEvent;

        // MRECs are automatically sized to 300x250.
        var adViewConfiguration = new MaxSdk.AdViewConfiguration(MaxSdk.AdViewPosition.TopLeft);
        MaxSdk.CreateBanner(MRecAdUnitId, adViewConfiguration);
    }
    public void ShowAdmobRect()
    {
        if (PlayerPrefs.GetInt("IsAdsRemoved") == 1)
        {
            return;
        }
        MaxSdk.ShowMRec(MRecAdUnitId);

    }

    public void HideAdmobRect()
    {
        MaxSdk.HideMRec(MRecAdUnitId);
    }
    private void ToggleMRecVisibility()
    {
        if (!isMRecShowing)
        {
            MaxSdk.ShowMRec(MRecAdUnitId);
            // showMRecButton.GetComponentInChildren<Text>().text = "Hide MREC";
        }
        else
        {
            MaxSdk.HideMRec(MRecAdUnitId);
            // showMRecButton.GetComponentInChildren<Text>().text = "Show MREC";
        }

        isMRecShowing = !isMRecShowing;
    }

    private void OnMRecAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // MRec ad is ready to be shown.
        // If you have already called MaxSdk.ShowMRec(MRecAdUnitId) it will automatically be shown on the next MRec refresh.
        Debug.Log("MRec ad loaded");
    }

    private void OnMRecAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // MRec ad failed to load. MAX will automatically try loading a new ad internally.
        Debug.Log("MRec ad failed to load with error code: " + errorInfo.Code);
    }

    private void OnMRecAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("MRec ad clicked");
    }

    private void OnMRecAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // MRec ad revenue paid. Use this callback to track user revenue.
        Debug.Log("MRec ad revenue paid");

        // Ad revenue
        double revenue = adInfo.Revenue;

        // Miscellaneous data
        string countryCode = MaxSdk.GetSdkConfiguration().CountryCode; // "US" for the United States, etc - Note: Do not confuse this with currency code which is "USD"!
        string networkName = adInfo.NetworkName; // Display name of the network that showed the ad (e.g. "AdColony")
        string adUnitIdentifier = adInfo.AdUnitIdentifier; // The MAX Ad Unit ID
        string placement = adInfo.Placement; // The placement this ad's postbacks are tied to


    }
    public void GiveReward()
    {
        try
        {


        }
        catch (System.Exception e)
        {
            print(e);
        }
    }

    #endregion


    /// <summary>
    /// Send an af_purchase event to AppsFlyer
    /// </summary>
    /// <param name="productId">Store product ID</param>
    /// <param name="currencyCode">ISO 4217 currency code (e.g. USD, EUR, PKR)</param>
    /// <param name="grossRevenue">Gross purchase price before store cut</param>
    /// <param name="quantity">Number of items purchased</param>
    public static void SendPurchaseEventCustomMATS(Product product)
    {
        if (product == null)
        {
            Debug.LogError("<color=red>[MATS_AppsFlyerIAP]</color> Product is null, cannot send event.");
            return;
        }

        string productId = product.definition.id;
        string currencyCode = product.metadata.isoCurrencyCode;
        double grossRevenue = (double)product.metadata.localizedPrice;
        double netRevenue = grossRevenue * 0.65;

        Dictionary<string, string> eventData = new Dictionary<string, string>
        {
            { AFInAppEvents.CURRENCY, currencyCode },
            { AFInAppEvents.REVENUE, netRevenue.ToString("F2") },
            { AFInAppEvents.CONTENT_ID, productId },
            { AFInAppEvents.QUANTITY, "1" }
        };

        if (product.definition.type == ProductType.Subscription)
        {
            AppsFlyer.sendEvent("af_subscribe", eventData);
            Debug.Log($"<color=cyan>[MATS_AppsFlyerIAP]</color> Sent af_subscribe event: Product={productId}, Revenue={netRevenue}");
        }
        else
        {
            AppsFlyer.sendEvent(AFInAppEvents.PURCHASE, eventData);
            Debug.Log($"<color=cyan>[MATS_AppsFlyerIAP]</color> Sent af_purchase event: Product={productId}, Revenue={netRevenue}");
        }
    }


}

