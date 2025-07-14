using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace TheKnights.AdsSystem
{
    public struct CurrentBannerState
    {
        public bool isActive;
        public BannerPosition CurrentBannerPos;

        public CurrentBannerState(bool _isActive, BannerPosition _currentBannerPos)
        {
            isActive = _isActive;
            CurrentBannerPos = _currentBannerPos;
        }
    }

    [System.Serializable]
    public abstract class AdPlugin : SerializedScriptableObject
    {
        [SerializeField] protected AdManager AdManager;

        [Tooltip("Is Debug log enabled for this AD plugin. Note Global Debug must be enabled in AD manager for it to work")]
        [SerializeField] public bool isDebug;

        [Tooltip("Is The Test Mode Enabled")]
        [SerializeField] protected bool testMode;

        [Header("AD ID's")]
        [SerializeField] protected string rewardedID;

        [SerializeField] protected string bannerID;
        [SerializeField] protected string interstitialID;

        public bool isInitialized { get; protected set; }

        public bool isInitializationOperationDone { get; protected set; }


        public CurrentBannerState CurrentSmallBannerState { get; protected set; }

        public bool isTestMode => testMode;

        protected virtual void OnEnable()
        {
            isInitialized = false;
            isInitializationOperationDone = false;

            if (!AdManager)
            {
                throw new System.Exception("Please assign AdManager reference to the AdPlugin " + this.GetType().ToString());
            }
        }

        public abstract void ShowInterstitialAdIfAvailable(Action<Status, ADMeta> completionCalLBack, object additionalInfo = null);

        public abstract void ShowRewardedAdIfAvailable(Action<Status, ADMeta> completionCalLBack);

        public abstract void ShowSmallBanner(BannerPosition bannerPosition);

        public abstract void ShowLargeBanner(BannerPosition bannerPosition);

        public abstract void HideSmallBanner();

        public abstract void HideLargeBanner();

        public abstract bool CheckIfInterstitialAdIsAvailable(object additionalInfo = null);

        public abstract bool CheckIfRewardedAdIsAvailable();

        public abstract void DestroyAllAds();

        public abstract void InitializeAds();

        public abstract void LoadInterstitialAd(string adID = null);

        public abstract void LoadRewardedAd();

        public abstract T GetRelativeBannerPositionForCurrentPlugin<T>(BannerPosition bannerPosition);

        public abstract void PreLoadAds();

    }
}