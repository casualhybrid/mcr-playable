using System.Collections.Generic;
using Unity.Services.Analytics;
using UnityEngine;

public class UserSpecificAnalyticEvents : MonoBehaviour
{
    [SerializeField] private AdsController adsController;
    [SerializeField] private List<int> timesToSendInterstitialADCompletedEvent;
    [SerializeField] private List<int> timesToSendRewardedADCompletedEvent;
    [SerializeField] private List<int> timesToSendSessionADCompletedEvent;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        adsController.OnInterstitialAdCompleted.AddListener(CheckAndSendMultipleTimesInterstitialADCompletedEvent);
        adsController.OnRewardedAdCompleted.AddListener(CheckAndSendMultipleTimesRewardedADCompletedEvent);
    }

    private void Start()
    {
        CheckAndSendMultipleTimesSessionStartedEvent();
    }

    private void CheckAndSendMultipleTimesSessionStartedEvent()
    {
        string sessionsStartedTillNow = "sessionsStartedTillNow";

        int sessionsTillNow = PlayerPrefs.GetInt(sessionsStartedTillNow, 0);

        sessionsTillNow++;

        PlayerPrefs.SetInt(sessionsStartedTillNow, sessionsTillNow);

        if (!timesToSendSessionADCompletedEvent.Contains(sessionsTillNow))
            return;

        AnalyticsManager.CustomData("SessionsStartedLifeTime", new Dictionary<string, object> { { "SessionsStarted", sessionsTillNow } });
    }

    private void CheckAndSendMultipleTimesInterstitialADCompletedEvent()
    {
        string interstitialADSWatchedTillNow = "InterstitialADsWatched";

        int adsWatchedCount = PlayerPrefs.GetInt(interstitialADSWatchedTillNow, 0);

        adsWatchedCount++;

        PlayerPrefs.SetInt(interstitialADSWatchedTillNow, adsWatchedCount);

        if (!timesToSendInterstitialADCompletedEvent.Contains(adsWatchedCount))
            return;

        AnalyticsManager.CustomData("InterstitialsWatchedLifeTime", new Dictionary<string, object> { { "TimesADWatched", adsWatchedCount } });
    }

    private void CheckAndSendMultipleTimesRewardedADCompletedEvent()
    {
        string rewardedADSWatchedTillNow = "RewardedADsWatched";

        int adsWatchedCount = PlayerPrefs.GetInt(rewardedADSWatchedTillNow, 0);

        adsWatchedCount++;

        PlayerPrefs.SetInt(rewardedADSWatchedTillNow, adsWatchedCount);

        if (!timesToSendRewardedADCompletedEvent.Contains(adsWatchedCount))
            return;

        AnalyticsManager.CustomData("RewardedWatchedLifeTime", new Dictionary<string, object> { { "TimesADWatched", adsWatchedCount } });
    }
}