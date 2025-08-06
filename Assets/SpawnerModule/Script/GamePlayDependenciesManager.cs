using System.Collections;
using System.Collections.Generic;
using TheKnights.SceneLoadingSystem;
using UnityEngine;


public class GamePlayDependenciesManager : MonoBehaviour
{
    [SerializeField] private SceneLoader sceneLoader;
    [SerializeField] private GameEvent[] dependenciesEvents;
    [SerializeField] private GameEvent onDependenciesLoaded;

    private readonly Dictionary<GameEvent, bool> loadedDependenciesDictionary = new Dictionary<GameEvent, bool>();

    private void Awake()
    {
        for (int i = 0; i < dependenciesEvents.Length; i++)
        {
            GameEvent depencencyEvent = dependenciesEvents[i];
            depencencyEvent.TheEvent.AddListener(DependencyLoaded);
            loadedDependenciesDictionary.Add(depencencyEvent, false);
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < dependenciesEvents.Length; i++)
        {
            dependenciesEvents[i].TheEvent.RemoveListener(DependencyLoaded);
        }
    }

    private void DependencyLoaded(GameEvent gameEvent)
    {
        loadedDependenciesDictionary[gameEvent] = true;

        foreach (var loadState in loadedDependenciesDictionary.Values)
        {
            if (!loadState)
                return;
        }

        Debug.Log("Dependency Loaded");
        onDependenciesLoaded.RaiseEvent();
        StartCoroutine(DisableLoadingCanvas());
    }
    private static bool firstTimeForThisSession = true;
    private IEnumerator DisableLoadingCanvas()
    {
        yield return new WaitForSecondsRealtime(.5f);
        sceneLoader.TurnOffLoadingCanvas();

        AnalyticsManager.CustomData("MainMenuScreen_Load");

        if (firstTimeForThisSession)
        {
            firstTimeForThisSession = false;
            AnalyticsManager.CustomData("MainMenuScreen_SessionLoad");
        }

        this.enabled = false;

     
    }
}