using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AudioModule/AudioConfig")]
public class AudioInstanceConfigSO : ScriptableObject
{
    [SerializeField] private AudioInstanceConfig audioInstanceConfig;

    public AudioInstanceConfig AudioInstanceConfig => audioInstanceConfig;
}
