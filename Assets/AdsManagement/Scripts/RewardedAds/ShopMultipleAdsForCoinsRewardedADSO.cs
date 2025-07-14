using System.Collections;
using System.Collections.Generic;
using TheKnights.AdsSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopMultipleAdsForCoinsRewardedADSO", menuName = "ScriptableObjects/Ads/RewardedAds/ShopMultipleAdsForCoinsRewardedADSO")]
public class ShopMultipleAdsForCoinsRewardedADSO : RewardedAdSO
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

        if(status == Status.Succeded)
        {
            timesWatched++;
            
            if(timesWatched == 3)
            {
                playerInventorySystem.UpdateKeyValues(new List<InventoryItem<int>> { new InventoryItem<int>("AccountCoins", 250, true) }, true, true, "MultiADCoinsRewarded_Shop");
                timesWatched = 0;
            }

            OnUserEarnedReward.Invoke();
        }
    }

}
