using UnityEngine;
using UnityEngine.Events;

public class InvokePatchFinishedEvent : MonoBehaviour
{
    public UnityEvent patchFinishedEvent;

    private void OnTriggerExit(Collider other)
    {
        patchFinishedEvent.Invoke();
    }
}
