using System.Collections.Generic;
using TheKnights.AdsSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "GameOverDoubleRewardedADSO", menuName = "ScriptableObjects/Ads/RewardedAds/GameOverDoubleRewardedADSO")]
public class GameOverDoubleRewardedADSO : RewardedAdSO
{
    [SerializeField] private GamePlaySessionInventory gamePlaySessionInventory;

    public override void CompletionCallBack(Status status, ADMeta adMeta)
    {
        base.CompletionCallBack(status, adMeta);

        if (status == Status.Succeded)
        {
            gamePlaySessionInventory.DoubleTheGamePlayReward();
            AnalyticsManager.CustomData("Rewarded_2X", new Dictionary<string, object> { { "DoubleRewardType", "GameOver" } });
           // Firebase.Analytics.FirebaseAnalytics.LogEvent("Rewarded_2X", "DoubleRewardType", "GameOver");
            OnUserEarnedReward.Invoke();
        }
    }
}