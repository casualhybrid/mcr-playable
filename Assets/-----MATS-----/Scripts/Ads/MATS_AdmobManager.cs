using UnityEngine;
using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Ump.Api;

public class MATS_AdmobManager : MonoBehaviour
{
    public static MATS_AdmobManager Instance { get; private set; }
    [Header("Toast Manager")]
    public MATS_ToastManager toastManager;

    [Header("Enable Test Mode")]
    public bool testMode = true;

    [Header("Banner Ad Positions")]
    public AdPosition bannerPosition = AdPosition.TopRight;
    public AdPosition rectangleBannerPosition = AdPosition.Center;

    private BannerView bannerAd, rectangleBannerAd;
    private InterstitialAd interstitialAd;
    private RewardedAd rewardedAd;
    private RewardedInterstitialAd rewardedInterstitialAd;
    private AppOpenAd appOpenAd;
    private ConsentInformation consentInfo;
    private bool adsInitialized = false;

    public Action OnRewardedAdCompleted;
    public Action OnRewardedInterstitialAdCompleted;
    private bool isShowingAd = false;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Serializable]
    public class AdIds
    {
        public string bannerAdId, rectangleBannerAdId;
        public string interstitialAdId, rewardedAdId, rewardedInterstitialAdId, appOpenAdId;

        private readonly string testBannerAdId = "ca-app-pub-3940256099942544/6300978111";
        
        private readonly string testRectangleBannerAdId = "ca-app-pub-3940256099942544/6300978111";
        private readonly string testInterstitialAdId = "ca-app-pub-3940256099942544/1033173712";
        private readonly string testRewardedAdId = "ca-app-pub-3940256099942544/5224354917";
        private readonly string testRewardedInterstitialAdId = "ca-app-pub-3940256099942544/5354046379";
        private readonly string testAppOpenAdId = "ca-app-pub-3940256099942544/3419835294";

        public string GetBannerAdId(bool test) => test ? testBannerAdId : bannerAdId;
       
        public string GetRectangleBannerAdId(bool test) => test ? testRectangleBannerAdId : rectangleBannerAdId;
        public string GetInterstitialAdId(bool test) => test ? testInterstitialAdId : interstitialAdId;
        public string GetRewardedAdId(bool test) => test ? testRewardedAdId : rewardedAdId;
        public string GetRewardedInterstitialAdId(bool test) => test ? testRewardedInterstitialAdId : rewardedInterstitialAdId;
        public string GetAppOpenAdId(bool test) => test ? testAppOpenAdId : appOpenAdId;
    }

    public AdIds adIds;

    void Start()
    {
        Invoke(nameof(StartAdsLoading), 1f);
    }

    void StartAdsLoading()
    {
        RequestGDPRConsent();
        Application.focusChanged += OnAppStateChanged;
        // LogAdData();
    }

    #region GDPR & Consent Management
    private void RequestGDPRConsent()
    {
        ConsentRequestParameters request = new ConsentRequestParameters
        {
            TagForUnderAgeOfConsent = false
        };

        ConsentInformation.Update(request, (FormError error) =>
        {
            if (error != null)
            {
                Debug.LogError("Consent Update Error: " + error.Message);
                return;
            }

            if (ConsentInformation.CanRequestAds())
            {
                InitializeAds();
            }
            else
            {
                ShowGDPRConsentForm();
            }
        });
    }

    private void ShowGDPRConsentForm()
    {
        ConsentForm.LoadAndShowConsentFormIfRequired((FormError error) =>
        {
            if (error != null)
            {
                Debug.LogError("GDPR Consent Form Error: " + error.Message);
            }

            if (ConsentInformation.CanRequestAds())
            {
                InitializeAds();
            }
            else
            {
                Debug.LogWarning("User did not provide consent. Ads will not be shown.");
            }
        });
    }
    #endregion

    private void InitializeAds()
    {
        if (adsInitialized) return;
        adsInitialized = true;

        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("Google Mobile Ads SDK Initialized");
            LoadAllAds();
        });
    }

    private void LoadAllAds()
    {
        LoadBannerAd();
        LoadRectangleBannerAd();
        LoadInterstitialAd();
        LoadRewardedAd();
        LoadRewardedInterstitialAd();
        LoadAppOpenAd();
    }

    #region Banner Ads
    private void LoadBannerAd()
    {
        if (bannerAd != null)
        {
            DestroyAd(bannerAd);
        }
        bannerAd = new BannerView(adIds.GetBannerAdId(testMode), AdSize.Banner, bannerPosition);
        bannerAd.LoadAd(new());
        ListenToAdEvents(bannerAd);
        bannerAd.Hide();
    }

    

    private void LoadRectangleBannerAd()
    {
        if (rectangleBannerAd != null)
        {
            DestroyAd(rectangleBannerAd);
        }
        rectangleBannerAd = new BannerView(adIds.GetRectangleBannerAdId(testMode), AdSize.MediumRectangle, rectangleBannerPosition);
        rectangleBannerAd.LoadAd(new());
        ListenToAdEvents(rectangleBannerAd);
        rectangleBannerAd.Hide();
    }


    public void DestroyAd(BannerView _bannerView)
    {
        if (_bannerView != null)
        {
            Debug.Log("Destroying banner view.");
            _bannerView.Destroy();
            _bannerView = null;
        }
    }

    public void ShowBannerAd() => bannerAd?.Show();
  
    public void ShowRectangleBannerAd() => rectangleBannerAd?.Show();
    public void HideBannerAd() => bannerAd?.Hide();
    
    public void HideRectangleBannerAd() => rectangleBannerAd?.Hide();

  
    public void HideAllBanner()
    {
        HideBannerAd();
        HideRectangleBannerAd();
    }
    /// <summary>
    /// listen to events the banner view may raise.
    /// </summary>
    private void ListenToAdEvents(BannerView _bannerView)
    {
        // Raised when an ad is loaded into the banner view.
        _bannerView.OnBannerAdLoaded += () =>
        {
            Debug.Log("Banner view loaded an ad with response : "
                + _bannerView.GetResponseInfo());
        };
        // Raised when an ad fails to load into the banner view.
        _bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError("Banner view failed to load an ad with error : "
                + error);
        };
        // Raised when the ad is estimated to have earned money.
        _bannerView.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Banner view paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        _bannerView.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Banner view recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        _bannerView.OnAdClicked += () =>
        {
            Debug.Log("Banner view was clicked.");
        };
        // Raised when an ad opened full screen content.
        _bannerView.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Banner view full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        _bannerView.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Banner view full screen content closed.");
        };
    }

    #endregion

    #region Interstitial Ad
    private void LoadInterstitialAd()
    {
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }
        InterstitialAd.Load(adIds.GetInterstitialAdId(testMode), new(), (ad, error) =>
        {
            if (error == null)
            {

                interstitialAd = ad;
                RegisterEventHandlers(interstitialAd);
            }
        });
    }



    public void ShowInterstitialAd()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            toastManager.SendToast("Ad Not Avalible", false);
        }
        else if (interstitialAd == null && interstitialAd.CanShowAd() == false)
        {
            toastManager.SendToast("Ad Not Avalible", false);
        }
        else
        {
            interstitialAd.Show();
        }

    }

    private void RegisterEventHandlers(InterstitialAd interstitialAd)
    {
        // Raised when the ad is estimated to have earned money.
        interstitialAd.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Interstitial ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        interstitialAd.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Interstitial ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        interstitialAd.OnAdClicked += () =>
        {
            Debug.Log("Interstitial ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        interstitialAd.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Interstitial ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        interstitialAd.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial ad full screen content closed.");
            LoadInterstitialAd();
        };
        // Raised when the ad failed to open full screen content.
        interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content " +
                           "with error : " + error);
            LoadInterstitialAd();
        };
    }
    #endregion

    #region Rewarded Ad
    private void LoadRewardedAd()
    {

        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }
        RewardedAd.Load(adIds.GetRewardedAdId(testMode), new(), (ad, error) =>
        {
            if (error == null)
            {
                rewardedAd = ad;
                RegisterEventHandlers(rewardedAd);
            }
        });
    }

    public void ShowRewardedAd()
    {

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            toastManager.SendToast("Ad Not Avalible", false);
        }
        else if (rewardedAd == null && rewardedAd.CanShowAd() == false)
        {
            toastManager.SendToast("Ad Not Avalible", false);
        }
        else
        {
            rewardedAd.Show((reward) => OnRewardedAdCompleted?.Invoke());
        }
    }

    private void RegisterEventHandlers(RewardedAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad full screen content closed.");
            LoadRewardedAd();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content " +
                           "with error : " + error);
            LoadRewardedAd();
        };
    }
    #endregion

    #region Rewarded Interstitial Ad
    private void LoadRewardedInterstitialAd()
    {
        if (rewardedInterstitialAd != null)
        {
            rewardedInterstitialAd.Destroy();
            rewardedInterstitialAd = null;
        }
        RewardedInterstitialAd.Load(adIds.GetRewardedInterstitialAdId(testMode), new(), (ad, error) =>
        {
            if (error == null)
            {
                rewardedInterstitialAd = ad;
                RegisterEventHandlers(rewardedInterstitialAd);
            }
        });
    }

    public void ShowRewardedInterstitialAd()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            toastManager.SendToast("Ad Not Avalible", false);
        }
        else if (rewardedInterstitialAd == null && rewardedInterstitialAd.CanShowAd() == false)
        {
            toastManager.SendToast("Ad Not Avalible", false);
        }
        else
        {
            rewardedInterstitialAd.Show((reward) => OnRewardedInterstitialAdCompleted?.Invoke());
        }
    }



    private void RegisterEventHandlers(RewardedInterstitialAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Rewarded interstitial ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded interstitial ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded interstitial ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded interstitial ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded interstitial ad full screen content closed.");
            LoadRewardedInterstitialAd();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded interstitial ad failed to open " +
                           "full screen content with error : " + error);
            LoadRewardedInterstitialAd();
        };
    }
    #endregion

    #region App Open Ad

    private void OnAppStateChanged(bool hasFocus)
    {
        if (hasFocus && !isShowingAd)
        {
            ShowAdIfAvailable();
        }
    }
    public void ShowAdIfAvailable()
    {
        if (IsAdAvailable() && !isShowingAd)
        {
            appOpenAd.Show();
            isShowingAd = true;

        }
        else
        {
            LoadAppOpenAd();
        }
    }
    private bool IsAdAvailable()
    {
        return appOpenAd != null;
    }
    private void LoadAppOpenAd()
    {
        AppOpenAd.Load(adIds.GetAppOpenAdId(testMode), ScreenOrientation.LandscapeLeft, new(), (ad, error) =>
        {
            if (error == null)
            {
                appOpenAd = ad;
                RegisterEventHandlers(ad);
            }
        });
    }

    public void ShowAppOpenAd()
    {
        if (appOpenAd != null && appOpenAd.CanShowAd())
        {
            appOpenAd.Show();
        }
    }

    private void RegisterEventHandlers(AppOpenAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("App open ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("App open ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("App open ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("App open ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("App open ad full screen content closed.");
            isShowingAd = false;
            LoadAppOpenAd();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {

            Debug.LogError("App open ad failed to open full screen content " +
                           "with error : " + error);
            LoadAppOpenAd();
        };
    }
    #endregion


}
