using UnityEngine;
using UnityEngine.Purchasing;
using System;

namespace TheKnights.Purchasing
{
    /// <summary>
    /// Derive from this class to implement a new IAP item
    /// </summary>
    public abstract class IAPItem : ScriptableObject
    {
        public event Action OnPurchaseCompleted;

        public event Action OnRightBeforePurchaseCompleted;

        [SerializeField] protected ProductType productType;

        /// <summary>
        /// The ID of the item as specified in the console
        /// </summary>
        public string ProductID { get; set; }

        /// <summary>
        /// Returns the type of in app item
        /// </summary>
        public ProductType GetProductType => productType;

        /// <summary>
        /// Reward the user as the in app purchase had been made successfully
        /// </summary>
        public abstract void ProcessPurchaseCompletion();

        public virtual bool ShouldTheInAppOfferBeShown()
        {
            return true;
        }

        protected void RaisePurchaseCompletionEvent()
        {
            OnPurchaseCompleted?.Invoke();
        }

        protected void RaiseRightBeforePurchaseCompleted()
        {
            OnRightBeforePurchaseCompleted?.Invoke();
        }
    }
}