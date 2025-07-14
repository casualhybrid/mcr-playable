using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessagePack;

[System.Serializable]

public abstract class SerialzableRemoteAdConfig : ScriptableObject
{
    public bool Status;

    public abstract void ResetData();

}
