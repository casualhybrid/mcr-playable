using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "SpeedHandler", menuName = "ScriptableObjects/SpeedHandler")]
public class SpeedHandler : ScriptableObject
{
    public bool isOverriden { get; private set; }
    public static float GameTimeScale { get; private set; }
    public float GameTimeScaleRatio => Mathf.Clamp01((GameTimeScale - gameManager.GetMinimumSpeed) / (gameManager.GetMaximumSpeed - gameManager.GetMinimumSpeed));

    [SerializeField] private GameManager gameManager;
    [SerializeField] private PlayerSharedData PlayerSharedData;
    [SerializeField] private PlayerContainedData PlayerContainedData;
    [SerializeField] private GameEvent playerHasCrashed;
    [SerializeField] private GameEvent playerHasRevived;
    [SerializeField] private GameEvent gameHasStarted;
    [SerializeField] private GameEvent onDependenciesLoaded;

    public static float GameTimeScaleBeforeOverriden { get; private set; }

    private Tween changeGameTimeScale;

    private float my = 0;
    private bool isInitialized;

    private void OnEnable()
    {
        SubscribeToEvents();
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        ResetVariable();
    }

    private void ResetVariable()
    {
        my = 0;
        GameTimeScale = gameManager.GetStartingSpeed;
        GameTimeScaleBeforeOverriden = GameTimeScale;
        isInitialized = false;
        isOverriden = false;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        DeSubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        gameHasStarted.TheEvent.AddListener(HandleGamePlayStarted);
        playerHasCrashed.TheEvent.AddListener(HandlePlayerCrashed);
        playerHasRevived.TheEvent.AddListener(HandlePlayerRevived);
        onDependenciesLoaded.TheEvent.AddListener(HandleDependenciesLoaded);
    }

    private void DeSubscribeToEvents()
    {
        gameHasStarted.TheEvent.RemoveListener(HandleGamePlayStarted);
        playerHasCrashed.TheEvent.RemoveListener(HandlePlayerCrashed);
        playerHasRevived.TheEvent.RemoveListener(HandlePlayerRevived);
        onDependenciesLoaded.TheEvent.RemoveListener(HandleDependenciesLoaded);
    }

    private void HandlePlayerCrashed(GameEvent theEvent)
    {
        isInitialized = false;
    }

    private void HandlePlayerRevived(GameEvent theEvent)
    {
        isInitialized = true;
    }

    private void HandleDependenciesLoaded(GameEvent theEvent)
    {
        IncreaseSpeed();
    }

    public void RevertGameTimeScaleToLastKnownNormalSpeed()
    {
        //   GameTimeScale = GameTimeScaleBeforeOverriden;

        RemoveOverrideGameTimeScaleMode(0);
    }

    public void ChangeGameTimeScaleToZero()
    {
        changeGameTimeScale?.Kill();
        GameTimeScale = 0;
        isOverriden = false;
    }

    public void ChangeGameTimeScaleInTime(float theSpeed, float time, bool overrideLogic = false, bool forceOverrideMaxSpeed = false, Ease ease = Ease.Linear, Action callBack =  null)
    {
       // UnityEngine.Console.Log($"Changing Game Time Scale To {theSpeed}");

        if (isOverriden && !overrideLogic)
        {
            UnityEngine.Console.LogWarning("Trying to change game time which is already being changed");
            return;
        }

        if (!isOverriden)
        {
            GameTimeScaleBeforeOverriden = GameTimeScale;
        }

        isOverriden = true;
        changeGameTimeScale?.Kill();

        //  UnityEngine.Console.Log($"Take From {GameTimeScale} to {theSpeed} in {time} time");

        if (time == 0)
        {
            if (theSpeed <= gameManager.GetMaximumSpeed || forceOverrideMaxSpeed)
            {
                GameTimeScale = theSpeed;
            }
        }
        else
        {
            changeGameTimeScale = DOTween.To(() => GameTimeScale, (x) => { if (x <= gameManager.GetMaximumSpeed || forceOverrideMaxSpeed) GameTimeScale = x; }, theSpeed, time).SetUpdate(UpdateType.Fixed).SetEase(ease).OnComplete(()=> {
                callBack?.Invoke();
            });
        }
    }

    private void HandleGamePlayStarted(GameEvent gameEvent)
    {
        isInitialized = true;
    }

    public IEnumerator ResetTimeScaleValuesAfterDelay(float[] _timeScaleValues, float[] _delaysToShiftTimeScale)
    {
        for (int i = 0; i < _timeScaleValues.Length; i++)
        {
            bool isLockedTimeScale = GameManager.GameIsPaused || GameManager.IsTimeScaleLocked;
            //   yield return new WaitWhile(() => (GameManager.GameIsPaused || GameManager.IsTimeScaleLocked));

            if (!isLockedTimeScale)
            {
                Time.timeScale = _timeScaleValues[i];
            }

            yield return new WaitForSecondsRealtime(_delaysToShiftTimeScale[i]);
        }

        //yield return new WaitWhile(() => (GameManager.GameIsPaused || GameManager.IsTimeScaleLocked));

        bool _isLockedTimeScale = GameManager.GameIsPaused || GameManager.IsTimeScaleLocked;

        if (!_isLockedTimeScale)
        {
            Time.timeScale = 1;
        }
    }

    public void RemoveOverrideGameTimeScaleMode(float time, Ease ease = Ease.Linear)
    {
        changeGameTimeScale?.Kill();

        if (time == 0)
        {
            GameTimeScale = GameTimeScaleBeforeOverriden;
            isOverriden = false;
        }
        else
        {
            changeGameTimeScale = DOTween.To(() => GameTimeScale, (x) => { GameTimeScale = x; }, GameTimeScaleBeforeOverriden, time).OnComplete(() => { isOverriden = false; }).SetUpdate(UpdateType.Fixed).SetEase(ease);
        }
    }

    public void IncreaseSpeed()
    {

        // anyinitialspeed means from which value certain speed will start
        // overallmultiplier is from multipling all speed by same value
        // max speed is th elimit
        // speed multiplier are for all specific speeds
 

        if (!isOverriden && isInitialized)
        {
            float t = Mathf.Clamp01(MyExtensions.NormalElapseTime(ref my, 450f));
            GameTimeScale = Mathf.Lerp(gameManager.GetStartingSpeed, gameManager.GetMaximumSpeed, MyExtensions.EaseOutCubic(t));
        }

        PlayerSharedData.ForwardSpeed = GetForwardSpeedBasedOnSpecificGameTimeScale(GameTimeScale);


        // Player sideway speed
        // PlayerSharedData.SidewaysSpeed = PlayerContainedData.PlayerData.PlayerInformation[0].SideWaysInitalSpeed / (1 + (GameTimeScaleRatio * PlayerContainedData.PlayerData.PlayerInformation[0].SideWaysMultiplier));
        PlayerSharedData.SidewaysSpeed = GetSideWaysSpeedBasedOnSpecificGameTimeScale(GameTimeScale);

        // Player slide speed
        PlayerSharedData.SlideSpeed = PlayerContainedData.PlayerData.PlayerInformation[0].SlideInitialSpeed / (Mathf.Pow(GameTimeScale, .42f) * PlayerContainedData.PlayerData.PlayerInformation[0].SlideMultiplier) ;

        Debug.Log(GameTimeScale);

    }

    public float GetForwardSpeedBasedOnSpecificGameTimeScale(float timeScaleSpecific)
    {
        return PlayerContainedData.PlayerData.PlayerInformation[0].ForwardSpeedInitialValue * PlayerContainedData.PlayerData.PlayerInformation[0].ForwarspeedMultiplier * timeScaleSpecific;
    }

    public float GetForwardSpeedBasedOnCurrentOriginalTimeScale()
    {
        return PlayerContainedData.PlayerData.PlayerInformation[0].ForwardSpeedInitialValue * PlayerContainedData.PlayerData.PlayerInformation[0].ForwarspeedMultiplier * (Application.isPlaying ? GameTimeScaleBeforeOverriden :  gameManager.GetStartingSpeed);
    }

    public float GetSideWaysSpeedBasedOnSpecificGameTimeScale(float timeScaleSpecific)
    {
        return PlayerContainedData.PlayerData.PlayerInformation[0].SideWaysInitalSpeed / (timeScaleSpecific * PlayerContainedData.PlayerData.PlayerInformation[0].SideWaysMultiplier);
    }

    public float GetSideWaysSpeedBasedOnCurrentOriginalTimeScale()
    {
        return GetSideWaysSpeedBasedOnSpecificGameTimeScale(GameTimeScaleBeforeOverriden);
    }

    public float GetSideWaysSpeedSynchronizedWithForwardSpeed()
    {
        return GetSideWaysSpeedBasedOnSpecificGameTimeScale(GameTimeScale);
    }

    public float GetGameTimeScaleBasedOnSpecificVelocity(float velMag)
    {
        float scale = velMag / (PlayerContainedData.PlayerData.PlayerInformation[0].ForwardSpeedInitialValue * PlayerContainedData.PlayerData.PlayerInformation[0].ForwarspeedMultiplier);
        return scale;
    }
}