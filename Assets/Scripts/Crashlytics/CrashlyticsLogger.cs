using Firebase.Crashlytics;
using UnityEngine;

public static partial class GameStateKeys
{
    public const string ActiveMainState = "ActiveMainState";
    public const string ActiveGamePlayState = "ActiveGamePlayState";
    public const string ActiveWindowState = "ActiveWindowState";
    public const string ActivePopupState = "ActivePopupState";
}

public static partial class GameStateValues
{
    public const string None = "None";

    #region MainStateValues

    public const string MenuMainState = "MenuMainState";
    public const string GamePlayMainState = "GamePlayMainState";
    public const string LoadingMainState = "LoadingMainState";
    public const string PluginsInitMainState = "PluginsInitMainState";

    #endregion MainStateValues

    #region GamePlayStateValues

    public const string CutSceneGamePlayState = "CutSceneGamePlayState";
    public const string GameStartedGamePlayState = "GameStartedGamePlayState";

    #endregion GamePlayStateValues
}

[CreateAssetMenu(fileName = "CrashlyticsLogger", menuName = "ScriptableObjects/Crashlytics/CrashlyticsLogger")]
public class CrashlyticsLogger : ScriptableObject
{
    public static string CurrentMainState { get; private set; }
    public static string CurrentGamePlayState { get; private set; }
    public static string CurrentWindowState { get; private set; }
    public static string CurrentPopupState { get; private set; }

    [SerializeField] private GameEvent onSceneLoadingStarted;
    [SerializeField] private GameEvent onDependenciesLoaded;
    [SerializeField] private GameEvent onCutSceneStarted;

    [SerializeField] private GameEvent cutSceneFinished;

    private string currentylyOpenedPopup;
    private string currentlyOpenedWindow;

    private void OnEnable()
    {
        FireBaseInitializer.OnFireBaseInitialized += SubscribeToEvents;
      //  SetMainStateAsPluginsInitialization();
    }

    #region EventsSubscribtion

    private void SubscribeToEvents()
    {
        onSceneLoadingStarted.TheEvent.AddListener(SetMainStateAsLoading);
        onDependenciesLoaded.TheEvent.AddListener(SetMainStateAsMenu);
        onDependenciesLoaded.TheEvent.AddListener(SetGamePlayStateAsCutScene);
        onCutSceneStarted.TheEvent.AddListener(SetMainStateAsGamePlay);
        cutSceneFinished.TheEvent.AddListener(SetGamePlayStateAsGameStarted);

        UIScreenEvents.OnScreenOperationEventBeforeAnimation.AddListener(HandleScreenBeforeOperation);
    }

    private void DeSubscribeToEvents()
    {
        onSceneLoadingStarted.TheEvent.RemoveListener(SetMainStateAsLoading);
        onDependenciesLoaded.TheEvent.RemoveListener(SetMainStateAsMenu);
        onDependenciesLoaded.TheEvent.RemoveListener(SetGamePlayStateAsCutScene);
        onCutSceneStarted.TheEvent.RemoveListener(SetMainStateAsGamePlay);
        cutSceneFinished.TheEvent.AddListener(SetGamePlayStateAsGameStarted);

        UIScreenEvents.OnScreenOperationEventBeforeAnimation.RemoveListener(HandleScreenBeforeOperation);
    }

    #endregion EventsSubscribtion

    #region SetMainStates

    private void SetMainStateAsLoading(GameEvent gameEvent)
    {
        SetMainGameState(GameStateKeys.ActiveMainState, GameStateValues.LoadingMainState);
    }

    private void SetMainStateAsMenu(GameEvent gameEvent)
    {
        SetMainGameState(GameStateKeys.ActiveMainState, GameStateValues.MenuMainState);
    }

    private void SetMainStateAsGamePlay(GameEvent gameEvent)
    {
        SetMainGameState(GameStateKeys.ActiveMainState, GameStateValues.GamePlayMainState);
    }

    private void SetMainStateAsPluginsInitialization()
    {
        SetMainGameState(GameStateKeys.ActiveMainState, GameStateValues.PluginsInitMainState);
    }

    public void SetMainGameState(string key, string value)
    {
        SetGameState(key, value);
        CurrentMainState = value;
    }

    #endregion SetMainStates

    #region SetGamePlayStates

    private void SetGamePlayStateAsCutScene(GameEvent gameEvent)
    {
        SetGamePlayState(GameStateKeys.ActiveGamePlayState, GameStateValues.CutSceneGamePlayState);
    }

    private void SetGamePlayStateAsGameStarted(GameEvent gameEvent)
    {
        SetGamePlayState(GameStateKeys.ActiveGamePlayState, GameStateValues.GameStartedGamePlayState);
    }

    public void SetGamePlayState(string key, string value)
    {
        SetGameState(key, value);
        CurrentGamePlayState = value;
    }

    #endregion SetGamePlayStates

    #region SetWindowState

    private void HandleScreenBeforeOperation(string screenID, ScreenOperation screenOperation, ScreenType screenType)
    {
        if (screenOperation == ScreenOperation.Close)
        {
            if (screenType == ScreenType.Window && screenID == currentlyOpenedWindow)
            {
                SetWindowState(GameStateKeys.ActiveWindowState, GameStateValues.None);
            }
            else if (screenType == ScreenType.PopUp && screenID == currentylyOpenedPopup)
            {
                SetPopupState(GameStateKeys.ActivePopupState, GameStateValues.None);
            }

            return;
        }

        if (screenType == ScreenType.Window)
        {
            currentlyOpenedWindow = screenID;
            SetWindowState(GameStateKeys.ActiveWindowState, screenID);
        }
        else if (screenType == ScreenType.PopUp)
        {
            currentylyOpenedPopup = screenID;
            SetPopupState(GameStateKeys.ActivePopupState, screenID);
        }
    }

    public void SetWindowState(string key, string value)
    {
        SetGameState(key, value);
        CurrentWindowState = value;
    }

    public void SetPopupState(string key, string value)
    {
        SetGameState(key, value);
        CurrentPopupState = value;
    }

    #endregion SetWindowState

    public void SetGameState(string key, string value)
    {
        Crashlytics.SetCustomKey(key, value);
    }
}