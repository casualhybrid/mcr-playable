using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SetScreenDPIBasedOnDeviceGroup 
{
    public static void SetFixedDPIScaleValuesAccordingToQualitySettings()
    {
        DeviceGroup deviceGroup = EstimateDevicePerformance.GetDeviceGroup();
        if (deviceGroup == DeviceGroup.LowEnd)
        {
            QualitySettings.resolutionScalingFixedDPIFactor = .7f;
        }
        else if (deviceGroup == DeviceGroup.MidEnd)
        {
            QualitySettings.resolutionScalingFixedDPIFactor = 0.75f;
        }
        else if (deviceGroup == DeviceGroup.HighEnd)
        {
            QualitySettings.resolutionScalingFixedDPIFactor = 0.8f;
        }
        else if (deviceGroup == DeviceGroup.VeryHighEnd)
        {
            QualitySettings.resolutionScalingFixedDPIFactor = 1f;
        }
    }
}
