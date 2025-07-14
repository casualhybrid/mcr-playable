using UnityEngine;

public class LaneSwitchChecker : MonoBehaviour
{
    [SerializeField] private GameEvent playerSwitchedLane;
    [SerializeField] private DailyGoalsManagerSO dailyGoalsManager;
    [SerializeField] private GamePlaySessionInventory gamePlaySessionInventory;
    [SerializeField] private float timetaken, maxTime;
    private bool isTimeStarted = default, isGoalActive = default;

    private void OnEnable()
    {
        CheckLaneSwitcherGoal();
        playerSwitchedLane.TheEvent.AddListener(PlayerSwitchedLane);
    }

    void CheckLaneSwitcherGoal() {
        foreach (GoalItemDataSet obj in dailyGoalsManager.dailyGoalsObj.genericDailyGoals)
        {
            if (obj.goalType == DailyGoalTypes.LaneSwitch) {
                isGoalActive = true;
            }
        }
    }

    void PlayerSwitchedLane(GameEvent theEvent) {
        if (isGoalActive)
        {
            isTimeStarted = true;
            gamePlaySessionInventory.noOfLaneSwitch += 1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isGoalActive)
        {
            if (isTimeStarted)
            {
                timetaken += 1 * Time.deltaTime;
                if (timetaken > maxTime)
                {
                    timetaken = 0;
                    isTimeStarted = false;
                }
            }
        }
    }

    private void OnDisable()
    {
        playerSwitchedLane.TheEvent.RemoveListener(PlayerSwitchedLane);
    }
}
