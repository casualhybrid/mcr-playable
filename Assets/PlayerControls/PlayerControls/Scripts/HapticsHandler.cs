using RDG;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using RotaryHeart.Lib.SerializableDictionary;

public class UnityHapticsConfig : UnityEvent<HapticsConfig>
{ }

[System.Serializable]
public struct HapticsConfig
{
    [SerializeField] private bool isThisRepeatingEffect;
    public bool GetIsThisRepeatingEffect => isThisRepeatingEffect;
    [ShowIf("isThisRepeatingEffect")] [SerializeField] private int repeatIndexValue;
    [ShowIf("isThisRepeatingEffect")] [SerializeField] private long[] milliSecondsList;
    [ShowIf("isThisRepeatingEffect")] [SerializeField] private int[] amplitudesList;
    public int GetRepeatIndexValue => repeatIndexValue;
    public long[] GetMilliSecondsList => milliSecondsList;
    public int[] GetAmplitudesList => amplitudesList;

    private bool isThisNotRepeatingEffect => !isThisRepeatingEffect;
    [ShowIf("isThisNotRepeatingEffect")] [SerializeField] private long milliSecond;
    [ShowIf("isThisNotRepeatingEffect")] [SerializeField] private int amplitude;
    public long GetMilliSecond => milliSecond;
    public int GetAmplitude => amplitude;

    [SerializeField] private bool cancelPrevious;
    public bool GetCancelPrevious => cancelPrevious;
}

[CreateAssetMenu(fileName = "HapticsHandler", menuName = "ScriptableObjects/HapticsHandler")]
public class HapticsHandler : ScriptableObject
{
    [System.Serializable]
    private class HapticsDictionary : SerializableDictionaryBase<GameEvent, HapticsConfig>
    { }

    [SerializeField] private HapticsDictionary hapticsDictionary;

    private void OnEnable()
    {
        foreach (var item in hapticsDictionary)
        {
            item.Key.TheEvent.AddListener(PlayHaptics);
        }
    }

    private void OnDisable()
    {
        foreach (var item in hapticsDictionary)
        {
            item.Key.TheEvent.RemoveListener(PlayHaptics);
        }
    }

    private void PlayHaptics(GameEvent theEvent)
    {
        if (!hapticsDictionary.ContainsKey(theEvent))
        {
            UnityEngine.Console.LogWarning($"Haptics dictionary doesn't contain the event {theEvent}");
            return;
        }

        if(PlayerPrefs.GetFloat("haptic") != 1)
        {
            return;
        }

        //   UnityEngine.Console.Log($"Playing Vibration {theEvent.name}");
        HapticsConfig hapticsConfig = hapticsDictionary[theEvent];
        Haptics.VibrateAccordingToConfig(hapticsConfig);
    }
}