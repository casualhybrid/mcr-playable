
//using TheKnights.AdsSystem;
using FMODUnity;
using System.Collections;

//using TheKnights.PlayServicesSystem;
using TheKnights.AdsSystem;

using TheKnights.FaceBook;
using TheKnights.PlayServicesSystem;
using TheKnights.Purchasing;
using TheKnights.SaveFileSystem;
using TheKnights.SceneLoadingSystem;

using UnityEngine;

public class PluginsInitializer : MonoBehaviour
{
    public static bool IsPluginInitCompleted { get; private set; }
    public const float MaxPluginInitLoadingVal = 0.2f; //0.8

    [SerializeField] private SaveManager saveManager;
    [SerializeField] private AdsController adsController;
    [SerializeField] private GDPRHandler gdprHandler;
    [SerializeField] private SceneLoaderHandler sceneLoaderHandler;
    [SerializeField] private AudioManagerSO audioManager;
    [SerializeField] private PlayServicesController playServicesController;
    [SerializeField] private IAPManager iAPManager;

    [SerializeField] private FaceBookManager faceBookManager;

    [SerializeField] private RemoteConfiguration remoteConfiguration;
    [SerializeField] private BlackListManager blackListManager;

    //  [SerializeField] private bool warmupShadersAtStart = true;
    //  [SerializeField] private ShaderVariantCollection shaderVariantCollection;

    private ISceneLoadCallBacks sceneLoadingListener;
    private float currentLoadingValue = 0;
    private bool isCustomLoadingFinished;
    private Coroutine fakeLoadRoutineRef;

    private void Awake()
    {
        Application.runInBackground = false;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        // int refreshRate = Screen.currentResolution.refreshRate;
        //  Application.targetFrameRate = Mathf.Clamp(refreshRate, 60, 120);
        //   UnityEngine.Console.Log("Screen refresh rate: " + Screen.currentResolution.refreshRate);
        Application.targetFrameRate = 60;
        //** Do not enable as this will cause non fatal exceptions not to appear in crashlytics
       // Debug.unityLogger.logEnabled = false;

        SubscribeEvents();
    }

    private void OnDestroy()
    {
        DeSubscribeEvents();
    }

    private IEnumerator Start()
    {
        yield return null;

        //if (warmupShadersAtStart)
        //{
        //    shaderVariantCollection.WarmUp();
        //    UnityEngine.Console.Log($"Are shaders warmed up? {shaderVariantCollection.isWarmedUp}. Variants: {shaderVariantCollection.variantCount}");
        //    yield return null;
        //}
        EstimateDevicePerformance.GetDeviceGroup();

        if (!EstimateDevicePerformance.isOreoDevice && !EstimateDevicePerformance.isRamLowerThanThreeGB && !EstimateDevicePerformance.IsPhoneBlackListed)
        {
            InitializeFaceBook();
        }
        else
        {
            InitializeStartingPlugins();
        }
    }

    private void InitializeStartingPlugins()
    {
        StartCoroutine(InitializeStartingPluginsRoutine());
    }

    private IEnumerator InitializeStartingPluginsRoutine()
    {
        UnityGameServicesManager.InitializeUnityUGS();

        SendAnalyticEventLoadingFirstTime();

        yield return null;

        sceneLoadingListener = sceneLoaderHandler.SpawnAndSetSceneLoadingCanvas();
        sceneLoadingListener.SceneLoadingStarted();
        StartCoroutine(LoadingFillRoutine());

        yield return null;

        // UnityEngine.Console.Log("Step Fetch Config");

        SetScreenDPIBasedOnDeviceGroup.SetFixedDPIScaleValuesAccordingToQualitySettings();

        yield return null;

        remoteConfiguration.FetchConfigAsSoonAsInitialized = true;
        remoteConfiguration.FetchConfig();

        yield return null;

        //  blackListManager.CheckAndMarkIfDeviceIsBlackListed();
        adsController.SetAdManagerAndADPluginsReferences();

        yield return null;

        InitializeFireBaseAndStartSaveGameLoad();
    }

    private IEnumerator LoadPluginsRoutine()
    {
        LoadGamePlayLevel();
        yield return null;

        AdManager adManagerChosen = adsController.GetCurrentADManager;

        bool isDeviceBlackListedWithNOADLevel = EstimateDevicePerformance.IsPhoneBlackListed && EstimateDevicePerformance.PhoneBlackListLevel > 0;

        if ((EstimateDevicePerformance.isRamLowerThanTwoGB && !RemoteConfiguration.IsAdsAllowedOnLowEndRam) || EstimateDevicePerformance.IsChipSetBlackListed || isDeviceBlackListedWithNOADLevel)
        {
            adManagerChosen.DisableADSGlobally();
        }

        FakeLoadToSpecifiedValue(MaxPluginInitLoadingVal, 9f); //4
        Debug.LogError("Sceneloader1");


        // ADD GDPR Wait
      //  yield return gdprHandler.InitializeGDPR();
        Debug.LogError("Sceneloader2");
        yield return null;

      //  yield return adManagerChosen.InitializeAds();
        Debug.LogError("Sceneloader3");
       
        Debug.LogError("Sceneloader4");
        StopFakeLoadRoutine();
        UpdateLoadingProgress(MaxPluginInitLoadingVal);

        //yield return null;
        // UnityEngine.Console.Log("Step Set Sounds");
        audioManager.SetUpStartingSounds();
        // playServicesController.InitializerPlayServices();

        yield return null;

        //   UnityEngine.Console.Log("Step Set IN APP");
        iAPManager.InitializeOnGameServicesInitComplete = true;
        iAPManager.Initialize();
        Debug.LogError("Sceneloader5");
        
        yield return null;
        yield return null;

        adsController.PreLoadGoogleAds();

        yield return null;

        // UnityEngine.Console.Log("Step Load GamePlay");
        Debug.LogError("Sceneloader6");
      //  yield return new WaitForSeconds(7);
       // LoadGamePlayLevel();
    }

    private void SubscribeEvents()
    {
        // FireBaseInitializer.OnFireBaseFailedToInitialize += LoadTheSaveGame;
        //  FireBaseInitializer.OnFireBaseInitialized += LoadTheSaveGame;

        saveManager.OnSessionLoaded.AddListener(SaveGameLoaded);
        InitializeStartingPlugins();
        FaceBookManager.OnFaceBookInitialized.AddListener(InitializeStartingPlugins);
        FaceBookManager.OnFaceBookFailedInitialized.AddListener(InitializeStartingPlugins);

        RuntimeManager.OnBanksLoaded += LoadPlugins;

        //  BlackListManager.BlackListOperationCompleted += BlackListManager_BlackListOperationCompleted;
    }

    private void InitializeFireBaseAndStartSaveGameLoad()
    {
        UpdateLoadingProgress(.15f);

        // UnityEngine.Console.Log("Step Initialize FireBase");
        //if (!EstimateDevicePerformance.isOreoDevice && !EstimateDevicePerformance.isRamLowerThanThreeGB && !EstimateDevicePerformance.IsPhoneBlackListed)
        //{
        FireBaseInitializer.InitializeFireBaseSDK();
        // }

        //  yield return null;

        // UnityEngine.Console.Log("Step Load Save Game");

        CoroutineRunner.Instance.WaitForUpdateAndExecute(LoadTheSaveGame);
    }

    private void DeSubscribeEvents()
    {
        // FireBaseInitializer.OnFireBaseFailedToInitialize -= LoadTheSaveGame;
        // FireBaseInitializer.OnFireBaseInitialized -= LoadTheSaveGame;

        saveManager.OnSessionLoaded.RemoveListener(SaveGameLoaded);

        FaceBookManager.OnFaceBookInitialized.RemoveListener(InitializeStartingPlugins);
        FaceBookManager.OnFaceBookFailedInitialized.RemoveListener(InitializeStartingPlugins);

        RuntimeManager.OnBanksLoaded -= LoadPlugins;
        //  BlackListManager.BlackListOperationCompleted -= BlackListManager_BlackListOperationCompleted;
    }

    private void LoadPlugins()
    {
        UpdateLoadingProgress(.5f);

        //  UnityEngine.Console.Log("Step Load Banks");

       

        StartCoroutine(LoadPluginsRoutine());
    }

    private void InitializeFaceBook()
    {
        //   UnityEngine.Console.Log("Step Load FaceBook");
        faceBookManager.InitializeFaceBookSDK();
    }

    private void SaveGameLoaded()
    {
        UpdateLoadingProgress(.35f);

        //if (!EstimateDevicePerformance.isOreoDevice && !EstimateDevicePerformance.isRamLowerThanThreeGB && !EstimateDevicePerformance.IsPhoneBlackListed)
        //{
        //    InitializeFaceBook();
        //}
        //else
        //{
        LoadBanks();
        //}
    }

    private void LoadTheSaveGame()
    {
        saveManager.SetupTheSaveGame();
    }

    private void LoadGamePlayLevel()
    {
        IsPluginInitCompleted = true;
        isCustomLoadingFinished = true;
        sceneLoaderHandler.LoadGamePlayScene();
    }

    private void LoadBanks()
    {
        

        CoroutineRunner.Instance.WaitForUpdateAndExecute(() => { RuntimeManager.Instance.CreateInstanceOfRunTimeManager(); });
    }

    private void SendAnalyticEventLoadingFirstTime()
    {
        if (!PlayerPrefs.HasKey("GameFirstTimeOpen"))
        {
            AnalyticsManager.CustomData("LoadingScreen_GameStart_FirstTime");

            PlayerPrefs.SetInt("GameFirstTimeOpen", 1);
        }
    }

    private void UpdateLoadingProgress(float val)
    {
        currentLoadingValue = val;
    }

    private IEnumerator LoadingFillRoutine()
    {
        while(!isCustomLoadingFinished)
        {
            yield return null;
            sceneLoadingListener?.OnLoadingProgressChanged(currentLoadingValue);
        }
    }

    private void FakeLoadToSpecifiedValue(float targetVal, float chunkSize)
    {
        StopFakeLoadRoutine();
        fakeLoadRoutineRef = StartCoroutine(FakeLoadToSpecifiedValueRoutine(targetVal, chunkSize));
    }

    private void StopFakeLoadRoutine()
    {
        if(fakeLoadRoutineRef != null)
        {
            StopCoroutine(fakeLoadRoutineRef);
        }
    }

    private IEnumerator FakeLoadToSpecifiedValueRoutine(float targetVal, float chunkSize)
    {
        float chunk = (targetVal - currentLoadingValue) / chunkSize;

        while (true)
        {
            yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(2, 4));
            currentLoadingValue = Mathf.Clamp(currentLoadingValue + chunk, 0, targetVal);
        }
    }
}