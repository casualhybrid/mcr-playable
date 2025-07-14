using deVoid.UIFramework;
using Knights.UISystem;
using System.Collections.Generic;
using TheKnights.Purchasing;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;

public class GamePlayUIController : MonoBehaviour
{
    [SerializeField] private UnityEvent OnWindowOpened;
    [SerializeField] private UnityEvent OnWindowClosed;

    [SerializeField] private InventoryUpdateEvent celebrateInventoryUpdate/*, sendInfoToPanel*/;
    [SerializeField] private GameEvent cutSceneStarted;
    [SerializeField] private GameEvent rewardedADFailedToShow_NotLoaded;
    [SerializeField] private GameEvent onGameOver;
    [SerializeField] private UISettings uISettings = null;
    [SerializeField] private GameEvent showRevivePanel;
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private ScriptableObject GameOverManager;
    [SerializeField] private AdsController adsController;
    [SerializeField] private float checkForAvailableCarInterval = 5;

    private float elapsedCheckForAvailableCarDuration = 0;
    private UIFrame uiFrame;
    private UIScreenEvents uIScreenEvents;

    private void Awake()
    {
        uIScreenEvents = new UIScreenEvents();
        uiFrame = uISettings.CreateUIInstance(this.gameObject.scene, uIScreenEvents, false);

        // Rest of events will be subscribed in handle game started
        cutSceneStarted.TheEvent.AddListener(HandleGameStarted);
    }

    private void OnDestroy()
    {
        DeSubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        onGameOver.TheEvent.AddListener(CloseAllScreens);
        adsController.OnInterstitialAdShowRequested.AddListener(ShowInterstitialIsLoadingScreen);
        uIScreenEvents.WindowEvent.AddListener(InvokeWindowOperationEvents);
        rewardedADFailedToShow_NotLoaded.TheEvent.AddListener(OpenAdIsLoadingScreen);
        celebrateInventoryUpdate.celebrationBatchAdded.AddListener(ShowCelebrationDialouge);
        uIScreenEvents.PanelEvent.AddListener(ShowPanelScreen);
        uIScreenEvents.WindowEvent.AddListener(ShowWindowScreen);
        showRevivePanel.TheEvent.AddListener(ShowRevivePanel);
        IAPManager.OnPurchaseFailed.AddListener(OpenPurchaseFailedPanel);
    }

    private void DeSubscribeToEvents()
    {
        onGameOver.TheEvent.RemoveListener(CloseAllScreens);
        adsController.OnInterstitialAdShowRequested.RemoveListener(ShowInterstitialIsLoadingScreen);
        uIScreenEvents.WindowEvent.RemoveListener(InvokeWindowOperationEvents);
        rewardedADFailedToShow_NotLoaded.TheEvent.RemoveListener(OpenAdIsLoadingScreen);
        celebrateInventoryUpdate.celebrationBatchAdded.RemoveListener(ShowCelebrationDialouge);
        uIScreenEvents.PanelEvent.RemoveListener(ShowPanelScreen);
        uIScreenEvents.WindowEvent.RemoveListener(ShowWindowScreen);
        cutSceneStarted.TheEvent.RemoveListener(HandleGameStarted);
        IAPManager.OnPurchaseFailed.RemoveListener(OpenPurchaseFailedPanel);
    }

    private void ShowInterstitialIsLoadingScreen()
    {
        uiFrame.OpenWindow(ScreenIds.AdIsLoadingBeforeCutScene);
    }

    private void InvokeWindowOperationEvents(string panelName, ScreenOperation screenOperation, IWindowProperties properties)
    {
        if (screenOperation == ScreenOperation.Open)
        {
            OnWindowOpened.Invoke();
        }
        else
        {
            OnWindowClosed.Invoke();
        }
    }

    private void Update()
    {
        if (!GameManager.IsGameStarted)
            return;

        elapsedCheckForAvailableCarDuration += Time.deltaTime;

        if (elapsedCheckForAvailableCarDuration > checkForAvailableCarInterval)
        {
            elapsedCheckForAvailableCarDuration = 0;
            CheckAndShowCarAvailableDialogue();
        }
    }

    private void HandleGameStarted(GameEvent theEvent)
    {
        SubscribeToEvents();

        //uiFrame.ShowScreen(ScreenIds.PauseButton);
        ShowWindowScreen(ScreenIds.HUDPanel, ScreenOperation.Open);
    }

    private void OpenPurchaseFailedPanel(PurchaseFailureReason purchaseFailureReason)
    {
        ShowPanelScreen(ScreenIds.InformationPanelScreen, ScreenOperation.Open, new InformationPanelProperties()
        { TitleText = "Purchase Faliure!", InformationText = "Reason: " + purchaseFailureReason.ToString() });
    }

    private void ShowRevivePanel(GameEvent theEvent)
    {
        ShowWindowScreen(ScreenIds.RevivePanel, ScreenOperation.Open);
    }

    public void ShowPanelScreen(string id, ScreenOperation screenOperation, IPanelProperties properties = null)
    {
        if (screenOperation == ScreenOperation.Open)
        {
            if (properties == null)
            {
                uiFrame.ShowPanel(id);
            }
            else
            {
                uiFrame.ShowPanel(id, properties);
            }
        }
        else
        {
            uiFrame.HidePanel(id);
        }
    }

  
    public void ShowWindowScreen(string id, ScreenOperation screenOperation, IWindowProperties windowProperties = null)
    {
        if (screenOperation == ScreenOperation.Open)
        {
            if (windowProperties != null)
            {
                uiFrame.OpenWindow(id, windowProperties);
            }
            else
            {
                uiFrame.OpenWindow(id);
            }

;
        }
        else
        {
            uiFrame.CloseWindow(id);
        }
    }

    private void ShowCelebrationDialouge(List<InventoryItem<int>> batchToCelebrate, string context, bool isDoubleRewardPossible)
    {
        uiFrame.OpenWindow(ScreenIds.InventoryCelebrationPanel, new CelebrationBatchProperties() { BatchToBeCelebrated = batchToCelebrate, Context = context , isDoubleRewardPossible = isDoubleRewardPossible }) ;
        //  sendInfoToPanel.celebrationEvent.Invoke(itemName, itemValue);
    }

    private void OpenAdIsLoadingScreen(GameEvent gameEvent)
    {
        uiFrame.OpenWindow(ScreenIds.AdIsLoading);
    }

    private void CheckAndShowCarAvailableDialogue()
    {
        var data = shopManager.GetAnyAvailableToBuyCar();

        if (!data.isValid)
            return;

        CarConfigurationData carData = data.basicAssetInfo as CarConfigurationData;

        UnityEngine.Console.Log($"Availalbe Car Is {carData.GetName}");

        uiFrame.ShowPanel("CarAvailableInGamePanel", new CarAvailablePanelProperties() { CarConfigData = new PlayableObjectDataWithIndex(carData, -1) });
    }

    private void CloseAllScreens(GameEvent gameEvent)
    {
        uiFrame.CloseAllWindows();
    }


}