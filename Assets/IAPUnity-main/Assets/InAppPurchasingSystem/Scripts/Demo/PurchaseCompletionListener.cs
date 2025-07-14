using System.Collections;
using System.Collections.Generic;
using TheKnights.Purchasing;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using deVoid.UIFramework;

public class PurchaseCompletionListener : MonoBehaviour
{
    [SerializeField] private UnityEvent OnBeforePurchaseComplete;
    [SerializeField] private UnityEvent OnPurchaseComplete;
    [SerializeField] private UnityEvent OnPurchaseFailed;

    [SerializeField] private IAPItem iapItem;

    protected AWindowController aWindowController;

    private void Awake()
    {
        aWindowController = GetComponent<AWindowController>();
    }

    private void OnEnable()
    {
        iapItem.OnRightBeforePurchaseCompleted += CloseTheWindow;
        iapItem.OnPurchaseCompleted += HandlePurchaseComplete;
        IAPManager.OnPurchaseFailed.AddListener(HandlePuchaseFaliure);

        if(aWindowController != null)
        aWindowController.OnWhileHiding.AddListener(UnSubscribeEvents);
    }

    private void OnDisable()
    {
        UnSubscribeEvents();
    }

    protected void UnSubscribeEvents()
    {
        iapItem.OnRightBeforePurchaseCompleted -= CloseTheWindow;
        iapItem.OnPurchaseCompleted -= HandlePurchaseComplete;
        IAPManager.OnPurchaseFailed.RemoveListener(HandlePuchaseFaliure);

        if (aWindowController != null)
            aWindowController.OnWhileHiding.RemoveListener(UnSubscribeEvents);
    }

    private void CloseTheWindow()
    {
        OnBeforePurchaseComplete.Invoke();
    }

    private void HandlePurchaseComplete()
    {
        OnPurchaseComplete.Invoke();
    }

    private void HandlePuchaseFaliure(PurchaseFailureReason purchaseFailureReason)
    {
        OnPurchaseFailed.Invoke();
    }
}
