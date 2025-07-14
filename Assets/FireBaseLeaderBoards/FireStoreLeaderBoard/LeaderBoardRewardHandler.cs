using Sirenix.OdinInspector;
using System.Collections.Generic;
using TheKnights.LeaderBoardSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "LeaderBoardRewardHandler", menuName = "ScriptableObjects/FireStoreLeaderBoard/LeaderBoardRewardHandler")]
public class LeaderBoardRewardHandler : ScriptableObject
{
    [SerializeField] private TheKnights.LeaderBoardSystem.LeaderBoardManager leaderBoardManager;
    [SerializeField] private LeaderBoardRankRewards leaderBoardRankRewards;
    [SerializeField] private GameEvent onUserClaimedLeaderBoardReward;
    [SerializeField] private InventorySystem playerInventory;

    [Button("TEMP")]
    public void GiveTempRewards()
    {
        LeaderBoardRank rank = LeaderBoardRank.Champion;

        // Give rewards according to rank
        var rewards = leaderBoardRankRewards.GetRewardsForSpecificRank(rank);

        List<InventoryItem<int>> inventoryItems = new List<InventoryItem<int>>();
        for (int i = 0; i < rewards.Length; i++)
        {
            ItemWithAmount inventoryItemSO = rewards[i];
            inventoryItems.Add(new InventoryItem<int>(inventoryItemSO.GetItemType.GetKey, inventoryItemSO.GetAmount, true));
        }

        playerInventory.UpdateKeyValues(inventoryItems, true, true);
    }

    public void ClaimLeaderBoardReward()
    {

        LeaderBoardRank rank = leaderBoardManager.CurrentRewardedGroupBeingClaimed.localPlayerData.LeaderBoardRank;

        // Give rewards according to rank
        var rewards = leaderBoardRankRewards.GetRewardsForSpecificRank(rank);

        List<InventoryItem<int>> inventoryItems = new List<InventoryItem<int>>();

        for (int i = 0; i < rewards.Length; i++)
        {
            ItemWithAmount inventoryItemSO = rewards[i];
            inventoryItems.Add(new InventoryItem<int>(inventoryItemSO.GetItemType.GetKey, inventoryItemSO.GetAmount, true));
        }

        playerInventory.UpdateKeyValues(inventoryItems, true, true, "LeaderBoardWeekCompleteReward");

        // Claim the reward
        leaderBoardManager.ClaimThePendingReward();

        onUserClaimedLeaderBoardReward.RaiseEvent();
    }
}