using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace TheKnights.PlayServicesSystem.LeaderBoards
{
    /// <summary>
    /// Inherit from this class to implement a new leaderboard
    /// </summary>
    public abstract class LeaderBoardBase : ScriptableObject
    {
        [Tooltip("The leaderboard's type")]
        [SerializeField] protected LeaderBoardType leaderBoardType;

        [Tooltip("The ID of the leaderboard as specified in the auto generated GPGSIds class")]
        [SerializeField] protected string id;

        [Tooltip("Load the leaderboard data relative to  what")]
       // [SerializeField] protected LeaderboardStart leaderBoardStart;

       // [Tooltip("Scope of the leaderboard")]
        //[SerializeField] protected LeaderboardCollection leaderboardCollection;

       // [Tooltip("TimeSpan of the leaderboard")]
       // [SerializeField] protected LeaderboardTimeSpan leaderboardTimeSpan;

       // [Tooltip("Row count of the leaderboard data")]
        [SerializeField] protected int rowCount;

        //#region Events

        //public event Action<LeaderBoardType, List<CustomLeaderBoardData>, bool> ShowLeaderBoardData;

        ////Events. Sent when all the user images have been downloaded
        //public event Action<LeaderBoardType> UserYourImages;

        //// Event sent when the new leaderboard score has been updated
        //public event Action<LeaderBoardType> LeaderBoardScoreUpdated;

        //#endregion Events

        /// <summary>
        /// Returns the current state of leaderboard e.g loaded, stale, isLoading
        /// </summary>
        public LeaderBoardState LeaderBoardState { get; private set; }

        /// <summary>
        /// Returns the type of leaderboard
        /// </summary>
        public LeaderBoardType GetLeaderBoardType => leaderBoardType;

        /// <summary>
        /// Player's current rank in the leaderboard
        /// </summary>
        public int PlayerSavedRank { get; private set; }

        /// <summary>
        /// Player's current score in the leaderboard
        /// </summary>
        public long PlayerSavedScore { get; private set; }

        /// <summary>
        /// Is the player present in the leaderboard data fetched?
        /// </summary>
        public bool LocalLeaderBoardObjectPresent { get; private set; }

        /// <summary>
        /// Gets the player that is one step ahead of the player
        /// </summary>
        public CustomLeaderBoardData PlayerAheadOfUser { get; private set; }

        /// <summary>
        /// Index of the user in the data feched
        /// </summary>
        public int localuserindex { get; private set; }

        // Queue of all the currently requested load operations
        protected Queue<AsyncOperationLeaderBoard> operationsQueue = new Queue<AsyncOperationLeaderBoard>();

        // The entire leaderboard data with all of the players
        private List<CustomLeaderBoardData> LoadedLeaderBoardListRef;

        // Image download task token
        private CancellationTokenSource theCancellationTokenSource;

        // Token produced from the source used to cancel the image downloading task
        private CancellationToken theCancellationToken;

        // Cached boolean for whether to download images aswell
        private bool attemptLoadingImages;

        private void OnEnable()
        {
            LeaderBoardState = LeaderBoardState.Stale;
            ResetLeaderBoardCache();
        }

        /// <summary>
        /// Show the leaderboard with the default play services UI
        /// </summary>
        public void ShowLeaderboardsPlayServicesUI()
        {
           /* if (PlayServicesController.isLoggedIn)
            {
               // PlayGamesPlatform.Instance.ShowLeaderboardUI();
            }
            else
            {
                UnityEngine.Console.Log("Cannot show leaderboard: not authenticated");
            }*/

        }

        /// <summary>
        /// Updates the leaderboard and reload it if required
        /// </summary>
        public virtual void UpdateLeaderBoard(AsyncOperationLeaderBoard handle, float score)
        {
            Action ActionToPerform = () =>
            {
                if (score <= 0)
                {
                    UnityEngine.Console.Log("Failed trying to update less or equal to zero score to leaderboard");
                    SendUpatedLeaderBoardScoreSignal(handle, leaderBoardType, Status.Failed);
                    return;
                }

               /* if (!PlayServicesController.isLoggedIn)
                {
                    UnityEngine.Console.Log("Can't update leaderboard as player's not logged in");
                    SendUpatedLeaderBoardScoreSignal(handle, leaderBoardType, Status.Failed);

       
                    return;
                }*/

               /* PlayGamesPlatform.Instance.ReportScore((long)score,
                   id,
                    (success) =>
                    {
                        if (!success)
                        {
                            UnityEngine.Console.Log("Failed Updating User Score To LeaderBoard");
                            SendUpatedLeaderBoardScoreSignal(handle, leaderBoardType, Status.Failed);
                            return;
                        }

                        UnityEngine.Console.Log("Successfully Updated User Score To LeaderBoard");

                        //try
                        //{
                        //    LeaderBoardScoreUpdated(GetLeaderBoardType);
                        //}
                        //catch
                        //{
                        //    UnityEngine.Console.Log("No subscriber for leaderboard score updated");
                        //}

                        SendUpatedLeaderBoardScoreSignal(handle, leaderBoardType, Status.Succeeded);
                    });*/
            };

            CoroutineRunner.Instance.StartCoroutine(WaitForAFrameAndExecute(ActionToPerform));
        }

        private void SendUpatedLeaderBoardScoreSignal(AsyncOperationLeaderBoard handle, LeaderBoardType leaderBoardType, Status status)
        {
            handle.isDone = true;
            handle.SendLeaderBoardSubmittedEvent(leaderBoardType, status);
        }

        /// <summary>
        /// Fetch the leaderboard data from play services
        /// </summary>
        /// <param name="forceReload">Whether the leaderboard should discard any previously loaded data and attempt to load again. It doesn't work if the leaderboard is in loading state</param>
        /// <param name="attemptLoadingImages">Should the images be downloaded aswell</param>
        public void ShowLeaderBoardCustom(AsyncOperationLeaderBoard handle, bool forceReload = false, bool attemptLoadingImages = true)
        {
            Action ActionToPerform = () =>
            {
                // Add this handler in the listeners waiting queue
                operationsQueue.Enqueue(handle);

                if (forceReload)
                {
                    // It will still fail if the leaderboads loading already
                    ResetLeaderBoardCache();
                }

                if (LeaderBoardState == LeaderBoardState.Loading)
                {
                    UnityEngine.Console.Log("Adding leaderboard load request to queue and ignoring processing. Reason: LeaderBoard State: " + LeaderBoardState);
                    return;
                }

                // Check for cached data First
                if (CheckAndSendCachedLeaderBoardDataIfAvailable())
                {
                    return;
                }

                try
                {
                    LeaderBoardState = LeaderBoardState.Loading;
                    this.attemptLoadingImages = attemptLoadingImages;

                    // Initialize local player object as not found
                    LocalLeaderBoardObjectPresent = false;

                    //PlayGamesPlatform.Instance.LoadScores(id, leaderBoardStart, rowCount, leaderboardCollection,
                   // leaderboardTimeSpan, LeaderBoardDataAcquiredCallback);
                }
                catch
                {
                    HandleFailedToGetLeaderboardData();
                }
            };
            CoroutineRunner.Instance.StartCoroutine(WaitForAFrameAndExecute(ActionToPerform));
        }

        protected IEnumerator WaitForAFrameAndExecute(Action ActionToPerform)
        {
            yield return null;

            ActionToPerform();
        }

        private void UpdateAndDispatchLoadFaliureSignal()
        {
            foreach (AsyncOperationLeaderBoard handle in operationsQueue)
            {
                handle.SendShowLeaderBoardDataEvent(leaderBoardType, null, Status.Failed);
            }
        }

        private void UpdateAndDispatchLoadSuccessSignal(List<CustomLeaderBoardData> data)
        {
            foreach (AsyncOperationLeaderBoard handle in operationsQueue)
            {
                handle.SendShowLeaderBoardDataEvent(leaderBoardType, data, Status.Succeeded);
            }
        }

        private void UpdateAndDispatchImagesDownloadedSignal(Status status)
        {
            foreach (AsyncOperationLeaderBoard handle in operationsQueue)
            {
                handle.SendImagesDownloadedEvent(leaderBoardType, status);
            }
        }

       /* private void LeaderBoardDataAcquiredCallback(LeaderboardScoreData data)
        {
            try
            {
                if (data == null || data.Scores == null || data.Scores.Length < 1)
                {
                    UnityEngine.Console.LogWarning("Not enough players to be displayed on the leaderboard");
                    HandleFailedToGetLeaderboardData();
                    return;
                }

                UnityEngine.Console.Log("Player Score: " + data.PlayerScore.value);
                UnityEngine.Console.Log("Player Rank: " + data.PlayerScore.rank);
                UnityEngine.Console.Log("Leaderboard data valid: " + data.Valid);
                UnityEngine.Console.Log("Leaderboard data count" + data.Scores.Length);

                // UserIds
                List<string> userIds = new List<string>();
                List<CustomLeaderBoardData> CustomLeaderData = new List<CustomLeaderBoardData>();

                foreach (IScore I in data.Scores)
                {
                    userIds.Add(I.userID);
                }

                Social.LoadUsers(userIds.ToArray(), (users) =>
                {
                    try
                    {
                        if (users == null || users.Length < 1)
                        {
                            UnityEngine.Console.LogWarning("Users length is Zero. Exiting.");
                            HandleFailedToGetLeaderboardData();
                            return;
                        }

                        foreach (IScore I in data.Scores)
                        {
                            IUserProfile user = FindUser(users, I.userID);
                            if (user == null) continue;

                            UnityEngine.Console.Log("User Name " + user.userName);
                            UnityEngine.Console.Log("User Score is " + I.value);

                            // Local Player
                            if (Social.localUser.id == user.id)
                            {
                                // Set the player ahead of the user (The latest entry added will be the one)
                                PlayerAheadOfUser = (CustomLeaderData != null && CustomLeaderData.Count > 0) ? CustomLeaderData[CustomLeaderData.Count - 1] : new CustomLeaderBoardData();

                                UnityEngine.Console.Log("Local Player Found");
                                UnityEngine.Console.Log("Your Name is " + user.userName + ". Saving your rank");
                                PlayerSavedRank = I.rank;
                                PlayerSavedScore = I.value;
                                CustomLeaderBoardData L = new CustomLeaderBoardData(user.userName, I.value, user, I.rank, true);
                                CustomLeaderData.Add(L);
                                // Marking Local Object as present
                                LocalLeaderBoardObjectPresent = true;
                                localuserindex = CustomLeaderData.Count - 1;
                            }
                            else
                            {
                                CustomLeaderBoardData T = new CustomLeaderBoardData(user.userName, I.value, user, I.rank, false);
                                CustomLeaderData.Add(T);
                            }
                        }

                        // Update the cached List
                        LoadedLeaderBoardListRef = CustomLeaderData;
                        UnityEngine.Console.Log("LeaderBoard List Cached");

                        List<CustomLeaderBoardData> copy = new List<CustomLeaderBoardData>(LoadedLeaderBoardListRef);
                        //try { ShowLeaderBoardData(GetLeaderBoardType, copy, true); }
                        //catch { }

                        UpdateAndDispatchLoadSuccessSignal(copy);

                        if (attemptLoadingImages)
                        {
                            DownloadAllImages(copy);
                        }
                        else
                        {
                            LeaderBoardState = LeaderBoardState.Loaded;
                            MarkOperationAsDoneAndResetDataOperationQueue();
                        }
                    }
                    catch
                    {
                        HandleFailedToGetLeaderboardData();
                        return;
                    }
                }

                );
            }
            catch
            {
                HandleFailedToGetLeaderboardData();
                return;
            }
        }*/

        private bool CheckAndSendCachedLeaderBoardDataIfAvailable()
        {
            if (LoadedLeaderBoardListRef != null && LoadedLeaderBoardListRef.Count > 0)
            {
                UnityEngine.Console.Log("Opening Cached Leaderboard Data");
                List<CustomLeaderBoardData> copy = new List<CustomLeaderBoardData>(LoadedLeaderBoardListRef);
                //try { ShowLeaderBoardData(GetLeaderBoardType, copy, true); }
                //catch { }

                UpdateAndDispatchLoadSuccessSignal(copy);

                if (attemptLoadingImages)
                {
                    DownloadAllImages(copy);
                }
                else
                {
                    MarkOperationAsDoneAndResetDataOperationQueue();
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        // Handle faliure while getting leaderboard data
        protected void HandleFailedToGetLeaderboardData()
        {
            UnityEngine.Console.LogWarning("Failed to retrieve leaderboard data");

            //try { ShowLeaderBoardData(GetLeaderBoardType, null, false); }
            //catch { }

            LeaderBoardState = LeaderBoardState.Stale;
            UpdateAndDispatchLoadFaliureSignal();
        }

        private async void DownloadAllImages(List<CustomLeaderBoardData> data)
        {
            if (data.Count <= 0) { UnityEngine.Console.Log("Image Count was 0"); return; }

            // Cancel the previous image downloading task
            if (theCancellationTokenSource != null)
                theCancellationTokenSource.Cancel();

            // Create new token & it's token source
            theCancellationTokenSource = new CancellationTokenSource();
            theCancellationToken = theCancellationTokenSource.Token;

            Task T = Task.Run(async () =>
            {
                foreach (var V in data)
                {
                    Task ImgDownload = Task.Run(async () =>
                    {
                        while (V.User.image == null) await Task.Yield();
                    }, theCancellationToken);
                    try
                    {
                        await ImgDownload;
                    }
                    catch (OperationCanceledException e)
                    {
                        UnityEngine.Console.LogWarning("Canceling Previous Image Downloading " + e);
                        break;
                    }
                }
            }, theCancellationToken);

            try
            {
                if (await Task.WhenAny(T, Task.Delay(30000)) == T)
                {
                    // Task completed within timeout
                    UnityEngine.Console.Log("Images Downloaded Successfully");

                    LeaderBoardState = LeaderBoardState.Loaded;
                    UpdateAndDispatchImagesDownloadedSignal(Status.Succeeded);
                    MarkOperationAsDoneAndResetDataOperationQueue();
                }
                else
                {
                    // Timeout logic
                    UnityEngine.Console.Log("Loading Images TimedOut");

                    if (theCancellationTokenSource != null)
                        theCancellationTokenSource.Cancel();

                    LeaderBoardState = LeaderBoardState.Loaded;
                    UpdateAndDispatchImagesDownloadedSignal(Status.Failed);
                    MarkOperationAsDoneAndResetDataOperationQueue();

                    return;
                }
            }
            catch (OperationCanceledException e)
            {
                UnityEngine.Console.LogWarning("Canceling Previous Image Downloading " + e);
                return;
            }

            // Download has finished
            try
            {
                UnityEngine.Console.Log("Sending Use Your Images Event");
                //UserYourImages(GetLeaderBoardType);
            }
            catch
            {
                UnityEngine.Console.LogWarning("No subscriber to downlolaed images");
            }
        }

        protected IUserProfile FindUser(IUserProfile[] users, string userid)
        {
            foreach (IUserProfile user in users)
            {
                if (user == null) { UnityEngine.Console.Log("LeaderBoard User Was Null"); continue; }

                if (user.id == userid)
                {
                    return user;
                }
            }

            return null;
        }

        /// <summary>
        /// Resets the leaderboard clearing any kind of cached data and parameters
        /// </summary>
        /// <returns>Whether the reset operation was successfull or not</returns>
        public virtual bool ResetLeaderBoardCache()
        {
            if (LeaderBoardState == LeaderBoardState.Loading)
            {
                UnityEngine.Console.LogWarning("LeaderBoard reset request declined as it's in loading state");
                return false;
            }

            localuserindex = 0;
            PlayerSavedRank = 0;
            PlayerSavedScore = 0;
            LocalLeaderBoardObjectPresent = false;
            LoadedLeaderBoardListRef = null;
            PlayerAheadOfUser = new CustomLeaderBoardData();

            ResetLeaderBoardDataOperationQueue();
            LeaderBoardState = LeaderBoardState.Stale;

            UnityEngine.Console.Log("LeaderBoard has been Reset");
            return true;
        }

        private void MarkOperationAsDoneAndResetDataOperationQueue()
        {
            int count = operationsQueue.Count;

            for (int i = 0; i < count; i++)
            {
                if (operationsQueue.Count == 0)
                {
                    continue;
                }

                AsyncOperationLeaderBoard asyncOperationLeaderBoard = operationsQueue.Dequeue();
                asyncOperationLeaderBoard.isDone = true;
            }

            ResetLeaderBoardDataOperationQueue();
        }

        private void ResetLeaderBoardDataOperationQueue()
        {
            if (operationsQueue.Count > 0)
            {
                UnityEngine.Console.LogWarning("Resetting the leaderboard data queue even though it contained pending handlers. Count: " + operationsQueue.Count);
                UpdateAndDispatchLoadFaliureSignal();
            }

            if (operationsQueue != null)
            {
                operationsQueue.Clear();
            }
            else
            {
                operationsQueue = new Queue<AsyncOperationLeaderBoard>();
            }
        }
    }
}