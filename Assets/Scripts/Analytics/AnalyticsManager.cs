using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Analytics;

public static class AnalyticsManager 
{
    public static void CustomData(string eventName, IDictionary<string, object> eventParams)
    {
        // if (!UnityGameServicesManager.IsGamingServiceInitialized)
        //  return;
        if (!FireBaseInitializer.isInitialized)
            return;
        // AnalyticsService.Instance.CustomData(eventName, eventParams);
        if (string.IsNullOrEmpty(eventName))
            return;

        // Convert Dictionary to Parameter[]
        List<Parameter> firebaseParams = new List<Parameter>();

        foreach (var kvp in eventParams)
        {
            string key = kvp.Key;
            object value = kvp.Value;

            if (value is int)
                firebaseParams.Add(new Parameter(key, (int)value));
            else if (value is long)
                firebaseParams.Add(new Parameter(key, (long)value));
            else if (value is float)
                firebaseParams.Add(new Parameter(key, (float)value));
            else if (value is double)
                firebaseParams.Add(new Parameter(key, (double)value));
            else if (value is string)
                firebaseParams.Add(new Parameter(key, value.ToString()));
            else
                firebaseParams.Add(new Parameter(key, value.ToString()));
        }

        FirebaseAnalytics.LogEvent(eventName, firebaseParams.ToArray());
        //Debug.LogError($"[FirebaseAnalytics] Event Sent: {eventName} | Params: {string.Join(", ", eventParams)}");
    }

    public static void CustomData(string eventName)
    {
        // if (!UnityGameServicesManager.IsGamingServiceInitialized)
        //  return;
        if (!FireBaseInitializer.isInitialized)
            return;
        //   AnalyticsService.Instance.CustomData(eventName);
        FirebaseAnalytics.LogEvent(eventName);
        Debug.LogError(eventName);

    }
}
