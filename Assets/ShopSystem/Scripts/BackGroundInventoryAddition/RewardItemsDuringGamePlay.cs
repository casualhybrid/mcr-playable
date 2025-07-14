using System;
using System.Collections.Generic;
using System.Linq;
using TheKnights.SaveFileSystem;
using UnityEngine;

public class RewardItemsDuringGamePlay : MonoBehaviour
{
    [SerializeField] private GamePlayBackGroundRewardsData gamePlayBackGroundRewardsData;
    [SerializeField] private GamePlaySessionInventory gamePlaySessionInventory;
    [SerializeField] private InventorySystem playerInventorySO;
    [SerializeField] private SaveManager saveManagerObj;
    [SerializeField] private UiInfoUpdaterSO timerInfoUpdater;
    [SerializeField] private GameEvent openDiamondStashEvent;
    [SerializeField] private GameEvent onAllRewardsDone;

    private Queue<float> timeQueue = new Queue<float>();

    private float gameplayTimeSec = 0f, gamePlayTime = 0f, timeToCheck = 0f;

    private const float rewardCheckUpdateInterval = 0.1f;
    private float elapsedRewardCheckInterval = rewardCheckUpdateInterval;

    private void Awake()
    {
        HashSet<float> userRewardAcquired = saveManagerObj.MainSaveFile.gamePlayBackGroundAwardsAcquired;

        foreach (var item in gamePlayBackGroundRewardsData.GetDictionaryKeys().OrderBy(x => x))
        {
            if (!userRewardAcquired.Contains(item))
            {
                timeQueue.Enqueue(item);
            }
        }

        if (timeQueue.Count == 0)
        {
            UnityEngine.Console.Log("User Has Already completed all the gameplay rewards");
            timerInfoUpdater.ShowingHighestScore(saveManagerObj.MainSaveFile.playerHighScore);
            this.gameObject.SetActive(false);
            return;
        }

        timeToCheck = timeQueue.Peek();
        TimeSpan totalRequiredTimeSpan = new TimeSpan(0, (int)timeToCheck, 0);
        timerInfoUpdater.totalStashTimer = "/" + totalRequiredTimeSpan.ToString("mm\\:ss");
        timerInfoUpdater.UpdateStashTimer(new TimeSpan());
    }

    private void Update()
    {
        if (!GameManager.IsGamePlayStarted)
            return;

        elapsedRewardCheckInterval += Time.deltaTime;

        if (elapsedRewardCheckInterval < rewardCheckUpdateInterval)
            return;

        elapsedRewardCheckInterval = 0;

        if (timeQueue.Count == 0)
        {
            UnityEngine.Console.Log("All BackGround GamePlay Rewards Completed");

            //Will show last highScore here...

            timerInfoUpdater.ShowingHighestScore(saveManagerObj.MainSaveFile.playerHighScore);

            onAllRewardsDone.RaiseEvent();
            this.gameObject.SetActive(false);
            return;
        }

        gamePlayTime = GameManager.gameplaySessionTimeInMinutes;
        //  timeToCheck = timeQueue.Peek();

        // Give Reward
        if (gamePlayTime >= timeToCheck)
        {
            timeQueue.Dequeue();

            ItemWithAmount rewardItem = gamePlayBackGroundRewardsData.GetRewardItemForKey(timeToCheck);

            // Give Reward Here
            //   playerInventorySO.UpdateKeyValues(new List<InventoryItem<int>>() { new InventoryItem<int>("AccountDiamonds", rewardItem.GetAmount) }); // Adds one to exsisting value
            gamePlaySessionInventory.AddThisKeyToGamePlayInventory("AccountDiamonds", rewardItem.GetAmount);
            gamePlaySessionInventory.AddThisGamePlayRewardToAcquiredGamePlayRewards(timeToCheck);
            //  UnityEngine.Console.Log($"Giving background gamseplay reward of {rewardItem.GetItemType.name} of amount {rewardItem.GetAmount}");

            openDiamondStashEvent.RaiseEvent();

            if (timeQueue.Count != 0)
            {
                timeToCheck = timeQueue.Peek();
                TimeSpan totalRequiredTimeSpan = new TimeSpan(0, (int)timeToCheck, 0);
                timerInfoUpdater.totalStashTimer = "/" + totalRequiredTimeSpan.ToString("mm\\:ss");
            }
        }

        gameplayTimeSec = GameManager.gameplaySessionTimeInSeconds;

        TimeSpan timeSpan = new TimeSpan(0, 0, (int)gameplayTimeSec);
        timerInfoUpdater.UpdateStashTimer(timeSpan);
    }

    public int GetNextStashTime(float gamePlayTime, float timeToCheck)
    {
        return Mathf.FloorToInt(gamePlayTime - timeToCheck);
    }
}