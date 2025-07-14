using System;
using UnityEngine;

[CreateAssetMenu(fileName = "UIUpdaterSO", menuName = "PlayerInventorySystem/UIUpdaterSO")]
public class UiInfoUpdaterSO : ScriptableObject
{
    public string stashesTimer = default;
    public string totalStashTimer = default;

    //public void ConvertSecondsToTime(int currentTime, int totalTime)
    //{
    //    //stashesTimer = Math.Round((float)(currentTime / 60),2) + ":" + Math.Round((float)(currentTime % 60), 2)
    //    //     + "/" + Math.Round((float)(totalTime), 2) + ":00";

    //    stashesTimer = (currentTime / 60).ToString("00") + ":" + (currentTime % 60).ToString("00")
    //         + "/" + totalTime.ToString("00") + ":00";

    //    //   stashesTimer = string.Format("{0}:{1}/{2}:00", new object[] { (currentTime / 60), (currentTime % 60), totalTime.ToString("00") } );
    //}

    public void UpdateStashTimer(TimeSpan current)
    {
        stashesTimer = current.ToString("mm\\:ss");
    }

    public void ShowingHighestScore(float highestScore)
    {
        stashesTimer = highestScore.ToString("00");
    }
}