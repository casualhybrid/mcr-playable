using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PickupsUtilityHelper", menuName = "ScriptableObjects/Pickups/PickupsUtilityHelper")]
public class PickupsUtilityHelper : ScriptableObject
{
    [Header("References")]
    [SerializeField] private EnvironmentData _environmentData;
    [SerializeField] private SpeedHandler speedHandler;
    [SerializeField] private PlayerContainedData PlayerContainedData;
    [SerializeField] private PowerUpDurationManager powerUpDurationManager;
    public bool isSafeToSpawn(float pickupSpawnPointZ, InventoryItemSO pickup, Queue<InventoryItemSO> pendingPickupsToSpawn = null) // method is only for AeroPlane Pickup
    {
        if(pickup == null)
        {
            Console.LogWarning("Checking safe to spawn for a null pickup");
            return true;
        }

        if (pickup != null && pickup.name != "AeroPlanePickup")
            return true;

        if (!_environmentData.firstRampBuildingHasBeenSpawned)
            return true;

        float durationFly = powerUpDurationManager.GetPowerDurationForCar(pickup);
        float gameTimeScaleForMaxFlySpeed = speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(PlayerContainedData.PlayerData.PlayerInformation[0].FlyingMaxSpeed);
        float distanceCoveredByPlane = (gameTimeScaleForMaxFlySpeed) * ((durationFly) / Time.fixedDeltaTime) * 1;
        float nextSwitchPathZPos = _environmentData.nextEnvSwitchPatchZPosition;
        float distanceFromNextSwitchPatch = nextSwitchPathZPos - pickupSpawnPointZ;

        if (distanceFromNextSwitchPatch < 0)
        {
            //if (pickup != null)
            //    UnityEngine.Console.Log($"Safe To Spawn {pickup.name} as distance from next patch is {distanceFromNextSwitchPatch} and flying length is {distanceCoveredByPlane}");

            return true;
        }

        if (distanceCoveredByPlane > distanceFromNextSwitchPatch)
        {
            if (pickup != null)
            {
                UnityEngine.Console.Log($"Not Safe To Spawn {pickup.name}");

                if (pendingPickupsToSpawn != null) // Only enqueues pickups in pendingPickups if the pendingPickups variables is recieved
                    pendingPickupsToSpawn.Enqueue(pickup);
            }
            return false;
        }

        return true;
    }
}
