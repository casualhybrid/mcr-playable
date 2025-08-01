using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundForDailyGoals : MonoBehaviour
{
    private void OnEnable()
    {
        PersistentAudioPlayer.Instance.PanelSounds();
    }
    private void OnDisable()
    {
        PersistentAudioPlayer.Instance.PlayAudio();
    }
}
