using System;
using UnityEngine;
using System.Collections;

public class RemoteConfigManager : MonoBehaviour
{
    public static RemoteConfigManager Instance { get; private set; }

    

    // Internal storage
    private long adsInterval;
    private bool hienQc;
    private bool showOpenAds;
    private bool showOpenAdsFirstOpen;
    private bool resumeAds; // NEW

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    
    private void ApplyDefaultValues()
    {
     
    }
}
