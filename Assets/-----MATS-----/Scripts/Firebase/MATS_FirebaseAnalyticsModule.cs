using UnityEngine;
#if FIREBASE_ANALYTICS
using Firebase.Analytics;
#endif

public class MATS_FirebaseAnalyticsModule : MonoBehaviour, IFirebaseModule
{
  

    public void Initialize()
    {

        Debug.Log("[Analytics] Firebase Analytics Initialized");
#if FIREBASE_ANALYTICS

        // Example: Log device info
        FirebaseAnalytics.LogEvent("startup_device_info",
            new Parameter("device_model", SystemInfo.deviceModel),
            new Parameter("os", SystemInfo.operatingSystem)
        );
#else
        Debug.LogWarning("[Analytics] Firebase Analytics not present");
#endif
    }

    public void LogEvent(string eventName)
    {
#if FIREBASE_ANALYTICS
        FirebaseAnalytics.LogEvent(eventName);
#endif
    }
}