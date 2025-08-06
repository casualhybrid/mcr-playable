using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;



//[System.Serializable]
//public class BlackListLevelDictionary : SerializableDictionaryBase<string, BlackListLevel>
//{ }

[System.Serializable]
public struct BlackListDeviceData
{
    public Dictionary<string, BlackListLevel> BlackListedDevicesList;
}

[System.Serializable]
public struct BlackListChipsetData
{
    public List<string> BlackListedChipsetList;
}

public class BlackListManager : MonoBehaviour
{
    public struct BlackListResult
    {
        public bool IsChipsetBlackListed;
        public bool IsModelBlackListed;

        public BlackListResult(bool isChipsetBlackListed, bool isModelBlackListed)
        {
            IsChipsetBlackListed = isChipsetBlackListed;
            IsModelBlackListed = isModelBlackListed;
        }
    }

    public static event Action OnDeviceIsBlackListed;

    public static event Action BlackListOperationCompleted;

    [SerializeField] private BlackListedDevices blackListedDevices;
    [SerializeField] private BlackListedChipset blackListedChipsets;

    private bool IsRunning;
    public bool operationCompletedOnce { get; private set; }
    private bool operationIsUptoDateWithRemote;

    private void OnEnable()
    {
        IsRunning = false;
    }

    private void Awake()
    {
        RemoteConfiguration.RemoteConfigurationDataFetched += RemoteConfiguration_RemoteConfigurationDataFetched;
        DontDestroyOnLoad(this.gameObject);
    }

    //private IEnumerator Start()
    //{
    //    yield return null;
    //    CheckAndMarkIfDeviceIsBlackListed();
    //}

    private void SendCompletionEventIfFirstTime()
    {
        if (operationCompletedOnce)
            return;

        operationCompletedOnce = true;
        BlackListOperationCompleted?.Invoke();
    }

    public async void CheckAndMarkIfDeviceIsBlackListed()
    {
        try
        {
            if (operationIsUptoDateWithRemote)
            {
                UnityEngine.Console.Log("BlackList Operation is already upto date with remote. Ignoring");
                SendCompletionEventIfFirstTime();
                return;
            }

            if (EstimateDevicePerformance.IsChipSetBlackListed)
            {
                SendCompletionEventIfFirstTime();
                return;
            }

            var model = SystemInfo.deviceModel;

            //  UnityEngine.Console.Log($"Device model is {model}");

            Task<BlackListResult> task = IsDeviceBlackListed(model);

            if (task == null)
            {
                SendCompletionEventIfFirstTime();
                return;
            }

            try
            {
                await task;
            }
            catch (Exception e)
            {
                UnityEngine.Console.LogError($"Exception Occurred While Checking For BlackListed. Exception: {e}");
            }
            finally
            {
                IsRunning = false;
            }

            SendChipsetUsedEvent();

            bool isBlackListed = task.Result.IsChipsetBlackListed || task.Result.IsModelBlackListed;

            EstimateDevicePerformance.IsPhoneBlackListed = isBlackListed;
            EstimateDevicePerformance.IsChipSetBlackListed = task.Result.IsChipsetBlackListed;

            if (isBlackListed)
            {
                OnDeviceIsBlackListed?.Invoke();

                if (task.Result.IsModelBlackListed)
                {
                    UnityEngine.Console.Log($"Device {model} is blacklisted");
                    AnalyticsManager.CustomData("DeviceBlackListed", new Dictionary<string, object> { { "Model", model } });
                }
                if (task.Result.IsChipsetBlackListed)
                {
                    UnityEngine.Console.Log($"Chipset {EstimateDevicePerformance.DeviceChipset} is blacklisted");
                    AnalyticsManager.CustomData("ChipsetBlackListed", new Dictionary<string, object> { { "Model", model }, { "Chipset", EstimateDevicePerformance.DeviceChipset } });
                }
            }
            else
            {
                UnityEngine.Console.Log($"Device {model} with chipset {EstimateDevicePerformance.DeviceChipset} is not blacklisted");
            }

            SendCompletionEventIfFirstTime();
        }
        catch (Exception e)
        {
            UnityEngine.Console.LogError("Error occurred during the blacklist operation. Error : " + e);
            SendCompletionEventIfFirstTime();
        }
    }

    private void RemoteConfiguration_RemoteConfigurationDataFetched()
    {
        StartCoroutine(WaitForRunningProccessAndUpdateBlackListedData());
    }

    private IEnumerator WaitForRunningProccessAndUpdateBlackListedData()
    {
        yield return new WaitWhile(() => IsRunning || !operationCompletedOnce);

        CheckAndMarkIfDeviceIsBlackListed();
    }

    private void SendChipsetUsedEvent()
    {
        string hasSentChipsetInfoString = "HasSentChipsetInformation";
        int hasSentChipsetInfo = PlayerPrefs.GetInt(hasSentChipsetInfoString, 0);
        if (hasSentChipsetInfo == 0 && !String.IsNullOrEmpty(EstimateDevicePerformance.DeviceChipset))
        {
            AnalyticsManager.CustomData("ChipsetBeingUsed", new Dictionary<string, object> { { "Model", SystemInfo.deviceModel }, { "Chipset", EstimateDevicePerformance.DeviceChipset } });
            PlayerPrefs.SetInt(hasSentChipsetInfoString, 1);
        }
    }

    public Task<BlackListResult> IsDeviceBlackListed(string model)
    {
        if (IsRunning)
        {
            UnityEngine.Console.LogWarning("The task for is device blacklisted is already running");
            return null;
        }

        IsRunning = true;

        return Task.Run(() =>
        {
           

            int attachedToJNI = -1;
            int deattachedFromJNI = -1;

            try
            {
                BlackListResult blackListResult = new BlackListResult();

                bool remoteDataAvailable = RemoteConfiguration.BlackListJson != null && RemoteConfiguration.BlackListChipsetJson != null;

                if (RemoteConfiguration.BlackListJson != null)
                {
                    BlackListDeviceData obj1 = JsonConvert.DeserializeObject<BlackListDeviceData>(RemoteConfiguration.BlackListJson);
                    blackListedDevices.BlackListedDevicesList = obj1;
                    RemoteConfiguration.BlackListJson = null;
                }

                if (RemoteConfiguration.BlackListChipsetJson != null)
                {
                    BlackListChipsetData obj2 = JsonConvert.DeserializeObject<BlackListChipsetData>(RemoteConfiguration.BlackListChipsetJson);
                    blackListedChipsets.BlackListedChipsetList = obj2.BlackListedChipsetList;
                    RemoteConfiguration.BlackListChipsetJson = null;
                }

                // Check for black listed model
                foreach (var item in blackListedDevices.BlackListedDevicesList.BlackListedDevicesList)
                {
                    var deviceModel = item.Key;

                    if (model.Contains(deviceModel) && !String.IsNullOrEmpty(deviceModel))
                    {
                        IsRunning = false;
                        EstimateDevicePerformance.PhoneBlackListLevel = item.Value;
                        blackListResult.IsModelBlackListed = true;
                        //   return new BlackListResult(false, true);
                    }
                }

                // Check for blacklisted chipset

                string chipset = null;

                attachedToJNI = AndroidJNI.AttachCurrentThread();

                if (attachedToJNI != 0)
                {
                    UnityEngine.Console.LogWarning("Failed to attach current thread to JVM. Not checking for chipset blacklist");
                    IsRunning = false;
                    blackListResult.IsChipsetBlackListed = false;
                    return blackListResult;
                }

#if UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass chipsetPlugin = new AndroidJavaClass("com.frosty.memoryfindermodule.FindChipset"))
            {
                chipset = chipsetPlugin.CallStatic<string>("getCPUSerial");
            }
#endif

                deattachedFromJNI = AndroidJNI.DetachCurrentThread();

                if (String.IsNullOrEmpty(chipset) || chipset == " ")
                {
                    UnityEngine.Console.Log("Unable to find device chipset");
                    operationIsUptoDateWithRemote = remoteDataAvailable;
                    IsRunning = false;
                    blackListResult.IsChipsetBlackListed = false;
                    return blackListResult;
                }

                EstimateDevicePerformance.DeviceChipset = chipset;

                var splitChipsetString = chipset.Split(" ", StringSplitOptions.RemoveEmptyEntries);

                if (splitChipsetString != null)
                {
                    for (int i = 0; i < blackListedChipsets.BlackListedChipsetList.Count; i++)
                    {
                        if (blackListResult.IsChipsetBlackListed)
                            break;

                        var deviceChipset = blackListedChipsets.BlackListedChipsetList[i];

                        for (int k = 0; k < splitChipsetString.Length; k++)
                        {
                            string split = splitChipsetString[k];

                        //    UnityEngine.Console.Log($"Splitted Chipset {split}");

                            // if (chipset.Contains(deviceChipset) && !String.IsNullOrEmpty(deviceChipset))
                            if (split.Equals(deviceChipset) && !String.IsNullOrEmpty(deviceChipset))
                            {
                                IsRunning = false;
                                blackListResult.IsChipsetBlackListed = true;
                                break;
                                // return new BlackListResult(true, false);
                            }
                        }
                    }
                }

                operationIsUptoDateWithRemote = remoteDataAvailable;
                IsRunning = false;
                return blackListResult;
            }
            catch
            {
                return new BlackListResult();
            }
            finally
            {
                if (attachedToJNI == 0 && deattachedFromJNI != 0)
                {
                    UnityEngine.Console.LogWarning("Thread wasn't deattached from JNI on thread exit. Trying to deattach now");
                    AndroidJNI.DetachCurrentThread();
                }

              
            }
        });
    }
}