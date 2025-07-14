using UnityEngine.SocialPlatforms;

namespace TheKnights.PlayServicesSystem.LeaderBoards
{
    /// <summary>
    /// Define a user's data in the leaderboard
    /// </summary>
    public struct CustomLeaderBoardData
    {
        /// <summary>
        /// The player's rank in the leaderboard
        /// </summary>
        public readonly int Rank;

        public readonly string UserName;
        public readonly long UserScore;

        /// <summary>
        /// The play services social profile of the user
        /// </summary>
        public readonly IUserProfile User;

        /// <summary>
        /// Does this data belongs to the local player
        /// </summary>
        public readonly bool isLocal;

        public CustomLeaderBoardData(string username, long score, IUserProfile userprofile, int rank, bool islocal)
        {
            UserName = username;
            UserScore = score;
            Rank = rank;
            User = userprofile;
            isLocal = islocal;
        }
    }
}