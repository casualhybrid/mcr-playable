using Firebase;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IFirebaseModule
{
    /// Called after Firebase is initialized and available.
    void Initialize();
}
public class MATS_FirebaseManager : MonoBehaviour
{
    public static MATS_FirebaseManager Instance { get; private set; }
    public static bool IsInitialized { get; private set; }

    public List<IFirebaseModule> registeredModules = new List<IFirebaseModule>();

#if FIREBASE_REMOTE_CONFIG
    
    public MATS_FirebaseRemoteConfigModule remoteConfigModule;

#endif

#if FIREBASE_CRASHLYTICS
    public MATS_FirebaseCrashlyticsModule crashlyticsModule;
#endif

#if FIREBASE_ANALYTICS
    public MATS_FirebaseAnalyticsModule analyticsModule;
#endif

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
            return;
        }
    }

    private void Start()
    {
        Invoke(nameof(InitializeFirebase), 1f);

    }

    private async void InitializeFirebase()
    {
        var status = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (status == DependencyStatus.Available)
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            IsInitialized = true;
            Debug.Log("[Firebase] Initialized");

            DiscoverAndInitializeModules();
        }
        else
        {
            Debug.LogError("[Firebase] Dependency Error: " + status);
        }



#if FIREBASE_REMOTE_CONFIG

        remoteConfigModule = GetComponent<MATS_FirebaseRemoteConfigModule>();

#endif

#if FIREBASE_CRASHLYTICS
     crashlyticsModule= GetComponent<MATS_FirebaseCrashlyticsModule>();
#endif

#if FIREBASE_ANALYTICS
        analyticsModule = GetComponent<MATS_FirebaseAnalyticsModule>();
#endif

    }

private void DiscoverAndInitializeModules()
    {
        registeredModules.Clear();

        foreach (var module in GetComponents<MonoBehaviour>())
        {
            if (module is IFirebaseModule firebaseModule)
            {
                registeredModules.Add(firebaseModule);
            }
        }

        Debug.Log($"[Firebase] {registeredModules.Count} modules discovered. Initializing...");

        foreach (var module in registeredModules)
        {
            try
            {
                module.Initialize();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Firebase] Module Init Failed: {module.GetType().Name} -> {ex.Message}");
            }
        }
    }
}
