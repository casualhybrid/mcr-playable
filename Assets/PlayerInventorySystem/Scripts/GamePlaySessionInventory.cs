using System.Collections.Generic;
using TheKnights.SaveFileSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "GamePlaySessionInventory", menuName = "ScriptableObjects/GamePlaySessionInventory")]
public class GamePlaySessionInventory : ScriptableObject
{
    public SaveManager saveManagerObj;

    public class GamePlayInventoryItemAdded : UnityEvent<IntegerValueItem>
    { }

    public GamePlayInventoryItemAdded OnGamePlayInventoryItemAdded = new GamePlayInventoryItemAdded();

    public List<IntegerValueItem> intKeyItems { get; private set; }
    public Dictionary<int, int> CharacterFigurinesPicked { get; private set; } = new Dictionary<int, int>();

    private HashSet<float> gamePlayBackGroundAwardsAcquired;

    public int PlayerCurrentLevel { get; set; }
     public float PlayerCurrentXP { get; set; }

    public int noOfWallRuns { get; set; }
    public int noOfDashKills { get; set; }
    public int noOfShockwaveKills { get; set; }
    public int noOfInAir { get; set; }
    public int noOfHits { get; set; }
    public int noOfEnemiesDestroyed { get; set; }
    public int noOfLaneSwitch { get; set; }
    public int goalsCompletionStatus { get; set; }
    public int alreadyGoalsCompleted { get; set; }

    [SerializeField] private InventorySystem playerInventory;
    [SerializeField] private GamePlaySessionData gamePlaySessionData;
    [SerializeField] private GameEvent InitializeSessionItems;

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += HandleSceneChagned;
        InitializeSessionItems.TheEvent.AddListener(InitializeGamePlaySessionItems);
    }

    private void OnDisable()
    {
        InitializeSessionItems.TheEvent.RemoveListener(InitializeGamePlaySessionItems);
        SceneManager.activeSceneChanged -= HandleSceneChagned;
    }

    private void HandleSceneChagned(Scene arg0, Scene arg1)
    {

        intKeyItems?.Clear();
        gamePlayBackGroundAwardsAcquired?.Clear();
        CharacterFigurinesPicked?.Clear();

        PlayerCurrentLevel = 0;
        PlayerCurrentXP = 0;

        noOfWallRuns = 0;
        noOfDashKills = 0;
        noOfShockwaveKills = 0;
        noOfInAir = 0;
        noOfHits = 0;
        noOfEnemiesDestroyed = 0;
        noOfLaneSwitch = 0;
        goalsCompletionStatus = 0;
        alreadyGoalsCompleted = 0;
    }

    private void InitializeGamePlaySessionItems(GameEvent gameEvent)
    {
        intKeyItems = new List<IntegerValueItem>();

        // Copy of inventory items
        for (int i = 0; i < playerInventory.intKeyItems.Count; i++)
        {
            IntegerValueItem integerValueItem = new IntegerValueItem();
            integerValueItem.itemValue = 0;
            integerValueItem.itemName = playerInventory.intKeyItems[i].itemName;

            intKeyItems.Add(integerValueItem);
        }

        // Copy of gameplay rewards
        gamePlayBackGroundAwardsAcquired = new HashSet<float>(saveManagerObj.MainSaveFile.gamePlayBackGroundAwardsAcquired);

        // Copy of goals
        noOfWallRuns = saveManagerObj.MainSaveFile.noOfWallRuns;
        noOfDashKills = saveManagerObj.MainSaveFile.noOfDashKills;
        noOfShockwaveKills = saveManagerObj.MainSaveFile.noOfShockwaveKills;
        noOfInAir = saveManagerObj.MainSaveFile.noOfInAir;
        noOfHits = saveManagerObj.MainSaveFile.noOfHits;
        noOfEnemiesDestroyed = saveManagerObj.MainSaveFile.noOfEnemiesDestroyed;
        noOfLaneSwitch = saveManagerObj.MainSaveFile.noOfLaneSwitch;
        goalsCompletionStatus = saveManagerObj.MainSaveFile.goalsCompletionStatus;
        alreadyGoalsCompleted = saveManagerObj.MainSaveFile.alreadyGoalsCompleted;

        // Player Level
        PlayerCurrentLevel = saveManagerObj.MainSaveFile.PlayerCurrentLevel;
        PlayerCurrentXP = saveManagerObj.MainSaveFile.PlayerCurrentXP;

        // Copy Figurines
        foreach (var pair in playerInventory.CharactersFigurinesAvailable)
        {
            CharacterFigurinesPicked.Add(pair.Key, 0);
        }

    }

    public void AddCharacterFigurine(int characterKey, int value)
    {
        int cur = CharacterFigurinesPicked[characterKey];
        CharacterFigurinesPicked[characterKey] = cur + value;
    }

    public void AddThisKeyToGamePlayInventory(string item, int value)
    {
        for (int i = 0; i < intKeyItems.Count; i++)
        {
            IntegerValueItem integerValueItem = intKeyItems[i];

            if (integerValueItem.itemName.Equals(item))
            {
                integerValueItem.itemValue += value;
                OnGamePlayInventoryItemAdded.Invoke(integerValueItem);
            }
        }
    }

    public void AddThisGamePlayRewardToAcquiredGamePlayRewards(float awardKey)
    {
        if (!gamePlayBackGroundAwardsAcquired.Contains(awardKey))
        {
            gamePlayBackGroundAwardsAcquired.Add(awardKey);
        }
    }

    public int GetSessionIntKeyData(string nameObj)
    {
        foreach (IntegerValueItem obj in intKeyItems)
            if (obj.itemName.Equals(nameObj))
                return obj.itemValue;

        return -1;
    }

    public void CopyGamePlaySessionItemsToSaveFile()
    {

        UpdateInventoryToPlayerInventory();
        UpdateCharacterFigurinesToPlayerInventory();
        UpateGamePlayRewardsToSaveFile();
        UpdateDailyGoalsDataToSaveFile();
        UpdateLevelInformationToSaveFile();
        UpdateUniqueEnvironmentsCovered();

    }

    private void UpdateUniqueEnvironmentsCovered()
    {
        int currentlySavedEnvCovered = saveManagerObj.MainSaveFile.UniqueEnvironmentsCompleted;
        int thisSessionCoveredEnv = MapProgressionSO.uniqueEnvironmentsCompletedThisSession;

        if (currentlySavedEnvCovered >= thisSessionCoveredEnv)
            return;

        saveManagerObj.MainSaveFile.UniqueEnvironmentsCompleted = thisSessionCoveredEnv;

    }

    private void UpdateInventoryToPlayerInventory()
    {
        for (int i = 0; i < intKeyItems.Count; i++)
        {
            IntegerValueItem integerValueItem = intKeyItems[i];

            // Do not save file
            playerInventory.UpdateKeyValues(new List<InventoryItem<int>>() { new InventoryItem<int>(integerValueItem.itemName, integerValueItem.itemValue) }, false, false);
        }
    }

    private void UpdateCharacterFigurinesToPlayerInventory()
    {
        foreach (var pair in CharacterFigurinesPicked)
        {
            playerInventory.UpdateCharacterFigurine(pair.Key, pair.Value);
        }
    }

    private void UpateGamePlayRewardsToSaveFile()
    {
        saveManagerObj.MainSaveFile.gamePlayBackGroundAwardsAcquired = new HashSet<float>(gamePlayBackGroundAwardsAcquired);
    }

    private void UpdateDailyGoalsDataToSaveFile()
    {
        saveManagerObj.MainSaveFile.noOfWallRuns = noOfWallRuns;
        saveManagerObj.MainSaveFile.noOfDashKills = noOfDashKills;
        saveManagerObj.MainSaveFile.noOfShockwaveKills = noOfShockwaveKills;
        saveManagerObj.MainSaveFile.noOfInAir = noOfInAir;
        saveManagerObj.MainSaveFile.noOfHits = noOfHits;
        saveManagerObj.MainSaveFile.noOfEnemiesDestroyed = noOfEnemiesDestroyed;
        saveManagerObj.MainSaveFile.noOfLaneSwitch = noOfLaneSwitch;
        saveManagerObj.MainSaveFile.goalsCompletionStatus = goalsCompletionStatus;
        saveManagerObj.MainSaveFile.alreadyGoalsCompleted = alreadyGoalsCompleted;
        
    }

    private void UpdateLevelInformationToSaveFile()
    {
        saveManagerObj.MainSaveFile.PlayerCurrentXP = PlayerCurrentXP;
        saveManagerObj.MainSaveFile.PlayerCurrentLevel = PlayerCurrentLevel;
    }

    public int GetPlayerHighestScore()
    {
        return Mathf.FloorToInt(saveManagerObj.MainSaveFile.playerHighScore);
    }

    public int GetCurrentSessionScore()
    {
        return Mathf.FloorToInt(gamePlaySessionData.DistanceCoveredInMeters);
    }

    public int GetFigurinesValue(int key)
    {
        return CharacterFigurinesPicked[key];
    }

    public int GetIntKeyValue(string itemKey)
    {
        foreach (IntegerValueItem obj in intKeyItems)
        {
            if (obj.itemName.Equals(itemKey))
            {
                return obj.itemValue;
            }
        }
        return 0;      // Item Not Found
    }

    public void DoubleTheGamePlayReward()
    {
        List<InventoryItem<int>> doubleRewards = new List<InventoryItem<int>>();

        for (int i = 0; i < intKeyItems.Count; i++)
        {
            IntegerValueItem integerValueItem = intKeyItems[i];

            if(integerValueItem.itemName == "AccountCoins" /*|| integerValueItem.itemName == "AccountDiamonds"*/)
            {
                //integerValueItem.isCelebrationAddition = true;
                doubleRewards.Add(integerValueItem);
            }

            // Do not save file
            playerInventory.UpdateKeyValues(doubleRewards);

        }
    }
}