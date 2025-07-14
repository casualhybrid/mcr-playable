using Firebase.Firestore;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace TheKnights.LeaderBoardSystem
{
    /// <summary>
    /// An asynchronous handler that will contain the result after the operation is completed
    /// </summary>
    /// <typeparam name="T">The type of result</typeparam>
    public class LeaderBoardAsyncHandler<T>
    {
        public event Action<LeaderBoardAsyncHandler<T>> OnLeaderBoardOperationDone;

        public T Result;

        public void RaiseOnLeaderBoardOperationDone()
        {
            OnLeaderBoardOperationDone?.Invoke(this);
        }
    }

    [CreateAssetMenu(fileName = "LeaderBoardManager", menuName = "ScriptableObjects/FireStoreLeaderBoard/LeaderBoardManager")]
    public class LeaderBoardManager : ScriptableObject
    {
        #region Properties

        /// <summary>
        /// Is the leaderboard manager currently processing an existing group query request?
        /// </summary>
        public bool isProcessingGroupQuery { get; private set; }

        /// <summary>
        /// Reprents the entire current sorted leaderboard containing all users
        /// </summary>
        public LeaderBoardGroupQueryResponseData CurrentGroupInformation
        { get { return currentGroupInformation; } private set { currentGroupInformation = value; } }

        /// <summary>
        /// The version represents the acquiring order of the data. The higher the version, the more updated the data is.
        /// </summary>
        public int LeaderBoardGroupDataVersion { get; private set; }

        /// <summary>
        /// Represents the rewarded group currently being claimed. This is used to make sure that the the reference
        /// stays even if we get a leaderboard wipe during the claim process. Discard the reference after claiming reward.
        /// </summary>
        public GroupInfo CurrentRewardedGroupBeingClaimed
        { get { return currentRewardedGroupBeingClaimed; } set { currentRewardedGroupBeingClaimed = value; } }

        ///// <summary>
        ///// Represents the current group info including week number, country, group IDs and stamps
        ///// </summary>
        //public GroupInfo CurrentGroupInfo { get; private set; }

        #endregion Properties

        #region Private Fields

        private FirebaseFirestore _db;

        private FirebaseFirestore db
        {
            get
            {
                if (_db == null)
                {
                    _db = FirebaseFirestore.DefaultInstance;
                }

                return _db;
            }
        }

        [InfoBox("Using cache is recommended if you want to avoid several read operations", InfoMessageType.Info)]
        [SerializeField] private bool useCacheIfAvailable = true;

        [System.NonSerialized] private LeaderBoardGroupQueryResponseData currentGroupInformation;
        [System.NonSerialized] private GroupInfo currentRewardedGroupBeingClaimed;

        private readonly List<LeaderBoardAsyncHandler<LeaderBoardQueryResult<LeaderBoardGroupQueryResponseData>>> leaderBoardAsyncHandlers =
            new List<LeaderBoardAsyncHandler<LeaderBoardQueryResult<LeaderBoardGroupQueryResponseData>>>();

        private ListenerRegistration weekChangeListener;

        #endregion Private Fields

        #region MonoBehaviourCallBacks

        private void OnEnable()
        {
            LeaderBoardGroupDataVersion = 0;
            weekChangeListener = null;
            isProcessingGroupQuery = false;
            CurrentRewardedGroupBeingClaimed = null;
            CurrentGroupInformation = null;

            SubscribeToEvents();
        }

        private void OnDisable()
        {
            DeSubscribeToEvents();
            DesubscribeToLeaderBoardWeekChange();
        }

        #endregion MonoBehaviourCallBacks

        #region EventsSubscription

        public event Action<LeaderBoardQueryResult<LeaderBoardGroupQueryResponseData>> OnLeaderBoardGroupDataOperationDone;

        public event Action<LeaderBoardQueryResult<string>> OnLeaderBoardHighScoreOperationDone;

        public event Action<LeaderBoardGroupUserData> OnPlayerGroupProfileUpdated;

        public event Action<LeaderBoardGroupQueryResponseData> OnLeaderBoardGroupDetailsUpdated;

        private void SubscribeToEvents()
        {
            FireBaseAuthentication.OnUserLinkedAnAccountToFireBase += HandleAccountLinkedToFireBase;
            FireBaseAuthentication.OnUserSignedInToFireBaseWithSocialAccount += HandleFireBaseSignedInWithSocialAccount;
            FireBaseAuthentication.OnUserAnonymouslySignedIn += ClearCachedPlayerPrefs;

            LeaderBoardQueryManager.OnLeaderBoardGroupInfoOperationDone += HandleLeaderBoardGroupInfoOperation;
            LeaderBoardQueryManager.OnLeaderBoardHighScoreOperationDone += HandleLeaderBoardGroupHighScoreOperation;
            LeaderBoardQueryManager.OnLeaderBoardRewardClaimOperationDone += HandleLeaderBoardRewardClaimOperation;
            LeaderBoardQueryManager.OnLeaderBoardGroupProfileUpdateOperationDone += HandleLeaderBoardProfileUpdateOperation;
        }

        private void DeSubscribeToEvents()
        {
            FireBaseAuthentication.OnUserLinkedAnAccountToFireBase -= HandleAccountLinkedToFireBase;
            FireBaseAuthentication.OnUserSignedInToFireBaseWithSocialAccount -= HandleFireBaseSignedInWithSocialAccount;
            FireBaseAuthentication.OnUserAnonymouslySignedIn -= ClearCachedPlayerPrefs;

            LeaderBoardQueryManager.OnLeaderBoardGroupInfoOperationDone -= HandleLeaderBoardGroupInfoOperation;
            LeaderBoardQueryManager.OnLeaderBoardHighScoreOperationDone -= HandleLeaderBoardGroupHighScoreOperation;
            LeaderBoardQueryManager.OnLeaderBoardRewardClaimOperationDone -= HandleLeaderBoardRewardClaimOperation;
            LeaderBoardQueryManager.OnLeaderBoardGroupProfileUpdateOperationDone -= HandleLeaderBoardProfileUpdateOperation;
        }

        #endregion EventsSubscription

        #region LeaderBoardGroupQuery

        /// <summary>
        /// Send request to obtain current leaderboard group information for the player
        /// </summary>
        /// <param name="forceRefresh">If true, the leaderboard will forecfully try to obtain new data ignoring the cache</param>
        public LeaderBoardAsyncHandler<LeaderBoardQueryResult<LeaderBoardGroupQueryResponseData>> GetLeaderBoardGroupDetails(bool forceRefresh = false)
        {
            var leaderBoardAsyncHandler = new LeaderBoardAsyncHandler<LeaderBoardQueryResult<LeaderBoardGroupQueryResponseData>>();

            // Return cache if available
            if (!forceRefresh && useCacheIfAvailable && CurrentGroupInformation != null && CurrentGroupInformation.IsValid)
            {
                leaderBoardAsyncHandler.Result = new LeaderBoardQueryResult<LeaderBoardGroupQueryResponseData>(CurrentGroupInformation, false, null, -1);
                PushResultToGroupDetailsListeners(leaderBoardAsyncHandler);
                return leaderBoardAsyncHandler;
            }

            // Only run one request at a time
            if (!isProcessingGroupQuery)
            {
                ProcessLeaderBoardGroupDetailsRequest(leaderBoardAsyncHandler);
            }
            else
            {
                leaderBoardAsyncHandlers.Add(leaderBoardAsyncHandler);
            }

            return leaderBoardAsyncHandler;
        }

        public void WipeCachedLeaderBoardRewardedGroup()
        {
            if (CurrentGroupInformation == null)
            {
                UnityEngine.Console.LogWarning("A request to wipe reward cached data was made " +
                    "but there's no cached group information");
                return;
            }

            GroupInfo pendingGroup = CurrentGroupInformation.pendingRewardInfo;

            if (pendingGroup == null || !pendingGroup.IsValid)
            {
                UnityEngine.Console.LogWarning("A request to wipe reward cached data was made " +
                  "but there's no cached rewarded group information");
                return;
            }

            CurrentGroupInformation.pendingRewardInfo.WipeData();
            CurrentGroupInformation.pendingRewardInfo = null;
        }

        private void WipeCurrentCachedLeaderBoardRewardedGroup()
        {
            if (CurrentGroupInformation == null)
            {
                UnityEngine.Console.LogWarning("A request to wipe current group cached data was made " +
                    "but there's no cached group information");
                return;
            }

            GroupInfo pendingGroup = CurrentGroupInformation.currentGroupInfo;

            if (pendingGroup == null || !pendingGroup.IsValid)
            {
                UnityEngine.Console.LogWarning("A request to wipe current group cached data was made " +
                  "but there's no cached rewarded group information");
                return;
            }

            CurrentGroupInformation.currentGroupInfo.WipeData();
            CurrentGroupInformation.currentGroupInfo = null;
        }

        public void WipeWholeLeaderBoardCache()
        {
            WipeCachedLeaderBoardRewardedGroup();
            WipeCurrentCachedLeaderBoardRewardedGroup();

            CurrentGroupInformation = null;

            UnityEngine.Console.Log("LeaderBoard wiped");
        }

        private async void ProcessLeaderBoardGroupDetailsRequest(LeaderBoardAsyncHandler<LeaderBoardQueryResult<LeaderBoardGroupQueryResponseData>> leaderBoardAsyncHandler)
        {
            isProcessingGroupQuery = true;
            leaderBoardAsyncHandlers.Add(leaderBoardAsyncHandler);

            // Wait for firebase to initialization process
            if (!FireBaseInitializer.IsFireBaseInitializationProcessDone)
             {
                await Task.Run(() =>
                {
                    while (!FireBaseInitializer.IsFireBaseInitializationProcessDone)
                    {
                        
                    }
                });
            }

            string playerToken = await FireBaseAuthentication.PlayerAuthenticationToken;

            if (string.IsNullOrEmpty(playerToken))
            {
                var result = GetGeneralFailedGroupQueryTask(new Exception("Failed to obtain authentication token"));
                PushResultToGroupDetailsListeners(result);
                return;
            }

            LeaderBoardQueryManager.Instance.RequestPlayersGroupInformation(playerToken);
        }

        private async void HandleLeaderBoardGroupInfoOperation(LeaderBoardQueryResult<string> operation)
        {
            if (operation.IsError || !operation.IsValid)
            {
                RequestTokenIfUnauthorized(operation.ResponseCode);
                UnityEngine.Console.Log($"LeaderBoard group query failed. Reason ${operation.Exception}");

                LeaderBoardQueryResult<LeaderBoardGroupQueryResponseData> res =
                  new LeaderBoardQueryResult<LeaderBoardGroupQueryResponseData>(null, operation.IsError, operation.Exception, operation.ResponseCode);

                PushResultToGroupDetailsListeners(res);
                return;
            }

            string responseStr = operation.ResponseBody;

            if (string.IsNullOrEmpty(responseStr))
            {
                string exceptionMessage = $"LeaderBoard group query response was valid but null/empty";
                UnityEngine.Console.Log(exceptionMessage);

                LeaderBoardQueryResult<LeaderBoardGroupQueryResponseData> res =
                 new LeaderBoardQueryResult<LeaderBoardGroupQueryResponseData>(null, true, new Exception(exceptionMessage), operation.ResponseCode);

                PushResultToGroupDetailsListeners(res);
                return;
            }

            LeaderBoardQueryResult<LeaderBoardGroupQueryResponseData> result = default;

            try
            {
                LeaderBoardGroupQueryResponseData leaderBoardResponseData = JsonUtility.FromJson<LeaderBoardGroupQueryResponseData>(responseStr);

                List<LeaderBoardGroupUserData> currentLeaderBoardGroupInformation = new List<LeaderBoardGroupUserData>();

                GroupInfo currentGroupInfo = leaderBoardResponseData.currentGroupInfo;
                GroupInfo pendingRewardGroupInfo = leaderBoardResponseData.pendingRewardInfo;

                var t = Task.WhenAll(SetLeaderBoardGroupUsersDataFromGroupID(currentGroupInfo, false), SetLeaderBoardGroupUsersDataFromGroupID(pendingRewardGroupInfo, true));

                try
                {
                    await t;
                }
                catch
                {
                    UnityEngine.Console.Log($"Exception while setting up current and pending groups users data {t.Exception}");
                }

                if ((currentGroupInfo == null || !currentGroupInfo.IsValid) && (pendingRewardGroupInfo == null || !pendingRewardGroupInfo.IsValid))
                {
                    throw new System.Exception("No info for neither current or pending reward groups");
                }

                result = new LeaderBoardQueryResult<LeaderBoardGroupQueryResponseData>(leaderBoardResponseData, operation.IsError, operation.Exception, operation.ResponseCode);

                WipeWholeLeaderBoardCache();

                // Cache the result
                CurrentGroupInformation = result.ResponseBody;
                CurrentGroupInformation.Version = ++LeaderBoardGroupDataVersion;

                // Start listening to week change
                if (weekChangeListener == null)
                {
                    ListenToLeaderBoardWeekChange();
                }
            }
            catch (Exception e)
            {
                UnityEngine.Console.LogWarning($"An error occurred while trying to process valid leaderboard group query data. Exception: {e.Message}");

                result = new LeaderBoardQueryResult<LeaderBoardGroupQueryResponseData>(null, true, e, operation.ResponseCode);
            }
            finally
            {
                PushResultToGroupDetailsListeners(result);
                OnLeaderBoardGroupDataOperationDone?.Invoke(result);

                if (!result.IsError && result.IsValid)
                {
                    SendLeaderBoardUpdatedEvent();
                }
            }
        }

        private void SetRanksOfLeaderBoardGroupPlayers(GroupInfo groupInfo)
        {
            List<LeaderBoardGroupUserData> users = groupInfo.usersData;

            if (groupInfo.leaderBoardRankScoreThresholds == null)
            {
                groupInfo.leaderBoardRankScoreThresholds = new int[Enum.GetNames(typeof(LeaderBoardRank)).Length];
            }

            int baseScore = users[0].score;
            float factor = 1;

            for (int l = 0; l < groupInfo.leaderBoardRankScoreThresholds.Length; l++)
            {
                groupInfo.leaderBoardRankScoreThresholds[l] = (int)((float)baseScore * factor);
                factor -= 0.2f;
            }

            for (int k = 0; k < users.Count; k++)
            {
                LeaderBoardGroupUserData entity = users[k];

                for (int i = 0; i < groupInfo.leaderBoardRankScoreThresholds.Length; i++)
                {
                    if (entity.score >= groupInfo.leaderBoardRankScoreThresholds[i])
                    {
                        entity.LeaderBoardRank = (LeaderBoardRank)i;
                        break;
                    }

                    if (i == groupInfo.leaderBoardRankScoreThresholds.Length - 1)
                    {
                        entity.LeaderBoardRank = (LeaderBoardRank)i;
                    }
                }
            }
        }

        private async Task SetLeaderBoardGroupUsersDataFromGroupID(GroupInfo currentGroupInfo, bool isRewardGroup)
        {
            UnityEngine.Console.Log($"Was group info null? {currentGroupInfo == null}");

            string lastRealLeaderBoardFetchedDateString = PlayerPrefs.GetString(PlayerPrefKeys.LastTimeRealLeaderBoardFetched, null);
            int cachedHighScore = PlayerPrefs.GetInt(PlayerPrefKeys.LastHighScoreSubmittedToLeaderBoard);
            string lastStampHighScoreWasSubmittedFor = PlayerPrefs.GetString(PlayerPrefKeys.LastStampHighScoreWasSubmittedFor);

            List<LeaderBoardGroupUserData> currentLeaderBoardGroupInformation = await Task.Run(async () =>
            {
                List<LeaderBoardGroupUserData> _currentLeaderBoardGroupInformation = new List<LeaderBoardGroupUserData>();

                UnityEngine.Console.Log("Making Query");

                Source source = Source.Server;

                // Try a cached query if its a NON rewarded group
                if (!isRewardGroup)
                {
                    DateTime lastRealLeaderBoardFetchedDate;
                    bool success = DateTime.TryParse(lastRealLeaderBoardFetchedDateString, out lastRealLeaderBoardFetchedDate);

                    if (success)
                    {
                        TimeSpan timePassedSinceLastRealUpdate = DateTime.Now.Subtract(lastRealLeaderBoardFetchedDate);

                        // Do cachaed update if within cached time zone and last used firebaseID is same as current
                        if (timePassedSinceLastRealUpdate.Hours < 3)
                        {
                            UnityEngine.Console.Log("Less than three hours has passed since real leadeboard update. Trying cached query for current group");
                            source = Source.Cache;
                        }
                    }
                    else
                    {
                        UnityEngine.Console.Log($"Failed parsing last real leaderboard fetched date string. Date string value : {lastRealLeaderBoardFetchedDateString}");
                    }
                }

                Query groupReferenceQuery = db.Collection($"LeaderBoardTournament/GeneralTournament/Tournaments/{currentGroupInfo.stamp}/GeneralCountryGroups" +
                    $"/{currentGroupInfo.userCountry}/CountrySubGroups/{currentGroupInfo.userGroupID}/SubGroupUsers").OrderByDescending("score");

                UnityEngine.Console.Log($"Running Query. Source: {source}");

                QuerySnapshot groupSnapShot;

                try
                {
                    groupSnapShot = await groupReferenceQuery.GetSnapshotAsync(source);
                }
                catch (Exception e)
                {
                    if (source == Source.Cache)
                    {
                        source = Source.Server;
                        UnityEngine.Console.Log("Cache query failed. Retrying from server");
                        groupSnapShot = await groupReferenceQuery.GetSnapshotAsync(source);
                    }
                    else
                    {
                        throw e;
                    }
                }

                if (groupSnapShot == null)
                    throw new System.Exception($"Group snapshot was null. Is reward group? : {isRewardGroup}");


                UnityEngine.Console.Log($"Query was successfull. Was it from cache? : {groupSnapShot.Metadata?.IsFromCache}");

                // Check whether it was a real time update (given group is NON rewarded)
                if (!isRewardGroup)
                {
                    if (source == Source.Server && groupSnapShot.Count > 0)
                    {
                        UnityEngine.Console.Log($"Cached last time leaderboard updated in real time");

                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        {
                            PlayerPrefs.SetString(PlayerPrefKeys.LastTimeRealLeaderBoardFetched, DateTime.Now.ToString());
                        });
                    }
                }


                var usersDocs = groupSnapShot.Documents;

                UnityEngine.Console.Log($"Query group {currentGroupInfo.userGroupID} documents count :" + groupSnapShot.Count);

                if (groupSnapShot.Count == 0)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => { ClearCachedPlayerPrefs(); });
                 
                    throw new System.Exception("No Users found");
                }

                foreach (var doc in usersDocs)
                {
                    LeaderBoardGroupUserData entity = doc.ConvertTo<LeaderBoardGroupUserData>();
                    _currentLeaderBoardGroupInformation.Add(entity);

                    if (entity.myID == FireBaseAuthentication.FireBaseUserID)
                    {
                        currentGroupInfo.localPlayerData = entity;
                    }
                }

                if (currentGroupInfo.localPlayerData == null)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() => { ClearCachedPlayerPrefs(); });
                    throw new System.Exception("No local user found");
                }

                if (_currentLeaderBoardGroupInformation.Count == 0)
                {
                    throw new System.Exception($"No Users Found {currentGroupInfo.userGroupID}");
                }

                currentGroupInfo.usersData = _currentLeaderBoardGroupInformation;

                // Read highscore from cache if source is Cached or from server (given last highscore submitted stamp is similar to the acquired ones and for the same ID)
                if (!isRewardGroup)
                {
                    if ((source == Source.Cache || (source == Source.Server && currentGroupInfo.stamp == lastStampHighScoreWasSubmittedFor))
                    && cachedHighScore > currentGroupInfo.localPlayerData.score)
                    {
                        currentGroupInfo.localPlayerData.score = cachedHighScore;

                        // Sort the data again
                        currentGroupInfo.usersData.Sort((a, b) => b.CompareTo(a));
                    }
                }

                // Set ranks
                SetRanksOfLeaderBoardGroupPlayers(currentGroupInfo);

            
                return _currentLeaderBoardGroupInformation;
            });

            UnityEngine.Console.Log("Starting loading country flag");

            // Get Country Flag

            TaskCompletionSource<bool> taskCompletionSourceFlag = new TaskCompletionSource<bool>();
            ResourceRequest flagLoadrequest = Resources.LoadAsync<Sprite>($"CountryFlags/{currentGroupInfo.userCountryIso}_64");
            bool isDone = false;
            flagLoadrequest.completed += (handle) => { isDone = true; taskCompletionSourceFlag.SetResult(isDone); };

            await taskCompletionSourceFlag.Task;

            if (flagLoadrequest != null && flagLoadrequest.asset != null)
            {
                Sprite flagSprite = flagLoadrequest.asset as Sprite;
                if (flagSprite != null)
                {
                    currentGroupInfo.countrySprite = flagSprite;
                }
            }
            else
            {
                UnityEngine.Console.LogWarning($"Failed to get country flag sprite for counter ISO {currentGroupInfo.userCountryIso}");
            }

            // Get user Pics

            foreach (var user in currentLeaderBoardGroupInformation)
            {
                try
                {
                    user.LoadUserProfileImageTextureFromBytes();

                    await Task.Yield();
                }
                catch (Exception e)
                {
                    UnityEngine.Console.LogWarning($"Loading image failed for user ${user.userName}.Error: {e}. Skipping User");
                }
            }
        }

        private async void PushResultToGroupDetailsListeners(LeaderBoardQueryResult<LeaderBoardGroupQueryResponseData> result)
        {
            await Task.Yield();

            isProcessingGroupQuery = false;

            for (int i = 0; i < leaderBoardAsyncHandlers.Count; i++)
            {
                var handler = leaderBoardAsyncHandlers[i];
                handler.Result = result;

                handler.RaiseOnLeaderBoardOperationDone();
            }

            leaderBoardAsyncHandlers.Clear();
        }

        private async void PushResultToGroupDetailsListeners(LeaderBoardAsyncHandler<LeaderBoardQueryResult<LeaderBoardGroupQueryResponseData>> handler)
        {
            await Task.Yield();

            handler.RaiseOnLeaderBoardOperationDone();
        }

        private LeaderBoardQueryResult<LeaderBoardGroupQueryResponseData> GetGeneralFailedGroupQueryTask(Exception e)
        {
            return new LeaderBoardQueryResult<LeaderBoardGroupQueryResponseData>(null, true, e, -1);
        }

        #endregion LeaderBoardGroupQuery

        #region LeaderBoardHighScoreRequest

        /// <summary>
        /// Submits a request to update the provided highscore of the player to the leaderboard group
        /// </summary>
        /// <param name="score">The highscore to update</param>
        public async void SubmitHighScoreToPlayersGroup(int score)
        {
            LocallyUpdateThePlayersHighScoreAndResortData(score);

            string playerToken = await FireBaseAuthentication.PlayerAuthenticationToken;

            if (string.IsNullOrEmpty(playerToken))
                return;

            LeaderBoardQueryManager.Instance.RequestHighScoreSubmissionToGroup(playerToken, score);
        }

        // Just a dummy effect. Has no effect on the database
        private void LocallyUpdateThePlayersHighScoreAndResortData(int score)
        {
            if (CurrentGroupInformation != null && CurrentGroupInformation.currentGroupInfo != null && CurrentGroupInformation.currentGroupInfo.IsValid)
            {
                LeaderBoardGroupUserData userData = CurrentGroupInformation.currentGroupInfo.localPlayerData;

                if (score <= userData.score)
                    return;

                PlayerPrefs.SetInt(PlayerPrefKeys.LastHighScoreSubmittedToLeaderBoard, score);
                PlayerPrefs.SetString(PlayerPrefKeys.LastStampHighScoreWasSubmittedFor, currentGroupInformation.currentGroupInfo.stamp);

                userData.score = score;

                CurrentGroupInformation.currentGroupInfo.usersData.Sort((a, b) => b.score.CompareTo(a.score));

                SetRanksOfLeaderBoardGroupPlayers(CurrentGroupInformation.currentGroupInfo);
            }
        }

        private void HandleLeaderBoardGroupHighScoreOperation(LeaderBoardQueryResult<string> operation)
        {
            if (operation.IsError || !operation.IsValid)
            {
                RequestTokenIfUnauthorized(operation.ResponseCode);
                UnityEngine.Console.Log($"Failed to update high score to the database. Reason {operation.Exception}");
            }

            if (operation.ResponseCode == 200)
            {
                UnityEngine.Console.Log($"Successfully submitted highscore to group");
            }

            OnLeaderBoardHighScoreOperationDone?.Invoke(operation);
        }

        #endregion LeaderBoardHighScoreRequest

        #region LeaderBoardListeners

        private void ListenToLeaderBoardWeekChange()
        {
            DocumentReference leaderBoardDocRef = db.Document("LeaderBoardTournament/GeneralTournament");

            //**Note: This will cause an initial read operation (callback would be invoked once on registering the listener)
            weekChangeListener = leaderBoardDocRef.Listen(MetadataChanges.Exclude, LeaderBoardWeekChangeListener);
        }

        private void DesubscribeToLeaderBoardWeekChange()
        {
            if (weekChangeListener != null)
            {
                weekChangeListener.Stop();
            }
        }

        private void LeaderBoardWeekChangeListener(DocumentSnapshot documentSnapshot)
        {
            if (documentSnapshot.Metadata.IsFromCache)
            {
                UnityEngine.Console.Log("Ignoring week document change read as the data is from the cache");
                return;
            }

            if (CurrentGroupInformation == null || CurrentGroupInformation.currentGroupInfo == null || !CurrentGroupInformation.currentGroupInfo.IsValid)
            {
                UnityEngine.Console.Log("A change in week was detected but there's no currently loaded leaderboard/Current group data");
                return;
            }

            var data = documentSnapshot.ToDictionary();

            int week = (int)(long)data["week"];

            int curWeek = int.Parse(CurrentGroupInformation.currentGroupInfo.weekNumber);

            if (week == curWeek)
            {
                UnityEngine.Console.Log($"Leaderboard setup document changed but there's no week change. Week No. {week}");
                return;
            }

            if (week < curWeek)
            {
                UnityEngine.Console.LogWarning($"The changed week is even less than the current week!");
                return;
            }

            PlayerPrefs.SetString(PlayerPrefKeys.LastTimeRealLeaderBoardFetched, null);

            UnityEngine.Console.Log($"LeaderBoard week has changed!. Old week was {curWeek}, New week is {week}");

            WipeWholeLeaderBoardCache();

            // Force refresh the leaderboard
            GetLeaderBoardGroupDetails(true);
        }

        #endregion LeaderBoardListeners

        #region LeaderBoardRewardClaimRequest

        public void ClaimThePendingReward()
        {
            string rewardStamp = CurrentRewardedGroupBeingClaimed.stamp;

            // Also wipe the currently claimed reward
            CurrentRewardedGroupBeingClaimed = null;

            SendLeaderBoardClaimRequest(rewardStamp);

            WipeCachedLeaderBoardRewardedGroup();

            UpdateLeaderBoardGroupVersionAndNotify();
        }

        private async void SendLeaderBoardClaimRequest(string rewardStamp)
        {
            string playerToken = await FireBaseAuthentication.PlayerAuthenticationToken;

            if (string.IsNullOrEmpty(playerToken))
            {
                UnityEngine.Console.LogWarning($"Attempt to send claim reward request was made but failed to get firebase token");
                return;
            }

            LeaderBoardQueryManager.Instance.RequestPendingRewardDeletion(playerToken, rewardStamp);
        }

        private void HandleLeaderBoardRewardClaimOperation(LeaderBoardQueryResult<string> result)
        {
            UnityEngine.Console.Log("Reward claim request result " + result.ResponseCode);
        }

        #endregion LeaderBoardRewardClaimRequest

        #region LeaderBoardProfileUpdateRequest

        /// <summary>
        /// Submits a request to update the players leaderboard group profile
        /// </summary>
        public async void SendRequestToUpdateGroupProfile()
        {
            string playerToken = await FireBaseAuthentication.PlayerAuthenticationToken;

            if (string.IsNullOrEmpty(playerToken))
                return;

            LeaderBoardQueryManager.Instance.RequestUpdateGroupProfile(playerToken);
        }

        private async void HandleLeaderBoardProfileUpdateOperation(LeaderBoardQueryResult<string> operation)
        {
            if (operation.IsError || !operation.IsValid)
            {
                RequestTokenIfUnauthorized(operation.ResponseCode);
                UnityEngine.Console.Log($"Failed to update player group profile. Reason {operation.Exception}");
                return;
            }

            if (operation.ResponseCode == 200)
            {
                UnityEngine.Console.Log($"Successfully obtained new user group profile. " + operation.ResponseBody);
            }

            try
            {
                // We can just return the data here to save a read but sadly jsonserializer can't deserialize properties
                // and i don't want to create a whole new class just for that (^_^)

                string pathToPlayerDoc = operation.ResponseBody;

                var doc = await db.Document(pathToPlayerDoc).GetSnapshotAsync();

                LeaderBoardGroupUserData userUpdatedProfile = doc.ConvertTo<LeaderBoardGroupUserData>();

                if (CurrentGroupInformation != null && CurrentGroupInformation.currentGroupInfo != null && CurrentGroupInformation.currentGroupInfo.IsValid)
                {
                    // Keep the old rank
                    userUpdatedProfile.LeaderBoardRank = CurrentGroupInformation.currentGroupInfo.localPlayerData.LeaderBoardRank;

                    // Keep the old score as our focus is on user info like name, pic
                    userUpdatedProfile.score = CurrentGroupInformation.currentGroupInfo.localPlayerData.score;

                    // fix
                    CurrentGroupInformation.currentGroupInfo.localPlayerData = userUpdatedProfile;

                    for (int i = 0; i < CurrentGroupInformation.currentGroupInfo.usersData.Count; i++)
                    {
                        var usr = CurrentGroupInformation.currentGroupInfo.usersData[i];

                        if (usr.myID == userUpdatedProfile.myID)
                        {
                            CurrentGroupInformation.currentGroupInfo.usersData[i] = userUpdatedProfile;
                            break;
                        }
                    }

                    // Update profile picture
                    userUpdatedProfile.LoadUserProfileImageTextureFromBytes();

                    OnPlayerGroupProfileUpdated?.Invoke(userUpdatedProfile);
                    UpdateLeaderBoardGroupVersionWithoutNotifying();
                }
                else
                {
                    UnityEngine.Console.Log("Player profile was received but there's no cached leaderboard. Ignoring");
                }
            }
            catch (Exception e)
            {
                UnityEngine.Console.LogWarning($"Failed to deserialize and update the local user profile even though the request was successfull. Exception {e}");
            }
        }

        #endregion LeaderBoardProfileUpdateRequest

        /// <summary>
        /// Get the local player's profile picture data in bytes if loaded
        /// </summary>
        /// <returns>The local player's profile picture in bytes</returns>
        public byte[] GetPlayerProfilePictureData()
        {
            try
            {
                if (CurrentGroupInformation != null && CurrentGroupInformation.currentGroupInfo != null && CurrentGroupInformation.currentGroupInfo.IsValid
                    && CurrentGroupInformation.currentGroupInfo.localPlayerData != null && CurrentGroupInformation.currentGroupInfo.localPlayerData.profilePic != null)
                {
                    return CurrentGroupInformation.currentGroupInfo.localPlayerData.profilePic.ToBytes();
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private async void RequestTokenIfUnauthorized(long statusCode)
        {
            if (statusCode == 401)
            {
                UnityEngine.Console.Log("Player is unauthorized to query/request leaderBoard. Sending request to refresh token");
                await FireBaseAuthentication.RefreshUserAuthenticationToken();
            }
        }

        private void HandleAccountLinkedToFireBase(string providerID)
        {
            SendRequestToUpdateGroupProfile();
        }

        private void HandleFireBaseSignedInWithSocialAccount(string providerID)
        {
            ClearCachedPlayerPrefs();
            UnityEngine.Console.Log($"User logged in to firebase with provider {providerID}");
            //. Wiping the leaderboard and refetching!
            //  WipeWholeLeaderBoardCache();
            GetLeaderBoardGroupDetails(true);
        }

        private void SendLeaderBoardUpdatedEvent()
        {
            OnLeaderBoardGroupDetailsUpdated?.Invoke(CurrentGroupInformation);
        }

        private void UpdateLeaderBoardGroupVersionAndNotify()
        {
            if (CurrentGroupInformation == null)
                return;

            CurrentGroupInformation.Version = ++LeaderBoardGroupDataVersion;

            OnLeaderBoardGroupDetailsUpdated?.Invoke(CurrentGroupInformation);
        }

        private void UpdateLeaderBoardGroupVersionWithoutNotifying()
        {
            if (CurrentGroupInformation == null)
                return;

            CurrentGroupInformation.Version = ++LeaderBoardGroupDataVersion;
        }

        private void ClearCachedPlayerPrefs()
        {
            PlayerPrefs.DeleteKey(PlayerPrefKeys.LastTimeRealLeaderBoardFetched);
            PlayerPrefs.DeleteKey(PlayerPrefKeys.LastStampHighScoreWasSubmittedFor);
            PlayerPrefs.DeleteKey(PlayerPrefKeys.LastHighScoreSubmittedToLeaderBoard);
        }
    }
}