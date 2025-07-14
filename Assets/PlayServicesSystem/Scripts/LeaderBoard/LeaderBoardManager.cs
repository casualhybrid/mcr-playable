using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;

namespace TheKnights.PlayServicesSystem.LeaderBoards
{
    [CreateAssetMenu(fileName = "LeaderBoardManager", menuName = "ScriptableObjects/PlayGames/LeaderBoardManager", order = 1)]
    public class LeaderBoardManager : ScriptableObject
    {
        [System.Serializable]
        private class LeaderBoardInstanceDIctionary : SerializableDictionaryBase<LeaderBoardType, LeaderBoardBase>
        {
        }

        [SerializeField] private LeaderBoardInstanceDIctionary LeaderBoards;

        /// <summary>
        /// Requests and loads the leaderboard data
        /// </summary>
        public AsyncOperationLeaderBoard LoadLeaderBoardData(LeaderBoardType leaderBoardType)
        {
            AsyncOperationLeaderBoard handle = new AsyncOperationLeaderBoard();

            LeaderBoardBase TheLeaderBoard = GetTheLeaderBoardInstance(leaderBoardType);

            TheLeaderBoard.ShowLeaderBoardCustom(handle);

            return handle;
        }

        /// Show the leaderboard with the default play services UI
        /// </summary>
        public void ShowLeaderboardsPlayServicesUI(LeaderBoardType leaderBoardType)
        {
            LeaderBoardBase TheLeaderBoard = GetTheLeaderBoardInstance(leaderBoardType);
            TheLeaderBoard.ShowLeaderboardsPlayServicesUI();
        }

        /// <summary>
        /// Update the particular leaderboard's score
        /// </summary>
        public AsyncOperationLeaderBoard UpdateLeaderBoard(float score, LeaderBoardType leaderBoardType)
        {
            AsyncOperationLeaderBoard handle = new AsyncOperationLeaderBoard();

            LeaderBoardBase TheLeaderBoard = GetTheLeaderBoardInstance(leaderBoardType);

            TheLeaderBoard.UpdateLeaderBoard(handle, score);

            return handle;
        }

        /// <summary>
        /// Returns the player's saved rank on the leaderboard
        /// </summary>
        public int GetPlayerSavedRank(LeaderBoardType leaderBoardType)
        {
            LeaderBoardBase TheLeaderBoard = GetTheLeaderBoardInstance(leaderBoardType);

            return TheLeaderBoard.PlayerSavedRank;
        }

        /// <summary>
        /// Player's current score in the leaderboard
        /// </summary>
        public long GetPlayerSavedScore(LeaderBoardType leaderBoardType)
        {
            LeaderBoardBase TheLeaderBoard = GetTheLeaderBoardInstance(leaderBoardType);

            return TheLeaderBoard.PlayerSavedScore;
        }

        /// <summary>
        /// Returns the current state of leaderboard e.g loaded, stale, isLoading
        /// </summary>
        public LeaderBoardState GetLeaderBoardState(LeaderBoardType leaderBoardType)
        {
            LeaderBoardBase TheLeaderBoard = GetTheLeaderBoardInstance(leaderBoardType);
            return TheLeaderBoard.LeaderBoardState;
        }

        /// <summary>
        /// Resets the leaderboard clearing any kind of cached data and parameters
        /// </summary>
        /// <returns>Whether the reset operation was successfull or not</returns>
        public bool ResetTheCachedLeaderBoard(LeaderBoardType leaderBoardType)
        {
            LeaderBoardBase TheLeaderBoard = GetTheLeaderBoardInstance(leaderBoardType);
            return TheLeaderBoard.ResetLeaderBoardCache();
        }

        /// <summary>
        /// Returns the info regarding the player ahead of the user for a particular leaderboard
        /// </summary>
        public CustomLeaderBoardData GetNextPlayerInfomration(LeaderBoardType leaderBoardType)
        {
            LeaderBoardBase TheLeaderBoard = GetTheLeaderBoardInstance(leaderBoardType);
            return TheLeaderBoard.PlayerAheadOfUser;
        }

        /// <summary>
        /// Returns the current local player index in the cached leaderboard data
        /// </summary>
        public int GetPlayerIndexInLeaderBoard(LeaderBoardType leaderBoardType)
        {
            LeaderBoardBase TheLeaderBoard = GetTheLeaderBoardInstance(leaderBoardType);
            return TheLeaderBoard.localuserindex;
        }

        /// <summary>
        /// Returns if local player is present in the currently loaded data
        /// </summary>
        public bool GetCurrentLoadedStateOfLocalPlayer(LeaderBoardType leaderBoardType)
        {
            LeaderBoardBase TheLeaderBoard = GetTheLeaderBoardInstance(leaderBoardType);
            return TheLeaderBoard.LocalLeaderBoardObjectPresent;
        }

        private LeaderBoardBase GetTheLeaderBoardInstance(LeaderBoardType leaderBoardType)
        {
            LeaderBoardBase TheLeaderBoard;
            LeaderBoards.TryGetValue(leaderBoardType, out TheLeaderBoard);

            if (TheLeaderBoard == null)
            {
                throw new System.Exception("Failed to find the leaderboard instance for type " + leaderBoardType);
            }

            return TheLeaderBoard;
        }
    }
}