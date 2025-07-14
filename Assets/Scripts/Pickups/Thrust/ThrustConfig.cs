using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrustConfig : MonoBehaviour
{
    public PickupsContainer followUpPickupsContainer;
    [Space(15)]
    public bool shouldSpawnFollowUpPickupsInSpecifiedColumn;
    [Range(-1, 1)] public int followUpPickupSpawnColumn;
}
