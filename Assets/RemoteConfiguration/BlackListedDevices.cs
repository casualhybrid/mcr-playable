using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Sirenix.Serialization;
using System;

[System.Serializable]
[CreateAssetMenu(fileName = "BlackListedDevices", menuName = "ScriptableObjects/BlackListedDevices")]
public class BlackListedDevices : SerializedScriptableObject
{
    [OdinSerialize] [NonSerialized] public BlackListDeviceData BlackListedDevicesList;

#if UNITY_EDITOR
    [Button("GenerateJSON")]
    public void ConvertToJson()
    {
        //var json = JsonUtility.ToJson(this);
        var json = JsonConvert.SerializeObject(BlackListedDevicesList);

        UnityEngine.Console.Log(json);
    }
#endif


}