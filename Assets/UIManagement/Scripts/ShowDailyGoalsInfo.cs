using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TheKnights.SaveFileSystem;
using static UnityEngine.ResourceManagement.ResourceProviders.AssetBundleResource;
using System;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class ShowDailyGoalsInfo : MonoBehaviour
{
    [SerializeField] private GamePlaySessionInventory gamePlaySessionInventory;
    public List<Image> fillImages;
    public DailyGoalsManagerSO dailyGoalManager;
    public List<DailyGoalContainer> dailyGoalUIContainer;

    [SerializeField] private bool isUseSessionInventory = false;
    //public Image goalsChecked, goalsUnchecked;

    private void OnEnable()
    {
        List<GoalItemDataSet> obj = dailyGoalManager.dailyGoalsObj.genericDailyGoals;
        for (int i = 0; i < dailyGoalManager.dailyGoalsObj.genericDailyGoals.Count; i++)
        {
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
            dailyGoalUIContainer[i].completedImage.SetActive(false);
            dailyGoalUIContainer[i].rewardedButton.gameObject.SetActive(true);
            switch (obj[i].goalType)
            {
                case DailyGoalTypes.WallRun:
                    {
                       

                        if (gamePlaySessionInventory.noOfWallRuns >= obj[i].targetToAchive)
                        {
                            dailyGoalUIContainer[i].rewardedButton.gameObject.SetActive(false);
                            dailyGoalUIContainer[i].completedImage.SetActive(true);
                        }
                    }
                    break;

                case DailyGoalTypes.DashKill:
                    {
                       if( gamePlaySessionInventory.noOfDashKills >= obj[i].targetToAchive)
                        {
                            dailyGoalUIContainer[i].rewardedButton.gameObject.SetActive(false);
                            dailyGoalUIContainer[i].completedImage.SetActive(true);
                        }
                    }
                    break;

                case DailyGoalTypes.ShockwaveKill:
                    {
                     
                        if (gamePlaySessionInventory.noOfShockwaveKills >= obj[i].targetToAchive)
                        {
                            dailyGoalUIContainer[i].rewardedButton.gameObject.SetActive(false);
                            dailyGoalUIContainer[i].completedImage.SetActive(true);
                        }
                    }
                    break;

                case DailyGoalTypes.InAir:
                    {
                       
                        if (gamePlaySessionInventory.noOfInAir >= obj[i].targetToAchive)
                        {
                            dailyGoalUIContainer[i].rewardedButton.gameObject.SetActive(false);
                            dailyGoalUIContainer[i].completedImage.SetActive(true);
                        }
                    }
                    break;

                case DailyGoalTypes.SideHit:
                    {
                      
                        if (gamePlaySessionInventory.noOfHits >= obj[i].targetToAchive)
                        {
                            dailyGoalUIContainer[i].rewardedButton.gameObject.SetActive(false);
                            dailyGoalUIContainer[i].completedImage.SetActive(true);
                        }
                    }
                    break;

                case DailyGoalTypes.LaneSwitch:
                    {
                      
                        if (gamePlaySessionInventory.noOfLaneSwitch >= obj[i].targetToAchive)
                        {
                            dailyGoalUIContainer[i].rewardedButton.gameObject.SetActive(false);
                            dailyGoalUIContainer[i].completedImage.SetActive(true);
                        }
                    }
                    break;

                case DailyGoalTypes.DestroyEnemies:
                    {
                       
                        if (gamePlaySessionInventory.noOfEnemiesDestroyed >= obj[i].targetToAchive)
                        {
                            dailyGoalUIContainer[i].rewardedButton.gameObject.SetActive(false);
                            dailyGoalUIContainer[i].completedImage.SetActive(true);
                        }
                    }
                    break;
            }
        }
    }

    public void CompleteGoalReward(int index)
    {
        MaxAdMobController.OnVideoAdCompleteReward += () => GiveRewardedReward(index);
        MaxAdMobController.Instance.ShowRewardedVideoAd();

    }


    void GiveRewardedReward(int index)
    {
        MaxAdMobController.OnVideoAdCompleteReward -= () => GiveRewardedReward(index);

        List<GoalItemDataSet> obj = dailyGoalManager.dailyGoalsObj.genericDailyGoals;


        switch (obj[index].goalType)
        {
            case DailyGoalTypes.WallRun:
                {
                    gamePlaySessionInventory.noOfWallRuns = obj[index].targetToAchive;
                }
                break;

            case DailyGoalTypes.DashKill:
                {
                    gamePlaySessionInventory.noOfDashKills = obj[index].targetToAchive;
                }
                break;

            case DailyGoalTypes.ShockwaveKill:
                {
                    gamePlaySessionInventory.noOfShockwaveKills = obj[index].targetToAchive;
                }
                break;

            case DailyGoalTypes.InAir:
                {
                    gamePlaySessionInventory.noOfInAir = obj[index].targetToAchive;
                }
                break;

            case DailyGoalTypes.SideHit:
                {
                    gamePlaySessionInventory.noOfHits = obj[index].targetToAchive;
                }
                break;

            case DailyGoalTypes.LaneSwitch:
                {
                    gamePlaySessionInventory.noOfLaneSwitch = obj[index].targetToAchive;
                }
                break;

            case DailyGoalTypes.DestroyEnemies:
                {
                    gamePlaySessionInventory.noOfEnemiesDestroyed = obj[index].targetToAchive;
                }
                break;
        }



        dailyGoalManager.UpdateGoalsStatus();
        dailyGoalManager.GoalCompleted(index);
        OnEnable();
    }


   


}


[System.Serializable]
public class DailyGoalContainer {
    public Image goalStatusImage;
    public TextMeshProUGUI goalTextObj;
    public TextMeshProUGUI goalTextInfo;
    public Button rewardedButton;
    public GameObject completedImage;
}