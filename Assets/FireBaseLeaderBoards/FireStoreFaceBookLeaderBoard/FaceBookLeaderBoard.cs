using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheKnights.FaceBook;
using UnityEngine;

namespace TheKnights.LeaderBoardSystem.FaceBook
{

    public class InternetNotAvailableException : System.Exception
    {
        public override string Message => "Internet not available";
    }


    /// <summary>
    /// Represents the fetched leaderboard data
    /// </summary>
    public class FaceBookLeaderBoardData
    {
        public List<FaceBookLeaderBoardUserData> Data { get; private set; }

        public FaceBookLeaderBoardUserData LocalPlayerData { get; private set; }

        public FaceBookLeaderBoardData(List<FaceBookLeaderBoardUserData> _data, FaceBookLeaderBoardUserData _localPlayerData)
        {
            Data = _data;
            LocalPlayerData = _localPlayerData;
        }

        public bool isValid => Data != null && Data.Count > 0 && LocalPlayerData != null;
    }

    [CreateAssetMenu(fileName = "FaceBookLeaderBoard", menuName = "ScriptableObjects/FireStoreLeaderBoard/FaceBook/FaceBookLeaderBoardManager")]
    public class FaceBookLeaderBoard : ScriptableObject
    {
        private static class LeaderBoardPaths
        {
            public const string RootUsers = "FaceBook/LeaderBoard/Users";
        }

        [SerializeField] private FaceBookManager faceBookManager;

       // private FirebaseFirestore _db;

        //private FirebaseFirestore db
        //{
        //    get
        //    {
        //        if (_db == null)
        //        {
        //            _db = FirebaseFirestore.DefaultInstance;
        //        }

        //        return _db;
        //    }
        //}

        private bool isLoading;
        private Task<FaceBookLeaderBoardData> currentlyRunningDataTask;

        public FaceBookLeaderBoardData CachedLeaderBoardData { get; private set; }

        private void OnEnable()
        {
            isLoading = false;
            currentlyRunningDataTask = null;

            FaceBookManager.OnUserLoggedOutFromFaceBook.AddListener(WipeCachedLeaderBoardData);
        }

        private void OnDisable()
        {
            FaceBookManager.OnUserLoggedOutFromFaceBook.RemoveListener(WipeCachedLeaderBoardData);
        }

        /// <summary>
        /// Submits a request to update the users highscore to facebook leaderboard
        /// </summary>
        /// <param name="score">The score to be submitted</param>
        /// <param name="profilePicBytes">Optional profile picture data to be uploaded</param>
        public async Task SubmitHighScoreToLeaderBoard(int score, byte[] profilePicBytes = null)
        {
           // string userFaceBookID = FaceBookManager.GetUserFaceBookID;
           // string userFaceBookName = FaceBookManager.GetUserFaceBookName;

            //if (string.IsNullOrEmpty(userFaceBookID))
            //{
            //    UnityEngine.Console.Log("Failed to submit score to facebook leaderboard as the user facebook id is null/empty");
            //    return;
            //}

           

            // Update the score locally
            

          //  DocumentReference userLeaderBoardDoc = db.Document($"{LeaderBoardPaths.RootUsers}/{userFaceBookID}");

            /* We run a transaction where we first get the user's current highscore value,
             *  comparing it with the requested ones. If the highscore/document is modified from somewhere
             * else while the operation is being carried out, the transaction will be cancelled. For more
             * information check out the documentation about transactions
            */
            try
            {
               // await db.RunTransactionAsync(async (transaction) =>
               //{
               //    DocumentSnapshot leaderBoardDocSnap = await transaction.GetSnapshotAsync(userLeaderBoardDoc);

               //    int currentHighScore = -1;

               //    try
               //    {
               //        // cast to long first since we need to unbox the value (originally "long" in firestore)
               //        currentHighScore = (int)(long)leaderBoardDocSnap.ToDictionary()["score"];
               //    }
               //    catch (Exception e)
               //    {
               //        UnityEngine.Console.Log($"Failed to retrieve old facebook leaderboard score. Maybe it doesn't exist yet. Exception {e}");
               //    }

               //    if (score <= currentHighScore)
               //    {
               //        throw new System.Exception($"Curr highscore {currentHighScore} is greater than the request ones {score}");
               //    }

               //    var dataToSubmit = new Dictionary<string, object>() { { "score", score }, { "name", userFaceBookName } };

               //    if (profilePicBytes != null)
               //    {
               //        dataToSubmit.Add("profilePic", Blob.CopyFrom(profilePicBytes));
               //    }

               //    transaction.Set(userLeaderBoardDoc, dataToSubmit, SetOptions.MergeAll);
               //});

               // UnityEngine.Console.Log("Transaction to update player highscore completed successfully");
            }
            catch (Exception e)
            {
                UnityEngine.Console.Log($"Transaction to update player facebook highscore failed. Exception: {e}");
            }
        }

        /// <summary>
        /// Request to retrieve facebook leaderboard data
        /// </summary>
        /// <returns>A task containing leaderboard data as result</returns>
        public Task<FaceBookLeaderBoardData> GetFaceBookLeaderBoardData(bool forceRefresh = false)
        {
            if (isLoading && currentlyRunningDataTask == null)
            {
                UnityEngine.Console.LogError("FaceBook leaderboard is loading while a request was recieved and the currently loading task is null");
                return null;
            }

            if (isLoading)
            {
                UnityEngine.Console.Log($"Facebook leaderboard already being fetched. Returning running task");
                return currentlyRunningDataTask;
            }

            if (!forceRefresh && CachedLeaderBoardData != null && CachedLeaderBoardData.isValid)
            {
                return Task.FromResult<FaceBookLeaderBoardData>(CachedLeaderBoardData);
            }

            isLoading = true;

          //  currentlyRunningDataTask = GetFaceBookLeaderBoardDataTask();

            return currentlyRunningDataTask;
        }

        //private async Task<FaceBookLeaderBoardData> GetFaceBookLeaderBoardDataTask()
        //{
        //    try
        //    {
        //        List<FaceBookUserProfile> faceBookFriends = await faceBookManager.GetUserFriends();

        //        if (faceBookFriends == null)
        //        {
        //            faceBookFriends = new List<FaceBookUserProfile>();
        //        }

        //        // Add users own profile
        //        FaceBookUserProfile userProfile = await faceBookManager.BuildAndRetrievePlayerInformation();

        //        if (userProfile == null)
        //        {
        //            throw new System.Exception("Failed getting user profile");
        //        }

        //        var _result = await Task.Run(async () =>
        //         {
        //             faceBookFriends.Add(userProfile);

        //             /*For some reason the IEnumearble returned by Linq Select method is more than just a
        //              * simple IEnumerable interface (some underlying class) which our query method doesn't
        //              * accept. That's why we have to convert it to list
        //              * Not sure what's going on here though!!!!!!!
        //              */
        //             IEnumerable<string> friendIDs = faceBookFriends.Select((x, index) =>
        //             {
        //                 return x.userID;
        //             }).ToList();

        //            // IEnumerable<DocumentSnapshot> friendsDocSnapshots = default;
        //             bool isSorted = false;

        //        //     if (friendIDs.Count() <= 10)
        //        //     {
        //        //         /*We cannot order it by score because when using inequality or equality filter on "__name__",
        //        //          * the first orderBy must also be on "__name__" property. We would have to sort the data ourselves.
        //        //          * However, one workaround could have been to use FaceBook ID as a field instead of as the document name
        //        //          * But, that would have costed a read operation since we wouldn't have known the automated ID of the
        //        //          * document.
        //        //          */
        //        //         QuerySnapshot friendsDocSnapshot = await db.Collection($"{LeaderBoardPaths.RootUsers}").WhereIn("__name__", friendIDs).GetSnapshotAsync();
        //        //         friendsDocSnapshots = friendsDocSnapshot.Documents;
        //        //     }
        //        //     else
        //        //     {
        //        //         List<Task<DocumentSnapshot>> t = new List<Task<DocumentSnapshot>>();

        //        //         foreach (var friend in faceBookFriends)
        //        //         {
        //        //             Task<DocumentSnapshot> docTask = db.Document($"{LeaderBoardPaths.RootUsers}/{friend.userID}").GetSnapshotAsync();
        //        //             t.Add(docTask);
        //        //         }

        //        //         try
        //        //         {
        //        //             await Task.WhenAll(t);
        //        //         }
        //        //         catch (Exception e)
        //        //         {
        //        //             UnityEngine.Console.LogWarning($"An exception occurred while trying to fetch a facebook leaderboard user data. Exception {e}");
        //        //         }

        //        //         List<DocumentSnapshot> documentSnapshots = new List<DocumentSnapshot>();

        //        //         foreach (Task<DocumentSnapshot> task in t)
        //        //         {
        //        //             if (!task.IsCompletedSuccessfully || task.Result == null)
        //        //             {
        //        //                 continue;
        //        //             }

        //        //             documentSnapshots.Add(task.Result);
        //        //         }

        //        //         friendsDocSnapshots = documentSnapshots;
        //        //     }

        //        //     if (friendsDocSnapshots.Count() == 0)
        //        //     {
        //        //         throw new System.Exception("No users found when qureyed from database");
        //        //     }

        //        //     List<FaceBookLeaderBoardUserData> result = new List<FaceBookLeaderBoardUserData>();
        //        //     FaceBookLeaderBoardUserData localPlayer = null;

        //        //     foreach (var friendDoc in friendsDocSnapshots)
        //        //     {
        //        //         try
        //        //         {
        //        //             FaceBookLeaderBoardUserData entity = friendDoc.ConvertTo<FaceBookLeaderBoardUserData>();
        //        //             result.Add(entity);

        //        //             if (friendDoc.Id == FaceBookManager.GetUserFaceBookID)
        //        //             {
        //        //                 localPlayer = entity;
        //        //             }
        //        //         }
        //        //         catch
        //        //         {
        //        //             UnityEngine.Console.LogWarning($"Failed to deseialize a facebook users leaderboard snapshot data");
        //        //         }
        //        //     }

        //        //     if (localPlayer == null)
        //        //         throw new System.Exception("Failed to find the local player");

        //        //     /* Sort if required.
        //        //      * Having a really high number of friends who play the game
        //        //      * is highly unlikely
        //        //     */
        //        //     if (!isSorted)
        //        //     {
        //        //         result.Sort((a, b) => b.CompareTo(a));
        //        //         isSorted = true;
        //        //     }

        //        //     return new FaceBookLeaderBoardData(result, localPlayer);
        //        // });

        //        //for (int i = 0; i < _result.Data.Count; i++)
        //        //{
        //        //    _result.Data[i].LoadProfilePictureFromBytes();
        //        //}

        //        //// Cache the result
        //        //CachedLeaderBoardData = _result;

        //        return _result;
        //    }
        //    catch (Exception e)
        //    {
        //        bool isInternetAvailable = await Utilities.IsInternetAvailable();

        //        if (!isInternetAvailable)
        //        {
        //            throw new InternetNotAvailableException();
        //        }

        //        throw e;
        //    }
        //    finally
        //    {
        //        isLoading = false;
        //    }
        //}

        private void WipeCachedLeaderBoardData()
        {
            if (CachedLeaderBoardData == null)
                return;

            for (int i = 0; i < CachedLeaderBoardData.Data.Count; i++)
            {
                var entry = CachedLeaderBoardData.Data[i];
                entry.WipeData();
            }

            CachedLeaderBoardData = null;
        }
    }
}