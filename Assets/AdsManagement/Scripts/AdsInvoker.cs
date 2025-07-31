using Knights.UISystem;
using System.Text.RegularExpressions;
using TheKnights.SceneLoadingSystem;
using UnityEngine;

public class AdsInvoker : MonoBehaviour
{
    [SerializeField] private InventorySystem playerInventory;
    [SerializeField] private GamePlaySessionData gamePlaySessionData;
    [SerializeField] private SceneLoader sceneLoader;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    [SerializeField] private AdsController adsController;

    // Interstitial Ads
  //  [SerializeField] private GameEvent OnGameOver;
    [SerializeField] private GameEvent OnTapToPlay;

    // Banner Ads
    [SerializeField] private GameEvent OnCutSceneStarted;
    [SerializeField] private GameEvent playerAboutToQuitTheGame;
    [SerializeField] private GameEvent OnLoadingScreenAppeared;
    [SerializeField] private GameEvent OnLoadingCompleted;
    [SerializeField] private GameEvent OnSceneLoadingStarted;

    private bool gameOverInterstitialShown = false;
    private int loadingScreensCount = 0;

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        UIScreenEvents.OnScreenOperationEventAfterAnimation.AddListener(HandleUIScreensOpened);

        //OnTapToPlay.TheEvent.AddListener(ShowBeforeCutSceneInterstitial);
        OnLoadingScreenAppeared.TheEvent.AddListener(ShowLoadingAds);
        OnLoadingCompleted.TheEvent.AddListener(ShowMainMenuBannerAd);
        OnCutSceneStarted.TheEvent.AddListener(ShowGamePlayBannerAd);
        OnSceneLoadingStarted.TheEvent.AddListener(HandleSceneLoadingStarted);
        playerAboutToQuitTheGame.TheEvent.AddListener(ShowQuitInterstitialAd);
    }

    private void HandleSceneLoadingStarted(GameEvent gameEvent)
    {
        gameOverInterstitialShown = false;
      //  adsController.HideSmallBanner();
    }

    private void ShowGameOverInterstitial()
    {
        bool conditionsSatisfied = VerifyAllSpecificInterstitialADConditions("GameOver");

        if (!conditionsSatisfied)
        {
            adsController.SendInterstitialFailedToShowEvent();
            return;
        }

        adsController.ShowInterstitialAd("GameOver");

        //CoroutineRunner.Instance.WaitForRealTimeDelayAndExecute(() => { adsController.ShowInterstitialAd("GameOver", (status, meta)=> {

        //    adsController.ShowSmallBannerAd(ScreenIds.GameOverPanel);
        
        //}) ; }, 2);
    }

    private void ShowPauseInterstitial()
    {
        bool conditionsSatisfied = VerifyAllSpecificInterstitialADConditions("PausePanel");

        if (!conditionsSatisfied)
        {
            adsController.SendInterstitialFailedToShowEvent();
            return;
        }

        adsController.ShowInterstitialAd("PausePanel");
      //  CoroutineRunner.Instance.WaitForRealTimeDelayAndExecute(() => { adsController.ShowInterstitialAd("PausePanel"); }, .55f);
    }

    private void ShowBeforeCutSceneInterstitial(GameEvent gameEvent)
    {
    //  UnityEngine.Console.Log("Trying to display cutsceane AD");
        bool conditionsSatisfied = VerifyAllSpecificInterstitialADConditions("BeforeCutScene");

        if (!conditionsSatisfied)
        {
            adsController.SendInterstitialFailedToShowEvent();
            return;
        }

        adsController.ShowInterstitialAd("BeforeCutScene", null, "Static");
       // CoroutineRunner.Instance.WaitForRealTimeDelayAndExecute(() => { adsController.ShowInterstitialAd("BeforeCutScene", null, "Static"); }, 2f);
    }

    
    private void ShowQuitInterstitialAd(GameEvent gameEvent)
    {
        bool conditionsSatisfied = VerifyAllSpecificInterstitialADConditions("Quit");

        if (!conditionsSatisfied)
        {
            adsController.SendInterstitialFailedToShowEvent();
            return;
        }

         adsController.ShowInterstitialAd("Quit");
    }

    private void ShowLoadingAds(GameEvent gameEvent)
    {
        if (loadingScreensCount >= 1)
        {
            //adsController.ShowSmallBannerAd("LoadingScreen");
            adsController.HideSmallBanner();
        }

        loadingScreensCount++;
        sceneLoader.ExecutePendingSceneLoadingOperations();
        return;

        if (loadingScreensCount < 1)
        {
            loadingScreensCount++;
            sceneLoader.ExecutePendingSceneLoadingOperations();
            return;
        }

        bool conditionsSatisfied = VerifyAllSpecificInterstitialADConditions("LoadingScreen");

        if (!conditionsSatisfied)
        {
            sceneLoader.ExecutePendingSceneLoadingOperations();
            return;
        }

        CoroutineRunner.Instance.WaitForRealTimeDelayAndExecute(() =>
        {
         

            adsController.ShowInterstitialAd("LoadingScreen");

            sceneLoader.ExecutePendingSceneLoadingOperations();
        }, .2f
        );
    }

    private bool VerifyAllSpecificInterstitialADConditions(string panel)
    {
        //InterstitialRemoteAdConfig interstitialRemoteAdConfig = CurrentRemoteAdConfiguration.InterstitialRemoteAdConfig;
        //InterstitialConfig interstitialConfig = interstitialRemoteAdConfig.GetInterstitialConfigForPanel(panel);

        //if (interstitialConfig != null)
        //{
        //    // Check requirements
        //    InterstitialAdRequrements interstitialAdRequrements = interstitialConfig.Requirements;

        //    if (interstitialAdRequrements != null)
        //    {
        //        string conditionExpression = interstitialAdRequrements.Condition;

        //        // If car is unlocked based on condition
        //        if (conditionExpression != null)
        //        {
        //            string[] conditions = conditionExpression.Split(',');

        //            for (int i = 0; i < conditions.Length; i++)
        //            {
        //                string condition = conditions[i];

        //                if (!CheckIfInterstitialConditionIsStatisfied(condition))
        //                {
        //                    UnityEngine.Console.Log($"Not showing interstitial AD. As the requiirement {condition} wasn't fullfilled");

        //                    return false;
        //                }
        //            }
        //        }
        //    }
        //}

        return true;
    }

    private bool CheckIfInterstitialConditionIsStatisfied(string condition)
    {
        switch (condition)
        {
            case "CarUnlocked_1":

                string result = Regex.Match(condition, @"\d+").Value;

                if (result != null)
                {
                    int intValue = -1;
                    int.TryParse(result, out intValue);

                    if (intValue != -1)
                    {
                        UnityEngine.Console.Log($"Checking if Car {intValue} is unlocked for Interstitial AD requirement");

                        // Temp Price
                        bool shouldShowInterstitial = /*playerInventory.isCarUnlocked(intValue) || */ playerInventory.GetIntKeyValue("AccountCoins") >= 100 ||
                           PlayerPrefs.GetInt($"InterstitialConditionCompleted_{condition}") == 1;

                        if (!shouldShowInterstitial)
                        {
                            UnityEngine.Console.Log($"Not showing interstitial AD as car {intValue}");
                        }
                        else
                        {
                            PlayerPrefs.SetInt($"InterstitialConditionCompleted_{condition}", 1);
                        }

                        return shouldShowInterstitial;
                    }
                }
                else
                {
                    UnityEngine.Console.LogWarning($"Error parsing int from interstitial AD Condition {condition}");
                    return true;
                }

                return true;

            default:

                UnityEngine.Console.Log($"An interstitial requirement {condition} was requested to be evaluated but wasn't handled");

                return true;
        }
    }

    private void ShowGamePlayBannerAd(GameEvent gameEvent)
    {
        adsController.ShowSmallBannerAd("HUDPanel");
    }

    private void ShowMainMenuBannerAd(GameEvent gameEvent)
    {
        //adsController.ShowSmallBannerAd("MainMenuPanel");
        adsController.HideSmallBanner();
    }

    private void HandleUIScreensOpened(string screenID, ScreenOperation screenOperation, ScreenType screenType)
    {
        if (screenOperation == ScreenOperation.Close)
            return;


        switch (screenID)
        {
            case ScreenIds.GameOverPanel:

                adsController.ShowSmallBannerAd(ScreenIds.GameOverPanel);
                if (!gameOverInterstitialShown)
                {
                    gameOverInterstitialShown = true;
                    ShowGameOverInterstitial(); //zzzimp
                }

                break;

            case ScreenIds.Settings:

                adsController.ShowSmallBannerAd(ScreenIds.Settings);
                break;

            case ScreenIds.LeaderBoardsPanel:

                adsController.ShowSmallBannerAd(ScreenIds.LeaderBoardsPanel);
                break;

            case ScreenIds.GoalsPanel:

                adsController.ShowSmallBannerAd(ScreenIds.GoalsPanel);
                break;

            case ScreenIds.CarSelectionPanel:

                adsController.ShowSmallBannerAd(ScreenIds.CarSelectionPanel);
                break;

            case ScreenIds.CharacterSelectionPanel:

                adsController.ShowSmallBannerAd(ScreenIds.CharacterSelectionPanel);
                break;

            case ScreenIds.PausePanel:

                adsController.ShowSmallBannerAd(ScreenIds.PausePanel);
                ShowPauseInterstitial();
             
                break;
            case ScreenIds.ShopPanel:

                adsController.ShowSmallBannerAd(ScreenIds.ShopPanel);
                break;

        }
    }
}