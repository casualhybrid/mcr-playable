using DG.Tweening.Core.Easing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheKnights.SaveFileSystem;
using UnityEngine;
using UnityEngine.Purchasing;

public class MATS_CheckIAP : MonoBehaviour
{
    private StoreController storeController;

    // Product IDs (make sure they match your store dashboard)
    private const string REMOVE_ADS = "com_minicarrush_removeads";
    private const string UNLOCK_ALL_CARS = "com_minicarrush_unlockallcars";
    private const string UNLOCK_ALL_CHARACTERS = "unlock_players";
    private const string UNLOCK_ALL_GAME = "unlock_all";

    public static bool adsRemoved;
    public static bool allCarsUnlocked;
    public static bool allCharactersUnlocked;
    public static bool allGame;
    public SaveManager saveManagerObj;
    [SerializeField] private InventorySystem playerInventory;
    [SerializeField] private GameEvent playerBoughtAllCars;
    async void Start()
    {
        await InitializeIAP();
    }

    async Task InitializeIAP()
    {
        storeController = UnityIAPServices.StoreController();

        // Attach handlers BEFORE connecting
        storeController.OnProductsFetched += OnProductsFetched;
        storeController.OnPurchasesFetched += OnPurchasesFetched;
        storeController.OnPurchaseFailed += OnPurchaseFailed;
        storeController.OnPurchasePending += OnPurchasePending;
        storeController.OnPurchaseDeferred += OnPurchaseDeferred;
        storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;

        try
        {
            await storeController.Connect();

            var initialProducts = new List<ProductDefinition>
            {
                new ProductDefinition(REMOVE_ADS, ProductType.NonConsumable),
                new ProductDefinition(UNLOCK_ALL_CARS, ProductType.NonConsumable),
                new ProductDefinition(UNLOCK_ALL_CHARACTERS, ProductType.NonConsumable),
                new ProductDefinition(UNLOCK_ALL_GAME, ProductType.NonConsumable)
            };

            storeController.FetchProducts(initialProducts);
        }
        catch (Exception ex)
        {
            Debug.LogError($"IAP connect/initialize failed: {ex}");
        }
    }

    private void OnProductsFetched(List<Product> products)
    {
        Debug.Log($"Products fetched: {products.Count}");
        storeController.FetchPurchases();
    }

    private void OnPurchasesFetched(Orders orders)
    {
        Debug.Log($"Purchases fetched: {orders.ConfirmedOrders.Count}");

        foreach (var confirmed in orders.ConfirmedOrders)
        {
            var info = confirmed.Info;
            if (info?.PurchasedProductInfo != null)
            {
                foreach (var pInfo in info.PurchasedProductInfo)
                {
                    Debug.Log($"Restored product: {pInfo.productId}");
                    ApplyEntitlement(pInfo.productId);
                }
            }
        }
    }

    private void ApplyEntitlement(string productId)
    {
        switch (productId)
        {
            case REMOVE_ADS:
                adsRemoved = true;
                PlayerPrefs.SetInt("IsAdsRemoved", 1);
                saveManagerObj.MainSaveFile.isAdsPurchased = true;
                saveManagerObj.SaveGame();

                Debug.Log("Entitlement applied: Remove Ads");
                break;

            case UNLOCK_ALL_CARS:
                allCarsUnlocked = true;
                PlayerPrefs.SetInt(UNLOCK_ALL_CARS, 1);

                playerBoughtAllCars.RaiseEvent();
                playerInventory.UnlockAllCars();
                Debug.Log("Entitlement applied: Unlock All Cars");
                break;

            case UNLOCK_ALL_CHARACTERS:
                allCharactersUnlocked = true;
                PlayerPrefs.SetInt(UNLOCK_ALL_CHARACTERS, 1);
                playerInventory.UnlockAllCharacters();
                Debug.Log("Entitlement applied: Unlock All Characters");
                break;
            case UNLOCK_ALL_GAME:
                allGame = true;
                playerBoughtAllCars.RaiseEvent();
                playerInventory.UnlockAllCars();
                PlayerPrefs.SetInt("IsAdsRemoved", 1);
                saveManagerObj.MainSaveFile.isAdsPurchased = true;
                playerInventory.UnlockAllCharacters();
                PlayerPrefs.SetInt(UNLOCK_ALL_GAME, 1);
                Debug.Log("Entitlement applied: Unlock All Game");
                break;
        }
    }

    public void RestorePurchases()
    {
        storeController.RestoreTransactions((success, message) =>
        {
            Debug.Log($"RestoreTransactions finished: success={success}, message={message}");
            if (success)
            {
                storeController.FetchPurchases();
            }
        });
    }

    // --- Purchase event handlers (correct v5 API) ---

    private void OnPurchaseConfirmed(Order order)
    {

        Debug.Log($"✅ Purchase confirmed. TxID: {order.Info.TransactionID}");

        foreach (var pInfo in order.Info.PurchasedProductInfo)
        {
            Debug.Log($"Confirmed product: {pInfo.productId}");
            ApplyEntitlement(pInfo.productId);
        }
    }

    private void OnPurchaseFailed(FailedOrder failedOrder)
    {
        Debug.LogError($"❌ Purchase failed. Reason: {failedOrder.FailureReason}, Message: {failedOrder.Info.TransactionID}");
    }

    private void OnPurchasePending(PendingOrder order)
    {
        Debug.Log($"⌛ Purchase pending. TxID: {order.Info.TransactionID}");
    }

    private void OnPurchaseDeferred(DeferredOrder order)
    {
        Debug.Log($"⏸ Purchase deferred. TxID: {order.Info.TransactionID}");
    }
}
