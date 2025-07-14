using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetableByFloatingPointReset : MonoBehaviour, IFloatingReset
{
    [SerializeField] private bool shouldOffsetOnRest = false;

    public bool ShoudNotOffsetOnRest { get; set; }

    private void Awake()
    {
        ShoudNotOffsetOnRest = !shouldOffsetOnRest;
    }

    public void OnBeforeFloatingPointReset()
    {
     
    }

    public void OnFloatingPointReset(float movedOffset)
    {
       
    }
}
