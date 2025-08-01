using deVoid.UIFramework;
using DG.Tweening;
using Knights.UISystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Services.Analytics;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms;
using Random = UnityEngine.Random;

public class ReviveManager : AWindowController
{
    [SerializeField] TextMeshProUGUI newCoinText;
    public GamePlaySessionInventory sessionInventory;
    [SerializeField] private GameEvent playerHasRevived, showPauseBtn, onGameOver;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image needleBackGround;
    [SerializeField] private Image headStartImageOnAdRevive;
    [SerializeField] private RectTransform needlePivot;
    [SerializeField] private TextMeshProUGUI DimondsRequiredText;

    //[SerializeField] private TextMeshProUGUI currentScore;
    //[SerializeField] private TextMeshProUGUI currentCategoryRankText;
    [SerializeField] private GameManager gameManager;

    [SerializeField] private GamePlaySessionData gamePlaySessionData;
    [SerializeField] private TheKnights.SaveFileSystem.SaveManager saveManager;
    [SerializeField] private GamePlayBackGroundRewardsData gamePlayBackGroundRewardsData;

    //[SerializeField] private LeaderBoardDataSO leaderboardData;
    [SerializeField] private MapProgressionSO mapData;

    [SerializeField] private InventorySystem playerInventoryObj;
    [SerializeField] private GamePlaySessionInventory sessionInventoryObj;
    [SerializeField] private PlayerSharedData PlayerSharedData;
    [SerializeField] private SpeedHandler speedHandler;
    [SerializeField] private PlayerContainedData PlayerContainedData;
    [SerializeField] private InputChannel InputChannel;
    [SerializeField] private Button reviveWithDiamondsButton;
    [SerializeField] private GameObject freeReviveGameObject;
    [SerializeField] private RectTransform reviveWithWatchADRT;
    [SerializeField] protected GameObject reviveADBtnRewardRibbon;
    [SerializeField] private RectTransform reviveWithDiamondRT;

    [SerializeField] private int consectiveDiamondRevive = 0;
    [SerializeField] private int consectiveAdsRevive = 0;

    [SerializeField] private GameObject mapProgression, generalProgression, playerToBeatProgression;
    [SerializeField] private ProgressPanel generalProgresScript;
    [SerializeField] private AdsController adsController;

    [SerializeField] private PickupsUtilityHelper pickupsUtilityHelper;
    [SerializeField] private SpecialPickupsEnumSO specialPickupsEnumSO;

    [SerializeField] protected ReviveRewardedADSO reviveRewardedSO;

    private int deductionValue = 0;
    private AudioPlayer reviveBtnSFX;
    private Coroutine gameOverPanelRoutine;
    //  private bool reviveAdHasStarted;

    private Vector2 defaultWatchADAnchorMin;
    private Vector2 defaultWatchADAnchorMax;
    private Vector2 defaultWatchADAnchoredPos;
    private Vector2 defaultWatchADPivot;

    private Vector2 defaultDiamondReviveAnchorMin;
    private Vector2 defaultDiamondReviveAnchorMax;
    private Vector2 defaultWDiamondReviveAnchoredPos;
    private Vector2 defaultDiamondRevivePivot;

    private bool isBackgroundPanelPressed = false;
    private bool isReviveSkipTapAnalyticsEventSent = false;

    private Tween clockNeedleTween;
    private Tween clockFillTween;
    private float elapsedTimeForClockFill;

    private bool showHighScreenScreen = false;
    private bool isClockAnimCompleted;

    private readonly Queue<Action> panelsToProcessOnCompletionQueue = new Queue<Action>();
    private string lastCompletionPanelOpened;
    [SerializeField] TextMeshProUGUI scoreText;

    public int minScore, maxScore;

    #region ProgressionVariables

    private Queue<float> timeQueue = new Queue<float>();
    private bool showMapOneTime = false;

    #endregion ProgressionVariables

    protected override void Awake()
    {
        base.Awake();
        PlayerPrefs.SetInt("failcounter", 1);
        consectiveAdsRevive = 0;
        consectiveDiamondRevive = 0;

        defaultWatchADAnchoredPos = reviveWithWatchADRT.anchoredPosition;
        defaultWatchADAnchorMin = reviveWithWatchADRT.anchorMin;
        defaultWatchADAnchorMax = reviveWithWatchADRT.anchorMax;
        defaultWatchADPivot = reviveWithWatchADRT.pivot;

        defaultWDiamondReviveAnchoredPos = reviveWithDiamondRT.anchoredPosition;
        defaultDiamondReviveAnchorMin = reviveWithDiamondRT.anchorMin;
        defaultDiamondReviveAnchorMax = reviveWithDiamondRT.anchorMax;
        defaultDiamondRevivePivot = reviveWithDiamondRT.pivot;

        #region progression

        HashSet<float> userRewardAcquired = saveManager.MainSaveFile.gamePlayBackGroundAwardsAcquired;

        foreach (var item in gamePlayBackGroundRewardsData.GetDictionaryKeys().OrderBy(x => x))
        {
            if (!userRewardAcquired.Contains(item))
            {
                timeQueue.Enqueue(item);
            }
        }

        #endregion progression
    }

    private void UpdateScoreDisplay()
    {
        int score = GetScoreBetweenMinMax();
        scoreText.text = "Only "+score.ToString()+ " points left \nto beat your next opponent";
    }

    private int GetScoreBetweenMinMax()
    {
        // You can replace this logic with your own logic
        return Random.Range(minScore, maxScore + 1);
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        UIScreenEvents.OnScreenOperationEventBeforeAnimation.RemoveListener(CycleThroughCompletionPanels);
    }

    private void OnEnable()
    {
        //  reviveAdHasStarted = false;
      
        newCoinText.text = sessionInventory.GetSessionIntKeyData("AccountCoins").ToString();
        isBackgroundPanelPressed = false;
        isReviveSkipTapAnalyticsEventSent = false;

        dimondIncrement(PlayerPrefs.GetInt("failcounter"));

        //currentScore.text = Mathf.FloorToInt(gamePlaySessionData.DistanceCoveredInMeters).ToString();
        //currentCategoryRankText.text = "#" + saveManager.MainSaveFile.currentCategoryRank;

        InputChannel.PauseInputsFromUser();

        StopGameOverRoutine();

        needleBackGround.fillAmount = 0;
        needlePivot.localRotation = Quaternion.Euler(0, 0, 0);
        elapsedTimeForClockFill = 0;

        // gameOverPanelRoutine = StartCoroutine(GameOverPanelOn());
        StartClockAnimation();

        SendRevivePanelAppeardEvent();

        bool hasPlayerEnoughDiamond = hasPlayerEnoughDiamondsToRevive();
        bool shouldShowRewardedADButton = adsController.AreAdsEnabled;

        if (shouldShowRewardedADButton)
        {
            bool isSafeToGiveHeadStart = pickupsUtilityHelper.isSafeToSpawn(PlayerSharedData.PlayerTransform.position.z, specialPickupsEnumSO.AeroPlanePickup as InventoryItemSO);
            headStartImageOnAdRevive.enabled = isSafeToGiveHeadStart;
        }

        freeReviveGameObject.SetActive(false);

        reviveWithDiamondsButton.interactable = hasPlayerEnoughDiamond;
        reviveWithDiamondsButton.gameObject.SetActive(hasPlayerEnoughDiamond || !shouldShowRewardedADButton);

        reviveWithWatchADRT.gameObject.SetActive(shouldShowRewardedADButton);

        bool shouldComplementaryAwardGiven = reviveRewardedSO.ShouldReviveComplementaryRewardBeGiven();
        reviveADBtnRewardRibbon.SetActive(shouldComplementaryAwardGiven);

        int activeButtons = reviveWithDiamondsButton.gameObject.activeSelf && reviveWithWatchADRT.gameObject.activeSelf ? 2 : 1;

        if (activeButtons == 2)
        {
            reviveWithWatchADRT.anchorMax = defaultWatchADAnchorMax;
            reviveWithWatchADRT.anchorMin = defaultWatchADAnchorMin;
            reviveWithWatchADRT.pivot = defaultWatchADPivot;
            reviveWithWatchADRT.anchoredPosition = defaultWatchADAnchoredPos;

            reviveWithDiamondRT.anchorMax = defaultDiamondReviveAnchorMax;
            reviveWithDiamondRT.anchorMin = defaultDiamondReviveAnchorMin;
            reviveWithDiamondRT.pivot = defaultDiamondRevivePivot;
            reviveWithDiamondRT.anchoredPosition = defaultWDiamondReviveAnchoredPos;
        }
        else
        {
            GameObject activeButton = reviveWithDiamondsButton.gameObject.activeSelf ? reviveWithDiamondsButton.gameObject : reviveWithWatchADRT.gameObject;
            RectTransform rect = activeButton.GetComponent<RectTransform>();

            PositionTheReviveButtonToCenter(rect);
        }

        adsController.OnRewardedFramingWindowEnded.AddListener(EnableFreeReviveButton);
        UpdateScoreDisplay();

        #region progression

        bool shouldShowGeneralProgression = timeQueue.Count > 0;

        if (NextPlayerToBeatUI.IsPlayerToBeatActive && NextPlayerToBeatUI.PlayerToBeatBattle != null)
        {
            playerToBeatProgression.SetActive(true);
            generalProgression.SetActive(false);
            mapProgression.SetActive(false);
        }
        else if (!showMapOneTime)
        {
            generalProgression.SetActive(shouldShowGeneralProgression);
            mapProgression.SetActive(!shouldShowGeneralProgression);
            playerToBeatProgression.SetActive(false);
            showMapOneTime = mapData.AllEnvironmentsCompleted; //this must be done in saveFile
        }
        else
        {
            generalProgresScript.changingPanelType(/*2*/ 1);
            generalProgression.SetActive(true);
            mapProgression.SetActive(false);
            playerToBeatProgression.SetActive(false);

            //if (leaderboardData.onLeaderboardTop)
            //{
            //    generalProgresScript.changingPanelType(1);
            //}
        }

        #endregion progression
    }

    private void PositionTheReviveButtonToCenter(RectTransform rect)
    {
        rect.anchorMax = new Vector2(0.5f, 0);
        rect.anchorMin = new Vector2(0.5f, 0);
        rect.pivot = new Vector2(0.5f, 0);
        rect.anchoredPosition = new Vector2(0, 92f);
    }

    private void EnableFreeReviveButton(bool status)
    {
        if (status)
            return;

        reviveWithWatchADRT.gameObject.SetActive(false);
        reviveWithDiamondRT.gameObject.SetActive(reviveWithDiamondsButton.interactable);

        if (reviveWithDiamondRT.gameObject.activeInHierarchy)
        {
            PositionTheReviveButtonToCenter(reviveWithDiamondRT);
        }
        else
        {
            freeReviveGameObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        adsController.OnRewardedFramingWindowEnded.RemoveListener(EnableFreeReviveButton);

        //  adsController.OnRewardedAdAboutToShow.RemoveListener(MarkReviveAdAsStarted);

        transform.DOKill();
    }

    private void Start()
    {
        reviveBtnSFX = gameObject.GetComponent<AudioPlayer>();
    }

    private void dimondIncrement(int failedvalue) // number of time player has failed
    {
        deductionValue = failedvalue;
        DimondsRequiredText.text = failedvalue.ToString("00");
    }

    //private void MarkReviveAdAsStarted()
    //{
    //    reviveAdHasStarted = true;
    //}

    private void StopGameOverRoutine()
    {
        clockFillTween?.Kill();
        clockNeedleTween?.Kill();

        if (gameOverPanelRoutine != null)
        {
            StopCoroutine(gameOverPanelRoutine);
        }
    }

    private bool hasPlayerEnoughDiamondsToRevive()
    {
        int sessionDiamonds = sessionInventoryObj.GetIntKeyValue("AccountDiamonds");
        int inventoryDiamonds = playerInventoryObj.GetIntKeyValue("AccountDiamonds");

        int totalDiamonds = sessionDiamonds + inventoryDiamonds;

        return totalDiamonds >= deductionValue;
    }

    
    public void ReviveWithDiamonds()
    {
        int inventoryDiamonds = playerInventoryObj.GetIntKeyValue("AccountDiamonds");

        // Check if player has enough diamonds using both session and general inventory
        // Giving priority to general inventory

        if (hasPlayerEnoughDiamondsToRevive())
        {
            int diamondsDeducted;

            // If general inventory has enough diamonds
            if (inventoryDiamonds >= deductionValue)
            {
                playerInventoryObj.UpdateKeyValues(new List<InventoryItem<int>>() { new InventoryItem<int>("AccountDiamonds", -deductionValue) });
                diamondsDeducted = deductionValue;
            }

            // Subtract the remaining diamonds from general inventory
            else
            {
                playerInventoryObj.UpdateKeyValues(new List<InventoryItem<int>>() { new InventoryItem<int>("AccountDiamonds", -inventoryDiamonds) });
                diamondsDeducted = inventoryDiamonds;
            }

            // Subtract the reamaining diamonds from session inventory
            if (diamondsDeducted != deductionValue)
            {
                sessionInventoryObj.AddThisKeyToGamePlayInventory("AccountDiamonds", -(deductionValue - diamondsDeducted));
            }
        }
        else
        {
            UnityEngine.Console.LogWarning("User had no diamonds but revive with diamonds was called");
            return;
        }

        PlayerPrefs.SetInt("failcounter", PlayerPrefs.GetInt("failcounter") * 2);
        revivePlayer();
    }

    public void FreeReviveThePlayer()
    {
        consectiveDiamondRevive = 0;
        consectiveAdsRevive = 0;

        AnalyticsManager.CustomData("PlayerRevivedForFree");

        revivePlayer();
    }

    public void revivePlayer()
    {
        reviveBtnSFX.ShootAudioEvent();

        StopGameOverRoutine();

        //  if (!PlayerSharedData.WallRunBuilding)
        //  {
        DisableHurdlesOnRevive(16 * SpeedHandler.GameTimeScaleBeforeOverriden);
        //   }

        InputChannel.UnPauseInputsFromUser();

        speedHandler.RevertGameTimeScaleToLastKnownNormalSpeed();

        PlayerContainedData.AnimationChannel.Normal();
        Debug.Log("Revive");
        ChasingEnemyHandler.isHitFirstTime = false;
        playerHasRevived.RaiseEvent();
    }

    public void SendRevivePanelAppeardEvent()
    {
        AnalyticsManager.CustomData("GamePlayScreen_ReviveScreen_Appeared", new Dictionary<string, object>
        {
            { "Distance", gamePlaySessionData.DistanceCoveredInMeters },
            { "TimeTaken", gamePlaySessionData.timeElapsedSinceSessionStarted}
        });

        //UnityEngine.Console.LogError("GamePlayScreen_ReviveScreen_Appeared, Distance: " + gamePlaySession.DistanceCoveredInMeters + ", Time: " + gamePlaySession.timeElapsedSinceSessionStarted);
    }

    public void ReviveScreen_Diamond_Btn_Click_Event()
    {
        consectiveDiamondRevive++;
        consectiveAdsRevive = 0;
        AnalyticsManager.CustomData("ReviveScreen_Diamond_Btn_Click", new Dictionary<string, object>()
        {
            { "Distance", gamePlaySessionData.DistanceCoveredInMeters},
            { "ConsecutiveRevive", consectiveDiamondRevive }
        });

        SendRevivedFromDiamondsORADEvent();
    }

    public void ReviveScreen_Ad_Btn_Click_Event()
    {
        consectiveAdsRevive++;
        consectiveDiamondRevive = 0;
        AnalyticsManager.CustomData("ReviveScreen_Rewarded_Btn_Click", new Dictionary<string, object>()
        {
            { "Distance", gamePlaySessionData.DistanceCoveredInMeters},
            { "ConsecutiveRevive", consectiveAdsRevive }
        });

        SendRevivedFromDiamondsORADEvent();
    }

    private void SendRevivedFromDiamondsORADEvent()
    {
        AnalyticsManager.CustomData("ReviveScreen_RevivedFromDiamondORAd", new Dictionary<string, object>()
        {
            { "Distance", gamePlaySessionData.DistanceCoveredInMeters},
            { "ConsecutiveRevive", consectiveAdsRevive }
        });
    }

    public void SetBackgroundPanelBoolPressed(bool val)
    {
        // used in revive panel event trigger
        if (!isReviveSkipTapAnalyticsEventSent && val)
        {
            //UnityEngine.Console.Log("Player has tapped the revive screen");
            AnalyticsManager.CustomData("RevivePanelBackgroundTapped");
            isReviveSkipTapAnalyticsEventSent = true;
        }

        isBackgroundPanelPressed = val;

        if (isBackgroundPanelPressed)
        {
            needlePivot.GetComponent<Image>().color = Color.red;
        }
        else
        {
            needlePivot.GetComponent<Image>().color = Color.white;
        }

        StartClockAnimation();
    }

    private void StartClockAnimation()
    {
        if (isClockAnimCompleted)
            return;

        float multiplier = isBackgroundPanelPressed ? 2.3f : 1;
        float curAngle = needlePivot.localRotation.eulerAngles.z;
        curAngle = curAngle > 0 ? curAngle - 360f : curAngle;

        float remainingAngle = curAngle + 360f;

        if (clockFillTween != null && clockFillTween.IsPlaying())
        {
            elapsedTimeForClockFill += clockFillTween.Elapsed();
        }

        clockFillTween?.Kill();
        clockNeedleTween?.Kill();

        clockFillTween = needleBackGround.DOFillAmount(1, (1 / 5f) * multiplier).SetEase(Ease.Linear).SetSpeedBased();
        clockNeedleTween = needlePivot.DOLocalRotate(new Vector3(0, 0, -remainingAngle), (360f / 5f) * multiplier, RotateMode.LocalAxisAdd).SetEase(Ease.Linear).SetSpeedBased();

        clockFillTween.OnComplete(() =>
        {
            isClockAnimCompleted = true;

            elapsedTimeForClockFill += clockFillTween.Elapsed();

            if (elapsedTimeForClockFill <= 3f)
            {
                AnalyticsManager.CustomData("RevivePanelHeld", new Dictionary<string, object> { { "TimeTaken", elapsedTimeForClockFill } });
            }

            StartCoroutine(GameOverPanelOn());
        });
    }

    private IEnumerator GameOverPanelOn()
    {
        canvasGroup.interactable = false;

        yield return new WaitForSeconds(0.15f);

        //   UI_Close();

        showHighScreenScreen = gamePlaySessionData.DistanceCoveredInMeters > saveManager.MainSaveFile.playerHighScore;

        onGameOver.RaiseEvent();

        UIScreenEvents.OnScreenOperationEventBeforeAnimation.AddListener(CycleThroughCompletionPanels);

        if (sessionInventoryObj.GetIntKeyValue("GameMysteryBox") > 0)
        {
            panelsToProcessOnCompletionQueue.Enqueue(() => {
                lastCompletionPanelOpened = ScreenIds.MysteryBoxPanelDailyReward;
                OpenTheWindow(ScreenIds.MysteryBoxPanelDailyReward);
            });
        }
        if (showHighScreenScreen)
        {
            showHighScreenScreen = false;

            panelsToProcessOnCompletionQueue.Enqueue(() => {
                lastCompletionPanelOpened = ScreenIds.HighScoreScreen;
                OpenTheWindow(ScreenIds.HighScoreScreen);
            });
        }

        panelsToProcessOnCompletionQueue.Enqueue(() => {
            UIScreenEvents.OnScreenOperationEventBeforeAnimation.RemoveListener(CycleThroughCompletionPanels);
            lastCompletionPanelOpened = ScreenIds.GameOverPanel;
            OpenTheWindow(ScreenIds.GameOverPanel);
        });

        panelsToProcessOnCompletionQueue.Dequeue().Invoke();

    
    }


    private void CycleThroughCompletionPanels(string panel, ScreenOperation operation, ScreenType screen)
    {
        if (operation == ScreenOperation.Open)
            return;

        if (panel != lastCompletionPanelOpened)
            return;

        if(panelsToProcessOnCompletionQueue.Count <= 1)
        {
            UIScreenEvents.OnScreenOperationEventBeforeAnimation.RemoveListener(CycleThroughCompletionPanels);
        }


        CoroutineRunner.Instance.WaitTillFrameEndAndExecute(() => { panelsToProcessOnCompletionQueue.Dequeue().Invoke(); });
     
    }

    private void DisableHurdlesOnRevive(float radius)
    {
        Collider[] hitColliders = Physics.OverlapSphere(PlayerSharedData.PlayerTransform.position, radius, 1 << LayerMask.NameToLayer("Obstacles"));
        foreach (var hitCollider in hitColliders)
        {
            var obstacle = hitCollider.GetComponentInParent<Obstacle>();

            if (obstacle == null)
            {
                // A collider other than normal ones
                var eventRequestComponent = hitCollider.GetComponent<EventTriggerOnRequest>();

                if (eventRequestComponent == null)
                {
                    UnityEngine.Console.LogWarning($"No obstacle component or an EventTriggerOnRequest was attached to the collider. Can't disable the obstacle with collider {hitCollider.name} on revive");
                }
                else
                {
                    eventRequestComponent.RaiseTheEvent();
                }

                continue;
            }

            if (obstacle.IsThisObstaclePartOfCustomEncounter)
            {
                // this one is handled by custom encounter script
                continue;
            }

            obstacle.SendObstacleFinishedEvent(0);

            GameObject obstacleGameObject = obstacle.gameObject;

            if (obstacleGameObject != null && obstacleGameObject.activeInHierarchy)
            {
                UnityEngine.Console.LogWarning($"An Obstacle {obstacleGameObject.name} is still active after a request was made to send it back to pool. Deactivating manually");
                obstacleGameObject.SetActive(false);
            }
        }
    }

    //private void OnDrawGizmos()
    //{
    //    if (!GameManager.IsGameStarted)
    //        return;

    //    Gizmos.DrawSphere(PlayerSharedData.PlayerTransform.position, 14);
    //}
}