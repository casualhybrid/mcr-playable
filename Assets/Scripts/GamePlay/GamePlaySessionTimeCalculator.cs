using System.Collections.Generic;
using UnityEngine;

public class GamePlaySessionTimeCalculator : MonoBehaviour
{
    [SerializeField] private GamePlaySessionData gamePlaySessionData;
    [SerializeField] private List<float> gamePlayTimeSecForAnalytics;
    private bool is1MinTimeLapsReaced = false;
    private bool is3MinTimeLapsReaced = false;
    private bool is5MinTimeLapsReaced = false;

    private float lifeTimeGamePlayTimeCovered;
    private float targetGamePlaySecTimeForAnalytics;
    private int curGamePlayTimeAnalyticsIndex;
    private bool targetGamePlaySpecificAnalyticsCompleted;

    private void Start()
    {
        curGamePlayTimeAnalyticsIndex = gamePlayTimeSecForAnalytics.Count;
        lifeTimeGamePlayTimeCovered = PlayerPrefs.GetFloat("GamePlayTimeByUser", 0);

        for (int i = 0; i < gamePlayTimeSecForAnalytics.Count; i++)
        {
            var time = gamePlayTimeSecForAnalytics[i];

            if (time > lifeTimeGamePlayTimeCovered)
            {
                curGamePlayTimeAnalyticsIndex = i;
                break;
            }
        }

        AssignNextTargetSpecificGamePlayTimeForAnalytics();
    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat("GamePlayTimeByUser", (lifeTimeGamePlayTimeCovered + gamePlaySessionData.timeElapsedSinceSessionStarted));
    }

    private void AssignNextTargetSpecificGamePlayTimeForAnalytics()
    {
        if (curGamePlayTimeAnalyticsIndex >= gamePlayTimeSecForAnalytics.Count)
        {
            targetGamePlaySpecificAnalyticsCompleted = true;
            return;
        }

        targetGamePlaySecTimeForAnalytics = gamePlayTimeSecForAnalytics[curGamePlayTimeAnalyticsIndex];
        curGamePlayTimeAnalyticsIndex++;
    }

    private void Update()
    {
        if (!GameManager.IsGameStarted)
            return;

        if (SpeedHandler.GameTimeScale == 0)
            return;

        gamePlaySessionData.timeElapsedSinceSessionStarted += Time.deltaTime;

        if (!targetGamePlaySpecificAnalyticsCompleted && (gamePlaySessionData.timeElapsedSinceSessionStarted + lifeTimeGamePlayTimeCovered) >= targetGamePlaySecTimeForAnalytics)
        {
            AnalyticsManager.CustomData("SpecificGamePlayTimeCovered", new Dictionary<string, object> { { "TimeInSeconds", targetGamePlaySecTimeForAnalytics } });

            AssignNextTargetSpecificGamePlayTimeForAnalytics();
        }

        if (gamePlaySessionData.timeElapsedSinceSessionStarted >= 300 && is5MinTimeLapsReaced == false)
        {
            is5MinTimeLapsReaced = true;
            AnalyticsManager.CustomData("GamePlayScreen_RaceTime", new Dictionary<string, object>
            {
                { "RaceTime", 5 }
            });
        }
        else if (gamePlaySessionData.timeElapsedSinceSessionStarted >= 180 && is3MinTimeLapsReaced == false)
        {
            is3MinTimeLapsReaced = true;
            AnalyticsManager.CustomData("GamePlayScreen_RaceTime", new Dictionary<string, object>
            {
                { "RaceTime", 3 }
            });
        }
        else if (gamePlaySessionData.timeElapsedSinceSessionStarted >= 60 && is1MinTimeLapsReaced == false)
        {
            is1MinTimeLapsReaced = true;
            AnalyticsManager.CustomData("GamePlayScreen_RaceTime", new Dictionary<string, object>
            {
                { "RaceTime", 1}
            });
        }
    }
}