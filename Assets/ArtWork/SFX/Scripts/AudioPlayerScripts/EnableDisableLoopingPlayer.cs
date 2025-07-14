using FMODUnity;
using UnityEngine;

public class EnableDisableLoopingPlayer : SFXSettings
{
    [SerializeField] private string eventStr;
    [SerializeField] private SettingsEventChannelSO soundSettingsChanged;

    private StudioEventEmitter soundEmmiterObj;

    private void OnEnable()
    {
        soundSettingsChanged.OnSettingChanged += ChangeSoundsSettings;
    }

    private void Start()
    {
        GameObject tempMusicObj = new GameObject();
        tempMusicObj.name = "FModInstanceMusicObj";
        tempMusicObj.transform.parent = gameObject.transform;
        tempMusicObj.transform.position = gameObject.transform.position;


        soundEmmiterObj = tempMusicObj.AddComponent<StudioEventEmitter>();
        soundEmmiterObj.PlayEvent = EmitterGameEvent.ObjectEnable;
        soundEmmiterObj.StopEvent = EmitterGameEvent.ObjectDisable;
        //soundEmmiterObj.Event = eventStr;
       // soundEmmiterObj.EventReference.Path = eventStr;
        FMOD.GUID guid = RuntimeManager.PathToGUID(eventStr);
        soundEmmiterObj.EventReference.Guid = guid;


       // if (IsSoundsOn())
            soundEmmiterObj.Play();
        //else
        //    soundEmmiterObj.Stop();
    }

    void ChangeSoundsSettings(bool soundChanged)
    {
      //  if (IsSoundsOn())
            soundEmmiterObj.Play();
        //else
        //    soundEmmiterObj.Stop();
    }

    private void OnDisable()
    {
        soundSettingsChanged.OnSettingChanged -= ChangeSoundsSettings;
    }
}
