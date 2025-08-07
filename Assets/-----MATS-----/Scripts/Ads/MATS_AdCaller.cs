using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MATS_AdCaller : MonoBehaviour
{
    [SerializeField] private MATS_MaxAdManager adManager;

    private void Reset()
    {
      //  adManager = FindObjectOfType<MATS_MaxAdManager>();
    }

    public void ShowInterstitial()
    {
        adManager?.ShowInterstitial();
    }

    public void ShowRewarded()
    {
        MATS_MaxAdManager.OnRewardedAdCompleted += Reward;
        adManager?.ShowRewardedAd();
    }

    void Reward()
    {
        Debug.Log("Rewarded Recieved");
        MATS_MaxAdManager.OnRewardedAdCompleted -= Reward;
    }

    public void ShowAppOpen()
    {
        adManager?.ShowAppOpenAd();
    }

    public void ToggleBanner()
    {
        adManager?.ToggleBannerVisibility();
    }

    public void ToggleMRec()
    {
        adManager?.ToggleMRecVisibility();
    }


    public void ShowMediations()
    {
        MaxSdk.ShowMediationDebugger();
    }
    
    
    public void RemoteConfigTestResult()
    {
        Debug.Log(MATS_FirebaseManager.Instance.remoteConfigModule.remoteConfigStrings.test);
    }
}