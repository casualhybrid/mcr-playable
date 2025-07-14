using TheKnights.AdsSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "ReviveRewardedADSO", menuName = "ScriptableObjects/Ads/RewardedAds/ReviveRewarded")]
public class ReviveRewardedADSO : RewardedAdSO
{
    [SerializeField] private PlayerSharedData playerSharedData;
    [SerializeField] private GamePlaySessionData gamePlaySessionData;
    [SerializeField] private GameEvent OnPlayerRevive;
    [SerializeField] private GameEvent OnHeadStart;
    [SerializeField] private GameEvent armour;
    [SerializeField] private PickupsUtilityHelper pickupsUtilityHelper;
    [SerializeField] private SpecialPickupsEnumSO specialPickupsEnumSO;

    public override void CompletionCallBack(Status status, ADMeta adMeta)
    {
        base.CompletionCallBack(status, adMeta);

        if (status != Status.Succeded)
        {
            UnityEngine.Console.Log($"Failed rewarding user with a revive");
            return;
        }

        // Give reward here

        UnityEngine.Console.Log($"Successfully rewarded with revive");

        //UnityEngine.Console.LogError(timesFailedDuringDuration);
        if (ShouldReviveComplementaryRewardBeGiven())
        {
            OnPlayerRevive.TheEvent.AddListener(HandlePlayerRevive);
        }

        OnUserEarnedReward.Invoke();
    }

    public bool ShouldReviveComplementaryRewardBeGiven()
    {
        //int timesFailedDuringDuration = gamePlaySessionData.timesFailedDuringDuration(10);

        //return timesFailedDuringDuration >= 3;
        return true;
    }

    private void HandlePlayerRevive(GameEvent gameEvent)
    {
        OnPlayerRevive.TheEvent.RemoveListener(HandlePlayerRevive);

        if (playerSharedData.WallRunBuilding)
            return;


        if (pickupsUtilityHelper.isSafeToSpawn(playerSharedData.PlayerTransform.position.z, specialPickupsEnumSO.AeroPlanePickup as InventoryItemSO))
        {
            OnHeadStart.RaiseEvent();
        }


        //  armour.RaiseEvent();
    }
}