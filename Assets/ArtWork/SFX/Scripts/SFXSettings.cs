using FMODUnity;
using UnityEngine;
using Sirenix.OdinInspector;

public class SFXSettings : SerializedMonoBehaviour
{
    //protected bool IsSoundsOn() {
    //    if (PlayerPrefs.GetFloat("sound") == 1)
    //        return true;
    //    return false;
    //}

    //protected bool IsMusicOn() {
    //    if (PlayerPrefs.GetFloat("music") == 1)
    //        return true;
    //    return false;
    //}

    protected void SetUpLoopingSounds(Transform sfxObj)
    {
       // if (PlayerPrefs.GetFloat("sound") == 0)
         //   MuteLoopingSounds(sfxObj);
        //else
            UnMuteLoopingSounds(sfxObj);
    }

    protected void SetUpLoopingMusic(Transform sfxObj) {
      //  if (PlayerPrefs.GetFloat("music") == 0)
          //  MuteLoopingSounds(sfxObj);
       // else
            UnMuteLoopingSounds(sfxObj);
    }

    private GameObject MuteLoopingSounds(Transform eventObj)
    {
        foreach (Transform musicObj in eventObj)
        {
            StudioEventEmitter soundEmmiterObj = musicObj.gameObject.GetComponent<StudioEventEmitter>();

            FMOD.Studio.EventInstance eventInstanceObj;
            eventInstanceObj = soundEmmiterObj.EventInstance;
            eventInstanceObj.setVolume(0);
        }

        return default;
    }

    private GameObject UnMuteLoopingSounds(Transform eventObj)
    {
        foreach (Transform musicObj in eventObj)
        {
            StudioEventEmitter soundEmmiterObj = musicObj.gameObject.GetComponent<StudioEventEmitter>();

            FMOD.Studio.EventInstance eventInstanceObj;
            eventInstanceObj = soundEmmiterObj.EventInstance;
            eventInstanceObj.setVolume(1);
        }

        return default;
    }
}
