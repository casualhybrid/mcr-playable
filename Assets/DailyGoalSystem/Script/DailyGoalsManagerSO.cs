using System.Collections.Generic;
using TheKnights.SaveFileSystem;
using Unity.Services.Analytics;
using UnityEngine;


[CreateAssetMenu(fileName = "DailyGoalsManager", menuName = "DailyGoalSystem/DailyGoalsManagerSO")]
public class DailyGoalsManagerSO : ScriptableObject
{
    [SerializeField] private GamePlaySessionInventory gamePlaySessionInventory;
    [SerializeField] private InventorySystem inventoryObj;
    [SerializeField] private SaveManager saveManager;

    [HideInInspector] public int currentDay = 0;
    [HideInInspector] public string completedGoalStr;

    public TimeManagerSO timeManager;
    public DailyGoalsSO dailyGoalsObj;

    [SerializeField]
    private GameEvent wallRunEvent, dashStartedEvent, dashEndedEvent,
        shockWaveStartedEvent, shockwaveEndedEvent, trafficCarDestroyEvent, pogoStickEvent,
        aeroplaneEvent, playerHitEvent, dailyGoalCompletedEvent;//, boostEventStarted, boostEventEnded;
    [SerializeField] private GameEvent gameHasStarted;

    [SerializeField]
    private bool isDashing = default, isShockwaving = default,
        /*isBoosting = default,*/ isDailyGoalsCompleted = default;

    private string oldDate = default;

    private void OnEnable()
    {
        gameHasStarted.TheEvent.AddListener(Initialize);
    }

    private void Initialize(GameEvent gameEvent)
    {
        wallRunEvent.TheEvent.AddListener(OnWallRunning);
        dashStartedEvent.TheEvent.AddListener(OnDashStarted);
        dashEndedEvent.TheEvent.AddListener(OnDashEnded);
        shockWaveStartedEvent.TheEvent.AddListener(OnShockwaveStarted);
        shockwaveEndedEvent.TheEvent.AddListener(OnShockwaveEnded);
        trafficCarDestroyEvent.TheEvent.AddListener(OnTrafficCarDestroyed);
        pogoStickEvent.TheEvent.AddListener(OnCarInAir);
        aeroplaneEvent.TheEvent.AddListener(OnCarInAir);
        playerHitEvent.TheEvent.AddListener(OnPlayerHit);
    }

    private void SettingUpDefaultSettings()
    {
        MainSaveFile mainSave = saveManager.MainSaveFile;
        isDashing = isShockwaving = /*isBoosting =*/ isDailyGoalsCompleted = false;
        mainSave.noOfDashKills = mainSave.noOfInAir = mainSave.noOfShockwaveKills = mainSave.noOfWallRuns = mainSave.noOfHits = mainSave.noOfEnemiesDestroyed = 0;
        mainSave.goalsCompletionStatus = 0; mainSave.alreadyGoalsCompleted = 0;
    }

    #region Events Defination Region

    private void OnWallRunning(GameEvent theEvent)
    {
        gamePlaySessionInventory.noOfWallRuns += 1;

        UpdateGoalsStatus();
    }

    private void OnDashStarted(GameEvent theEvent)
    {
        isDashing = true;
    }

    private void OnDashEnded(GameEvent theEvent)
    {
        isDashing = false;
    }

    private void OnShockwaveStarted(GameEvent theEvent)
    {
        isShockwaving = true;
    }

    private void OnShockwaveEnded(GameEvent theEvent)
    {
        isShockwaving = false;
    }

    //void OnBoostStarted(GameEvent theEvent) {
    //    isBoosting = true;
    //}

    //void OnBoostEnded(GameEvent theEvent)
    //{
    //    isBoosting = false;
    //}

    private void OnPlayerHit(GameEvent theEvent)
    {
        gamePlaySessionInventory.noOfHits += 1;

        UpdateGoalsStatus();
    }

    private void OnCarInAir(GameEvent theEvent)
    {
        gamePlaySessionInventory.noOfInAir += 1;

        UpdateGoalsStatus();
    }

    private void OnTrafficCarDestroyed(GameEvent theEvent)
    {
        if (isDashing)
        {
            gamePlaySessionInventory.noOfDashKills += 1;
        }
        if (isShockwaving)
        {
            gamePlaySessionInventory.noOfShockwaveKills += 1;
        }

        gamePlaySessionInventory.noOfEnemiesDestroyed += 1;

        UpdateGoalsStatus();
    }

    #endregion Events Defination Region

    public void SetUpValuesForGame(MainSaveFile saveFileObj)
    {
        oldDate = saveFileObj.sessionDate;

        // UnityEngine.Console.Log("Saved Date = " + oldDate + " || Returned Hours = " + timeManager.GetDaysWithHours(oldDate));

        if (string.IsNullOrEmpty(oldDate) || IsDailyGoalAvailable()  )
        {
            // We have to call this function when ever new day begins
            saveFileObj.gameDailyGoals = dailyGoalsObj.PoolDataSets();
            saveFileObj.sessionDate = timeManager.GetCurrentTimeDateInString();
            SettingUpDefaultSettings();
        }
    }

    public bool IsDailyGoalAvailable()
    {
        // if will be true when no of days passed is >= 1
        //return true;    // for debuging purposes

        if (string.IsNullOrEmpty(oldDate))
            return true;

        if (timeManager.GetDaysWithHours(oldDate) >= 1)
        {
         //   UnityEngine.Console.Log("Daily Goal Is  Available");

            return true;
        }

       // UnityEngine.Console.Log("Daily Goal Is Not Available");
        return false;
    }

    private bool CheckDailyGoalsStatus()
    {
        float compValue = 0;
        for (int i = 0; i < dailyGoalsObj.genericDailyGoals.Count; i++)
        {
            compValue += Mathf.Pow(2, i);
        }

        // UnityEngine.Console.Log("Completeion Value = " + compValue);

        if (gamePlaySessionInventory.goalsCompletionStatus == (int)compValue)
        {
            return true;
        }
        return false;
    }

    public int GetCurrentGoalStatus(DailyGoalTypes goalType)
    {
        switch (goalType)
        {
            case DailyGoalTypes.WallRun:
                return saveManager.MainSaveFile.noOfWallRuns;

            case DailyGoalTypes.DashKill:
                return saveManager.MainSaveFile.noOfDashKills;

            case DailyGoalTypes.ShockwaveKill:
                return saveManager.MainSaveFile.noOfShockwaveKills;

            case DailyGoalTypes.InAir:
                return saveManager.MainSaveFile.noOfInAir;

            case DailyGoalTypes.SideHit:
                return saveManager.MainSaveFile.noOfHits;

            case DailyGoalTypes.LaneSwitch:
                return saveManager.MainSaveFile.noOfLaneSwitch;

            case DailyGoalTypes.DestroyEnemies:
                return saveManager.MainSaveFile.noOfEnemiesDestroyed;
        }
        return -1;
    }

    public int GetCurrentGoalStatusGamePlaySessionInventory(DailyGoalTypes goalType)
    {
        switch (goalType)
        {
            case DailyGoalTypes.WallRun:
                return gamePlaySessionInventory.noOfWallRuns;

            case DailyGoalTypes.DashKill:
                return gamePlaySessionInventory.noOfDashKills;

            case DailyGoalTypes.ShockwaveKill:
                return gamePlaySessionInventory.noOfShockwaveKills;

            case DailyGoalTypes.InAir:
                return gamePlaySessionInventory.noOfInAir;

            case DailyGoalTypes.SideHit:
                return gamePlaySessionInventory.noOfHits;

            case DailyGoalTypes.LaneSwitch:
                return gamePlaySessionInventory.noOfLaneSwitch;

            case DailyGoalTypes.DestroyEnemies:
                return gamePlaySessionInventory.noOfEnemiesDestroyed;
        }
        return -1;
    }

    private void UpdateGoalsStatus()
    {
        if (!isDailyGoalsCompleted)        // For real this if will help to stop code when daily goals are completed
        {
            for (int i = 0; i < dailyGoalsObj.genericDailyGoals.Count; i++)
            {
                GoalItemDataSet obj = dailyGoalsObj.genericDailyGoals[i];
                switch (obj.goalType)
                {
                    case DailyGoalTypes.WallRun:
                        if (gamePlaySessionInventory.noOfWallRuns >= obj.targetToAchive)
                        {
                            completedGoalStr = "Wall Run " + obj.targetToAchive + "/" + obj.targetToAchive;
                            GoalCompleted(i);
                        }
                        break;

                    case DailyGoalTypes.DashKill:
                        if (gamePlaySessionInventory.noOfDashKills >= obj.targetToAchive)
                        {
                            completedGoalStr = "Dash Kills " + obj.targetToAchive + "/" + obj.targetToAchive;
                            GoalCompleted(i);
                        }
                        break;

                    case DailyGoalTypes.ShockwaveKill:
                        if (gamePlaySessionInventory.noOfShockwaveKills >= obj.targetToAchive)
                        {
                            completedGoalStr = "Shockwave Kills " + obj.targetToAchive + "/" + obj.targetToAchive;
                            GoalCompleted(i);
                        }
                        break;

                    case DailyGoalTypes.InAir:
                        if (gamePlaySessionInventory.noOfInAir >= obj.targetToAchive)
                        {
                            completedGoalStr = "In Air " + obj.targetToAchive + "/" + obj.targetToAchive;
                            GoalCompleted(i);
                        }
                        break;

                    case DailyGoalTypes.SideHit:
                        if (gamePlaySessionInventory.noOfHits >= obj.targetToAchive)
                        {
                            completedGoalStr = "No of Hits " + obj.targetToAchive + "/" + obj.targetToAchive;
                            GoalCompleted(i);
                        }
                        break;

                    case DailyGoalTypes.DestroyEnemies:
                        if (gamePlaySessionInventory.noOfEnemiesDestroyed >= obj.targetToAchive)
                        {
                            completedGoalStr = "No of Enemies Destroyed " + obj.targetToAchive + "/" + obj.targetToAchive;
                            GoalCompleted(i);
                        }
                        break;
                }
            }

            if (CheckDailyGoalsStatus())
            {
                //  Here we have to tell through UI that all Daily goals has been completed
                isDailyGoalsCompleted = true;
            }
        }
    }

    public int GoalCompleted(int goalIndex)
    {
       // UnityEngine.Console.Log("Goal Completed");

        //  Here we have to tell through UI that goal has been completed
        //      (dailyGoalsObj.genericDailyGoals[goalIndex])

        gamePlaySessionInventory.goalsCompletionStatus |= (int)Mathf.Pow(2, goalIndex);

        if (gamePlaySessionInventory.goalsCompletionStatus != gamePlaySessionInventory.alreadyGoalsCompleted)
        {
            gamePlaySessionInventory.alreadyGoalsCompleted = gamePlaySessionInventory.goalsCompletionStatus;
            //    inventoryObj.UpdateKeyValues(new List<InventoryItem<int>>() { new InventoryItem<int>("AccountCoins", 100) });
            gamePlaySessionInventory.AddThisKeyToGamePlayInventory("AccountCoins", 100);

            UnityEngine.Console.Log($"Sending Daily Goal Completed");
            dailyGoalCompletedEvent.RaiseEvent();

            GoalItemDataSet goalItemDataSet = dailyGoalsObj.genericDailyGoals[goalIndex];
            DailyGoalTypes dailyGoalType = goalItemDataSet.goalType;

            // dialouge generation event can be set up here..
            AnalyticsManager.CustomData("DailyGoalsScreen_Goal_Complete", new Dictionary<string, object> { { "GoalType", dailyGoalType.ToString() }  });

        }

        return gamePlaySessionInventory.goalsCompletionStatus;
    }

    private void OnDisable()
    {
        gameHasStarted.TheEvent.RemoveListener(Initialize);

        wallRunEvent.TheEvent.RemoveListener(OnWallRunning);
        dashStartedEvent.TheEvent.RemoveListener(OnDashStarted);
        dashEndedEvent.TheEvent.RemoveListener(OnDashEnded);
        shockWaveStartedEvent.TheEvent.RemoveListener(OnShockwaveStarted);
        shockwaveEndedEvent.TheEvent.RemoveListener(OnShockwaveEnded);
        trafficCarDestroyEvent.TheEvent.RemoveListener(OnTrafficCarDestroyed);
        pogoStickEvent.TheEvent.RemoveListener(OnCarInAir);
        aeroplaneEvent.TheEvent.RemoveListener(OnCarInAir);
        playerHitEvent.TheEvent.RemoveListener(OnPlayerHit);
    }
}