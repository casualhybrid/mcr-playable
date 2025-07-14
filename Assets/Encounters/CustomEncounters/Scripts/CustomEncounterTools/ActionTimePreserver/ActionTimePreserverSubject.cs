using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionTimePreserverSubject : MonoBehaviour
{
    public string[] selfTags = new string[] {"Default"};
    [ReadOnly] public Transform lastObjectInGroupTransform;
    [ReadOnly] public Transform firstObjectInGroupTransform;

    public Vector3 virtualDeltaPos {get; set;}
    public Transform relativeObjectTransformForDistanceCalc {get; set;}
    public Action<Vector3, bool> OnMove {get; set;}
    [ReadOnly] [SerializeField] private Vector3 preserverSubjectInitialPosition;

    private void OnValidate()
    {
        preserverSubjectInitialPosition = transform.localPosition;
    }

    public void Move(Vector3 deltaPos, bool moveVirtualPos = false)
    {
        if (moveVirtualPos)
        {
            virtualDeltaPos += deltaPos;
        }
        else
        {
            transform.position += deltaPos;
        }

        if (OnMove != null)
        {
            OnMove(deltaPos, moveVirtualPos);
        }
    }

    public void ResetPreserverSubjectPosition()
    {
        transform.localPosition = preserverSubjectInitialPosition;
    }

    public void ResetVirtualDeltaPosition()
    {
        virtualDeltaPos = Vector3.zero;
    }

    public float GetDeltaDistanceToPreserveActionTime(float preservationMultiplier, float calculatedObstacleSpacingMultiplier)
    {
        if (relativeObjectTransformForDistanceCalc == null)
            return 0;

        //UnityEngine.Console.Log($"Positions {lastObjectInGroupTransform.position.z} and {relativeObjectTransformForDistanceCalc.position.z}");
        float deltaDistanceToRelativeObject = lastObjectInGroupTransform.position.z - relativeObjectTransformForDistanceCalc.position.z;
        return deltaDistanceToRelativeObject * preservationMultiplier *  (calculatedObstacleSpacingMultiplier - 1); // subtracted 1 from space multiplier to get delta distance instead of total distance
    }
}
