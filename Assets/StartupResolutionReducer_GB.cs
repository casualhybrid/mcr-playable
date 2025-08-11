using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartupResolutionReducer_GB : MonoBehaviour
{
    [Header("RAM Settings (GB)")]
    public float lowRAMThreshold = 2f;    // Below this → strong reduction
    public float mediumRAMThreshold = 4f; // Below this → slight reduction

    [Header("Resolution Scales")]
    [Range(0.5f, 1f)] public float lowRAMScale = 0.7f;
    [Range(0.5f, 1f)] public float mediumRAMScale = 0.85f;
    [Range(0.5f, 1f)] public float highRAMScale = 1f;

    void Start()
    {
        int originalWidth = Screen.width;
        int originalHeight = Screen.height;

        // Get RAM in GB
        float totalRAM_GB = SystemInfo.systemMemorySize / 1024f;
        float scale = highRAMScale;

        if (totalRAM_GB < lowRAMThreshold)
        {
            scale = lowRAMScale;
            Debug.Log($"[Startup] Low RAM Device ({totalRAM_GB:F1} GB) → Strong resolution reduction");
        }
        else if (totalRAM_GB < mediumRAMThreshold)
        {
            scale = mediumRAMScale;
            Debug.Log($"[Startup] Medium RAM Device ({totalRAM_GB:F1} GB) → Slight resolution reduction");
        }
        else
        {
            Debug.Log($"[Startup] High RAM Device ({totalRAM_GB:F1} GB) → Full resolution");
        }

        int newWidth = Mathf.RoundToInt(originalWidth * scale);
        int newHeight = Mathf.RoundToInt(originalHeight * scale);

        Screen.SetResolution(newWidth, newHeight, true);
        Debug.Log($"[Startup] Resolution set to {newWidth}x{newHeight} from {originalWidth}x{originalHeight}");
    }
}