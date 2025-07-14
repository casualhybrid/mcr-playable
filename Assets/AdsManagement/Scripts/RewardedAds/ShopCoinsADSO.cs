using System.Collections.Generic;
using TheKnights.AdsSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopCoinsADSO", menuName = "ScriptableObjects/Ads/RewardedAds/ShopCoinsAwarded")]
public class ShopCoinsADSO : RewardedAdSO
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


        inventorySystem.UpdateKeyValues(new List<InventoryItem<int>> { new InventoryItem<int>("AccountCoins", 50, true) }, true, true, "SmallCoinsRewarded_Shop");

        OnUserEarnedReward.Invoke();
    }
}