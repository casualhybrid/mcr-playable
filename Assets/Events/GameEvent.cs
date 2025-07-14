using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class UnityGameEvent : UnityEvent<GameEvent> { }

[CreateAssetMenu(fileName = "GameEvent", menuName = "ScriptableObjects/GameEvent")]
public class GameEvent : ScriptableObject
{
    public UnityGameEvent TheEvent { get; private set; } = new UnityGameEvent();

    [Button("Raise Event")]
    public void RaiseEvent()
    {
        if (name == "GamePlayDependenciesLoaded")
            Debug.Log($"GamePlayDependenciesLoaded: Event Raised: {name}");
        TheEvent.Invoke(this);
    }
}