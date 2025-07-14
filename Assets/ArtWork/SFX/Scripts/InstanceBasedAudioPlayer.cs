using System.Collections.Generic;
using UnityEngine;

public class InstanceBasedAudioPlayer : MonoBehaviour    //: BaseAudioPlayer
{

    [Tooltip("The AudioManager listens to this event, fired by objects in any scene, to play sounds")]
    [SerializeField] private List<AudioEventChannelSO> audioEventChannel = new List<AudioEventChannelSO>();
    [SerializeField] private string eventStr;
    [HideInInspector] public int selectedIndex = 0;

    //protected override void Awake()
    //{
    //    // This Awake functio is getting all banks
    //    base.Awake();
    //}

    /// <summary>
    /// These two functions are only written for Instance based Sounds
    /// </summary>
    #region Functions For Playing Instance Based Sounds
    public void PlayInstanceAudio()
    {
        // Audio Channel at 0 index would have play channel 
        audioEventChannel[0].RaiseEvent(gameObject, eventStr);
    }

    public void StopInstanceAudio()
    {
        // Audio Index at 1 index would have stop channel
        audioEventChannel[1].RaiseEvent(gameObject, eventStr);
    }
    #endregion
}
