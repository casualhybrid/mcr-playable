using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    [SerializeField] private GameEvent theGameEvent;
    [SerializeField] private UnityEvent response;

    private void OnEnable()
    {
        theGameEvent.TheEvent.AddListener(RaiseResponse);
    }

    private void OnDisable()
    {
        theGameEvent.TheEvent.RemoveListener(RaiseResponse);
    }

    private void RaiseResponse(GameEvent theEvent)
    {
        response.Invoke();
    }


}
