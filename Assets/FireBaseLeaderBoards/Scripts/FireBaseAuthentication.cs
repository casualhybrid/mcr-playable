using Facebook.Unity;
using Firebase;
using Firebase.Auth;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using TheKnights.FaceBook;
using TheKnights.PlayServicesSystem;
using UnityEngine;

public static class ProviderIDs
{
    public const string GooglePlay = "playgames.google.com";
    public const string FaceBook = "facebook.com";
}

public class FireBaseAuthentication : Singleton<FireBaseAuthentication>
{
    #region Public Properties

    /// <summary>
    /// The authentication token for the currently logged in firebase User
    /// </summary>
    public static Task<string> PlayerAuthenticationToken
    {
        get
        {
            //if (!String.IsNullOrEmpty(playerAuthenticationToken))
            //{
            //    return Task.FromResult<string>(playerAuthenticationToken);
            //}
            //else
            //{
                return RefreshUserAuthenticationToken();
           // }
        }
    }

    /// <summary>
    /// The unique authentication fireBase ID for the currently logged in user
    /// </summary>
    public static string FireBaseUserID => firebaseAuth.CurrentUser != null ? firebaseAuth.CurrentUser.UserId : null;

    /// <summary>
    /// Is the user signed in to firebase non-anonymously?
    /// </summary>
    public static bool IsSignedInUsingSocialNetwork => firebaseAuth.CurrentUser != null && !firebaseAuth.CurrentUser.IsAnonymous;

    /// <summary>
    /// Is the user currently logged in to firebase
    /// </summary>
    public static bool IsLoggedInToFireBase => firebaseAuth.CurrentUser != null;

    /// <summary>
    /// Is user's firebase account linked to a google play account?
    /// </summary>
    public static bool IsSignedInUsingGoogle => firebaseAuth.CurrentUser != null && firebaseAuth.CurrentUser.ProviderId == "Play Games";

    /// <summary>
    /// Is user's firebase account linked to a facebook account?
    /// </summary>
    public static bool IsSignedInUsingFaceBook => firebaseAuth.CurrentUser != null && firebaseAuth.CurrentUser.ProviderId == "Facebook";

    /// <summary>
    /// Is the user currently logged in anonymously to facebook
    /// </summary>
    public static bool IsAnonymouslyLoggedIn => firebaseAuth.CurrentUser != null && firebaseAuth.CurrentUser.IsAnonymous;

    #endregion Public Properties

    #region PrivateFields

    [SerializeField] private FaceBookManager faceBookManager;

    private static FirebaseAuth firebaseAuth
    {
        get
        {
            if(_fireBaseAuth == null)
            {
                _fireBaseAuth = FirebaseAuth.DefaultInstance;
            }

            return _fireBaseAuth;
        }
    }

    private static FirebaseAuth _fireBaseAuth;

    private static string playerAuthenticationToken;

    private static bool isAcquringAuthenticationToken = false;

    #endregion PrivateFields

    #region Events

    public static event Action OnUserAuthenticatedToFireBase;

    public static event Action _FireBaseisready;

    public static event Action _FireBaseFailedToInitialize;

    public static event Action<string> OnUserLinkedAnAccountToFireBase;

    public static event Action<string> OnUserSignedInToFireBaseWithSocialAccount;

    public static event Action OnUserAnonymouslySignedIn;
    #endregion Events

    protected override void Awake()
    {
        base.Awake();
        FireBaseInitializer.OnFireBaseInitialized += InitializeAuthentication;
    }

    public void InitializeAuthentication()
    {
        StartCoroutine(WaitForSocialNetworksAndAttemptSignIn());
    }

    private IEnumerator WaitForSocialNetworksAndAttemptSignIn()
    {
       // UnityEngine.Console.Log("Waiting For Social SDKs initiazation Process to complete");
        //   yield return new WaitUntil(() => FaceBookManager.IsSDKInitializationProcessCompleted);
        // yield return new WaitUntil(() => PlayServicesController.HasSDKInitializedAlongWithSignInAttempt);
        yield return null;
        //UnityEngine.Console.Log("Social SDK's initialzation process completed");

        FaceBookManager.OnUserLoggedInToFaceBook.AddListener(LinkFaceBookWithActiveAccountIfRequired);
        PlayServicesController.OnUserLoggedIn.AddListener(LinkGooglePlayWithActiveAccountIfRequired);

        SignInTheUserToFireBase();
    }

    #region AccountsLinking

    private async void LinkGooglePlayWithActiveAccountIfRequired()
    {
        if (!IsLoggedInToFireBase)
        {
            UnityEngine.Console.Log($"The user isn't logged in to firebase when trying to check for account linkage with google play. Attemping to sign in with google play");
            SignInTheUserWithGooglePlay();
            return;
        }

        if (IsSpecificProviderPresentInCurrentlyLoggedAccount(ProviderIDs.GooglePlay))
        {
            UnityEngine.Console.Log("The currently logged firebase account is already linked with a google play account. Attemping to sign in with the logged in google play");
            SignInTheUserWithGooglePlay();
            return;
        }

        string currentProvider = firebaseAuth.CurrentUser.ProviderId;
        string token = "Nill";//PlayGamesPlatform.Instance.GetServerAuthCode();

        if (token == null)
        {
            UnityEngine.Console.LogWarning($"Google play token was null while trying to link google play account with {currentProvider}");
            return;
        }

        Credential credential = PlayGamesAuthProvider.GetCredential(token);

        Task<AuthResult> task = firebaseAuth.CurrentUser.LinkWithCredentialAsync(credential);
        try
        {
            await task;
        }
        catch (Exception e)
        {
            UnityEngine.Console.LogWarning($"Failed to link google play account with {currentProvider}. Reason {e}");
        }
        finally
        {
            if (task != null && task.IsCompletedSuccessfully && task.Result != null)
            {
                UnityEngine.Console.Log($"Successfully linked google play with {currentProvider}");
                OnUserLinkedAnAccountToFireBase?.Invoke(currentProvider);
            }
            else
            {
                UnityEngine.Console.Log($"Linkage of {currentProvider} and google play failed. Trying to sign in with google play");
                SignInTheUserWithGooglePlay();
            }
        }
    }

    private async void LinkFaceBookWithActiveAccountIfRequired()
    {
        if (!IsLoggedInToFireBase)
        {
            UnityEngine.Console.Log($"The user isn't logged in to firebase when trying to check for account linkage with facebook. Attemping to sign in with facebook");
            SignInTheUserWithFaceBook();
            return;
        }

        if (IsSpecificProviderPresentInCurrentlyLoggedAccount(ProviderIDs.FaceBook))
        {
            UnityEngine.Console.Log("The currently logged firebase account is already linked with a facebook account. Attemping to sign in with the logged in facebook");
            SignInTheUserWithFaceBook();
            return;
        }

        string currentProvider = firebaseAuth.CurrentUser.ProviderId;
        string token = FaceBookManager.GetAccessTokenString;

        if (token == null)
        {
            UnityEngine.Console.LogWarning($"Facebook token was null while trying to link facebook account with {currentProvider}");
            return;
        }

        Credential credential = FacebookAuthProvider.GetCredential(token);

        Task<AuthResult> task = firebaseAuth.CurrentUser.LinkWithCredentialAsync(credential);
        try
        {
            await task;
        }
        catch (Exception e)
        {
            UnityEngine.Console.LogWarning($"Failed to link facebook account with {currentProvider}. Reason {e}");
        }
        finally
        {
            if (task != null && task.IsCompletedSuccessfully)
            {
                UnityEngine.Console.Log($"Successfully linked facebook with {currentProvider}");
                OnUserLinkedAnAccountToFireBase?.Invoke(currentProvider);
            }
            else
            {
                UnityEngine.Console.Log($"Linkage of {currentProvider} and facebook failed. Trying to sign in with facebook");
                SignInTheUserWithFaceBook();
            }
        }
    }

    #endregion AccountsLinking

    #region SigningInToFireBase

    public void SignInTheUserToFireBase()
    {
        if (firebaseAuth == null)
        {
            UnityEngine.Console.LogWarning("Reference to firebase AUTH was null when attempting firebase sign in");
            return;
        }

        if (IsSignedInUsingSocialNetwork)
        {
            UnityEngine.Console.Log($"The User {firebaseAuth.CurrentUser.DisplayName} is already signed in to firebase with social network");
            PrintUserDetails(firebaseAuth.CurrentUser);
            InvokeUserAuthenticatedEvent();
            return;
        }

        if (!IsAnonymouslyLoggedIn && !FB.IsLoggedIn)
        {
            UnityEngine.Console.Log($"User is not logged in to social networks. Attempting anonymous signing in to firebase");
            SignInTheUserAnonymously();
        }
        else if (FB.IsLoggedIn)
        {
            UnityEngine.Console.Log($"Attempting signing in to firebase using facebook");

            // Anonymous account will be lost as there's no linkage once firebase is logged with facebook
            SignInTheUserWithFaceBook();
        }
        //else if (PlayGamesPlatform.Instance.localUser.authenticated)
        //{
        //    UnityEngine.Console.Log($"Attempting signing in to firebase using google play");
        //    // Anonymous account will be lost as there's no linkage once firebase is logged with google play
        //    SignInTheUserWithGooglePlay();
        //}
        else if (IsAnonymouslyLoggedIn)
        {
            PrintUserDetails(firebaseAuth.CurrentUser);
            InvokeUserAuthenticatedEvent();
        }
    }

    private async void SignInTheUserWithGooglePlay()
    {
        //var result = PlayGamesPlatform.Instance.GetServerAuthCode();

        //if (!string.IsNullOrEmpty(result))
        //{
        //    UnityEngine.Console.Log($"Google Play User's OAuth Token Is {result}");
        //    Credential credential = PlayGamesAuthProvider.GetCredential(result);
        //    Task<FirebaseUser> t = firebaseAuth.SignInWithCredentialAsync(credential);

        //    try
        //    {
        //        await t;
        //    }
        //    catch (Exception e)
        //    {
        //        UnityEngine.Console.LogWarning($"Exception occurred while trying to log in the user to firebase with google play. {e}");
        //    }
        //    finally
        //    {
        //        if (t != null && t.IsCompletedSuccessfully && t.Result != null)
        //        {
        //            UnityEngine.Console.Log($"Success FireBaseUser with google {t.Result.DisplayName}, {t.Result.Email}");
        //            PrintUserDetails(t.Result);
        //            InvokeUserAuthenticatedEvent();
        //            OnUserSignedInToFireBaseWithSocialAccount?.Invoke(ProviderIDs.GooglePlay);
        //        }
        //    }
        //}
        //else
        //{
        //    UnityEngine.Console.LogError("Can't get Google Play token");
        //}
    }

    private async void SignInTheUserWithFaceBook()
    {
        string token = FaceBookManager.GetAccessTokenString;

        if (token == null)
        {
            UnityEngine.Console.LogWarning($"Facebook token was null while trying to sign in user to firebase using facebook");
            return;
        }

        Credential credential = FacebookAuthProvider.GetCredential(token);
        Task<FirebaseUser> t = firebaseAuth.SignInWithCredentialAsync(credential);

        try
        {
            await t;
        }
        catch (Exception e)
        {
            UnityEngine.Console.LogWarning($"Exception occurred while trying to log in the user to firebase with facebook. {e}");
        }
        finally
        {
            if (t != null && t.IsCompletedSuccessfully && t.Result != null)
            {
                UnityEngine.Console.Log($"Successfully logged in to firebase using facebook");
                PrintUserDetails(t.Result);
                InvokeUserAuthenticatedEvent();
                OnUserSignedInToFireBaseWithSocialAccount?.Invoke(ProviderIDs.FaceBook);
            }
        }
    }

    private async void SignInTheUserAnonymously()
    {
        if (IsAnonymouslyLoggedIn)
        {
            UnityEngine.Console.Log("Not signing the user anonmously as it's already anonymous");
            return;
        }

        Task<AuthResult> t = firebaseAuth.SignInAnonymouslyAsync();

        try
        {
            await t;
        }
        catch (Exception e)
        {
            UnityEngine.Console.LogWarning($"Exception occurred while trying to anonymously log in the user to firebase. {e}");
        }
        finally
        {
            if (t != null && t.IsCompletedSuccessfully && t.Result != null)
            {
                UnityEngine.Console.Log($"Successfully logged in to firebase anonymously");
                PrintUserDetails(t.Result.User);
                OnUserAnonymouslySignedIn?.Invoke();
                InvokeUserAuthenticatedEvent();
            }
        }
    }

    #endregion SigningInToFireBase

    #region HelperMethods

    public string GetUserNameAccordingToDefinedProvidersPriority()
    {
        if (firebaseAuth.CurrentUser == null)
            return null;

        var providersData = firebaseAuth.CurrentUser.ProviderData;

        int indexOfFaceBookEntry, indexOfGooglePlay;
        indexOfFaceBookEntry = indexOfGooglePlay = -1;
        int i = 0;

        foreach (var item in providersData)
        {
            if (item.ProviderId == ProviderIDs.FaceBook)
            {
                indexOfFaceBookEntry = i;
                break;
            }

            if (item.ProviderId == ProviderIDs.GooglePlay)
            {
                indexOfGooglePlay = i;
            }

            i++;
        }

        if (indexOfFaceBookEntry != -1)
        {
            return providersData.ElementAt(indexOfFaceBookEntry).DisplayName;
        }
        else if (indexOfGooglePlay != -1)
        {
            return providersData.ElementAt(indexOfGooglePlay).DisplayName;
        }
        else
        {
            return "You";
        }
    }

    public string GetUserNameSpecificToProvider(string providerID)
    {
        if (firebaseAuth.CurrentUser == null)
            return null;

        foreach (var item in firebaseAuth.CurrentUser.ProviderData)
        {
            if (item.ProviderId != providerID)
                continue;

            return item.DisplayName;
        }

        return null;
    }

    private void InvokeUserAuthenticatedEvent()
    {
        OnUserAuthenticatedToFireBase?.Invoke();
    }

    private void PrintUserDetails(FirebaseUser user)
    {
        if (user != null)
        {
            UnityEngine.Console.Log($"UserName Is {user.DisplayName}");
            UnityEngine.Console.Log($"User Email is {user.Email}");
            UnityEngine.Console.Log($"Is the user anonymous ? {user.IsAnonymous}");
            UnityEngine.Console.Log($"User ID of signed in  {user.UserId}");
            UnityEngine.Console.Log($"Provider ID {user.ProviderId}");

            foreach (var item in user.ProviderData)
            {
                UnityEngine.Console.Log($"A provider id {item.ProviderId}");
            }

            UnityEngine.Console.Log($"Meta Data {user.Metadata}");
        }
        else
        {
            UnityEngine.Console.Log("Failed authenticatin to firebase");
        }
    }

    private bool IsSpecificProviderPresentInCurrentlyLoggedAccount(string provider)
    {
        if (firebaseAuth.CurrentUser == null)
            return false;

        foreach (var info in firebaseAuth.CurrentUser.ProviderData)
        {
            UnityEngine.Console.Log($"Provider Exisits {info.ProviderId} while requested is {provider}");

            if (info.ProviderId == provider)
                return true;
        }

        return false;
    }

    #endregion HelperMethods

    /// <summary>
    /// Requests the user firebase authentication token asynchronously
    /// </summary>
    /// <returns>A task containing authentication token as result</returns>
    public static async Task<string> RefreshUserAuthenticationToken()
    {
        try
        {
            if (firebaseAuth.CurrentUser == null)
            {
                return null;
            }

            isAcquringAuthenticationToken = true;

            UnityEngine.Console.Log("Started acquiring Token");

            FirebaseUser user = firebaseAuth.CurrentUser;
            Task<string> t = user.TokenAsync(true).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    UnityEngine.Console.LogError("TokenAsync was canceled.");
                    return null;
                }

                if (task.IsFaulted)
                {
                    UnityEngine.Console.LogWarning("TokenAsync encountered an error: " + task.Exception);
                    return null;
                }

                string idToken = task.Result;

                // Cache the token
                playerAuthenticationToken = idToken;

                UnityEngine.Console.Log("$Successfully obtained firebase authentication token");

                return idToken;
            });

            await t;

            isAcquringAuthenticationToken = false;

            return playerAuthenticationToken;
        }
        catch (Exception e)
        {
            isAcquringAuthenticationToken = false;

            UnityEngine.Console.LogWarning($"Exception while trying to fetch firebase authentication" +
                $"Token. Exception: ${e}");

            return null;
        }
    }
}