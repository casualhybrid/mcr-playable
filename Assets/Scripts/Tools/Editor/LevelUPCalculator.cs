using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public class LevelUPCalculator : OdinEditorWindow
{
    [SerializeField] private float xp;
    [SerializeField] private PlayerXPLevelPerCar PlayerXPLevelPerCar;
    [SerializeField] private float exponentialPower =.3f;

    [MenuItem("Tools/Level Calculator")]
    private static void OpenWindow()
    {
        GetWindow<LevelUPCalculator>().Show();
    }

    public int CurrentLevel;

    public float DistanceToCover;

    [LabelText("Level Reached")]
    public int LevelReached;

    [Button("Calculate Level")]
    public void CalculateLevel()
    {
        int level = CurrentLevel;
        float totalXPObtainedForDistance = (xp * DistanceToCover) / PlayerXPLevelPerCar.XPLevelPerKm["PlayerCar1"].distanceInUnits;

        while (true)
        {
            float xpReqForNextLvl = PlayerXPLevelPerCar.XPLevelPerKm["PlayerCar1"].xpRequiredInitial * (Mathf.Pow(level + 1, exponentialPower));
            totalXPObtainedForDistance -= xpReqForNextLvl;
            if (totalXPObtainedForDistance >= 0)
            {
                level++;
            }
            else
            {
                break;
            }
        }

        LevelReached = level;
    }
}