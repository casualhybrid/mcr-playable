using System;
using System.Collections.Generic;


namespace TheKnights.PlayServicesSystem.LeaderBoards
{
    public enum Status
    {
        Failed, Succeeded
    }

    public class AsyncOperationLeaderBoard
    {
        /// <summary>
        /// Determines if the requested operation completed independent of its status
        /// </summary>
        public bool isDone;

        /// <summary>
        /// The exception encountered during the requested operation
        /// </summary>
        public Exception OperationException;

        public event Action<LeaderBoardType, Status> OnUserYourImages;

        /// <summary>
        /// Subscribe to this event if a leaderboard data request was made
        /// </summary>
        public event Action<LeaderBoardType, List<CustomLeaderBoardData>, Status> OnShowLeaderBoardData;

        /// <summary>
        /// Subscribe to this event if a leaderboard score update request was made
        /// </summary>
        public event Action<LeaderBoardType, Status> LeaderBoardScoreSubmitted;

        public void SendShowLeaderBoardDataEvent(LeaderBoardType leaderBoardType, List<CustomLeaderBoardData> customLeaderBoardDatas, Status status)
        {
            OnShowLeaderBoardData?.Invoke(leaderBoardType, customLeaderBoardDatas, status);
        }

        public void SendImagesDownloadedEvent(LeaderBoardType leaderBoardType, Status status)
        {
            OnUserYourImages?.Invoke(leaderBoardType, status);
        }

        public void SendLeaderBoardSubmittedEvent(LeaderBoardType leaderBoardType, Status status)
        {
            LeaderBoardScoreSubmitted?.Invoke(leaderBoardType, status);
        }
    }
}