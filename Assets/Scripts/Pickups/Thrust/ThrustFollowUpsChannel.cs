using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ThrustFollowUpsChannel", menuName = "ScriptableObjects/Channels/ThrustFollowUpsChannel")]
public class ThrustFollowUpsChannel : ScriptableObject
{
    [HideInInspector] public Action<ThrustConfig> spawnFollowups;

    public void SpawnFollowups(ThrustConfig pickupsContainer)
    {
        spawnFollowups(pickupsContainer);
    }
}
