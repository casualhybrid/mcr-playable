using System.Collections.Generic;
using TheKnights.AdsSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "MysteryBoxDoubleRewardedADSO", menuName = "ScriptableObjects/Ads/RewardedAds/MysteryBoxDoubleRewardedAD")]
public class MysteryBoxDoubleRewardedADSO : RewardedAdSO
{
    [SerializeField] private InventorySystem inventorySystem;

  
    public override void CompletionCallBack(Status status, ADMeta adMeta)
    {
        base.CompletionCallBack(status, adMeta);

        if (status != Status.Succeded)
        {
            UnityEngine.Console.Log($"Failed rewarding user with a double mysteryBox");
            return;
        }

        // Give reward here

        var lastRewardMystery = inventorySystem.LastMysteryBoxReward;

        UnityEngine.Console.Log($"Doubling {lastRewardMystery.Key} mystery box reward");
        inventorySystem.UpdateKeyValues(new List<InventoryItem<int>> { new InventoryItem<int>(lastRewardMystery.Key, lastRewardMystery.Value) });


        OnUserEarnedReward.Invoke();
    }
}