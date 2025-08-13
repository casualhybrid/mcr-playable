using deVoid.UIFramework;
using Knights.UISystem;
using System;
using System.Collections;
using System.Collections.Generic;
using TheKnights.Purchasing;
using TheKnights.SaveFileSystem;
using TMPro;
using UnityEngine;

using UnityEngine.Purchasing;
using UnityEngine.UI;

public class ShopManagerUI : AWindowController
{
    [SerializeField] GameObject profile;
    public SaveManager saveManagerObj;
    public IAPManager iAPManagerObj;
    public InventorySystem inventoryObj;
    public ShopPurchaseEvent purchaseEvent;

    public TextMeshProUGUI[] textObj;

    private GeneralIAPItem purchaseItemID;

    [SerializeField] private GameEvent playerBoughtAdFree;
    [SerializeField] private GameEvent uiCounterEvent;

    [SerializeField] private GameObject BuyAdFreePanel;
   


    //zaid
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private ScrollRect scrollRectGoldDiamond;
    [SerializeField] private RectTransform boostCard;
    [SerializeField] private RectTransform contentPanel;
    [SerializeField] private Sprite[] BtnSprite;
    [SerializeField] private GameObject[] BtnImg, offerPanels;

    private Action pendingPurchasedItemResult;

    private static int timesOpened;

    [SerializeField] GameObject inventoryPanel;
    protected override void Awake()
    {
        base.Awake();
    }

  

    private void OnEnable()
    {
        if (profile)
            profile.SetActive(false);
        InventoryCelebrationPanel.isShop = true;
        PersistentAudioPlayer.Instance.PanelSounds();
        StartCoroutine(WaitForAFrameAndEnableScrollRect());
        SnapTo();
        //iAPManagerObj.OnIAPInitializationFailed.AddListener(() => { ChangeFeedBackText("Initialization Failed"); });
        //iAPManagerObj.OnIAPInitialized.AddListener(() => { InAppInitialization("Initialization Successfull"); });
        //iAPManagerObj.OnPurchaseFailed.AddListener((reason) => { PurchaseFailed("Purchase Failed. Reason: " + reason); });
        //iAPManagerObj.OnPurchaseSuccessfull.AddListener(() => { PurchaseSuccessful("Purchase Successfull"); });

        IAPManager.OnIAPInitializationFailed.AddListener(ChangeFeedBackText);
        IAPManager.OnIAPInitialized.AddListener(InAppInitialization);
        IAPManager.OnPurchaseFailed.AddListener(PurchaseFailed);
        IAPManager.OnPurchaseSuccessfull.AddListener(PurchaseSuccessful);

       StartCoroutine(InitializeUICounterEvent());

        if (inventoryObj.saveManagerObj.MainSaveFile.isAdsPurchased == true)
        {
            BuyAdFreePanel.SetActive(false);
            //contentPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(1080, /*3175*/5110);
        }
        BtnSelect(1);

        timesOpened++;
        inventoryPanel = WindowParaLayer.instance.Get();
       // OpenRemoveADSPopupIfPossible();
    }


    private void OpenRemoveADSPopupIfPossible()
    {
        if (timesOpened == 1)
            return;

        if(timesOpened % 2 != 0 && !saveManagerObj.MainSaveFile.isAdsPurchased)
        {
            //OpenTheWindow(ScreenIds.RemoveADS_IAP_Popup);
        }
    }

    private IEnumerator WaitForAFrameAndEnableScrollRect()
    {
        yield return null;
        scrollRect.enabled = true;
        yield return null;
        scrollRectGoldDiamond.enabled = true;

    }

    private void Select(int num)
    {
        BtnImg[num].GetComponent<Image>().sprite = BtnSprite[1];
        offerPanels[num].SetActive(true);
        BtnImg[num].transform.SetAsLastSibling();
    }
    private void DeSelect(int num)
    {
        BtnImg[num].GetComponent<Image>().sprite = BtnSprite[0];
        offerPanels[num].SetActive(false);
    }
    public void BtnSelect(int num)
    {
        if (num == 0)
        {
            Select(0);
            DeSelect(1);
        }
        else
        {
            Select(1);
            DeSelect(0);
        }

    }
    // Ammmm
    private IEnumerator InitializeUICounterEvent()
    {
     //   await Task.Delay(TimeSpan.FromMilliseconds(100));   // Making sure all components are enabled
        yield return null;
        uiCounterEvent.RaiseEvent(); // Event is raised so that we can save initial state of inventory
    }

    public void BuyAdsWithDiamonds()
    {

        List<string> thingsGot = new List<string>();//z
        List<int> amountsGot = new List<int>();//z
        if (inventoryObj.GetIntKeyValue("AccountDiamonds") >= 100)
        {
            inventoryObj.saveManagerObj.MainSaveFile.isAdsPurchased = true;
            inventoryObj.UpdateKeyValues(new List<InventoryItem<int>>() { new InventoryItem<int>("AccountDiamonds", -100) }, false);
            playerBoughtAdFree.RaiseEvent();

            thingsGot.Add("No Ads Package");
            amountsGot.Add(1);

            OpenTheWindow(ScreenIds.PurchaseSuccess);
            BuyAdFreePanel.SetActive(false);
            //zaid
            //contentPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(1080, /*4505*/5110);
        }
        else
        {
            print("Item is not purchaseable...");

            OpenTheWindow(ScreenIds.ResourcesNotAvailable);
        }
        purchaseEvent.RaiseEvent(thingsGot, "AccountDiamonds",  100, amountsGot);
    }

    public void SnapTo(/*RectTransform target*/)
    {
       // Canvas.ForceUpdateCanvases();
        contentPanel.anchoredPosition = new Vector2(0, 0);
        //contentPanel.anchoredPosition =
        //        (Vector2)scrollRect.transform.InverseTransformPoint(contentPanel.position)
        //        - (Vector2)scrollRect.transform.InverseTransformPoint(target.position);
    }

    public void SnapToBoost(/*RectTransform target*/)
    {
        //Canvas.ForceUpdateCanvases();
        Vector2 requiredPos = (Vector2)scrollRect.transform.InverseTransformPoint(contentPanel.position)
                - (Vector2)scrollRect.transform.InverseTransformPoint(boostCard.position);

        contentPanel.anchoredPosition = new Vector2(0, (requiredPos.y - 600));
    }

    /// <summary>
    ///     This function is solo implementation for Shop Panel in Main Menu... and needed to be reset
    /// </summary>
    /// <param name="itemObj"></param>
    public void BuySelectedBundleWithDiamonds(GeneralIAPItem itemObj)
    {
        List<InventoryItem<int>> inventoryItems = new List<InventoryItem<int>>();
        List<string> thingsGot = new List<string>();//z
        List<int> amountsGot = new List<int>();//z
        bool isPurchaseble = false;
        foreach (ItemWithAmount itemWithPrice in itemObj.inventoryItemsPrice)
        {
            if (itemWithPrice.GetAmount > inventoryObj.GetIntKeyValue("AccountDiamonds"))
            {
                isPurchaseble = false;
                break;
            }

            isPurchaseble = true;
        }

        if (isPurchaseble)
        {

            for (int i = 0; i < itemObj.inventoryItemsToGive.Length; i++)
            {
                ItemWithAmount itemWithAmount = itemObj.inventoryItemsToGive[i];
                inventoryItems.Add(new InventoryItem<int>(itemWithAmount.GetItemType.GetKey, itemWithAmount.GetAmount, true));
                thingsGot.Add(itemWithAmount.GetItemType.GetKey);//z
                amountsGot.Add(itemWithAmount.GetAmount);//z
            }

            inventoryItems.Add(new InventoryItem<int>("AccountDiamonds", -itemObj.inventoryItemsPrice[0].GetAmount));


            inventoryObj.UpdateKeyValues(inventoryItems,true,true,null,false);

            SendCoinPackageClickeAnalyticsEvent(itemObj.name, true);
        }
        else
        {
            //   print("Item is not purchaseable...");

            SendCoinPackageClickeAnalyticsEvent(itemObj.name, false);
            OpenTheWindow(ScreenIds.ResourcesNotAvailable);
        }


        purchaseEvent.RaiseEvent(thingsGot, "AccountDiamonds", itemObj.inventoryItemsPrice[0].GetAmount, amountsGot);
    }

    private void SendCoinPackageClickeAnalyticsEvent(string productID, bool purchaseStatus)
    {
        string purchaseStatusString = "Failed";

        if (purchaseStatus)
        {
            purchaseStatusString = "Success";
        }

        AnalyticsManager.CustomData("Store_CoinPackage_Buy", new Dictionary<string, object>()
        {
            {"productID",productID},
            {"Status",purchaseStatusString}
        });

    }

    /// <summary>
    ///     This function is solo implementation for Shop Panel in Main Menu... and needed to be reset
    /// </summary>
    /// <param name="itemObj"></param>
    public void BuySelectedBundleWithMoney(GeneralIAPItem itemObj)
    {
        purchaseItemID = itemObj;


        List<string> thingsGot = new List<string>();//z
        List<int> amountsGot = new List<int>();//z

        for (int i = 0; i < itemObj.inventoryItemsToGive.Length; i++)
        {
            ItemWithAmount itemWithAmount = itemObj.inventoryItemsToGive[i];
            thingsGot.Add(itemWithAmount.GetItemType.GetKey);//z
            amountsGot.Add(itemWithAmount.GetAmount);//z
        }

        pendingPurchasedItemResult = () => {
            purchaseEvent.RaiseEvent(thingsGot, "Dollars", itemObj.ProductID[itemObj.ProductID.Length - 1], amountsGot);
        };


        iAPManagerObj.BuyTheProduct(itemObj.ProductID);

 


    }
    public void StandardPAck(GeneralIAPItem itemObj)
    {
        iAPManagerObj.BuyTheProduct(itemObj.ProductID);
        inventoryObj.saveManagerObj.MainSaveFile.isAdsPurchased = true;

    }

    public void RewardCoinPosition(RectTransform target)
    {
        Vector2 globalPos = target.position; // World position
        Vector2 localPoint;
        RectTransform canvasRect = target.GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(null, globalPos),
            null,
            out localPoint
        );

        UIEffectsHandler.newPosition = localPoint;

        // UIEffectsHandler.newPosition = target.position; // World position as Vector2
        Debug.Log("Position : " + target.position + localPoint);

        //UIEffectsHandler.newPosition = new Vector2(232f, 585f);
    }
    public void StandardPack(GeneralIAPItem itemObj)
    {
        iAPManagerObj.BuyTheProduct(itemObj.ProductID);
        inventoryObj.UpdateKeyValues(new List<InventoryItem<int>>() { new InventoryItem<int>("AccountCoins", 2000) });
        inventoryObj.UpdateKeyValues(new List<InventoryItem<int>>() { new InventoryItem<int>("AccountDiamonds", 300) });
        inventoryObj.saveManagerObj.MainSaveFile.isAdsPurchased = true;

        playerBoughtAdFree.RaiseEvent();
        OpenTheWindow(ScreenIds.PurchaseSuccess);
        BuyAdFreePanel.SetActive(false);
    }

    private void PurchaseSuccessful()
    {
        if (pendingPurchasedItemResult == null)
            return;

      //  OpenTheWindow(ScreenIds.PurchaseSuccess);

        pendingPurchasedItemResult?.Invoke();
    }

    private void PurchaseFailed(PurchaseFailureReason reason)
    {
     //   OpenTheWindow(ScreenIds.ResourcesNotAvailable);

       // pendingPurchasedItemResult?.Invoke();

    }

    private void InAppInitialization()
    {
        //textObj[0].text = iAPManagerObj.GetlocalizedPriceString("com_minicarrush_iapbundle_1");
        //textObj[1].text = iAPManagerObj.GetlocalizedPriceString("com_minicarrush_iapbundle_2");
        //textObj[2].text = iAPManagerObj.GetlocalizedPriceString("com_minicarrush_iapbundle_3");
        //textObj[3].text = iAPManagerObj.GetlocalizedPriceString("com_minicarrush_iapbundle_4");
        //textObj[4].text = iAPManagerObj.GetlocalizedPriceString("com_minicarrush_iapbundle_5");
    }

    private void ChangeFeedBackText()
    {
        print("IAP Status = " + /*text*/"Initialization Failed");
    }

    public void CloseShopPanel()
    {
        PersistentAudioPlayer.Instance.PlayAudio();
        InventoryCelebrationPanel.isShop = false;
        inventoryPanel = WindowParaLayer.instance.Get();
        //Debug.LogError(inventoryPanel.name);
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
        //  saveManagerObj.SaveGame(0, false);
        this.UI_Close();
    }

    private void OnDisable()
    {
        scrollRectGoldDiamond.enabled = false;
        scrollRect.enabled = false;

        IAPManager.OnIAPInitializationFailed.RemoveListener(ChangeFeedBackText);
        IAPManager.OnIAPInitialized.RemoveListener(InAppInitialization);
        IAPManager.OnPurchaseFailed.RemoveListener(PurchaseFailed);
        IAPManager.OnPurchaseSuccessfull.RemoveListener(PurchaseSuccessful);

    }


    public void SendEvents(string theEvent)
    {
        AnalyticsManager.CustomData(theEvent);
    }

    public void OpenUnlockAllCarsScreenIAP()
    {
        OpenTheWindow(ScreenIds.UnlockCars_IAP_Popup);
    }

    public void OpenUnlockAllCharactersScreenIAP()
    {
        OpenTheWindow(ScreenIds.UnlockCharacters_IAP_Popup);
    }

    public void OpenUnlockAllCharactersAndCarsScreenIAP()
    {
        OpenTheWindow(ScreenIds.UnlockCarsAndCharacters);
    }
}
