
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace TheKnights.FaceBook
{
    public partial class FaceBookPermissions
    {
        public const string GamingProfile = "gaming_profile";
        public const string UserFriends = "user_friends";

        public const string Email = "email";
        public const string GamingUserPicture = "gaming_user_picture ";
    }

    [CreateAssetMenu(fileName = "FaceBookManager", menuName = "ScriptableObjects/FaceBook/FaceBookManager")]
    public class FaceBookManager : ScriptableObject
    {
        public class UnityFBFriendsFetchedEvent : UnityEvent<List<FaceBookUserProfile>>
        { }

        public class UntiyFaceBookProfileEvent : UnityEvent<FaceBookUserProfile>
        { }

        /// <summary>
        /// Event invoked when the facebook friends has been fetched
        /// </summary>
        [HideInInspector] public static UnityFBFriendsFetchedEvent OnFaceBookFriendsFetched { get; private set; } = new UnityFBFriendsFetchedEvent();

        /// <summary>
        /// Event invoked upon facebook sdk initialization
        /// </summary>
        [HideInInspector] public static UnityEvent OnFaceBookInitialized { get; private set; } = new UnityEvent();

        /// <summary>
        /// Event invoked upon facebook user profile retreival
        /// </summary>
        [HideInInspector] public static UntiyFaceBookProfileEvent OnUserFaceBookProfileCreated { get; private set; } = new UntiyFaceBookProfileEvent();

        /// <summary>
        /// Event invoked upon facebook sdk failed initialization
        /// </summary>
        [HideInInspector] public static UnityEvent OnFaceBookFailedInitialized { get; private set; } = new UnityEvent();

        /// <summary>
        /// Event invoked upon facebook logged in
        /// </summary>
        [HideInInspector] public static UnityEvent OnUserLoggedInToFaceBook { get; private set; } = new UnityEvent();

        /// <summary>
        /// Event invoked upon facebook logged out
        /// </summary>
        [HideInInspector] public static UnityEvent OnUserLoggedOutFromFaceBook { get; private set; } = new UnityEvent();

        /// <summary>
        /// Event invoked upon facebook failed to log in
        /// </summary>
        [HideInInspector] public static UnityEvent OnUserFailedLogginInToFaceBook { get; private set; } = new UnityEvent();

        /// <summary>
        /// Event invoked when the friends avatars have been downloaded
        /// </summary>
        [HideInInspector] public static UnityEvent OnUsersAvatarsDownloaded { get; private set; } = new UnityEvent();

        /// <summary>
        /// Event invoked when the user avatar have been downloaded
        /// </summary>
        [HideInInspector] public static UnityEvent OnUserAvatarDownloaded { get; private set; } = new UnityEvent();

        /// <summary>
        /// Is the player currently logging in to facebook
        /// </summary>
        public static bool IsLoggingIn { get; private set; }

        /// <summary>
        /// Is the SDK initialzation process done irrespective of success
        /// </summary>
        public static bool IsSDKInitializationProcessCompleted { get; private set; }

        /// <summary>
        /// Is player friends currently being fetched
        /// </summary>
        public static bool IsFetchingFriends { get; private set; }

        /// <summary>
        /// Is player profile being fetched
        /// </summary>
        public static bool IsFetchingUserProfile { get; private set; }

        /// <summary>
        /// Is the user currently logged in to facebook
        /// </summary>
       //  public static bool isLoggedInToFaceBook => FB.IsLoggedIn;

        /// <summary>
        /// Is the facebook SDK initialized
        /// </summary>
      //  public static bool isInitialized => FB.IsInitialized;

        /// <summary>
        /// Get the FaceBook User ID for the player
        /// </summary>
        

        /// <summary>
        /// Get the FaceBook User Name for the player
        /// </summary>
        public static string GetUserFaceBookName { get; private set; } = "You";

        /// <summary>
        /// Get the FaceBook Avatar URL
        /// </summary>
        public static string UserAvatarURL { get; private set; } = default;

        /// <summary>
        /// User profile picture sprite downloaded after fetching user profile
        /// </summary>
        public static Sprite userAvatarSprite { get; private set; } = default;

        /// <summary>
        /// The facebook profile of the user
        /// </summary>
        public static FaceBookUserProfile FaceBookUserProfile { get; private set; }

        /// <summary>
        /// Get the facebook users dictionary
        /// </summary>
        public static readonly Dictionary<string, FaceBookUserProfile> FaceBookUsersDictionary = new Dictionary<string, FaceBookUserProfile>();

        // The cached user friends profiles
        public static List<FaceBookUserProfile> userCachedFriendsData;

        // Dictionary containing string to image components with their sprites pending to be assigned upon downloading completion
        private readonly Dictionary<string, List<Image>> pendingListenersForAvatarDownloading = new Dictionary<string, List<Image>>();

        // Dictionary containing string to image components with their sprites pending to be assigned upon downloading completion
        private readonly Dictionary<string, List<RawImage>> pendingListenersForRawAvatarDownloading = new Dictionary<string, List<RawImage>>();

        private readonly Dictionary<string, bool> inProgressAvatarDownloadingCollection = new Dictionary<string, bool>();

        // Assigned upon building player profile and is preffered over the ID obtained from authentication token
        private static string userFaceBookID;

        #region CurrentlyActiveTasks

        private TaskCompletionSource<FaceBookUserProfile> faceBookUserProfileFetchTask;
        private TaskCompletionSource<List<FaceBookUserProfile>> taskCompletionSource;

        #endregion CurrentlyActiveTasks

        public async void LogInUserToFaceBook()
        {
            if (IsLoggingIn)
            {
                UnityEngine.Console.Log("Player's already logging in to facebook");
                return;
            }

           

            IsLoggingIn = true;

            var perms = new List<string>() { FaceBookPermissions.GamingProfile, FaceBookPermissions.UserFriends, FaceBookPermissions.Email, FaceBookPermissions.GamingUserPicture };
           
        }

        public void LogOutUserFromFaceBook()
        {
            
           

            ResetLoadedProfiles();

            OnUserLoggedOutFromFaceBook.Invoke();

        }

        /// <summary>
        /// Toggle the user's authentication. Signing on/off depending on the current state
        /// </summary>
        public void ToggleUserAuthentication()
        {
            
        }

        /// <summary>
        /// Creates and caches user facebook profile
        /// </summary>
        /// <param name="ResultHandler">Callback upon retrieval of the data</param>
       

        /// <summary>
        /// Retrieve user friends who have atleast signed in to game once with FaceBook
        /// </summary>
        /// <param name="ResultHandler">A list of dictionaries containing each friends data</param>
        

        // Create facebook data profiles based on the data retrieved from facebook api
       
        // Whether the user has a specific permisson
       

        /// <summary>
        /// Sets the image's sprite to that of the user's avatar.
        /// The process can be async
        /// </summary>
        /// <param name="id">ID of the user</param>
       

        /// <summary>
        /// Sets the image's sprite to that of the user's avatar.
        /// The process can be async
        /// </summary>
        /// <param name="id">ID of the user</param>
        

        /// <summary>
        /// Shares time playstore link to the user's timeline
        /// </summary>
       
       

        private void CheckForOutdatedAccessToken()
        {
            
        }

       

        public void InitializeFaceBookSDK()
        {
            //if (!FB.IsInitialized)
            //{
            //    // Initialize the Facebook SDK
            //    FB.Init(InitCallback, OnHideUnity);
            //}
            //else
            //{
            //    IsSDKInitializationProcessCompleted = true;

            //    // Already initialized, signal an app activation App Event
            //    FB.ActivateApp();
            //    OnFaceBookInitialized.Invoke();

            //    //LogInUserToFaceBook();
            //}
        }

        private async void InitCallback()
        {
            IsSDKInitializationProcessCompleted = true;

            //if (FB.IsInitialized)
            //{
            //    // Signal an app activation App Event
            //    FB.ActivateApp();
            //    // Continue with Facebook SDK
            //    // ...

            //    OnFaceBookInitialized.Invoke();

            //    // Is Already logged in on facebook sdk initialization
            //    if (FB.IsLoggedIn)
            //    {
            //        OnUserLoggedInToFaceBook.Invoke();

            //        await BuildAndRetrievePlayerInformation();
            //        await GetUserFriends();
            //    }

            //    // LogInUserToFaceBook();
            //}
            //else
            //{
            //    UnityEngine.Console.LogWarning("Failed to Initialize the Facebook SDK");
            //    OnFaceBookFailedInitialized.Invoke();
            //}
        }

        private void OnHideUnity(bool isGameShown)
        {
            if (!isGameShown)
            {
                // Pause the game - we will need to hide
                Time.timeScale = 0;
            }
            else
            {
                // Resume the game - we're getting focus again
                Time.timeScale = 1;
            }
        }

        private void SendCompletionEvent<T>(Action<T> Result, T data, UnityEvent<T> eventToInvoke)
        {
            Result?.Invoke(data);

            eventToInvoke.Invoke(data);
        }

        //private async void LoadAllUsersAvatars(List<FaceBookUserProfile> faceBookUserProfiles)
        //{
        //    List<Task> avatarDownloadingTasks = new List<Task>();

        //    using (var timeoutCancellationTokenSource = new CancellationTokenSource())
        //    {
        //        for (int i = 0; i < faceBookUserProfiles.Count; i++)
        //        {
        //            avatarDownloadingTasks.Add(DownloadUsersAvatar(faceBookUserProfiles[i], timeoutCancellationTokenSource.Token));
        //        }

        //        Task waitForAllTasks = Task.WhenAll(avatarDownloadingTasks);

        //        // One minute timeout
        //        var completedTask = await Task.WhenAny(waitForAllTasks, Task.Delay(60000, timeoutCancellationTokenSource.Token));

        //        timeoutCancellationTokenSource.Cancel();

        //        // Completed successfully
        //        if (completedTask == waitForAllTasks)
        //        {
        //            UnityEngine.Console.Log("All user avatars has been downloaded");

        //            OnUsersAvatarsDownloaded.Invoke();
        //        }
        //        else
        //        {
        //            UnityEngine.Console.Log("TimedOut while downloading facebook users avatars");
        //        }
        //    }
        //}

        private Task DownloadUsersAvatar(FaceBookUserProfile faceBookUserProfile)
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            try
            {

                // If user avatar has already been downloaded, mark proccess as completed
                if (faceBookUserProfile.userAvatarTexture != null)
                {
                    taskCompletionSource.SetResult(true);
                }
                else
                {
                    // To avoid multiple webrequests for the same avatar
                    bool isAlreadyInProgress = false;
                    inProgressAvatarDownloadingCollection.TryGetValue(faceBookUserProfile.userID, out isAlreadyInProgress);

                    if (isAlreadyInProgress)
                    {
                        UnityEngine.Console.Log("Multiple web request for the same facebook user avatar detected. Ignoring request");
                        taskCompletionSource.SetResult(true);
                    }
                    else
                    {
                        if (inProgressAvatarDownloadingCollection.ContainsKey(faceBookUserProfile.userID))
                        {
                            inProgressAvatarDownloadingCollection[faceBookUserProfile.userID] = true;
                        }
                        else
                        {
                            inProgressAvatarDownloadingCollection.Add(faceBookUserProfile.userID, true);
                        }

                        // Start downloading the image
                        UnityEngine.Console.Log($"Starting downloading avatar of URL {faceBookUserProfile.avatarURL}");

                        UnityWebRequest www = UnityWebRequestTexture.GetTexture(faceBookUserProfile.avatarURL);

                        var handle = www.SendWebRequest();

                        handle.completed += (asyncOperation) =>
                        {
                            Exception e = null;

                            try
                            {
                                inProgressAvatarDownloadingCollection[faceBookUserProfile.userID] = false;

                                // Also remove Pending Listeners on faliure???
                                if (handle.webRequest.error != null)
                                {
                                    RemovePendingAvatarListeners(faceBookUserProfile);

                                    UnityEngine.Console.Log("An error occurred while downloading facebook user avatar");
                                    return;
                                }

                                Texture2D myTexture = DownloadHandlerTexture.GetContent(www);

                                //Sprite avatarSprite = Sprite.Create(myTexture, new Rect(Vector3.zero, new Vector2(myTexture.width, myTexture.height)), Vector2.zero);

                                //UnityEngine.Console.Log($"Sprite Width Height {myTexture.width} x {myTexture.height} ");

                                faceBookUserProfile.userAvatarTexture = myTexture;

                                UnityEngine.Console.Log($"Avatar Downloaded for url { faceBookUserProfile.avatarURL}");

                                // Local User
                                if (faceBookUserProfile == FaceBookUserProfile)
                                {
                                    UnityEngine.Console.Log("Player Avatar Has Been Downloaded");
                                    OnUserAvatarDownloaded.Invoke();

                                    UnityEngine.Console.Log("Downloaded Facebook Format texture " + myTexture.format);
                                    UnityEngine.Console.Log("Downloaded Facebook Graphics texture " + myTexture.graphicsFormat);
                                }

                                // Check for pending listeners
                                AssignAvatarsToPendingListeners(faceBookUserProfile, myTexture);
                            }
                            catch(Exception ex)
                            {
                                e = ex;
                            }
                            finally
                            {
                                taskCompletionSource.SetResult(e == null);
                            }
                        };
                    }
                }
            }
            catch(Exception e)
            {
                taskCompletionSource.SetException(e);
            }
          
            return taskCompletionSource.Task;
        }

        private void AssignAvatarsToPendingListeners(FaceBookUserProfile faceBookUserProfile, Texture2D avatarTexture)
        {
            // Images
            List<Image> pendingListeners;
            pendingListenersForAvatarDownloading.TryGetValue(faceBookUserProfile.userID, out pendingListeners);

            if (pendingListeners != null && pendingListeners.Count > 0)
            {
                for (int i = 0; i < pendingListeners.Count; i++)
                {
                    Image image = pendingListeners[i];

                    if (image != null)
                    {
                        image.sprite = faceBookUserProfile.userAvatarSprite;
                    }
                }

                pendingListeners.Clear();

                pendingListenersForAvatarDownloading.Remove(faceBookUserProfile.userID);
            }

            // Raw Images
            List<RawImage> pendingRawListeners;
            pendingListenersForRawAvatarDownloading.TryGetValue(faceBookUserProfile.userID, out pendingRawListeners);

            if (pendingRawListeners != null && pendingRawListeners.Count > 0)
            {
                for (int i = 0; i < pendingRawListeners.Count; i++)
                {
                    RawImage rawImage = pendingRawListeners[i];

                    if (rawImage != null)
                        rawImage.texture = avatarTexture;
                }

                pendingRawListeners.Clear();

                pendingListenersForRawAvatarDownloading.Remove(faceBookUserProfile.userID);
            }
        }

        private void RemovePendingAvatarListeners(FaceBookUserProfile faceBookUserProfile)
        {
            List<Image> pendingListeners;
            pendingListenersForAvatarDownloading.TryGetValue(faceBookUserProfile.userID, out pendingListeners);

            if (pendingListeners != null && pendingListeners.Count > 0)
            {
                pendingListeners.Clear();
                pendingListenersForAvatarDownloading.Remove(faceBookUserProfile.userID);
            }
        }

        private void AddFaceBookUserToDictionary(FaceBookUserProfile faceBookUserProfile)
        {
            string id = faceBookUserProfile.userID;

            if (FaceBookUsersDictionary.ContainsKey(id))
            {
                return;
            }

            if (id == null)
                return;

            FaceBookUsersDictionary.Add(id, faceBookUserProfile);
        }

        private void ResetLoadedProfiles()
        {
            FaceBookUserProfile = null;
            userCachedFriendsData?.Clear();

            taskCompletionSource.TrySetCanceled();
            faceBookUserProfileFetchTask?.TrySetCanceled();
        }
    }
}