using UnityEngine;
using UnityEngine.EventSystems;

public class DestroyIfEventSystemIsPresent : MonoBehaviour
{
    private void Awake()
    {
        EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();

        if (eventSystems.Length > 1)
        {
            Destroy(this.gameObject);
        }
    }
}