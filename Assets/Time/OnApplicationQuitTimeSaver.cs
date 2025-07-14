using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKnights.SaveFileSystem;
using System.Threading;
using Unity.Profiling;
using UnityEngine.Profiling;

public class OnApplicationQuitTimeSaver : Singleton<OnApplicationQuitTimeSaver>
{
    #region Variables
    [Header("Instances subscribing to 'inactivityPeriodUpdatedEvent' event")] // To subscribe to 'inactivityPeriodUpdatedEvent' before it is called
    [SerializeField] private DifficultyScaleSO difficultyScaleSO;

    [Header("References")]
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private TimeManagerSO timeManager;
    [SerializeField] private GameEvent inactivityPeriodUpdatedEvent;
    private const string applicationQuitTimePlayerPrefKey = "ApplicationQuitTime"; // This string should never be changed
    #endregion

    #region Unity Callbacks
    private void OnEnable()
    {
        saveManager.OnSessionLoaded.AddListener(OnSessionLoaded);
    }

    private void OnDisable()
    {
        saveManager.OnSessionLoaded.RemoveListener(OnSessionLoaded);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
            return;

        PlayerPrefs.SetString(applicationQuitTimePlayerPrefKey, timeManager.GetCurrentTimeDateInString());
    }

   
    #endregion

    #region Event Handling
    private void OnSessionLoaded()
    {
        if (PlayerPrefs.HasKey(applicationQuitTimePlayerPrefKey))
        {
            timeManager.inactivityPeriod = timeManager.GetTotalHours(timeManager.ConvertStringToDateTime(PlayerPrefs.GetString(applicationQuitTimePlayerPrefKey)));
            inactivityPeriodUpdatedEvent.RaiseEvent();

            saveManager.MainSaveFile.applicationQuitTime = PlayerPrefs.GetString(applicationQuitTimePlayerPrefKey);
        }
    }
    #endregion
}
