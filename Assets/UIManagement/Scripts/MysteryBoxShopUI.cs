using deVoid.UIFramework;
using DG.Tweening;
using Knights.UISystem;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class MysteryBoxShopUI : MonoBehaviour
{
    [SerializeField] private AWindowController windowControllerObj;
    [SerializeField] private InventorySystem inventoryObj;
    [SerializeField] private ShopPurchaseEvent purchaseEvent;

    [SerializeField] private GameObject notificationObj; //zaid
    [SerializeField] private TextMeshProUGUI noOfBoxTxt; //zaid
    [SerializeField] private GameEvent updateUIEvent;
    [SerializeField] private Button openBtn;
    [SerializeField] private GameObject icon;


    private void Start()
    {
        if (inventoryObj.GetIntKeyValue("GameMysteryBox") > 0)
        {
            if (noOfBoxTxt != null)
            {
                noOfBoxTxt.text = inventoryObj.GetIntKeyValue("GameMysteryBox").ToString("00");
            }

            gameObject.GetComponent<CanvasGroup>().alpha = 1.0f;
            gameObject.SetActive(true);

            if (notificationObj != null)
            {
                notificationObj.SetActive(true);
            }
            if (openBtn != null)
            {
                openBtn.interactable = true;
            }
            if (icon != null)
            {
                icon.GetComponent<DOTweenAnimation>().DOPlay();
            }
        }
        else
        {
            //   gameObject.SetActive(false);
            gameObject.GetComponent<CanvasGroup>().alpha = 1.0f; //changed it from 0.6 to 1.0;

            if (notificationObj != null)
            {
                notificationObj.SetActive(false);
            }
            if (openBtn != null)
            {
                openBtn.interactable = false;
            }
            if (icon != null)
            {
                icon.GetComponent<DOTweenAnimation>().DOPause();
            }
        }
    }
    private void OnEnable()
    {
        updateUIEvent.TheEvent.AddListener(UpdateUIFunc);
        if (inventoryObj.GetIntKeyValue("GameMysteryBox") > 0)
        {
            if (noOfBoxTxt != null)
            {
                noOfBoxTxt.text = inventoryObj.GetIntKeyValue("GameMysteryBox").ToString("00");
            }

            gameObject.GetComponent<CanvasGroup>().alpha = 1.0f;
            gameObject.SetActive(true);

            if (notificationObj != null)
            {
                notificationObj.SetActive(true);
            }
            if (openBtn != null)
            {
                openBtn.interactable = true;
            }
            if(icon != null)
            {
                icon.GetComponent<DOTweenAnimation>().DOPlay();
            }
        }
        else
        {
         //   gameObject.SetActive(false);
            gameObject.GetComponent<CanvasGroup>().alpha = 1.0f; //changed it from 0.6 to 1.0;

            if (notificationObj != null)
            {
                notificationObj.SetActive(false);
            }
            if(openBtn != null)
            {
                openBtn.interactable = false;
            }
            if (icon != null)
            {
                icon.GetComponent<DOTweenAnimation>().DOPause();
            }
        }
    }

    public void BuyMysteryBox()
    {
        List<string> thingsGot = new List<string>();//z
        List<int> amountsGot = new List<int>();//z

        if (inventoryObj.GetIntKeyValue("AccountCoins") >= 100)
        {
            List<InventoryItem<int>> inventoryItems = new List<InventoryItem<int>>();
            inventoryItems.Add(new InventoryItem<int>("AccountCoins", - 100));
            inventoryItems.Add(new InventoryItem<int>("GameMysteryBox", 1, true));

            thingsGot.Add("Mystery Box");
            amountsGot.Add(1);

            inventoryObj.UpdateKeyValues(inventoryItems, true, true, "MysteryBoxFromCoins_Shop");

            gameObject.GetComponent<CanvasGroup>().alpha = 1.0f;

            if (noOfBoxTxt != null)
            {
                noOfBoxTxt.text = inventoryObj.GetIntKeyValue("GameMysteryBox").ToString("00");
            }
        }
        else
        {
            windowControllerObj.OpenTheWindow(ScreenIds.ResourcesNotAvailable);
        }

        purchaseEvent.RaiseEvent(thingsGot, "AccountCoins", 100, amountsGot);
    }

    public void BuyMysteryBoxAD()
    {
       
            List<InventoryItem<int>> inventoryItems = new List<InventoryItem<int>>();
            inventoryItems.Add(new InventoryItem<int>("GameMysteryBox", 1, true));
            inventoryObj.UpdateKeyValues(inventoryItems, true, true, "MysteryBoxAD_Shop");

            if (noOfBoxTxt != null)
            {
                noOfBoxTxt.text = inventoryObj.GetIntKeyValue("GameMysteryBox").ToString("00");
            }
        
    }

    public void OpenMysteryBoxPanel()
    {
        if (inventoryObj.GetIntKeyValue("GameMysteryBox") > 0)
        {
            windowControllerObj.OpenTheWindow(ScreenIds.MysteryBoxPanelShop);
        }
        //else
        //{
        //    gameObject.SetActive(false);
        //}
    }

    private void UpdateUIFunc(GameEvent theEvent)
    {
        if (noOfBoxTxt != null)
        {
            noOfBoxTxt.text = inventoryObj.GetIntKeyValue("GameMysteryBox").ToString("00");
        }

        if (inventoryObj.GetIntKeyValue("GameMysteryBox") > 0)
        {
            gameObject.GetComponent<CanvasGroup>().alpha = 1.0f;

            if (noOfBoxTxt != null)
            {
                noOfBoxTxt.text = inventoryObj.GetIntKeyValue("GameMysteryBox").ToString("00");
            }
            if (icon != null)
            {
                icon.GetComponent<DOTweenAnimation>().DOPlay();
            }

            gameObject.SetActive(true);

            if (notificationObj != null)
            {
                notificationObj.SetActive(true);

            }
            if (openBtn != null)
            {
                openBtn.interactable = true;
            }
           
        }
        else
        {
           // gameObject.SetActive(false);
            gameObject.GetComponent<CanvasGroup>().alpha = 1.0f;    //changed it from 0.6 to 1.0;

            if (icon != null)
            {
                icon.GetComponent<DOTweenAnimation>().DOPause();
            }
            if (notificationObj != null)
            {
                notificationObj.SetActive(false);
            }
            if (openBtn != null)
            {
                openBtn.interactable = false;
            }
            
        }
    }

    private void OnDisable()
    {
        updateUIEvent.TheEvent.RemoveListener(UpdateUIFunc);
    }
}