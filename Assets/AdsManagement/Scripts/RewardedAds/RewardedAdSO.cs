using System;
using System.Collections;
using System.Collections.Generic;
using TheKnights.AdsSystem;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;

[System.Serializable]
public struct RewardedADRewardMetaData
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private int value;

    public RewardedADRewardMetaData(Sprite _sprite, int _value)
    {
        sprite = _sprite;
        value = _value;
    }

    public Sprite Sprite => sprite;
    public int Value => value;

}


public abstract class RewardedAdSO : ScriptableObject
{
    private enum RewardedADOpenedCondition
    {
        PriorityWasLoadedAlready, PriorityLoadedReservedTime, BackedupAnyADAvailable
    }

    public UnityEvent OnUserEarnedReward { get; private set; } = new UnityEvent();

    [InfoBox("Caution! This meta is only used for display purposes. The player would not be awarded diffrently if the amount value is changed here", InfoMessageType.Warning)]
    [SerializeField] protected RewardedADRewardMetaData[] rewardedADRewardMetaDatas;

    [Space]

    [SerializeField] protected GameEvent rewardedAdFailedToShow_NotLoaded;
    [SerializeField] protected GameEvent rewardedADFramingWindowFinished;
    [SerializeField] protected AdsController AdsController;

  

    protected bool hasPriorityRewardedFailedOnce = false;

    private RewardedADOpenedCondition rewardedADOpenedCondition;


    public virtual bool IsRewardedAdOnCoolDown()
    {
        return false;
    }

    public virtual void ShowRewardedAD()
    {
        //rewardedADOpenedCondition = RewardedADOpenedCondition.PriorityWasLoadedAlready;

        //hasPriorityRewardedFailedOnce = false;

        //AdsController.OnRewardedAdAboutToShow.AddListener(RewardedAdAboutToShow);
        //rewardedAdFailedToShow_NotLoaded.RaiseEvent();
        //AdsController.SetTheRewardedADPanelMetaData(rewardedADRewardMetaDatas);

        //// if rewarded is not ready load ads here to save that 2 seconds
        //if (!AdsController.IsPriorityRewardedADAvailalbe())
        //{
        //    AdsController.LoadAllRewardedAds();
        //}

        //CoroutineRunner.Instance.StartCoroutine(ShowPriortiyRewardedADAfterDelay());

        MaxAdMobController.Instance.ShowRewardedVideoAd(this);
    }

    protected IEnumerator ShowPriortiyRewardedADAfterDelay()
    {
        yield return new WaitForSecondsRealtime(1f);
        AdsController.ShowPriorityRewardedAD(CompletionCallBack);
    }

    protected virtual void ShowAnyAvailableRewardedADInOrder()
    {
        rewardedADOpenedCondition = RewardedADOpenedCondition.BackedupAnyADAvailable;

        AdsController.OnRewardedADFailedToShowInGivenTimeFrame.RemoveListener(ShowAnyAvailableRewardedADInOrder);
        AdsController.OnRewardedAdAboutToShow.AddListener(RewardedAdAboutToShow);
        Action<Status, ADMeta> action = CompletionCallBack;
        action += (Status s, ADMeta adMeta) =>
        {
            if (s == Status.Failed)
            {
                ResetShowAsSoonAsLoadedAndSendFrameEndedEvents(false);
            }
        };

        AdsController.ShowRewardedAD(action);
    }

    public virtual void RewardedAdAboutToShow()
    {
        AnalyticsManager.CustomData("RewardedAdOpened", new Dictionary<string, object> { { "RewardedADType", this.name }, {"RewardedOpenCondition", rewardedADOpenedCondition.ToString()} });
    }

    public virtual void CompletionCallBack(Status status, ADMeta adMeta)
    {
        AdsController.OnRewardedADFailedToShowInGivenTimeFrame.RemoveListener(ShowAnyAvailableRewardedADInOrder);
        AdsController.OnRewardedAdAboutToShow.RemoveListener(RewardedAdAboutToShow);

        if (status == Status.Succeded)
        {
            AnalyticsManager.CustomData("RewardedAdCompleted", new Dictionary<string, object> { { "RewardedADType", this.name }, { "mediationAdapterName", adMeta.AdapterName} });

            ResetShowAsSoonAsLoadedAndSendFrameEndedEvents(true);
        }
        else if (status == Status.Failed)
        {
            if (!hasPriorityRewardedFailedOnce)
            {
                hasPriorityRewardedFailedOnce = true;

                UnityEngine.Console.Log("Failed Rewarded. Enqueing");

                rewardedADOpenedCondition = RewardedADOpenedCondition.PriorityLoadedReservedTime;

                AdsController.OnRewardedADFailedToShowInGivenTimeFrame.AddListener(ShowAnyAvailableRewardedADInOrder);

                AdsController.ShowRewardedAsSoonAsAvailableAndEnqueueRequest(() =>
                {
                    AdsController.OnRewardedAdAboutToShow.AddListener(RewardedAdAboutToShow);
                    AdsController.ShowPriorityRewardedAD(CompletionCallBack);
                });
            }
        }
        else // Skipped or unknown
        {
            ResetShowAsSoonAsLoadedAndSendFrameEndedEvents(true);

            AnalyticsManager.CustomData("RewardedAdSkipped", new Dictionary<string, object> { { "RewardedADType", this.name } });
        }
    }

    // *NOTE : wasAbleToShowAD doesn't necessarily mean that the rewarded AD was successfull (reward granted)
    // AD could have been skipped
    private void ResetShowAsSoonAsLoadedAndSendFrameEndedEvents(bool wasAbleToShowAD)
    {
        // Just to be safe
        AdsController.ResetShowRewardedAdAsSoonAsItsLoadedAndTheEnquedRequest();

        rewardedADFramingWindowFinished.RaiseEvent();

        AdsController.SendRewardedFramingWindowEndedEvent(wasAbleToShowAD);
    }
}