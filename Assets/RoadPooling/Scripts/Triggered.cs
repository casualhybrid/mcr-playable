using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Triggered : MonoBehaviour
{
    [SerializeField] private UnityEvent response;
    [SerializeField] private string colliderTag = "Player";
    public GameEvent Event;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(colliderTag))
        {
            StartCoroutine(SetTriggeredForSeconds(3f));
            Event?.RaiseEvent();
            response.Invoke();

        }
    }
    private IEnumerator SetTriggeredForSeconds(float seconds)
    {
        WaterParkEnter.isFlying = true;
        yield return new WaitForSeconds(seconds);
        WaterParkEnter.isFlying = false;
    }
}