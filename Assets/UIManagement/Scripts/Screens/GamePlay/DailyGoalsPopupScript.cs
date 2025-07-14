using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Knights.UISystem;
using deVoid.UIFramework;

public class DailyGoalsPopupScript : APanelController
{
    [SerializeField] private TextMeshProUGUI statusTxt;
    [SerializeField] private DailyGoalsManagerSO dailyGoalManagerObj;
    [SerializeField] private UIEffectsChannel uIEffectsChannel;

    private UIFrame uiFrame;

    protected override void Awake()
    {
        base.Awake();
        uiFrame = GetComponentInParent<UIFrame>();
    }


    private void OnEnable()
    {
        statusTxt.text = dailyGoalManagerObj.completedGoalStr;
        var startingScreenPos = RectTransformUtility.WorldToScreenPoint(uiFrame.UICamera, transform.position);
        string itemKey = "AccountCoins";
        uIEffectsChannel.RaiseItemAddEffectRequest(startingScreenPos, InventoryPositions.GetAnimDataForItem(itemKey).screenLocation, itemKey, 3, true);
    
    }
}
