using RotaryHeart.Lib.SerializableDictionary;
using System.Linq;
using TheKnights.PlayServicesSystem.LeaderBoards;
using TheKnights.SaveFileSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "LeaderBoardProgressionData", menuName = "ScriptableObjects/PlayGames/LeaderBoards/LeaderBoardProgressionData", order = 1)]
public class LeaderBoardProgressionData : ScriptableObject
{
    [System.Serializable]
    private class RankUpRequirementDictionary : SerializableDictionaryBase<LeaderBoardType, int> { }

    [SerializeField] private RankUpRequirementDictionary rankUpReqDictionary;
    [SerializeField] private SaveManager saveManager;

    public int GetScoreRequiredForCategory(LeaderBoardType leaderBoardType)
    {
        return rankUpReqDictionary[leaderBoardType];
    }

    public LeaderBoardType GetNextPossibleRank(float score)
    {
        var sorted = rankUpReqDictionary.OrderByDescending(x => x.Value);

        foreach (var item in sorted)
        {
            if (score >= item.Value)
            {
                return item.Key;
            }
        }


        return saveManager.MainSaveFile.currentLeaderBoardRank;


    }
}