using System.Collections;
using UnityEngine;
using FMODUnity;

public class BackgroundSoundHandler : SFXSettings
{
    [SerializeField] private GameEvent introSfxEvent, verseSfxEvent, chorusSfxEvent, playerCrashedEvent,
        buildingVerticaleRunEvent, buildingVerticaleRunEndEvent, playerRevived, playerBeingChasedEvent, playerChaseEndEvent;
    [SerializeField] private SettingsEventChannelSO musicSettingsChanged;
    [SerializeField] private AudioManagerSO audioManagerSO;
    [SerializeField] private PlayerSharedData playerSharedData;
    private GameObject backgroundSound;
    private StudioEventEmitter backgroundSoundEmitter;
 
    private bool isGameStart = true;

    private void OnEnable()
    {
        musicSettingsChanged.OnSettingChanged += ChangeSoundsSettings;
        introSfxEvent.TheEvent.AddListener(PlayIntroMusic);
        verseSfxEvent.TheEvent.AddListener(PlayVerseMusic);
        chorusSfxEvent.TheEvent.AddListener(PlayChorusMusic);
        playerCrashedEvent.TheEvent.AddListener(PlayerDied);
        playerRevived.TheEvent.AddListener(PlayVerseMusic);
        playerBeingChasedEvent.TheEvent.AddListener(PlayerBeingChased);
        playerChaseEndEvent.TheEvent.AddListener(PlayVerseMusic);
        buildingVerticaleRunEvent.TheEvent.AddListener(PlayChorusMusic);
        buildingVerticaleRunEndEvent.TheEvent.AddListener(PlayVerseMusic);
        
    }

    private void Update()
    {
        if (!backgroundSound) {
            if (gameObject.transform.childCount > 0) {
                backgroundSound = gameObject.transform.GetChild(0).gameObject;
                backgroundSoundEmitter = backgroundSound.GetComponent<StudioEventEmitter>();

                SetUpLoopingMusic(transform);

            }
        }
        if (audioManagerSO.loopingEvents.Count > 0) {
          //  print("Looping Audios detected..." + audioManagerSO.loopingEvents.Count);
        }
    }


    void ChangeSoundsSettings(bool soundChanged)
    {
        SetUpLoopingMusic(transform);
    }

    void PlayerBeingChased(GameEvent theEvent)
    {
        if (!isGameStart) {
            if (!playerSharedData.isCrashed)
                backgroundSoundEmitter.SetParameter("Music", 6);
        }
        isGameStart = false;
    }

    void PlayIntroMusic(GameEvent theEvent)
    {
        backgroundSoundEmitter.SetParameter("Music", 1);
    }

    void PlayVerseMusic(GameEvent theEvent)
    {
        backgroundSoundEmitter.SetParameter("Music", 2);
    }

    void PlayChorusMusic(GameEvent theEvent)
    {
        backgroundSoundEmitter.SetParameter("Music", 3);
        
    }

    void PlayerDied(GameEvent theEvent)
    {
        backgroundSoundEmitter.SetParameter("Music", 5);
     //   UnityEngine.Console.Log("Changig Value to 5");
    }

    void ChangeEnviormentMusic(GameEvent theEvent)
    {
        backgroundSoundEmitter.SetParameter("Music", 4);
    }


    private void OnDisable()
    {
        musicSettingsChanged.OnSettingChanged -= ChangeSoundsSettings;
        introSfxEvent.TheEvent.RemoveListener(PlayIntroMusic);
        verseSfxEvent.TheEvent.RemoveListener(PlayVerseMusic);
        chorusSfxEvent.TheEvent.RemoveListener(PlayChorusMusic);
        playerCrashedEvent.TheEvent.RemoveListener(PlayerDied);
        playerRevived.TheEvent.RemoveListener(PlayVerseMusic);
        playerBeingChasedEvent.TheEvent.RemoveListener(PlayerBeingChased);
        playerChaseEndEvent.TheEvent.RemoveListener(PlayVerseMusic);
        buildingVerticaleRunEvent.TheEvent.RemoveListener(PlayChorusMusic);
        buildingVerticaleRunEndEvent.TheEvent.RemoveListener(PlayVerseMusic);
    }
}
