using UnityEngine;
using MessagePack;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DailyGoals", menuName = "DailyGoalSystem/DailyGoalsSO")]
public class DailyGoalsSO : ScriptableObject
{
    [SerializeField] private List<GoalItemDataSet> goalsDataSet;
    [SerializeField] private List<int> genericTargetList;
    [SerializeField] private int noOfGoalsForDay;

    public List<GoalItemDataSet> genericDailyGoals;


    /// <summary>
    /// This function will populate dailyGoals with random goals from goalsDataSet
    /// </summary>
    public List<GoalItemDataSet> PoolDataSets() {
        genericDailyGoals.Clear();

        int listSize = 0;
        while (listSize < 3) {
            int randIndex = Random.Range(0, goalsDataSet.Count);
            bool isItemExist = genericDailyGoals.Contains(goalsDataSet[randIndex]);
            if (!isItemExist) {
                int randNo = Random.Range(0, genericTargetList.Count);
                goalsDataSet[randIndex].targetToAchive = genericTargetList[randNo];
                genericDailyGoals.Add(goalsDataSet[randIndex]);
                listSize += 1;
            }
        }
        return genericDailyGoals;
    }
}

public enum DailyGoalTypes { 
    WallRun, DashKill, ShockwaveKill, InAir, SideHit, DestroyEnemies, LaneSwitch
};