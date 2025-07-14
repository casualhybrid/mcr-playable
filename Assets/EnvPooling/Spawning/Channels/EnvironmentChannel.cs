using UnityEngine;
using UnityEngine.Events;
using Unity.Services.Analytics;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "EnvironmentChannel", menuName = "ScriptableObjects/Environement/EnvironmentChannel")]
public class EnvironmentChannel : ScriptableObject
{
    public UnityAction<Environment> OnPlayerEnvironmentChanged;

    public UnityAction OnRequestPauseSwitchEnvironment;

    public UnityAction OnRequestUnPauseSwitchEnvironment;

    [SerializeField] private PlayerSharedData playerSharedData;

    public void RaisePlayerEnvironmentChangedEvent(Environment enviornment)
    {
        AnalyticsManager.CustomData("GamePlayScreen_EnvironmentCompleted", new Dictionary<string, object> { { "Environment", playerSharedData.playerCurrentEnvironment.name } });

        playerSharedData.playerCurrentEnvironment = enviornment;
        OnPlayerEnvironmentChanged?.Invoke(enviornment);
    }

    public void RaiseRequestPauseSwitchEnvironmentGenerationEvent()
    {
        OnRequestPauseSwitchEnvironment.Invoke();
    }

    public void RaiseRequestUnPauseSwitchEnvironmentGenerationEvent()
    {
        OnRequestUnPauseSwitchEnvironment?.Invoke();
    }
}
