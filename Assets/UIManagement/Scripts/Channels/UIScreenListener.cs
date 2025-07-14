using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class UIScreenListener : MonoBehaviour
{
    [SerializeField] private bool listenToAllScreens = false;
    [SerializeField] private UnityEvent onPanelOpened;
    [SerializeField] private UnityEvent onPanelClosed;

    [SerializeField] private UnityEvent onPanelOpenedBeforeAnimation;
    [SerializeField] private UnityEvent onPanelClosedBeforeAnimation;

    [ShowIf("isNotListeningToAllScreens")]
    [SerializeField] private List<string> panelsNameToListenTo;

    private bool isNotListeningToAllScreens => !listenToAllScreens;


    private void OnEnable()
    {
        UIScreenEvents.OnScreenOperationEventAfterAnimation.AddListener(HandleScreenAfterOperation);
        UIScreenEvents.OnScreenOperationEventBeforeAnimation.AddListener(HandleScreenBeforeOperation);
    }

    private void OnDisable()
    {
        UIScreenEvents.OnScreenOperationEventBeforeAnimation.RemoveListener(HandleScreenBeforeOperation);
        UIScreenEvents.OnScreenOperationEventAfterAnimation.RemoveListener(HandleScreenAfterOperation);
    }

    private void HandleScreenAfterOperation(string panel, ScreenOperation operation, ScreenType screenType)
    {
        if (!listenToAllScreens && !panelsNameToListenTo.Any(x => x == panel))
            return;

       // if(this.gameObject.name == "CurvedWorld_Controller")
     //   UnityEngine.Console.Log($"Operation {operation} on screen {panel}");

        if (operation == ScreenOperation.Close)
        {
            onPanelClosed.Invoke();
        }
        else
        {
            onPanelOpened.Invoke();
        }
    }

    private void HandleScreenBeforeOperation(string panel, ScreenOperation operation, ScreenType screenType)
    {
        if (!panelsNameToListenTo.Any(x => x == panel))
            return;


        if (operation == ScreenOperation.Close)
        {
            onPanelClosedBeforeAnimation.Invoke();
        }
        else
        {
            onPanelOpenedBeforeAnimation.Invoke();
        }
    }
}