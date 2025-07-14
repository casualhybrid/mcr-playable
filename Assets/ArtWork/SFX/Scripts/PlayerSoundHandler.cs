using FMODUnity;
using TheKnights.SaveFileSystem;
using UnityEngine;

public class PlayerSoundHandler : SFXSettings
{
    [SerializeField] private PlayerSharedData playerData;
    [SerializeField] private AudioManagerSO audioManagerSo;
    [SerializeField] private SettingsEventChannelSO soundSettingsChanged;
    [SerializeField] private SaveManager saveManager;

    [SerializeField]
    private string jumpSfxStr, changeLaneSfxStr, dodgedSfxStr, duckedSfxStr,
        deathSfxStr, engineWithSurfaceSfxStr, springJumpSfxStr, dashSfxStr,
        damageSfxStr, shockwaveSfxStr, airLaneChangeSfxStr, cutSceneSFXStr,
        sideRampSfxStr, playerLandedonGroundSfxStr, rampJumpSfxStr, chaserAbusingSfxStr, CarSound,
        boostRunSfxStr, selectgender, diamondStr, pogoStickSfxStr, magnetPowerSfxStr, armourPowerSfxStr, aeroplanePowerSfxStr;

    [SerializeField]
    private GameEvent  playerJumpedEvent, playerChangeLaneEvent, playerDodgedEvent,
        playerInstantiated, playerDuckEvent, playerDeathEvent, armourUsedUp, gameStartedEvent, playerStartedStumblingEvent, playerEndStumbledEvent,
        springJumpEvent, dashStartedSfxEvent, dashEndedSfxEvent, playerDamageEvent, playerCutSceneStumblingEvent,
        boostStartedSfxEvent, boostEndedSfxEvent, playerHasRevivedEvent,
        shockwaveEvent, cutSceneStartEvent, onTheSideRampEvent, offTheSideRampEvent, playerLandedOngroundEvent,
        rampRunEvent, chaserAbusingEvent,CharacterHasBeenslected,StopCarEngine,StartCarEngine,diamondpickup,pogostickHasBeenPickedUp, magnetHasBeenPickedUp, armourHasBeenPickedUp,
        aeroplaneStateStarted;

    private FMOD.Studio.EventInstance sideRampEventInstance, wallRunEventInstance, boostEventInstance, instanceForParamValue;
    private FMOD.Studio.PLAYBACK_STATE sideRampPBState, wallRunPBState, boostRunPBState;

    [SerializeField] private AudioEventChannelSO oneShotChannelSO = default, loopingChannelSO = default;

    private GameObject surfaceEmitterObj;
    public StudioEventEmitter carSurfaceEmitter, carEngineEmitter,Car1EngineSound,Car2EngineSound,
        Car3EngineSound,Car4EngineSound,Car5EngineSound,DiamondpickUp;

    private bool isEngineRunning = false, isPlayerEngineReady = false, isPlayerBoosting = false,
        isStumblePlaying = false, /*isSoundMuted = false,*/ isPlayerGroundStateChanged = false;

    private float netSpeed = 0, oldSpeed = 0f;

    private const string surfaceContactString = "Surface_Contact";
    private const string materialString = "Material";

    // Start is called before the first frame update
    private void OnEnable()
    {
        soundSettingsChanged.OnSettingChanged += ChangeSoundsSettings;
        playerJumpedEvent.TheEvent.AddListener(PlayJumpSFX);
        playerChangeLaneEvent.TheEvent.AddListener(PlayLaneChangeSFX);
        playerDodgedEvent.TheEvent.AddListener(PlayDodgedSFX);
        playerInstantiated.TheEvent.AddListener(PlayEngineSound);
        playerDuckEvent.TheEvent.AddListener(PlayDuckSFX);
        playerDeathEvent.TheEvent.AddListener(PlayerDeathSFX);
        armourUsedUp.TheEvent.AddListener(PlayerArmourCrashSFX);
        gameStartedEvent.TheEvent.AddListener(StartGame);
        springJumpEvent.TheEvent.AddListener(PlayDoubleJumpSFX);
        dashStartedSfxEvent.TheEvent.AddListener(PlayStartedDashSFX);
        dashEndedSfxEvent.TheEvent.AddListener(PlayerEndedDashSFX);
        boostStartedSfxEvent.TheEvent.AddListener(PlayStartedBoostSFX);
        boostEndedSfxEvent.TheEvent.AddListener(PlayerEndedBoostSFX);
        playerDamageEvent.TheEvent.AddListener(PlayPlayerHitSFX);
        shockwaveEvent.TheEvent.AddListener(PlayShockWaveSFX);
        cutSceneStartEvent.TheEvent.AddListener(PlayCutSceneSFX);
        onTheSideRampEvent.TheEvent.AddListener(PlayOnTheSideRampSFX);
        offTheSideRampEvent.TheEvent.AddListener(StopSideRampSFX);
        playerLandedOngroundEvent.TheEvent.AddListener(PlayerLandedAfterWallClimbSFX);
        rampRunEvent.TheEvent.AddListener(PlayRampRunSFX);
        chaserAbusingEvent.TheEvent.AddListener(ChaserAbusingSFXInstance);
        playerHasRevivedEvent.TheEvent.AddListener(PlayerHasRevived);
        playerStartedStumblingEvent.TheEvent.AddListener(PlayerStartedStumblingSFX);
        playerEndStumbledEvent.TheEvent.AddListener(PlayerEndedStumblingSFX);
        playerCutSceneStumblingEvent.TheEvent.AddListener(PlayerCutSceneStumble);
        CharacterHasBeenslected.TheEvent.AddListener(CharacterSelectedGender);
        diamondpickup.TheEvent.AddListener(DiamondPickupSound);
        pogostickHasBeenPickedUp.TheEvent.AddListener(PogoStickSound);
        magnetHasBeenPickedUp.TheEvent.AddListener(MagnetPowerPickupSound);
        armourHasBeenPickedUp.TheEvent.AddListener(ArmourPowerPickupSound);
        aeroplaneStateStarted.TheEvent.AddListener(PlayAeroPlaneSFX);


        StopCarEngine.TheEvent.AddListener(EngineOff);
        StartCarEngine.TheEvent.AddListener(EngineOn);
    }
    private void Start()
    {
        //if (PlayerPrefs.GetFloat("sound") == 0)
        //    isSoundMuted = true;
        //else
        //    isSoundMuted = false;

        sideRampEventInstance = RuntimeManager.CreateInstance(sideRampSfxStr);
        boostEventInstance = RuntimeManager.CreateInstance(boostRunSfxStr);

        int param = saveManager.MainSaveFile.currentlySelectedCharacter + 1;
        boostEventInstance.setParameterByName("Character", param);

        SetUpLoopingSounds(gameObject.transform);       // carEngine Sound
    }

    public void DiamondPickupSound(GameEvent gameEvent)
    {
        if (saveManager.MainSaveFile.currentlySelectedCharacter ==0 )//PlayerPrefs.GetInt("CharacterGender") == 0)
            oneShotChannelSO.RaiseEvent(gameObject, diamondStr,"Character",1);
        else  if (saveManager.MainSaveFile.currentlySelectedCharacter ==1)
        {
            oneShotChannelSO.RaiseEvent(gameObject, diamondStr,"Character",2); 
        }
        else
        {
            oneShotChannelSO.RaiseEvent(gameObject, diamondStr,"Character",3);
        }
    }

    public void PogoStickSound(GameEvent gameEvent)
    {
        //if (!isSoundMuted)
        int param = saveManager.MainSaveFile.currentlySelectedCharacter + 1;
            oneShotChannelSO.RaiseEvent(gameObject, pogoStickSfxStr, "Character", param);
    }

    private void MagnetPowerPickupSound(GameEvent gameEvent)
    {
        int param = saveManager.MainSaveFile.currentlySelectedCharacter + 1;
        oneShotChannelSO.RaiseEvent(gameObject, magnetPowerSfxStr, "Character", param);
    }

    private void ArmourPowerPickupSound(GameEvent gameEvent)
    {
        int param = saveManager.MainSaveFile.currentlySelectedCharacter + 1;
        oneShotChannelSO.RaiseEvent(gameObject, armourPowerSfxStr, "Character", param);
    }


    private void SelectsoundOfCharacters(GameEvent gameEvent)
    {
        
    }

    public void EngineOff(GameEvent gameEvent)
    {
       // if (!isSoundMuted)
       // {
            UpdateSoundOff();
//            Car1EngineSound.Stop();
//            Car1EngineSound.enabled = false;
            //    carSurfaceEmitter.Stop();
      //  }

    }
    public void EngineOn(GameEvent gameEvent)
    {
       // if (!isSoundMuted)
     //   {
            if (saveManager.MainSaveFile.currentlySelectedCar == 0) 
            {
                //Car1EngineSound.Play();
            }
            else if (saveManager.MainSaveFile.currentlySelectedCar == 2)
            {
                Car2EngineSound.Play();
            }
            else if (saveManager.MainSaveFile.currentlySelectedCar == 4)
            {
                Car3EngineSound.Play();
            }
            else if (saveManager.MainSaveFile.currentlySelectedCar == 3)
            {
                Car4EngineSound.Play();
            }
            else if (saveManager.MainSaveFile.currentlySelectedCar == 1)
            {
                
//                oneShotChannelSO.RaiseEvent(gameObject, CarEngines,"Type",5);
                Car5EngineSound.Play();
                
            }
            carEngineEmitter.Play();
//            carSurfaceEmitter.Play();
        //    StartCarEngine.RaiseEvent();
     //   }
    }


    void EngineAndSurfaceSoundsCheck() {
     
       
        
      //  if (!isSoundMuted)
            UpdateSoundOn();
       // else
         //   UpdateSoundOff();
//            carEngineEmitter.Stop();
//        if (!isSoundMuted)
//            carSurfaceEmitter.Play();
//        else
//            carSurfaceEmitter.Stop();

        
    }

    void ChangeSoundsSettings(bool soundChanged)
    {
      //  isSoundMuted = !soundChanged;
        EngineAndSurfaceSoundsCheck();
    }

    private void ChaserAbusingSFXInstance(GameEvent theEvent)
    {
        //if (!isSoundMuted)
            oneShotChannelSO.RaiseEvent(gameObject, chaserAbusingSfxStr);
    }

    private void PlayRampRunSFX(GameEvent theEvent)
    {
      //  if (!isSoundMuted)
            oneShotChannelSO.RaiseEvent(gameObject, rampJumpSfxStr);
    }

    private void PlayStartedBoostSFX(GameEvent theEvent)
    {
      //  if (!isSoundMuted)
      //  {
            isPlayerBoosting = true;
            boostEventInstance.getPlaybackState(out boostRunPBState);
            boostEventInstance.start();
      //  }
    }

    private void PlayerEndedBoostSFX(GameEvent theEvent)
    {
      //  if (!isSoundMuted)
       // {
            isPlayerBoosting = false;
            boostEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        //}
    }

    private void PlayCutSceneSFX(GameEvent theEvent)
    {
      //  if (!isSoundMuted)
       // {
            if (saveManager.MainSaveFile.currentlySelectedCar == 0)
            {
                //oneShotChannelSO.RaiseEvent(gameObject, cutSceneSFXStr,"Type",1);
            }
            else if (saveManager.MainSaveFile.currentlySelectedCar == 2)
            {
                oneShotChannelSO.RaiseEvent(gameObject, cutSceneSFXStr,"Type",2);
            }
            else if (saveManager.MainSaveFile.currentlySelectedCar == 4)
            {
                oneShotChannelSO.RaiseEvent(gameObject, cutSceneSFXStr,"Type",3);
            }
            else if (saveManager.MainSaveFile.currentlySelectedCar == 3)
            {
                oneShotChannelSO.RaiseEvent(gameObject, cutSceneSFXStr,"Type",4);
            }
            else if (saveManager.MainSaveFile.currentlySelectedCar == 1)
            {
                oneShotChannelSO.RaiseEvent(gameObject, cutSceneSFXStr,"Type",5);
            }
       // }
    }

    private void PlayerLandedAfterWallClimbSFX(GameEvent theEvent)
    {
      //  if (!isSoundMuted)
            oneShotChannelSO.RaiseEvent(gameObject, playerLandedonGroundSfxStr);
    }

    private void PlayOnTheSideRampSFX(GameEvent theEvent)
    {
       // if (!isSoundMuted)
       // {
            sideRampEventInstance.getPlaybackState(out sideRampPBState);
            sideRampEventInstance.start();
      //  }
    }

    private void StopSideRampSFX(GameEvent theEvent)
    {
       // if (!isSoundMuted)
            sideRampEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    private void PlayAeroPlaneSFX(GameEvent theEvent)
    {
        int param = saveManager.MainSaveFile.currentlySelectedCharacter + 1;

        oneShotChannelSO.RaiseEvent(gameObject, aeroplanePowerSfxStr, "Character", param);
    }

    private void PlayShockWaveSFX(GameEvent theEvent)
    {
      //  if (!isSoundMuted)
        //{
            if (saveManager.MainSaveFile.currentlySelectedCharacter == 0)
            {
                 oneShotChannelSO.RaiseEvent(gameObject, shockwaveSfxStr,"Character",1);
            }
            else if (saveManager.MainSaveFile.currentlySelectedCharacter == 1)
            {
                oneShotChannelSO.RaiseEvent(gameObject, shockwaveSfxStr,"Character",2);
            }
            else
            {
                oneShotChannelSO.RaiseEvent(gameObject, shockwaveSfxStr,"Character",3);
            }
       // }

       
    }

    private void PlayPlayerHitSFX(GameEvent theEvent)
    {
       // if (!isSoundMuted)
       // {
             if (saveManager.MainSaveFile.currentlySelectedCharacter == 0)
             { 
                 oneShotChannelSO.RaiseEvent(gameObject, damageSfxStr,"Character",1);
             }
             else if (saveManager.MainSaveFile.currentlySelectedCharacter == 1)
             {
                 oneShotChannelSO.RaiseEvent(gameObject, damageSfxStr,"Character",2);
             }
             else
             {
                 oneShotChannelSO.RaiseEvent(gameObject, damageSfxStr,"Character",3);
             }
       // }
    }

    private void PlayStartedDashSFX(GameEvent theEvent)
    {
       // if (!isSoundMuted)
       // {
            if (saveManager.MainSaveFile.currentlySelectedCharacter ==0 )
            oneShotChannelSO.RaiseEvent(gameObject, dashSfxStr,"Character",1);
            else if (saveManager.MainSaveFile.currentlySelectedCharacter ==1 ) 
            {
                oneShotChannelSO.RaiseEvent(gameObject, dashSfxStr,"Character",2); 
            }
            else
            {
                oneShotChannelSO.RaiseEvent(gameObject, dashSfxStr,"Character",3);
            }
       // }
    }

    private void PlayerEndedDashSFX(GameEvent theEvent)
    {
     //   print("Ended Dash.....");
    }

    private void PlayDoubleJumpSFX(GameEvent theEvent)
    {
        //if (!isSoundMuted)
            oneShotChannelSO.RaiseEvent(gameObject, springJumpSfxStr);
    }

    private void PlayJumpSFX(GameEvent theEvent)
    {
      //  if (!isSoundMuted)
            if (saveManager.MainSaveFile.currentlySelectedCharacter ==0 )
            {
                oneShotChannelSO.RaiseEvent(gameObject, jumpSfxStr,"Character",1);
            }
            else  if (saveManager.MainSaveFile.currentlySelectedCharacter ==1 )
            {
                oneShotChannelSO.RaiseEvent(gameObject, jumpSfxStr,"Character",2);
            }
            else
            {
                oneShotChannelSO.RaiseEvent(gameObject, jumpSfxStr,"Character",3);
            }
    }

    private void PlayLaneChangeSFX(GameEvent theEvent)
    {
       // if (!isSoundMuted)
        //{
            if (saveManager.MainSaveFile.currentlySelectedCharacter ==0 )
            {
                if (playerData.InAir)
                    oneShotChannelSO.RaiseEvent(gameObject, airLaneChangeSfxStr,"Character",1);
                else
                    oneShotChannelSO.RaiseEvent(gameObject, changeLaneSfxStr,"Character",1);
            }
            else  if (saveManager.MainSaveFile.currentlySelectedCharacter ==1 )
            {
                if (playerData.InAir)
                    oneShotChannelSO.RaiseEvent(gameObject, airLaneChangeSfxStr,"Character",2);
                else
                    oneShotChannelSO.RaiseEvent(gameObject, changeLaneSfxStr,"Character",2);
            }
            else
            {
                if (playerData.InAir)
                    oneShotChannelSO.RaiseEvent(gameObject, airLaneChangeSfxStr,"Character",3);
                else
                    oneShotChannelSO.RaiseEvent(gameObject, changeLaneSfxStr,"Character",3);
            }
       // }
    }
    
    private void PlayDodgedSFX(GameEvent theEvent)
    {
       // if (!isSoundMuted)
            oneShotChannelSO.RaiseEvent(gameObject, dodgedSfxStr);
    }

    public void CharacterSelectedGender(GameEvent theEvent)
    {
       // if (!isSoundMuted)
       // {
            if (saveManager.MainSaveFile.currentlySelectedCharacter ==0 )
            {
                if (playerData.CurrentStateName != PlayerState.PlayerCanceljump)
                    oneShotChannelSO.RaiseEvent(gameObject, selectgender,"Character",1);
            }
            else  if (saveManager.MainSaveFile.currentlySelectedCharacter ==1 )
            {
                oneShotChannelSO.RaiseEvent(gameObject, selectgender,"Character",2);
            }

            else
            {
                oneShotChannelSO.RaiseEvent(gameObject, selectgender,"Character",3); 
            }
        
     //   }
    }

    private void PlayDuckSFX(GameEvent theEvent)
    {
        int param = saveManager.MainSaveFile.currentlySelectedCharacter + 1;
          
        if (playerData.CurrentStateName != PlayerState.PlayerCanceljump)
        {
            oneShotChannelSO.RaiseEvent(gameObject, duckedSfxStr, "Character", param);
        }
    }

    private void PlayerArmourCrashSFX(GameEvent theEvent)
    {
       // if (!isSoundMuted)
       // {
            oneShotChannelSO.RaiseEvent(gameObject, deathSfxStr);
       // }
    }

    private void PlayerDeathSFX(GameEvent theEvent)
    {
       // if (!isSoundMuted)
       // {
            oneShotChannelSO.RaiseEvent(gameObject, deathSfxStr);

            //carSurfaceEmitter.SetParameter("Speed", 0);
            carSurfaceEmitter.Stop();
            Car1EngineSound.Stop();
            Car2EngineSound.Stop();
            Car3EngineSound.Stop();
            Car4EngineSound.Stop();
            Car5EngineSound.Stop();
            //isEngineRunning = false;
       // }
        //carEngineSoundEmitter.SetParameter("Speed", 0.2f);
    }

    private void PlayerHasRevived(GameEvent theEvent)
    {
      //  if (!isSoundMuted)
      //  {
            UpdateSoundOn();
       // }
       // else
       // {
       //     UpdateSoundOff();
       // }

//            CarsEngineSounds.Play();
        //carEngineSoundEmitter.SetParameter("Speed", 1);
    }

    public void PlayEngineSound(GameEvent theEvent)
    {
        //carEngineSoundEmitter.SetParameter("Speed", 1);
        //loopingChannelSO.RaiseEvent(gameObject, carEngineSfxStr);
        //isEngineRunning = true;

        //if (!isSoundMuted)
        {
            loopingChannelSO.RaiseEvent(gameObject, engineWithSurfaceSfxStr);
            isEngineRunning = true;

        }
    }

    private void PlayerStartedStumblingSFX(GameEvent theEvent)
    {
        //carEngineSoundEmitter.SetParameter("Speed", 0);

       // if (!isSoundMuted)
       // {
            UpdateSoundOff();

            UpdateSoundOn();
        //}
    }

    private void PlayerEndedStumblingSFX(GameEvent theEvent)
    {
        //carEngineSoundEmitter.SetParameter("Speed", 1);
    }

    private void PlayerCutSceneStumble(GameEvent theEvent)
    {
        UpdateSoundOff();
    }

    public void UpdateSoundOff()
    {
        Car1EngineSound.enabled = false;
        Car2EngineSound.enabled = false;
        Car3EngineSound.enabled = false;
        Car4EngineSound.enabled = false;
        Car5EngineSound.enabled = false;
            
            
        Car3EngineSound.Stop();
        Car1EngineSound.Stop();
        Car4EngineSound.Stop();
        Car5EngineSound.Stop();
        Car2EngineSound.Stop();
    }
    public void UpdateSoundOn()
    {
       // if (!isSoundMuted)
      //  {
            if (saveManager.MainSaveFile.currentlySelectedCar == 0)
            {
                Car1EngineSound.enabled = false;
                Car2EngineSound.enabled = false;
                Car3EngineSound.enabled = false;
                Car4EngineSound.enabled = false;
                Car5EngineSound.enabled = false;
                Car1EngineSound.enabled = true;
            }
            else if (saveManager.MainSaveFile.currentlySelectedCar == 2)
            {
                Car1EngineSound.enabled = false;
                Car2EngineSound.enabled = false;
                Car3EngineSound.enabled = false;
                Car4EngineSound.enabled = false;
                Car5EngineSound.enabled = false;
                Car2EngineSound.enabled = true;
            }
            else if (saveManager.MainSaveFile.currentlySelectedCar == 4)
            {
                Car1EngineSound.enabled = false;
                Car2EngineSound.enabled = false;
                Car3EngineSound.enabled = false;
                Car4EngineSound.enabled = false;
                Car5EngineSound.enabled = false;
                Car3EngineSound.enabled = true;
            }
            else if (saveManager.MainSaveFile.currentlySelectedCar == 3)
            {
                Car1EngineSound.enabled = false;
                Car2EngineSound.enabled = false;
                Car3EngineSound.enabled = false;
                Car4EngineSound.enabled = false;
                Car5EngineSound.enabled = false;
                Car4EngineSound.enabled = true;
            }
            else if (saveManager.MainSaveFile.currentlySelectedCar == 1)
            {
                Car1EngineSound.enabled = false;
                Car2EngineSound.enabled = false;
                Car3EngineSound.enabled = false;
                Car4EngineSound.enabled = false;
                Car5EngineSound.enabled = false;
                Car5EngineSound.enabled = true;
            }
      //  }
    }

    private void StartGame(GameEvent theEvent)
    {
      //  if (!isSoundMuted)
      //  {
            UpdateSoundOn();
            carSurfaceEmitter.Play();
      //  }
        //if (!isSoundMuted)
        //{
        //    loopingChannelSO.RaiseEvent(gameObject, engineWithSurfaceSfxStr);
        //    isEngineRunning = true;
        //}
    }
    private void Update()
    {
      //  if (!isSoundMuted)
      //  {
            if (isEngineRunning)
            {
                if (!isPlayerEngineReady)
                {
                    surfaceEmitterObj = audioManagerSo.GetEventSound(engineWithSurfaceSfxStr);
                    SetUpLoopingSounds(surfaceEmitterObj.transform);

                    if (surfaceEmitterObj != null)
                    {
                       
                        carSurfaceEmitter = surfaceEmitterObj.GetComponent<StudioEventEmitter>();
                        if (carSurfaceEmitter) {
                            // Getting Car Engine
                            carEngineEmitter = gameObject.GetComponent<StudioEventEmitter>();
                            carEngineEmitter.SetParameter("Type", 1);
                           // if (!isSoundMuted)
                                carEngineEmitter.Play();
                           // else
                               // carEngineEmitter.Stop();
                            //carEngineSoundEmitter.SetParameter("Speed", 1);
                            carSurfaceEmitter.SetParameter(surfaceContactString, 1);
                            carSurfaceEmitter.Stop();
                        }
                        isPlayerEngineReady = true;
                    }
                }
                else
                {
                    if (isPlayerBoosting)
                    {
                        netSpeed = oldSpeed * 2f;
                    }
                    else
                    {
                        netSpeed = playerData.ForwardSpeed;// * tempSpeed;
                        oldSpeed = netSpeed;
                        if (netSpeed > 1)
                            netSpeed = 1;
                        //print("Speed = " + netSpeed);
                    }
                    //carEngineSoundEmitter.SetParameter("Speed", netSpeed);          //
                    //UnityEngine.Console.Log("Surface sound started = " + playerData.IsGrounded);
                    if (playerData.IsGrounded)
                    {
//                        carSurfaceEmitter.SetParameter(surfaceContactString, 1);
//                        carEngineEmitter.SetParameter(surfaceContactString, 1);
                    }
                    else
                    {
//                        carSurfaceEmitter.SetParameter(surfaceContactString, 0);
//                        carEngineEmitter.SetParameter(surfaceContactString, 0);
                    }
                    float num = -1.0f;
                    carSurfaceEmitter.EventInstance.getParameterByName(materialString, out num);
                }
            }
     //   }
    }

    // Update is called once per frame
    private void OnDisable()
    {
        soundSettingsChanged.OnSettingChanged -= ChangeSoundsSettings;
        pogostickHasBeenPickedUp.TheEvent.RemoveListener(PogoStickSound);
        playerJumpedEvent.TheEvent.RemoveListener(PlayJumpSFX);
        playerChangeLaneEvent.TheEvent.RemoveListener(PlayLaneChangeSFX);
        playerDodgedEvent.TheEvent.RemoveListener(PlayDodgedSFX);
        playerInstantiated.TheEvent.RemoveListener(PlayEngineSound);
        playerDuckEvent.TheEvent.RemoveListener(PlayDuckSFX);
        playerDeathEvent.TheEvent.RemoveListener(PlayerDeathSFX);
        armourUsedUp.TheEvent.RemoveListener(PlayerArmourCrashSFX);
        gameStartedEvent.TheEvent.RemoveListener(StartGame);
        springJumpEvent.TheEvent.RemoveListener(PlayDoubleJumpSFX);
        dashStartedSfxEvent.TheEvent.RemoveListener(PlayStartedDashSFX);
        dashEndedSfxEvent.TheEvent.RemoveListener(PlayerEndedDashSFX);
        boostStartedSfxEvent.TheEvent.RemoveListener(PlayStartedBoostSFX);
        boostEndedSfxEvent.TheEvent.RemoveListener(PlayerEndedBoostSFX);
        playerDamageEvent.TheEvent.RemoveListener(PlayPlayerHitSFX);
        shockwaveEvent.TheEvent.RemoveListener(PlayShockWaveSFX);
        cutSceneStartEvent.TheEvent.RemoveListener(PlayCutSceneSFX);
        onTheSideRampEvent.TheEvent.RemoveListener(PlayOnTheSideRampSFX);
        offTheSideRampEvent.TheEvent.RemoveListener(StopSideRampSFX);
        playerLandedOngroundEvent.TheEvent.RemoveListener(PlayerLandedAfterWallClimbSFX);
        rampRunEvent.TheEvent.RemoveListener(PlayRampRunSFX);
        chaserAbusingEvent.TheEvent.RemoveListener(ChaserAbusingSFXInstance);
        playerHasRevivedEvent.TheEvent.RemoveListener(PlayerHasRevived);
        playerStartedStumblingEvent.TheEvent.RemoveListener(PlayerStartedStumblingSFX);
        playerEndStumbledEvent.TheEvent.RemoveListener(PlayerEndedStumblingSFX);
        playerCutSceneStumblingEvent.TheEvent.RemoveListener(PlayerCutSceneStumble);
        magnetHasBeenPickedUp.TheEvent.RemoveListener(MagnetPowerPickupSound);
        armourHasBeenPickedUp.TheEvent.RemoveListener(ArmourPowerPickupSound);
        aeroplaneStateStarted.TheEvent.RemoveListener(PlayAeroPlaneSFX);
    }
}