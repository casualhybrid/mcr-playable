using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CinemachineBrain))]
public class CamerResetOnFloatingReset : MonoBehaviour, IFloatingReset
{
    private CinemachineBrain cinemachineBrain;

    public bool ShoudNotOffsetOnRest { get; set; } = true;

    public void OnFloatingPointReset(float movedOffset)
    {
      //  cinemachineBrain.enabled = true;
    }

    public void OnBeforeFloatingPointReset()
    {
      //  cinemachineBrain.enabled = false;
    }

    private void Awake()
    {
        cinemachineBrain = GetComponent<CinemachineBrain>();
    }
}
