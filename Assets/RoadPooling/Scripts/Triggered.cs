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
            Event?.RaiseEvent();
            response.Invoke();
        }
    }
}