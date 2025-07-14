using deVoid.UIFramework;
using Knights.UISystem;
using System.Collections;
using System.Collections.Generic;
using TheKnights.Purchasing;
using TheKnights.SaveFileSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
//using Sirenix.OdinInspector;

public class UIAdditiveSceneUIController : MonoBehaviour
{
    [SerializeField] UnityEvent OnWindowOpened;
    [SerializeField] UnityEvent OnWindowClosed;
    public InventoryUpdateEvent celebrateInventoryUpdate/*, sendInfoToPanel*/;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private GameEvent cutSceneStarted;
    [SerializeField] private GameEvent settingsButtonClicked;

    [SerializeField] private GameEvent rewardedADFailedToShow_NotLoaded;
    [SerializeField] private UISettings uISettings = null;
    [SerializeField] private AdsController adsController;
    [SerializeField] private InventorySystem playerInventory;

    private UIFrame uiFrame;
    private UIScreenEvents uIScreenEvents;

    private bool shownAppUpdatePanel = false;
    private static int timesScriptStarted = 0;


    private void Awake()
    {
        uIScreenEvents = new UIScreenEvents();
        uiFrame = uISettings.CreateUIInstance(this.gameObject.scene, uIScreenEvents, false);

        uiFrame.OpenWindow(ScreenIds.MainMenuPanel);
        ForceUpdateAppValidator.NewAppVersionIsAvailable += ForceUpdateAppValidator_NewAppVersionIsAvailable;
    }


    private void PlayerInventory_OnCarsUnlocked(List<PlayableObjectDataWithIndex> obj)
    {
        uiFrame.OpenWindow(ScreenIds.CarUnlockedScreen, new CarsAvailableProperties() {CarsConfigData = obj });
    }

    private void OnDestroy()
    {
        ForceUpdateAppValidator.NewAppVersionIsAvailable -= ForceUpdateAppValidator_NewAppVersionIsAvailable;  
        playerInventory.OnCarsUnlocked -= PlayerInventory_OnCarsUnlocked;
    }

    private void ForceUpdateAppValidator_NewAppVersionIsAvailable()
    {
        if (!shownAppUpdatePanel && ForceUpdateAppValidator.IsPendingUpdate)
        {
            shownAppUpdatePanel = true;
            uiFrame.OpenWindow(ScreenIds.ForceUpdatePrompt);

        }
    }

    private IEnumerator Start()
    {
        timesScriptStarted++;

        //if(!saveManager.MainSaveFile.TutorialHasCompleted)
        //{
        //    uiFrame.MainCanvas.enabled = false;
        //}

        yield return new WaitForSeconds(1f);

        ForceUpdateAppValidator_NewAppVersionIsAvailable();

        var data = shopManager.CheckIfCarIsAvailableWithPlayerCoinsBeingMoreThanRequired(InventoryToItemPriceRelation.Fifth);

       // if(data.isValid)
       // {
           //// var carConfig = data.basicAssetInfo as CarConfigurationData;
           // UnityEngine.Console.Log("Car available with 5x coins " + carConfig.GetName);
           // CarAvailableProperties carAvailableProperties = new CarAvailableProperties() { CarConfigData = data };
            //uiFrame.OpenWindow<CarAvailableProperties>(ScreenIds.CarAvailablePopup, carAvailableProperties);
       // }
        //else
        //{
          //  UnityEngine.Console.Log("No available car with 5x coins");
        //}


      //  RateUs();
       // ShowRemoveADSPopupIfPossible();

    }

    private void RateUs()
    {
        if (timesScriptStarted % 2 == 0 && PlayerPrefs.GetInt("isAppRated", 0) == 0)
        {
            uiFrame.OpenWindow(ScreenIds.PanelAppRating);
        }
    }

    private void ShowRemoveADSPopupIfPossible()
    {
        if((timesScriptStarted == 3 || timesScriptStarted == 5) && !saveManager.MainSaveFile.isAdsPurchased)
        {
            uiFrame.OpenWindow(ScreenIds.RemoveADS_IAP_Popup);
        }
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        DeSubscribeToEvents();
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Escape))
    //    {
    //        uiFrame.OpenWindow(ScreenIds.QuitPromptGame);
    //    }
    //}

    private void SubscribeToEvents()
    {
        adsController.OnInterstitialAdShowRequested.AddListener(ShowInterstitialIsLoadingScreen);
        uIScreenEvents.WindowEvent.AddListener(InvokeWindowOperationEvents);
        rewardedADFailedToShow_NotLoaded.TheEvent.AddListener(OpenAdIsLoadingScreen);
        celebrateInventoryUpdate.celebrationBatchAdded.AddListener(ShowCelebrationDialouge);
        uIScreenEvents.PanelEvent.AddListener(ShowPanelScreen);
        uIScreenEvents.WindowEvent.AddListener(ShowWindowScreen);
        cutSceneStarted.TheEvent.AddListener(HandleGameStarted);
    //    onCarsUnlocked.TheEvent.AddListener(OpenCarSelectionScreen);
       // LeaderboardRewardManager.Instance.OnLeaderboardReward += LeaderboardRewardManager_OnLeaderboardReward;
        //LeaderboardRewardManager.Instance.OnLeaderboardRewardClosed += Instance_OnLeaderboardRewardClosed;
        // Settings Button Event Registered
        settingsButtonClicked.TheEvent.AddListener(OnSettingsButtonClicked);
        playerInventory.OnCarsUnlocked += PlayerInventory_OnCarsUnlocked;
        playerInventory.OnCharactersUnlocked += PlayerInventory_OnCharactersUnlocked;
        IAPManager.OnPurchaseFailed.AddListener(OpenPurchaseFailedPanel);
    }

    private void PlayerInventory_OnCharactersUnlocked(List<PlayableObjectDataWithIndex> obj)
    {
        uiFrame.OpenWindow(ScreenIds.CharacterUnlockedScreen, new CharactersAvailableProperties() {CharactersConfigData = obj });
    }

    private void OpenPurchaseFailedPanel(PurchaseFailureReason purchaseFailureReason)
    {
        ShowPanelScreen(ScreenIds.InformationPanelScreen, ScreenOperation.Open, new InformationPanelProperties()
        { TitleText = "Purchase Faliure!", InformationText = "Reason: " + purchaseFailureReason.ToString() });
    }

    //private void Instance_OnLeaderboardRewardClosed()
    //{
    //    uiFrame.CloseWindow(ScreenIds.RewardPanel);
    //}

    //private void LeaderboardRewardManager_OnLeaderboardReward()
    //{
    //    uiFrame.OpenWindow(ScreenIds.RewardPanel);
    //}

    private void DeSubscribeToEvents()
    {
        adsController.OnInterstitialAdShowRequested.RemoveListener(ShowInterstitialIsLoadingScreen);
        uIScreenEvents.WindowEvent.RemoveListener(InvokeWindowOperationEvents);
        rewardedADFailedToShow_NotLoaded.TheEvent.RemoveListener(OpenAdIsLoadingScreen);
        celebrateInventoryUpdate.celebrationBatchAdded.RemoveListener(ShowCelebrationDialouge);
        uIScreenEvents.PanelEvent.RemoveListener(ShowPanelScreen);
        uIScreenEvents.WindowEvent.RemoveListener(ShowWindowScreen);
        cutSceneStarted.TheEvent.RemoveListener(HandleGameStarted);
        //onCarsUnlocked.TheEvent.RemoveListener(OpenCarSelectionScreen);
        // LeaderboardRewardManager.Instance.OnLeaderboardReward -= LeaderboardRewardManager_OnLeaderboardReward;
        // LeaderboardRewardManager.Instance.OnLeaderboardRewardClosed -= Instance_OnLeaderboardRewardClosed;
        // Settings Button Event Removed
        settingsButtonClicked.TheEvent.RemoveListener(OnSettingsButtonClicked);
        playerInventory.OnCarsUnlocked -= PlayerInventory_OnCarsUnlocked;
        playerInventory.OnCharactersUnlocked -= PlayerInventory_OnCharactersUnlocked;
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

    private void HandleGameStarted(GameEvent theEvent)
    {
        StopAllCoroutines();
        uiFrame.CloseAllWindows();
        uiFrame.HideAllPanels();
    }

    private void OpenCarSelectionScreen(GameEvent gameEvent)
    {
       // uiFrame.OpenWindow(ScreenIds.CarSelectionPanel);
    }
    private void ShowCelebrationDialouge(List<InventoryItem<int>> batchToCelebrate, string context, bool isDoubleRewardPossible)
    {
        uiFrame.OpenWindow(ScreenIds.InventoryCelebrationPanel, new CelebrationBatchProperties() { BatchToBeCelebrated = batchToCelebrate, Context = context, isDoubleRewardPossible = isDoubleRewardPossible }) ;
      //  sendInfoToPanel.celebrationEvent.Invoke(itemName, itemValue);
    }

    private void OnSettingsButtonClicked(GameEvent theEvent)
    {
        //uiFrame.OpenWindow(ScreenIds.Settings);
        if (Debug.isDebugBuild)
        {
            UnityEngine.Console.Log("SettingsButtonClicked");
        }
    }

    public void ShowPanelScreen(string id, ScreenOperation screenOperation, IPanelProperties properties)
    {
        if (screenOperation == ScreenOperation.Open)
        {
            if(properties == null)
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

    public void ShowWindowScreen(string id, ScreenOperation screenOperation, IWindowProperties properties)
    {
        if (screenOperation == ScreenOperation.Open)
        {
            if(properties == null)
            {
                uiFrame.OpenWindow(id);
            }
            else
            {
                uiFrame.OpenWindow(id, properties);
            }
            
        }
        else
        {
            uiFrame.CloseWindow(id);
        }
    }

    private void OpenAdIsLoadingScreen(GameEvent gameEvent)
    {
        uiFrame.OpenWindow(ScreenIds.AdIsLoading);
    }
}