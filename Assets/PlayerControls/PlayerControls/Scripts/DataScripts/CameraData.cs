using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CameraData", menuName = "ScriptableObjects/CameraData")]
public class CameraData : ScriptableObject
{
    public Camera TheMainCamera { get; private set; }
    public Camera SecondaryCamera { get; private set; }

    [Tooltip("target offset for z on boost and dash")]
    [SerializeField] public float targetZOffsetBoost;
    [Tooltip("starting position for y")]
    [SerializeField] public float yStartPos; //saving start pos because we dont want camera to move yaxis on jump
    [Tooltip("we want to change xoffset when car is not in the centre")]
    [SerializeField] public float xLanesOffset; // we want to change xoffset when car is not in the centre
    [Tooltip("we want to change xoffset when car is in the middle")]
    [SerializeField] public float xCenterOffset; // we want to change xoffset when car is in the middle

    public void SetMainCameraReference(Camera cam)
    {
        TheMainCamera = cam;
    }

    public void SetSecondaryCameraReference(Camera cam)
    {
        SecondaryCamera = cam;
    }
}
