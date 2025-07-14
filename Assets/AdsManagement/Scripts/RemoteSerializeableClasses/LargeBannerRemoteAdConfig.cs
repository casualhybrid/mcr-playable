using MessagePack;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class LargeBannerConfig
{
    [Key(0)] public bool Status;
    [Key(1)] public string Panel;
    [Key(2)] public string Location;
}

[CreateAssetMenu(fileName = "LargeBannerRemoteAdConfig", menuName = "ScriptableObjects/AdsConfiguration/LargeBannerRemoteAdConfig")]

[System.Serializable]
public class LargeBannerRemoteAdConfig : SerialzableRemoteAdConfig, ISerializationCallbackReceiver
{
    [Key(0)] public List<LargeBannerConfig> Config;
    [System.NonSerialized] public readonly Dictionary<string, LargeBannerConfig> ConfigDictionary = new Dictionary<string, LargeBannerConfig>();

    public void OnAfterDeserialize()
    {
        if (Config != null && Config.Count > 0)
        {
            ConfigDictionary?.Clear();

            for (int i = 0; i < Config.Count; i++)
            {
                LargeBannerConfig largeBannerConfig = Config[i];
                ConfigDictionary.Add(largeBannerConfig.Panel, largeBannerConfig);
            }
        }
    }

    public void OnBeforeSerialize()
    {
    }

    public override void ResetData()
    {
        Config?.Clear();
        ConfigDictionary?.Clear();
    }
}