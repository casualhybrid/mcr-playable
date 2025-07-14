using UnityEngine;
using UnityEngine.Events;

public class DestroyedObstacleListener : MonoBehaviour
{
    [SerializeField] private UnityEvent onDestroyed = new UnityEvent();
    [SerializeField] private Obstacle obstacle;

    [SerializeField] private bool isListenToDashDestruction;
    [SerializeField] private bool isListenToBoostDestruction;
    [SerializeField] private bool isListenToArmourDestruction;
    [SerializeField] private bool isListenToLaserDestruction;
    [SerializeField] private bool isListenToShockWaveDestruciton;

    private void Awake()
    {
        if (isListenToDashDestruction)
        {
            obstacle.OnDestroyedCarThroughDash.AddListener(RaiseTheEvent);
        }

        if (isListenToBoostDestruction)
        {
            obstacle.OnDestroyedCarThroughBoost.AddListener(RaiseTheEvent);
        }

        if (isListenToArmourDestruction)
        {
            obstacle.OnDestroyedCarThroughArmour.AddListener(RaiseTheEvent);
        }

        if (isListenToLaserDestruction)
        {
            obstacle.OnDestroyedCarThroughLaser.AddListener(RaiseTheEvent);
        }

        if (isListenToShockWaveDestruciton)
        {
            obstacle.OnDestroyedCarThroughShockWave.AddListener(RaiseTheEvent);
        }
    }



    private void RaiseTheEvent()
    {
        onDestroyed.Invoke();
    }
}