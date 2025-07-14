using MessagePack;
using System.Collections.Generic;
using UnityEngine;

[MessagePackObject]
public class LeaderBoardSerializeableData
{
    [Key(1)] public string UserName;
    [Key(2)] public long Score;

    /// <summary>
    /// Does this data belongs to the local player
    /// </summary>
    [Key(3)] public bool isLocal;

    [Key(4)] public string userID;

    public LeaderBoardSerializeableData(string username, long score, string id, bool islocal)
    {
        UserName = username;
        Score = score;
        userID = id;
        isLocal = islocal;
    }

    public LeaderBoardSerializeableData()
    {
    }
}

[MessagePackObject]
public class LeaderBoardSubmissionData
{
    [Key(1)] public string UserName;
    [Key(2)] public long Score;

    public LeaderBoardSubmissionData(string username, long score)
    {
        UserName = username;
        Score = score;
    }

    public LeaderBoardSubmissionData()
    {
    }
}

[MessagePackObject]
public class LeaderBoardDataSet
{
    [IgnoreMember] public const string LeaderBoardData = "LeaderBoardData.save";

    [Key(0)] public List<LeaderBoardSerializeableData> LeaderBoardSerializedData;

    public LeaderBoardDataSet(List<LeaderBoardSerializeableData> leaderBoardSerializedData)
    {
        LeaderBoardSerializedData = leaderBoardSerializedData;
    }
}

public static class LeaderBoardDataSetExtensions
{
    /// <summary>
    /// Adjusts player position in the already ascending sorted data based on the current high score
    /// </summary>

    public static void SortPlayerPositionInTheData(this LeaderBoardDataSet data, LeaderBoardSerializeableData localPlayerData, bool isDescending = false)
    {
        int playerScore = (int)localPlayerData.Score;

        #region Obselete

        //int playerSortedIndex = 0;
        //int playercurrentIndex;

        //for (int i = 0; i < data.LeaderBoardSerializedData.Count; i++)
        //{
        //    LeaderBoardSerializeableData entry = data.LeaderBoardSerializedData[i];
        //    bool isLocalEntry = entry.isLocal;

        //    if (isLocalEntry)
        //    {
        //        playercurrentIndex = i;
        //        continue;
        //    }

        //    if (playerScore >= entry.Score)
        //    {
        //        playerSortedIndex = i;
        //    }
        //}

        #endregion Obselete

        int indexWherePlayerAdded = -1;
        bool localPlayerSorted = false;
        List<LeaderBoardSerializeableData> LeaderBoardSerializedData = new List<LeaderBoardSerializeableData>();
        LeaderBoardSerializeableData localEntry = default;

        for (int i = 0; i < data.LeaderBoardSerializedData.Count; i++)
        {
            LeaderBoardSerializeableData entry = data.LeaderBoardSerializedData[i];

            // If entry is local cache it for later use
            if (entry.isLocal)
            {
                UnityEngine.Console.Log("Marking " + entry.UserName + " as local");

                localEntry = entry;
                continue;
            }

            bool condition = !isDescending ? playerScore <= entry.Score : playerScore >= entry.Score;
            if (condition && !localPlayerSorted)
            {
                UnityEngine.Console.Log("Player Score Greater Than This One");

                indexWherePlayerAdded = LeaderBoardSerializedData.Count;

                // We will update the information after the loop based on the local entry cached above

                UnityEngine.Console.Log("Adding player");
                LeaderBoardSerializedData.Add(localPlayerData);

                localPlayerSorted = true;
            }

            UnityEngine.Console.Log("Adding Other");
            LeaderBoardSerializedData.Add(entry);

            // If it's the last index and local user hans't been added yet
            if (i == data.LeaderBoardSerializedData.Count - 1 && !localPlayerSorted)
            {
                indexWherePlayerAdded = LeaderBoardSerializedData.Count;
                // We will update the information after the loop based on the local entry cached above
                LeaderBoardSerializedData.Add(localPlayerData);
                localPlayerSorted = true;
            }
        }

        data.LeaderBoardSerializedData = LeaderBoardSerializedData;

        if (localEntry == null)
        {
            UnityEngine.Console.LogWarning("The Local Entry was null while sorting and adjusting player position. This shouldn't happen");
            return;
        }

        // If the player still isn't sorted then add it. Can be because there's only player entry is the data?
        if (!localPlayerSorted)
        {
            LeaderBoardSerializedData.Add(localPlayerData);
        }
        else
        {
            LeaderBoardSerializeableData finalPlayerEntry = LeaderBoardSerializedData[indexWherePlayerAdded];
            finalPlayerEntry.UserName = localEntry.UserName;
            finalPlayerEntry.Score = playerScore;
            finalPlayerEntry.userID = localEntry.userID;
        }


        // Temp
        //UnityEngine.Console.Log("After Sorting");
        //foreach (var item in data.LeaderBoardSerializedData)
        //{
        //    UnityEngine.Console.Log("Entry " + item.UserName);
        //}
    }
}