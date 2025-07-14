using UnityEngine;
using UnityEngine.Events;

public class RewardedAdCompletionHandler : MonoBehaviour
{
    [SerializeField] private RewardedAdSO rewardedAD;
    [SerializeField] private UnityEvent OnUserEarnedReward = new UnityEvent();

    private void OnEnable()
    {
        rewardedAD.OnUserEarnedReward.AddListener(HandleUserEarnedReward);
    }

    private void OnDisable()
    {
        rewardedAD.OnUserEarnedReward.RemoveListener(HandleUserEarnedReward);
    }

    private void HandleUserEarnedReward()
    {
     //   UnityEngine.Console.Log($"Completion Handler Ran Rewarded");
        OnUserEarnedReward.Invoke();
    }
}