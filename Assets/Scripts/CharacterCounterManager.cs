using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class CharacterCounter
{
    public string characterID;
    public TextMeshProUGUI counterText;
    public Button rewardButton; // This is the video ad button
    public int currentCount;
    public int totalCount;
    public Image counterImage;

    public GameObject completeGO;   // Show when complete or claimed
    public GameObject videoAdGO;    // Show when ad needed

    public ResetDuration resetDuration; // 👈 added
}

public enum ResetDuration
{
    OneMinute,
    OneDay,
    SevenDays
}

public class CharacterCounterManager : MonoBehaviour
{
    [SerializeField] List<CharacterCounter> characterCounters;

    private void OnEnable()
    {
        SetCharacter();
    }

    void SetCharacter()
    {
        for (int i = 0; i < characterCounters.Count; i++)
        {
            var counter = characterCounters[i];
            string rewardKey = "RewardClaimed_" + counter.characterID;
            string timeKey = "RewardTime_" + counter.characterID;

            // Reset check
            bool rewardClaimed = PlayerPrefs.GetInt(rewardKey, 0) == 1;
            if (rewardClaimed)
            {
                long lastTicks = long.Parse(PlayerPrefs.GetString(timeKey, "0"));
                DateTime lastClaimTime = new DateTime(lastTicks);
                TimeSpan diff = DateTime.Now - lastClaimTime;

                double requiredSeconds = 0;
                switch (counter.resetDuration)
                {
                    case ResetDuration.OneMinute: requiredSeconds = 60; break;
                    case ResetDuration.OneDay: requiredSeconds = 86400; break;
                    case ResetDuration.SevenDays: requiredSeconds = 604800; break;
                }

                if (diff.TotalSeconds >= requiredSeconds)
                {
                    PlayerPrefs.SetInt(rewardKey, 0); // Reset reward
                    PlayerPrefs.Save();
                    rewardClaimed = false;
                }
            }

            // Update current count and UI
            counter.currentCount = PlayerPrefs.GetInt("CollisionCount_" + counter.characterID, 0);
            counter.counterImage.fillAmount = (float)counter.currentCount / counter.totalCount;
            counter.counterText.text = counter.currentCount + "/" + counter.totalCount;

            // UI state based on claim
            if (rewardClaimed || counter.currentCount >= counter.totalCount)
            {
                counter.videoAdGO.SetActive(false);
                counter.completeGO.SetActive(true);
                counter.counterImage.fillAmount = 1f;
                counter.counterText.text = counter.totalCount + "/" + counter.totalCount;
                continue;
            }

            counter.videoAdGO.SetActive(true);
            counter.completeGO.SetActive(false);

            if (counter.rewardButton != null)
            {
                counter.rewardButton.onClick.RemoveAllListeners();
                counter.rewardButton.onClick.AddListener(() =>
                {
                    Debug.Log($"Showing rewarded ad for {counter.characterID}");

                    YourCustomAdFunction(() =>
                    {
                        Debug.Log($"Ad finished. Rewarding {counter.characterID}");

                        PlayerPrefs.SetInt(rewardKey, 1);
                        PlayerPrefs.SetString(timeKey, DateTime.Now.Ticks.ToString());
                        PlayerPrefs.Save();

                        counter.videoAdGO.SetActive(false);
                        counter.completeGO.SetActive(true);
                        counter.counterImage.fillAmount = 1f;
                        counter.counterText.text = counter.totalCount + "/" + counter.totalCount;
                    });
                });
            }
        }
    }

    // 👇 Replace this method call with your actual ad function
    void YourCustomAdFunction(Action onComplete)
    {
        // Tumhari ad logic yahan aye gi
        onComplete?.Invoke();
    }
}
