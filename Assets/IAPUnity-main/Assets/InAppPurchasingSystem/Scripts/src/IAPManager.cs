using RotaryHeart.Lib.SerializableDictionary;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;

namespace TheKnights.Purchasing
{
    [CreateAssetMenu(fileName = "IAPManager", menuName = "ScriptableObjects/InAppPurchasing/IAPManager", order = 1)]
    public class IAPManager : ScriptableObject
    {
        [System.Serializable]
        private class IAPItemsDictionary : SerializableDictionaryBase<string, IAPItem>
        { }

        public class PurchaseFailedEvent : UnityEvent<PurchaseFailureReason>
        { }

        [HideInInspector] public static UnityEvent OnPurchaseSuccessfull = new UnityEvent();
        [HideInInspector] public static PurchaseFailedEvent OnPurchaseFailed = new PurchaseFailedEvent();
        [HideInInspector] public static UnityEvent OnIAPInitialized = new UnityEvent();
        [HideInInspector] public static UnityEvent OnIAPInitializationFailed = new UnityEvent();

        /// <summary>
        /// Get an string array containing all the iAP IDs
        /// </summary>
        public string[] GetIAPDictionaryKeys
        { get { return iapItemsDictionary.Keys.ToArray(); } }

        /// <summary>
        /// Is the in app purchasing initialized
        /// </summary>
        public bool isInitialized { get; private set; }

        [System.NonSerialized] private bool isInitializationStartedOnce;

        [SerializeField] private IAPItemsDictionary iapItemsDictionary;

        public bool InitializeOnGameServicesInitComplete { get; set; } = false;

        private IAPPurchaser purchaser;

        // SomeHow even the private fields are maintaining there state after exiting play mode.
        // It can be because the scriptable object instance is not destroyed as it exisits in assets.
        // This won't be a problem on device as as soon as the game will close the instance will be destroyed

        private void OnEnable()
        {
            purchaser = null;
            isInitialized = false;
            InitializeOnGameServicesInitComplete = false;

            UnityGameServicesManager.OnGamingServiceSDKInitialized += UnityGameServicesManager_OnGamingServiceSDKInitialized;
            UnityGameServicesManager.OnGamingServiceFailedToInitialize += UnityGameServicesManager_OnGamingServiceSDKInitialized;
        }

        private void UnityGameServicesManager_OnGamingServiceSDKInitialized()
        {
            UnityGameServicesManager.OnGamingServiceSDKInitialized -= UnityGameServicesManager_OnGamingServiceSDKInitialized;
            UnityGameServicesManager.OnGamingServiceFailedToInitialize -= UnityGameServicesManager_OnGamingServiceSDKInitialized;

            if (InitializeOnGameServicesInitComplete)
            {
                CoroutineRunner.Instance.WaitForUpdateAndExecute(Initialize);
            }
        }

        /// <summary>
        /// Initializes the In app purchasing. This method should preferably be called at the start of the game once
        /// </summary>
        public virtual void Initialize()
        {
            if (!UnityGameServicesManager.isGamingServeInitializationProcessDone)
                return;

            if (isInitialized)
            {
                //  UnityEngine.Console.LogWarning("In app purchasing is already initialized.");
                return;
            }

            if (isInitializationStartedOnce)
                return;

            isInitializationStartedOnce = true;

            CoroutineRunner.Instance.StartCoroutine(InitializeRoutine());
        }

        private IEnumerator InitializeRoutine()
        {
            if (purchaser == null)
            {
                purchaser = new IAPPurchaser();
                SubscribeToEvents();
                SetProductIDs();
            }

            // prewarms
            var builder = purchaser.GetConfigurationBuilder;

            yield return null;

            purchaser.InitializePurchasing(iapItemsDictionary.Values);
        }

        /// <summary>
        /// Attempts to buy the requested in app item relative to the provided ID
        /// </summary>
        /// <param name="id">The IAP item ID</param>
        public void BuyTheProduct(string id)
        {
            AnalyticsManager.CustomData("InAppOfferBuyPressed", new Dictionary<string, object> { { "productID", id } });

            if (purchaser == null || !isInitialized)
            {
                OnPurchaseFailed?.Invoke(PurchaseFailureReason.PurchasingUnavailable);
                return;
            }

            purchaser.BuyProductID(id);
        }


        public void BuyTheProduct(IAPItem iapItem)
        {
            AnalyticsManager.CustomData("InAppOfferBuyPressed", new Dictionary<string, object> { { "ProductID", iapItem.ProductID } });

            if (purchaser == null)
            {
                OnPurchaseFailed?.Invoke(PurchaseFailureReason.PurchasingUnavailable);
                return;
            }

            purchaser.BuyProductID(iapItem.ProductID);
        }


        public string GetlocalizedPriceString(IAPItem iapItem)
        {
            if (isInitialized)
                return purchaser.GetLocalizedPrice(iapItem.ProductID);
            else
                return null;
        }

        // Set the product ids for each iap item instance as specified in the dictionary keys
        private void SetProductIDs()
        {
            foreach (var V in iapItemsDictionary)
            {
                V.Value.ProductID = V.Key;
            }
        }

        private void SubscribeToEvents()
        {
            purchaser.OnInitialize += PurchaserHasInitialized;
            purchaser.OnInitializationFailed += PurchaserFailedToInitialize;
            purchaser.OnPurchaseMade += PurchaseHasBeenMade;
            purchaser.OnPurchaseHasFailed += PurchaseHasFailed;
        }

        private void PurchaseHasBeenMade(string id)
        {
            //The player should be rewarded here

            IAPItem item;
            iapItemsDictionary.TryGetValue(id, out item);

            if (item != null)
            {
                item.ProcessPurchaseCompletion();
                OnPurchaseSuccessfull.Invoke();
            }
            else
            {
                throw new System.Exception("A purchase was made but couldn't find an IAP item relative to the ID " + id);
            }
        }

        private void PurchaserFailedToInitialize()
        {
            UnityEngine.Console.LogWarning("Failed to initialize In App Purchasing");
            OnIAPInitializationFailed.Invoke();
        }

        private void PurchaserHasInitialized()
        {
            //  UnityEngine.Console.Log("In app purchasing initialized successfully");
            isInitialized = true;
            OnIAPInitialized.Invoke();
        }

        private void PurchaseHasFailed(string id, PurchaseFailureReason reason)
        {
            UnityEngine.Console.Log("Purchase has failed of product ID " + id + " Reason: " + reason);
            OnPurchaseFailed.Invoke(reason);
        }
    }
}