using System;
using System.Collections.Generic;
using Unity.Services.Core;
using UnityEngine;

public static class UnityGameServicesManager
{
    /// <summary>
    /// Whether the initilization process is completed independent of the result
    /// </summary>
    public static bool isGamingServeInitializationProcessDone = false;

    public static bool IsGamingServiceInitialized;

    public static event Action OnGamingServiceSDKInitialized;

    public static event Action OnUserSignedInToAuthenticationService;

    public static event Action OnGamingServiceFailedToInitialize;

    public static event Action OnUserFailedToSignInToGamingService;

    public static async void InitializeUnityUGS()
    {
        bool isError = false;

        try
        {
            bool isInternetAvailable = await Utilities.IsInternetAvailable();

            if (!isInternetAvailable)
            {
                UnityEngine.Console.Log("Failed to initialize gaming services. Reason: No internet connection");
                throw new System.Exception("No internet connection");
            }

            await UnityServices.InitializeAsync();
        }
        catch (Exception e)
        {
            isError = true;
            UnityEngine.Console.Log($"Exception occurred while trying to initialize unity gaming services. Exception: {e}");
        }
        finally
        {
            isGamingServeInitializationProcessDone = true;

            if (!isError && UnityServices.State != ServicesInitializationState.Uninitialized)
            {
                IsGamingServiceInitialized = true;

                UnityEngine.Console.Log("Unity Game Services Initialized");

                OnGamingServiceSDKInitialized?.Invoke();

                SignInTheUserToGamingService();
                CheckForAnalyticServiceConsents();
            }
            else
            {
                UnityEngine.Console.Log("Failed to initialize gaming services");
                OnGamingServiceFailedToInitialize?.Invoke();
            }
        }
    }

    private static async void SignInTheUserToGamingService()
    {
       // yield return new WaitForSeconds(0.2f)
        OnUserSignedInToAuthenticationService?.Invoke();
        //bool isError = false;

        //try
        //{
        //    if (!AuthenticationService.Instance.IsSignedIn)
        //    {
        //        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        //        UnityEngine.Console.Log("Signed in to authentication service");
        //    }
        //}
        //catch
        //{
        //    isError = true;
        //}
        //finally
        //{
        //    if (!isError && AuthenticationService.Instance.IsSignedIn)
        //    {
        //        OnUserSignedInToAuthenticationService?.Invoke();
        //    }
        //    else
        //    {
        //        OnUserFailedToSignInToGamingService?.Invoke();
        //    }
        //}
    }

    private static async void CheckForAnalyticServiceConsents()
    {
        //try
        //{
        //    List<string> consentIdentifiers = await AnalyticsService.Instance.CheckForRequiredConsents();

        //    UnityEngine.Console.Log("Checked for required analytics consents");

        //    foreach (var item in consentIdentifiers)
        //    {
        //        UnityEngine.Console.Log("Analytics Consent " + item);
        //    }
        //}
        //catch (ConsentCheckException e)
        //{
        //    // Something went wrong when checking the GeoIP, check the e.Reason and handle appropriately.
        //    UnityEngine.Console.LogError("Exception Occurred " + e);
        //}
    }
}