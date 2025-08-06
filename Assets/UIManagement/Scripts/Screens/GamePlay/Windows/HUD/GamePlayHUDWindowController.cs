using deVoid.UIFramework;
using Knights.UISystem;
using System;
using System.Collections;
using System.Collections.Generic;
using TheKnights.SaveFileSystem;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class GamePlayHUDWindowController : AWindowController
{
    [SerializeField] private GamePlaySessionInventory gamePlaySessionInventory;
    [SerializeField] TextMeshProUGUI highestScore;
    [SerializeField] private GamePlaySessionData gamePlaySessionData;


    [SerializeField] private InventorySystem playerInventory;

    [SerializeField] private SaveManager saveManager;

    [SerializeField] private Canvas headStartCanvas;
    [SerializeField] private TMP_Text headStartCounterTxt;
    [SerializeField] private GameObject headStartBuyPanelObject;
    [SerializeField] private GameObject PuaseButtonGameObject;
    [SerializeField] private GameObject coinsBar, boosterBar;

    [SerializeField] private TextMeshProUGUI diamondStashesText;
    [SerializeField] private TextMeshProUGUI scoresText;
    [SerializeField] private TextMeshProUGUI highscoreText;

    [SerializeField] private Image vignetteEffectImg;

    [SerializeField] private GameEvent gameHasStarted, showPauseBtnEvent;
    [SerializeField] private GameEvent headStartTapped, dailyGoalCompletedEvent, openDiamondStashEvent, allGamePlayRewardsEarned, playerCrashedEvent;
    [SerializeField] private GameEvent playerStartedBuildingClimb, playerExittedVerticalBuildingClimb;

    private bool isInitialized, isHighScoreAchieved = false;

    private const float scoreUpdateInterval = 0.1f;
    private float elapsedScoreUpdateInterval = scoreUpdateInterval;

    protected override void Awake()
    {
        base.Awake();
        gameHasStarted.TheEvent.AddListener(HandleGameHasStarted);

        showPauseBtnEvent.TheEvent.AddListener(PlayerRevived);
    }

    private void OnEnable()
    {
        playerStartedBuildingClimb.TheEvent.AddListener(HandlePlayerStartedBuildingClimb);
        playerExittedVerticalBuildingClimb.TheEvent.AddListener(HandlePlayerExittedVerticalBuildingClimb);
        dailyGoalCompletedEvent.TheEvent.AddListener(OpenDailyGoalsPopUp);
        openDiamondStashEvent.TheEvent.AddListener(OpenDiamondStashCompletedPopup);
        playerCrashedEvent.TheEvent.AddListener(PlayerCrashed);
        allGamePlayRewardsEarned.TheEvent.AddListener(OpenDiamondStashCompletePanel);
    }

    private void OnDisable()
    {
        playerStartedBuildingClimb.TheEvent.RemoveListener(HandlePlayerStartedBuildingClimb);
        playerExittedVerticalBuildingClimb.TheEvent.RemoveListener(HandlePlayerExittedVerticalBuildingClimb);
        dailyGoalCompletedEvent.TheEvent.RemoveListener(OpenDailyGoalsPopUp);
        openDiamondStashEvent.TheEvent.RemoveListener(OpenDiamondStashCompletedPopup);
        playerCrashedEvent.TheEvent.RemoveListener(PlayerCrashed);
        allGamePlayRewardsEarned.TheEvent.RemoveListener(OpenDiamondStashCompletePanel);
    }

    public void HandlePlayerStartedBuildingClimb(GameEvent gameEvent)
    {
        vignetteEffectImg.enabled = true;
        vignetteEffectImg.DOFade(.7f, 3f);
    }

    public void HandlePlayerExittedVerticalBuildingClimb(GameEvent gameEvent)
    {
        vignetteEffectImg.DOFade(0f, 3f).OnComplete(() =>
        {
            vignetteEffectImg.enabled = false;
        });
    }

    private void Start()
    {
        headStartCanvas.enabled = false;
        PuaseButtonGameObject.SetActive(false);

        UpdateHighScoreText();
    }

    private void PlayerRevived(GameEvent theEvent)
    {
        PuaseButtonGameObject.SetActive(true);
    }

    private void PlayerCrashed(GameEvent theEvent)
    {
        PuaseButtonGameObject.SetActive(false);
    }

    private void OpenDiamondStashCompletePanel(GameEvent theEvent)
    {
        OpenThePanel(ScreenIds.DiamondStashesPopUp);
        WaitForPopUpClose(() =>
        {
            CloseThePanel(ScreenIds.DiamondStashesPopUp);
        });

        // FIX IT
    }

    private void OpenDailyGoalsPopUp(GameEvent theEvent)
    {
         UnityEngine.Console.Log("Open Daily Goal Panel");
        OpenThePanel(ScreenIds.DailyGoalsPopUp);
        WaitForPopUpClose(() =>
        {
            CloseThePanel(ScreenIds.DailyGoalsPopUp);
        });
        //StartCoroutine(WaitForPopUpClose(ScreenIds.DailyGoalsPopUp));
    }

    private void OpenHighScorePopUp()
    {
        UnityEngine.Console.Log("HighScore Achieved");
        OpenThePanel(ScreenIds.HighScorePopUp);
        WaitForPopUpClose(() =>
        {
            CloseThePanel(ScreenIds.HighScorePopUp);
        });
        //StartCoroutine(WaitForPopUpClose(ScreenIds.HighScorePopUp));
    }

    private void OpenDiamondStashCompletedPopup(GameEvent theEvent)
    {
        OpenThePanel(ScreenIds.DiamondStashCompletePopUp);
        WaitForPopUpClose(() =>
        {
            CloseThePanel(ScreenIds.DiamondStashCompletePopUp);
        });
    }

    private void WaitForPopUpClose(Action action)
    {
       CoroutineRunner.Instance.StartCoroutine(WaitForPopUpCloseRoutine(action));
    }

    private IEnumerator WaitForPopUpCloseRoutine(Action action)
    {
        yield return new WaitForSecondsRealtime(3);
        action?.Invoke();
    }

    public void SendEvents(string eventName)
    {
        AnalyticsManager.CustomData(eventName);
        //if (Debug.isDebugBuild)
        //{
        //          UnityEngine.Console.LogError("Analytics = " + eventName);
        //}
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        gameHasStarted.TheEvent.RemoveListener(HandleGameHasStarted);
        showPauseBtnEvent.TheEvent.RemoveListener(PlayerRevived);
    }

    private void Update()
    {
        if (!isInitialized)
            return;

        elapsedScoreUpdateInterval += Time.deltaTime;

        if (elapsedScoreUpdateInterval < scoreUpdateInterval)
            return;

        elapsedScoreUpdateInterval = 0;

        if (gamePlaySessionData.DistanceCoveredInMeters < 10)
        {
            scoresText.text = Mathf.FloorToInt(gamePlaySessionData.DistanceCoveredInMeters).ToString("00");
        }
        // Will create less garbage as it will not try to prepend "0" which is redundant if value is in double digits
        else
        {
            scoresText.text = Mathf.FloorToInt(gamePlaySessionData.DistanceCoveredInMeters).ToString();
        }

        if (!isHighScoreAchieved)
        {
            if (gamePlaySessionData.DistanceCoveredInMeters > saveManager.MainSaveFile.playerHighScore)
            {
                if (gamePlaySessionData.DistanceCoveredInMeters > 100)
                    OpenHighScorePopUp();
                isHighScoreAchieved = true;
                UnityEngine.Console.Log("HighScore Achived..");
            }
            else
                isHighScoreAchieved = false;
        }


        float highscoreRemaining = saveManager.MainSaveFile.playerHighScore - gamePlaySessionData.DistanceCoveredInMeters;

        if (highscoreRemaining >= 0)
        {
            highscoreText.text = highscoreRemaining.ToString("F2");
        }

    }

    private void UpdateHighScoreText()
    {
        highestScore.text = gamePlaySessionInventory.GetPlayerHighestScore().ToString();
        highscoreText.text = saveManager.MainSaveFile.playerHighScore.ToString();
    }

    private void HandleGameHasStarted(GameEvent gameEvent)
    {
        isInitialized = true;
        headStartCanvas.enabled = true;
        headStartCounterTxt.text = playerInventory.GetIntKeyValue("GameHeadStart").ToString();

        CancelInvoke(nameof(TurnOffHeadStartAfterTime));
        Invoke(nameof(TurnOffHeadStartAfterTime), 3.5f);

        PuaseButtonGameObject.SetActive(true);
    }

    public void HandleHeadStartButton()
    {
        headStartCanvas.enabled = true;
        headStartCounterTxt.text = playerInventory.GetIntKeyValue("GameHeadStart").ToString();

        CancelInvoke(nameof(TurnOffHeadStartAfterTime));
        Invoke(nameof(TurnOffHeadStartAfterTime), 3.5f);
    }

    public void HeadStartTapped()
    {
        bool hasheadStartInInventory = playerInventory.GetIntKeyValue("GameHeadStart") > 0;
        if (hasheadStartInInventory)
        {
            headStartTapped.RaiseEvent();
            playerInventory.UpdateKeyValues(new List<InventoryItem<int>> { new InventoryItem<int>("GameHeadStart", -1) });
            headStartCanvas.gameObject.SetActive(false);
        }
        else
        {
            headStartBuyPanelObject.SetActive(true);
        }

        AnalyticsManager.CustomData("GameplayScreen_HeadStart_Btn_Click");
    }

    public void BuyHeadStart()
    {
        if (playerInventory.GetIntKeyValue("AccountCoins") >= 100)
        {
            playerInventory.UpdateKeyValues(new List<InventoryItem<int>> { new InventoryItem<int>("AccountCoins", -100) });
            playerInventory.UpdateKeyValues(new List<InventoryItem<int>> { new InventoryItem<int>("GameHeadStart", 1) });
        }
    }

    private void TurnOffHeadStartAfterTime()
    {
        headStartCanvas.gameObject.SetActive(false);
    }
}