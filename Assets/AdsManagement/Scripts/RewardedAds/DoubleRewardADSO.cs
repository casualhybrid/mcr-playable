using System.Collections.Generic;
using TheKnights.AdsSystem;
using UnityEngine;


[CreateAssetMenu(fileName = "DoubleRewardADSO", menuName = "ScriptableObjects/Ads/RewardedAds/DoubleRewardRewarded")]
public class DoubleRewardADSO : RewardedAdSO
{
    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private GameEvent doubleRewardAdComplete;


    public override void CompletionCallBack(Status status, ADMeta adMeta)
    {
        base.CompletionCallBack(status, adMeta);

        if (status != Status.Succeded)
        {
            UnityEngine.Console.Log($"Failed rewarding user with a double reward");
            return;
        }

        // Give reward here

        doubleRewardAdComplete.RaiseEvent();

    }


}