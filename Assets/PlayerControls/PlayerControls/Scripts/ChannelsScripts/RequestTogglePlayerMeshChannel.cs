using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "RequestTogglePlayerMeshChannel", menuName = "ScriptableObjects/RequestTogglePlayerMeshChannel")]
public class RequestTogglePlayerMeshChannel : ScriptableObject
{
    public UnityEvent OnRequestToDisableSkinnedMeshes { get; private set; } = new UnityEvent();
    public UnityEvent OnRequestToEnableSkinnedMeshes { get; private set; } = new UnityEvent();


    public void RaiseRequestToDisableAllSkinnedMeshes()
    {
        UnityEngine.Console.Log("Request To Disable Meshes");

        OnRequestToDisableSkinnedMeshes.Invoke();
    }

    public void RaiseRequestToEnableAllSkinnedMeshes()
    {
        UnityEngine.Console.Log("Request To Enable Meshes");
        OnRequestToEnableSkinnedMeshes.Invoke();
    }
}
