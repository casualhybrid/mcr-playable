using UnityEngine;
using UnityEngine.Events;

public class PowerUp : MonoBehaviour, ISetupExecuteAndExit
{
    [SerializeField] protected UnityEvent powerHasStarted;
    [SerializeField] protected UnityEvent powerHasStopped;
    [SerializeField] protected GameEvent powerUpPickedEvent;
    [SerializeField] private GameEvent powerUpEnded;
    public bool isDone { get; protected set; }

    public GameEvent PowerUpPickedEvent => powerUpPickedEvent;

    public virtual void Execute()
    {
    }

    public virtual void Exit()
    {
        isDone = true;
        powerHasStopped.Invoke();
        powerUpEnded?.RaiseEvent();
    }

    public virtual void SetUp()
    {
        isDone = false;
        powerHasStarted.Invoke();
    }
}