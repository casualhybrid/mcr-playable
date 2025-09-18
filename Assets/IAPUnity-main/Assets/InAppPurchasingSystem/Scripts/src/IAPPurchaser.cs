using AppsFlyerSDK;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

namespace TheKnights.Purchasing
{
    public class IAPPurchaser : IStoreListener
    {
        public event Action OnInitialize;

        public event Action OnInitializationFailed;

        public event Action<string, PurchaseFailureReason> OnPurchaseHasFailed;

        public event Action<string> OnPurchaseMade;

        private IStoreController m_StoreController;          // The Unity Purchasing system.
        private IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.

        // Apple App Store-specific product identifier for the subscription product.
        //   private static string kProductNameAppleSubscription = "com.unity3d.subscription.new";

        // Google Play Store-specific product identifier subscription product.
        //   private static string kProductNameGooglePlaySubscription = "com.unity3d.subscription.original";

        public ConfigurationBuilder GetConfigurationBuilder
        {
            get
            {
                if (configurationBuilder == null)
                {
                    configurationBuilder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
                }

                return configurationBuilder;
            }
        }

        private ConfigurationBuilder configurationBuilder;

        public void InitializePurchasing(ICollection<IAPItem> iapItems)
        {
            // If we have already connected to Purchasing ...
            if (IsInitialized() || m_StoreController != null)
            {
                // ... we are done here.

                return;
            }

            // Create a builder, first passing in a suite of Unity provided stores.
            var builder = GetConfigurationBuilder;

            // Add a product to sell / restore by way of its identifier, associating the general identifier
            // with its store-specific identifiers.

            foreach (var item in iapItems)
            {
                builder.AddProduct(item.ProductID, item.GetProductType);
            }

            // Continue adding the non-consumable product.
            // builder.AddProduct(kProductIDNonConsumable, ProductType.NonConsumable);
            // And finish adding the subscription product. Notice this uses store-specific IDs, illustrating
            // if the Product ID was configured differently between Apple and Google stores. Also note that
            // one uses the general kProductIDSubscription handle inside the game - the store-specific IDs
            // must only be referenced here.
            /*             builder.AddProduct(kProductIDSubscription, ProductType.Subscription, new IDs(){
                            { kProductNameAppleSubscription, AppleAppStore.Name },
                            { kProductNameGooglePlaySubscription, GooglePlay.Name },
                        }); */

            // Kick off the remainder of the set-up with an asynchrounous call, passing the configuration
            // and this class' instance. Expect a response either in OnInitialized or OnInitializeFailed.
            UnityPurchasing.Initialize(this, builder);
        }

        private bool IsInitialized()
        {
            // Only say we are initialized if both the Purchasing references are set.
            return m_StoreController != null && m_StoreExtensionProvider != null;
        }

        public void BuyProductID(string productId)
        {
            // If Purchasing has been initialized ...
            if (IsInitialized())
            {
                // ... look up the Product reference with the general product identifier and the Purchasing
                // system's products collection.
                Product product = m_StoreController.products.WithID(productId);

                // If the look up found a product for this device's store and that product is ready to be sold ...
                if (product != null && product.availableToPurchase)
                {
                    UnityEngine.Console.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                    // ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed
                    // asynchronously.
                    m_StoreController.InitiatePurchase(product);
                }
                // Otherwise ...
                else
                {
                    // ... report the product look-up failure situation
                    UnityEngine.Console.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                    OnPurchaseHasFailed?.Invoke(productId, PurchaseFailureReason.ProductUnavailable);
                }
            }
            // Otherwise ...
            else
            {
                // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or
                // retrying initiailization.
                UnityEngine.Console.Log("BuyProductID FAIL. Not initialized.");
                OnPurchaseHasFailed?.Invoke(productId, PurchaseFailureReason.Unknown);
            }
        }

        // Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google.
        // Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
        public void RestorePurchases()
        {
            //// If Purchasing has not yet been set up ...
            //if (!IsInitialized())
            //{
            //    // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
            //    UnityEngine.Console.Log("RestorePurchases FAIL. Not initialized.");
            //    return;
            //}

            //// If we are running on an Apple device ...
            //if (Application.platform == RuntimePlatform.IPhonePlayer ||
            //    Application.platform == RuntimePlatform.OSXPlayer)
            //{
            //    // ... begin restoring purchases
            //    UnityEngine.Console.Log("RestorePurchases started ...");

            //    // Fetch the Apple store-specific subsystem.
            //    var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
            //    // Begin the asynchronous process of restoring purchases. Expect a confirmation response in
            //    // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
            //    apple.RestoreTransactions((result) =>
            //    {
            //        // The first phase of restoration. If no more responses are received on ProcessPurchase then
            //        // no purchases are available to be restored.
            //        UnityEngine.Console.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
            //    });
            //}
            //// Otherwise ...
            //else
            //{
            //    // We are not running on an Apple device. No work is necessary to restore purchases.
            //    UnityEngine.Console.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
            //}
        }

        //
        // --- IStoreListener
        //

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            // Purchasing has succeeded initializing. Collect our Purchasing references.
            //  UnityEngine.Console.Log("OnInitialized: PASS");

            // Overall Purchasing system, configured with products for this application.
            m_StoreController = controller;
            // Store specific subsystem, for accessing device-specific store features.
            m_StoreExtensionProvider = extensions;

            OnInitialize.Invoke();
        }

        //Custom Function to get local prices
        public string GetLocalizedPrice(string ProductTitle)
        {
            try
            {
                if (m_StoreController == null)
                    return null;

                if (m_StoreController.products == null)
                    return null;

                Product product = m_StoreController.products.WithID(ProductTitle);

                if (product == null)
                    return null;

                return product.metadata.localizedPriceString;

                //foreach (var product in m_StoreController.products.all)
                //{
                //    if (product.definition.id.Equals(ProductTitle))
                //    {
                //        return product.metadata.localizedPrice;
                //    }
                //}
            }
            catch
            {
                UnityEngine.Console.LogWarning($"Failed to get localized Price for ID {ProductTitle}. Error During Getting Prices");
                return null;
            }
        }

        //Custom Function to get ISOCode
        public string GetISOCode(string ProductTitle)
        {
            try
            {
                foreach (var product in m_StoreController.products.all)
                {
                    if (product.definition.id.Equals(ProductTitle))
                    {
                        return product.metadata.isoCurrencyCode;
                    }
                }
            }
            catch
            {
                UnityEngine.Console.LogWarning("Failed to get localized Prices. Error During Getting Prices");
                return "";
            }

            return "";
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
            // UnityEngine.Console.Log("OnInitializeFailed InitializationFailureReason:" + error);

            OnInitializationFailed?.Invoke();
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {

        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            OnPurchaseMade?.Invoke(args.purchasedProduct.definition.id);

            //if (String.Equals(args.purchasedProduct.definition.id, IAP_Strings.RemoveAds, StringComparison.Ordinal))
            //{
            //    UnityEngine.Console.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));

            //    //Save game both locally and on cloud
            //}
            //else
            //{
            //    UnityEngine.Console.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
            //}
            MaxAdMobController.SendPurchaseEventCustomMATS(args.purchasedProduct);
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing
            // this reason with the user to guide their troubleshooting actions.
            UnityEngine.Console.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));

            OnPurchaseHasFailed?.Invoke(product.transactionID, failureReason);
        }


    }
}