using deVoid.UIFramework;
using Knights.UISystem;
using System.Collections;
using System.Collections.Generic;
using TheKnights.FaceBook;
using TheKnights.SaveFileSystem;
using TMPro;
using Unity.Services.Analytics;
using UnityEngine;
using TheKnights.LeaderBoardSystem;
using UnityEngine.Events;

public class GameOverUIManager : AWindowController
{
    [SerializeField] TextMeshProUGUI doubleCoin;
    public InventorySystem inventoryObj;
    public GamePlaySessionInventory sessionInventory;
    public AdsController adControl;
    public SaveManager saveManagerObj;
    public GameObject coinsAddParticlaeObj;

    public GameObject highScoreRibbon;

    public TextMeshProUGUI ScoreText;
    public GamePlaySessionData gamePlaySessionData;
    public GamePlaySessionInventory gamePlaySessionInventroy;

    [SerializeField] private TheKnights.LeaderBoardSystem.LeaderBoardManager leaderBoardManager;
    [SerializeField] private RectTransform leaderBoardBtn;
    [SerializeField] private RectTransform playButtonT;
    [SerializeField] private RectTransform doubleRewardedLowerRect;
    [SerializeField] private GameObject doubleRewardedLower;

    [SerializeField] private UIEffectsChannel uiEffectsChannel;
    [SerializeField] private GameEvent uiCounterEvent;
    [SerializeField] private InputChannel InputChannel;

    [SerializeField] private GameObject mapProgression, generalProgression;
    [SerializeField] private MapProgressionSO mapData;
  //  [SerializeField] private GameObject facebookLoginBar;

    [SerializeField] private UnityEvent onAdFinished;

    // private bool adIsShowingScreenShowed;
    private bool initialized;
    private LeaderBoardAsyncHandler<LeaderBoardQueryResult<LeaderBoardGroupQueryResponseData>> leaderBoardHandler;

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
     //   SetupFaceBookLoginBarUIRelatedDisplay();

        if (initialized)
            return;

      SetLowerButtonPositionsAccordingToRewardedADAvailability();

        initialized = true;

        SubscribeToAdEvents();
    //    FaceBookManager.OnUserLoggedInToFaceBook.AddListener(SetupFaceBookLoginBarUIRelatedDisplay);

        SendGameOverScreenAppearedEvent();

        highScoreRibbon.SetActive(GamePlaySessionCompleted.IsHighScoreMadeLastSession);
    }

    private IEnumerator Start()
    {
        yield return null;
        CheckAndOpenIfPendingLeaderBoardRewardAvailable();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        DeSubscribeToAdEvents();
        //  FaceBookManager.OnUserLoggedInToFaceBook.RemoveListener(SetupFaceBookLoginBarUIRelatedDisplay);

        UnsubscribeLeaderBoardDataRetrieved();
    }

    private void SubscribeToAdEvents()
    {
        adControl.OnInterstitialAdCompleted.AddListener(completedAd);
        adControl.OnInterstitialAdFailedToShow.AddListener(completedAd);
    }

    private void DeSubscribeToAdEvents()
    {
        adControl.OnInterstitialAdCompleted.RemoveListener(completedAd);
        adControl.OnInterstitialAdFailedToShow.RemoveListener(completedAd);
    }

    private void SetLowerButtonPositionsAccordingToRewardedADAvailability()
    {
        bool isRewardedADAvailable = adControl.IsRewardedADAvailable();

        if (!isRewardedADAvailable)
        {
            DisableDoubleRewardedAndReAlignButtons();
        }
    }

    public void DisableDoubleRewardedAndReAlignButtons()
    {
        doubleRewardedLower.SetActive(false);

        leaderBoardBtn.anchorMin = doubleRewardedLowerRect.anchorMin;
        leaderBoardBtn.anchorMax = doubleRewardedLowerRect.anchorMax;

        leaderBoardBtn.anchoredPosition = doubleRewardedLowerRect.anchoredPosition;
        //Vector2 leaderboardButtonAnchoredPos = leaderBoardBtn.anchoredPosition;
        //Vector2 playButtonAnchoredPos = playButtonT.anchoredPosition;

        //homeButtonAnchoredPos.x = 322f;
        //playButtonAnchoredPos.x = -322f;

        //homeButtonT.anchoredPosition = homeButtonAnchoredPos;
        //playButtonT.anchoredPosition = playButtonAnchoredPos;
    }

    private void completedAd()
    {
        //if (adIsShowingScreenShowed)
        //{
        //    UIScreenEvents.OnScreenOperationEventAfterAnimation.AddListener(HandleScreenAfterOperation);
        //    CloseTheWindow(ScreenIds.AdIsLoadingBeforeCutScene);
        //}
        //else
        //{
        PerformGameOverSteps();
        //}
    }

    private void PerformGameOverSteps()
    {
        adControl.ShowSmallBannerAd(ScreenIds.GameOverPanel);

        DeSubscribeToAdEvents();

        //UnityEngine.Console.LogError("OnInterstitialAdCompleted event has been called!");
        SettingUpGameOverPanel();

        // NetworkManager.Instance.GetLeaderboardFromAPI();

        #region progression

        mapProgression.SetActive(!mapData.AllEnvironmentsCompleted);
        generalProgression.SetActive(mapData.AllEnvironmentsCompleted);

        #endregion progression

        onAdFinished.Invoke();

        if (gamePlaySessionInventroy.intKeyItems != null)
        {
            for (int i = 0; i < gamePlaySessionInventroy.intKeyItems.Count; i++)
            {
                var item = gamePlaySessionInventroy.intKeyItems[i];

                if (item.itemValue > 0)
                {
                    uiEffectsChannel.RaiseOnRequireInventoryPanel(item.itemName, item.itemValue);
                }
            }
        }
    }

    private void SettingUpGameOverPanel()
    {
        StartCoroutine(SettingUpGameOverPanelRoutine());
    }

    private IEnumerator SettingUpGameOverPanelRoutine()
    {
        yield return new WaitForSeconds(.1f);
        uiCounterEvent.RaiseEvent();
        yield return new WaitForSeconds(.5f);
        coinsAddParticlaeObj.SetActive(sessionInventory.GetSessionIntKeyData("AccountCoins") > 0 ? true : false);
        // LeaderBoard();
        ScoreText.text = Mathf.FloorToInt(gamePlaySessionData.DistanceCoveredInMeters).ToString();
        doubleCoin.text=(sessionInventory.GetSessionIntKeyData("AccountCoins")*2).ToString();
        InputChannel.PauseInputsFromUser();
    }

    //private void Update()
    //{
    //    if (NetworkManager.Instance.leaderboardTimeLeft != null)
    //    {
    //        TimeSpan ts = NetworkManager.Instance.leaderboardTimeLeft.Subtract(DateTime.Now);
    //        // timeLeftLbl.text = ts.Hours.ToString("D2") + ":" + ts.Minutes.ToString("D2") + ":" + ts.Seconds.ToString("D2");

    //        // Less Garbage
    //        timeLeftLbl.text = ts.ToString("hh\\:mm\\:ss");
    //    }
    //}

    public void SendGameOverScreenAppearedEvent()
    {
        AnalyticsManager.CustomData("GameOverScreen_Appeared", new Dictionary<string, object>()
        {
            {"Distance", gamePlaySessionData.DistanceCoveredInMeters},
            { "TimeTaken", gamePlaySessionData.timeElapsedSinceSessionStarted}
        });


        string lifeTimeDistStr = "LifeTimeDistance";
        float lifeTimeDist = PlayerPrefs.GetFloat(lifeTimeDistStr, 0);
        lifeTimeDist += gamePlaySessionData.DistanceCoveredInMeters;
        PlayerPrefs.SetFloat(lifeTimeDistStr, lifeTimeDist);

        AnalyticsManager.CustomData("LifeTimeDistanceCovered", new Dictionary<string, object>()
        {
            {"Distance", lifeTimeDist}
        });
    }

    public void SendEvents(string eventName)
    {
        AnalyticsManager.CustomData(eventName);
    }

    //private void SetupFaceBookLoginBarUIRelatedDisplay()
    //{
    //    if (!FaceBookManager.isInitialized)
    //        return;

    //    RectTransform mapProgressionRect = mapProgression.GetComponent<RectTransform>();

    //    facebookLoginBar.SetActive(!FaceBookManager.isLoggedInToFaceBook);

    //    Vector2 mapProgressionSizeDelta = mapProgressionRect.sizeDelta;
    //    mapProgressionSizeDelta.y = !FaceBookManager.isLoggedInToFaceBook ? 252.2838f : 340.1135f;

    //    mapProgressionRect.sizeDelta = mapProgressionSizeDelta;
    //}

    private void CheckAndOpenIfPendingLeaderBoardRewardAvailable()
    {
        leaderBoardHandler = leaderBoardManager.GetLeaderBoardGroupDetails();
        leaderBoardHandler.OnLeaderBoardOperationDone += OpenLeaderBoardScreenIfPendingRewardAvailable;
    }

    private void OpenLeaderBoardScreenIfPendingRewardAvailable(LeaderBoardAsyncHandler<LeaderBoardQueryResult<LeaderBoardGroupQueryResponseData>> data)
    {
        leaderBoardHandler.OnLeaderBoardOperationDone -= OpenLeaderBoardScreenIfPendingRewardAvailable;

        if (data == null || !data.Result.IsValid || data.Result.ResponseBody == null)
            return;

        GroupInfo pendingReward = data.Result.ResponseBody.pendingRewardInfo;

        if (pendingReward == null || !pendingReward.IsValid)
            return;

        OpenTheWindow(ScreenIds.LeaderBoardsPanel);
    }
    public void SoundSwitch()
    {
        PersistentAudioPlayer.Instance.ResetSound();
    }

    private void UnsubscribeLeaderBoardDataRetrieved()
    {
        if(leaderBoardHandler != null)
        {
            leaderBoardHandler.OnLeaderBoardOperationDone -= OpenLeaderBoardScreenIfPendingRewardAvailable;
        }
    }

    public void UnsubLeaderBoardDataRetrievalAndOpenLeaderBoardScreen()
    {
        UnsubscribeLeaderBoardDataRetrieved();
        OpenTheWindow(ScreenIds.LeaderBoardsPanel);
    }
}