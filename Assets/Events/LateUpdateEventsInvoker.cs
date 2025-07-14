using UnityEngine;
using UnityEngine.Events;

public class LateUpdateEventsInvoker : MonoBehaviour
{
    [SerializeField] private UnityEvent response;

    private void LateUpdate()
    {
        if (!GameManager.DependenciesLoaded)
            return;
 
        response.Invoke();
    }
}
