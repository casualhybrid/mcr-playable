using TMPro;
using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;

public class HighScorePopupScript : APanelController
{
    [SerializeField] private TextMeshProUGUI statusTxt;
    [SerializeField] private GamePlaySessionInventory gameplayInventoryObj;

 

    private void OnEnable()
    {
        statusTxt.text = (gameplayInventoryObj.GetCurrentSessionScore() + 1).ToString("00");
    }
}
