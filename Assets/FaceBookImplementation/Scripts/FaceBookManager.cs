using Facebook.Unity;
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
        public static bool isLoggedInToFaceBook => FB.IsLoggedIn;

        /// <summary>
        /// Is the facebook SDK initialized
        /// </summary>
        public static bool isInitialized => FB.IsInitialized;

        /// <summary>
        /// Get the FaceBook User ID for the player
        /// </summary>
        public static string GetUserFaceBookID
        {
            get
            {
                if (!FB.IsInitialized || !FB.IsLoggedIn || AccessToken.CurrentAccessToken == null)
                {
                    return null;
                }

                string id;

                // Return the ID obtained from building player profile
                if (!String.IsNullOrEmpty(userFaceBookID))
                {
                    id = userFaceBookID;
                }
                // Get it from the access token
                else
                {
                    id = AccessToken.CurrentAccessToken.UserId;
                }

                if (id == "me")
                {
                    return null;
                }

                return id;
            }
        }

        public static string GetAccessTokenString
        {
            get
            {
                if (!FB.IsInitialized || !FB.IsLoggedIn || AccessToken.CurrentAccessToken == null)
                {
                    return null;
                }

                return AccessToken.CurrentAccessToken.TokenString;
            }
        }

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

            if (!FB.IsInitialized)
            {
                UnityEngine.Console.Log("Failed to login to facebook as SDK wasn't initialized");
                return;
            }

            if (FB.IsLoggedIn)
            {
                UnityEngine.Console.Log("User is already logged in to facebook");

                await BuildAndRetrievePlayerInformation();
                await GetUserFriends();

                return;
            }

            IsLoggingIn = true;

            var perms = new List<string>() { FaceBookPermissions.GamingProfile, FaceBookPermissions.UserFriends, FaceBookPermissions.Email, FaceBookPermissions.GamingUserPicture };
            FB.LogInWithReadPermissions(perms, AuthCallback);
        }

        public void LogOutUserFromFaceBook()
        {
            if (!FB.IsInitialized)
            {
                UnityEngine.Console.Log("Failed to logout of facebook as SDK wasn't initialized");
                return;
            }

            if (!FB.IsLoggedIn)
            {
                UnityEngine.Console.Log("Can't log out as player isn't logged in to facebook");

                return;
            }

            FB.LogOut();

            ResetLoadedProfiles();

            OnUserLoggedOutFromFaceBook.Invoke();

        }

        /// <summary>
        /// Toggle the user's authentication. Signing on/off depending on the current state
        /// </summary>
        public void ToggleUserAuthentication()
        {
            if (!FB.IsInitialized)
            {
                UnityEngine.Console.Log("FaceBook isn't initialized yet!");
                return;
            }

            if (FB.IsLoggedIn)
            {
                LogOutUserFromFaceBook();
            }
            else
            {
                LogInUserToFaceBook();
            }
        }

        /// <summary>
        /// Creates and caches user facebook profile
        /// </summary>
        /// <param name="ResultHandler">Callback upon retrieval of the data</param>
        public Task<FaceBookUserProfile> BuildAndRetrievePlayerInformation(Action<FaceBookUserProfile> ResultHandler = null)
        {
            if (IsFetchingUserProfile)
            {
                UnityEngine.Console.LogWarning("Failed getting user facebook name as it's already being fetched");
                //  SendCompletionEvent(ResultHandler, null, OnUserFaceBookProfileCreated);
                return faceBookUserProfileFetchTask.Task;
            }

            if (!FB.IsInitialized)
            {
                string msg = "Failed getting user facebook name as the SDK wasn't initialized";
                UnityEngine.Console.LogWarning(msg);
                SendCompletionEvent(ResultHandler, null, OnUserFaceBookProfileCreated);
                return Task.FromException<FaceBookUserProfile>(new Exception(msg));
            }

            if (!FB.IsLoggedIn)
            {
                string msg = "Failed getting user facebook name as the player isn't logged in to facebook";
                UnityEngine.Console.LogWarning(msg);
                SendCompletionEvent(ResultHandler, null, OnUserFaceBookProfileCreated);
                return Task.FromException<FaceBookUserProfile>(new Exception(msg));
            }

            if (!CheckIfHavePermission(FaceBookPermissions.GamingProfile))
            {
                string msg = "No permission to check for user facebook name";
                UnityEngine.Console.LogWarning(msg);
                SendCompletionEvent(ResultHandler, null, OnUserFaceBookProfileCreated);
                return Task.FromException<FaceBookUserProfile>(new Exception(msg));
            }

            if (FaceBookUserProfile != null)
            {
                string msg = "Player facebook profile has already been built. Returning Cached profile";
                UnityEngine.Console.Log(msg);
                SendCompletionEvent(ResultHandler, FaceBookUserProfile, OnUserFaceBookProfileCreated);
                // GetUserFriends((res) => { });
                return Task.FromResult(FaceBookUserProfile);
            }

            IsFetchingUserProfile = true;

            faceBookUserProfileFetchTask = new TaskCompletionSource<FaceBookUserProfile>();

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                FB.API("/me?fields=name,picture", HttpMethod.GET, (graphResult) =>
                {
                    try
                    {
                        //IsFetchingUserProfile = false;

                        if (graphResult.Cancelled || graphResult.Error != null)
                        {
                            UnityEngine.Console.Log("Failed building user's facebook profile");
                            //  SendCompletionEvent(ResultHandler, null, OnUserFaceBookProfileCreated);
                            throw new Exception("Failed building user's facebook profile");
                        }

                        var resultDictionary = graphResult.ResultDictionary;

                        if (resultDictionary == null)
                        {
                            UnityEngine.Console.Log("The returned result dictionary for building user's facebook profile is null");
                            //  SendCompletionEvent(ResultHandler, null, OnUserFaceBookProfileCreated);
                            throw new Exception("The returned result dictionary for building user's facebook profile is null");
                        }

                        object userNameObject, userIDObject;
                        resultDictionary.TryGetValue("name", out userNameObject);

                        if (userNameObject == null)
                        {
                            UnityEngine.Console.Log("Failed building facebook user profile from the returned result dictionary");
                            // SendCompletionEvent(ResultHandler, null, OnUserFaceBookProfileCreated);
                            throw new Exception("Failed building facebook user profile from the returned result dictionary");
                        }

                        resultDictionary.TryGetValue("id", out userIDObject);

                        if (userIDObject == null)
                        {
                            UnityEngine.Console.Log("Failed getting facebook ID from the returned result dictionary");
                            //  SendCompletionEvent(ResultHandler, null, OnUserFaceBookProfileCreated);
                            throw new Exception("Failed getting facebook ID from the returned result dictionary");
                        }

                        string userName = userNameObject as string;
                        string id = userIDObject as string;

                        UnityEngine.Console.Log("UserName " + userName);

                        // Cache UserName
                        GetUserFaceBookName = userName;

                        // Cache ID
                        userFaceBookID = id;

                        // Get User Picture URL
                        string url = "url";
                        object pictureObject, pictureData;
                        resultDictionary.TryGetValue("picture", out pictureObject);

                        if (pictureObject != null)
                        {
                            var pictureDictionary = pictureObject as IDictionary<string, object>;
                            pictureDictionary.TryGetValue("data", out pictureData);

                            if (pictureData != null)
                            {
                                var pictureDataDictionary = pictureData as IDictionary<string, object>;

                                if (pictureDataDictionary.ContainsKey(url))
                                {
                                    UnityEngine.Console.Log("Player Avatar URL set");
                                    UserAvatarURL = pictureDataDictionary[url] as string;
                                }
                            }
                        }

                        // Create user profile
                        FaceBookUserProfile faceBookUserProfile = new FaceBookUserProfile(UserAvatarURL, userName, id);
                        FaceBookUserProfile = faceBookUserProfile;

                        //  userAvatarSprite = FaceBookUserProfile.userAvatar;

                        AddFaceBookUserToDictionary(FaceBookUserProfile);

                        // Give a timeout as well?
                        // DownloadUsersAvatar(FaceBookUserProfile, new CancellationToken());

                        //   SendCompletionEvent(ResultHandler, FaceBookUserProfile, OnUserFaceBookProfileCreated);

                        //   GetUserFriends((res) => { });
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Console.Log($"An exception occrred while trying to send get user profile request to rest API. Exception {e}");
                    }
                    finally
                    {
                        IsFetchingUserProfile = false;
                        SendCompletionEvent(ResultHandler, FaceBookUserProfile, OnUserFaceBookProfileCreated);
                        faceBookUserProfileFetchTask.SetResult(FaceBookUserProfile);
                        //   faceBookUserProfileFetchTask = null;
                    }
                });
            });

            return faceBookUserProfileFetchTask.Task;
        }

        /// <summary>
        /// Retrieve user friends who have atleast signed in to game once with FaceBook
        /// </summary>
        /// <param name="ResultHandler">A list of dictionaries containing each friends data</param>
        public Task<List<FaceBookUserProfile>> GetUserFriends(Action<List<FaceBookUserProfile>> ResultHandler = null)
        {
            if (IsFetchingFriends)
            {
                UnityEngine.Console.Log("Friends fetching already in progress");
                return taskCompletionSource.Task;
            }

            if (!FB.IsInitialized)
            {
                UnityEngine.Console.LogWarning("Failed getting user facebook name as the SDK wasn't initialized");
                return Task.FromResult<List<FaceBookUserProfile>>(null);
            }

            if (!FB.IsLoggedIn)
            {
                UnityEngine.Console.LogWarning("Failed getting user facebook name as the player isn't logged in to facebook");
                return Task.FromResult<List<FaceBookUserProfile>>(null);
            }

            if (!CheckIfHavePermission(FaceBookPermissions.UserFriends))
            {
                UnityEngine.Console.LogWarning("User doesn't have permission to get friends");
                return Task.FromResult<List<FaceBookUserProfile>>(null);
            }

            // If Cached data is available use it
            if (userCachedFriendsData != null && userCachedFriendsData.Count > 0)
            {
                return Task.FromResult<List<FaceBookUserProfile>>(userCachedFriendsData);
            }

            IsFetchingFriends = true;

            taskCompletionSource = new TaskCompletionSource<List<FaceBookUserProfile>>();

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                //&debug=all
                FB.API("/me/friends?fields=name,id,picture", HttpMethod.GET, (graphResult) =>
                {
                    try
                    {
                        if (graphResult.Cancelled || graphResult.Error != null)
                        {
                            UnityEngine.Console.Log($"Failed getting FaceBook friends. Error {graphResult.Error} ");

                            UnityEngine.Console.Log($"Error Response {graphResult.RawResult}");

                            throw new System.Exception(graphResult.Error);
                        }

                        UnityEngine.Console.Log("User Friends " + graphResult.ResultDictionary.Count);

                        List<object> data = graphResult.ResultDictionary["data"] as List<object>;

                        if (userCachedFriendsData == null)
                        {
                            userCachedFriendsData = new List<FaceBookUserProfile>();
                        }
                        else
                        {
                            userCachedFriendsData.Clear();
                        }

                        List<FaceBookUserProfile> faceBookUserProfiles = CreateFaceBookProfilesFromSnapShotsData(data);

                        // Cache User Friends
                        userCachedFriendsData = faceBookUserProfiles;

                        // IsFetchingFriends = false;

                        // Download player avatar images
                        //LoadAllUsersAvatars(faceBookUserProfiles);
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Console.Log($"Exception occurred on facebook friend get request to rest API. Exception: {e}");
                    }
                    finally
                    {
                        IsFetchingFriends = false;

                        SendCompletionEvent(ResultHandler, userCachedFriendsData, OnFaceBookFriendsFetched);

                        taskCompletionSource.SetResult(userCachedFriendsData);

                        //  taskCompletionSource = null;
                    }
                });
            });

            return taskCompletionSource.Task;
        }

        // Create facebook data profiles based on the data retrieved from facebook api
        private List<FaceBookUserProfile> CreateFaceBookProfilesFromSnapShotsData(List<object> data)
        {
            List<FaceBookUserProfile> faceBookUserProfiles = new List<FaceBookUserProfile>();

            for (int i = 0; i < data.Count; i++)
            {
                try
                {
                    IDictionary<string, object> entry = data[i] as IDictionary<string, object>;
                    string id = entry["id"] as string;
                    string name = entry["name"] as string;

                    var v = entry["picture"] as IDictionary<string, object>;
                    var s = v["data"] as IDictionary<string, object>;

                    string avatarURL = s["url"] as string;

                    FaceBookUserProfile faceBookUserProfile = new FaceBookUserProfile(avatarURL, name, id);
                    faceBookUserProfiles.Add(faceBookUserProfile);

                    AddFaceBookUserToDictionary(faceBookUserProfile);

                    UnityEngine.Console.Log("Profile Created " + faceBookUserProfile.userName);
                }
                catch (Exception e)
                {
                    UnityEngine.Console.Log($"Failed to create a facebook profile from data. Exception {e}");
                }
            }

            return faceBookUserProfiles;
        }

        // Whether the user has a specific permisson
        private bool CheckIfHavePermission(string permissionToCheck)
        {
            if (!FB.IsInitialized)
            {
                UnityEngine.Console.LogWarning("Can't check for permission as facebook sdk isn't initialized");
                return false;
            }

            if (!FB.IsLoggedIn)
            {
                UnityEngine.Console.LogWarning("Can't check for permissionas facebook sdk isn't initialized");
                return false;
            }

            AccessToken accessToken = AccessToken.CurrentAccessToken;

            if (accessToken == null)
            {
                UnityEngine.Console.Log($"Failed checking for {permissionToCheck} as the player access token is null");
                return false;
            }

            var permissions = accessToken.Permissions;

            foreach (var perm in permissions)
            {
                if (perm == permissionToCheck)
                    return true;
            }

            //// Doesn't have the permission. Maybe ask for it?
            //FB.Mobile.RefreshCurrentAccessToken((result) =>
            //{
            //    if (result.Cancelled || result.Error != null)
            //    {
            //        UnityEngine.Console.Log($"Error resfreshing current access token. Error {result.Error}");
            //        return;
            //    }

            //    UnityEngine.Console.Log("Refreshed current access token " + result.RawResult);
            //});

            return false;
        }

        /// <summary>
        /// Sets the image's sprite to that of the user's avatar.
        /// The process can be async
        /// </summary>
        /// <param name="id">ID of the user</param>
        public void SetImageAvatarForTheUser(string id, Image image)
        {
            UnityEngine.Console.Log($"Requsted Image For {id}");

            if (!isLoggedInToFaceBook)
                return;

            if (image == null || String.IsNullOrEmpty(id))
                return;

            FaceBookUserProfile facebookUserProfile = null;

            FaceBookUsersDictionary.TryGetValue(id, out facebookUserProfile);

            if (facebookUserProfile != null && facebookUserProfile.userAvatarTexture != null)
            {
                image.sprite = facebookUserProfile.userAvatarSprite;
            }
            else
            {
                List<Image> pendingListeners;

                if (!pendingListenersForAvatarDownloading.ContainsKey(id))
                {
                    pendingListeners = new List<Image>() { image };
                    pendingListenersForAvatarDownloading.Add(id, pendingListeners);
                }
                else
                {
                    pendingListeners = pendingListenersForAvatarDownloading[id];

                    // Maybe remove the check but!!!!!!!!
                    if (!pendingListeners.Contains(image))
                    {
                        pendingListeners.Add(image);
                    }
                }

                // Request image download
                if (facebookUserProfile != null)
                {
                    DownloadUsersAvatar(facebookUserProfile);
                }
            }
        }

        /// <summary>
        /// Sets the image's sprite to that of the user's avatar.
        /// The process can be async
        /// </summary>
        /// <param name="id">ID of the user</param>
        public void SetImageAvatarForTheUser(string id, RawImage rawImage)
        {
            // UnityEngine.Console.Log($"Requsted Image For {id}");

            if (!isLoggedInToFaceBook)
                return;

            if (rawImage == null || String.IsNullOrEmpty(id))
                return;

            FaceBookUserProfile facebookUserProfile = null;

            FaceBookUsersDictionary.TryGetValue(id, out facebookUserProfile);

            if (facebookUserProfile != null && facebookUserProfile.userAvatarTexture != null)
            {
                rawImage.texture = facebookUserProfile.userAvatarTexture;
            }
            else
            {
                List<RawImage> pendingListeners;

                if (!pendingListenersForRawAvatarDownloading.ContainsKey(id))
                {
                    pendingListeners = new List<RawImage>() { rawImage };
                    pendingListenersForRawAvatarDownloading.Add(id, pendingListeners);
                }
                else
                {
                    pendingListeners = pendingListenersForRawAvatarDownloading[id];

                    // Maybe remove the check but!!!!!!!!
                    if (!pendingListeners.Contains(rawImage))
                    {
                        pendingListeners.Add(rawImage);
                    }
                }

                // Request image download
                if (facebookUserProfile != null)
                {
                    DownloadUsersAvatar(facebookUserProfile);
                }
            }
        }

        /// <summary>
        /// Shares time playstore link to the user's timeline
        /// </summary>
        public void ShareGameToTimeLine()
        {
            FB.ShareLink(new Uri("https://play.google.com/store/apps/details?id=com.gamexis.minicar.rush.racing.googleplay"));
        }

        public void InviteFriendsToGame()
        {
            FB.AppRequest("Hey checkout this game!", null, new List<string> { "app_non_users" }, null, null, null, "Worlds Best Game", (res) =>
            {
                if (res.Cancelled || res.Error != null)
                {
                    UnityEngine.Console.Log($"Failed to send app invites. Error {res.Error}");
                    return;
                }

                UnityEngine.Console.Log("App invite sent successfully");
            });

            //       GetSocialUi
            //.CreateInvitesView()
            //.SetLinkParams(linkParams)
            //.Show();

            //if (!GetSocial.IsInitialized)
            //{
            //    // use GetSocial
            //    UnityEngine.Console.Log("GetSocial SDK not initialized");
            //    return;
            //}

            //bool wasShown = InvitesViewBuilder.Create().Show();
            //UnityEngine.Console.Log("Smart Invites view was shown: " + wasShown);
        }

        private void CheckForOutdatedAccessToken()
        {
            if (!FB.IsInitialized)
            {
                UnityEngine.Console.Log("Failed to check for access token expiration as SDK wasn't initialized");
                return;
            }

            AccessToken currentToken = AccessToken.CurrentAccessToken;
        }

        private async void AuthCallback(ILoginResult result)
        {
            IsLoggingIn = false;

            if (FB.IsLoggedIn)
            {
                // AccessToken class will have session details
                var aToken = AccessToken.CurrentAccessToken;
                // Print current access token's User ID
                UnityEngine.Console.Log($"User FaceBook ID {aToken.UserId}");
                // Print current access token's granted permissions
                foreach (string perm in aToken.Permissions)
                {
                    UnityEngine.Console.Log(perm);
                }

                OnUserLoggedInToFaceBook.Invoke();

                await BuildAndRetrievePlayerInformation();
                await GetUserFriends();
            }
            else
            {
                UnityEngine.Console.LogWarning("User cancelled login");

                OnUserFailedLogginInToFaceBook.Invoke();
            }
        }

        public void InitializeFaceBookSDK()
        {
            if (!FB.IsInitialized)
            {
                // Initialize the Facebook SDK
                FB.Init(InitCallback, OnHideUnity);
            }
            else
            {
                IsSDKInitializationProcessCompleted = true;

                // Already initialized, signal an app activation App Event
                FB.ActivateApp();
                OnFaceBookInitialized.Invoke();

                //LogInUserToFaceBook();
            }
        }

        private async void InitCallback()
        {
            IsSDKInitializationProcessCompleted = true;

            if (FB.IsInitialized)
            {
                // Signal an app activation App Event
                FB.ActivateApp();
                // Continue with Facebook SDK
                // ...

                OnFaceBookInitialized.Invoke();

                // Is Already logged in on facebook sdk initialization
                if (FB.IsLoggedIn)
                {
                    OnUserLoggedInToFaceBook.Invoke();

                    await BuildAndRetrievePlayerInformation();
                    await GetUserFriends();
                }

                // LogInUserToFaceBook();
            }
            else
            {
                UnityEngine.Console.LogWarning("Failed to Initialize the Facebook SDK");
                OnFaceBookFailedInitialized.Invoke();
            }
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