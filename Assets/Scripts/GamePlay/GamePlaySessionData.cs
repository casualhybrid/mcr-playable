using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "GamePlaySessionData", menuName = "ScriptableObjects/GamePlaySessionData")]
public class GamePlaySessionData : ScriptableObject
{
    public float DistanceCoveredInMeters { get; set; }
    public int timesFailedDuringSession { get; set; }
    public float timeElapsedSinceSessionStarted { get; set; }

    private List<float> playerCrashTimeStamps = new List<float>();

    [SerializeField] private GameEvent onPlayerCrashed;

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += HandleActiveSceneChanged;
        onPlayerCrashed.TheEvent.AddListener(HandlePlayerCrashed);
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
        onPlayerCrashed.TheEvent.RemoveListener(HandlePlayerCrashed);
    }

    private void HandleActiveSceneChanged(Scene arg0, Scene arg1)
    {
        DistanceCoveredInMeters = 0;
        timesFailedDuringSession = 0;
        timeElapsedSinceSessionStarted = 0;
        playerCrashTimeStamps?.Clear();
    }

    private void HandlePlayerCrashed(GameEvent gameEvent)
    {
        //if (!GameManager.IsGameStarted)
        //    return;

        timesFailedDuringSession++;
        playerCrashTimeStamps.Add(timeElapsedSinceSessionStarted);
    }

    public int timesFailedDuringDuration(float duration)
    {
        int timesFailedDuringDuration = 0;

        for (int i = playerCrashTimeStamps.Count - 1; i >= 0; i--)
        {
            float timeStamp = playerCrashTimeStamps[i];

            if (timeElapsedSinceSessionStarted - timeStamp > duration)
            {
                break;
            }

            timesFailedDuringDuration++;
        }

        UnityEngine.Console.Log($"Times failed during {duration} is {timesFailedDuringDuration}");
        return timesFailedDuringDuration;
    }
}