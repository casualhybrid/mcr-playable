using System.Collections.Generic;
using TheKnights.AdsSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "HeadStartRewardedAD", menuName = "ScriptableObjects/Ads/RewardedAds/HeadStart")]
public class HeadStartRewardedADSO : RewardedAdSO
{
    [SerializeField] private InventorySystem inventorySystem;

  
    public override void CompletionCallBack(Status status, ADMeta adMeta)
    {
        base.CompletionCallBack(status, adMeta);

        if (status != Status.Succeded)
        {
            UnityEngine.Console.Log($"Failed rewarding user with a headstart");
            return;
        }

        // Give reward here
    
        UnityEngine.Console.Log($"Successfully rewarded with headstart");

  
        inventorySystem.UpdateKeyValues(new List<InventoryItem<int>> { new InventoryItem<int>("GameHeadStart", 1) });

        OnUserEarnedReward.Invoke();
    }
}