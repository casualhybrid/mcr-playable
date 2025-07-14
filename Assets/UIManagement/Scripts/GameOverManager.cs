using UnityEngine;
using System.Collections;
[CreateAssetMenu(fileName = "GameOverManager", menuName = "ScriptableObjects/GameOverManager")]
public class GameOverManager : ScriptableObject
{
    [SerializeField] private AdsController adsController;
    [SerializeField] private GameEvent playerHasCrahsed, showRevivePanel/*, showGameOverPanel*/;
    private bool revivebool/*, gameoverbool*/;
  //  [SerializeField] private InventorySystem playerInventoryObj;
   // [SerializeField] private GamePlaySessionInventory sessionInventoryObj;

    private void OnEnable()
    {
        revivebool = true;
        playerHasCrahsed.TheEvent.AddListener(DisplayGameOverUI);
    }

    private void OnDisable()
    {
        playerHasCrahsed.TheEvent.RemoveListener(DisplayGameOverUI);
    }

    private void DisplayGameOverUI(GameEvent gameEvent)
    {
        //if (!GameManager.IsGameStarted)
        //    return;

        //if(hasPlayerEnoughDiamondsToRevive())
        //{



        //}

        adsController.LoadGoogleAdsInterstitial();

        CoroutineRunner.Instance.StartCoroutine(ShowRevivePanel(3.5f));
    }

    //private bool hasPlayerEnoughDiamondsToRevive()
    //{
    //    int sessionDiamonds = sessionInventoryObj.GetIntKeyValue("AccountDiamonds");
    //    int inventoryDiamonds = playerInventoryObj.GetIntKeyValue("AccountDiamonds");

    //    int totalDiamonds = sessionDiamonds + inventoryDiamonds;

    //    return totalDiamonds >= PlayerPrefs.GetInt("failcounter");
    //}


    IEnumerator ShowRevivePanel(float delay)
    {
       yield return new WaitForSeconds(delay);
        if (revivebool)
        {
           showRevivePanel.RaiseEvent();
        }
        //if (gameoverbool)
        //{
        //    showGameOverPanel.RaiseEvent();
        //}
    }

}