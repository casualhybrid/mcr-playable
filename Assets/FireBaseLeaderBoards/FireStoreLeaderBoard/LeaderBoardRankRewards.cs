using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace TheKnights.LeaderBoardSystem
{
    [CreateAssetMenu(fileName = "LeaderBoardRankRewards", menuName = "ScriptableObjects/FireStoreLeaderBoard/LeaderBoardRankRewards")]
    public class LeaderBoardRankRewards : SerializedScriptableObject
    {
        [OdinSerialize] private Dictionary<LeaderBoardRank, ItemWithAmount[]> rewardsDictionary;

        public ItemWithAmount[] GetRewardsForSpecificRank(LeaderBoardRank rank)
        {
            ItemWithAmount[] inventoryItems;
            rewardsDictionary.TryGetValue(rank, out inventoryItems);

            if(inventoryItems == null)
            {
                UnityEngine.Console.LogWarning($"No leaderboard awards found for rank {rank}");
            }

            return inventoryItems;
        }
    }
}
