using System.Linq;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(ActionTimePreserver), typeof(PreplacedCoinGenerator))]
public class SpringRampHandler : MonoBehaviour
{
    [SerializeField] private InventoryItemSO[] pickupInventoryItemList;
    [SerializeField] private GameObject pickupSpawnPoint;

    [SerializeField] private GameObject springRampPrefab;
    [SerializeField] private GameObject springRampSpawnPoint;

    [SerializeField] private bool forceSpawnSpringRamp;
    public bool ForceSpawnSpringRamp => forceSpawnSpringRamp;

    private bool isSpringRampEnabled = false;
    private GameObject pickupObj;
    private GameObject springRampObj;

    private static float lastSpawnedZPosition = -1;
    private readonly float minimumDistanceBeforeSpawn = 2 * GameManager.ChunkDistance;

    [SerializeField] private InventoryItemSO aeroplanePickupType;
    
    [SerializeField] private GamePlaySessionData gamePlaySessionData;
    [SerializeField] private PickupGenerationData pickupGenerationData;
    [SerializeField] private PickupsUtilityHelper pickupsUtility;

    private ActionTimePreserver actionTimePreserver;
    private PreplacedCoinGenerator preplacedCoinGenerator;

    private void Awake()
    {
        actionTimePreserver = GetComponent<ActionTimePreserver>();
        preplacedCoinGenerator = GetComponent<PreplacedCoinGenerator>();
    }

    public void InitializeSpringRamp()
    {
        Debug.LogError("Init............");
        float currentZPosition = gamePlaySessionData.DistanceCoveredInMeters;

        if (minimumDistanceBeforeSpawn >= currentZPosition && !forceSpawnSpringRamp)
            return;

        float distanceCoveredAfterLastPickupSpawned = currentZPosition - lastSpawnedZPosition;

        if (distanceCoveredAfterLastPickupSpawned >= minimumDistanceBeforeSpawn || lastSpawnedZPosition == -1f || forceSpawnSpringRamp)
        {
            Debug.LogError("Init1............");
            isSpringRampEnabled = true;
            lastSpawnedZPosition = gamePlaySessionData.DistanceCoveredInMeters;

            springRampObj = Instantiate(springRampPrefab, springRampSpawnPoint.transform, true);
            springRampObj.transform.localPosition = Vector3.zero;

            GameObject pickupToSpawn = GetPickupToSpawn();

            pickupObj = Instantiate(pickupToSpawn, pickupSpawnPoint.transform, true);
            pickupObj.transform.localPosition = Vector3.zero;

            actionTimePreserver.InitializeActionTimePreserver();
            preplacedCoinGenerator.GenerateCoins();
        
            pickupObj.GetComponent<SpecialPickup>().RaiseSpecialPickupSpawnedEvent();
        }
    }

    private GameObject GetPickupToSpawn()
    {
        PickupSpawnData[] pickupSpawnData = pickupGenerationData.GetPickupSpawnData.Values.ToArray();
        List<PickupSpawnData> possiblePickupsToSpawn = new();

        for (int i = 0; i < pickupSpawnData.Length; i++)
        {
            PickupSpawnData data = pickupSpawnData[i];

            if (data.GetPickupType == aeroplanePickupType)
            {
                PickupCondition condition = pickupGenerationData.PickupCurrentStateData.GetPickupCurrentStateDictionary[data.GetPickupType];

                float currentZPosition = gamePlaySessionData.DistanceCoveredInMeters;
                float distanceCoveredAfterLastPickupSpawned = currentZPosition - condition.lastSpawnedZPosition;

                if (distanceCoveredAfterLastPickupSpawned < data.GetMinimumDistanceBeforeSpawn)
                {
                    continue;
                }

                if (!pickupsUtility.isSafeToSpawn(pickupSpawnPoint.transform.position.z, aeroplanePickupType))
                {
                    continue;
                }
            }

            for (int j = 0; j < pickupInventoryItemList.Length; j++)
            {
                if (data.GetPickupType == pickupInventoryItemList[j])
                {
                    possiblePickupsToSpawn.Add(data);
                    break;
                }
            }
        }

        return possiblePickupsToSpawn[Random.Range(0, possiblePickupsToSpawn.Count)].GetPrefab;
    }

    public void ReturnSpringRamp()
    {
        if (isSpringRampEnabled)
        {
            foreach (ActionTimePreserverSubject preservedSubject in actionTimePreserver.PreservedSubjects)
            {
                preservedSubject.ResetPreserverSubjectPosition();
            }
            preplacedCoinGenerator.ReturnCoins();
            Destroy(springRampObj);
            Destroy(pickupObj);

            isSpringRampEnabled = false;
        }
    }
}
