using System.Collections.Generic;
using TheKnights.AdsSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "BoostRewardedADSO", menuName = "ScriptableObjects/Ads/RewardedAds/BoostRewarded")]
public class BoostRewardedADSO : RewardedAdSO
{
    [SerializeField] private InventorySystem inventorySystem;

  

    public override void CompletionCallBack(Status status, ADMeta adMeta)
    {
        base.CompletionCallBack(status, adMeta);

        UnityEngine.Console.Log("Rewarded Completion CallBack Receieved");

        if (status != Status.Succeded)
        {
            UnityEngine.Console.Log($"Failed rewarding user with a boost");
            return;
        }

        // Give reward here

        UnityEngine.Console.Log($"Successfully rewarded with boost");

        inventorySystem.UpdateKeyValues(new List<InventoryItem<int>> { new InventoryItem<int>("GameBoost", 1,true)}, true, true, "BoostRewarded_PauseInventory");

        OnUserEarnedReward.Invoke();
    }
}   