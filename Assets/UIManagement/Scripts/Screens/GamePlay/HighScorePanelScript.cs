using deVoid.UIFramework;
using Knights.UISystem;
using TheKnights.SaveFileSystem;
using TMPro;
using UnityEngine;

public class HighScorePanelScript : AWindowController
{
    [SerializeField] private TextMeshProUGUI statusTxt;
    [SerializeField] private GamePlaySessionInventory gameplayInventoryObj;
    [SerializeField] private PlayerCarLoadingHandler loadingHandler;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private CarsDataBase carsDataBase;
    public GameObject parentObjForCars;
    private GameObject displayGameObject;

    private void OnEnable()
    {
        //if (PlayerPrefs.GetInt("timesHighScoreObtainedString") >= 2)
        //{
        //    UnityEngine.Console.Log("Showing Rating");
        //    InGameRatingHandler.Instance.LaunchInAppReviewFlow();
        //}

        statusTxt.text = (gameplayInventoryObj.GetCurrentSessionScore()).ToString("00");

        var configData = carsDataBase.GetCarConfigurationData(saveManager.MainSaveFile.currentlySelectedCar);
       // loadingHandler.LoadAssets(configData);
        PlayerCarAssets playerCarAssets = loadingHandler.GetLoadedPlayerAssets(configData.GetName);
        displayGameObject = playerCarAssets.DisplayGameObject;

        CarGenerationDelay();
    }

  

    private void CarGenerationDelay()
    {
        GameObject currentDisplayingCar = Instantiate(displayGameObject, parentObjForCars.transform, false);
        currentDisplayingCar.GetComponent<Animator>().SetBool("carEnter", true);
    }
}