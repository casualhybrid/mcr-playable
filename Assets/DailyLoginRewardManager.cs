using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyLoginRewardManager : MonoBehaviour
{
    [Serializable]
    public class DailyRewardButton
    {
        public Button rewardButton;
        public Image buttonImage;
        public GameObject claimObject;
        public int amount;
        public GameObject rewardedVideoIcon;
        public GameObject claimedImage;
    }

    public enum RewardIntervalType
    {
        SevenDays,
        OneMinute
    }

    [Header("Daily Reward Settings")]
    public List<DailyRewardButton> dailyButtons;
    public Sprite blueSprite, yellowSprite;

    [Header("Timing Control")]
    public RewardIntervalType rewardIntervalType = RewardIntervalType.OneMinute;

    private int currentDayIndex;
    private DateTime nextRewardTime;

    private const string NextRewardKey = "NextRewardTime";
    private const string DayIndexKey = "DailyLoginDay";

    private bool isRewardedVideoActive = false;

    public static int newCurrentIndex;

    private void Start()
    {
        Log("Manager Start → loading progress");
        LoadProgress();
        RefreshUI();
    }

    // ------------------- PROGRESS -------------------
    private void LoadProgress()
    {
        currentDayIndex = PlayerPrefs.GetInt(DayIndexKey, 0);

        if (PlayerPrefs.HasKey(NextRewardKey))
            nextRewardTime = DateTime.Parse(PlayerPrefs.GetString(NextRewardKey));
        else
            nextRewardTime = DateTime.UtcNow;

        Log($"LoadProgress → DayIndex: {currentDayIndex}, NextRewardTime: {nextRewardTime}");
    }

    private void SaveProgress()
    {
        PlayerPrefs.SetInt(DayIndexKey, currentDayIndex);
        PlayerPrefs.SetString(NextRewardKey, nextRewardTime.ToString());
        PlayerPrefs.Save();

        Log($"SaveProgress → DayIndex: {currentDayIndex}, NextRewardTime: {nextRewardTime}");
    }

    // ------------------- UI REFRESH -------------------
    private void RefreshUI()
    {
        Log("RefreshUI → checking state...");

        if (currentDayIndex >= dailyButtons.Count)
        {
            Log("All rewards claimed. Disabling buttons.");
            DisableAllButtons();
            return;
        }

        if (DateTime.UtcNow >= nextRewardTime)
        {
            isRewardedVideoActive = false;
            Log($"Day {currentDayIndex + 1} available → setting up normal daily reward.");
            SetupDailyReward();
        }
        else
        {
            isRewardedVideoActive = true;
            Log($"Day {currentDayIndex + 1} locked until {nextRewardTime} → enabling Rewarded Video option.");
            SetupRewardedAdReward();
        }
    }

    private void SetupDailyReward()
    {
        Log("SetupDailyReward → Highlighting current day button.");

        for (int i = 0; i < dailyButtons.Count; i++)
        {
            int index = i;
            DailyRewardButton btn = dailyButtons[i];

            bool isCurrent = (index == currentDayIndex);

            btn.buttonImage.sprite = isCurrent ? yellowSprite : blueSprite;
            btn.rewardButton.interactable = isCurrent;
            btn.claimObject.SetActive(isCurrent);

            btn.claimedImage.SetActive(index < currentDayIndex);

            btn.rewardedVideoIcon.SetActive(false);
            btn.rewardButton.onClick.RemoveAllListeners();

            if (isCurrent)
            {
                Log($"Day {index + 1} is available → enabling claim button.");
                btn.rewardButton.onClick.AddListener(() => ClaimReward(index));
            }
        }
    }

    private void SetupRewardedAdReward()
    {
        Log("SetupRewardedAdReward → Rewarded Video active for current day.");

        DisableAllButtons();

        DailyRewardButton btn = dailyButtons[currentDayIndex];

        btn.buttonImage.sprite = yellowSprite;
        btn.rewardButton.interactable = true;
        btn.claimObject.SetActive(true);
        btn.rewardedVideoIcon.SetActive(true);

        for (int i = 0; i < dailyButtons.Count; i++)
            dailyButtons[i].claimedImage.SetActive(i < currentDayIndex);

        btn.rewardButton.onClick.RemoveAllListeners();
        btn.rewardButton.onClick.AddListener(() => ShowRewardedVideo(currentDayIndex));
    }

    private void DisableAllButtons()
    {
        Log("DisableAllButtons → resetting all states.");
        foreach (var btn in dailyButtons)
        {
            btn.rewardButton.interactable = false;
            btn.claimObject.SetActive(false);
            btn.rewardedVideoIcon.SetActive(false);
            btn.buttonImage.sprite = blueSprite;
        }
    }

    // ------------------- CLAIM REWARD -------------------
    public void ClaimReward(int index)
    {
        Log($"ClaimReward → Claiming reward for Day {index + 1}, Amount: {dailyButtons[index].amount}");

        GamePlayMysteryBoxOpenPanel.currentIndex = index;
        newCurrentIndex = index;
        GamePlayMysteryBoxOpenPanel.isDailyReward = true;
        GamePlayMysteryBoxOpenPanel.amountReward = dailyButtons[index].amount;

        DailyRewardButton btn = dailyButtons[index];
        btn.rewardButton.interactable = false;
        btn.claimObject.SetActive(false);
        btn.claimedImage.SetActive(true);

        currentDayIndex++;

        if (!isRewardedVideoActive)
        {
            TimeSpan waitDuration = rewardIntervalType == RewardIntervalType.OneMinute
                ? TimeSpan.FromMinutes(1)
                : TimeSpan.FromDays(1);

            nextRewardTime = DateTime.UtcNow + waitDuration;
            Log($"Next reward scheduled at {nextRewardTime}");
        }

        SaveProgress();
        RefreshUI();
    }

    // ------------------- REWARDED AD -------------------
    private void ShowRewardedVideo(int index)
    {
        Log("ShowRewardedVideo → Attempting to show ad.");

        if (!MaxAdMobController.Instance.IsRewardedAdAvailable())
        {
            Log("Rewarded ad not available.");
            return;
        }

        MaxAdMobController.Instance.ShowRewardedVideoAd();

        void HandleVideoComplete()
        {
            MaxAdMobController.OnVideoAdCompleteReward -= HandleVideoComplete;
            Log("Rewarded Ad Completed → granting reward.");
            ClaimReward(index);
        }

        MaxAdMobController.OnVideoAdCompleteReward += HandleVideoComplete;

    }

    // ------------------- PANEL CLOSE -------------------
    public void Close()
    {
        Log("Close → Closing daily reward panel.");
        GamePlayMysteryBoxOpenPanel.isDailyReward = false;
    }

    // ------------------- DEBUG UTIL -------------------
    private void Log(string message)
    {
        Debug.Log($"<color=#00BFFF>[DAILY REWARD]</color> {message}");
    }
}
