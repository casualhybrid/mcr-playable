using FMODUnity;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// <c>AudioManager</c> will handle all requests from different objects to play specific audio
/// </summary>
[CreateAssetMenu(menuName = "AudioModule/Audio Manager")]
public class AudioManagerSO : ScriptableObject
{
    [Header("Listening on channels")]
    [Tooltip("The AudioManager listens to this event, fired by objects in any scene, to play OneShots")]
    [SerializeField] private AudioEventChannelSO oneShotEventChannel = default;

    [Tooltip("The AudioManager listens to this event, fired by objects in any scene, to play Music")]
    [SerializeField] private AudioEventChannelSO musicEventChannelPlay = default;

    [Tooltip("The AudioManager listens to this event, fired by objects in any scene, to stop Music")]
    [SerializeField] private AudioEventChannelSO musicEventChannelStop = default;

    [Tooltip("The AudioManager listens to this event, fired by objects in any scene, to play Instance Audios")]
    [SerializeField] private AudioEventChannelSO instanceBasedEventPlay = default;

    [Tooltip("The AudioManager listens to this event, fired by objects in any scene, to stop Instance Audios")]
    [SerializeField] private AudioEventChannelSO instanceBasedEventStop = default;

    [SerializeField] private SettingsEventChannelSO soundSettingsChanged;
    [SerializeField] private SettingsEventChannelSO musicSettingsChanged;

    public List<GameObject> loopingEvents = new List<GameObject>();
    private List<FMOD.Studio.EventInstance> eventInstances = new List<FMOD.Studio.EventInstance>();



    public void OnEnable()
    {
        SceneManager.activeSceneChanged += HandleActiveSceneChanged;
        oneShotEventChannel.OnAudioRequested += PlayOneShotAudios;
        oneShotEventChannel.OnAudioWithParameterRequested += PlayOneShotAudios;
        oneShotEventChannel.OnAudioWithConfigurationRequsted += PlayOneShotAudios;
        musicEventChannelPlay.OnAudioRequested += PlayLoopingAudios;
        musicEventChannelStop.OnAudioRequested += StopLoopingAudios;
        instanceBasedEventPlay.OnAudioRequested += PlayEventBasedAudios;
        instanceBasedEventStop.OnAudioRequested += StopEventBasedAudios;

        soundSettingsChanged.OnSettingChanged += HandleSoundVolumeChanged;
        musicSettingsChanged.OnSettingChanged += HandleMusicVolumeChanged;

        RuntimeManager.OnBanksLoaded += SetStartingVolume;

     

        SetUpStartingSounds();
    }

    private void SetStartingVolume()
    {
        HandleSoundVolumeChanged(PlayerPrefs.GetFloat("sound",0) == 1);
        HandleMusicVolumeChanged(PlayerPrefs.GetFloat("music", 0) == 1);
    }

    private void HandleActiveSceneChanged(Scene scene, Scene s)
    {
        SetUpStartingSounds();
    }

    public void SetUpStartingSounds()
    {
        loopingEvents.Clear();
    }

    private void PlayOneShotAudios(GameObject obj, string strObj)
    {
        //UnityEngine.Console.Log("Play One Shot..");
        GameObject tempShotObj = new GameObject();
        tempShotObj.name = "FModOneShotObj";
        tempShotObj.transform.parent = obj.transform;
        tempShotObj.transform.position = obj.transform.position;

        FMOD.Studio.EventInstance oneShotDec;
        FMOD.Studio.EventDescription eventLength;
        float destroyTime = 0f;
        int clipLength = 0;

        oneShotDec = RuntimeManager.CreateInstance(strObj);
        oneShotDec.getDescription(out eventLength);
        eventLength.getLength(out clipLength);

        oneShotDec.set3DAttributes(obj.transform.position.To3DAttributes());
        oneShotDec.start();
        oneShotDec.release();

        destroyTime = ((float)clipLength) / 1000f;
        Destroy(tempShotObj, destroyTime);
    }

    private void PlayOneShotAudios(GameObject obj, string strObj, string parameter, float parameterValue)
    {
        //if (PlayerPrefs.GetFloat("sound") == 0)
        //    return;

        //UnityEngine.Console.Log("Play One Shot..");
        GameObject tempShotObj = new GameObject();
        tempShotObj.name = "FModOneShotObj";
        tempShotObj.transform.parent = obj.transform;
        tempShotObj.transform.position = obj.transform.position;

        FMOD.Studio.EventInstance oneShotDec;
        FMOD.Studio.EventDescription eventLength;
        float destroyTime = 0f;
        int clipLength = 0;

        oneShotDec = RuntimeManager.CreateInstance(strObj);
        oneShotDec.getDescription(out eventLength);
        eventLength.getLength(out clipLength);

        oneShotDec.setParameterByName(parameter, parameterValue);
        oneShotDec.set3DAttributes(obj.transform.position.To3DAttributes());
        oneShotDec.start();
        oneShotDec.release();

        destroyTime = ((float)clipLength) / 1000f;
        Destroy(tempShotObj, destroyTime);
    }

    private void PlayOneShotAudios(GameObject obj, string strObj, AudioInstanceConfig audioInstanceConfig)
    {
        GameObject tempShotObj = new GameObject();
        tempShotObj.name = "FModOneShotObj";
        tempShotObj.transform.parent = obj.transform;
        tempShotObj.transform.position = obj.transform.position;

        FMOD.Studio.EventInstance oneShotDec;
        FMOD.Studio.EventDescription eventLength;
        float destroyTime = 0f;
        int clipLength = 0;

        oneShotDec = RuntimeManager.CreateInstance(strObj);
        oneShotDec.getDescription(out eventLength);
        eventLength.getLength(out clipLength);

        oneShotDec.setPitch(audioInstanceConfig.Pitch);

        if(!String.IsNullOrEmpty(audioInstanceConfig.parameterName))
        {
            oneShotDec.setParameterByName(audioInstanceConfig.parameterName, audioInstanceConfig.parameterValue);
        }

        //   oneShotDec.setParameterByName(parameter, parameterValue);
        oneShotDec.set3DAttributes(obj.transform.position.To3DAttributes());
        oneShotDec.start();
        oneShotDec.release();

        destroyTime = ((float)clipLength) / 1000f;
        Destroy(tempShotObj, destroyTime);
    }

    private void PlayLoopingAudios(GameObject obj, string strObj)
    {
        GameObject tempMusicObj = new GameObject();
        loopingEvents.Add(tempMusicObj);
        tempMusicObj.name = "FModBGMusicObj";
        tempMusicObj.transform.parent = obj.transform;
        tempMusicObj.transform.position = obj.transform.position;

        StudioEventEmitter soundEmmiterObj;
        soundEmmiterObj = tempMusicObj.AddComponent<StudioEventEmitter>();
        soundEmmiterObj.PlayEvent = EmitterGameEvent.ObjectEnable;
        soundEmmiterObj.StopEvent = EmitterGameEvent.ObjectDisable;
        //  soundEmmiterObj.Event = strObj;
        //      soundEmmiterObj.EventReference.Path = strObj;
        FMOD.GUID guid = RuntimeManager.PathToGUID(strObj);
        soundEmmiterObj.EventReference.Guid = guid;
        soundEmmiterObj.Play();

        //   loopingEvents.Add(tempMusicObj);
        //  UnityEngine.Console.Log("Playing Loop Sound " + strObj);
        //UnityEngine.Console.Log("Play Looping Sound = " + loopingEvents.Count);
    }

    private void StopLoopingAudios(GameObject obj, string strObj)
    {
        try
        {
            //     UnityEngine.Console.Log("Request To Stop ");
            FMOD.GUID passedGUID = RuntimeManager.PathToGUID(strObj);

            foreach (GameObject musicObj in loopingEvents)
            {
                StudioEventEmitter soundEmmiterObj = musicObj.GetComponent<StudioEventEmitter>();
                FMOD.GUID guid = soundEmmiterObj.EventReference.Guid;

                //   UnityEngine.Console.Log("Reached Stop");

                //string path = " ";
                //FMOD.Studio.EventDescription edState;
                //edState = soundEmmiterObj.EventDescription;
                //edState.getPath(out path);

                //  if (strObj.EndsWith(path))
                if (guid.Equals(passedGUID))
                {
                    //   UnityEngine.Console.Log("Detected");

                    if (soundEmmiterObj.IsPlaying())
                    {
                        soundEmmiterObj.Stop();
                        loopingEvents.Remove(musicObj);
                        Destroy(musicObj);
                        break;
                    }
                }
            }
        }
        catch (Exception e)
        {
            UnityEngine.Console.LogError("Exception " + e);
        }
    }

    private void PlayEventBasedAudios(GameObject obj, string strObj)
    {
       // if (PlayerPrefs.GetFloat("sound") == 0)
          //  return;

        FMOD.Studio.EventInstance tempAudioInstance;
        tempAudioInstance = RuntimeManager.CreateInstance(strObj);
        eventInstances.Add(tempAudioInstance);

        FMOD.Studio.PLAYBACK_STATE pbState;
        tempAudioInstance.getPlaybackState(out pbState);
        if (pbState != FMOD.Studio.PLAYBACK_STATE.PLAYING)
        {
            tempAudioInstance.start();
        }
    }

    private void StopEventBasedAudios(GameObject obj, string strObj)
    {
        foreach (FMOD.Studio.EventInstance instance in eventInstances)
        {
            string path = " ";
            FMOD.Studio.EventDescription edState;
            instance.getDescription(out edState);
            edState.getPath(out path);

            if (strObj.EndsWith(path))
            {
                FMOD.Studio.PLAYBACK_STATE pbState;
                instance.getPlaybackState(out pbState);
                //print("Event Description = " + path);

                if (pbState == FMOD.Studio.PLAYBACK_STATE.PLAYING)
                {
                    instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                    eventInstances.Remove(instance);
                    break;
                }
            }
        }
    }

    public GameObject GetEventSound(string eventName)
    {
        foreach (GameObject musicObj in loopingEvents)
        {
            StudioEventEmitter soundEmmiterObj = musicObj.GetComponent<StudioEventEmitter>();
            // string path = " ";
            // FMOD.Studio.EventDescription edState;
            // edState = soundEmmiterObj.EventDescription;

            //var res =  edState.getPath(out path);

            // string path = soundEmmiterObj.EventReference.Path;
            FMOD.GUID guidOriginal = RuntimeManager.PathToGUID(eventName);
            FMOD.GUID guid = soundEmmiterObj.EventReference.Guid;

            //  if (eventName.Equals(path))//if (eventName.EndsWith(path))
            if (guidOriginal.Equals(guid))
            {
                return musicObj;
            }
        }

        return default;
    }

    public GameObject MuteLoopingSounds(Transform[] eventObj)
    {
        foreach (Transform musicObj in eventObj)
        {
            StudioEventEmitter soundEmmiterObj = musicObj.GetComponent<StudioEventEmitter>();
            string path = " ";
            FMOD.Studio.EventDescription edState;
            edState = soundEmmiterObj.EventDescription;
            edState.getPath(out path);

            soundEmmiterObj.EventInstance.setVolume(0);
        }

        return default;
    }

    public void OnDisable()
    {
        SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
        oneShotEventChannel.OnAudioRequested -= PlayOneShotAudios;
        oneShotEventChannel.OnAudioWithParameterRequested -= PlayOneShotAudios;
        oneShotEventChannel.OnAudioWithConfigurationRequsted -= PlayOneShotAudios;
        musicEventChannelPlay.OnAudioRequested -= PlayLoopingAudios;
        musicEventChannelStop.OnAudioRequested -= StopLoopingAudios;
        instanceBasedEventPlay.OnAudioRequested -= PlayEventBasedAudios;
        instanceBasedEventStop.OnAudioRequested -= StopEventBasedAudios;

        soundSettingsChanged.OnSettingChanged -= HandleSoundVolumeChanged;
        musicSettingsChanged.OnSettingChanged -= HandleMusicVolumeChanged;

        RuntimeManager.OnBanksLoaded -= SetStartingVolume;


    }

    private void HandleSoundVolumeChanged(bool status)
    {
        RuntimeManager.GetVCA("vca:/SFX").setVolume(status ? 1 : 0);
    }

    private void HandleMusicVolumeChanged(bool status)
    {
        RuntimeManager.GetVCA("vca:/MUSIC").setVolume(status ? 1 : 0);
    }
}