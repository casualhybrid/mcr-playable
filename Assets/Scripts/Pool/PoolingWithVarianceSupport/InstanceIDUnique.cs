using System;
using UnityEngine;

public class InstanceIDUnique : MonoBehaviour
{
    public event Action<InstanceIDUnique> OnObjectDestroyed;

    [SerializeField] private int instnaceID;

    public int InstanceID => instnaceID;

#if UNITY_EDITOR
    private void OnValidate()
    {
        instnaceID = GetInstanceID();
    }
#endif

    public void SendCompletionEvent()
    {
        OnObjectDestroyed?.Invoke(this);
    }
}