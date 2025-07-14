using System.Collections;
using System.Collections.Generic;
using TheKnights.AdsSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "MenuCoinsADSO", menuName = "ScriptableObjects/Ads/RewardedAds/MenuCoinsADSO")]
public class MenuCoinsADSO : RewardedAdSO
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


        inventorySystem.UpdateKeyValues(new List<InventoryItem<int>> { new InventoryItem<int>("AccountCoins", 50, true) }, true, true, "CoinsRewarded_Menu");

        OnUserEarnedReward.Invoke();
    }
}
