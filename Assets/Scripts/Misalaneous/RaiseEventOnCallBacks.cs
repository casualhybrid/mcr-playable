using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RaiseEventOnCallBacks : MonoBehaviour
{
    [SerializeField] private MonoBehaviourCallBack callBackForEvent;
    [SerializeField] private UnityEvent theEvent;
  
    private enum MonoBehaviourCallBack
    {
       OnEnable, Start, Awake
    }

    private void Awake()
    {
        if(callBackForEvent == MonoBehaviourCallBack.Awake)
        {
            theEvent.Invoke();
        }
    }

    private void OnEnable()
    {
        if (callBackForEvent == MonoBehaviourCallBack.OnEnable)
        {
            theEvent.Invoke();
        }
    }

    private void Start()
    {
        if (callBackForEvent == MonoBehaviourCallBack.Start)
        {
            theEvent.Invoke();
        }
    }

}
