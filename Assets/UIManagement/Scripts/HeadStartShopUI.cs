using TMPro;
using UnityEngine;
using Knights.UISystem;
using UnityEngine.UI;
using deVoid.UIFramework;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasGroup))]
public class HeadStartShopUI : MonoBehaviour
{
    [SerializeField] private AWindowController windowControllerObj;
    [SerializeField] private InventorySystem inventoryObj;
    [SerializeField] private GameEvent updateUIEvent;
    [SerializeField] private GameObject notificationObj;
    [SerializeField] private TextMeshProUGUI noOfBoxTxt;
    [SerializeField] private ShopPurchaseEvent purchaseEvent;

    private void OnEnable() {
        updateUIEvent.TheEvent.AddListener(UpdateUIFunc);
        if (inventoryObj.GetIntKeyValue("GameHeadStart") > 0)
        {
            noOfBoxTxt.text = inventoryObj.GetIntKeyValue("GameHeadStart").ToString("00");
            notificationObj.SetActive(true);
            //gameObject.GetComponent<CanvasGroup>().alpha = 1.0f;
            //gameObject.SetActive(true);
            //notificationObj.SetActive(true);
        }
        else
        {
            //noOfBoxTxt.text = inventoryObj.GetIntKeyValue("GameHeadStart").ToString("00");
            notificationObj.SetActive(false);
        }
    }

    public void BuyHeadStart()
    {
        List<string> thingsGot = new List<string>();//z
        List<int> amountsGot = new List<int>();//z
        if (inventoryObj.GetIntKeyValue("AccountCoins") >= 100)
        {


            List<InventoryItem<int>> inventoryItems = new List<InventoryItem<int>>();
            inventoryItems.Add(new InventoryItem<int>("AccountCoins", -100));
            inventoryItems.Add(new InventoryItem<int>("GameHeadStart", 1, true));

            thingsGot.Add("HeadStart");
            amountsGot.Add(1);

            inventoryObj.UpdateKeyValues(inventoryItems, true , true, "HeadStartFromCoins_Shop");

            gameObject.GetComponent<CanvasGroup>().alpha = 1.0f;
            noOfBoxTxt.text = inventoryObj.GetIntKeyValue("GameHeadStart").ToString("00");
        }
        else
        {
            windowControllerObj.OpenTheWindow(ScreenIds.ResourcesNotAvailable);
        }
        purchaseEvent.RaiseEvent(thingsGot, "AccountCoins", 100, amountsGot);
    }



    //public void OpenHeadStartPanel() {
    //    if (inventoryObj.GetIntKeyValue("GameHeadStart") > 0)
    //    {
    //        windowControllerObj.OpenTheWindow(ScreenIds.MysteryBoxPanelShop);
    //    }
    //    else
    //    {
    //        //gameObject.SetActive(false);
    //    }
    //}

    void UpdateUIFunc(GameEvent theEvent) {
        //noOfBoxTxt.text = inventoryObj.GetIntKeyValue("GameHeadStart").ToString("00");

        if (inventoryObj.GetIntKeyValue("GameHeadStart") > 0)
        {
            //gameObject.GetComponent<CanvasGroup>().alpha = 1.0f;
            noOfBoxTxt.text = inventoryObj.GetIntKeyValue("GameHeadStart").ToString("00");
            notificationObj.SetActive(true);
            //    gameObject.SetActive(true);
            //    //notificationObj.SetActive(true);
        }
        else
        {
            //noOfBoxTxt.text = inventoryObj.GetIntKeyValue("GameHeadStart").ToString("00");
            notificationObj.SetActive(false);
        }
    }

    private void OnDisable()
    {
        updateUIEvent.TheEvent.RemoveListener(UpdateUIFunc);
    }
}
