using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

#if FIREBASE_REMOTE_CONFIG
using Firebase.RemoteConfig;
#endif

public class MATS_FirebaseRemoteConfigModule : MonoBehaviour, IFirebaseModule
{
    public MATS_RemoteConfigStrings remoteConfigStrings;

    public async void Initialize()
    {
#if FIREBASE_REMOTE_CONFIG
        Debug.Log("[RemoteConfig] Initializing...");

        var defaults = new Dictionary<string, object>
        {
            { "test", "default_value" }
        };

        await FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults);
        Debug.Log("[RemoteConfig] Defaults set");

        await FetchDataAsync();
#endif
    }

    public async Task FetchDataAsync()
    {
#if FIREBASE_REMOTE_CONFIG
        Debug.Log("[RemoteConfig] Fetching...");

        try
        {
            await FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);
            bool activated = await FirebaseRemoteConfig.DefaultInstance.FetchAndActivateAsync();

            if (activated)
            {
                Debug.Log("[RemoteConfig] Data fetched and activated.");
                DisplayData();
            }
            else
            {
                Debug.LogWarning("[RemoteConfig] Activation returned false (no new data).");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[RemoteConfig] Fetch error: " + ex.Message);
        }
#endif
    }


    public void DisplayData()
    {
#if FIREBASE_REMOTE_CONFIG && !UNITY_EDITOR
        if (remoteConfigStrings == null)
        {
            Debug.LogError("[RemoteConfig] remoteConfigStrings is null.");
            return;
        }

        string key = nameof(remoteConfigStrings.test);

        try
        {
            if (FirebaseRemoteConfig.DefaultInstance.GetKeysByPrefix("").Contains(key))
            {
                string stringValue = FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;
                remoteConfigStrings.test = stringValue;

                Debug.Log($"[RemoteConfig] Key '{key}' found: {remoteConfigStrings.test}");
            }
            else
            {
                Debug.LogWarning($"[RemoteConfig] Key '{key}' not found.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[RemoteConfig] Error reading key: " + ex.Message);
        }
#endif
    }
}
