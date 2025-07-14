using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResetObject : MonoBehaviour
{
    public UnityEvent OnReset;

    public void ResetTheObject()
    {
        OnReset.Invoke();
    }
}
