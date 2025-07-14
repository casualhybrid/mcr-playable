using MessagePack;
using System;
using System.Collections.Generic;
using TheKnights.PlayServicesSystem.LeaderBoards;
using UnityEngine;

//using TheKnights.FireBase.LeaderBoard;
//using TheKnights.PlayServicesSystem.LeaderBoards;

[MessagePackObject]
public class MainSaveFile : IMessagePackSerializationCallbackReceiver
{
    /// <summary>
    ///
    /// </summary>
    [IgnoreMember] public const string MainSaveGame = "MainSaveGame.save";

    [Key(0)] public int FileVersion;

    [Key(1)] public bool FileSaved;
    [Key(2)] public List<IntegerValueItem> gameIntItems = new List<IntegerValueItem>();
    [Key(3)] public List<InventoryObject> gameVehicles = new List<InventoryObject>();
    [Key(4)] public List<GoalItemDataSet> gameDailyGoals = new List<GoalItemDataSet>();

    [Key(5)] public string sessionDate = default;

    [Key(6)] public int PlayerCurrentLevel ;
    [Key(7)] public float PlayerCurrentXP;

    // These keys are used to track Daily Goals progession
    [Key(8)] public int noOfWallRuns = default;

    [Key(9)] public int noOfDashKills = default;
    [Key(10)] public int noOfShockwaveKills = default;
    [Key(11)] public int noOfInAir = default;
    [Key(12)] public int noOfHits = default;
    [Key(13)] public int noOfEnemiesDestroyed = default;
    [Key(14)] public int noOfLaneSwitch = default;
    [Key(15)] public int goalsCompletionStatus = 0;
    [Key(16)] public int alreadyGoalsCompleted = 0;

    [Key(17)] public float playerHighScore;

    [Key(18)] public HashSet<float> gamePlayBackGroundAwardsAcquired = new HashSet<float>();

    [Key(19)] public bool isAdsPurchased = default;

    // LeaderBoard
    [Key(20)] public LeaderBoardType currentLeaderBoardRank;

    [Key(21)] public string leaderBoardUserName = "You";

    [Key(22)] public List<string> facebookRequestIdsPendingDeletion = new List<string>();

    [Key(23)] public int currentlySelectedCar = 0;
    [Key(24)] public int currentlySelectedCharacter;
    [Key(25)] public int currentCategoryRank = 0;
    [Key(26)] public int currentCategoryScore = 0;

    // Inactivity Period
    [Key(27)] public string applicationQuitTime;

    // Encounters and Difficulty
    [Key(28)] public float currentIntendedPlayerDifficultyRating ;
    [Key(29)] public bool isSkillPlacement = true;
    [Key(30)] public float highestSkillPlacementDifficultyRating;

    [Key(34)] public bool TutorialHasCompleted = false;

    [Key(35)] public int UniqueEnvironmentsCompleted;
    [Key(36)] public bool HasSignedInToFaceBookOnce;

    [Key(37)] public Dictionary<int, int> CharactersFigurinesAvailable = new Dictionary<int, int>();
    public MainSaveFile()
    { }

    public MainSaveFile(InventorySystem iSObj, DailyGoalsManagerSO dailyGoalsManager)
    {
        foreach (IntegerValueItem obj in iSObj.intKeyItems)
        {
            IntegerValueItem intObj = new IntegerValueItem();
            intObj.itemName = obj.itemName;
            intObj.itemValue = obj.itemValue;
            gameIntItems.Add(intObj);
        }

        foreach (InventoryObject obj in iSObj.gameCars)
        {
            InventoryObject intObj = new InventoryObject();
            intObj.name = obj.name;
            intObj.isObjectUnlocked = obj.isObjectUnlocked;
            gameVehicles.Add(intObj);
        }

        foreach (var key in iSObj.CharactersDataBase.CharactersDataBaseDictionary.Keys)
        {
            CharactersFigurinesAvailable.Add(key, 0);
        }

        //  sessionDate = dailyGoalsManager.timeManager.GetCurrentTimeDateInString();

        gameDailyGoals = dailyGoalsManager.dailyGoalsObj.PoolDataSets();

        //Set player level
        PlayerCurrentLevel = 1;
    }

    public void UpdateMainSaveFileIfRequired(InventorySystem iSObj, DailyGoalsManagerSO dailyGoalsManager)
    {
        // add cars and rest as well

        foreach (var key in iSObj.CharactersDataBase.CharactersDataBaseDictionary.Keys)
        {
            if (!CharactersFigurinesAvailable.ContainsKey(key))
            {
                CharactersFigurinesAvailable.Add(key, 0);
            }
        }
    }

    //public Action ConvertFromOldSaveIfRequired(InventorySystem iSObj)
    //{
    //    bool wasRetrieved = false;
    //    List<string> keysToRemoveLater = new List<string>();

    //    foreach (var pair in iSObj.CharactersDataBase.CharactersDataBaseDictionary.Values)
    //    {
    //        int i = pair.IndexKey;

    //        string key = "figurinePartsCount" + i;

    //        if (!PlayerPrefs.HasKey(key))
    //            continue;

    //        keysToRemoveLater.Add(key);
    //        int fig = PlayerPrefs.GetInt(key);

    //        CharactersFigurinesAvailable[i] = fig;
    //        wasRetrieved = true;
    //    }

    //    if(!wasRetrieved)
    //    {
    //        return null;
    //    }

    //    return () => {

    //        for (int i = 0; i < keysToRemoveLater.Count; i++)
    //        {
    //            PlayerPrefs.DeleteKey(keysToRemoveLater[i]);
    //        }
        
    //    };
        
    //}


    public void OnAfterDeserialize()
    {
    }

    public void OnBeforeSerialize()
    {
        ++FileVersion;
    }

   
}