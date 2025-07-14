using System;
using Unity.Services.RemoteConfig;
using UnityEngine;

public class RemoteConfiguration : MonoBehaviour
{
    public static event Action RemoteConfigurationDataFetched;

    public static bool IsTutorialRemoved { get; private set; }

    public static int TutorialType { get; private set; } = 1;
    public static bool IsAdsAllowedOnLowEndRam { get; private set; } = true;

    public static string LatestAppVersion { get; private set; }

    public static string BlackListJson = null;

    public static string BlackListChipsetJson = null;

    [Header("BlackListed Devices")]
    [SerializeField] private BlackListedDevices BlackListedDevices;

    //[Header("DefaultValues")]
    //[SerializeField] private InterstitialRemoteAdConfig InterstitialRemoteAdConfigDefault;

    //[SerializeField] private SmallBannerRemoteAdConfig SmallBannerRemoteAdConfigDefault;
    //[SerializeField] private LargeBannerRemoteAdConfig LargeBannerRemoteAdConfigDefault;

    //[Header("ValuesToBeOverwrittenByRemote")]
    //[SerializeField] private InterstitialRemoteAdConfig InterstitialRemoteAdConfig;

    //[SerializeField] private SmallBannerRemoteAdConfig SmallBannerRemoteAdConfig;
    //[SerializeField] private LargeBannerRemoteAdConfig LargeBannerRemoteAdConfig;

    public bool FetchConfigAsSoonAsInitialized { get; set; } = false;
    public bool isInitialized { get; private set; } = false;

    public struct userAttributes
    {
        // Optionally declare variables for any custom user attributes; if none keep an empty struct:
    }

    public struct appAttributes
    {
        // Optionally declare variables for any custom app attributes; if none keep an empty struct:
        //public string appVersion;
    }

    // Retrieve and apply the current key-value pairs from the service on Awake:
    private void Awake()
    {
        //CurrentRemoteAdConfiguration.InterstitialRemoteAdConfig = InterstitialRemoteAdConfigDefault;
        //CurrentRemoteAdConfiguration.SmallBannerRemoteAdConfig = SmallBannerRemoteAdConfigDefault;
        //CurrentRemoteAdConfiguration.LargeBannerRemoteAdConfig = LargeBannerRemoteAdConfigDefault;

         // Add a listener to apply settings when successfully retrieved:
        RemoteConfigService.Instance.FetchCompleted += ApplyRemoteSettings;

        // Set the user’s unique ID:
        // ConfigManager.SetCustomUserID("some-user-id");

        // Set the environment ID:
        RemoteConfigService.Instance.SetEnvironmentID("dab3ddc3-2c17-4263-9f28-f61bbfc54552");


        UnityGameServicesManager.OnUserSignedInToAuthenticationService += InitializeRemoteConfig;
        UnityGameServicesManager.OnUserFailedToSignInToGamingService += InitializeRemoteConfig;
        UnityGameServicesManager.OnGamingServiceFailedToInitialize += InitializeRemoteConfig;
    }

    private void InitializeRemoteConfig()
    {
        if (isInitialized)
            return;
       

        isInitialized = true;

        if (FetchConfigAsSoonAsInitialized)
            FetchConfig();
    }

    public void FetchConfig()
    {
        if (!isInitialized)
            return;

        // Fetch configuration setting from the remote service:
        RemoteConfigService.Instance.FetchConfigsAsync(new userAttributes(), new appAttributes());
    }

    private void ApplyRemoteSettings(ConfigResponse configResponse)
    {
        UnityEngine.Console.Log("Applying Remote Settings with response" + configResponse);

        // Conditionally update settings, depending on the response's origin:
        switch (configResponse.requestOrigin)
        {
            case ConfigOrigin.Default:
                UnityEngine.Console.Log("No settings loaded this session; using default values.");
                break;

            case ConfigOrigin.Cached:
                UnityEngine.Console.Log("No settings loaded this session; using cached values from a previous session.");
                ManageRemoteAD_Data();
                break;

            case ConfigOrigin.Remote:
                UnityEngine.Console.Log("New settings loaded this session; update values accordingly.");
                ManageRemoteAD_Data();

                break;
        }
    }

    private void ManageRemoteAD_Data()
    {
     //   TutorialType = RemoteConfigService.Instance.appConfig.GetInt("TutorialType");
       // IsTutorialRemoved = RemoteConfigService.Instance.appConfig.GetBool("IsTutorialRemoved");
        IsAdsAllowedOnLowEndRam = RemoteConfigService.Instance.appConfig.GetBool("IsAdsAllowedOnLowEndRam");
        LatestAppVersion = RemoteConfigService.Instance.appConfig.GetString("LatestAppVersion");

        //InterstitialRemoteAdConfig.ResetData();
        //SmallBannerRemoteAdConfig.ResetData();
        //LargeBannerRemoteAdConfig.ResetData();

        //CurrentRemoteAdConfiguration.InterstitialRemoteAdConfig = InterstitialRemoteAdConfig;
        //CurrentRemoteAdConfiguration.SmallBannerRemoteAdConfig = SmallBannerRemoteAdConfig;
        //CurrentRemoteAdConfiguration.LargeBannerRemoteAdConfig = LargeBannerRemoteAdConfig;

       // string RawvalueInterstitialPattern = RemoteConfigService.Instance.appConfig.GetJson("InterstitialPattern");
        //   UnityEngine.Console.Log("Value fetched from remote is RAW " + RawvalueInterstitialPattern);
      //  JsonUtility.FromJsonOverwrite(RawvalueInterstitialPattern, CurrentRemoteAdConfiguration.InterstitialRemoteAdConfig);

      //  string RawvalueLargeBannerPattern = RemoteConfigService.Instance.appConfig.GetJson("LargeBannerPattern");
        //    UnityEngine.Console.Log("Value fetched from remote is RAW " + RawvalueLargeBannerPattern);
      //  JsonUtility.FromJsonOverwrite(RawvalueLargeBannerPattern, CurrentRemoteAdConfiguration.LargeBannerRemoteAdConfig);

      //  string RawvalueSmallBannerPattern = RemoteConfigService.Instance.appConfig.GetJson("SmallBannerPattern");
        //  UnityEngine.Console.Log("Value fetched from remote is RAW " + RawvalueSmallBannerPattern);
        //JsonUtility.FromJsonOverwrite(RawvalueSmallBannerPattern, CurrentRemoteAdConfiguration.SmallBannerRemoteAdConfig);

        // Black Listed Devices
     //   BlackListJson = RemoteConfigService.Instance.appConfig.GetJson("BlackListedDevices");

      //  BlackListChipsetJson = RemoteConfigService.Instance.appConfig.GetJson("BlackListedChipset");

        RemoteConfigurationDataFetched?.Invoke();
    }
}