using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;

[System.Serializable]
public struct TimeScaleEffectsConfig
{
    [SerializeField] private float[] timeScaleValues;
    public float[] GetTimeScaleValues => timeScaleValues;

    [SerializeField] private float[] delaysToShiftTimeScale;
    public float[] GetDelaysToShiftTimeScale => delaysToShiftTimeScale;
}

[CreateAssetMenu(fileName = "TimeScaleEffectsHandler", menuName = "ScriptableObjects/Effects/TimeScaleEffectsHandler")]
public class TimeScaleEffectsHandler : ScriptableObject
{
    [System.NonSerialized] public static bool IsPaused;

    [SerializeField] private SpeedHandler speedHandler;

    private Coroutine timeScaleEffectRoutine;
    
    [System.Serializable]
    private class TimeScaleEffectsDictionary : SerializableDictionaryBase<GameEvent, TimeScaleEffectsConfig>
    { }

    [SerializeField] private TimeScaleEffectsDictionary timeScaleEffectsDictionary;

    private void OnEnable()
    {
        IsPaused = false;

        foreach (var item in timeScaleEffectsDictionary)
        {
            item.Key.TheEvent.AddListener(PlayTimeScaleEffect);
        }
    }

    private void OnDisable()
    {
        foreach (var item in timeScaleEffectsDictionary)
        {
            item.Key.TheEvent.RemoveListener(PlayTimeScaleEffect);
        }
    }

    private void PlayTimeScaleEffect(GameEvent theEvent)
    {
        if (IsPaused)
            return;

        if (!timeScaleEffectsDictionary.ContainsKey(theEvent))
        {
            UnityEngine.Console.LogWarning($"Timescale effects dictionary doesn't contain the event {theEvent}");
            return;
        }

        if (timeScaleEffectRoutine != null)
        {
            CoroutineRunner.Instance.StopCoroutine(timeScaleEffectRoutine);
        }

        TimeScaleEffectsConfig timeScaleEffectsConfig = timeScaleEffectsDictionary[theEvent];
        timeScaleEffectRoutine = CoroutineRunner.Instance.StartCoroutine(speedHandler.ResetTimeScaleValuesAfterDelay(timeScaleEffectsConfig.GetTimeScaleValues, timeScaleEffectsConfig.GetDelaysToShiftTimeScale));
    }
}
