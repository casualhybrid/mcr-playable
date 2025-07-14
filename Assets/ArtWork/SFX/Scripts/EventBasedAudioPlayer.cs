using UnityEngine;

public class EventBasedAudioPlayer : MonoBehaviour
{
    [SerializeField] private AudioEventChannelSO audioEventChannel = default;
    [SerializeField] private string eventStr;
    /// <summary>
    /// Both OneShot and Looping audios are handled from PlayAudio function
    /// </summary>
    public void ShootAudioEvent()
    {
      //  if (PlayerPrefs.GetFloat("sound") == 1)
            audioEventChannel.RaiseEvent(gameObject, eventStr);
    }
}
