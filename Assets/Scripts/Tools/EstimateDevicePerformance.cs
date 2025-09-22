using System.Collections.Generic;
using UnityEngine;


public enum DeviceGroup
{
    LowEnd, MidEnd, HighEnd, VeryHighEnd
}

public static class EstimateDevicePerformance
{
    private static float systemMemoryStandard = 8000;
    private static float processorCountStandard = 8;

    private static float SystemMemoryImpact = 1f;
    private static float processorCountImpact = 0f;

    private static DeviceGroup deviceGroup;
    private static bool deviceGroupAssigned;

    public static bool isOpenGLTwoDevice { get; private set; }

    public static bool isRamLowerThanFourGB { get; private set; }

    public static bool isRamLowerThanThreeGB { get; private set; }

    public static bool isRamLowerThanTwoGB { get; private set; }

    public static bool isOreoDevice { get; private set; }

    public static bool IsPhoneBlackListed { get; set; } = false;


    public static BlackListLevel PhoneBlackListLevel { get; set; }
    public static bool IsChipSetBlackListed { get; set; }

    public static string DeviceChipset { get; set; } 

    /// <summary>
    /// Estimates which group the device belongs to based on hardware parameters. Can be inaccurate, so only use it at first launch.
    /// </summary>
    public static DeviceGroup GetDeviceGroup()
    {
        if (deviceGroupAssigned)
            return deviceGroup;

        string apiVersion = SystemInfo.graphicsDeviceVersion;
        isOpenGLTwoDevice = apiVersion.Contains("OpenGL 2.0");

        float SystemMemory = SystemInfo.systemMemorySize;
        //  float VideoMemory = SystemInfo.graphicsMemorySize;
        float CpuCount = SystemInfo.processorCount;

   
       AnalyticsManager.CustomData("GraphicVersion", new Dictionary<string, object> { { "GraphicsApi", apiVersion } });

        if (SystemMemory <= 3072)
        {
            isRamLowerThanThreeGB = true;
        }

        if (SystemMemory <= 2048)
        {
            isRamLowerThanTwoGB = true;
        }

        if (SystemMemory <= 4096)
        {
            isRamLowerThanFourGB = true;
        }


        // if (SystemMemory >= 5500)
        //  {
        //GraphicManager.isHighRam = true;
        // }

        isOreoDevice = SystemInfo.operatingSystem.Contains("API-27") ? true : false;

        // UnityEngine.Console.Log("SystemMemory: " + SystemMemory + ", VideoMemory: " + VideoMemory+ ", CPUCount: " + CpuCount);
        // UnityEngine.Console.Log("GraphicsAPI: " + SystemInfo.graphicsDeviceType);

        float SystemMemoryNormalized = Mathf.Clamp01(SystemMemory / systemMemoryStandard);
        float CpuCountNormalized = Mathf.Clamp01(CpuCount / processorCountStandard);

        float SystemMemoryScore = SystemMemoryNormalized * SystemMemoryImpact;
        float CPUCountScore = CpuCountNormalized * processorCountImpact;

        float TotalScore = SystemMemoryScore + CPUCountScore;

        DeviceGroup group;

        if(TotalScore >= 0.9f)
        {
            group = DeviceGroup.VeryHighEnd;
        }
        else if (TotalScore >= 0.68f)
        {
            group = DeviceGroup.HighEnd;
        }
        else if (TotalScore >= 0.35f)
        {
            group = DeviceGroup.MidEnd;
        }
        else
        {
            group = DeviceGroup.LowEnd;
        }

        string log = "Device Group Is: " + group + " with a score of: " + TotalScore;
        UnityEngine.Console.Log(log);

       

        deviceGroupAssigned = true;
        deviceGroup = group;

        return group;
    }
}