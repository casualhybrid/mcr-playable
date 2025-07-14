using System;
using FMODUnity;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "AudioModule/GameSoundsData")]
public class GameSoundsData : ScriptableObject {
    public List<FModSoundObj> gameSoundsDataObj;
}

[Serializable]
public class FModSoundObj {
    [ReadOnly] public string sfxName;
    [HideLabel, LabelWidth(50)] [HorizontalGroup]  public FMODUnity.EventReference sfxPath;
    [HideLabel, LabelWidth(150)] [HorizontalGroup] public GameEvent sfxChannel;
}