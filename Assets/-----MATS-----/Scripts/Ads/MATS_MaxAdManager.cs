using System;
using UnityEngine;
using static MaxSdkBase;

public class MATS_MaxAdManager : MonoBehaviour
{
    public static MATS_MaxAdManager instsance;
    private bool isBannerShowing;
    private bool isMRecShowing;

    [Header("Banner Positions")]
    public BannerPosition bannerPosition = BannerPosition.TopCenter;
    public AdViewPosition mRecPosition = AdViewPosition.BottomLeft;

    private int interstitialRetryAttempt;
    private int rewardedRetryAttempt;
    private int appOpenRetryAttempt;

    public static event Action OnRewardedAdCompleted;

    #region Unity Lifecycle
    public void Awake()
    {
        if (instsance == null)
        {
            instsance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        InitAds();
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            ShowAppOpenAd();
        }
    }

    #endregion

    #region Initialization

    private void InitAds()
    {
        MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration =>
        {
            Debug.Log("[MAX Ads] SDK Initialized");

            InitializeInterstitialAds();
            InitializeRewardedAds();
            InitializeBannerAds();
          //  InitializeMRecAds();
           // InitializeAppOpenAds();
        };

        MaxSdk.SetSdkKey(MATS_AdKeys.MaxSdkKey);
        MaxSdk.InitializeSdk();
    }

    #endregion

    #region Interstitial Ads

    public void ShowInterstitial()
    {
        if (MaxSdk.IsInterstitialReady(MATS_AdKeys.InterstitialAdUnitId))
        {
            Debug.Log("[MAX Ads] Showing Interstitial");
            MaxSdk.ShowInterstitial(MATS_AdKeys.InterstitialAdUnitId);
        }
        else
        {
            Debug.Log("[MAX Ads] Interstitial not ready");
        }
    }

    private void InitializeInterstitialAds()
    {
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += InterstitialFailedToDisplayEvent;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialDismissedEvent;
        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnInterstitialRevenuePaidEvent;

        LoadInterstitial();
    }

    private void LoadInterstitial()
    {
        Debug.Log("[MAX Ads] Loading Interstitial...");
        MaxSdk.LoadInterstitial(MATS_AdKeys.InterstitialAdUnitId);
    }

    private void OnInterstitialLoadedEvent(string adUnitId, AdInfo adInfo)
    {
        interstitialRetryAttempt = 0;
        Debug.Log("[MAX Ads] Interstitial Loaded");
    }

    private void OnInterstitialFailedEvent(string adUnitId, ErrorInfo errorInfo)
    {
        interstitialRetryAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(6, interstitialRetryAttempt));
        Debug.Log($"[MAX Ads] Interstitial failed to load: {errorInfo.Message}. Retrying in {retryDelay}s");
        Invoke(nameof(LoadInterstitial), (float)retryDelay);
    }

    private void InterstitialFailedToDisplayEvent(string adUnitId, ErrorInfo errorInfo, AdInfo adInfo)
    {
        Debug.Log($"[MAX Ads] Interstitial failed to display: {errorInfo.Message}");
        LoadInterstitial();
    }

    private void OnInterstitialDismissedEvent(string adUnitId, AdInfo adInfo)
    {
        Debug.Log("[MAX Ads] Interstitial Dismissed");
        LoadInterstitial();
    }

    private void OnInterstitialRevenuePaidEvent(string adUnitId, AdInfo adInfo)
    {
        Debug.Log($"[MAX Ads] Interstitial revenue paid: ${adInfo.Revenue}");
        if (adInfo.Revenue > 0)
        {
            Debug.Log("[MAX SDK] ::: In Interstitial Ad Clicked CallBack");
            Firebase.Analytics.Parameter[] adParameters =
            {
                    new("ad_platform", "applovin_max"),
                    new("ad_source", adInfo.NetworkName),
                    new("ad_unit_name", adInfo.AdUnitIdentifier),
                    new("ad_format", adInfo.AdFormat),
                    new("currency", "USD"),
                    new("value", (adInfo.Revenue)),
                };

            Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", adParameters);
        }
        else
        {
            Debug.LogError("Failed to parse valid revenue value from ad info or revenue is not greater than 0");
        }
    

           //AppsFlyerAdRevenue.logAdRevenue("applovin",
           //AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeIronSource,
           //adInfo.Revenue, "USD",
           //parameters);
    

    }
   
    #endregion

    #region Rewarded Ads

    public void ShowRewardedAd()
    {
        if (MaxSdk.IsRewardedAdReady(MATS_AdKeys.RewardedAdUnitId))
        {
            Debug.Log("[MAX Ads] Showing Rewarded Ad");
            MaxSdk.ShowRewardedAd(MATS_AdKeys.RewardedAdUnitId);
        }
        else
        {
            Debug.Log("[MAX Ads] Rewarded Ad not ready");
        }
    }

    private void InitializeRewardedAds()
    {
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
        MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdDismissedEvent;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;

        LoadRewardedAd();
    }

    private void LoadRewardedAd()
    {
        Debug.Log("[MAX Ads] Loading Rewarded Ad...");
        MaxSdk.LoadRewardedAd(MATS_AdKeys.RewardedAdUnitId);
    }

    private void OnRewardedAdLoadedEvent(string adUnitId, AdInfo adInfo)
    {
        rewardedRetryAttempt = 0;
        Debug.Log("[MAX Ads] Rewarded Ad Loaded");
    }

    private void OnRewardedAdFailedEvent(string adUnitId, ErrorInfo errorInfo)
    {
        rewardedRetryAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(6, rewardedRetryAttempt));
        Debug.Log($"[MAX Ads] Rewarded failed to load: {errorInfo.Message}. Retrying in {retryDelay}s");
        Invoke(nameof(LoadRewardedAd), (float)retryDelay);
    }

    private void OnRewardedAdFailedToDisplayEvent(string adUnitId, ErrorInfo errorInfo, AdInfo adInfo)
    {
        Debug.Log($"[MAX Ads] Rewarded failed to display: {errorInfo.Message}");
        LoadRewardedAd();
    }

    private void OnRewardedAdDisplayedEvent(string adUnitId, AdInfo adInfo)
    {
        Debug.Log("[MAX Ads] Rewarded Ad Displayed");
    }

    private void OnRewardedAdClickedEvent(string adUnitId, AdInfo adInfo)
    {
        Debug.Log("[MAX Ads] Rewarded Ad Clicked");
    }

    private void OnRewardedAdDismissedEvent(string adUnitId, AdInfo adInfo)
    {
        Debug.Log("[MAX Ads] Rewarded Ad Dismissed");
        LoadRewardedAd();
    }

    private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, AdInfo adInfo)
    {
        Debug.Log("[MAX Ads] Rewarded Ad - Reward Granted");
        OnRewardedAdCompleted?.Invoke();
    }

    private void OnRewardedAdRevenuePaidEvent(string adUnitId, AdInfo adInfo)
    {
        Debug.Log($"[MAX Ads] Rewarded Ad revenue paid: ${adInfo.Revenue}");
    }

    #endregion

    #region Banner Ads

    public void ToggleBannerVisibility()
    {
        if (!isBannerShowing)
        {
            MaxSdk.ShowBanner(MATS_AdKeys.BannerAdUnitId);
        }
        else
        {
            MaxSdk.HideBanner(MATS_AdKeys.BannerAdUnitId);
        }

        isBannerShowing = !isBannerShowing;
    }

    private void InitializeBannerAds()
    {
        MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdFailedEvent;
        MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;

        MaxSdk.CreateBanner(MATS_AdKeys.BannerAdUnitId, bannerPosition);
        MaxSdk.SetBannerBackgroundColor(MATS_AdKeys.BannerAdUnitId, Color.black);
    }

    private void OnBannerAdLoadedEvent(string adUnitId, AdInfo adInfo)
    {
        Debug.Log("[MAX Ads] Banner Loaded");
    }

    private void OnBannerAdFailedEvent(string adUnitId, ErrorInfo errorInfo)
    {
        Debug.Log($"[MAX Ads] Banner failed: {errorInfo.Message}");
    }

    private void OnBannerAdClickedEvent(string adUnitId, AdInfo adInfo)
    {
        Debug.Log("[MAX Ads] Banner Clicked");
    }

    private void OnBannerAdRevenuePaidEvent(string adUnitId, AdInfo adInfo)
    {
        Debug.Log($"[MAX Ads] Banner revenue paid: ${adInfo.Revenue}");
    }

    #endregion

    #region MREC Ads

    public void ToggleMRecVisibility()
    {
        if (!isMRecShowing)
        {
            MaxSdk.ShowMRec(MATS_AdKeys.MRecAdUnitId);
        }
        else
        {
            MaxSdk.HideMRec(MATS_AdKeys.MRecAdUnitId);
        }

        isMRecShowing = !isMRecShowing;
    }

    private void InitializeMRecAds()
    {
        MaxSdkCallbacks.MRec.OnAdLoadedEvent += OnMRecAdLoadedEvent;
        MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += OnMRecAdFailedEvent;
        MaxSdkCallbacks.MRec.OnAdClickedEvent += OnMRecAdClickedEvent;
        MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnMRecAdRevenuePaidEvent;

        MaxSdk.CreateMRec(MATS_AdKeys.MRecAdUnitId, mRecPosition);
    }

    private void OnMRecAdLoadedEvent(string adUnitId, AdInfo adInfo)
    {
        Debug.Log("[MAX Ads] MREC Loaded");
    }

    private void OnMRecAdFailedEvent(string adUnitId, ErrorInfo errorInfo)
    {
        Debug.Log($"[MAX Ads] MREC failed: {errorInfo.Message}");
    }

    private void OnMRecAdClickedEvent(string adUnitId, AdInfo adInfo)
    {
        Debug.Log("[MAX Ads] MREC Clicked");
    }

    private void OnMRecAdRevenuePaidEvent(string adUnitId, AdInfo adInfo)
    {
        Debug.Log($"[MAX Ads] MREC revenue paid: ${adInfo.Revenue}");
    }

    #endregion

    #region App Open Ads

    public void ShowAppOpenAd()
    {
        if (MaxSdk.IsAppOpenAdReady(MATS_AdKeys.AppOpenAdUnitId))
        {
            Debug.Log("[MAX Ads] Showing App Open Ad");
            MaxSdk.ShowAppOpenAd(MATS_AdKeys.AppOpenAdUnitId);
        }
        else
        {
            Debug.Log("[MAX Ads] App Open Ad not ready");
        }
    }

    private void InitializeAppOpenAds()
    {
        MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += OnAppOpenAdLoadedEvent;
        MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent += OnAppOpenAdFailedEvent;
        MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += OnAppOpenAdFailedToDisplayEvent;
        MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += OnAppOpenAdDismissedEvent;
        MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += OnAppOpenAdRevenuePaidEvent;

        LoadAppOpenAd();
    }

    private void LoadAppOpenAd()
    {
        Debug.Log("[MAX Ads] Loading App Open Ad...");
        MaxSdk.LoadAppOpenAd(MATS_AdKeys.AppOpenAdUnitId);
    }

    private void OnAppOpenAdLoadedEvent(string adUnitId, AdInfo adInfo)
    {
        Debug.Log("[MAX Ads] App Open Ad Loaded");
        appOpenRetryAttempt = 0;
    }

    private void OnAppOpenAdFailedEvent(string adUnitId, ErrorInfo errorInfo)
    {
        appOpenRetryAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(6, appOpenRetryAttempt));
        Debug.Log($"[MAX Ads] App Open Ad failed: {errorInfo.Message}. Retrying in {retryDelay}s");
        Invoke(nameof(LoadAppOpenAd), (float)retryDelay);
    }

    private void OnAppOpenAdFailedToDisplayEvent(string adUnitId, ErrorInfo errorInfo, AdInfo adInfo)
    {
        Debug.Log($"[MAX Ads] App Open Ad failed to display: {errorInfo.Message}");
        LoadAppOpenAd();
    }

    private void OnAppOpenAdDismissedEvent(string adUnitId, AdInfo adInfo)
    {
        Debug.Log("[MAX Ads] App Open Ad Dismissed");
        LoadAppOpenAd();
    }

    private void OnAppOpenAdRevenuePaidEvent(string adUnitId, AdInfo adInfo)
    {
        Debug.Log($"[MAX Ads] App Open Ad revenue paid: ${adInfo.Revenue}");
    }

    #endregion

   
}
