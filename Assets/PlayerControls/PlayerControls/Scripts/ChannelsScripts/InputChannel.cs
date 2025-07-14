using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "InputChannel", menuName = "ScriptableObjects/Channels/InputChannel")]
public class InputChannel : ScriptableObject
{
    [HideInInspector]
    public UnityEvent SwipeUpOccured, SwipeDownOccured, SwipeLeftOccured, SwipeRightOccured, SingleTapOccured, DoubleTabOccured;

    private readonly HashSet<UnityEvent> disabledInputEvents = new HashSet<UnityEvent>();

    private bool pauseInputs;
    private bool pauseDoubleBoostInput;
    private bool pauseDashInput;

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;            
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        ResetVariable();
    }

    private void ResetVariable()
    {
        pauseInputs = false;
    }

    public void SwipeUp()
    {
        if (disabledInputEvents.Contains(SwipeUpOccured))
        {
            return;
        }

        if (pauseInputs)
            return;

        SwipeUpOccured.Invoke();
    }

    public void SwipeDown()
    {
        if (disabledInputEvents.Contains(SwipeDownOccured))
        {
            return;
        }

        if (pauseInputs)
            return;
        SwipeDownOccured.Invoke();
    }

    public void SwipeLeft()
    {
        if (disabledInputEvents.Contains(SwipeLeftOccured))
        {
            return;
        }

        if (pauseInputs)
            return;
        SwipeLeftOccured.Invoke();
    }

    public void SwipeRight()
    {
        if (disabledInputEvents.Contains(SwipeRightOccured))
        {
            return;
        }

        if (pauseInputs)
            return;
        SwipeRightOccured.Invoke();
    }

    public void SingleTap()
    {
        //  UnityEngine.Console.Log("SingleTapped");

        if (disabledInputEvents.Contains(SingleTapOccured))
        {
            return;
        }

        if (pauseInputs)
            return;

        if (pauseDashInput)
            return;

        SingleTapOccured.Invoke();
    }

    public void DoubleTab()
    {
        //  UnityEngine.Console.Log("DoubleTapped");

        if (disabledInputEvents.Contains(DoubleTabOccured))
        {
            return;
        }

        if (pauseInputs)
            return;

        if (pauseDoubleBoostInput)
            return;

        DoubleTabOccured.Invoke();
    }

    public void PauseInputsFromUser()
    {
        pauseInputs = true;
    }

    public void UnPauseInputsFromUser()
    {
        pauseInputs = false;
    }


    public void PauseDoubleBoostInputsFromUser()
    {
        pauseDoubleBoostInput = true;
    }

    public void UnPauseDoubleBoostInputsFromUser()
    {
        pauseDoubleBoostInput = false;
    }

    public void PauseSingleTapInputsFromUser()
    {
        pauseDashInput = true;
    }

    public void UnPauseSingleTapInputsFromUser()
    {
        pauseDashInput = false;
    }

    public void DisableThisInputEvent(UnityEvent eventToDisable)
    {
        if (disabledInputEvents.Contains(eventToDisable)) {
            UnityEngine.Console.LogWarning($"The input event {eventToDisable} you are trying to disable is already disabled!");
            return;
        }

        disabledInputEvents.Add(eventToDisable);
    }

    public void DisableAllButThisInputEvent(UnityEvent eventToRetain)
    {
        disabledInputEvents.Clear();
        disabledInputEvents.Add(SwipeUpOccured);
        disabledInputEvents.Add(SwipeDownOccured);
        disabledInputEvents.Add(SwipeLeftOccured);
        disabledInputEvents.Add(SwipeRightOccured);
        disabledInputEvents.Add(SingleTapOccured);
        disabledInputEvents.Add(DoubleTabOccured);
        disabledInputEvents.Remove(eventToRetain);
    }

    public void EnableThisInputEvent(UnityEvent eventToDisable)
    {
        if(!disabledInputEvents.Contains(eventToDisable))
        {
            UnityEngine.Console.LogWarning($"The input event {eventToDisable} you are trying to enable is already enabled!");
            return;
        }

        disabledInputEvents.Remove(eventToDisable);
    }

    public void EnableAllDisabledInputs()
    {
        disabledInputEvents.Clear();
    }

}