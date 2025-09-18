using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MATS_GameManager : MonoBehaviour
{
    public static MATS_GameManager instance;
    public bool isQualityLevelSet = false;
    public bool isTutorialPlaying = false;

    public GameObject Dummy;

    // public UnityEngine.Rendering.PostProcessing.PostProcessLayer[] scenePostProcessingLayers;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }


    public static int GetCurrentLevel()
    {
        return PlayerPrefs.GetInt("level_counter", 0) + 1; // Start from 1
    }

    public static void IncrementLevel()
    {
        int current = PlayerPrefs.GetInt("level_counter", 0);
        PlayerPrefs.SetInt("level_counter", current + 1);
        PlayerPrefs.Save();
    }





}
