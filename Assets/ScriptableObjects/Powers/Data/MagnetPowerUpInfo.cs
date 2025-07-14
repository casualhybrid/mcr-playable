using UnityEngine;

[CreateAssetMenu(fileName = "MagnetPowerUpInfo", menuName = "ScriptableObjects/StaticData/PowerUpsData/MagnetPowerUpData")]
public class MagnetPowerUpInfo : ScriptableObject
{
    public float DefaultDuration => GameManager.ChunkDistance / 4;  // 15 seconds
}
