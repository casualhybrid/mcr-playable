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
    public RewardIntervalType rewardIntervalType = RewardIntervalType.SevenDays;

    private int currentDayIndex = 0;
    private DateTime nextRewardTime;
    private const string NextRewardKey = "NextRewardTime";
    private const string DayIndexKey = "DailyLoginDay";

    private void Start()
    {
        LoadProgress();
        CheckRewardAvailability();
    }

    private void LoadProgress()
    {
        currentDayIndex = PlayerPrefs.GetInt(DayIndexKey, 0);

        if (PlayerPrefs.HasKey(NextRewardKey))
            nextRewardTime = DateTime.Parse(PlayerPrefs.GetString(NextRewardKey));
        else
            nextRewardTime = DateTime.UtcNow;
    }

    private void CheckRewardAvailability()
    {
        if (currentDayIndex >= dailyButtons.Count)
        {
            Debug.Log("All rewards claimed.");
            DisableAllButtons();
            return;
        }

        if (DateTime.UtcNow >= nextRewardTime)
        {
            SetupButtons(); // reward available
        }
        else
        {
            DisableAllButtons(); // wait for timer
        }
    }

    private void SetupButtons()
    {
        for (int i = 0; i < dailyButtons.Count; i++)
        {
            int index = i;
            bool isCurrent = (index == currentDayIndex);

            dailyButtons[i].buttonImage.sprite = isCurrent ? yellowSprite : blueSprite;
            dailyButtons[i].rewardButton.interactable = isCurrent;
            dailyButtons[i].claimObject.SetActive(isCurrent);

            dailyButtons[i].rewardButton.onClick.RemoveAllListeners();

            if (isCurrent)
                dailyButtons[i].rewardButton.onClick.AddListener(() => ClaimReward(index));
        }
    }

    private void DisableAllButtons()
    {
        foreach (var btn in dailyButtons)
        {
            btn.rewardButton.interactable = false;
            btn.claimObject.SetActive(false);
            btn.buttonImage.sprite = blueSprite;
        }
    }

    private void ClaimReward(int index)
    {
        Debug.Log($"Reward claimed for Day {index + 1}");

        dailyButtons[index].rewardButton.interactable = false;
        dailyButtons[index].claimObject.SetActive(false);

        // Advance logic
        currentDayIndex++;
        PlayerPrefs.SetInt(DayIndexKey, currentDayIndex);

        // Set next unlock time
        TimeSpan waitDuration = rewardIntervalType == RewardIntervalType.OneMinute
            ? TimeSpan.FromMinutes(1)
            : TimeSpan.FromDays(1);

        nextRewardTime = DateTime.UtcNow + waitDuration;
        PlayerPrefs.SetString(NextRewardKey, nextRewardTime.ToString());

        PlayerPrefs.Save();

        DisableAllButtons(); // prevent spamming
    }
}
