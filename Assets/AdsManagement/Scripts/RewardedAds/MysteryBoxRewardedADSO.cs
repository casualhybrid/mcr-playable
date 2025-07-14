using System.Collections.Generic;
using TheKnights.AdsSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "MysteryBoxRewardedADSO", menuName = "ScriptableObjects/Ads/RewardedAds/MysteryBoxRewarded")]
public class MysteryBoxRewardedADSO : RewardedAdSO
{
    [SerializeField] private InventorySystem inventorySystem;



    public override void CompletionCallBack(Status status, ADMeta adMeta)
    {
        base.CompletionCallBack(status, adMeta);

        if (status != Status.Succeded)
        {
            UnityEngine.Console.Log($"Failed rewarding user with a mysteryBox");
            return;
        }


        // Give reward here


        UnityEngine.Console.Log($"Successfully rewarded with mysteryBox");

        // Make inventory system accept single key

     
        inventorySystem.UpdateKeyValues(new List<InventoryItem<int>> { new InventoryItem<int>("GameMysteryBox", 1,true) }, true, true, "MysteryBoxRewarded_Shop");

        OnUserEarnedReward.Invoke();
    }
}