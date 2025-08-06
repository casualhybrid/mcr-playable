
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TheKnights.AdsSystem
{
    [CreateAssetMenu(fileName = "GoogleAdsPlugin", menuName = "ScriptableObjects/GoogleAdsPlugin", order = 3)]
    public class GoogleAdsPlugin : AdPlugin
    {
        [Space]
        [Tooltip("Upon initialization loads and caches the ads to be used later on")]
        [SerializeField] private bool preLoadADS;

        [SerializeField] private List<AdType> adsToPreLoad;

        [SerializeField] private Dictionary<string, GoogleInterstitial> googleInterstitials;

        [SerializeField] private bool useInterstitialAsRewardedAD = false;

        public static bool isLargeBannerShowing { get; private set; }

        private static bool isInitializedPluginSpecific;

        private bool isLoadingRewarded;

        //Interstitial Events
        [HideInInspector] public UnityEvent OnAdLoadedEvent;

        [HideInInspector] public UnityEvent OnAdFailedToLoadEvent;
        [HideInInspector] public UnityEvent OnAdOpeningEvent;

        //Banner Loaded
        [HideInInspector] public UnityEvent LargeBannerShown;

        [HideInInspector] public UnityEvent LargeBannerHide;

        //Reference to AdUnits
       // private BannerView smallbanner;

       // private BannerView largebanner;
       // private RewardedAd rewardedAd;

        private bool rewardEarned;

        // Pending Rewarded Listeners
        private Action<Status, ADMeta> pendingRewardedListener;

        private string _bannerID;
        private string _rewardedID;

        private readonly Dictionary<string, GoogleInterstitial> googleInterstitialsUnique = new Dictionary<string, GoogleInterstitial>();

        protected override void OnEnable()
        {
            base.OnEnable();

            rewardEarned = false;
            isLoadingRewarded = false;
        }

        public override void InitializeAds()
        {
            Debug.Log("Ads: InitializeAds");
            googleInterstitialsUnique.Clear();

            if (testMode)
            {
                _bannerID = "ca-app-pub-3940256099942544/6300978111";
                _rewardedID = !useInterstitialAsRewardedAD ? "ca-app-pub-3940256099942544/5224354917" : "ca-app-pub-3940256099942544/1033173712";
            }
            else
            {
                _bannerID = bannerID;
                _rewardedID = rewardedID;
            }

            // Set IDs and unique interstitials
            foreach (var item in googleInterstitials)
            {
                if (!googleInterstitialsUnique.ContainsValue(item.Value))
                {
                    item.Value.Initialize();
                    googleInterstitialsUnique.Add(item.Key, item.Value);
                }
            }

            if (isInitializedPluginSpecific || isInitialized)
            {
                isInitialized = true;
                isInitializationOperationDone = true;
                return;
            }

            isInitializationOperationDone = false;

            CoroutineRunner.Instance.StartCoroutine(InitializeTheAdsRoutine());
        }

        private IEnumerator InitializeTheAdsRoutine()
        {
//#if UNITY_ANDROID && !UNITY_EDITOR
//            WebviewWarmer.PreWarming();

//            yield return new WaitUntil(() => WebviewWarmer.IsOpertaionCompleted);
//#endif
            yield return null;

            //// *** REMOVE ON RELEASE
            //List<string> deviceIds = new List<string>();
            //deviceIds.Add("E5D40EA819C6A390FB57ABB6DAD27649");
            //RequestConfiguration requestConfiguration = new RequestConfiguration
            //    .Builder()
            //    .SetTestDeviceIds(deviceIds)
            //    .build();
            ////**

            //MobileAds.SetRequestConfiguration(requestConfiguration);

            //MobileAds.Initialize((init) =>
            //{
            //    isInitializationOperationDone = true;
            //    isInitialized = true;
            //    isInitializedPluginSpecific = true;

            //    Dictionary<string, AdapterStatus> map = init.getAdapterStatusMap();
            //    foreach (KeyValuePair<string, AdapterStatus> keyValuePair in map)
            //    {
            //        string className = keyValuePair.Key;
            //        AdapterStatus status = keyValuePair.Value;
            //        switch (status.InitializationState)
            //        {
            //            case AdapterState.NotReady:
            //                // The adapter initialization did not complete.
            //                UnityEngine.Console.Log("Adapter: " + className + " not ready." + status.Description);
            //                break;

            //            case AdapterState.Ready:
            //                // The adapter was successfully initialized.
            //                UnityEngine.Console.Log("Adapter: " + className + " is initialized.");
            //                break;
            //        }
            //    }

            // //   PreLoadAds();

            //    if (isDebug)
            //        UnityEngine.Console.Log("Google Ads Initialized");
            //});
        }

        public override void PreLoadAds()
        {
            //if (preLoadADS)
            //{
            //    for (int i = 0; i < adsToPreLoad.Count; i++)
            //    {
            //        AdType ad = adsToPreLoad[i];

            //        switch (ad)
            //        {
            //            case AdType.Banner:
            //                RequestSmallBanner(AdPosition.Bottom);
            //                HideSmallBanner();
            //                break;

            //            case AdType.Rewarded:
            //                LoadRewardedAd();
            //                break;

            //            case AdType.Interstital:
            //                foreach (var item in googleInterstitialsUnique)
            //                {
            //                    if (!item.Value.isPreLoadAD)
            //                        continue;

            //                    item.Value.LoadInterstitial();
            //                }
            //                break;
            //        }
            //    }
            //}
        }

        //public void RemoveInterstitialListeners()
        //{
        //    OnAdClosedEvent.RemoveAllListeners();
        //    OnFailedToShowEvent.RemoveAllListeners();
        //}

        // Loads the AdUnits In background so they can be displayed on demand

        #region LoadAdUnitsInBackGround

        public override void LoadInterstitialAd(string adID = null)
        {
            foreach (var item in googleInterstitialsUnique)
            {
                if (item.Key == "Rewarded")
                    continue;

                if (adID != null && adID != item.Key)
                    continue;

                item.Value.LoadInterstitial();
            }
        }

        #endregion LoadAdUnitsInBackGround

        //Show the loaded ads

        #region ShowAdsMethods

        public override void ShowInterstitialAdIfAvailable(Action<TheKnights.AdsSystem.Status, ADMeta> completionCalLBack, object additionalInfo = null)
        {
            GoogleInterstitial interstitial = GetInterstitialFromID(additionalInfo);
            interstitial.ShowInterstitialAdIfAvailable(completionCalLBack);
        }

//        private void RequestSmallBanner(AdPosition position)
//        {
//            DestroySmallBanner(false);

//            if (!isInitialized)
//                return;

//            // float deviceScale = MobileAds.Utils.GetDeviceScale();
//            // UnityEngine.Console.Log("Device Scale is " + deviceScale);
//            //  float adWidth = Screen.width * 0.68376f;
//            // adWidth /= deviceScale;
//            AdSize adaptiveSize = AdSize.GetPortraitAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);   /*AdSize.GetPortraitAnchoredAdaptiveBannerAdSizeWithWidth((int)adWidth)*/;

//            this.smallbanner = new BannerView(_bannerID, adaptiveSize, position);

//#if !UNITY_EDITOR

//            int bannerHeight = ScaleThePixelsAccordingToDownScaleRatio(this.smallbanner.GetHeightInPixels());
//            int bannerWidth = ScaleThePixelsAccordingToDownScaleRatio(this.smallbanner.GetWidthInPixels());

//             int xDP = DensityIndependentPixels(((int)Screen.safeArea.position.x + ((int)Screen.safeArea.width / 2)) - (bannerWidth / 2));
//            int yDP = DensityIndependentPixels((Display.main.renderingHeight - (int)Screen.safeArea.position.y) - bannerHeight);
//            //  UnityEngine.Console.Log($"Main Height {Display.main.systemHeight} and Height {Screen.height} and safe area height is { (int)Screen.safeArea.height}");

//            // Create a 320x50 banner at the top of the screen.
//            this.smallbanner.SetPosition(xDP, yDP);
//#endif

//            //SubscribeToEvents
//            SubscribeSmallBannerCallbacks();
//            // Create an empty ad request.
//            AdRequest request = new AdRequest.Builder().Build();
//            // Load the banner with the request.
//            this.smallbanner.LoadAd(request);
//        }

//        private void RequestLargeBanner(AdPosition position)
//        {
//            DestroyLargeBanner();

//            if (!isInitialized)
//                return;

//            // Create a 320x50 banner at the top of the screen.
//            this.largebanner = new BannerView(_bannerID, AdSize.MediumRectangle, position);
//            //SubscribeToEvents
//            SubscribeLargeBannerCallbacks();
//            // Create an empty ad request.
//            AdRequest request = new AdRequest.Builder().Build();
//            // Load the banner with the request.
//            this.largebanner.LoadAd(request);
//        }

        #endregion ShowAdsMethods

        #region EventsSubscription

        private void SubscribeSmallBannerCallbacks()
        {
            // Called when an ad request has successfully loaded.
            // this.smallbanner.OnAdLoaded += this.HandleOnAdLoaded;
            // Called when an ad request failed to load.
            //this.smallbanner.OnAdFailedToLoad += (sender, args) =>
            //{
            //    UnityMainThreadDispatcher.Instance().Enqueue(() =>
            //    { DestroySmallBanner(false); });
            //};

            //this.smallbanner.OnAdLoaded += (sender, args) =>
            //{
            //    UnityEngine.Console.Log("Small Banner Loaded");
            //};
        }

        private void SubscribeLargeBannerCallbacks()
        {
            // Called when an ad request has successfully loaded.
            // this.largebanner.OnAdLoaded += this.HandleOnAdLoaded;
            // Called when an ad request failed to load.
            //this.largebanner.OnAdFailedToLoad += (sender, args) =>
            //{
            //    UnityMainThreadDispatcher.Instance().Enqueue(() =>
            //    { DestroyLargeBanner(); });
            //};
            //this.largebanner.OnAdLoaded += (sender, args) =>
            //{
            //    UnityMainThreadDispatcher.Instance().Enqueue(() =>
            //    {
            //        isLargeBannerShowing = true; LargeBannerShown.Invoke();
            //    });
            //};
        }

        #endregion EventsSubscription

        //Destroy Methods for banner and interstitial

        #region DestroyAds

        public void DestroySmallBanner(bool recordState = true)
        {
            //if (recordState)
            //{
            //    CurrentSmallBannerState = new CurrentBannerState(false, CurrentSmallBannerState.CurrentBannerPos);
            //}

            //if (this.smallbanner != null)
            //{
            //    this.smallbanner.Destroy();
            //    this.smallbanner = null;
            //}
        }

        public void DestroyLargeBanner()
        {
            //if (this.largebanner != null)
            //{
            //    this.largebanner.Destroy();
            //    this.largebanner = null;

            //    isLargeBannerShowing = false;
            //    LargeBannerHide.Invoke();
            //}
        }

        public void DestroyInterstiaial(object additionalInfo = null)
        {
            GoogleInterstitial interstitial = GetInterstitialFromID(additionalInfo);
            interstitial.DestroyInterstiaial();
        }

        public void DestroyAllInterstitialAds()
        {
            foreach (var item in googleInterstitialsUnique.Values)
            {
                item.DestroyInterstiaial();
            }
        }

        public void DestroyAllBanners()
        {
            DestroySmallBanner();
            DestroyLargeBanner();
        }

        public override void DestroyAllAds()
        {
            DestroyAllBanners();
            DestroyAllInterstitialAds();
        }

        #endregion DestroyAds

        #region HideBanners

        public override void HideSmallBanner()
        {
            //CurrentSmallBannerState = new CurrentBannerState(false, CurrentSmallBannerState.CurrentBannerPos);

            //if (smallbanner != null)
            //{
            //    smallbanner.Hide();
            //}
        }

        public override void HideLargeBanner()
        {
            //if (largebanner != null)
            //{
            //    largebanner.Hide();
            //    isLargeBannerShowing = false;
            //    LargeBannerHide.Invoke();
            //}
        }

        public void HideAllBanners()
        {
            HideSmallBanner();
            HideLargeBanner();
        }

        #endregion HideBanners

        #region ShowBannners

        //private int DensityIndependentPixels(int pixel)
        //{
        //    float resScale = (float)Display.main.renderingHeight / (float)Display.main.systemHeight;
        //    float deviceScale = MobileAds.Utils.GetDeviceScale() * resScale;
        //    return (int)(pixel / deviceScale);
        //}

        //private float ScreenPixels(float pixel)
        //{
        //    float pix = pixel;
        //    float deviceScale = MobileAds.Utils.GetDeviceScale();
        //    return (int)(pixel * deviceScale);
        //}

        private int ScaleThePixelsAccordingToDownScaleRatio(float pixel)
        {
            float resScale = (float)Display.main.renderingHeight / (float)Display.main.systemHeight;
            return (int)(pixel * resScale);
        }

        public override void ShowSmallBanner(BannerPosition bannerPosition)
        {
            //AdPosition adPosition = GetRelativeBannerPositionForCurrentPlugin<AdPosition>(bannerPosition);

            //// Save the position as current
            //CurrentSmallBannerState = new CurrentBannerState(true, bannerPosition);

            //SetSmallBannerPosition(adPosition);
        }

        public override void ShowLargeBanner(BannerPosition bannerPosition)
        {
           // AdPosition adPosition = GetRelativeBannerPositionForCurrentPlugin<AdPosition>(bannerPosition);
           // SetLargeBannerPosition(adPosition);
        }

//        private void SetSmallBannerPosition(AdPosition position)
//        {
//            if (this.smallbanner != null)
//            {
//                this.smallbanner.Show();

//#if UNITY_EDITOR
//                this.smallbanner.SetPosition(position);

//#else
//                int bannerHeight = ScaleThePixelsAccordingToDownScaleRatio(this.smallbanner.GetHeightInPixels());
//                int bannerWidth = ScaleThePixelsAccordingToDownScaleRatio(this.smallbanner.GetWidthInPixels());

//                int xDP = DensityIndependentPixels(((int)Screen.safeArea.position.x + ((int)Screen.safeArea.width / 2)) - (bannerWidth / 2));

//                int yDP = DensityIndependentPixels((Display.main.renderingHeight - (int)Screen.safeArea.position.y) - bannerHeight);
//                //  UnityEngine.Console.Log($"Main Height {Display.main.systemHeight} and Height {Screen.height} and safe area height is { (int)Screen.safeArea.height}");
//                this.smallbanner.SetPosition(xDP, yDP);
//#endif
//            }
//            else
//            {
//                RequestSmallBanner(position);
//            }
//        }

        //private void SetLargeBannerPosition(AdPosition position)
        //{
        //    if (this.largebanner != null)
        //    {
        //        largebanner.Show();
        //        this.largebanner.SetPosition(position);
        //        isLargeBannerShowing = true;
        //        LargeBannerShown.Invoke();
        //    }
        //    else
        //    {
        //        RequestLargeBanner(position);
        //    }
        //}

        //public override T GetRelativeBannerPositionForCurrentPlugin<T>(BannerPosition bannerPosition)
        //{
        //    AdPosition adPosition = bannerPosition == BannerPosition.Bottom ? AdPosition.Bottom : bannerPosition == BannerPosition.BottomLeft ? AdPosition.BottomLeft : bannerPosition == BannerPosition.BottomRight ? AdPosition.BottomRight
        //        : bannerPosition == BannerPosition.Center ? AdPosition.Center : bannerPosition == BannerPosition.Top ? AdPosition.Top : bannerPosition == BannerPosition.TopLeft ? AdPosition.TopLeft : bannerPosition == BannerPosition.TopRight ? AdPosition.TopRight : AdPosition.Bottom;

        //    T result = (T)Convert.ChangeType(adPosition, typeof(T));

        //    return result;
        //}

        #endregion ShowBannners

        public override bool CheckIfInterstitialAdIsAvailable(object additionalInfo = null)
        {
            if (additionalInfo == null)
            {
                foreach (var pair in googleInterstitials)
                {
                    bool isAvailable = pair.Value.CheckIfInterstitialAdIsAvailable();

                    if (isAvailable)
                        return true;
                }

                return false;
            }

            GoogleInterstitial interstitial = GetInterstitialFromID(additionalInfo);

            return interstitial.CheckIfInterstitialAdIsAvailable();
        }

        #region Rewarded

        // Loads the AdUnits In background so they can be displayed on demand
        // Instead of loading it at that time.

        public override void LoadRewardedAd()
        {
            // If we are using static interstital AD for rewarded

            if (useInterstitialAsRewardedAD)
            {
                GoogleInterstitial rewardedStaticInterstitial = GetStaticRewardedInterstital();

                rewardedStaticInterstitial.LoadInterstitial();

                return;
            }

            // Load the rewarded AD as normal

            if (isLoadingRewarded)
            {
                if (isDebug)
                    UnityEngine.Console.Log("Rewarded Ad is already being loaded");
                return;
            }

            //this.rewardedAd = new RewardedAd(_rewardedID);

            ////Subscribe to events
            //SubscribeRewardedCallbacks();
            //// Create an empty ad request.
            //AdRequest request = new AdRequest.Builder().Build();

            //isLoadingRewarded = true;
            //// Load the rewarded ad with the request.
            //this.rewardedAd.LoadAd(request);
        }

        public override void ShowRewardedAdIfAvailable(Action<Status, ADMeta> completionCalLBack)
        {
            pendingRewardedListener = completionCalLBack;

            if (!isInitialized)
            {
                // OnGoogleRewardedFailed.Invoke();
                AdManager.RewardedAdHasFailedToShow();
                ProcessPendingCallBack(AdType.Rewarded, Status.Failed);
                return;
            }

            // If we are using rewarded static interstital
            if (useInterstitialAsRewardedAD)
            {
                GoogleInterstitial rewardedStaticInterstitial = GetStaticRewardedInterstital();

                if (rewardedStaticInterstitial.isLoaded)
                {
                    rewardedStaticInterstitial.ShowInterstitialAdIfAvailable();
                }
                else
                {
                    AdManager.RewardedAdHasFailedToShow();
                    ProcessPendingCallBack(AdType.Rewarded, Status.Failed);
                    rewardedStaticInterstitial.LoadInterstitial();
                }

                return;
            }

            // Normal rewarded
            //if (this.rewardedAd != null && this.rewardedAd.IsLoaded())
            //{
            //    AdManager.RewardedADRightBeforeShow();

            //    CoroutineRunner.Instance.WaitForUpdateAndExecute(() => { this.rewardedAd.Show(); });
            //}
            //else
            //{
            //    AdManager.RewardedAdHasFailedToShow();
            //    ProcessPendingCallBack(AdType.Rewarded, Status.Failed);
            //    LoadRewardedAd();
            //}
        }

        private void SubscribeRewardedCallbacks()
        {
            // Called when an ad request has successfully loaded.
            //this.rewardedAd.OnAdLoaded += (ob, s) =>
            //{
            //    UnityMainThreadDispatcher.Instance().Enqueue(() =>
            //    {
            //        if (isDebug)
            //        {
            //            UnityEngine.Console.Log("Rewarded AD Loaded Admob");
            //        }

            //        isLoadingRewarded = false;

            //        string adapterName = null;
            //        if (this.rewardedAd != null)
            //        {
            //            var responseInfo = this.rewardedAd.GetResponseInfo();
            //            if (responseInfo != null)
            //            {
            //                adapterName = responseInfo.GetMediationAdapterClassName();
            //            }
            //        }

            //        AdManager.RewardedAdHasLoaded(this, new ADMeta(adapterName));
            //    });
            //};
            // Called when an ad request failed to load.
           // this.rewardedAd.OnAdFailedToLoad += (sender, e) => { isLoadingRewarded = false; };
            // Called when an ad is shown.
            //   this.rewardedAd.OnAdOpening += HandleRewardedAdOpening;
            // Called when an ad request failed to show.
            //this.rewardedAd.OnAdFailedToShow += (o, sender) =>
            //{
            //    UnityMainThreadDispatcher.Instance().Enqueue(() =>
            //    {
            //        //    OnGoogleRewardedFailed.Invoke();
            //        AdManager.RewardedAdHasFailedToShow();
            //        ProcessPendingCallBack(AdType.Rewarded, Status.Failed);
            //    });
            //};
            // Called when the user should be rewarded for interacting with the ad.
            //this.rewardedAd.OnUserEarnedReward += (s, reward) =>
            //{
            //    UnityMainThreadDispatcher.Instance().Enqueue(() =>
            //    {
            //        rewardEarned = true;
            //        //   OnGoogleRewardedCompleted.Invoke();

            //        string adapterName = null;
            //        if (this.rewardedAd != null)
            //        {
            //            var responseInfo = this.rewardedAd.GetResponseInfo();
            //            if (responseInfo != null)
            //            {
            //                adapterName = responseInfo.GetMediationAdapterClassName();
            //            }
            //        }

            //        AdManager.RewardedAdHasCompleted(new ADMeta(adapterName));
            //        ProcessPendingCallBack(AdType.Rewarded, Status.Succeded, new ADMeta(adapterName));
            //    });
            //};


            //// Called when the ad is closed.
            //this.rewardedAd.OnAdClosed += (o, s) =>
            //{
            //    UnityMainThreadDispatcher.Instance().Enqueue(() =>
            //    {
            //        // Closeed before completion
            //        if (!rewardEarned)
            //        {
            //            //  OnGoogleRewardedFailed.Invoke();
            //            AdManager.RewardedAdHasSkipped();
            //            ProcessPendingCallBack(AdType.Rewarded, Status.Skipped);
            //        }

            //        rewardEarned = false;
            //        LoadRewardedAd();
            //        AdManager.RewardedAdHasBeenClosed();
            //    });
            //};

            //this.rewardedAd.OnAdOpening += (o, s) =>
            //{
            //    UnityMainThreadDispatcher.Instance().Enqueue(() =>
            //    {
            //        AdManager.RewardedAdHasOpened();
            //    });
            //};
        }

        public override bool CheckIfRewardedAdIsAvailable()
        {
            //// If we are using static interstitial as rewarded
            //if (useInterstitialAsRewardedAD)
            //{
            //    GoogleInterstitial rewardedStaticInterstitial = GetStaticRewardedInterstital();

            //    return rewardedStaticInterstitial.isLoaded;
            //}

            //// Normal rewarded AD

            //bool isLoaded = this.rewardedAd != null && this.rewardedAd.IsLoaded();
            ////if (!isLoaded)
            ////{
            ////    LoadRewardedAd();
            ////}

            //return (this.rewardedAd != null && isLoaded);
            return false;
        }

        public void SendRewardedADHasLoaded(ADMeta aDMeta)
        {
            AdManager.RewardedAdHasLoaded(this, aDMeta);
        }

        public void SendInterstitialADHasLoaded(ADMeta aDMeta)
        {
            AdManager.InterstitialADHasLoaded(aDMeta);
        }

        public void SendRewardedADFailedToLoad()
        {
        }

        public void SendRewardedADFailedToShow()
        {
            AdManager.RewardedAdHasFailedToShow();
            ProcessPendingCallBack(AdType.Rewarded, Status.Failed);
        }

        public void SendOnUserEarnedReward(ADMeta aDMeta)
        {
            AdManager.RewardedAdHasCompleted(aDMeta);
            ProcessPendingCallBack(AdType.Rewarded, Status.Succeded, aDMeta);
        }

        public void SendRewardedADClosed()
        {
            AdManager.RewardedAdHasBeenClosed();
        }

        public void SendOnRewardedADOpened()
        {
            AdManager.RewardedAdHasOpened();
        }

        private GoogleInterstitial GetStaticRewardedInterstital()
        {
            GoogleInterstitial rewardedStaticInterstitial;
            googleInterstitials.TryGetValue("Rewarded", out rewardedStaticInterstitial);

            if (rewardedStaticInterstitial == null)
            {
                throw new System.Exception("Trying to load rewarded static interstital when there's no rewarded interstitial ad referenced in google ads plugin");
            }

            return rewardedStaticInterstitial;
        }

        #endregion Rewarded

        protected void ProcessPendingCallBack(AdType adType, Status status, ADMeta aDMeta = default)
        {
            Action<Status, ADMeta> pendingCallBack = adType == AdType.Rewarded ? pendingRewardedListener : null;

            if (pendingCallBack == null)
            {
                UnityEngine.Console.LogWarning($"A call to process AD pending callback was made but there's not callback!. AdType {adType} and status {status}");
                return;
            }

            pendingCallBack(status, aDMeta);
            pendingCallBack = null;
        }

        private GoogleInterstitial GetInterstitialFromID(object id)
        {
            string additionalData = id as string;

            //  UnityEngine.Console.Log("ADdition Data is " + additionalData);

            if (additionalData == null)
            {
                if (isDebug)
                    UnityEngine.Console.LogWarning("Can't find interstial AD as ID supplied is null");
                return null;
            }

            GoogleInterstitial interstitial;
            googleInterstitials.TryGetValue(additionalData, out interstitial);

            return interstitial;
        }

        public void InterstitialADHasFailed()
        {
            AdManager.InterstitialAdHasFailedToShow();
        }

        public void InterstitialADHasSucceeded(ADMeta aDMeta)
        {
            AdManager.InterstitialAdHasCompleted(aDMeta);
        }

        public void InterstitialADAboutToShow()
        {
            AdManager.InterstitialADAboutToShow();
        }

        public void InterstitialADRightBeforeShow()
        {
            AdManager.InterstitialADRightBeforeShow();
        }

        public void RewardedADRightBeforeShow()
        {
            AdManager.RewardedADRightBeforeShow();
        }
    }
}