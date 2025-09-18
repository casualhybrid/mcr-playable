using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MATSQualitySettings : MonoBehaviour
{
    // public TMP_Dropdown qualityLevel;

    [Header("Cameras")]
    public Camera[] sceneAllCameras;

    [Header("Lights")]
    public Light[] sceneAllLights;

    public Image[] qualityButtons;
    public Sprite[] buttonSprites;

    // [Header("Fog")]
    //public LightingBox.Effects.GlobalFog[] sceneAllFogEffects;

    //[Header("Volume")]
    //public UnityEngine.Rendering.PostProcessing.PostProcessVolume scenePostProcessingVolumes;

    //[Header("Layers")]
    //  public UnityEngine.Rendering.PostProcessing.PostProcessLayer[] scenePostProcessingLayers;
    private void OnEnable()
    {

        if (MATS_GameManager.instance)
        {
            // scenePostProcessingLayers = MATS_GameManager.instance.scenePostProcessingLayers;


        }


        SetQualityBasedOnRAM();
    }
    public void Set_QualityLevel(int index)
    {
        Debug.Log($"Selecteed Quality Drop Down   " + index);
        // Very Low
        if (index == 0)
        {
            Screen.SetResolution((int)(PlayerPrefs.GetInt("OriginalX") * 0.45f),
                (int)(PlayerPrefs.GetInt("OriginalY") * 0.45f), true);
            /* if (scenePostProcessingLayers == null && scenePostProcessingLayers.Length < 0)
             {
                 return;
             }
             foreach (UnityEngine.Rendering.PostProcessing.PostProcessLayer lll in scenePostProcessingLayers)
             {
                 lll.enabled = true;
                 lll.antialiasingMode = UnityEngine.Rendering.PostProcessing.PostProcessLayer.Antialiasing.None;
             }*/
            /*      foreach (Light light in sceneAllLights)
                  {
                      light.shadows = LightShadows.None;
                  }
                  foreach (LightingBox.Effects.GlobalFog gf in sceneAllFogEffects)
                  {
                      gf.enabled = false;
                  }*/
            Set_Reflection(false, false);
        }
        //_________________________________________
        //  Low
        if (index == 1)
        {
            Screen.SetResolution((int)(PlayerPrefs.GetInt("OriginalX") * 0.45f),
                (int)(PlayerPrefs.GetInt("OriginalY") * 0.45f), true);
            /*if (scenePostProcessingLayers == null && scenePostProcessingLayers.Length < 0)
            {
                return;
            }
            foreach (UnityEngine.Rendering.PostProcessing.PostProcessLayer lll in
                scenePostProcessingLayers)
            {
                lll.enabled = true;
                lll.antialiasingMode = UnityEngine.Rendering.PostProcessing.PostProcessLayer.Antialiasing.None;
            }*/
            /*       foreach (Light light in sceneAllLights)
                   {
                       light.shadows = LightShadows.Soft;
                       light.shadowResolution =
                           UnityEngine.Rendering.LightShadowResolution.Low;
                   }
                   foreach (LightingBox.Effects.GlobalFog gf in sceneAllFogEffects)
                   {
                       gf.enabled = false;
                   }*/
            Set_Reflection(false, false);
        }
        //_________________________________________
        //  Medium
        if (index == 2)
        {
            Screen.SetResolution((int)(PlayerPrefs.GetInt("OriginalX") * 0.5f),
                (int)(PlayerPrefs.GetInt("OriginalY") * 0.5f), true);
            /* if (scenePostProcessingLayers == null && scenePostProcessingLayers.Length < 0)
             {
                 return;
             }
             foreach (UnityEngine.Rendering.PostProcessing.PostProcessLayer lll in scenePostProcessingLayers)
             {
                 lll.enabled = true;
                 lll.antialiasingMode = UnityEngine.Rendering.PostProcessing.
                     PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
                 lll.subpixelMorphologicalAntialiasing.quality =
                     UnityEngine.Rendering.PostProcessing.SubpixelMorphologicalAntialiasing.Quality.Low;
             }*/
            /*     foreach (Light light in sceneAllLights)
                 {
                     light.shadows = LightShadows.Soft;
                     light.shadowResolution =
                         UnityEngine.Rendering.LightShadowResolution.Medium;
                 }
                 foreach (LightingBox.Effects.GlobalFog gf in sceneAllFogEffects)
                 {
                     gf.enabled = true;
                 }*/
            Set_Reflection(false, false);
        }
        //_________________________________________
        //  High
        if (index == 3)
        {
            Screen.SetResolution((int)(PlayerPrefs.GetInt("OriginalX") * 0.55f),
                (int)(PlayerPrefs.GetInt("OriginalY") * 0.55f), true);
            /* if (scenePostProcessingLayers == null && scenePostProcessingLayers.Length < 0)
             {
                 return;
             }
             foreach (UnityEngine.Rendering.PostProcessing.PostProcessLayer lll in
                 scenePostProcessingLayers)
             {
                 lll.enabled = true;
                 lll.antialiasingMode = UnityEngine.Rendering.PostProcessing.
                     PostProcessLayer.Antialiasing.TemporalAntialiasing;
             }*/
            /*      foreach (Light light in sceneAllLights)
                  {
                      light.shadows = LightShadows.Soft;
                      light.shadowResolution =
                          UnityEngine.Rendering.LightShadowResolution.High;
                  }
                  foreach (LightingBox.Effects.GlobalFog gf in sceneAllFogEffects)
                  {
                      gf.enabled = true;
                  }*/
            Set_Reflection(true, false);
        }
        //_________________________________________
        //  Ultra
        if (index == 4)
        {
            Screen.SetResolution((int)(PlayerPrefs.GetInt("OriginalX") * 0.6f),
                (int)(PlayerPrefs.GetInt("OriginalY") * 0.6f), true);
            /* if (scenePostProcessingLayers == null && scenePostProcessingLayers.Length < 0)
             {
                 return;
             }
             foreach (UnityEngine.Rendering.PostProcessing.PostProcessLayer lll in
                 scenePostProcessingLayers)
             {
                 lll.enabled = true;
                 lll.antialiasingMode = UnityEngine.Rendering.PostProcessing.
                     PostProcessLayer.Antialiasing.TemporalAntialiasing;
             }*/
            /*   foreach (Light light in sceneAllLights)
               {
                   light.shadows = LightShadows.Soft;
                   light.shadowResolution =
                       UnityEngine.Rendering.LightShadowResolution.High;
               }
               foreach (LightingBox.Effects.GlobalFog gf in sceneAllFogEffects)
               {
                   gf.enabled = true;
               }*/
            Set_Reflection(false, true);
        }
        //_________________________________________
        //  Max
        if (index == 5)
        {
            Screen.SetResolution((int)(PlayerPrefs.GetInt("OriginalX") * 0.7f),
                (int)(PlayerPrefs.GetInt("OriginalY") * 0.7f), true);
            /*if (scenePostProcessingLayers == null && scenePostProcessingLayers.Length < 0)
            {
                return;
            }
            foreach (UnityEngine.Rendering.PostProcessing.PostProcessLayer lll in
                scenePostProcessingLayers)
            {
                lll.enabled = true;
                lll.antialiasingMode = UnityEngine.Rendering.PostProcessing.
                    PostProcessLayer.Antialiasing.TemporalAntialiasing;
            }*/
            /*     foreach (Light light in sceneAllLights)
                 {
                     light.shadows = LightShadows.Soft;
                     light.shadowResolution =
                         UnityEngine.Rendering.LightShadowResolution.High;
                 }*/
            /*   foreach (LightingBox.Effects.GlobalFog gf in sceneAllFogEffects)
               {
                   gf.enabled = true;
               }*/
            Set_Reflection(false, true);
        }
    }

    int ReflectionState;
    // Screen space reflections
    public void Set_Reflection(bool ssr_1, bool ssr_2)
    {
        /*Trive.Rendering.StochasticReflections ssr2;

        UnityEngine.Rendering.PostProcessing.ScreenSpaceReflections ssr1;

        scenePostProcessingVolumes.profile.TryGetSettings(out ssr2);
        scenePostProcessingVolumes.profile.TryGetSettings(out ssr1);

        if (ssr_1)
        {
            foreach (Camera cam in sceneAllCameras)
                cam.renderingPath = RenderingPath.DeferredShading;

            ssr2.enabled.value = false;
            ssr1.enabled.value = true;
        }
        if (ssr_2)
        {
            foreach (Camera cam in sceneAllCameras)
                cam.renderingPath = RenderingPath.DeferredShading;

            ssr2.enabled.value = true;

            ssr2.resolveDownsample.value = false;
            ssr2.raycastDownsample.value = false;

            ssr1.enabled.value = false;
        }
        if (!ssr_1 && !ssr_2)
        {
            foreach (Camera cam in sceneAllCameras)
                cam.renderingPath = RenderingPath.Forward;

            ssr2.enabled.value = false;
            ssr1.enabled.value = false;
        }*/
    }


    void SetQualityBasedOnRAM()
    {
        if (MATS_GameManager.instance)
        {
            if (MATS_GameManager.instance.isQualityLevelSet)
            {
                return;
            }

            MATS_GameManager.instance.isQualityLevelSet = true;

        }


        int ramMB = SystemInfo.systemMemorySize;
        int selectedQuality = 0;

        // Set thresholds based on RAM



        if (ramMB < 2000)
        {
            // Very Low
            selectedQuality = 0;

            Screen.SetResolution((int)(PlayerPrefs.GetInt("OriginalX") * 0.45f),
                (int)(PlayerPrefs.GetInt("OriginalY") * 0.45f), true);
        }
        else if (ramMB < 3000)
        {
            // Low
            Screen.SetResolution((int)(PlayerPrefs.GetInt("OriginalX") * 0.45f),
                (int)(PlayerPrefs.GetInt("OriginalY") * 0.45f), true);
            selectedQuality = 1;
        }
        else if (ramMB < 4000)
        {
            // Medium
            Screen.SetResolution((int)(PlayerPrefs.GetInt("OriginalX") * 0.5f),
                 (int)(PlayerPrefs.GetInt("OriginalY") * 0.5f), true);
            selectedQuality = 2;
        }
        else if (ramMB < 6000)
        {
            Screen.SetResolution((int)(PlayerPrefs.GetInt("OriginalX") * 0.55f),
                (int)(PlayerPrefs.GetInt("OriginalY") * 0.55f), true);
            selectedQuality = 3;
        } // High
        else if (ramMB < 8000)
        {
            Screen.SetResolution((int)(PlayerPrefs.GetInt("OriginalX") * 0.6f),
                (int)(PlayerPrefs.GetInt("OriginalY") * 0.6f), true);
            selectedQuality = 4;
        } // Ultra
        else
        {
            Screen.SetResolution((int)(PlayerPrefs.GetInt("OriginalX") * 0.7f),
                (int)(PlayerPrefs.GetInt("OriginalY") * 0.7f), true);
            selectedQuality = 5;
        } // Max
        Debug.Log($"Selecteed Quality According to RAM   " + selectedQuality);
        // Set dropdown and apply
        /* if (qualityLevel != null)
         {
             qualityLevel.value = selectedQuality;
             Set_QualityLevel();
         }*/


        for (int i = 0; i < qualityButtons.Length; i++)
        {
            qualityButtons[i].sprite = buttonSprites[0];
        }


        if (selectedQuality == 0 || selectedQuality == 1)
        {
            qualityButtons[0].sprite = buttonSprites[1];
        }
        else if (selectedQuality == 3 || selectedQuality == 3)
        {
            qualityButtons[1].sprite = buttonSprites[1];
        }
        else if (selectedQuality == 4 || selectedQuality == 5)
        {
            qualityButtons[2].sprite = buttonSprites[1];
        }
        Set_QualityLevel(selectedQuality);
    }

}