using System.Collections;
using System.Collections.Generic;
using TheKnights.AdsSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopMultipleAdsForDiamondsRewardedADSO", menuName = "ScriptableObjects/Ads/RewardedAds/ShopMultipleAdsForDiamondsRewardedADSO")]
public class ShopMultipleAdsForDiamondsRewardedADSO : RewardedAdSO
{
    [SerializeField] private InventorySystem playerInventorySystem;
    private int timesWatched;

    private void OnEnable()
    {
        timesWatched = 0;
    }

    public override void CompletionCallBack(Status status, ADMeta adMeta)
    {
        base.CompletionCallBack(status, adMeta);

        if (status == Status.Succeded)
        {
            timesWatched++;

            if (timesWatched == 3)
            {
                playerInventorySystem.UpdateKeyValues(new List<InventoryItem<int>> { new InventoryItem<int>("AccountDiamonds", 30, true) }, true , true, "MultiADRewardedRewarded_Shop");
                timesWatched = 0;
            }

            OnUserEarnedReward.Invoke();
        }
    }

}
