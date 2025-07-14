using deVoid.UIFramework;
using Knights.UISystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class CarAvailableProperties : WindowProperties
{
    [HideInInspector] public PlayableObjectDataWithIndex CarConfigData;
}

public class CarIsAvailableUI : AWindowController<CarAvailableProperties>
{
    [SerializeField] private PlayerCarLoadingHandler loadingHandler;
    [SerializeField] private CarsDataBase carsDataBase;
    [SerializeField] private InventorySystem inventoryObj;
    [SerializeField] private ShopPurchaseEvent purchaseEvent;
    [SerializeField] private TextMeshProUGUI carNameText;
    [SerializeField] private TextMeshProUGUI carPriceText;
    [SerializeField] private GameObject closeButton;

    public GameObject parentObjForCars;

    private void OnEnable()
    {
        StartCoroutine(EnableCloseButtonAfterDelay());
    }


    private IEnumerator EnableCloseButtonAfterDelay()
    {
        yield return new WaitForSeconds(2);
        closeButton.SetActive(true);
    }

    private void CarGenerationDelay(CarConfigurationData configData)
    {
        var asyncHandle = loadingHandler.LoadAssets(configData);
        asyncHandle.AssetsLoaded += AsyncHandle_AssetsLoaded;
    }

    private void AsyncHandle_AssetsLoaded(PlayerCarAssets playerCarAssets)
    {
        GameObject currentDisplayingCar = Instantiate(playerCarAssets.DisplayGameObject, parentObjForCars.transform, false);
        currentDisplayingCar.GetComponent<Animator>().SetBool("carEnter", true);
    }

    protected override void OnPropertiesSet()
    {
        base.OnPropertiesSet();
        PlayableObjectDataWithIndex configData = Properties.CarConfigData;
        var carConfig = configData.basicAssetInfo as CarConfigurationData;
        carNameText.text = configData.basicAssetInfo.GetName + " available to be purchased!";
        carPriceText.text = carConfig.GetPrice.ToString();
        CarGenerationDelay(carConfig);
    }

    // Temporary buy method
    public void UnlockCarWithCoins()
    {
        PlayableObjectDataWithIndex configData = Properties.CarConfigData;

        List<string> thingsGot = new List<string>();//z
        List<int> amountsGot = new List<int>();//z

        var carConfig = configData.basicAssetInfo as CarConfigurationData;

        int price = carConfig.GetPrice;

        if (inventoryObj.GetIntKeyValue("AccountCoins") >= price)
        {
            CarAvailableProperties carAvailableProperties = Properties;

            UI_Close();
            OpenTheWindow(ScreenIds.CarSelectionPanel, carAvailableProperties);

            inventoryObj.UpdateKeyValues(new List<InventoryItem<int>>() { new InventoryItem<int>("AccountCoins", -price) });

            inventoryObj.UnlockCar(configData.index);

            thingsGot.Add($"Car{configData.index}");
            amountsGot.Add(1);
        
            // SendCarEvent("CarBuyWithCoin", carsDataBase.GetCarConfigurationData(selectedCarIndex).GetName);
        }
        else
        {
            UI_Close();
            OpenTheWindow(ScreenIds.ResourcesNotAvailable);
        }
        purchaseEvent.RaiseEvent(thingsGot, "AccountCoins", price, amountsGot);
    }

   
}