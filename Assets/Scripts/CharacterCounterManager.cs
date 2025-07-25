using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class CharacterCounter
{
    public string characterID;
    public TextMeshProUGUI counterText;
    public Button rewardButton;
    public int currentCount;
    public int totalCount;
    public Image counterImage;
}


public class CharacterCounterManager : MonoBehaviour
{
    [SerializeField] List<CharacterCounter> characterCounters;
    [SerializeField] private Button rewardButton; //  Your reward button

    private void OnEnable()
    {
        SetCharacter();
    }


    void SetCharacter()
    {
        for (int i = 0; i < characterCounters.Count; i++)
        {
            var counter = characterCounters[i];

            // Load current count
            counter.currentCount = PlayerPrefs.GetInt("CollisionCount_" + counter.characterID, 0);
            //Debug.LogError(PlayerPrefs.GetInt("CollisionCount_" + counter.characterID, 0));
            counter.counterImage.fillAmount = (float)PlayerPrefs.GetInt("CollisionCount_" + counter.characterID, 0) / counter.totalCount;
            // Update text
            counter.counterText.text = counter.currentCount + "/" + counter.totalCount;

            // Check if reward already claimed for this character
            bool rewardClaimed = PlayerPrefs.GetInt("RewardClaimed_" + counter.characterID, 0) == 1;

            // Enable button only if count met and reward not yet claimed
            if (counter.rewardButton != null)
            {
                bool canClaim = (counter.currentCount >= counter.totalCount) && !rewardClaimed;
                counter.rewardButton.interactable = canClaim;

                // Clean old listeners
                counter.rewardButton.onClick.RemoveAllListeners();

                // Add reward logic for this character
                counter.rewardButton.onClick.AddListener(() =>
                {
                    Debug.Log($" Reward Given for {counter.characterID}");

                    // Mark reward as claimed
                    PlayerPrefs.SetInt("RewardClaimed_" + counter.characterID, 1);
                    PlayerPrefs.Save();

                    // Disable button after claiming
                    counter.rewardButton.interactable = false;
                });
            }
        }
    }
}
