using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Implementation of Monobehavior based Event Sounds
/// <c>AudioPlayer</c> class will request <c>AudioManager</c> to play specific sounds through channels
/// Some of features of this script is handled from AudioPlayerEditor.cs script.
/// </summary>
public class AudioPlayer : SFXSettings
{
    [Tooltip("The AudioManager listens to this event, fired by objects in any scene, to play sounds")]
    [SerializeField] private AudioEventChannelSO audioEventChannel = default;

    [SerializeField] private bool useCustomConfig = false;

    [ShowIf("useCustomConfig")]
    [SerializeField] private AudioInstanceConfigSO audioInstanceConfigSO;

    [SerializeField] private TypeOfChannel channelType;
    [SerializeField] private MonobehaviourBasedEventTypes eventType;
    [SerializeField] private string eventStr;

    [ShowIf("eventType", MonobehaviourBasedEventTypes.OnGameEvent)]
    [SerializeField] private GameEvent gameEvent;

    public AudioInstanceConfig AudioInstanceConfig
    {
        get
        {
            if (audioInstanceConfigModified == null)
            {
                if (audioInstanceConfigSO != null)
                {
                    audioInstanceConfigModified = CloneAudioConfigData(audioInstanceConfigSO.AudioInstanceConfig, audioInstanceConfigModified);
                }
                else
                {
                    UnityEngine.Console.LogWarning("An audio configuration was requested but it's not assigned. Creating a new one");
                    audioInstanceConfigModified = new AudioInstanceConfig();
                }
            }

            return audioInstanceConfigModified;
        }
        set { audioInstanceConfigModified = value; _useCustomConfig = true; }
    }

    private bool _useCustomConfig;
    private AudioInstanceConfig audioInstanceConfigModified;

    private void Awake()
    {
        if (eventType == MonobehaviourBasedEventTypes.Awake)
        {
            ShootAudioEvent();
        }
        else if (eventType == MonobehaviourBasedEventTypes.OnButtonPressed)
        {
            Button button = GetComponent<Button>();

            if (button != null)
            {
                button.onClick.AddListener(ShootAudioEvent);
            }
        }
    }

    private void OnEnable()
    {
        _useCustomConfig = useCustomConfig;

        if (audioInstanceConfigSO != null && useCustomConfig)
        {
            audioInstanceConfigModified = CloneAudioConfigData(audioInstanceConfigSO.AudioInstanceConfig, audioInstanceConfigModified);
        }

        if (!useCustomConfig && audioInstanceConfigSO != null)
        {
            UnityEngine.Console.LogWarning($"Custom audio config isn't checked but a reference to audio config is assiged. Object {name}");
        }

        if (eventType == MonobehaviourBasedEventTypes.OnEnable)
        {
            ShootAudioEvent();
        }
        else if (eventType == MonobehaviourBasedEventTypes.OnGameEvent && gameEvent != null)
        {
            gameEvent.TheEvent.AddListener(ShootAudioEventOnGameEvent);
        }
    }

    private void Start()
    {
        if (eventType == MonobehaviourBasedEventTypes.Start)
        {
            ShootAudioEvent();
        }
    }

    private void OnDisable()
    {
        if (eventType == MonobehaviourBasedEventTypes.OnDisable)
        {
            ShootAudioEvent();
        }

        if (gameEvent != null)
        {
            gameEvent.TheEvent.RemoveListener(ShootAudioEventOnGameEvent);
        }
    }

    private AudioInstanceConfig CloneAudioConfigData(AudioInstanceConfig source, AudioInstanceConfig target)
    {
        if (target == null)
            target = new AudioInstanceConfig();

        target.parameterName = source.parameterName;
        target.parameterValue = source.parameterValue;
        target.Pitch = source.Pitch;
        return target;
    }

    private void ShootAudioEventOnGameEvent(GameEvent gameEvent)
    {
        ShootAudioEvent();
    }

    /// <summary>
    /// Both OneShot and Looping audios are handled from PlayAudio function
    /// </summary>
    public void ShootAudioEvent()
    {
      //  if (!IsSoundsOn())
      //      return;

        if (_useCustomConfig)
        {
            audioEventChannel.RaiseEvent(gameObject, eventStr, audioInstanceConfigModified);
        }
        else
        {
            audioEventChannel.RaiseEvent(gameObject, eventStr);
        }
    }

    public void SendEvent(string eventName)
    {
        AnalyticsManager.CustomData(eventName);
        UnityEngine.Console.LogError(eventName);
    }
}

/// <summary>
/// Dont change the pattern of these Enum and add new Enum at the end
/// </summary>
public enum MonobehaviourBasedEventTypes
{
    None, Awake, OnEnable, Start, OnDisable, OnGameEvent, OnButtonPressed
};

/// <summary>
/// Dont change the pattern of these Enum and add new Enum at the end
/// </summary>
public enum TypeOfChannel
{
    OneShot, Looping, EventBased
};