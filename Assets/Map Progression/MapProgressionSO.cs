using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TheKnights.SaveFileSystem;

[CreateAssetMenu(fileName = "MapProgressionSO", menuName = "ScriptableObjects/MapProgressionData")]
public class MapProgressionSO : ScriptableObject
{
    public static int uniqueEnvironmentsCompletedThisSession { get; private set; } 

    [SerializeField] private GamePlaySessionData gamePlaySessionData;
    [SerializeField] private EnvironmentSwitchingOrder environmentSwitchingOrder;
    [SerializeField] private EnvironmentChannel environmentChannel;
    [SerializeField] private SaveManager saveManager;


    public bool AllEnvironmentsCompletedThisSession => uniqueEnvironmentsCompletedThisSession >= environmentSwitchingOrder.GetTotalUniqueEnvironments;
    public bool AllEnvironmentsCompleted => saveManager.MainSaveFile.UniqueEnvironmentsCompleted >= environmentSwitchingOrder.GetTotalUniqueEnvironments;

    public float distanceCoveredByPlayerInThisEnv => gamePlaySessionData.DistanceCoveredInMeters - distanceCoveredAtStartOfThisEnv;

    private float distanceCoveredAtStartOfThisEnv;

    private void OnEnable()
    {
        environmentChannel.OnPlayerEnvironmentChanged += AddAnEnvironmentAsCompleted;
        SceneManager.activeSceneChanged += ResetVariables;
    }

    private void OnDisable()
    {
        environmentChannel.OnPlayerEnvironmentChanged -= AddAnEnvironmentAsCompleted;
        SceneManager.activeSceneChanged -= ResetVariables;
    }

    private void ResetVariables(Scene arg0, Scene arg1)
    {
        distanceCoveredAtStartOfThisEnv = 0;
        uniqueEnvironmentsCompletedThisSession = 0;
    }

    private void AddAnEnvironmentAsCompleted(Environment env)
    {
        distanceCoveredAtStartOfThisEnv = gamePlaySessionData.DistanceCoveredInMeters;

        if (uniqueEnvironmentsCompletedThisSession == environmentSwitchingOrder.GetTotalUniqueEnvironments)
            return;

        UnityEngine.Console.Log($"Incrementing environment completed. New Env : {env.name}");
        uniqueEnvironmentsCompletedThisSession++;
    }

    //public void CompletedMapsUpdate(Environment env)
    //{
    //    if (!CompletedEnviornments.Contains(env))
    //    {
    //        CompletedEnviornments.Add(env);
    //        AllEnvironmentsCompleted = (CompletedEnviornments.Count == totalMapCount);
    //    }
    //}

    //public float calculateDistanceCovered(float temp)
    //{
    //    try
    //    {
    //        float normalizedDistance = temp / estimatedTotalDistance;
    //        if (AllEnvironmentsCompleted)
    //        {
    //            Distance = 1; /*UnityEngine.Console.LogError("Case2");*/
    //        }
    //        else if (Distance < normalizedDistance)
    //        {
    //            if (CompletedEnviornments.Count == 1)
    //            {
    //                if (normalizedDistance > (completionPoints[0] - 0.05f))
    //                    Distance = (completionPoints[0] - 0.3f);
    //                else
    //                    Distance = normalizedDistance;

    //                //UnityEngine.Console.LogError("Case1");
    //            }
    //            else
    //            {
    //                float someVar = 0;
    //                someVar = completionPoints[CompletedEnviornments.Count - 2] + (normalizedDistance - (balancingPoints[CompletedEnviornments.Count - 2] / estimatedTotalDistance));/* UnityEngine.Console.LogError("Case3");*/
    //                if (someVar > completionPoints[CompletedEnviornments.Count - 1])
    //                {
    //                    Distance = (completionPoints[CompletedEnviornments.Count - 1] - 0.06f);
    //                }
    //                else Distance = someVar;
    //            }
    //        }
    //        return Distance;
    //    }
    //    catch
    //    {
    //        UnityEngine.Console.LogWarning("Exception occurred while calculating map progression");
    //        return 1;
    //    }
    //}
}