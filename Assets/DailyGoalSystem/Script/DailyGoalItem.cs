
using System.Collections.Generic;

[System.Serializable]
public class DailyGoalItem
{
    public List<GoalItem> goalOfTheDay;
    public int goalsCompletionStatus;
}

[System.Serializable]
public class GoalItem 
{
    public DailyGoalTypes goalType;
    public string itemDescription;
    public int targetToAchive;
}