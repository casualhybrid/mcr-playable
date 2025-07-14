using System.Collections;
using System.Collections.Generic;
using TheKnights.Purchasing;
using UnityEngine;

public class DisableIfIAPNotEligible : MonoBehaviour
{
    [SerializeField] private IAPItem iapItem;

    private void OnEnable()
    {
        if (!CheckAndDisableIfNotEligible())
            return;

        IAPManager.OnPurchaseSuccessfull.AddListener(ListenToPurchaseCompletions);
    }

    private void OnDisable()
    {
        IAPManager.OnPurchaseSuccessfull.RemoveListener(ListenToPurchaseCompletions);
    }

    private void ListenToPurchaseCompletions()
    {
        CheckAndDisableIfNotEligible();
    }

    private bool CheckAndDisableIfNotEligible()
    {
        bool isEligible = iapItem.ShouldTheInAppOfferBeShown();
        this.gameObject.SetActive(isEligible);

        return isEligible;
    }
}
