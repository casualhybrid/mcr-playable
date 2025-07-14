using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MultipleRewardedADUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timesWatchedText;
    [SerializeField] private int timesToWatchAD = 3;

    private int currentlyWatched = 0;

    public void IncrementTimedWatched()
    {
        currentlyWatched++;

        if (currentlyWatched == 3)
            currentlyWatched = 0;

        timesWatchedText.text = $"<b>{currentlyWatched} / {timesToWatchAD}</b> Ads Watched";
    
    }
}
