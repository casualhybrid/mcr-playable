using deVoid.UIFramework;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_IOS
using UnityEngine.iOS;
#endif

using UnityEngine.UI;

public class RateGame : AWindowController
{
    [Header("Variables")]
    [Header("References")]
    public Button[] starButton;

    public Button acceptButton;

    [HideInInspector] public int ratedApp; // rate stor value can be used for something after rating

    private void OnEnable()
    {
        Init();
    }

    private void Init()
    {
        // get is rated app value
        bool isRated = PlayerPrefs.GetInt("isAppRated", 0) == 1 ? true : false;

        if (!isRated)
        {
            ratedApp = 0;
            RateApplication(0);
            acceptButton.interactable = false;
        }
        else
        {
            CloseWindow(false);
        }
    }

    public void AcceptRating()
    {
        // analytics rating
        ReportRateApp(ratedApp);

        if (ratedApp >= 4)
        {
#if UNITY_ANDROID
            //InGameRatingHandler.Instance.LaunchInAppReviewFlow();
#elif UNITY_IOS
            Device.RequestStoreReview();
#endif
        }

        CloseWindow(true);
    }

    public void RateLater()
    {
        // analytics action type
        //  ReportRateType("Rate later");
        // set next rating window open after "remindRating"
        // PlayerPrefs.SetInt("remindRating", PlayerPrefs.GetInt("remindRating", remindRating) + remindRating);
        // close App Rating window
        CloseWindow(false);
    }

    public void RateApplication(int rate)
    {
        ratedApp = rate;

        // active rate button if use click some stars
        if (rate > 0)
            acceptButton.GetComponent<Button>().interactable = true;

        // enable stars equal than user rated
        for (int i = 0; i < rate; i++)
        {
            foreach (Transform t in starButton[i].transform)
            {
                t.gameObject.SetActive(true);
            }
        }

        // enable stars greater than user rated
        for (int i = rate; i < starButton.Length; i++)
        {
            foreach (Transform t in starButton[i].transform)
            {
                t.gameObject.SetActive(false);
            }
        }
    }

    private void CloseWindow(bool isRated)
    {
        if (isRated)
        {
          //  ReportRateType("Rated and close");
            PlayerPrefs.SetInt("isAppRated", 1);
        }

        UI_Close();
    }

    private void ReportRateApp(int stars)
    {
        AnalyticsManager.CustomData("GameRated", new Dictionary<string, object>
        {
            { "GameRating", stars }
        });

       
    }

}