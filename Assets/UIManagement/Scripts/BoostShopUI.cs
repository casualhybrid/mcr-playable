using deVoid.UIFramework;
using Knights.UISystem;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class BoostShopUI : MonoBehaviour
{
    [SerializeField] private AWindowController windowControllerObj;
    [SerializeField] private InventorySystem inventoryObj;
    [SerializeField] private ShopPurchaseEvent purchaseEvent;
    //[SerializeField] private GameObject notificationObj; //zaid
    //[SerializeField] private TextMeshProUGUI noOfBoxTxt; //zaid
    [SerializeField] private GameEvent updateUIEvent;

    private void OnEnable()
    {
        updateUIEvent.TheEvent.AddListener(UpdateUIFunc);
        //if (inventoryObj.GetIntKeyValue("GameBoost") > 0)
        //{
        //    //noOfBoxTxt.text = inventoryObj.GetIntKeyValue("GameMysteryBox").ToString("00");
        //    gameObject.GetComponent<CanvasGroup>().alpha = 1.0f;
        //    gameObject.SetActive(true);
        //    //notificationObj.SetActive(true);
        //}
        //else
        //{
        //    //gameObject.SetActive(false);
        //    gameObject.GetComponent<CanvasGroup>().alpha = 0.6f;
        //    //notificationObj.SetActive(false);
        //}
    }

    public void BuyGameBoost()
    {
        List<string> thingsGot = new List<string>();//z
        List<int> amountsGot = new List<int>();//z
        if (inventoryObj.GetIntKeyValue("AccountCoins") >= 200)
        {
            List<InventoryItem<int>> inventoryItems = new List<InventoryItem<int>>();
            inventoryItems.Add(new InventoryItem<int>("AccountCoins", -200)); 
            inventoryItems.Add(new InventoryItem<int>("GameBoost", 1, true));

            thingsGot.Add("Boost");
            amountsGot.Add(1);

            inventoryObj.UpdateKeyValues(inventoryItems, true, true, "BoostFromCoins_Shop");

            gameObject.GetComponent<CanvasGroup>().alpha = 1.0f;
            //noOfBoxTxt.text = inventoryObj.GetIntKeyValue("GameMysteryBox").ToString("00");
        }
        else
        {
            windowControllerObj.OpenTheWindow(ScreenIds.ResourcesNotAvailable);
        }
        purchaseEvent.RaiseEvent(thingsGot, "AccountCoins", 200, amountsGot);
    }

    //public void OpenGameBoostPanel()
    //{
    //    if (inventoryObj.GetIntKeyValue("GameBoost") > 0)
    //    {
    //        windowControllerObj.OpenTheWindow(ScreenIds.MysteryBoxPanelShop);
    //    }
    //    else
    //    {
    //        //gameObject.SetActive(false);
    //    }
    //}

    private void UpdateUIFunc(GameEvent theEvent)
    {
        //noOfBoxTxt.text = inventoryObj.GetIntKeyValue("GameMysteryBox").ToString("00");

        if (inventoryObj.GetIntKeyValue("GameBoost") > 0)
        {
            gameObject.GetComponent<CanvasGroup>().alpha = 1.0f;
            //noOfBoxTxt.text = inventoryObj.GetIntKeyValue("GameMysteryBox").ToString("00");
            gameObject.SetActive(true);
            //notificationObj.SetActive(true);
        }
        else
        {
            //gameObject.SetActive(false);
            gameObject.GetComponent<CanvasGroup>().alpha = 0.6f;
            //notificationObj.SetActive(false);
        }
    }

    private void OnDisable()
    {
        updateUIEvent.TheEvent.RemoveListener(UpdateUIFunc);
    }
}