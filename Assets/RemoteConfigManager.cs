using System;
using UnityEngine;
using Firebase;
using Firebase.RemoteConfig;
using System.Collections;

public class RemoteConfigManager : MonoBehaviour
{
    public static RemoteConfigManager Instance { get; private set; }

    // === Remote Config Keys ===
    private const string KEY_ADS_INTERVAL = "ads_interval";
    private const string KEY_HIEN_QC = "hien_qc";
    private const string KEY_SHOW_OPEN_ADS = "show_open_ads";
    private const string KEY_SHOW_OPEN_ADS_FIRST_OPEN = "show_open_ads_first_open";
    private const string KEY_RESUME_ADS = "resume_ads"; // NEW

    // === Default Values ===
    private const long DEFAULT_ADS_INTERVAL = 25;
    private const bool DEFAULT_HIEN_QC = false;
    private const bool DEFAULT_SHOW_OPEN_ADS = false;
    private const bool DEFAULT_SHOW_OPEN_ADS_FIRST_OPEN = false;
    private const bool DEFAULT_RESUME_ADS = true; // NEW

    // === Public Properties for Easy Access ===
    public static long AdsInterval => Instance != null ? Instance.adsInterval : DEFAULT_ADS_INTERVAL;
    public static bool HienQc => Instance != null ? Instance.hienQc : DEFAULT_HIEN_QC;
    public static bool ShowOpenAds => Instance != null ? Instance.showOpenAds : DEFAULT_SHOW_OPEN_ADS;
    public static bool ShowOpenAdsFirstOpen => Instance != null ? Instance.showOpenAdsFirstOpen : DEFAULT_SHOW_OPEN_ADS_FIRST_OPEN;
    public static bool ResumeAds => Instance != null ? Instance.resumeAds : DEFAULT_RESUME_ADS; // NEW

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

    private IEnumerator Start()
    {
        Debug.Log("[RemoteConfig] Initializing Firebase...");

        var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => dependencyTask.IsCompleted);

        if (dependencyTask.Result == DependencyStatus.Available)
        {
            Debug.Log("[RemoteConfig] Firebase dependencies available.");

            // Set default values in Remote Config
            var defaults = new System.Collections.Generic.Dictionary<string, object>
            {
                { KEY_ADS_INTERVAL, DEFAULT_ADS_INTERVAL },
                { KEY_HIEN_QC, DEFAULT_HIEN_QC },
                { KEY_SHOW_OPEN_ADS, DEFAULT_SHOW_OPEN_ADS },
                { KEY_SHOW_OPEN_ADS_FIRST_OPEN, DEFAULT_SHOW_OPEN_ADS_FIRST_OPEN },
                { KEY_RESUME_ADS, DEFAULT_RESUME_ADS } // NEW
            };
            FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults);

            Debug.Log("[RemoteConfig] Default values set.");

            yield return FetchRemoteConfig();
        }
        else
        {
            Debug.LogError("[RemoteConfig] Firebase dependencies not available. Using defaults.");
            ApplyDefaultValues();
        }
    }

    private IEnumerator FetchRemoteConfig()
    {
        Debug.Log("[RemoteConfig] Fetching latest config from server...");

        var fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);
        yield return new WaitUntil(() => fetchTask.IsCompleted);

        if (fetchTask.IsCompleted && !fetchTask.IsFaulted && !fetchTask.IsCanceled)
        {
            FirebaseRemoteConfig.DefaultInstance.ActivateAsync();

            adsInterval = FirebaseRemoteConfig.DefaultInstance.GetValue(KEY_ADS_INTERVAL).LongValue;
            hienQc = FirebaseRemoteConfig.DefaultInstance.GetValue(KEY_HIEN_QC).BooleanValue;
            showOpenAds = FirebaseRemoteConfig.DefaultInstance.GetValue(KEY_SHOW_OPEN_ADS).BooleanValue;
            showOpenAdsFirstOpen = FirebaseRemoteConfig.DefaultInstance.GetValue(KEY_SHOW_OPEN_ADS_FIRST_OPEN).BooleanValue;
            resumeAds = FirebaseRemoteConfig.DefaultInstance.GetValue(KEY_RESUME_ADS).BooleanValue; // NEW

            Debug.Log("[RemoteConfig] Fetch successful! Values:");
            Debug.Log($"  ads_interval = {adsInterval}");
            Debug.Log($"  hien_qc = {hienQc}");
            Debug.Log($"  show_open_ads = {showOpenAds}");
            Debug.Log($"  show_open_ads_first_open = {showOpenAdsFirstOpen}");
            Debug.Log($"  resume_ads = {resumeAds}"); // NEW
        }
        else
        {
            Debug.LogWarning("[RemoteConfig] Fetch failed. Using default values.");
            ApplyDefaultValues();
        }
    }

    private void ApplyDefaultValues()
    {
        adsInterval = DEFAULT_ADS_INTERVAL;
        hienQc = DEFAULT_HIEN_QC;
        showOpenAds = DEFAULT_SHOW_OPEN_ADS;
        showOpenAdsFirstOpen = DEFAULT_SHOW_OPEN_ADS_FIRST_OPEN;
        resumeAds = DEFAULT_RESUME_ADS; // NEW

        Debug.Log("[RemoteConfig] Defaults applied:");
        Debug.Log($"  ads_interval = {adsInterval}");
        Debug.Log($"  hien_qc = {hienQc}");
        Debug.Log($"  show_open_ads = {showOpenAds}");
        Debug.Log($"  show_open_ads_first_open = {showOpenAdsFirstOpen}");
        Debug.Log($"  resume_ads = {resumeAds}"); // NEW
    }
}
