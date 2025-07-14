using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TheKnights.PlayServicesSystem.LeaderBoards;
using TheKnights.SaveFileSystem;
using UnityEngine;


public class RankedLeaderBoardManager : MonoBehaviour
{
    [SerializeField] private SaveManager saveManager;
    public TheKnights.PlayServicesSystem.LeaderBoards.LeaderBoardManager LeaderBoardManager;

    public void RequestDataFetch(LeaderBoardType leaderBoardType)
    {
        bool cachedDataExists = CheckIfLeaderBoardDataCacheExists(leaderBoardType);

        if (cachedDataExists)
        {
            LoadTheSavedLeaderBoardDataFromDisk(leaderBoardType);
        }
        else
        {
            AsyncOperationLeaderBoard handle = LeaderBoardManager.LoadLeaderBoardData(leaderBoardType);
            handle.OnShowLeaderBoardData += Handle_ShowLeaderBoardData;
        }
    }

    private void Handle_ShowLeaderBoardData(LeaderBoardType leaderBoardType, List<CustomLeaderBoardData> data, TheKnights.PlayServicesSystem.LeaderBoards.Status status)
    {
        if (status == TheKnights.PlayServicesSystem.LeaderBoards.Status.Failed)
        {
            UnityEngine.Console.LogWarning($"Failed loading leaderboard data {leaderBoardType}");
            return;
        }

        bool cachedDataExists = CheckIfLeaderBoardDataCacheExists(leaderBoardType);

        if (!cachedDataExists)
        {
            UnityEngine.Console.Log($"Cached data exists for {leaderBoardType}");

            // Attempt to save leaderBoard data to disk
            SaveLeaderBoardDataToDisk(data, leaderBoardType);
        }
    }

    // Saves the leaderboard data set to disk
    private void SaveLeaderBoardDataToDisk(List<CustomLeaderBoardData> data, LeaderBoardType leaderBoardType)
    {
        if (data == null || data.Count == 0)
        {
            UnityEngine.Console.LogWarning("Failed to save leaderboard data set to disk. Reason: Data is Empty");
            return;
        }

        List<LeaderBoardSerializeableData> leaderBoardSerializeableData = new List<LeaderBoardSerializeableData>();

        for (int i = 0; i < data.Count; i++)
        {
            CustomLeaderBoardData _data = data[i];
            //leaderBoardSerializeableData.Add(new LeaderBoardSerializeableData(_data.UserName, _data.UserScore, _data.User.id, _data.Rank, _data.isLocal));
        }

        SaveTheGame.SaveTheFile<LeaderBoardDataSet>(new LeaderBoardDataSet(leaderBoardSerializeableData), leaderBoardType.ToString() + LeaderBoardDataSet.LeaderBoardData);
    }

    // Loads the leaderboard data from disk
    private async void LoadTheSavedLeaderBoardDataFromDisk(LeaderBoardType leaderBoardType)
    {
        Task<LeaderBoardDataSet> task = LoadTheGame.LoadFileFromROM<LeaderBoardDataSet>(leaderBoardType.ToString() + LeaderBoardDataSet.LeaderBoardData);
        await task;

        if (task.Status == TaskStatus.Faulted || task.Status == TaskStatus.Canceled)
        {
            UnityEngine.Console.LogWarning($"Failed Fetching saved leaderboard data from disk {task.Exception} ");
            return;
        }

        LeaderBoardDataSet leaderBoardDataSet = task.Result;
    }

    // Returns if the file exists on disk
    private bool CheckIfLeaderBoardDataCacheExists(LeaderBoardType leaderBoardType)
    {
        return File.Exists(FrostyMaxSaveManager.FrostReadWrite.persistentpath + "/" + leaderBoardType.ToString() + LeaderBoardDataSet.LeaderBoardData);
    }
}