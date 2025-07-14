using System.Collections;
using UnityEngine;

public class CoroutineWithReturn<T> : IEnumerator
{
    public T Result;

    public bool IsDone;
    public object Current { get; }

    public bool MoveNext()
    {
        return !IsDone;
    }

    public void Reset()
    {
    }

    public CoroutineWithReturn(MonoBehaviour owner, IEnumerator routineToRun)
    {
        Current = owner.StartCoroutine(Wrap(routineToRun));
    }

    private IEnumerator Wrap(IEnumerator routine)
    {
        // This checks if the routine needs at least one frame to execute.
        // If not, LoopCoroutine will wait 1 frame to avoid an infinite
        // loop which will crash Unity
        if (routine.MoveNext())
            yield return routine.Current;
        else
            yield return null;

        while (routine.MoveNext())
        {
            yield return routine.Current;
        }

        Result = (T)routine.Current;
        IsDone = true;
    }
}