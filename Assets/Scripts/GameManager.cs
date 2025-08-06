using DG.Tweening;
using TheKnights.SaveFileSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "GameManager", menuName = "ScriptableObjects/GameManager")]
public class GameManager : ScriptableObject
{
    public static bool GameIsPaused { get; private set; }
    public static bool IsTimeScaleLocked { get; set; }

    [SerializeField] private RequestPlayerGotHitChannel requestPlayerGotHitChannel;
    [SerializeField] private InputChannel inputChannel;
    [SerializeField] private AdsController adsController;
    [SerializeField] private InventorySystem inventoryObj;
    [SerializeField] private float relaxTimerForJump;
    [SerializeField] private bool forceInvincible;
    [SerializeField] private SaveManager saveManager;
    public bool Invincible { get; set; }
    [SerializeField] private float terminalVelocity;
    [SerializeField] private float startSpeed = 1f;
    [SerializeField] private float minimumGameSpeed;
    [SerializeField] private float maximumGameSpeed;
    [SerializeField] private PlayerSharedData playerRunTimeData;
    [SerializeField] private SpeedHandler speedHandler;
    [SerializeField] private GameEvent armourHasBeenUsedUp;
    [SerializeField] private GameEvent gameHasBeenPaused;
    [SerializeField] private GameEvent gameHasBeenResumed;
    [SerializeField] private GameEvent playerHasCrashed;
    [SerializeField] private GameEvent playerHasCrashedNormal;
    [SerializeField] private GameEvent playerHasHitFromFront;
    [SerializeField] private GameEvent playerHasHitFromSide;
    [SerializeField] private GameEvent playerHasStumbled;
    [SerializeField] private GameEvent BoostPickedUp;
  //  [SerializeField] private GameEvent FigrinePartPickUp;
    [SerializeField] private GameEvent GameHasStarted;
    [SerializeField] private GameEvent cutSceneTimeLineFinished;
    [SerializeField] private GameEvent playerHasCrashedFromFront;
    [SerializeField] private GameEvent playerHasCrashedFromSide;
    [SerializeField] private GameEvent playerAboutToQuitTheGame;
    [SerializeField] private GameEvent dependenciesLoaded;

    public static bool DependenciesLoaded = false;
    public static bool CallEventOneTime = false;
    public static bool IsGameStarted = false;
    public static bool IsGamePlayStarted = false;
    public const float ChunkDistance = 575f; //575
    public const float BoostExplosionDistance = 44f;

    public float GetMinimumSpeed => minimumGameSpeed;
    public float GetMaximumSpeed => maximumGameSpeed;
    public float GetStartingSpeed => startSpeed;

    public float GetTerminalVelocity => terminalVelocity;
    public bool IsForcedInvincible => forceInvincible;
    public float GetRelaxTimerForJump => relaxTimerForJump;

    /// <summary>
    /// GamePlay Time In Seconds
    /// </summary>
    public static float gameplaySessionTimeInSeconds { get; set; }

    /// <summary>
    /// GamePlay Time In Minutes
    /// </summary>
    public static float gameplaySessionTimeInMinutes
    { get { return gameplaySessionTimeInSeconds / 60f; } }

    private static Tween slowlyIncreaseTimeScaleTween;

    private void OnEnable()
    {
        SubscribeToEvents();
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        ResetVariable();
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        DeSubscribeToEvents();
    }

    public void SendCrashEvents(GameEvent gameEvent)
    {
        AnalyticsManager.CustomData("GamePlayScreen_HitFail");
    }

    private void ResetVariable()
    {
        IsGamePlayStarted = false;
        IsGameStarted = false;
        DependenciesLoaded = false;
        GameIsPaused = false;
        IsTimeScaleLocked = false;
        Time.timeScale = 1;
        gameplaySessionTimeInSeconds = 0;
        Invincible = forceInvincible;
    }

    private void SubscribeToEvents()
    {
        requestPlayerGotHitChannel.OnRequestPlayerToGetStumble += StumblePlayerOrFailIfAlreadyStumbled;
        playerHasHitFromFront.TheEvent.AddListener(PlayerHasBeenHitFromFront);
        playerHasHitFromSide.TheEvent.AddListener(PlayerHasBeenHitFromSide);
      //  FigrinePartPickUp.TheEvent.AddListener(figurinePickup);
        GameHasStarted.TheEvent.AddListener(HandleGameStarted);
        cutSceneTimeLineFinished.TheEvent.AddListener(HandleCutSceneTimeLineFinished);
        dependenciesLoaded.TheEvent.AddListener(HandleDependenciesLoaded);
    }

    private void DeSubscribeToEvents()
    {
        requestPlayerGotHitChannel.OnRequestPlayerToGetStumble -= StumblePlayerOrFailIfAlreadyStumbled;
        playerHasHitFromFront.TheEvent.RemoveListener(PlayerHasBeenHitFromFront);
        playerHasHitFromSide.TheEvent.RemoveListener(PlayerHasBeenHitFromSide);
        GameHasStarted.TheEvent.RemoveListener(HandleGameStarted);
     //   FigrinePartPickUp.TheEvent.RemoveListener(figurinePickup);
        cutSceneTimeLineFinished.TheEvent.RemoveListener(HandleCutSceneTimeLineFinished);
        dependenciesLoaded.TheEvent.RemoveListener(HandleDependenciesLoaded);
    }

    private void PlayerHasBeenHitFromFront(GameEvent theEvent)
    {
      

        if (playerRunTimeData.IsDash && playerRunTimeData.isInInvincibleZoneDuringDash)
        {
            UnityEngine.Console.LogWarning("We saved you noob! Git Gud!");
            return;
        }

        if (FailGame())
        {
            playerHasCrashedFromFront.RaiseEvent();
            PersistentAudioPlayer.Instance.PlayTumTumSound();
            //    UnityEngine.Console.Log("playerHasCrashedFromFront");
        }
    }

    private bool FailGame()
    {
        if (Invincible)
        {
            return false;
        }

        if (playerRunTimeData.IsArmour)
        {
            armourHasBeenUsedUp.RaiseEvent();
            return false;
        }

        speedHandler.ChangeGameTimeScaleToZero();

        // For both tutorial and normal gameplay
        playerHasCrashed.RaiseEvent();

        if (saveManager.MainSaveFile.TutorialHasCompleted)
        {
            // For normal gameplay only
            playerHasCrashedNormal.RaiseEvent();
        }

        return true;
    }

    private void PlayerHasBeenHitFromSide(GameEvent theEvent)
    {
        StumblePlayerOrFailIfAlreadyStumbled();
    }


    private void StumblePlayerOrFailIfAlreadyStumbled()
    {
        if (Invincible)
        {
            return;
        }

        if(playerRunTimeData.IsArmour)
        {
            armourHasBeenUsedUp.RaiseEvent();
            return;
        }

        if (playerRunTimeData.isStumbled)
        {
            if (FailGame())
            {
                playerHasCrashedFromSide.RaiseEvent();
                //     UnityEngine.Console.Log("playerHasCrashedFromFront");
            }
            return;
        }

        playerRunTimeData.isStumbled = true;
        playerHasStumbled.RaiseEvent();
    }

    //private void figurinePickup(GameEvent theEvent)
    //{
    //    PlayerPrefs.SetInt("figurinePartsCount1", PlayerPrefs.GetInt("figurinePartsCount1") + 1);
    //}

    public void PauseTheGame()
    {
        KillSlowlyIncreaseTimeScale();
        GameIsPaused = true;
        Time.timeScale = 0;
        gameHasBeenPaused.RaiseEvent();
        inputChannel.PauseInputsFromUser();
    }

    public void UnPauseTheGame()
    {
        KillSlowlyIncreaseTimeScale();
        GameIsPaused = false;
        IsTimeScaleLocked = true;
        PauseAndSlowlyIncreaseTimeScale();
        inputChannel.UnPauseInputsFromUser();
        gameHasBeenResumed.RaiseEvent();
    }

    private void HandleGameStarted(GameEvent gameEvent)
    {
        AnalyticsManager.CustomData("GameHasStarted");

        IsGameStarted = true;
    }

    private void HandleDependenciesLoaded(GameEvent gameEvent)
    {
        DependenciesLoaded = true;
    }

    private void HandleCutSceneTimeLineFinished(GameEvent gameEvent)
    {
        IsGamePlayStarted = true;
    }

    public static void PauseAndSlowlyIncreaseTimeScale()
    {
        Time.timeScale = 0;
        slowlyIncreaseTimeScaleTween = DOTween.To(() => Time.timeScale, (x) => { Time.timeScale = x; }, 1, 3f).SetEase(Ease.InSine).SetUpdate(true).OnComplete(()=> { IsTimeScaleLocked = false; });
    }

    private void KillSlowlyIncreaseTimeScale()
    {
        IsTimeScaleLocked = false;
        slowlyIncreaseTimeScaleTween?.Kill();
        slowlyIncreaseTimeScaleTween = null;
    }

    public void QuitTheGame()
    {
        adsController.OnInterstitialAdCompleted.AddListener(() => { CoroutineRunner.Instance.WaitForUpdateAndExecute(() => { Application.Quit(); }); });
        adsController.OnInterstitialAdFailedToShow.AddListener(() => { CoroutineRunner.Instance.WaitForUpdateAndExecute(() => { Application.Quit(); }); });

        playerAboutToQuitTheGame.RaiseEvent();
    }
}