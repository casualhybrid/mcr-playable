using UnityEngine;
using FMODUnity;
using System;
using System.Collections.Generic;

public class AmbianceSoundHandler : SFXSettings
{
    [SerializeField] private GameEvent buildingVerticaleWalkEvent,/* buildingVerticaleWalkEndEvent,*/ playerReviveEvent; /*private GameEvent bridgeStartedEvent, bridgeEndedEvent,*/

    [SerializeField] private EnvironmentChannel environmentChannel;
    [SerializeField] private EnvironmentsEnumSO environmentsEnumSO;
    [SerializeField] private Dictionary<Environment, int> environmentAmbientParameters;
    [SerializeField] private SettingsEventChannelSO soundSettingsChanged;
    [SerializeField] private AudioManagerSO audioManagerSO;
    [SerializeField] private string carSurfaceSfxStr;
    private GameObject /*ambianceSound,*/ playerEngineSurfaceSfx;
    private StudioEventEmitter ambianceSoundEmitter, carEngineSoundEmitter;
    private int envNo = 0, envMatSFXNo = 0;
    private bool isPlayerEngineReady = false;/*, isSoundMuted = false;*/

    private void Awake()
    {
        ambianceSoundEmitter = GetComponent<StudioEventEmitter>();
        environmentChannel.OnPlayerEnvironmentChanged += HandlePlayerEnviornmentChangedSound;
    }

    private void OnDestroy()
    {
        environmentChannel.OnPlayerEnvironmentChanged -= HandlePlayerEnviornmentChangedSound;

    }

    private void HandlePlayerEnviornmentChangedSound(Environment env)
    {
        int key = -1;
        environmentAmbientParameters.TryGetValue(env, out key);

        if (key == -1)
            return;

       // UnityEngine.Console.Log("Key Is " + key);

        ambianceSoundEmitter.SetParameter("Amb", key);


    }

    private void OnEnable()
    {
        soundSettingsChanged.OnSettingChanged += ChangeSoundsSettings;
        buildingVerticaleWalkEvent.TheEvent.AddListener(PlayBuildingWalkAmbianceSFX);
      //  buildingVerticaleWalkEndEvent.TheEvent.AddListener(ChangeEnviormentMusic);
        //bridgeStartedEvent.TheEvent.AddListener(PlayBridgeSurfaceSFX);
        //bridgeEndedEvent.TheEvent.AddListener(EndBridgeSurfaceSFX);
        playerReviveEvent.TheEvent.AddListener(RevivePlayer);
    }

    //private void Start()
    //{
    //    if (PlayerPrefs.GetFloat("sound") == 0)
    //        isSoundMuted = true;
    //    else
    //        isSoundMuted = false;
    //}

    void Update()
    {

        //if (!ambianceSound)
        //{
        //    if (gameObject.transform.childCount > 0)
        //    {
        //        ambianceSound = gameObject.transform.GetChild(0).gameObject;
        //        ambianceSoundEmitter = ambianceSound.GetComponent<StudioEventEmitter>();
        //        SetUpLoopingSounds(transform);
        //    }
        //}


        if (!carEngineSoundEmitter)
        {
            playerEngineSurfaceSfx = audioManagerSO.GetEventSound(carSurfaceSfxStr);
            if (playerEngineSurfaceSfx != null)
            {
                carEngineSoundEmitter = playerEngineSurfaceSfx.GetComponent<StudioEventEmitter>();
                isPlayerEngineReady = true;
            }
        }
    }

    void RevivePlayer(GameEvent theEvent) {
        //  if (!isSoundMuted) {
        try
        {
            carEngineSoundEmitter.Play();
            carEngineSoundEmitter.SetParameter("Material", envMatSFXNo);

        }
        catch (Exception e) 
        {
            Debug.LogException(e);  
        }
      //  }
       // else
        //    carEngineSoundEmitter.Stop();
    }

    void PlayBridgeSurfaceSFX(GameEvent theGameEvent)
    {
     //   UnityEngine.Console.Log("Bridge sound started");
        if (isPlayerEngineReady)
            carEngineSoundEmitter.SetParameter("Material", 4);
    }

    void EndBridgeSurfaceSFX(GameEvent theGameEvent)
    {
     //   UnityEngine.Console.Log("Bridge sound Ended");
        if (isPlayerEngineReady)
            carEngineSoundEmitter.SetParameter("Material", envMatSFXNo);
    }

    void ChangeSoundsSettings(bool soundChanged) {
       // if (PlayerPrefs.GetFloat("sound") == 0)
       //     isSoundMuted = true;
       // else
       //     isSoundMuted = false;
        SetUpLoopingSounds(transform);
    }

    void PlayCityAmbianceSFX(GameEvent gameEvent)
    {
        envNo = 1;
        envMatSFXNo = 1;
        if (isPlayerEngineReady)
            carEngineSoundEmitter.SetParameter("Material", 1);
        //ambianceSoundEmitter.SetParameter("Amb", 1);
    }

    void PlayParkAmbianceSFX(GameEvent gameEvent)
    {
        envNo = 2;
        envMatSFXNo = 1;
        if (isPlayerEngineReady)
            carEngineSoundEmitter.SetParameter("Material", 1);
        //ambianceSoundEmitter.SetParameter("Amb", 2);
    }

    void PlaySeaAmbianceSFX(GameEvent gameEvent)
    {
     //  print("Playing Desert Env Sounds...");
        envNo = 3;
        envMatSFXNo = 2;
        if (isPlayerEngineReady)
            carEngineSoundEmitter.SetParameter("Material", 2);
        //ambianceSoundEmitter.SetParameter("Amb", 3);
    }

    void PlayBuildingWalkAmbianceSFX(GameEvent gameEvent)
    {
        UnityEngine.Console.Log("Key " + 4);

        envNo = 4;
        ambianceSoundEmitter.SetParameter("Amb", 4);

        envMatSFXNo = 3;
        if (isPlayerEngineReady)
            carEngineSoundEmitter.SetParameter("Material", 3);
    }

    //void ChangeEnviormentMusic(GameEvent theEvent)
    //{
    //    ambianceSoundEmitter.SetParameter("Amb", envNo);
    //    if (isPlayerEngineReady)
    //        carEngineSoundEmitter.SetParameter("Material", 1);
    //}

    private void OnDisable()
    {
        soundSettingsChanged.OnSettingChanged -= ChangeSoundsSettings;
        buildingVerticaleWalkEvent.TheEvent.RemoveListener(PlayBuildingWalkAmbianceSFX);
       // buildingVerticaleWalkEndEvent.TheEvent.RemoveListener(ChangeEnviormentMusic);
        //bridgeStartedEvent.TheEvent.RemoveListener(PlayBridgeSurfaceSFX);
        //bridgeEndedEvent.TheEvent.RemoveListener(EndBridgeSurfaceSFX);
        playerReviveEvent.TheEvent.RemoveListener(RevivePlayer);
    }
}
