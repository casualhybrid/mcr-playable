using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFloatingReset
{
    bool ShoudNotOffsetOnRest { get; set; }
    void OnFloatingPointReset(float movedOffset);
    void OnBeforeFloatingPointReset();
}
