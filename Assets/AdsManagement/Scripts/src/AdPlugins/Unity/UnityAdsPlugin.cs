using System;
using UnityEngine;
using UnityEngine.Advertisements;

namespace TheKnights.AdsSystem
{
    [CreateAssetMenu(fileName = "UnityAdsPlugin", menuName = "ScriptableObjects/UnityAdsPlugin", order = 2)]
    public class UnityAdsPlugin : AdPlugin //, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
    {
        [SerializeField] protected string gameID;

     //   [Tooltip("If true then the Load AD must be called before calling Show")]
       // [SerializeField] protected bool enablePerPlacementLoad;

        [Tooltip("Upon initialization loads and caches the ads to be used later on.Only works if perPlacementLoad is disabled")]
        [SerializeField] private bool preLoadADS;

        private bool isLoadingRewarded;
        private bool isLoadingInterstitial;

        private bool isInterstitialReady;
        private bool isRewardedReady;

        private static bool isInitializedPluginSpecific;


        protected override void OnEnable()
        {
            base.OnEnable();
            isLoadingRewarded = false;
            isLoadingInterstitial = false;
            isInterstitialReady = false;
            isRewardedReady = false;
        }

        public override void InitializeAds()
        {
            //if (isInitializedPluginSpecific || isInitialized)
            //{
            //    isInitializationOperationDone = true;
            //    return;
            //}
            

            //if (!Advertisement.isSupported)
            //{
            //    if (isDebug)
            //        UnityEngine.Console.LogWarning("Unity Ads are not supported on this platform");

            //    isInitializationOperationDone = true;

            //    return;
            //}

            //isInitializationOperationDone = false;
            //Advertisement.Initialize(gameID, testMode,/* enablePerPlacementLoad,*/ this);
            //isInitializationOperationDone = true;
        }

        public override bool CheckIfInterstitialAdIsAvailable(object additionalInfo = null)
        {
            string id = additionalInfo as string;

            if (id == "Static")
            {
                if (isDebug)
                    UnityEngine.Console.Log("Unity ADS Doesn't support static ADS. Ignoring check if loaded request for static AD");

                return false;
            }

            //  bool isAdvertisementReady = Advertisement.IsReady(interstitialID);

            if (!isInterstitialReady)
            {
                if (isDebug)
                    UnityEngine.Console.Log("Interstitial wans't ready. Requesting To Load");

                LoadInterstitialAd();
            }

            return isInitialized && isInterstitialReady;
        }

        public override bool CheckIfRewardedAdIsAvailable()
        {

           // bool isAdvertisementReady = Advertisement.IsReady(rewardedID);

            if (!isRewardedReady)
            {
                if (isDebug)
                    UnityEngine.Console.Log("Rewarded wans't ready. Requesting To Load");

              //  LoadRewardedAd();
            }

            return isInitialized && isRewardedReady;
        }

        public override void ShowLargeBanner(BannerPosition bannerPosition)
        {
            //  throw new NotImplementedException();
        }

        public override void ShowSmallBanner(BannerPosition bannerPosition)
        {
            //throw new NotImplementedException();
        }

        //public override T GetRelativeBannerPositionForCurrentPlugin<T>(BannerPosition bannerPosition)
        //{
        //    //  throw new NotImplementedException();
        //    return default(T);
        //}

        // Pending Interstitial Listeners
        private Action<Status, ADMeta> pendingInterstitialListener;

        // Pending Rewarded Listeners
        private Action<Status, ADMeta> pendingRewardedListener;

        public override void ShowInterstitialAdIfAvailable(Action<Status, ADMeta> completionCalLBack, object additionalInfo = null)
        {
            //pendingInterstitialListener = completionCalLBack;

            //if (!CheckIfInterstitialAdIsAvailable())
            //{
            //    AdManager.InterstitialAdHasFailedToShow();
            //    ProcessPendingCallBack(AdType.Interstital, Status.Failed);
            //    return;
            //}

            //// Note that if the ad content wasn't previously loaded, this method will fail

            //if (isDebug)
            //    UnityEngine.Console.Log("Showing Ad: " + interstitialID);

            //AdManager.InterstitialADRightBeforeShow();

            //CoroutineRunner.Instance.WaitForUpdateAndExecute(() => { Advertisement.Show(interstitialID, this); });
          
        }

        public override void LoadInterstitialAd(string adType = null)
        {
            // IMPORTANT! Only load content AFTER initialization

            //if (!isInitialized || isLoadingInterstitial || isInterstitialReady /*|| !enablePerPlacementLoad*/)
            //    return;

            //isLoadingInterstitial = true;

            //if (isDebug)
            //    UnityEngine.Console.Log("Loading Ad: " + interstitialID);

            //Advertisement.Load(interstitialID, this);
        }

        public override void LoadRewardedAd()
        {
            // IMPORTANT! Only load content AFTER initialization

            //if (!isInitialized || isLoadingRewarded || isRewardedReady /*|| !enablePerPlacementLoad*/)
            //    return;

            //isLoadingRewarded = true;

            //if (isDebug)
            //    UnityEngine.Console.Log("Loading Ad: " + rewardedID);

            //Advertisement.Load(rewardedID, this);
        }

        public override void DestroyAllAds()
        {
        }

        public override void ShowRewardedAdIfAvailable(Action<Status, ADMeta> completionCalLBack)
        {
            //pendingRewardedListener = completionCalLBack;

            //if (!CheckIfRewardedAdIsAvailable())
            //{
            //    LoadRewardedAd();
            //    AdManager.RewardedAdHasFailedToShow();
            //    ProcessPendingCallBack(AdType.Rewarded, Status.Failed);
            //    return;
            //}

            //// Note that if the ad content wasn't previously loaded, this method will fail

            //if (isDebug)
            //{
            //    UnityEngine.Console.Log("Showing Ad: " + rewardedID);
            //}

            //AdManager.RewardedADRightBeforeShow();

            //CoroutineRunner.Instance.WaitForUpdateAndExecute(() => { Advertisement.Show(rewardedID, this); });

          
        }

        public void OnInitializationComplete()
        {
            if (isDebug)
                UnityEngine.Console.Log("Unity Ads initialization complete.");

            isInitializationOperationDone = true;
            isInitializedPluginSpecific = true;
            isInitialized = true;

            PreLoadAds();
        }

        //public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        //{
        //    if (isDebug)
        //        UnityEngine.Console.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");

        //    isInitializationOperationDone = true;

        //}

        //public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
        //{
        //    UnityMainThreadDispatcher.Instance().Enqueue(() =>
        //    {
        //        if (isDebug)
        //            UnityEngine.Console.Log($"Error showing Ad Unit {placementId}: {error.ToString()} - {message}");

        //        if (placementId == rewardedID)
        //        {
        //            isRewardedReady = false;
        //            AdManager.RewardedAdHasFailedToShow();
        //            ProcessPendingCallBack(AdType.Rewarded, Status.Failed);
        //        }
        //        else if (placementId == interstitialID)
        //        {
        //            isInterstitialReady = false;
        //            AdManager.InterstitialAdHasFailedToShow();
        //            ProcessPendingCallBack(AdType.Interstital, Status.Failed);
        //        }
        //    });
        //}

        public void OnUnityAdsShowStart(string placementId)
        {
            if (placementId == rewardedID)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    if (placementId == rewardedID)
                    {
                        AdManager.RewardedAdHasOpened();
                    }
                });
            }
            else if (placementId == interstitialID)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    if (placementId == rewardedID)
                    {
                        AdManager.InterstitialADAboutToShow();
                    }
                });
            }
        }

        public void OnUnityAdsShowClick(string placementId)
        {
        }

        //public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
        //{
        //    UnityMainThreadDispatcher.Instance().Enqueue(() =>
        //    {
        //        // What about other statuses

        //        if(placementId == rewardedID)
        //        {
        //            isRewardedReady = false;
        //        }

        //        if (placementId == rewardedID && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        //        {
        //            if (isDebug)
        //                UnityEngine.Console.Log("Unity Ads Rewarded Ad Completed");

        //            // Grant a reward.
        //            AdManager.RewardedAdHasCompleted();
        //            ProcessPendingCallBack(AdType.Rewarded, Status.Succeded);

        //            // Load another ad:
        //            LoadRewardedAd();
        //        }
        //        else if (placementId == rewardedID && !showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        //        {
        //            if (isDebug)
        //                UnityEngine.Console.Log($"Unity Ads Rewarded Ad Ended With Status {showCompletionState}");

        //            AdManager.RewardedAdHasFailedToShow(); // change
        //            Status status = showCompletionState == UnityAdsShowCompletionState.SKIPPED ? Status.Skipped : Status.Unknown;
        //            ProcessPendingCallBack(AdType.Rewarded, status);
      
        //        }
        //        else if (placementId == interstitialID /*&& showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED)*/)
        //        {
        //            if (isDebug)
        //                UnityEngine.Console.Log("Unity Interstitial Ad Completed");

        //            isInterstitialReady = false;
        //            AdManager.InterstitialAdHasCompleted();
        //            ProcessPendingCallBack(AdType.Interstital, Status.Succeded);

        //            LoadInterstitialAd();
        //        }
        //    });
        //}

        public void OnUnityAdsAdLoaded(string placementId)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (isDebug)
                    UnityEngine.Console.Log("Unity Ad Loaded " + placementId);

                if (placementId == rewardedID)
                {
                    isLoadingRewarded = false;
                    isRewardedReady = true;
                    AdManager.RewardedAdHasLoaded(this);
                }
                else if (placementId == interstitialID)
                {
                    isInterstitialReady = true;
                    isLoadingInterstitial = false;
                    AdManager.InterstitialADHasLoaded();
                }
            });
        }

        //public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
        //{
        //    UnityMainThreadDispatcher.Instance().Enqueue(() =>
        //    {
        //        if (isDebug)
        //            UnityEngine.Console.Log($"Error loading Ad Unit: {placementId} - {error.ToString()} - {message}");

        //        if (placementId == rewardedID)
        //        {
        //            isRewardedReady = false;
        //            isLoadingRewarded = false;
        //        }
        //        else if (placementId == interstitialID)
        //        {
        //            isInterstitialReady = false;
        //            isLoadingInterstitial = false;
        //        }
        //    });
        //}

        public override void HideSmallBanner()
        {
        }

        public override void HideLargeBanner()
        {
        }

        protected void ProcessPendingCallBack(AdType adType, Status status, ADMeta aDMeta = default)
        {
            Action<Status, ADMeta> pendingCallBack = adType == AdType.Interstital ? pendingInterstitialListener : adType == AdType.Rewarded ? pendingRewardedListener : null;

            if (pendingCallBack == null)
            {
                UnityEngine.Console.LogWarning($"A call to process AD pending callback was made but there's not callback!. AdType {adType} and status {status}");
                return;
            }

            pendingCallBack(status,aDMeta);
            pendingCallBack = null;
        }

        public override void PreLoadAds()
        {
            if (preLoadADS)
            {
                LoadInterstitialAd();
                LoadRewardedAd();
            }
        }
    }
}