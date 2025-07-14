using MessagePack;

[MessagePackObject]
[System.Serializable]
public class GoalItemDataSet
{
    [Key(0)] public DailyGoalTypes goalType;
    [Key(1)] public string itemDescription;
    [Key(2)] public int targetToAchive;
}
