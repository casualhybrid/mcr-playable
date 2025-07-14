using System.Collections;
using System.Collections.Generic;
using TheKnights.AdsSystem;
using UnityEngine;
[CreateAssetMenu(fileName = "RewardedPowerUpInGameADSO", menuName = "ScriptableObjects/Ads/RewardedAds/RewardedPowerUpInGameADSO")]

public class RewardedPowerUpInGameADSO : RewardedAdSO
{
    public void AddThisItemToMetaDisplayData(RewardedADRewardMetaData meta)
    {
        rewardedADRewardMetaDatas = new RewardedADRewardMetaData[1];
        rewardedADRewardMetaDatas[0] = meta;
    }

    public override void CompletionCallBack(Status status, ADMeta adMeta)
    {
        base.CompletionCallBack(status, adMeta);

        if(status == Status.Succeded)
        {
            OnUserEarnedReward.Invoke();
        }
    }

    private void OnValidate()
    {
        rewardedADRewardMetaDatas = null;
    }
}
