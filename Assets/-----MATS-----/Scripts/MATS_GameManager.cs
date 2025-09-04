using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MATS_GameManager : MonoBehaviour
{
    public static MATS_GameManager instance;
    public bool isQualityLevelSet=false;

   // public UnityEngine.Rendering.PostProcessing.PostProcessLayer[] scenePostProcessingLayers;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
}
