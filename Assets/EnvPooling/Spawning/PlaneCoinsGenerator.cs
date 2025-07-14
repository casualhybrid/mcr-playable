using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlaneCoinsGenerator : MonoBehaviour
{
    [SerializeField] private SpeedHandler speedHandler;
    [SerializeField] private PlayerContainedData PlayerContainedData;
    [SerializeField] private PlayerSharedData playerSharedData;
    [SerializeField] private PlayerData characterData;
    [SerializeField] private CoinsArray coinArrayGameObject;
    [SerializeField] private float yPosition;
    [SerializeField] private float lengthToGenerateCoins;
    [SerializeField] private float maxLengthOfSegment;
    [SerializeField] private float minLengthForSegment;
    [SerializeField] private PowerUpDurationManager powerUpDurationManager;
    [SerializeField] private SpecialPickupsEnumSO specialPickupsEnumSO;
    private CoinsArray theCoinArray;
    private CoinsSpawnType lastCoinTypeGenerated = CoinsSpawnType.ZigZag;

    private float coveredLength;

    [Button("GenerateCoins")]
    public void GenerateCoins()
    {
        //   lastCoinTypeGenerated = CoinsSpawnType.Row;
        coveredLength = 0;

        float gameTimeScaleForMaxFlySpeed = speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(PlayerContainedData.PlayerData.PlayerInformation[0].FlyingMaxSpeed);

        float timeToReachMaxHeight = PlayerContainedData.PlayerData.PlayerInformation[0].TimeToReachFlyingTopHeight;
        float zDistanceCoveredDuringTakeOff = gameTimeScaleForMaxFlySpeed * (timeToReachMaxHeight / Time.fixedDeltaTime);

        //  lengthToGenerateCoins = (characterData.PlayerInformation[0].ForwarspeedMultiplier * characterData.PlayerInformation[0].ForwardSpeedInitialValue) * ((10f) / Time.fixedDeltaTime) * PlayerContainedData.PlayerData.PlayerInformation[0].FlyingMaxSpeed;
        lengthToGenerateCoins = gameTimeScaleForMaxFlySpeed * (powerUpDurationManager.GetPowerDurationForCar(specialPickupsEnumSO.AeroPlanePickup) / Time.fixedDeltaTime);

        lengthToGenerateCoins -= zDistanceCoveredDuringTakeOff;

        if (theCoinArray != null)
        {
            Destroy(theCoinArray.gameObject);
            theCoinArray = null;
        }

        Vector3 startPosition = theCoinArray == null ? transform.position : theCoinArray.GetLastCoinPosition();

        float startingRandXPosition;

        startingRandXPosition = GetValidXPositionForStartingCoinGeneration();
        InstantiateCoinsArray(new Vector3(startingRandXPosition, startPosition.y + coinArrayGameObject.transform.position.y, playerSharedData.PlayerTransform.position.z + zDistanceCoveredDuringTakeOff));

        StartCoroutine(GenerateCoinsRoutine());
    }

    private IEnumerator GenerateCoinsRoutine()
    {
        while (coveredLength < lengthToGenerateCoins)
        {
            int _coinGenerationChance = RandomNumGenerator.GetRandomNum(new IntRange(0, 3, 35f),
                     new IntRange(4, 5, 65f));

            // 0-3 represents row
            // 4-5 represents zigzag
            if (_coinGenerationChance <= 3 && lastCoinTypeGenerated != CoinsSpawnType.ZigZag)
            {
                // if (lastCoinTypeGenerated != CoinsSpawnType.ZigZag)
                TryGeneratingZigCoins();
            }
            else
            {
                TryGeneratingRowCoins();
            }

            // Wait a frame
            yield return null;
        }
    }

    private bool TryGeneratingRowCoins()
    {
        //   UnityEngine.Console.Log("Generate Row Coins");
        lastCoinTypeGenerated = CoinsSpawnType.Row;

        // Vector3 startPosition = theCoinArray == null ? transform.position : theCoinArray.GetLastCoinPosition();

        float startingRandXPosition;

        //if (theCoinArray == null)
        //{
        //    startingRandXPosition = GetValidXPositionForStartingCoinGeneration();
        //    InstantiateCoinsArray(new Vector3(startingRandXPosition, startPosition.y + coinArrayGameObject.transform.position.y, playerSharedData.PlayerTransform.position.z));
        //}
        //  else
        //  {
        startingRandXPosition = theCoinArray.GetLastCoinPosition().x;
        // }

        theCoinArray.lengthToGenerateCoins = GetRandomSegmentForRowCoins();
        coveredLength += theCoinArray.lengthToGenerateCoins;

        List<Vector3> coinSpawnPoints = theCoinArray.GenerateRowCoinPoints(startingRandXPosition, true);
        theCoinArray.SpawnRowCoins(coinSpawnPoints);

        return true;
    }

    private float GetValidXPositionForStartingCoinGeneration()
    {
        int rand = UnityEngine.Random.Range(0, 3);

        float x = rand == 0 ? -1 : rand == 1 ? 0 : 1;

        return x;
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
        theCoinArray.transform.position = new Vector3(position.x, yPosition, position.z);

        return theCoinArray;
    }

    private bool TryGeneratingZigCoins()
    {
        // Vector3 startPosition = theCoinArray == null ? transform.position : theCoinArray.GetLastCoinPosition();
        lastCoinTypeGenerated = CoinsSpawnType.ZigZag;

        // if (theCoinArray == null)
        //  {
        //     InstantiateCoinsArray(startPosition);
        //  }

        Vector3 startPoint = theCoinArray.GetLastCoinPosition();

        Vector3 endPoint;
        int dir;

        if (Mathf.Approximately(startPoint.x, -1))
        {
            endPoint = theCoinArray.GetZiggedPosition(1);
            dir = 1;
        }
        else if (Mathf.Approximately(startPoint.x, 1))
        {
            endPoint = theCoinArray.GetZiggedPosition(-1);
            dir = -1;
        }
        else
        {
            int rand = UnityEngine.Random.Range(0, 2);

            if (rand == 0)
            {
                endPoint = theCoinArray.GetZiggedPosition(1);
                dir = 1;
            }
            else
            {
                endPoint = theCoinArray.GetZiggedPosition(-1);
                dir = -1;
            }
        }

        coveredLength += (endPoint.z - startPoint.z);
        return DoZiggedRayCastCoinGeneration(startPoint, endPoint, dir);
    }

    private bool DoZiggedRayCastCoinGeneration(Vector3 startPoint, Vector3 endPoint, float direction)
    {
        //  UnityEngine.Console.Log("Generating ZIG COINS");

        //Vector3 dir = (endPoint - startPoint).normalized;
        //  float dist = Vector3.Distance(endPoint, startPoint);

        //   Debug.DrawRay(new Vector3(startPoint.x, obstacle.transform.position.y + rayCastYOffset, startPoint.z), dir * dist, Color.green, 10);

        float forwardSpeedForMaxFlySpeed = speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(PlayerContainedData.PlayerData.PlayerInformation[0].FlyingMaxSpeed);
        float sideWaysSpeedForMaxFlySpeed = speedHandler.GetSideWaysSpeedBasedOnSpecificGameTimeScale(PlayerContainedData.PlayerData.PlayerInformation[0].FlyingMaxSpeed);

        List<Vector3> zigSpawnPoints = theCoinArray.GenerateZigZagPoints(direction, true, forwardSpeedForMaxFlySpeed, true, sideWaysSpeedForMaxFlySpeed);
        theCoinArray.ZigTheCoins(zigSpawnPoints, null, false);

        //   lastCoinTypeGenerated = CoinsSpawnType.ZigZag;
        return true;
    }

    private float GetRandomSegmentForRowCoins()
    {
        float rand = UnityEngine.Random.Range(0f, 1f);

        float segmentLength = rand * (maxLengthOfSegment - minLengthForSegment) + minLengthForSegment;

        return segmentLength;
    }
}