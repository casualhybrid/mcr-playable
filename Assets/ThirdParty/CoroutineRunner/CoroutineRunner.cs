using System;
using System.Collections;
using UnityEngine;

public class CoroutineRunner : Singleton<CoroutineRunner>
{
    public void WaitForFixedUpdateAndExecute(Action action)
    {
        StartCoroutine(WaitForFixedUpdateAndExecuteRoutine(action));
    }

    public void WaitTillFrameEndAndExecute(Action action)
    {
        StartCoroutine(WaitTillFrameEndAndExecuteRoutine(action));
    }

    public void WaitForUpdateAndExecute(Action action)
    {
        StartCoroutine(WaitForUpdateAndExecuteRoutine(action));
    }

    public void WaitForRealTimeDelayAndExecute(Action action, float delay)
    {
        StartCoroutine(WaitForRealTimeDelayAndExecuteRoutine(action, delay));
    }

    private IEnumerator WaitForFixedUpdateAndExecuteRoutine(Action action)
    {
        yield return new WaitForFixedUpdate();
        action();
    }

    private IEnumerator WaitTillFrameEndAndExecuteRoutine(Action action)
    {
        yield return new WaitForEndOfFrame();
        action();
    }

    private IEnumerator WaitForUpdateAndExecuteRoutine(Action action)
    {
        yield return null;
        action();
    }

    private IEnumerator WaitForRealTimeDelayAndExecuteRoutine(Action action, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        action();
    }



}