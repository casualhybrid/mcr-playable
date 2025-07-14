using MessagePack;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class SmallBannerConfig
{
    [Key(0)] public bool Status;
    [Key(0)] public string Panel;
    [Key(0)] public string Location;
}

[CreateAssetMenu(fileName = "SmallBannerRemoteAdConfig", menuName = "ScriptableObjects/AdsConfiguration/SmallBannerRemoteAdConfig")]

[System.Serializable]
public class SmallBannerRemoteAdConfig : SerialzableRemoteAdConfig, ISerializationCallbackReceiver
{
    [Key(0)] public List<SmallBannerConfig> Config;

    [System.NonSerialized] public readonly Dictionary<string, SmallBannerConfig> ConfigDictionary = new Dictionary<string, SmallBannerConfig>();

    public void OnAfterDeserialize()
    {
        if (Config != null && Config.Count > 0)
        {
            ConfigDictionary?.Clear();

            for (int i = 0; i < Config.Count; i++)
            {
                SmallBannerConfig smallBannerConfig = Config[i];
                ConfigDictionary.Add(smallBannerConfig.Panel, smallBannerConfig);
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