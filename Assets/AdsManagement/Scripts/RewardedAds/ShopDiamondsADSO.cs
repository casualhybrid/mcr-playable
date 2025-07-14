using System.Collections.Generic;
using TheKnights.AdsSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopDiamondsADSO", menuName = "ScriptableObjects/Ads/RewardedAds/ShopDiamondsAwarded")]
public class ShopDiamondsADSO : RewardedAdSO
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


     //   UnityEngine.Console.Log($"Successfully rewarded with mysteryBox");

        // Make inventory system accept single key


        inventorySystem.UpdateKeyValues(new List<InventoryItem<int>> { new InventoryItem<int>("AccountDiamonds", 5, true) }, true, true, "SmallDiamondsRewarded_Shop");

        OnUserEarnedReward.Invoke();
    }
}