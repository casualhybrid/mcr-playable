using UnityEngine;
#if FIREBASE_CRASHLYTICS
using Firebase.Crashlytics;
#endif


public class MATS_FirebaseCrashlyticsModule : MonoBehaviour, IFirebaseModule
{
 
    public void Initialize()
    {
#if FIREBASE_CRASHLYTICS
        Crashlytics.IsCrashlyticsCollectionEnabled = true;
        Debug.Log("[Crashlytics] Initialized");
#else
        Debug.LogWarning("[Crashlytics] Not Present");
#endif
    }

    public void LogError(string message)
    {
#if FIREBASE_CRASHLYTICS
        Crashlytics.Log(message);
#endif
    }

    public void LogException(System.Exception e)
    {
#if FIREBASE_CRASHLYTICS
        Crashlytics.LogException(e);
#endif
    }
}