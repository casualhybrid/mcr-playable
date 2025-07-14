using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PogoStickProceduralCoinsGenerator : MonoBehaviour
{
    [SerializeField] private PlayerData characterData;
    [SerializeField] private PlayerSharedData playerSharedData;
    [SerializeField] private CoinsArray coinArrayGameObject;
    [SerializeField] private PickupsUtilityHelper pickupsUtilityHelper;
    [Range(0, 1)] [SerializeField] private float areaPercentToSpawnCoins;
    [SerializeField] private ThrustFollowUpsChannel thrustFollowUpsChannel;

    private CoinsArray theCoinArray;

    private void OnEnable()
    {
        thrustFollowUpsChannel.spawnFollowups += GenerateCoins;
    }

    private void OnDisable()
    {
        thrustFollowUpsChannel.spawnFollowups -= GenerateCoins;
    }

    public void GenerateCoins(ThrustConfig thrustConfig)
    {
        theCoinArray = InstantiateCoinsArray(playerSharedData.PlayerTransform.position);
        InventoryItemSO inventoryItemSO = null;
        int pickupColumn = 0;
        
        if (thrustConfig == null)
        {
            UnityEngine.Console.LogError("ThrustConfig is null!");
            return;
        }

        if (thrustConfig.shouldSpawnFollowUpPickupsInSpecifiedColumn)
        {
            pickupColumn = thrustConfig.followUpPickupSpawnColumn;
        }
        else
        {
            pickupColumn = UnityEngine.Random.Range(-1, 2);
        }

        for (int i = -1; i < 2; i++)
        {
            if (i == pickupColumn)
            {
                if (thrustConfig.followUpPickupsContainer.possiblePickups.Count > 0)
                {
                    bool isPossiblePickupAssigned = false;
                    List<InventoryItemSO> possiblePickupsInCurrentSituation = new List<InventoryItemSO>(thrustConfig.followUpPickupsContainer.possiblePickups);

                    while(possiblePickupsInCurrentSituation.Count > 0 && !isPossiblePickupAssigned)
                    {
                        int randPickupIndex = UnityEngine.Random.Range(0, possiblePickupsInCurrentSituation.Count);

                        if (pickupsUtilityHelper.isSafeToSpawn(theCoinArray.GetLastCoinPosition().z, possiblePickupsInCurrentSituation[randPickupIndex]))
                        {
                            inventoryItemSO = possiblePickupsInCurrentSituation[randPickupIndex];
                            isPossiblePickupAssigned = true;
                        }
                        else
                        {
                            possiblePickupsInCurrentSituation.RemoveAt(randPickupIndex);
                        }
                    }
                }
            }

            List<Vector3> coinSpawnPoints = theCoinArray.GenerateCurvedCoinPoints(characterData.PlayerInformation[0].ThurstJumpHeight, characterData.PlayerInformation[0].ThurstDuration, areaPercentToSpawnCoins, true, new Vector3(i, playerSharedData.PlayerTransform.position.y, playerSharedData.PlayerTransform.position.z));
            theCoinArray.SpawnCurvedCoins(coinSpawnPoints , inventoryItemSO, PickupItemPlacement.End, false);

            inventoryItemSO = null;
        }
    }

    private CoinsArray InstantiateCoinsArray(Vector3 position)
    {
        if (Application.isPlaying)
        {
            theCoinArray = Instantiate(coinArrayGameObject, position, coinArrayGameObject.transform.rotation);
        }
        else
        {
#if UNITY_EDITOR
            theCoinArray = PrefabUtility.InstantiatePrefab(coinArrayGameObject) as CoinsArray;
#endif
        }

        theCoinArray.transform.SetParent(this.transform);
        theCoinArray.transform.position = new Vector3(position.x, position.y, position.z);

        return theCoinArray;
    }
}