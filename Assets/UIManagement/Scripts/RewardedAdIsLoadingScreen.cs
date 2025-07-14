using deVoid.UIFramework;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Analytics;
using TMPro;

public class RewardedAdIsLoadingScreen : AWindowController
{
    [SerializeField] private GameEvent rewardLoadingADPanelClosed;
    [SerializeField] private AdsController adsController;
    [SerializeField] private float delayBeforeClosingThePanel;
    [SerializeField] private Transform rewardContentT;

    private bool rewardedADShown = false;
    private float timeScaleOnEnable = 1;

    private Coroutine coroutine;

    private void OnEnable()
    {
        adsController.PauseFMODMaster();
        timeScaleOnEnable = Time.timeScale;
        GameManager.IsTimeScaleLocked = true;
        Time.timeScale = 0;
        rewardedADShown = false;
        adsController.SetRewardedADPanelRewardMeta += SetRewardMeta;
        adsController.OnRewardedAdAboutToShow.AddListener(MarkRewardedADShownAndPauseCloseRoutine);
        adsController.OnRewardedAdCompleted.AddListener(MarkRewardedADShownAndCloseThePanel);
        adsController.OnRewardedAdSkipped.AddListener(MarkRewardedADShownAndCloseThePanel);

        coroutine = StartCoroutine(CloseThePanelAfterDelay());
    }

    private void OnDisable()
    {
        GameManager.IsTimeScaleLocked = false;
        Time.timeScale = timeScaleOnEnable;
        adsController.OnRewardedAdAboutToShow.RemoveListener(MarkRewardedADShownAndPauseCloseRoutine);
        adsController.OnRewardedAdCompleted.RemoveListener(MarkRewardedADShownAndCloseThePanel);
        adsController.OnRewardedAdSkipped.RemoveListener(MarkRewardedADShownAndCloseThePanel);
        adsController.SetRewardedADPanelRewardMeta -= SetRewardMeta;
        adsController.ResetShowRewardedAdAsSoonAsItsLoadedAndTheEnquedRequest();

        if(!rewardedADShown)
        {
            AnalyticsManager.CustomData("RewardedADScreenClosedWithoutShowingAD");
        }
    }

    private void MarkRewardedADShownAndPauseCloseRoutine()
    {
        rewardedADShown = true;

        if(coroutine != null)
        {
            StopCoroutine(coroutine);
        }
    }

    private void MarkRewardedADShownAndCloseThePanel()
    {
        rewardedADShown = true;
        ClosePanel();
    }

    private void ClosePanel()
    {
        rewardLoadingADPanelClosed.RaiseEvent();
        CloseTheWindow(ScreenId);
    }

    private IEnumerator CloseThePanelAfterDelay()
    {
        yield return new WaitForSecondsRealtime(delayBeforeClosingThePanel);

        if (!rewardedADShown)
        {
            adsController.RewardedADFailedToShowInTimeFrame();
        }

     // yield return new WaitForSecondsRealtime(1);
        yield return null;
        yield return null;

      //  if (!rewardedADShown)
     //   {
            adsController.ResumeFMODMaster();
       // }

        ClosePanel();
    }

    private void SetRewardMeta(RewardedADRewardMetaData[] meta)
    {
        if (meta == null || meta.Length == 0)
            return;

        foreach (Transform T in rewardContentT)
        {
            T.gameObject.SetActive(false);
        }

        for (int i = 0; i < meta.Length; i++)
        {
            RewardedADRewardMetaData entry = meta[i];

            if (rewardContentT.childCount > i)
            {
                GameObject rewardObj = rewardContentT.GetChild(i).gameObject;
                rewardObj.SetActive(true);
                rewardContentT.GetChild(i).GetComponentsInChildren<Image>()[1].sprite = entry.Sprite ;

                TextMeshProUGUI amountText = rewardContentT.GetChild(i).GetComponentInChildren<TextMeshProUGUI>();
                amountText.text = entry.Value > 0 ? entry.Value.ToString() : "";
            }
            else
            {
                UnityEngine.Console.LogWarning("The rewards to get are greater than available display objects in the AD is loading screen");
            }
        }
    }
}