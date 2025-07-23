using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TheKnights.SaveFileSystem;
using Unity.Services.Analytics;

[CreateAssetMenu(fileName = "DifficultyScaleSO", menuName = "ScriptableObjects/Encounters/DifficultyScaleSO")]
public class DifficultyScaleSO : ScriptableObject
{
    #region Variables
    //[Header("Current Stats")]
    public float currentPlayerDifficultyRating // This considers the cases in which the intended one is overriden
    {
        get
        {
            //return 1000;
            return GetDifficultyRatingAccordingToCurrentScenario();
        }
    }
    //[Space]
    public float currentIntendedPlayerDifficultyRating // Throughout all sessions
    {
        get
        {
            return saveManager.MainSaveFile.currentIntendedPlayerDifficultyRating;
        }
        set
        {
            saveManager.MainSaveFile.currentIntendedPlayerDifficultyRating = Mathf.Clamp(value, difficultyRatingMinimumValue, difficultyRatingMaximumValue);
        }
    }

    public float CurrentIntendedDifficultyToMaxRatioPercent => (currentIntendedPlayerDifficultyRating / difficultyRatingMaximumValue) * 100f;

    [ReadOnly] [SerializeField] private float currentIndendedDifficulty_ReadOnly;

    public float GetMinimumDifficultyValue => difficultyRatingMinimumValue;
    public float GetMaximumDifficultyValue => difficultyRatingMaximumValue;


    [Header("Current Stats")]
    [Space]
    private float totalContinousDistanceCovered; // Limited to the session
    //[Space]
    public bool isSkillPlacement
    {
        get
        {
            return saveManager.MainSaveFile.isSkillPlacement;
        }
        set
        {
            saveManager.MainSaveFile.isSkillPlacement = value;
        }
    }
    private float highestSkillPlacementDifficultyRating
    {
        get
        {
            return saveManager.MainSaveFile.highestSkillPlacementDifficultyRating;
        }
        set
        {
            saveManager.MainSaveFile.highestSkillPlacementDifficultyRating = value;
        }
    }
    [Space]
    private float rampBuildingDifficultyRatingLerp; // Limited to the session
    [Space]
    private bool isDifficultyFluctuating; // Limited to the session
    private float fluctuationDifficultyLerp; // Limited to the session
    private float totalDifficultyFluctuationDistanceCovered; // Limited to the session
    private float difficultyFluctuationCurrentDistanceThreshold; // Limited to the session
    private float currentDeviatedDifficultyRating; // Limited to the session
    private float difficultyFluctuationCurrentLength; // Limited to the session
    private float fluctuationStartingPointZPosition; // Limited to the session
    //[Space]
    private bool isPlayerInGoalPowerupState // Limited to the session
    {
        get
        {
            return (isPlayerInAirplaneState || isPlayerInThrustState || isPlayerInBoostingState);
        }
    }
    private bool isPlayerInAirplaneState; // Limited to the session
    private bool isPlayerInThrustState; // Limited to the session
    private bool isPlayerInBoostingState; // Limited to the session

    [Space(20)]

    [Header("Difficulty Dynamics")]
    [Header("General")]
    public float difficultyRatingSamplingFrequency = 30f;
    private const float difficultyRatingMinimumValue = 0f;
    private const float difficultyRatingMaximumValue = 100f;
    [Header("Increase Across Distance")]
    [SerializeField] private float difficultyRatingIncreaseRatePerUnityUnits = 0.01f;
    [Header("Decrease on Failure")]
    [SerializeField] private float playerCrashDifficultyRatingDecrease = 5f;
    [Header("Decrease After Inactivity")]
    [Tooltip("Time Period In Hours")]
    [SerializeField] private float ratingDecrementInactivityTimePeriodThreshold = 24f; // In hours
    [SerializeField] private float ratingDecrementAfterInactivityTimePeriod = 5f;
    [Header("Increase Exponentially Across Continous Distance")]
    [SerializeField] private float distanceRequiredToReachThirdQuarterOfTheScale = 3500f; // Distance required to reach 75% value of the multiplier. If multiplier goes from 1 to 2, after this much distance, the multipler would have a value of 1.75
    [SerializeField] private float continousDistanceMaxMultiplier = 1.5f; // This specifies the max value the continous distance multiplier can have. Ranges from 1 to the set value. Reaches the 75% mark of set value in the same amount of distance specified in 'distanceRequiredToReachThirdQuarterOfTheScale'.

    [Header("Skill Placement Criteria")]
    [SerializeField] private float skillPlacementMultiplier = 5f;

    [Space(20)]

    [Header("Difficulty Curve")]
    [Header("Range (Variation)")]
    [SerializeField] private float rangeLengthForObstacleSpawning = 5f;
    [Header("Wall Climb Building Transition Downtime")]
    [SerializeField] private float rampBuildingDifficultyRating = 10f; // ----- DUMMY VALUE -----
    [SerializeField] private float rampBuildingDifficultyEasingDistance = 100f; // 'rampBuildingDifficultyEasingOffset' inclusive
    [SerializeField] private float rampBuildingDifficultyEasingOffset = 30f;
    [Header("Difficulty Spikes and Dips")]
    [SerializeField] private float difficultyFluctuationMinimumDistanceThreshold = 60f;
    [SerializeField] private float difficultyFluctuationMaximumDistanceThreshold = 200f;
    [SerializeField] private float difficultyFluctuationMinimumLength = 30f;
    [SerializeField] private float difficultyFluctuationMaximumLength = 60f;
    [SerializeField] private float difficultyFluctuationMaximumDifficultyDeviation = 30f;

    [Space(20)]

    [Header("References")]
    [Header("Events")]
    [SerializeField] private GameEvent playerHasCrashedEvent;
    [SerializeField] private GameEvent inactivityPeriodUpdatedEvent;
    [SerializeField] private GameEvent playerEnteredAirplanePowerupEvent;
    [SerializeField] private GameEvent playerExitedAirplanePowerupEvent;
    [SerializeField] private GameEvent playerEnteredThrustPowerupEvent;
    [SerializeField] private GameEvent playerExitedThrustPowerupEvent;
    [SerializeField] private GameEvent playerStartedBoostingEvent;
    [SerializeField] private GameEvent playerStoppedBoostingEvent;
    [Header("Other")]
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private TimeManagerSO timeManager;
    [SerializeField] private PlayerSharedData playerSharedData;
    [SerializeField] private EnvironmentData environmentData;

    private const float difficultySampleForAnalytics = 10;
    private int currentDifficultySampleForAnalytics;
    #endregion

    #region Unity Callbacks
    private void OnEnable()
    {
        SceneManager.activeSceneChanged += HandleActiveSceneChanged;

        playerHasCrashedEvent.TheEvent.AddListener(HandlePlayerHasCrashed);
        inactivityPeriodUpdatedEvent.TheEvent.AddListener(HandleInactivityPeriodUpdated);
        playerEnteredAirplanePowerupEvent.TheEvent.AddListener(HandlePlayerEnteredAirplanePowerup);
        playerExitedAirplanePowerupEvent.TheEvent.AddListener(HandlePlayerExitedAirplanePowerup);
        playerEnteredThrustPowerupEvent.TheEvent.AddListener(HandlePlayerEnteredThrustPowerup);
        playerExitedThrustPowerupEvent.TheEvent.AddListener(HandlePlayerExitedThrustPowerup);
        playerStartedBoostingEvent.TheEvent.AddListener(HandlePlayerStartedBoosting);
        playerStoppedBoostingEvent.TheEvent.AddListener(HandlePlayerStoppedBoosting);
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= HandleActiveSceneChanged;

        playerHasCrashedEvent.TheEvent.RemoveListener(HandlePlayerHasCrashed);
        inactivityPeriodUpdatedEvent.TheEvent.RemoveListener(HandleInactivityPeriodUpdated);
        playerEnteredAirplanePowerupEvent.TheEvent.RemoveListener(HandlePlayerEnteredAirplanePowerup);
        playerExitedAirplanePowerupEvent.TheEvent.RemoveListener(HandlePlayerExitedAirplanePowerup);
        playerEnteredThrustPowerupEvent.TheEvent.RemoveListener(HandlePlayerEnteredThrustPowerup);
        playerExitedThrustPowerupEvent.TheEvent.RemoveListener(HandlePlayerExitedThrustPowerup);
        playerStartedBoostingEvent.TheEvent.AddListener(HandlePlayerStartedBoosting);
        playerStoppedBoostingEvent.TheEvent.AddListener(HandlePlayerStoppedBoosting);
    }
    #endregion

    #region Initialization & Reset

    public void InitializeCurrentDifficultySample()
    {
        int diffSampleInt = (int)(currentIntendedPlayerDifficultyRating / difficultySampleForAnalytics);
        currentDifficultySampleForAnalytics = diffSampleInt;
    }

    private void ResetScriptableObject()
    {
        isPlayerInAirplaneState = false;
        isPlayerInBoostingState = false;
        isPlayerInThrustState = false;

        ResetTotalContinousDistanceCovered();
        ResetRampBuildingDifficultyRatingLerp();
        ResetDifficultyFluctuationValue();

        InitializeDifficultyFluctuationValues();
    }

    private void ResetTotalContinousDistanceCovered()
    {
        totalContinousDistanceCovered = 0;
    }

    private void ResetRampBuildingDifficultyRatingLerp()
    {
        rampBuildingDifficultyRatingLerp = 0; // 0 means it isn't counting ramp buildings in its calculations
    }

    private void ResetDifficultyFluctuationValue()
    {
        fluctuationDifficultyLerp = 0;
        totalDifficultyFluctuationDistanceCovered = 0;
        fluctuationStartingPointZPosition = 0;
        currentDeviatedDifficultyRating = 0;
        isDifficultyFluctuating = false;
    }

    private void InitializeDifficultyFluctuationValues()
    {
        difficultyFluctuationCurrentDistanceThreshold = UnityEngine.Random.Range(difficultyFluctuationMinimumDistanceThreshold, difficultyFluctuationMaximumDistanceThreshold);
        difficultyFluctuationCurrentLength = UnityEngine.Random.Range(difficultyFluctuationMinimumLength, difficultyFluctuationMaximumLength);
    }
    #endregion

    #region Difficulty Rating and Range
    public void OnDifficultyRatingUpdate(float distanceCoveredSinceTheLastRatingUpdate) // --- Could be more accurate if currentplayerDifficultyRating is shifted x units back for obstacle generation offset but can only be done after encounter generation is finalized ---
    {
        UpdateRampBuildingDifficultyLerp();
        UpdateDifficultyFluctuationDistance(distanceCoveredSinceTheLastRatingUpdate);

        if ((rampBuildingDifficultyRatingLerp > 0 && environmentData.firstRampBuildingHasBeenSpawned && currentIntendedPlayerDifficultyRating > rampBuildingDifficultyRating) || // Don't increment difficulty rating during ramp building difficulty rating easings only if currentIntendedRating is greater than rampBuildingRating
            isPlayerInGoalPowerupState) // Don't increment difficulty rating during goal powerups
            return;

            // {
            ChangeTotalContinousDistanceCovered(distanceCoveredSinceTheLastRatingUpdate);
            ChangeDiffiultyRating(distanceCoveredSinceTheLastRatingUpdate); // Change difficulty rating at the end because the functions above calculate appropriate multipliers for it
       // }
    }

    private float GetDifficultyRatingAccordingToCurrentScenario() // Not to be confused with 'CurrentIntendedDifficultyRating'
    {
        if (rampBuildingDifficultyRatingLerp > 0 && environmentData.firstRampBuildingHasBeenSpawned)
        {
            return Mathf.Lerp(currentIntendedPlayerDifficultyRating, rampBuildingDifficultyRating, rampBuildingDifficultyRatingLerp);
        }

        if (isDifficultyFluctuating) // This never conflicts with 'rampBuildingDifficultyRatingLerp'
        {
            return Mathf.Lerp(currentIntendedPlayerDifficultyRating, currentDeviatedDifficultyRating, fluctuationDifficultyLerp);
        }

        return currentIntendedPlayerDifficultyRating;
    }

    public float GetRandomPointOnCurrentDifficultyRange()
    {
        Debug.LogError("Current : "+currentPlayerDifficultyRating);
        if (currentPlayerDifficultyRating < rangeLengthForObstacleSpawning)
        {
            return UnityEngine.Random.Range(0f, rangeLengthForObstacleSpawning);
        }
        
        return UnityEngine.Random.Range(currentPlayerDifficultyRating - rangeLengthForObstacleSpawning, currentPlayerDifficultyRating);
    }
    #endregion

    #region Difficulty Dynamics
    private void ChangeDiffiultyRating(float ratingChange, bool takesDifficultyRatingIncreaseRatePerDistanceMultiplierIntoAccount = true, bool takesContinousDistanceMultiplierIntoAccount = true, bool takesSkillPlacementMultiplierIntoAccount = true)
    {
        float ratingChangeMultiplier = (takesDifficultyRatingIncreaseRatePerDistanceMultiplierIntoAccount ? difficultyRatingIncreaseRatePerUnityUnits : 1) * (takesContinousDistanceMultiplierIntoAccount ? GetContinousDistanceMultiplier() : 1) * (isSkillPlacement && takesSkillPlacementMultiplierIntoAccount ? skillPlacementMultiplier : 1);
        currentIntendedPlayerDifficultyRating += ratingChange * ratingChangeMultiplier;

        currentIndendedDifficulty_ReadOnly = currentIntendedPlayerDifficultyRating;

        int diffSampleInt = (int)(currentIntendedPlayerDifficultyRating / difficultySampleForAnalytics);

       if(currentDifficultySampleForAnalytics != diffSampleInt)
        {
            currentDifficultySampleForAnalytics = diffSampleInt;

            if (diffSampleInt != 0)
            {
                AnalyticsManager.CustomData("GamePlayScreen_DifficultyReached", new Dictionary<string, object> { { "GamePlayDifficulty", (diffSampleInt * difficultySampleForAnalytics) } });
               // UnityEngine.Console.Log($"Sending Difficulty Reached {(diffSampleInt * difficultySampleForAnalytics)}");
            }
        }

    }

    private void ChangeTotalContinousDistanceCovered(float distanceCoveredThisFrame)
    {
        totalContinousDistanceCovered += distanceCoveredThisFrame;
    }

    private void UpdateRampBuildingDifficultyLerp()
    {
        float absoluteDistanceBetweenPlayerAndBuilding = Mathf.Abs(environmentData.distanceBetweenCurrentRampBuildingAndPlayer);
        rampBuildingDifficultyRatingLerp = Mathf.Clamp01(((rampBuildingDifficultyEasingDistance) - (absoluteDistanceBetweenPlayerAndBuilding)) / (rampBuildingDifficultyEasingDistance - rampBuildingDifficultyEasingOffset)); // = 1 when is closest to building and = 0 when out of building easing distance
    }

    private float GetContinousDistanceMultiplier() // Gives more accurate results on higher sampling rates
    {
        // To visualize this graph, go to https://www.desmos.com/calculator and put this equation in
        return (((1 / (continousDistanceMaxMultiplier - 1)) + 1) - (1 / (((totalContinousDistanceCovered * 3) / distanceRequiredToReachThirdQuarterOfTheScale) + 1))) * (continousDistanceMaxMultiplier - 1);
    }
    #endregion

    #region Difficulty Curve
    private void UpdateDifficultyFluctuationDistance(float distanceCoveredSinceTheLastRatingUpdate)
    {
        if (isSkillPlacement) // No spikes and dips on placement
            return;

        if (!isDifficultyFluctuating)
        {
            totalDifficultyFluctuationDistanceCovered += distanceCoveredSinceTheLastRatingUpdate;

            if (totalDifficultyFluctuationDistanceCovered > difficultyFluctuationCurrentDistanceThreshold)
            {
                float spikeStartingPointZPositionOvershoot = totalDifficultyFluctuationDistanceCovered - difficultyFluctuationCurrentDistanceThreshold;
                totalDifficultyFluctuationDistanceCovered -= difficultyFluctuationCurrentDistanceThreshold;

                InitializeDifficultyFluctuationValues();

                bool isRampBuildingAheadOfPlayer = environmentData.distanceBetweenCurrentRampBuildingAndPlayer > 0 ? true : false;
                float safeDistanceNearRampBuildingForDifficultyFluctuation = rampBuildingDifficultyEasingDistance + (isRampBuildingAheadOfPlayer ? difficultyFluctuationCurrentLength : 0); // different when player is behind and when the player is ahead of the ramp building

                if (environmentData.firstRampBuildingHasBeenSpawned ? (Mathf.Abs(environmentData.distanceBetweenCurrentRampBuildingAndPlayer) > safeDistanceNearRampBuildingForDifficultyFluctuation) : true)
                {
                    CalculateDifficultyFluctuation(); // Takes 'currentIntendedPlayerDifficultyRating' into account so it can't be pre-calculated

                    fluctuationStartingPointZPosition = playerSharedData.PlayerTransform.position.z - spikeStartingPointZPositionOvershoot;
                    isDifficultyFluctuating = true;
                }
            }
        }

        if (isDifficultyFluctuating) // 'else' is not used on purpose
        {
            float distanceBetweenPlayerAndSpikeMaxDisplacementPoint = Mathf.Abs((fluctuationStartingPointZPosition + (difficultyFluctuationCurrentLength / 2)) - playerSharedData.PlayerTransform.position.z);
            fluctuationDifficultyLerp = Mathf.Clamp01(((difficultyFluctuationCurrentLength / 2) - distanceBetweenPlayerAndSpikeMaxDisplacementPoint) / (difficultyFluctuationCurrentLength / 2)); // = 1 when is closest to building and = 0 when out of building easing distance

            if (playerSharedData.PlayerTransform.position.z > fluctuationStartingPointZPosition + difficultyFluctuationCurrentLength)
            {
                isDifficultyFluctuating = false;
            }
        }
    }

    private void CalculateDifficultyFluctuation()
    {
        float fluctuationDirection = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
        float difficultyFluctuationCalculatedDifficultyDeviation = Mathf.Clamp(currentIntendedPlayerDifficultyRating + (difficultyFluctuationMaximumDifficultyDeviation * fluctuationDirection), difficultyRatingMinimumValue, difficultyRatingMaximumValue) - currentIntendedPlayerDifficultyRating;
        currentDeviatedDifficultyRating = currentIntendedPlayerDifficultyRating + UnityEngine.Random.Range(0, difficultyFluctuationCalculatedDifficultyDeviation);
    }
    #endregion

    #region Event Handling
    private void HandleActiveSceneChanged(Scene replacedScene, Scene nextScene)
    {
        ResetScriptableObject();
    }

    private void HandlePlayerHasCrashed(GameEvent gameEvent)
    {
        //if (!GameManager.IsGameStarted)
        //    return;

        if (isSkillPlacement)
        {
            if (currentIntendedPlayerDifficultyRating > highestSkillPlacementDifficultyRating)
            {
                highestSkillPlacementDifficultyRating = currentIntendedPlayerDifficultyRating;
                ChangeDiffiultyRating(-playerCrashDifficultyRatingDecrease, false, false, true); // We are multiplying skill placement multiplier over here
            }
            else
            {
                UnityEngine.Console.Log("Skill placement Finished");
                isSkillPlacement = false;
                AnalyticsManager.CustomData("GamePlayScreen_SkillPlaced", new Dictionary<string, object> { { "GamePlayDifficulty", currentIntendedPlayerDifficultyRating } });
                ChangeDiffiultyRating(highestSkillPlacementDifficultyRating - currentIntendedPlayerDifficultyRating, false, false, false); // This simply does the following: currentIntendedPlayerDifficultyRating = currentIntendedPlayerDifficultyRating
            }
        }
        else
        {
            ChangeDiffiultyRating(-playerCrashDifficultyRatingDecrease, false, false, false);
        }

        ResetTotalContinousDistanceCovered();
    }

    private void HandleInactivityPeriodUpdated(GameEvent gameEvent)
    {
        if (timeManager.inactivityPeriod >= ratingDecrementInactivityTimePeriodThreshold)
        {
            if (!isSkillPlacement)
            {
                ChangeDiffiultyRating(-playerCrashDifficultyRatingDecrease, false, false, false);
            }
        }
    }

    private void HandlePlayerEnteredAirplanePowerup(GameEvent gameEvent)
    {
        isPlayerInAirplaneState = true;
    }

    private void HandlePlayerExitedAirplanePowerup(GameEvent gameEvent)
    {
        isPlayerInAirplaneState = false;
    }

    private void HandlePlayerEnteredThrustPowerup(GameEvent gameEvent)
    {
        isPlayerInThrustState = true;
    }

    private void HandlePlayerExitedThrustPowerup(GameEvent gameEvent)
    {
        isPlayerInThrustState = false;
    }

    private void HandlePlayerStartedBoosting(GameEvent gameEvent)
    {
        isPlayerInBoostingState = true;
    }

    private void HandlePlayerStoppedBoosting(GameEvent gameEvent)
    {
        isPlayerInBoostingState = false;
    }
    #endregion

    
}
