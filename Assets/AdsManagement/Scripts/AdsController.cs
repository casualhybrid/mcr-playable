using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using TheKnights.AdsSystem;
using TheKnights.SaveFileSystem;
using Unity.Services.Analytics;
using UnityEngine;

using UnityEngine.Events;

public enum BannerAdNotPossibleReason
{
    None, AdFree, GloballyDisabled, FailedGettingConfigForAd, LocallyDisabled
}

public struct BannerADRequestResult
{
    public bool IsSucceeded;
    public BannerAdNotPossibleReason BannerAdNotPossibleReason;

    public BannerADRequestResult(bool _isSucceeded, BannerAdNotPossibleReason _bannerAdNotPossibleReason)
    {
        IsSucceeded = _isSucceeded;
        BannerAdNotPossibleReason = _bannerAdNotPossibleReason;
    }
}

[CreateAssetMenu(fileName = "AdController", menuName = "ScriptableObjects/Ads/AdController")]
public class AdsController : ScriptableObject
{
    [SerializeField] private GameEvent dependenciesLoaded;
    [SerializeField] protected GameEvent rewardedAdFailedToShow_NotLoaded;
    [SerializeField] private GameEvent rewardedFramingWindowFinished;

    [SerializeField] private SaveManager saveManager;
    [SerializeField] private AdManager adManagerOriginal;
    [SerializeField] private AdManager adManagerLowEnd;

    public event Action AdManagerHasChanged;

    [HideInInspector] public UnityEvent OnRewardedADFailedToShowInGivenTimeFrame;
    [HideInInspector] public UnityEvent OnRewardedAdLoaded;
    [HideInInspector] public UnityEvent OnRewardedAdCompleted;
    [HideInInspector] public UnityEvent OnRewardedAdSkipped;
    [HideInInspector] public UnityEvent OnRewardedAdClosed;
    [HideInInspector] public UnityEvent OnRewardedAdAboutToShow;
    [HideInInspector] public UnityEvent OnRewardedAdFailedToShow;
    [HideInInspector] public UnityEvent<bool> OnRewardedFramingWindowEnded;

    [HideInInspector] public UnityEvent OnInterstitialAdCompleted;
    [HideInInspector] public UnityEvent OnInterstitialAdShowRequested;
    [HideInInspector] public UnityEvent OnInterstitialAdAboutToShow;
    [HideInInspector] public UnityEvent OnInterstitialAdFailedToShow;

    public event Action<RewardedADRewardMetaData[]> SetRewardedADPanelRewardMeta;

    [SerializeField] private AdPlugin googleAdPluginOriginal;
    [SerializeField] private AdPlugin googleAdPluginLowEnd;
    [SerializeField] private AdPlugin unityAdPluginOriginal;
    [SerializeField] private AdPlugin unityAdPluginLowEnd;

    [SerializeField] private GameEvent adsHaveBeenPurchased;

    private string lastInterstitialPanel;

    private AdManager adManager;
    private AdPlugin googleAdPlugin;
    private AdPlugin unityAdPlugin;

    private bool initializeRequiredAdsOnce = false;
    public bool showRewardedAdAsSoonAsItsLoaded { get; private set; }
    private Action pendingShowRewardedADCommand;

    public bool isRewardedFramingWindowActive { get; private set; }

    public bool AreAdsEnabled => adManager.AreAdsEnabled;

    public AdManager GetCurrentADManager => adManager;
    public AdPlugin GetCurrentGoogleAdPlugin => googleAdPlugin;
    public AdPlugin GetCurrentUnityAdPlugin => unityAdPlugin;

    private Bus _masterBus;

    private CurrentBannerState currentBannerState;

    private void OnEnable()
    {
        //BlackListManager.BlackListOperationCompleted += BlackListManager_BlackListOperationCompleted;

        lastInterstitialPanel = "";
        isRewardedFramingWindowActive = false;
        showRewardedAdAsSoonAsItsLoaded = false;
        initializeRequiredAdsOnce = false;
    }

    public void SetAdManagerAndADPluginsReferences()
    {
        // BlackListManager.BlackListOperationCompleted -= BlackListManager_BlackListOperationCompleted;

        EstimateDevicePerformance.GetDeviceGroup();
        bool isLowSpec = EstimateDevicePerformance.isOreoDevice || EstimateDevicePerformance.isRamLowerThanTwoGB || EstimateDevicePerformance.IsPhoneBlackListed;
        adManager = isLowSpec ? adManagerLowEnd : adManagerOriginal;
        googleAdPlugin = isLowSpec ? googleAdPluginLowEnd : googleAdPluginOriginal;
        unityAdPlugin = isLowSpec ? unityAdPluginLowEnd : unityAdPluginOriginal;
        SubscribeToEvents();
    }

    private void SetADPluginsAccordingToDeviceParameters()
    {
        UnityEngine.Console.Log("Remote Config Recieved");

        AdManager adManagerInUse = adManager;
        bool isLowSpec = EstimateDevicePerformance.isOreoDevice || EstimateDevicePerformance.isRamLowerThanTwoGB || EstimateDevicePerformance.IsPhoneBlackListed;
        adManager = isLowSpec ? adManagerLowEnd : adManagerOriginal;
        googleAdPlugin = isLowSpec ? googleAdPluginLowEnd : googleAdPluginOriginal;
        unityAdPlugin = isLowSpec ? unityAdPluginLowEnd : unityAdPluginOriginal;

        if (adManager != adManagerInUse)
        {
            UnSubscribeADManagerEvents(adManagerInUse);
            UnityEngine.Console.Log("AD manager changed to " + adManager.name);

            bool isDeviceBlackListedWithNOADLevel = EstimateDevicePerformance.IsPhoneBlackListed && EstimateDevicePerformance.PhoneBlackListLevel > 0;

            if ((EstimateDevicePerformance.isRamLowerThanTwoGB && !RemoteConfiguration.IsAdsAllowedOnLowEndRam) || EstimateDevicePerformance.IsChipSetBlackListed || isDeviceBlackListedWithNOADLevel)
            {
                adManager.DisableADSGlobally();
            }

            adManagerInUse.DestroyAllAds();
            adManager.InitializeAds();
            SubscribeADManagerEvents();

            AdManagerHasChanged?.Invoke();
        }
    }

    private void SubscribeToEvents()
    {
        ScreenSafeAreaMonitor.OnSafeAreaChanged += RepositionTheBanner;

        BlackListManager.OnDeviceIsBlackListed += HandleDeviceBlackListed;

        RemoteConfiguration.RemoteConfigurationDataFetched += RemoteConfiguration_RemoteConfigurationDataFetched;

        dependenciesLoaded.TheEvent.AddListener(LoadAds);

        rewardedAdFailedToShow_NotLoaded.TheEvent.AddListener(RewardedFrameWindowStarted);

        rewardedFramingWindowFinished.TheEvent.AddListener(RewardedFrameWindowHasFinished);

        adsHaveBeenPurchased.TheEvent.AddListener(DestroyAllAds);

        SubscribeADManagerEvents();
    }

    private void RemoteConfiguration_RemoteConfigurationDataFetched()
    {
        RemoteConfiguration.RemoteConfigurationDataFetched -= RemoteConfiguration_RemoteConfigurationDataFetched;

        if (EstimateDevicePerformance.isRamLowerThanTwoGB && !RemoteConfiguration.IsAdsAllowedOnLowEndRam)
        {
            UnityEngine.Console.Log($"Remote Data Fetched: Low Ram so turning off Ads");
            adManager.DestroyAllAds();
            adManager.DisableADSGlobally();
        }
    }

    #region AdManagerEvent Listeners

    private void SubscribeADManagerEvents()
    {
        adManager.OnInterstitialADLoaded.AddListener(InterstitialADHasLoadedCallBack);

        adManager.OnInterstitialADAboutToShow.AddListener(AdInterstitialAboutToShowCallBack);

        adManager.OnInterstitialAdFailedToShow.AddListener(InterstitialAdFailedToShowCallBack);

        adManager.OnInterstitialAdCompleted.AddListener(IntersitialAdCompletedCallBack);

        adManager.OnRewardedADAboutToShow.AddListener(RewardedADAboutToShowCallBack);

        adManager.OnRewardedAdCompleted.AddListener(RewardedADCompletedCallBack);

        adManager.OnRewardedAdFailedToShow.AddListener(RewardedADFailedToShowCallBack);

        adManager.OnRewardedAdLoaded.AddListener(RewardedADLoadedCalLBack);

        adManager.OnRewardedAdHasBeenSkipped.AddListener(RewardedADSkippedCallBack);

        adManager.OnRewardedAdHasBeenClosed.AddListener(RewardedADClosedCallBack);

        //  adManager.OnInterstitialADRightBeforeShow.AddListener(GetCurrentSmallBannerStateAndHideIt);

        //     adManager.OnRewardedADRightBeforeShow.AddListener(GetCurrentSmallBannerStateAndHideIt);
    }

    private void UnSubscribeADManagerEvents(AdManager _adManager)
    {
        adManager.OnInterstitialADLoaded.RemoveListener(InterstitialADHasLoadedCallBack);

        _adManager.OnInterstitialADAboutToShow.RemoveListener(AdInterstitialAboutToShowCallBack);

        _adManager.OnInterstitialAdFailedToShow.RemoveListener(InterstitialAdFailedToShowCallBack);

        _adManager.OnInterstitialAdCompleted.RemoveListener(IntersitialAdCompletedCallBack);

        _adManager.OnRewardedADAboutToShow.RemoveListener(RewardedADAboutToShowCallBack);

        _adManager.OnRewardedAdCompleted.RemoveListener(RewardedADCompletedCallBack);

        _adManager.OnRewardedAdFailedToShow.RemoveListener(RewardedADFailedToShowCallBack);

        _adManager.OnRewardedAdLoaded.RemoveListener(RewardedADLoadedCalLBack);

        adManager.OnRewardedAdHasBeenSkipped.RemoveListener(RewardedADSkippedCallBack);

        adManager.OnRewardedAdHasBeenClosed.RemoveListener(RewardedADClosedCallBack);

        // _adManager.OnInterstitialADRightBeforeShow.RemoveListener(GetCurrentSmallBannerStateAndHideIt);

        // _adManager.OnRewardedADRightBeforeShow.RemoveListener(GetCurrentSmallBannerStateAndHideIt);
    }

    private void InterstitialADHasLoadedCallBack(ADMeta adMeta)
    {
        AnalyticsManager.CustomData("InterstitialADHasLoaded", new Dictionary<string, object> { { "mediationAdapterName", adMeta.AdapterName } });
    }

    private void AdInterstitialAboutToShowCallBack()
    {
        AnalyticsManager.CustomData("InterstitialOpened", new Dictionary<string, object> { { "Screen", lastInterstitialPanel } });
        OnInterstitialAdAboutToShow.Invoke();
    }

    private void GetCurrentSmallBannerStateAndHideIt()
    {
        currentBannerState = adManager.GetCurrentSmallBannerState();

        //   UnityEngine.Console.Log($"Current Small Banner Status Saved As {currentBannerState.isActive}");

        adManager.HideBanner(BannerType.SmallBanner);
    }

    private void RestoreSmallBannerToStateBeforeInterstitial()
    {
        //  UnityEngine.Console.Log($"Restoring Small Banner Status To ? {currentBannerState.isActive}");

        if (!currentBannerState.isActive)
            return;

        // UnityEngine.Console.Log("Showing The Restored Banner AD");
        adManager.ShowBannerAd(BannerType.SmallBanner, currentBannerState.CurrentBannerPos);
    }

    private void InterstitialAdFailedToShowCallBack()
    {
        RestoreSmallBannerToStateBeforeInterstitial();
        ResumeFMODMaster(); OnInterstitialAdFailedToShow.Invoke();
    }

    private void IntersitialAdCompletedCallBack(ADMeta adMeta)
    {
        RestoreSmallBannerToStateBeforeInterstitial();

        ResumeFMODMaster();

        AnalyticsManager.CustomData("InterstitialAdCompleted", new Dictionary<string, object> { { "Screen", lastInterstitialPanel }, { "mediationAdapterName", adMeta.AdapterName } });

        OnInterstitialAdCompleted.Invoke();
    }

    private void RewardedADAboutToShowCallBack()
    {
        if (Debug.isDebugBuild)
        {
            UnityEngine.Console.Log($"Rewarded ad about to show");
        }

        OnRewardedAdAboutToShow.Invoke();
    }

    private void RewardedADCompletedCallBack(ADMeta adMeta)
    {
        //  RestoreSmallBannerToStateBeforeInterstitial();
        /*ResumeFMODMaster();*/ OnRewardedAdCompleted.Invoke();
    }

    private void RewardedADFailedToShowCallBack()
    {
        //  RestoreSmallBannerToStateBeforeInterstitial();
        ResumeFMODMaster(); OnRewardedAdFailedToShow.Invoke();
    }

    private void RewardedADSkippedCallBack()
    {
        //  RestoreSmallBannerToStateBeforeInterstitial();
         OnRewardedAdSkipped.Invoke();
    }

    private void RewardedADClosedCallBack()
    {
        //  RestoreSmallBannerToStateBeforeInterstitial();
        ResumeFMODMaster(); OnRewardedAdClosed.Invoke();
    }

    private void RewardedADLoadedCalLBack(bool isPriority, ADMeta adMeta)
    {
        AnalyticsManager.CustomData("RewardedADLoaded", new Dictionary<string, object> { { "mediationAdapterName", adMeta.AdapterName } });

        if (showRewardedAdAsSoonAsItsLoaded && isPriority)
        {
            showRewardedAdAsSoonAsItsLoaded = false;

            // UnityEngine.Console.Log("Showing AD That WAS QUEUED");

            // Show pending rewarded Ads here
            pendingShowRewardedADCommand?.Invoke();

            if (pendingShowRewardedADCommand == null)
            {
                UnityEngine.Console.LogWarning("Show reward as soon as its loaded was requested but there was no pending request");
            }

            pendingShowRewardedADCommand = null;
        }

        OnRewardedAdLoaded.Invoke();
    }

    #endregion AdManagerEvent Listeners

    private void HandleDeviceBlackListed()
    {
        BlackListManager.OnDeviceIsBlackListed -= HandleDeviceBlackListed;
        SetADPluginsAccordingToDeviceParameters();
    }

    public void ShowInterstitialAd(string panelName, Action<TheKnights.AdsSystem.Status, ADMeta> completionCallBack = null, string adType = "Video")
    {
        // User has purchased no ads
        if (saveManager.MainSaveFile.isAdsPurchased)
        {
            OnInterstitialAdFailedToShow.Invoke();
            completionCallBack?.Invoke(TheKnights.AdsSystem.Status.Failed, new ADMeta());
            return;
        }

        //InterstitialRemoteAdConfig interstitialRemoteAdConfig = CurrentRemoteAdConfiguration.InterstitialRemoteAdConfig;

        //// Global status for interstitial Ads
        //if (!interstitialRemoteAdConfig.Status)
        //{
        //    OnInterstitialAdFailedToShow.Invoke();
        //    completionCallBack?.Invoke(TheKnights.AdsSystem.Status.Failed, new ADMeta());
        //    return;
        //}

        //InterstitialConfig interstitialConfig;
        //interstitialRemoteAdConfig.ConfigDictionary.TryGetValue(panelName, out interstitialConfig);

        //if (interstitialConfig == null)
        //{
        //    UnityEngine.Console.LogWarning($"Failed getting interstitialConfig from the dictionary. Panel {panelName}");
        //    OnInterstitialAdFailedToShow.Invoke();
        //    completionCallBack?.Invoke(TheKnights.AdsSystem.Status.Failed, new ADMeta());
        //    return;
        //}

        //// Check if the specific interstitial is disabled
        //if (!interstitialConfig.Status)
        //{
        //    UnityEngine.Console.Log($"The interstitial for {panelName} is disabled");
        //    OnInterstitialAdFailedToShow.Invoke();
        //    completionCallBack?.Invoke(TheKnights.AdsSystem.Status.Failed, new ADMeta());
        //    return;
        //}

        // Check special tweaking conditions
        //SpecialConditionInterstial specialConditionInterstial = interstitialConfig.SpecialCondition;

        //int timesFailed = gamePlaySessionData.timesFailedDuringSession;

        //if (specialConditionInterstial != null && timesFailed > specialConditionInterstial.Times)
        //{
        //    // Call for video Ad?

        //    return;
        //}

        bool isRequestedADAvailable = IsInterstitialADAvailalbe(adType); 

        if (!isRequestedADAvailable)
        {
            UnityEngine.Console.Log($"The interstitial of type {adType} for {panelName} is not available");
            OnInterstitialAdFailedToShow.Invoke();
            completionCallBack?.Invoke(TheKnights.AdsSystem.Status.Failed, new ADMeta());
            return;
        }

        // invoke panel open event here
        GetCurrentSmallBannerStateAndHideIt();
        OnInterstitialAdShowRequested.Invoke();

        // Pause FMOD master
        PauseFMODMaster();

        CoroutineRunner.Instance.WaitForRealTimeDelayAndExecute(() =>
        {
            lastInterstitialPanel = panelName;

            AnalyticsManager.CustomData("InterstitialADRequested", new Dictionary<string, object> { { "InterstitialADType", panelName } });

            adManager.ShowInterstitialAd(completionCallBack, adType);
        }, 0.3f);
    }

    private void LoadAds(GameEvent gameEvent)
    {
        Debug.Log("LoadAds():");

        if (initializeRequiredAdsOnce)
            return;

        initializeRequiredAdsOnce = true;

        CoroutineRunner.Instance.StartCoroutine(WaitAndLoadAds());
    }

    private IEnumerator WaitAndLoadAds()
    {
        yield return null;
        LoadAllInterstitialAds();
        yield return null;
        LoadAllRewardedAds();
    }

    /// <summary>
    /// Load add interstitial Ads
    /// </summary>
    public void LoadAllInterstitialAds()
    {
        if (saveManager.MainSaveFile.isAdsPurchased)
        {
            return;
        }

        adManager.LoadInterstitialAd(null, "Video");
    }

    /// <summary>
    /// Load the interstial google ad
    /// </summary>
    public void LoadGoogleAdsInterstitial()
    {
        if (saveManager.MainSaveFile.isAdsPurchased)
        {
            return;
        }

        adManager.LoadInterstitialAd(googleAdPlugin, "Video");
    }

    /// <summary>
    /// Load the static interstial google ad
    /// </summary>
    public void LoadStaticInterstitialAD()
    {
        if (saveManager.MainSaveFile.isAdsPurchased)
        {
            return;
        }

        adManager.LoadInterstitialAd(googleAdPlugin, "Static");
    }

    /// <summary>
    /// Load the rewarded google ad
    /// </summary>
    public void LoadGoogleRewardedAd()
    {
        adManager.LoadRewardedAd(googleAdPlugin);
    }

    /// <summary>
    /// PreLoad the google ads
    /// </summary>
    public void PreLoadGoogleAds()
    {
        adManager.PreLoadTheAds(googleAdPlugin);
    }

    /// <summary>
    /// Load the rewarded unity ad
    /// </summary>
    public void LoadUnityRewardedAd()
    {
        adManager.LoadRewardedAd(unityAdPlugin);
    }

    public void ShowRewardedAsSoonAsAvailableAndEnqueueRequest(Action request)
    {
        showRewardedAdAsSoonAsItsLoaded = true;
        pendingShowRewardedADCommand = request;
    }

    public void ResetShowRewardedAdAsSoonAsItsLoadedAndTheEnquedRequest()
    {
        showRewardedAdAsSoonAsItsLoaded = false;
        pendingShowRewardedADCommand = null;
    }

    public void SendRewardedFramingWindowEndedEvent(bool status)
    {
        OnRewardedFramingWindowEnded.Invoke(status);
    }

    public void ShowRewardedAD(Action<TheKnights.AdsSystem.Status, ADMeta> completionCallBack = null)
    {
        // Pause FMOD master
        PauseFMODMaster();

        adManager.ShowRewardedAd(completionCallBack);
    }

    public void ShowPriorityRewardedAD(Action<TheKnights.AdsSystem.Status, ADMeta> completionCallBack = null)
    {
        // Pause FMOD master
        PauseFMODMaster();

        adManager.ShowRewardedAd(completionCallBack, true);
    }

    public void RewardedADFailedToShowInTimeFrame()
    {
        ResetShowRewardedAdAsSoonAsItsLoadedAndTheEnquedRequest();
        OnRewardedADFailedToShowInGivenTimeFrame.Invoke();
    }

    private void RepositionTheBanner()
    {
        CurrentBannerState state = adManager.GetCurrentSmallBannerState();

        if (!state.isActive)
            return;

        adManager.ShowBannerAd(BannerType.SmallBanner, /*bannerPosition*/ BannerPosition.Bottom);
    }

    public void ShowSmallBannerAd(string panelName)
    {
        if (!IsSmallBannerPossible(panelName).IsSucceeded)
        {
            HideSmallBanner();
            return;
        }

        //SmallBannerRemoteAdConfig smallBannerRemoteAdConfig = CurrentRemoteAdConfiguration.SmallBannerRemoteAdConfig;
        //SmallBannerConfig smallBannerConfig;
        //smallBannerRemoteAdConfig.ConfigDictionary.TryGetValue(panelName, out smallBannerConfig);

        //BannerPosition bannerPosition;
        //Enum.TryParse(smallBannerConfig.Location, true, out bannerPosition);

        adManager.ShowBannerAd(BannerType.SmallBanner, /*bannerPosition*/ BannerPosition.Bottom);
    }

    public bool IsInterstitialStaticADAvailalbe()
    {
        return adManager.CheckIfStaticInterstitialADIsAvailable();
    }

    public bool IsInterstitialADAvailalbe(string adType = null)
    {
        return adManager.CheckIfInterstitialADIsAvailable();
    }

    public bool IsUnityRewardedADAvailalbe()
    {
        return adManager.CheckIfParticularRewardedADIsAvailable(unityAdPlugin);
    }

    public bool IsPriorityRewardedADAvailalbe()
    {
        return adManager.CheckIfPriorityRewardedADIsAvailable();
    }

    public bool IsRewardedADAvailable()
    {
        return adManager.CheckIfRewardedADIsAvailable();
    }

    public void LoadAllRewardedAds()
    {
        adManager.LoadAllRewardedAds();
    }

    public void SetTheRewardedADPanelMetaData(RewardedADRewardMetaData[] rewardedADRewardMetaDatas)
    {
        if (SetRewardedADPanelRewardMeta != null)
        {
            SetRewardedADPanelRewardMeta.Invoke(rewardedADRewardMetaDatas);
        }
    }

    public void ShowLargeBannerAd(string panelName)
    {
        if (saveManager.MainSaveFile.isAdsPurchased)
        {
            return;
        }

       // LargeBannerRemoteAdConfig largeBannerRemoteAdConfig = CurrentRemoteAdConfiguration.LargeBannerRemoteAdConfig;

        // Check if large banners are globally disabled
       // if (!largeBannerRemoteAdConfig.Status)
       //     return;

      //  LargeBannerConfig largeBannerConfig;
       // largeBannerRemoteAdConfig.ConfigDictionary.TryGetValue(panelName, out largeBannerConfig);

        //if (largeBannerConfig == null)
        //{
        //    UnityEngine.Console.LogWarning($"Failed getting largeBannerConfig from the dictionary. Panel {panelName}");
        //    return;
        //}

        //// Check if the specific large banner is disabled
        //if (!largeBannerConfig.Status)
        //{
        //    UnityEngine.Console.Log($"The large banner {panelName} is disabled");
        //    return;
        //}

        BannerPosition bannerPosition = BannerPosition.Bottom;
       // Enum.TryParse(largeBannerConfig.Location, true, out bannerPosition);

        adManager.ShowBannerAd(BannerType.LargeBanner, bannerPosition);
    }

    public void ShowRewardedAd(string panelName, Action<TheKnights.AdsSystem.Status, ADMeta> completionCallBack = null)
    {
        // Pause FMOD master
        PauseFMODMaster();

        adManager.ShowRewardedAd(completionCallBack);
    }

    public void HideSmallBanner()
    {
        adManager.HideBanner(BannerType.SmallBanner);
    }

    public void HideLargeBanner()
    {
        adManager.HideBanner(BannerType.LargeBanner);
    }

    public BannerADRequestResult IsSmallBannerPossible(string panelName)
    {
        try
        {
            if (saveManager.MainSaveFile.isAdsPurchased)
            {
                UnityEngine.Console.Log("Ads are purchased so banner Not possible");

                return new BannerADRequestResult(false, BannerAdNotPossibleReason.AdFree);
            }

          //  SmallBannerRemoteAdConfig smallBannerRemoteAdConfig = CurrentRemoteAdConfiguration.SmallBannerRemoteAdConfig;

            // Check if small banner are globally disabled
            if (/*!smallBannerRemoteAdConfig.Status || */!adManager.AreAdsEnabled)
            {
                UnityEngine.Console.Log("Small banners are disabled globally");
                return new BannerADRequestResult(false, BannerAdNotPossibleReason.GloballyDisabled);
            }

            //SmallBannerConfig smallBannerConfig;
            //smallBannerRemoteAdConfig.ConfigDictionary.TryGetValue(panelName, out smallBannerConfig);

            //if (smallBannerConfig == null)
            //{
            //    UnityEngine.Console.LogWarning($"Failed getting smallBannerConfig from the dictionary. Panel {panelName}");
            //    return new BannerADRequestResult(false, BannerAdNotPossibleReason.FailedGettingConfigForAd);
            //}

            //// Check if the specific small banner is disabled
            //if (!smallBannerConfig.Status)
            //{
            //    UnityEngine.Console.Log($"The small banner {panelName} is disabled");
            //    return new BannerADRequestResult(false, BannerAdNotPossibleReason.LocallyDisabled);
            //}
        }
        catch
        {
            return new BannerADRequestResult(false, BannerAdNotPossibleReason.None);
        }

        return new BannerADRequestResult(true, BannerAdNotPossibleReason.None);
    }

    public void SendInterstitialFailedToShowEvent()
    {
        OnInterstitialAdFailedToShow.Invoke();
    }

    private void DestroyAllAds(GameEvent gameEvent)
    {
        adManager.DestroyAllAds();
    }

    private void RewardedFrameWindowStarted(GameEvent gameEvent)
    {
        GetCurrentSmallBannerStateAndHideIt();
        // UnityEngine.Console.Log("Rewarded Window Started");
        isRewardedFramingWindowActive = true;
    }

    private void RewardedFrameWindowHasFinished(GameEvent gameEvent)
    {
        RestoreSmallBannerToStateBeforeInterstitial();
        //  UnityEngine.Console.Log("Rewarded Window Finished");
        isRewardedFramingWindowActive = false;
    }

    public void PauseFMODMaster()
    {
        _masterBus = _masterBus.isValid() ? _masterBus : RuntimeManager.GetBus("bus:/");
        _masterBus.setPaused(true);
    }

    public void ResumeFMODMaster()
    {
        _masterBus = _masterBus.isValid() ? _masterBus : RuntimeManager.GetBus("bus:/");
        _masterBus.setPaused(false);
    }
}