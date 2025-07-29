using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimerSetOnEnable : MonoBehaviour
{
    [SerializeField] GamePlayRewardedPowerUpScreen gameplayReward;
    [SerializeField] TextMeshProUGUI timeText;
    private void OnEnable()
    {
        StartCoroutine(StartCountdown());
    }
    private IEnumerator StartCountdown()
    {
        int timer = 10;

        while (timer >= 1)
        {
            timeText.text = timer.ToString();
            Debug.Log("⏱️ Showing: " + timer);
            yield return new WaitForSecondsRealtime(1f);
            timer--;
        }

        Debug.Log("✅ Countdown finished. Closing reward panel.");

        if (gameplayReward != null)
        {
            gameplayReward.CloseWindow();
        }
        else
        {
            Debug.LogError("❌ gameplayReward is NULL!");
        }
    }



}
