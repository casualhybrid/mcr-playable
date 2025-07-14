using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShowDailyGoalsInfo : MonoBehaviour
{
    public List<Image> fillImages;
    public DailyGoalsManagerSO dailyGoalManager;
    public List<DailyGoalContainer> dailyGoalUIContainer;

    [SerializeField] private bool isUseSessionInventory = false;
    //public Image goalsChecked, goalsUnchecked;

    private void OnEnable()
    {
        List<GoalItemDataSet> obj = dailyGoalManager.dailyGoalsObj.genericDailyGoals;
        for (int i = 0; i < dailyGoalManager.dailyGoalsObj.genericDailyGoals.Count; i++) {
            //if (dailyGoalManager.GetCurrentGoalStatus(obj[i].goalType) >= obj[i].targetToAchive)
            //    dailyGoalUIContainer[i].goalStatusImage.gameObject.SetActive(true);
            //else 
            //    dailyGoalUIContainer[i].goalStatusImage.gameObject.SetActive(false); //uncomment when tick icon is placed

            DailyGoalTypes goalType = obj[i].goalType;
            int curGoalStatus = isUseSessionInventory ? dailyGoalManager.GetCurrentGoalStatusGamePlaySessionInventory(goalType) : dailyGoalManager.GetCurrentGoalStatus(goalType);

            dailyGoalUIContainer[i].goalTextObj.text = obj[i].itemDescription;
            dailyGoalUIContainer[i].goalTextInfo.text = (curGoalStatus > obj[i].targetToAchive ? obj[i].targetToAchive :
                curGoalStatus) + "/" + obj[i].targetToAchive;
            /*Debug.Log("Test : " +
     ((float)(curGoalStatus > obj[i].targetToAchive ? obj[i].targetToAchive : curGoalStatus)
     / (float)obj[i].targetToAchive));*/
            fillImages[i].fillAmount = (float)(curGoalStatus > obj[i].targetToAchive ? obj[i].targetToAchive : curGoalStatus) / (float)obj[i].targetToAchive;
        }
       
        
    }
}

[System.Serializable]
public class DailyGoalContainer {
    public Image goalStatusImage;
    public TextMeshProUGUI goalTextObj;
    public TextMeshProUGUI goalTextInfo;
}