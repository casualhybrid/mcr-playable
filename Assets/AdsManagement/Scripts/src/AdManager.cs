using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TheKnights.AdsSystem
{
    public struct ADMeta 
    {
        private string _adapterName;

        public string AdapterName
        {
            get
            {
                if (_adapterName == null)
                {
                    _adapterName = "Unknown";
                }

                return _adapterName;
            }
        }

        public ADMeta(string adapterName)
        {
            if (String.IsNullOrEmpty(adapterName))
                adapterName = "Unknown";

            _adapterName = adapterName;
        }
    }

    public enum Status
    {
        Failed, Succeded, Skipped, Unknown
    }

    [CreateAssetMenu(fileName = "AdManager", menuName = "ScriptableObjects/AdManager", order = 1)]
    public class AdManager : ScriptableObject
    {
        //Events the caller would need to subscribe in order to know when a specific AD has been completed (Either through failure or Success)
        //A common example consists of the following steps

        // -> Caller subscribe to RewardedAdCompleted and RewardedADFailedToShow Events
        // -> Caller Disables resource intensive operations such as changing the timescale to zero, disabling the cameras and so on
        // -> Caller receives the event, desubscribing to them, re enabling the operations and continuing the game

        [HideInInspector] public UnityEvent<bool, ADMeta> OnRewardedAdLoaded;
        [HideInInspector] public UnityEvent OnRewardedADAboutToShow;
        [HideInInspector] public UnityEvent<ADMeta> OnInterstitialADLoaded;
        [HideInInspector] public UnityEvent OnInterstitialADRightBeforeShow;
        [HideInInspector] public UnityEvent OnInterstitialADAboutToShow;
        [HideInInspector] public UnityEvent<ADMeta> OnRewardedAdCompleted;
        [HideInInspector] public UnityEvent OnRewardedAdFailedToShow;
        [HideInInspector] public UnityEvent OnRewardedAdHasBeenSkipped;
        [HideInInspector] public UnityEvent OnRewardedAdHasBeenClosed;
        [HideInInspector] public UnityEvent OnRewardedADRightBeforeShow;
        [HideInInspector] public UnityEvent<ADMeta> OnInterstitialAdCompleted;
        [HideInInspector] public UnityEvent OnInterstitialAdFailedToShow;

        [Header("Global AD Settings")]
        [Tooltip("Sets the global status of ADS")]
        [SerializeField] private bool enableADS = true;

        [SerializeField] private List<AdPlugin> AdPluginsAll;

        [Tooltip("Show ADS Debug Info in console")]
        [SerializeField] private bool isDebug;

        [Space]
        [Header("Interstitial Ad Settings")]
        [Tooltip("Should order of list be considered when showing Interstitial Ads")]
        [SerializeField] private bool isInterstitalPriority;

        [SerializeField] private List<AdPlugin> AdPluginsPriortizedForInterstitial;
        [SerializeField] private AdPlugin AdPluginNonPriortizedForInterstitial;

        [Space]
        [Header("Rewarded Ad Settings")]
        [Tooltip("Should order of list be considered when showing rewarded Ads")]
        [SerializeField] private bool isRewardedPriority;

        [SerializeField] private List<AdPlugin> AdPluginsPriortizedForRewarded;
        [SerializeField] private AdPlugin AdPluginNonPriortizedForRewarded;

        [Space]
        [Header("********BANNER SETTINGS **************")]
        [Space]
        [Header("Small Banner Settings")]
        [SerializeField] private bool isPrioritySmallBanners;

        [SerializeField] private List<AdPlugin> AdPluginsPriortizedForSmallBanners;
        [SerializeField] private AdPlugin AdPluginNonPriortizedSmallBanner;

        [Header("Large Banner Settings")]
        [SerializeField] private bool isPriorityLargeBanners;

        [SerializeField] private List<AdPlugin> AdPluginsPriortizedForLargeBanners;
        [SerializeField] private AdPlugin AdPluginNonPriortizedLargeBanner;

        public bool AreAdsEnabled => _enableADS;

        private bool _enableADS = true;

        private void OnEnable()
        {
            _enableADS = enableADS;

            //Just making sure that the All plugin list isn't missing any of the plugins being used in the priorities lists
            bool error = false;

            foreach (var V in AdPluginsPriortizedForInterstitial)
            {
                error = !AdPluginsAll.Contains(V);
            }

            foreach (var V in AdPluginsPriortizedForRewarded)
            {
                error = !AdPluginsAll.Contains(V);
            }

            if (error)
                throw new System.Exception("An AD Plugin being used is missing in the Global Plugins List");
        }

        /// <summary>
        /// Disables the ads gloablly preventing initialization
        /// </summary>
        public void DisableADSGlobally()
        {
            _enableADS = false;
        }

        /// <summary>
        /// Enable the ads gloablly
        /// </summary>
        public void EnableADSGlobally()
        {
            _enableADS = true;
        }

        /// <summary>
        /// Initializes each AD Plugin based on the order in the list
        /// </summary>
        public Coroutine InitializeAds()
        {
            if (!_enableADS)
            {
                UnityEngine.Console.LogWarning("Caution! ADS are disabled globally.");
                return null;
            }

            return CoroutineRunner.Instance.StartCoroutine(InitializeAdsRoutine());
        }

        private IEnumerator InitializeAdsRoutine()
        {
            foreach (var V in AdPluginsAll)
            {
                V.InitializeAds();

                yield return new WaitUntil(() => V.isInitializationOperationDone);
                yield return null;
            }

            yield return null;
        }

        /// <summary>
        /// LoadsTheInterstialAd of the specidif plugin or all plugins if null is passed
        /// </summary>
        public void LoadInterstitialAd(AdPlugin adPlugin, string adType = null)
        {
            if (!_enableADS)
            {
                if (isDebug)
                    UnityEngine.Console.Log("Not loading Interstitial Ads as Ads are disabled");

                return;
            }

            // Load interstitial AD for all plugins
            if (adPlugin == null)
            {
                foreach (var plugin in AdPluginsAll)
                {
                    plugin.LoadInterstitialAd(adType);
                }

                return;
            }

            if (!AdPluginsAll.Contains(adPlugin))
            {
                UnityEngine.Console.LogWarning($"Attempting to load interstitial AD for plugin which is not present in adPluginsALL {adPlugin.name}");
                return;
            }

            adPlugin.LoadInterstitialAd(adType);
        }

        /// <summary>
        /// Loads the rewarded of the specidif plugin
        /// </summary>
        public void LoadRewardedAd(AdPlugin adPlugin)
        {
            if (!_enableADS)
            {
                if (isDebug)
                    UnityEngine.Console.Log("Not loading rewarded Ads as Ads are disabled");

                return;
            }

            if (!AdPluginsAll.Contains(adPlugin))
            {
                UnityEngine.Console.LogWarning($"Attempting to load rewarded AD for plugin which is not present in adPluginsALL {adPlugin.name}");
                return;
            }

            adPlugin.LoadRewardedAd();
        }

        public void PreLoadTheAds(AdPlugin adPlugin)
        {
            Debug.Log("Ads: ");
            if (!_enableADS)
            {
                if (isDebug)
                    UnityEngine.Console.Log("Not Preloading Ads as Ads are disabled");

                return;
            }

            if (!AdPluginsAll.Contains(adPlugin))
            {
                UnityEngine.Console.LogWarning($"Attempting to Preload AD plugin which is not present in adPluginsALL {adPlugin.name}");
                return;
            }

            adPlugin.PreLoadAds();
        }

        /// <summary>
        /// Loads the rewarded ads for every ad plugin
        /// </summary>
        public void LoadAllRewardedAds()
        {
            if (!_enableADS)
            {
                if (isDebug)
                    UnityEngine.Console.Log("Not loading rewarded Ads as Ads are disabled");

                return;
            }

            foreach (var adPlugin in AdPluginsAll)
            {
                adPlugin.LoadRewardedAd();
            }
        }

        ///// <summary>
        ///// Show the Ad based on the chosen configuration and adType
        ///// </summary>
        ///// <param name="adType">The type of Ad to be shown</param>
        //public void ShowAd(AdType adType, Action<Status> completionCalLBack = null, BannerType bannerType = BannerType.SmallBanner, BannerPosition bannerPosition = BannerPosition.Bottom)
        //{
        //    if (!_enableADS)
        //    {
        //        if (isDebug)
        //            UnityEngine.Console.Log("Not Showing Ads as global ads are disabled");

        //        // If the ads are disabled then invoke the relative faliure to show AD event
        //        if (adType == AdType.Interstital)
        //        {
        //            InterstitialAdHasFailedToShow();
        //        }
        //        else if (adType == AdType.Rewarded)
        //        {
        //            RewardedAdHasFailedToShow();
        //        }

        //        completionCalLBack(Status.Failed);
        //        return;
        //    }

        //    //Show the ad according to the type
        //    if (adType == AdType.Banner)
        //    {
        //        ShowBannerAd(bannerType, bannerPosition);
        //    }
        //    else if (adType == AdType.Interstital)
        //    {
        //        ShowInterstitialAd(completionCalLBack);
        //    }
        //    else if (adType == AdType.Rewarded)
        //    {
        //        ShowRewardedAd(completionCalLBack);
        //    }
        //}

        /// <summary>
        /// Hides the banner
        /// </summary>
        public void HideBanner(BannerType bannerType = BannerType.SmallBanner)
        {
            if (bannerType == BannerType.SmallBanner)
            {
                AdPluginNonPriortizedSmallBanner.HideSmallBanner();
            }
            else if (bannerType == BannerType.LargeBanner)
            {
                AdPluginNonPriortizedSmallBanner.HideLargeBanner();
            }
        }

        /// <summary>
        /// Destroys all cached ads completely
        /// </summary>
        public void DestroyAllAds()
        {
            foreach (var V in AdPluginsAll)
            {
                V.DestroyAllAds();
            }
        }

        public void ShowInterstitialAd(Action<Status, ADMeta> completionCalLBack, string adType = null)
        {
            if (!_enableADS)
            {
                if (isDebug)
                    UnityEngine.Console.Log("Not Showing Interstitial Ads as global ads are disabled");

                InterstitialAdHasFailedToShow();

                completionCalLBack?.Invoke(Status.Failed, new ADMeta());
                return;
            }

            if (isInterstitalPriority)
            {
                AdPlugin adPlugin = GetAdPluginBasedOnPriority(AdType.Interstital, adType);

                if (adPlugin != null)
                {
                    adPlugin.ShowInterstitialAdIfAvailable(completionCalLBack, adType);
                }
                else
                {
                    InterstitialAdHasFailedToShow();
                    completionCalLBack?.Invoke(Status.Failed, new ADMeta());
                }
            }
            else
            {
                AdPluginNonPriortizedForInterstitial.ShowInterstitialAdIfAvailable(completionCalLBack, adType);
            }
        }

        public bool CheckIfInterstitialADIsAvailable(string adType = null)
        {
            if (!_enableADS)
            {
                if (isDebug)
                    UnityEngine.Console.Log("Can't check if interstitial is available as ADS are OFF");

                return false;
            }

            if (!isInterstitalPriority)
            {
                return AdPluginNonPriortizedForInterstitial.CheckIfInterstitialAdIsAvailable(adType);
            }
            else
            {
                AdPlugin adPlugin = GetAdPluginBasedOnPriority(AdType.Interstital, adType);

                return adPlugin != null;
            }
        }

        public bool CheckIfRewardedADIsAvailable()
        {
            if (!_enableADS)
            {
                if (isDebug)
                    UnityEngine.Console.Log("Can't check if rewarded is available as ADS are OFF");

                return false;
            }

            if (!isRewardedPriority)
            {
                return AdPluginNonPriortizedForRewarded.CheckIfRewardedAdIsAvailable();
            }
            else
            {
                AdPlugin adPlugin = GetAdPluginBasedOnPriority(AdType.Rewarded);

                return adPlugin != null;
            }
        }


        public bool CheckIfStaticInterstitialADIsAvailable()
        {
            if (!_enableADS)
            {
                if (isDebug)
                    UnityEngine.Console.Log("Can't check if static interstitial is available as ADS are OFF");

                return false;
            }

            if (!isInterstitalPriority)
            {
                return AdPluginNonPriortizedForInterstitial.CheckIfInterstitialAdIsAvailable("Static");
            }
            else
            {
                AdPlugin adPlugin = GetAdPluginBasedOnPriority(AdType.Interstital, "Static");

                return adPlugin != null;
            }
        }

        public bool CheckIfParticularRewardedADIsAvailable(AdPlugin adPlugin)
        {
            if (!AdPluginsAll.Contains(adPlugin))
            {
                UnityEngine.Console.LogWarning($"Attempting to check rewarded AD availability for plugin which is not present in adPluginsALL {adPlugin.name}");
                return false;
            }

            return adPlugin.CheckIfRewardedAdIsAvailable();
        }

        public bool CheckIfPriorityRewardedADIsAvailable()
        {
            if (!_enableADS)
            {
                if (isDebug)
                    UnityEngine.Console.Log("Cant check whether priority ad is available as global ads are disabled");

                return false;
            }

            if (isRewardedPriority)
            {
                AdPlugin adPlugin = GetThePriorityADPlugin(AdType.Rewarded);

                if (adPlugin != null)
                {
                    return adPlugin.CheckIfRewardedAdIsAvailable();
                }
                else
                {
                    UnityEngine.Console.LogWarning("Theres no priority rewarded AD");
                    return false;
                }
            }
            else
            {
                if (isDebug)
                {
                    UnityEngine.Console.LogWarning("Request to check whether priority rewarded AD is available was made but the piority rewarded AD is OFF");
                }

                return AdPluginNonPriortizedForRewarded.CheckIfRewardedAdIsAvailable();
            }
        }

        public void ShowRewardedAd(Action<TheKnights.AdsSystem.Status, ADMeta> completionCalLBack, bool usePriorityPluginOnly = false)
        {
            if (!_enableADS)
            {
                if (isDebug)
                    UnityEngine.Console.Log("Not Showing Rewarded as global ads are disabled");

                // If the ads are disabled then invoke the relative faliure to show AD event

                RewardedAdHasFailedToShow();

                completionCalLBack?.Invoke(Status.Failed, new ADMeta());
                return;
            }

            if (isRewardedPriority)
            {
                AdPlugin adPlugin = !usePriorityPluginOnly ? GetAdPluginBasedOnPriority(AdType.Rewarded) : GetThePriorityADPlugin(AdType.Rewarded);

                if (adPlugin != null)
                {
                    adPlugin.ShowRewardedAdIfAvailable(completionCalLBack);
                }
                else
                {
                    RewardedAdHasFailedToShow();
                    completionCalLBack?.Invoke(Status.Failed, new ADMeta());
                }
            }
            else
            {
                AdPluginNonPriortizedForRewarded.ShowRewardedAdIfAvailable(completionCalLBack);
            }
        }

        public void ShowBannerAd(BannerType bannerType, BannerPosition bannerPosition)
        {
            if (!_enableADS)
            {
                if (isDebug)
                    UnityEngine.Console.Log("Not Showing Banner as global ads are disabled");

                return;
            }

            if (bannerType == BannerType.SmallBanner)
            {
                ShowSmallBannerAd(bannerPosition);
            }
            else if (bannerType == BannerType.LargeBanner)
            {
                ShowLargeBannerAd(bannerPosition);
            }
        }

        public CurrentBannerState GetCurrentSmallBannerState()
        {
            if (!_enableADS)
            {
                return new CurrentBannerState();
            }

            return AdPluginNonPriortizedSmallBanner.CurrentSmallBannerState;
        }

        private void ShowSmallBannerAd(BannerPosition bannerPosition)
        {
            AdPluginNonPriortizedSmallBanner.ShowSmallBanner(bannerPosition);
        }

        private void ShowLargeBannerAd(BannerPosition bannerPosition)
        {
            AdPluginNonPriortizedLargeBanner.ShowLargeBanner(bannerPosition);
        }

        //Returns the Ad Plugin based on the current priority
        private AdPlugin GetAdPluginBasedOnPriority(AdType adType, object additionalInfo = null)
        {
            if (adType == AdType.Interstital)
            {
                foreach (var v in AdPluginsPriortizedForInterstitial)
                {
                    if (v.CheckIfInterstitialAdIsAvailable(additionalInfo))
                    {
                        return v;
                    }
                }
            }
            else if (adType == AdType.Rewarded)
            {
                foreach (var v in AdPluginsPriortizedForRewarded)
                {
                    if (v.CheckIfRewardedAdIsAvailable())
                    {
                        return v;
                    }
                }
            }

            if (isDebug)
                UnityEngine.Console.Log("No Ad of type " + adType + "is available");

            return null;
        }

        private AdPlugin GetThePriorityADPlugin(AdType adType)
        {
            if (adType == AdType.Interstital)
            {
                if (AdPluginsPriortizedForInterstitial.Count == 0)
                {
                    UnityEngine.Console.LogWarning("There are no plugins in the priority list interstitial");
                    return null;
                }

                return AdPluginsPriortizedForInterstitial[0];
            }
            else if (adType == AdType.Rewarded)
            {
                if (AdPluginsPriortizedForRewarded.Count == 0)
                {
                    UnityEngine.Console.LogWarning("There are no plugins in the priority list rewarded");
                    return null;
                }

                return AdPluginsPriortizedForRewarded[0];
            }

            if (isDebug)
                UnityEngine.Console.Log("No Priority Plugin of type " + adType + "is available");

            return null;
        }

        #region EventsInvokations

        public void RewardedAdHasOpened()
        {
            OnRewardedADAboutToShow.Invoke();

            if (isDebug)
                UnityEngine.Console.Log("Rewarded Ad about to show");
        }

        public void InterstitialADHasLoaded(ADMeta aDMeta = default)
        {
            OnInterstitialADLoaded.Invoke(aDMeta);

            if (isDebug)
            {
                UnityEngine.Console.Log("Interstitial AD loaded");
            }
        }

        public void RewardedAdHasLoaded(AdPlugin adPlugin, ADMeta aDMeta = default)
        {
            AdPlugin priorityPlugin = GetThePriorityADPlugin(AdType.Rewarded);

            OnRewardedAdLoaded.Invoke(priorityPlugin == adPlugin, aDMeta);

            if (isDebug)
            {
                UnityEngine.Console.Log("Rewarded AD has loaded");
            }
        }

        public void RewardedAdHasCompleted(ADMeta aDMeta = default)
        {
            OnRewardedAdCompleted.Invoke(aDMeta);

            if (isDebug)
                UnityEngine.Console.Log("Rewarded Ad Completed");
        }

        public void RewardedADRightBeforeShow()
        {
            OnRewardedADRightBeforeShow.Invoke();
        }

        public void RewardedAdHasFailedToShow()
        {
            OnRewardedAdFailedToShow.Invoke();

            if (isDebug)
                UnityEngine.Console.Log("Rewarded Ad Failed to show");
        }

        public void RewardedAdHasSkipped()
        {
            OnRewardedAdHasBeenSkipped.Invoke();

            if (isDebug)
                UnityEngine.Console.Log("Rewarded Ad has been skipped");
        }

        public void RewardedAdHasBeenClosed()
        {
            OnRewardedAdHasBeenClosed.Invoke();

            if (isDebug)
                UnityEngine.Console.Log("Rewarded Ad has been closed");
        }


        public void InterstitialAdHasCompleted(ADMeta aDMeta = default)
        {
            if (isDebug)
            {
                UnityEngine.Console.Log("Interstitial Ad Completed");
                UnityEngine.Console.Log("sending interstial completed");
            }

            OnInterstitialAdCompleted.Invoke(aDMeta);
        }

        public void InterstitialAdHasFailedToShow()
        {
            if (isDebug)
            {
                UnityEngine.Console.Log("Interstitial Ad Failed to show");
                UnityEngine.Console.Log("sending interstial failed to  completed");
            }

            OnInterstitialAdFailedToShow.Invoke();
        }

        public void InterstitialADAboutToShow()
        {
            //  UnityEngine.Console.Log("sending interstial about to show");
            OnInterstitialADAboutToShow.Invoke();

            if (isDebug)
                UnityEngine.Console.Log("Interstitial Ad about to show");
        }

        public void InterstitialADRightBeforeShow()
        {
            //  UnityEngine.Console.Log("sending interstial about to show");
            OnInterstitialADRightBeforeShow.Invoke();

            if (isDebug)
                UnityEngine.Console.Log("Interstitial Ad Right before show");
        }

        #endregion EventsInvokations
    }
}