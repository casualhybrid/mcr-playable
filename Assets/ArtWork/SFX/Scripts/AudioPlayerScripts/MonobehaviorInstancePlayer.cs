using FMODUnity;
using UnityEngine;

public class MonobehaviorInstancePlayer : SFXSettings
{
    [SerializeField] private string eventStr;

    private FMOD.Studio.EventInstance tempAudioInstance;
    private FMOD.Studio.PLAYBACK_STATE pbState;

    private void OnEnable()
    {
      //  if (IsSoundsOn())
       // {
            tempAudioInstance = RuntimeManager.CreateInstance(eventStr);
            tempAudioInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject.transform.position));
            tempAudioInstance.getPlaybackState(out pbState);
            if (pbState != FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                tempAudioInstance.start();
            }
        //}
    }

    private void OnDisable()
    {

      //  if (IsSoundsOn())
      //  {
            tempAudioInstance.getPlaybackState(out pbState);
            if (pbState == FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                tempAudioInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                tempAudioInstance.release();
            }
       // }
    }
}
