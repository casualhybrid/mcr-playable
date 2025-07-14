using deVoid.UIFramework;
using DG.Tweening;
using Knights.UISystem;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardedPowerupsGamePlayManager : MonoBehaviour
{
    [SerializeField] private GameEvent[] powerupAcquiredEvents;
    [SerializeField] private DOTweenAnimation punchScaleAnimation;
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameEvent onGameStarted;
    [SerializeField] private GameEvent onPlayerCrashed;

    [SerializeField] private float nextPowerupRewardInterval;
    [SerializeField] private float rewardedOppurtunityWindowDuration;

    [SerializeField] private InventoryItemsMetaData inventoryItemsMetaData;
    [SerializeField] private PickupGenerationData pickupGenerationData;
    [SerializeField] private PickupsUtilityHelper pickupsUtilityHelper;
    [SerializeField] private EnvironmentChannel environmentChannel;
    [SerializeField] private PlayerSharedData playerSharedData;
    [SerializeField] private SpecialPickupsEnumSO specialPickupsEnumSO;
    [SerializeField] private InventoryItemSO[] powerupsRewarded;

    [SerializeField] private Image powerupImage;
    [SerializeField] private TextMeshProUGUI powerupText;
    [SerializeField] private TextMeshProUGUI powerupTimerRemainingDurationTxt;

    [SerializeField] private AWindowController hudController;
    [SerializeField] private GameManager gameManager;

    private GameEvent currentRewardedPowerupEvent;
    private InventoryItemSO currentPowerUp;
    private InventoryItemMeta currentPowerUpMeta;

    private bool isInitialized;

    private float elapsedRewardWindowTime;
    private float elapsedTimeForNextRewardedPowerup;

    private bool rewardWindowActive;

    private bool isPaused;

    private const float updateIntervalTick = 0.05f; // 20 times per second
    private float elapsedTickDuration;

    //  private int currentSafeAreaUnpauseOverridenIndex;

    private void Awake()
    {
        onGameStarted.TheEvent.AddListener(Initialize);
        onPlayerCrashed.TheEvent.AddListener(PauseTheCurrentRewardedOffer);
    }

    private void OnEnable()
    {
        if (!isInitialized)
            return;

        ResumeTheCurrentRewardedOffer();
    }

    private void OnDisable()
    {
        if (!isInitialized)
            return;

        PauseTheCurrentRewardedOffer();
    }

    private void OnDestroy()
    {
        onGameStarted.TheEvent.RemoveListener(Initialize);
        onPlayerCrashed.TheEvent.RemoveListener(PauseTheCurrentRewardedOffer);

        for (int i = 0; i < powerupAcquiredEvents.Length; i++)
        {
            powerupAcquiredEvents[i].TheEvent.RemoveListener(CloseRewardedPowerupIfTheAcquiredIsSame);
        }
    }

    private void Initialize(GameEvent gameEvent)
    {
        for (int i = 0; i < powerupAcquiredEvents.Length; i++)
        {
            powerupAcquiredEvents[i].TheEvent.AddListener(CloseRewardedPowerupIfTheAcquiredIsSame);
        }

        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized)
            return;

        if (isPaused)
            return;

        elapsedTickDuration += Time.deltaTime;

        if (elapsedTickDuration < updateIntervalTick)
        {
            return;
        }

        if (rewardWindowActive)
        {
            elapsedRewardWindowTime += elapsedTickDuration;

            powerupTimerRemainingDurationTxt.text = Mathf.CeilToInt(rewardedOppurtunityWindowDuration - elapsedRewardWindowTime).ToString();

            if (elapsedRewardWindowTime >= rewardedOppurtunityWindowDuration)
            {
                CloseTheCurrentRewardedOffer();
            }
        }
        else
        {
            elapsedTimeForNextRewardedPowerup += elapsedTickDuration;

            if (elapsedTimeForNextRewardedPowerup >= nextPowerupRewardInterval)
            {
                SelectRewardedPowerupOppurtunity();
            }
        }

        elapsedTickDuration = 0;
    }

    private void SelectRewardedPowerupOppurtunity()
    {
        List<InventoryItemSO> pickupToSpawn = new List<InventoryItemSO>();

        for (int i = 0; i < powerupsRewarded.Length; i++)
        {
            InventoryItemSO powerup = powerupsRewarded[i];
         
            bool isSafeToSpawn = pickupsUtilityHelper.isSafeToSpawn(playerSharedData.PlayerTransform.position.z, powerup) && !IsThisPowerUpCurrentlyActive(powerup);

            if(powerup == specialPickupsEnumSO.AeroPlanePickup && playerSharedData.CurrentStateName == PlayerState.PlayerBuildingRunState)
            {
                isSafeToSpawn = false;
            }

            if (isSafeToSpawn)
            {
                pickupToSpawn.Add(powerup);
            }
        }

        if (pickupToSpawn.Count == 0)
            return;

        InventoryItemSO randomPower = pickupToSpawn[UnityEngine.Random.Range(0, pickupToSpawn.Count)];
        GameObject powerPrefab = pickupGenerationData.GetSpawnDataForPickup(randomPower).GetPrefab;
        Triggered triggered = powerPrefab.GetComponentInChildren<Triggered>();
        InventoryItemMeta meta = inventoryItemsMetaData.GetInventoryItemMeta(randomPower);
        currentRewardedPowerupEvent = triggered.Event;
        currentPowerUp = randomPower;
        currentPowerUpMeta = meta;

        powerupText.text = meta.Name;
        powerupImage.sprite = meta.Sprite;

        elapsedRewardWindowTime = 0;
        canvas.enabled = true;
        rewardWindowActive = true;

        punchScaleAnimation.DOPlay();

        if (randomPower == specialPickupsEnumSO.AeroPlanePickup)
        {
            environmentChannel.RaiseRequestPauseSwitchEnvironmentGenerationEvent();
        }
    }

    public void OpenTheRewardedPowerupScreenAndPause()
    {
        PauseTheCurrentRewardedOffer();
        gameManager.PauseTheGame();
        hudController.OpenTheWindow(ScreenIds.GamePlayPowerUpADScreen, new GamePlayPowerupRewardProperties() { PowerUpMetaData = currentPowerUpMeta });

        UIScreenEvents.OnScreenOperationEventBeforeAnimation.AddListener(ListenForPowerUpScreenCloseAndResume);
    }

    public void CloseRewardedPowerupScreen_ResumeRewardPower()
    {
        UIScreenEvents.OnScreenOperationEventBeforeAnimation.RemoveListener(ListenForPowerUpScreenCloseAndResume);

        isPaused = false;
        gameManager.UnPauseTheGame();
        hudController.CloseTheWindow(ScreenIds.GamePlayPowerUpADScreen);

        RequestRewardedADForPowerup();
    }

    private void ListenForPowerUpScreenCloseAndResume(string screen, ScreenOperation operation, ScreenType type)
    {
        if (operation != ScreenOperation.Close)
            return;

        if (screen != ScreenIds.GamePlayPowerUpADScreen)
            return;

        UIScreenEvents.OnScreenOperationEventBeforeAnimation.RemoveListener(ListenForPowerUpScreenCloseAndResume);

        isPaused = false;
        CloseTheCurrentRewardedOffer();
        hudController.OpenTheWindow(ScreenIds.GetReadyPanel);
    }

    private void RequestRewardedADForPowerup()
    {
        if (currentRewardedPowerupEvent == null)
            throw new System.Exception("A gameplay powerup should be rewarded but its null");

        currentRewardedPowerupEvent.RaiseEvent();

        // CloseTheCurrentRewardedOffer();
    }

    private void PauseTheCurrentRewardedOffer(GameEvent gameEvent = null)
    {
        isPaused = true;
        canvas.enabled = false;

        punchScaleAnimation.DOPause();
    }

    private void ResumeTheCurrentRewardedOffer()
    {
        isPaused = false;
        canvas.enabled = rewardWindowActive;

        if (rewardWindowActive)
        {
            punchScaleAnimation.DOPlay();
        }
    }

    private void CloseTheCurrentRewardedOffer()
    {
        if (currentPowerUp == specialPickupsEnumSO.AeroPlanePickup)
        {
            environmentChannel.RaiseRequestUnPauseSwitchEnvironmentGenerationEvent();
        }

        currentPowerUp = null;
        currentPowerUpMeta = default;
        rewardWindowActive = false;
        currentRewardedPowerupEvent = null;
        canvas.enabled = false;
        elapsedTimeForNextRewardedPowerup = 0;
        punchScaleAnimation.DOPause();
    }

    private void CloseRewardedPowerupIfTheAcquiredIsSame(GameEvent gameEvent)
    {
        if (gameEvent == currentRewardedPowerupEvent)
        {
            CloseTheCurrentRewardedOffer();
        }
    }

    private bool IsThisPowerUpCurrentlyActive(InventoryItemSO inventoryItemSO)
    {
        if (inventoryItemSO == specialPickupsEnumSO.AeroPlanePickup)
        {
            return playerSharedData.CurrentStateName == "PlayerAeroplaneState";
        }
        else if (inventoryItemSO == specialPickupsEnumSO.ArmourPickup)
        {
            return playerSharedData.IsArmour;
        }
        else if (inventoryItemSO == specialPickupsEnumSO.MagnetPickup)
        {
            return playerSharedData.isMagnetActive;
        }

        throw new System.Exception("Invalid powerup to check for current status " + inventoryItemSO.name);
    }
}