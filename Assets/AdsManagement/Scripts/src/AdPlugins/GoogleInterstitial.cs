using System;
using TheKnights.AdsSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "GoogleInterstitial", menuName = "ScriptableObjects/GoogleInterstitial", order = 3)]
public class GoogleInterstitial : ScriptableObject
{
    [SerializeField] private GoogleAdsPlugin googleAdsPlugin;
    [SerializeField] private bool loadInterstitialOnAdClose = true;
    [SerializeField] private bool useAsStaticRewarded = false;
    [SerializeField] private bool preLoadAD = false;

    [SerializeField] private string interstitialID;

    private string _interstitialID;

    private bool isLoadingInterstitial;
    //private InterstitialAd interstitial;

    public bool isLoaded => false;// interstitial != null && interstitial.IsLoaded();
    public bool isPreLoadAD => preLoadAD;

    private void OnEnable()
    {
        isLoadingInterstitial = false; 
    }

    public void Initialize()
    {
        if(googleAdsPlugin.isTestMode)
        {
            _interstitialID = "ca-app-pub-3940256099942544/1033173712";
        }
        else
        {
            _interstitialID = interstitialID;
        }
    }

    // Pending Interstitial Listeners
    private Action<Status,ADMeta> pendingInterstitialListener;

    public void LoadInterstitial()
    {
        //if (isLoadingInterstitial)
        //{
        //    if (googleAdsPlugin.isDebug)
        //    {
        //        UnityEngine.Console.Log("Interstitial Ad is already loading");
        //    }

        //    return;
        //}

        //if (interstitial != null && interstitial.IsLoaded())
        //{
        //    return;
        //}

        //DestroyInterstiaial();

        //if (!googleAdsPlugin.isInitialized)
        //{
        //    return;
        //}

        //// Initialize an InterstitialAd.
        //this.interstitial = new InterstitialAd(_interstitialID);
        ////SubscribetoEvents
        //SubscribeInterstiaialCallbacks();
        //// Create an empty ad request.

        //AdRequest request = new AdRequest.Builder().Build();
        //isLoadingInterstitial = true;

        //// Load the interstitial with the request.
        //this.interstitial.LoadAd(request);
    }

    public void ShowInterstitialAdIfAvailable(Action<Status, ADMeta> completionCalLBack = null)
    {
        //if(useAsStaticRewarded)
        //{
        //    googleAdsPlugin.RewardedADRightBeforeShow();

        //    CoroutineRunner.Instance.WaitForUpdateAndExecute(() => {     // we are assuming we check for null where we called it :3 sad
        //        this.interstitial.Show();
        //    });

        

        //    return;

        //}

        //pendingInterstitialListener = completionCalLBack;

        ////#if UNITY_ANDROID && !UNITY_EDITOR

        //if (!googleAdsPlugin.isInitialized)
        //{
        //    googleAdsPlugin.InterstitialADHasFailed();
        //    ProcessPendingCallBack(AdType.Interstital, Status.Failed);
        //    return;
        //}

        //if (this.interstitial != null && this.interstitial.IsLoaded())
        //{
        //    googleAdsPlugin.InterstitialADRightBeforeShow();

        //    CoroutineRunner.Instance.WaitForUpdateAndExecute(() => { this.interstitial.Show(); });
    
        //}
        //else
        //{
        //    googleAdsPlugin.InterstitialADHasFailed();
        //    ProcessPendingCallBack(AdType.Interstital, Status.Failed);
        //    LoadInterstitial();
        //}
    }

    private void SubscribeInterstiaialCallbacks()
    {
        //// Called when an ad request has successfully loaded.
        //this.interstitial.OnAdLoaded += (object sender, EventArgs e) =>
        //{
        //    UnityMainThreadDispatcher.Instance().Enqueue(() =>
        //    {
        //        if (googleAdsPlugin.isDebug)
        //            UnityEngine.Console.Log("Interstitial Loaded Admob");

        //        isLoadingInterstitial = false;

        //        string adapterName = null;
        //        if (this.interstitial != null)
        //        {
        //            var responseInfo = this.interstitial.GetResponseInfo();
        //            if (responseInfo != null)
        //            {
        //                adapterName = responseInfo.GetMediationAdapterClassName();
        //            }
        //        }

        //        if (useAsStaticRewarded)
        //        {
        //            googleAdsPlugin.SendRewardedADHasLoaded(new ADMeta(adapterName));
        //        }
        //        else
        //        {
        //            googleAdsPlugin.SendInterstitialADHasLoaded(new ADMeta(adapterName));
        //        }
            
        //        //   OnAdLoadedEvent.Invoke();
        //    });
        //};
        //// Called when an ad request failed to load.
        //this.interstitial.OnAdFailedToLoad += (object sender, AdFailedToLoadEventArgs e) =>
        //{
        //    if (googleAdsPlugin.isDebug)
        //        UnityEngine.Console.Log("Interstitial Failed To Load Admob");

        //    UnityMainThreadDispatcher.Instance().Enqueue(() =>
        //    {
        //        isLoadingInterstitial = false; /*OnAdFailedToLoadEvent.Invoke();*/

        //        if (useAsStaticRewarded)
        //        {
        //            googleAdsPlugin.SendRewardedADFailedToLoad();
        //        }
        //    });
        //};

        //this.interstitial.OnAdFailedToShow += (object sender, AdErrorEventArgs args) => {

        //    UnityMainThreadDispatcher.Instance().Enqueue(() =>
        //    {
        //        if (googleAdsPlugin.isDebug)
        //        {
        //            UnityEngine.Console.Log("Interstitial Failed To Show");
        //        }

        //        // Since AD has failed to show it might be faulted?
        //        LoadInterstitial();

        //        if (useAsStaticRewarded)
        //        {
        //            googleAdsPlugin.SendRewardedADFailedToShow();
        //        }
        //        else
        //        {
        //            //if (loadInterstitialOnAdClose)
        //            //{
        //            //    LoadInterstitial();
        //            //}

        //            googleAdsPlugin.InterstitialADHasFailed();
        //            ProcessPendingCallBack(AdType.Interstital, Status.Failed);
        //        }
        //    });
        //};

        //// Called when an ad is shown.
        //this.interstitial.OnAdOpening += (object sender, EventArgs e) =>
        //{
        //    UnityMainThreadDispatcher.Instance().Enqueue(() =>
        //    {
        //        if (useAsStaticRewarded)
        //        {
        //            googleAdsPlugin.SendOnRewardedADOpened();
        //        }
        //        else
        //        {
        //            googleAdsPlugin.InterstitialADAboutToShow();
        //        }

        //        /*OnAdOpeningEvent.Invoke();*/
        //    });
        //};
        //// Called when the ad is closed.
        //this.interstitial.OnAdClosed += (object sender, EventArgs e) =>
        //{
        //    UnityMainThreadDispatcher.Instance().Enqueue(() =>
        //    {
        //        if (googleAdsPlugin.isDebug)
        //        {
        //            UnityEngine.Console.Log("Interstitial Closed");
        //        }

        //        string adapterName = null;
        //        if (this.interstitial != null)
        //        {
        //            var responseInfo = this.interstitial.GetResponseInfo();
        //            if (responseInfo != null)
        //            {
        //                adapterName = responseInfo.GetMediationAdapterClassName();
        //            }
        //        }


        //        if (useAsStaticRewarded)
        //        {
        //            googleAdsPlugin.SendOnUserEarnedReward(new ADMeta(adapterName));
        //            googleAdsPlugin.SendRewardedADClosed();
        //        }
        //        else
        //        {
        //            if(loadInterstitialOnAdClose)
        //            {
        //                LoadInterstitial();
        //            }
                  
        //            googleAdsPlugin.InterstitialADHasSucceeded(new ADMeta(adapterName));
        //            ProcessPendingCallBack(AdType.Interstital, Status.Succeded);
        //        }
        //    });
        //};

        //// Called when the ad click caused the user to leave the application.
        ////this.interstitial.OnAdLeavingApplication += (object sender, EventArgs e) =>
        ////{
        ////    UnityMainThreadDispatcher.Instance().Enqueue(() =>
        ////    { Logger.Debug("Interstitial Clicked Leave App"); OnAdLeavingApplicationEvent.Invoke(); });
        ////};
    }

    public void DestroyInterstiaial()
    {
        //UnityEngine.Console.Log("Destroy Interstisial");

        //if (this.interstitial != null)
        //{
        //    this.interstitial.Destroy();
        //    this.interstitial = null;
        //    isLoadingInterstitial = false;
        //}
    }

    public bool CheckIfInterstitialAdIsAvailable()
    {

        //if (interstitial != null && interstitial.IsLoaded())
        //    return true;
        //else
        //{
        //    LoadInterstitial();
        //    return false;
        //}
        return false;
    }

    protected void ProcessPendingCallBack(AdType adType, Status status, ADMeta adMeta = default)
    {
        Action<Status,ADMeta> pendingCallBack = adType == AdType.Interstital ? pendingInterstitialListener : null;

        if (pendingCallBack == null)
        {
            UnityEngine.Console.LogWarning($"A call to process AD pending callback was made but there's not callback!. AdType {adType} and status {status}");
            return;
        }

        pendingCallBack(status,adMeta);
        pendingCallBack = null;
    }
}