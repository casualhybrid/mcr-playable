using System;
using System.Collections.Generic;
using System.Threading.Tasks;
//using TheKnights.FaceBook;
using UnityEngine;
using UnityEngine.Events;

namespace TheKnights.PlayServicesSystem
{
    [CreateAssetMenu(fileName = "PlayServicesController", menuName = "ScriptableObjects/PlayGames/PlayServicesController", order = 1)]
    public class PlayServicesController : ScriptableObject
    {
        [Header("Global Configuration")]
        [Tooltip("When the play services are initialized, sign in the player aswell")]
        [SerializeField] private bool autoSignInUponInitializing = true;

        [Tooltip("Enable Debug Logs for the play services internal API")]
        [SerializeField] private bool isDebugPlayServices = true;

        [Tooltip("Is the first sign in attempt silent and without showing any consent to user?")]
        [SerializeField] private bool trySilentLoginFirstTime = true;

        /// <summary>
        /// Indicates whether the sdk has finished initializing and atleast one sign in attempt has been made irrespective of result
        /// </summary>
        public static bool HasSDKInitializedAlongWithSignInAttempt;

        /// <summary>
        /// Send when the user has successfully logged in to play services
        /// </summary>
        [HideInInspector] public static UnityEvent OnUserLoggedIn = new UnityEvent();

        /// <summary>
        /// Send when the user has failed logging in to play services
        /// </summary>
        [HideInInspector] public static UnityEvent OnUserLoggedInFailed = new UnityEvent();

        /// <summary>
        /// Is the player currently being signed in to play services
        /// </summary>
        public static bool isSigningIn { get; private set; }

        /// <summary>
        /// Is the user currently logged in to play services
        /// </summary>
        //public static bool isLoggedIn 
        //{
            
        //    PlayGamesPlatform.Instance.IsAuthenticated();

        // }

        /// <summary>
        /// Is the user's playID valid
        /// </summary>
        public static bool isUserIDValid => userID != default;

        public static string userID { get; set; } = string.Empty;
        public static string userName { get; set; } = default;

        public static event Action<Dictionary<string, Texture2D>> UserYourImages;

        //private PlayGamesClientConfiguration config;

        // Need to reset values on editor as event the non seialized private values presists after exiting play mode
        private void OnEnable()
        {
            if (userID == string.Empty)
            {
                userID =SystemInfo.deviceUniqueIdentifier;
             //   UnityEngine.Console.Log($"Device Unique Identifier is {userID}");
            }
            isSigningIn = false;
        }

        /// <summary>
        /// Initializes the play services and attempts to sign in the player
        /// </summary>
        public void InitializerPlayServices()
        {
               // Create client configuration
            //   config = new
            //   PlayGamesClientConfiguration.Builder()
            //    .EnableSavedGames()
            //   .Build();

            //// Enable debugging output (recommended)
            //PlayGamesPlatform.DebugLogEnabled = isDebugPlayServices;

            //// Initialize and activate the platform
            //PlayGamesPlatform.InitializeInstance(config);
            //PlayGamesPlatform.Activate();

            //if (autoSignInUponInitializing)
            //{
            //    SignIn(trySilentLoginFirstTime);
            //}
        }

        #region SignIn_SignOut

        /// <summary>
        /// Attempts to sign in to play services
        /// </summary>
        /// <param name="isSignInSilent">Should the sign in be silent or with consent</param>
        public void SignIn(bool isSignInSilent = true)
        {
            if (isSigningIn)
            {
                UnityEngine.Console.Log("Already trying to sign In");
                return;
            }

            //if (!isLoggedIn)
            //{
            //    isSigningIn = true;
            //    PlayGamesPlatform.Instance.Authenticate(SignInCallback, isSignInSilent);
            //}
            //else
            //{
            //    UnityEngine.Console.Log("User is already signed in");
            //}
        }

        /// <summary>
        /// Attemps to sign out the user from play services
        /// </summary>
        public void SignOut()
        {
            //if (isLoggedIn)
            //{
            //    PlayGamesPlatform.Instance.SignOut();
            //}
            //else
            //{
            //    UnityEngine.Console.Log("User is already signed out");
            //}
        }

        private void SignInCallback(bool success)
        {
            HasSDKInitializedAlongWithSignInAttempt = true;

            isSigningIn = false;

            //if (success)
            //{
            //    userID = PlayGamesPlatform.Instance.GetUserId();
            //    userName = PlayGamesPlatform.Instance.GetUserDisplayName();
            //    OnUserLoggedIn.Invoke();
            //}
            //else
            //{
            //    OnUserLoggedInFailed.Invoke();
            //}
        }

        #endregion SignIn_SignOut

        // Clean Memory?
        public void DownloadAllImagesFromGivenUserIds(string[] ids)
        {
        //    PlayGamesClientFactory.GetPlatformPlayGamesClient(config).LoadUsers(ids, async (data) =>
        //    {
        //        if (data.Length <= 0)
        //        {
        //            UnityEngine.Console.Log("Image Count was 0");
        //            return;
        //        }

        //        //// Cancel the previous image downloading task
        //        //if (theCancellationTokenSource != null)
        //        //    theCancellationTokenSource.Cancel();

        //        //// Create new token & it's token source
        //        //theCancellationTokenSource = new CancellationTokenSource();
        //        //theCancellationToken = theCancellationTokenSource.Token;

        //        Task T = Task.Run(async () =>
        //        {
        //            foreach (var V in data)
        //            {
        //                Task ImgDownload = Task.Run(async () =>
        //                {
        //                    while (V.image == null) await Task.Yield();
        //                }/*, theCancellationToken*/);
        //                try
        //                {
        //                    await ImgDownload;
        //                }
        //                catch (OperationCanceledException e)
        //                {
        //                    UnityEngine.Console.LogWarning("Canceling Previous Image Downloading " + e);
        //                    break;
        //                }
        //            }
        //        }/*, theCancellationToken*/);

        //        Dictionary<string, Texture2D> idToTexturedictionary = new Dictionary<string, Texture2D>();

        //        try
        //        {
        //            if (await Task.WhenAny(T, Task.Delay(30000)) == T)
        //            {
        //                // Task completed within timeout
        //                UnityEngine.Console.Log("Images Downloaded Successfully");


        //                //  Texture2D[] texture2Ds = new Texture2D[data.Length];

        //                for (int i = 0; i < data.Length; i++)
        //                {
        //                    //   texture2Ds[i] = data[i].image;
        //                    idToTexturedictionary.Add(data[i].id, data[i].image);
        //                }

        //                //    UpdateAndDispatchImagesDownloadedSignal(Status.Succeeded);
        //            }
        //            else
        //            {
        //                // Timeout logic
        //                UnityEngine.Console.Log("Loading Images TimedOut");

        //                //if (theCancellationTokenSource != null)
        //                //    theCancellationTokenSource.Cancel();

        //                //   UpdateAndDispatchImagesDownloadedSignal(Status.Failed);

        //                return;
        //            }
        //        }
        //        catch (OperationCanceledException e)
        //        {
        //            UnityEngine.Console.LogWarning("Canceling Previous Image Downloading " + e);
        //            return;
        //        }

        //        // Download has finished
        //        try
        //        {
        //            UnityEngine.Console.Log("Sending Use Your Images Event");
        //            UserYourImages(idToTexturedictionary);
        //        }
        //        catch
        //        {
        //            UnityEngine.Console.LogWarning("No subscriber to downlolaed images");
        //        }
        //    });
        }
    }
}
