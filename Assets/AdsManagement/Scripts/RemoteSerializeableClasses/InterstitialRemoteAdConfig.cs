using MessagePack;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class SpecialConditionInterstial
{
    [Key(0)] public string AdType;
    [Key(1)] public int Times;
}

[MessagePackObject]
[System.Serializable]
public class InterstitialAdRequrements
{
    [Key(0)] public string Condition;
}

[System.Serializable]

public class InterstitialConfig
{
    [Key(0)] public bool Status;
    [Key(1)] public string Panel;
    [Key(2)] public string DefaultType;
    [Key(3)] public int AdFrequency;
    [Key(4)] public InterstitialAdRequrements Requirements;
    [Key(5)] public SpecialConditionInterstial SpecialCondition;
}

[CreateAssetMenu(fileName = "InterstitialRemoteAdConfig", menuName = "ScriptableObjects/AdsConfiguration/InterstitialRemoteAdConfig")]
[System.Serializable]

public class InterstitialRemoteAdConfig : SerialzableRemoteAdConfig, ISerializationCallbackReceiver
{
    [Key(0)] public List<InterstitialConfig> Config;

    [System.NonSerialized] public readonly Dictionary<string, InterstitialConfig> ConfigDictionary = new Dictionary<string, InterstitialConfig>();

    public InterstitialConfig GetInterstitialConfigForPanel(string panel)
    {
        InterstitialConfig interstitialConfig;
        ConfigDictionary.TryGetValue(panel, out interstitialConfig);

        if (interstitialConfig == null)
        {
            UnityEngine.Console.LogWarning($"Interstitial Ad Configuration for {panel} was requested but wasn't found. Maybe it doesn't exist in the remote");
        }

        return interstitialConfig;
    }

    public void OnAfterDeserialize()
    {
        if (Config != null && Config.Count > 0)
        {
            ConfigDictionary?.Clear();

            for (int i = 0; i < Config.Count; i++)
            {
                InterstitialConfig interstitialConfig = Config[i];
                ConfigDictionary.Add(interstitialConfig.Panel, interstitialConfig);
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