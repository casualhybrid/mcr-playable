using System;
using UnityEngine;

public static class ForceUpdateAppValidator
{
    public static event Action NewAppVersionIsAvailable;
    public static bool IsPendingUpdate { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void SubscribeToRemoteConfigFetched()
    {
        RemoteConfiguration.RemoteConfigurationDataFetched += RemoteConfiguration_RemoteConfigurationDataFetched;
    }

    private static bool CheckIfCurrentVersionIsOlder(string[] currentSplittedVersion, string[] remoteSplittedVersion, int index = 0)
    {
        if (index >= currentSplittedVersion.Length && index >= remoteSplittedVersion.Length)
            return true;

        if (index >= currentSplittedVersion.Length)
            return false;

        if (index >= remoteSplittedVersion.Length)
            return true;

        string a1 = currentSplittedVersion[index];
        string b1 = remoteSplittedVersion[index];

        int a = int.Parse(a1);
        int b = int.Parse(b1);

        if (a == b)
            return CheckIfCurrentVersionIsOlder(currentSplittedVersion, remoteSplittedVersion, ++index);
        else
            return a > b;
    }

    private static void RemoteConfiguration_RemoteConfigurationDataFetched()
    {
        try
        {
            string currentVersion = Application.version;
            string remoteVersion = RemoteConfiguration.LatestAppVersion;

            var splittedCurrentVersion = currentVersion.Split(".");
            var splittedRemoteVersion = remoteVersion.Split(".");

            IsPendingUpdate = !CheckIfCurrentVersionIsOlder(splittedCurrentVersion, splittedRemoteVersion);

            if (IsPendingUpdate)
            {
                UnityEngine.Console.Log($"Show Force Update Panel. Current Version: {currentVersion} and Remote Version: {remoteVersion}");
                NewAppVersionIsAvailable?.Invoke();
            }
            else
            {
                UnityEngine.Console.Log($"Version already up to date. Current Version: {currentVersion} and Remote Version: {remoteVersion}");
            }
        }
        catch (Exception e)
        {
            UnityEngine.Console.LogWarning($"Error occurred while checking for force update app {e}");
            IsPendingUpdate = false;
        }
    }
}