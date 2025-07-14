using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using TheKnights.LeaderBoardSystem;

[System.Serializable]
public class LeaderBoardRankGeneralInfo
{
    [SerializeField] private Sprite rankSprite;

    public Sprite GetRankSprite => rankSprite;
}

[CreateAssetMenu(fileName = "LeaderBoardRankDataSet", menuName = "ScriptableObjects/StaticData/LeaderBoard/LeaderBoardRankDataSet")]
public class LeaderBoardRankDataSet : SerializedScriptableObject
{
    [SerializeField] private Dictionary<LeaderBoardRank, LeaderBoardRankGeneralInfo> leaderBoardRankSetDictionary;

    public Sprite GetLeaderBoardRankSprite(LeaderBoardRank rank)
    {
        LeaderBoardRankGeneralInfo generalInfo;
        bool success = leaderBoardRankSetDictionary.TryGetValue(rank, out generalInfo);

        if(!success)
        {
            throw new System.Exception($"Leaderboard rank information not found for rank : {rank}");
        }

        return generalInfo.GetRankSprite;
    }
}
