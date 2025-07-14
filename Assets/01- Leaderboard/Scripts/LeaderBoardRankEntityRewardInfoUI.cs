using TheKnights.LeaderBoardSystem;
using TMPro;
using UnityEngine;

public class LeaderBoardRankEntityRewardInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreToBeatText;
    [SerializeField] private TextMeshProUGUI rankText;

    [SerializeField] private LeaderBoardRankEntityAwardUI awardPrefab;
    [SerializeField] private Transform awardsContent;

    [SerializeField] private LeaderBoardRankRewards leaderBoardRankRewards;
    [SerializeField] private InventoryItemsMetaData inventoryItemsMetaData;

    private bool isRewardInformationSet;

    public void SetInformation(string scoreToBeat, LeaderBoardRank rank)
    {
        scoreToBeatText.text = scoreToBeat;
        rankText.text = rank.ToString();
    }

    public void SetRewardsInformation(LeaderBoardRank rank)
    {
        if (isRewardInformationSet)
            return;

        var rewards = leaderBoardRankRewards.GetRewardsForSpecificRank(rank);

        for (int i = 0; i < rewards.Length; i++)
        {
            ItemWithAmount reward = rewards[i];
            InventoryItemMeta meta = inventoryItemsMetaData.GetInventoryItemMeta(reward.GetItemType);

            LeaderBoardRankEntityAwardUI awardEntity = Instantiate(awardPrefab, awardsContent);

            awardEntity.SetInformation(meta.Sprite, reward.GetAmount.ToString());
        }

        isRewardInformationSet = true;
    }
}