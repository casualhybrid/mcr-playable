using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ProceduralCoinGenerator : MonoBehaviour, IFloatingReset
{
    private struct RowResult
    {
        public readonly bool isNotValidForGen;
        public float length;
        public readonly Transform obstacleTransform;
        public readonly float startingZPos;
        public bool isAdjacentLaneBlocked;

        public RowResult(float _length, Transform _obstanceTransform, float _startingZPos, bool _isNotValidForGen = false)
        {
            length = _length;
            obstacleTransform = _obstanceTransform;
            startingZPos = _startingZPos;
            isAdjacentLaneBlocked = false;
            isNotValidForGen = _isNotValidForGen;
        }

        public RowResult(bool _isNotValidForGen, Transform colliderT)
        {
            isNotValidForGen = _isNotValidForGen;
            length = 0;
            obstacleTransform = colliderT;
            startingZPos = 0;
            isAdjacentLaneBlocked = false;
        }
    }

    public bool pauseProceduralPickupGeneration { get; set; }

    public bool ShoudNotOffsetOnRest { get; set; } = true;

    [SerializeField] private PlayerSharedData sharedData;

    [SerializeField] private float minDistanceBeforeAPickupCanBeSpawnedAgain;
    [SerializeField] private float minimumPossibleLengthForGeneration;
    [SerializeField] private float bufferLength;
    [SerializeField] private float minimumDistanceBetweenBatchOfCoins;
    [SerializeField] private float maximumDistanceBetweenBatchOfCoins;
    [SerializeField] private float minChunksBeforeMultipleCoinRowsCanAppear;
    [SerializeField] private float minChunksBeforeMultiplePoweUpsCanAppear;
    [SerializeField] private float samplingLengthToCheckForCoinGeneration;
    [SerializeField] private int maximumCoinsInABatchSpawn = 5;
    [SerializeField] private CoinsArray coinArrayGameObject;
    [SerializeField] private GameEvent obstaclesHasBeenSpawned;
    [SerializeField] private float rayCastYOffset;
    [SerializeField] private Vector3 sizeOfZigPointEndedSafeArea = new Vector3(.5f, 1f, .5f);
    [SerializeField] private Vector3 sizeOfCurvedEndedSafeArea = new Vector3(.5f, 1f, .5f);
    [SerializeField] private float lengthOfSafeAreaToCheckBehindObstacles = .5f;

    [SerializeField] private CoinGenerationData coinGenerationData;
    [SerializeField] private PlayerContainedData PlayerContainedData;
    [SerializeField] private PlayerSharedData playerSharedData;
    [SerializeField] private PlayerData characterData;
    [SerializeField] private CharactersDataBase charactersDataBase;
    [SerializeField] private EnvironmentData _environmentData;
    [SerializeField] private SpeedHandler speedHandler;
    [SerializeField] private PickupsUtilityHelper pickupsUtilityHelper;
    [SerializeField] private SpecialPickupsEnumSO specialPickupsEnumSO;
    [SerializeField] private GamePlaySessionData gamePlaySessionData;
    [SerializeField] private GamePlaySessionInventory gamePlaySessionInventory;
    [SerializeField] private InventorySystem inventorySystem;
  

    [Range(0, 10)]
    [SerializeField] private float zOffsetToStartRowGen;

    private CoinsSpawnType lastCoinTypeGenerated = CoinsSpawnType.ZigZag;
    private CoinsSpawnType pickupGenMethod;

    //   private List<Vector3> tempCenter = new List<Vector3>();

    private ObstaclesSpawning ObstaclesSpawning;

    [SerializeField] private PickupGenerationData pickupGenerationData;

    private Queue<InventoryItemSO> pendingPickupsToSpawn = new Queue<InventoryItemSO>();

    private bool isDoubleCoins = false;
    private bool isSpawnedPickupThisBatch = false;
    private float doubleCoinsLastSpawnPointZ = 0f;
    private readonly float doubleCoinsMinDistBeforeSpawn = 2 * GameManager.ChunkDistance;

    private float minDistanceBeforeMultipleCoinRowsCanAppear => GameManager.ChunkDistance * minChunksBeforeMultipleCoinRowsCanAppear;
    private float minDistanceBeforeMultiplePowerUpsCanAppear => GameManager.ChunkDistance * minChunksBeforeMultiplePoweUpsCanAppear;

    [SerializeField] private InventoryItemSO aeroplanePickupType;

    private float lastZCoinPosition = -1f;
    private float lastZDistanceMultipleRowCoinsSpawned;
    private float lastZDistanceMultiplePowerUpsSpawned;

    private void Awake()
    {
        ObstaclesSpawning = FindObjectOfType<ObstaclesSpawning>();
        // obstacle = GetComponent<Obstacle>();
        pickupGenerationData.PickupCurrentStateData.InitializePickupCurrentStateDictionary();
        SubscribeToEvent();
    }


    private void OnDestroy()
    {
        DeSubscribeToEvents();
    }

    private void SubscribeToEvent()
    {
        ObstaclesSpawning.OnSafeAreaCoinSpawnStartEndSet += HandleSafeAreaCoinStartEndSet;
        obstaclesHasBeenSpawned.TheEvent.AddListener(ObstaclesHasBeenSpawned);
    }

    private void DeSubscribeToEvents()
    {
        ObstaclesSpawning.OnSafeAreaCoinSpawnStartEndSet -= HandleSafeAreaCoinStartEndSet;
        obstaclesHasBeenSpawned.TheEvent.RemoveListener(ObstaclesHasBeenSpawned);
    }

    private void HandleSafeAreaCoinStartEndSet()
    {
        StartCoroutine(GeneralProceduralCoinsAfterFixedUpdate(false, true));
    }

    private void HandleCoinsStartEndPointsSet()
    {
        StartCoroutine(GeneralProceduralCoinsAfterFixedUpdate(false));
    }

    private void ObstaclesHasBeenSpawned(GameEvent gameEvent)
    {
        StartCoroutine(GeneralProceduralCoinsAfterFixedUpdate());
    }

    private IEnumerator GeneralProceduralCoinsAfterFixedUpdate(bool waitForFixedUpdate = true, bool justSpawnMultiRowCoins = false)
    {
        float zDistanceLastObstacle = ObstaclesSpawning.coinSpawnEndPoint;
        float origin = ObstaclesSpawning.coinSpawnStartPoint;

        float diffFromLastCoinSpawned = ObstaclesSpawning.coinSpawnStartPoint - lastZCoinPosition;

        if (diffFromLastCoinSpawned < minimumDistanceBetweenBatchOfCoins && lastZCoinPosition != -1f)
        {
            float offsetOrigin = minimumDistanceBetweenBatchOfCoins - diffFromLastCoinSpawned;

            offsetOrigin = origin + offsetOrigin >= ObstaclesSpawning.coinSpawnEndPoint ? 0 : offsetOrigin;

            origin = origin + offsetOrigin;

            // UnityEngine.Console.LogError($"Added Offset {offsetOrigin} and now the origin is {origin} and LastZCoinPosition Is {lastZCoinPosition}");
        }

        Debug.DrawRay(new Vector3(0, 0, origin), Vector3.up * 100f, Color.white, 5);
        Debug.DrawRay(new Vector3(0, 0, ObstaclesSpawning.coinSpawnEndPoint), Vector3.up * 100f, Color.red, 5);

        CoinsArray theCoinArray = InstantiateCoinsArray(new Vector3(0, 0, origin));

        if (waitForFixedUpdate)
        {
            yield return new WaitForFixedUpdate();
        }

        GenerateProceduralCoins(origin, theCoinArray, zDistanceLastObstacle, justSpawnMultiRowCoins);

        //  Debug.Break();
    }

    [Button("GenerateCoins")]
    public void GenerateProceduralCoins(float origin, CoinsArray theCoinArray, float zDistanceWhereLastObstacleEnded, bool justSpawnMultiRowCoins = false)
    {
        GenerateProceduralCoinsRoutine(origin, theCoinArray, zDistanceWhereLastObstacleEnded, justSpawnMultiRowCoins);
    }

    private void GenerateProceduralCoinsRoutine(float origin, CoinsArray theCoinArray, float zDistanceWhereLastObstacleEnded, bool justSpawnMultiRowCoins = false)
    {
        RandomizeNextPickupSpawnWay();

        isSpawnedPickupThisBatch = false;
        int doubleChannce = UnityEngine.Random.Range(0, 8);
        float playerZ = playerSharedData.PlayerTransform.position.z;

        isDoubleCoins = doubleChannce == 3 && playerZ - doubleCoinsLastSpawnPointZ > doubleCoinsMinDistBeforeSpawn;
        doubleCoinsLastSpawnPointZ = isDoubleCoins ? playerZ : doubleCoinsLastSpawnPointZ;

        //  UnityEngine.Console.Log("***********SPAWN COINS STARTED***************");

        // Safe Limit so loop doesn't get stuck
        const int maxLoopsIterations = 15;

        int k = 0;

        while (k < maxLoopsIterations)
        {
            k++;

            if (k == maxLoopsIterations)
            {
                UnityEngine.Console.LogWarning("Generate coins loop iterations exceeded the maximum limit set. Terminating");
            }

            // Reset
            lastCoinTypeGenerated = CoinsSpawnType.ZigZag;

            Debug.DrawRay(new Vector3(0, 0, origin), Vector3.up * 100f, Color.yellow, 5);

            if (justSpawnMultiRowCoins)
            {
                int refInt = 0;
                TryGeneratingRowCoins(origin, theCoinArray, zDistanceWhereLastObstacleEnded, ref refInt, true);
                theCoinArray.DissolveCoinBuffer();
                theCoinArray.IsInitialized = true;
                break;
            }

            /*bool success = */
            SpawnBatchOfCoins(origin, theCoinArray, zDistanceWhereLastObstacleEnded);

            float lastCoinPosition = theCoinArray.GetLastCoinPosition(true).z;
            float randBetween01 = UnityEngine.Random.Range(0f, 1f);
            float distanceBetweenBatch = Mathf.Lerp(minimumDistanceBetweenBatchOfCoins, maximumDistanceBetweenBatchOfCoins, randBetween01);
            float nextBatchZPosition = (theCoinArray.GetNumberOfCoinsSpawned(true) == 0 ? origin + samplingLengthToCheckForCoinGeneration : lastCoinPosition + distanceBetweenBatch);

            theCoinArray.DissolveCoinBuffer();
            theCoinArray.IsInitialized = true;

            if (nextBatchZPosition >= zDistanceWhereLastObstacleEnded)
            {
                break;
            }

            theCoinArray = InstantiateCoinsArray(new Vector3(0, 0, nextBatchZPosition));
            origin = nextBatchZPosition;
        }

        //  UnityEngine.Console.Log($"***********SPAWN COINS FINISHED {k} ***************");
    }

    private bool SpawnBatchOfCoins(float origin, CoinsArray theCoinArray, float zDistanceWhereLastObstacleEnded)
    {
        bool success;
        int coinsInBatchSpawned = 0;
        int maxRowCoinsSpawned = 0;

        lastCoinTypeGenerated = CoinsSpawnType.ZigZag;

        // Safe Limit so loop doesn't get stuck
        const int maxLoopsIterations = 10;

        int k = 0;

        while (k < maxLoopsIterations)
        {
            k++;

            if (k == maxLoopsIterations)
            {
                UnityEngine.Console.LogWarning("Spawn batch of coins loop iterations exceeded the maximum limit set. Terminating");
            }

            if (lastCoinTypeGenerated == CoinsSpawnType.Row)
            {
                success = TryGeneratingZigCoins(origin, theCoinArray, zDistanceWhereLastObstacleEnded, ref coinsInBatchSpawned);

                maxRowCoinsSpawned = success ? 0 : maxRowCoinsSpawned;
            }
            else
            {
                if (lastCoinTypeGenerated == CoinsSpawnType.ZigZag)
                {
                    int rand = UnityEngine.Random.Range(0, 4);

                    if (rand == 2)
                    {
                        if (theCoinArray.GetNumberOfCoinsSpawned(true) > 0)
                        {
                            if (IsItSafeToSpawnSpecificNoOfRowCoinsInSpecificLane(1, theCoinArray, zDistanceWhereLastObstacleEnded, ref coinsInBatchSpawned))
                            {
                                success = TryGeneratingZigCoins(origin, theCoinArray, zDistanceWhereLastObstacleEnded, ref coinsInBatchSpawned);
                                maxRowCoinsSpawned = success ? 0 : maxRowCoinsSpawned;
                            }
                            else
                            {
                                success = false;
                            }
                        }
                        else
                        {
                            success = false;
                        }
                    }
                }

                int coinsBeforeRowSpawned = coinsInBatchSpawned;
                success = TryGeneratingRowCoins(origin, theCoinArray, zDistanceWhereLastObstacleEnded, ref coinsInBatchSpawned);
                maxRowCoinsSpawned = Mathf.Max(coinsInBatchSpawned - coinsBeforeRowSpawned, maxRowCoinsSpawned);
            }

            bool exitOut = UnityEngine.Random.Range(0, 3) == 2 ? false : maxRowCoinsSpawned >= 5;

            if (exitOut || !success)
                break;
        }

        // If the last spawned were zig then remove it so it won't end in a zig
        if (lastCoinTypeGenerated == CoinsSpawnType.ZigZag && theCoinArray.GetNumberOfCoinsSpawned(true) > 0)
        {
            theCoinArray.RemoveLastZigFromBuffer();
        }

        //   UnityEngine.Console.Log($"ALL COINS FINISHED ****** {k}");

        lastZCoinPosition = theCoinArray.GetLastCoinPosition(true).z;

        return coinsInBatchSpawned > 0;
    }

    private bool TryGeneratingZigCoins(float origin, CoinsArray theCoinArray, float zDistanceWhereLastObstacleEnded, ref int coinsSpawnedInBatch)
    {
        //   Vector3 startPosition = theCoinArray == null ? new Vector3(0, 0, origin) : theCoinArray.GetLastCoinPosition(true);

        Vector3 startPoint = theCoinArray.GetLastCoinPosition(true);

        bool isInDontZigZone = CheckForSpecificObstacleIsInZone("DontGenZigZag", (startPoint + new Vector3(0, 0, sizeOfZigPointEndedSafeArea.z / 2.2f)), sizeOfZigPointEndedSafeArea);

        //  Debug.DrawRay(startPoint, Vector3.up * 100f, Color.white, 50f);

        if (isInDontZigZone)
        {
            UnityEngine.Console.Log("Not Generating ZigCoins as in NoZigZone");
            return false;
        }

        Vector3 endPoint;
        int dir;

        if (Mathf.Approximately(startPoint.x, -1))
        {
            endPoint = theCoinArray.GetZiggedPosition(1, true);
            dir = 1;

            if (isZigEndingNearObstacle(endPoint, false))
                return false;
        }
        else if (Mathf.Approximately(startPoint.x, 1))
        {
            endPoint = theCoinArray.GetZiggedPosition(-1, true);
            dir = -1;

            if (isZigEndingNearObstacle(endPoint, false))
                return false;
        }
        else
        {
            Vector3 endPointL = theCoinArray.GetZiggedPosition(-1, true);
            Vector3 endPointR = theCoinArray.GetZiggedPosition(1, true);

            bool isEndPointLNearObstacle = isZigEndingNearObstacle(endPointL, true);
            bool isEndPointRNearObstacle = isZigEndingNearObstacle(endPointR, true);

            if (isEndPointLNearObstacle && isEndPointRNearObstacle)
                return false;

            int rand = !isEndPointLNearObstacle && !isEndPointRNearObstacle ? UnityEngine.Random.Range(0, 2) : !isEndPointLNearObstacle ? 1 : 0;

            if (rand == 0)
            {
                endPoint = endPointR;
                dir = 1;
            }
            else
            {
                endPoint = endPointL;
                dir = -1;
            }
        }

        bool success = DoZiggedRayCastCoinGeneration(startPoint, endPoint, dir, theCoinArray, zDistanceWhereLastObstacleEnded);

        coinsSpawnedInBatch = success ? coinsSpawnedInBatch + 2 : coinsSpawnedInBatch;

        return success;
    }

    private bool isZigEndingNearObstacle(Vector3 endPoint, bool isMidLane)
    {
        if (isMidLane)
        {
            //Debug.DrawRay(endPoint, Vector3.up * 1000, Color.red, 10);

            Vector3 sizeOfZigPointEndedSafeAreaExtent = sizeOfZigPointEndedSafeArea;
            sizeOfZigPointEndedSafeAreaExtent.z += 5;

            bool isInDontZigThisLaneZone = CheckForSpecificObstacleIsInZone("DontGenZigZagOnThisLane", (endPoint + new Vector3(0, 0, sizeOfZigPointEndedSafeArea.z / 2f)), sizeOfZigPointEndedSafeAreaExtent);

            if (isInDontZigThisLaneZone)
                return /*!*/true;
        }

        bool isEndingNearObstacle = CheckIfZoneIsObstructed((endPoint + new Vector3(0, 0, sizeOfZigPointEndedSafeArea.z /*/ 2f*/)), sizeOfZigPointEndedSafeArea);

        if (isEndingNearObstacle)
            return true;

        // Check if its ending in blockage (Like the truck)
        Vector3 rayCastOrigin = endPoint;
        rayCastOrigin.y = rayCastYOffset;

        Ray ray = new Ray(rayCastOrigin, Vector3.forward);
        var collider = GetRayCastHitForObstacle(ray, sizeOfZigPointEndedSafeArea.z * 3f);

        if (GetRayCastHitForObstacle(ray, sizeOfZigPointEndedSafeArea.z * 3f))
        {
            CustomTag customTag = collider.GetComponent<CustomTag>();

            if (customTag != null && !customTag.HasTag("JumpableObject"))
                return true;
        }
        //else
        //{
        //    Debug.DrawRay(rayCastOrigin, Vector3.forward * (sizeOfZigPointEndedSafeArea.z * 3f), Color.black, 5);
        //}

        return /*!*/isEndingNearObstacle;
    }

    private Collider GetRayCastHitForObstacle(Ray ray, float length, string tagOfObstacle = null)
    {
        int layerMask = 1 << LayerMask.NameToLayer("Obstacles");
        RaycastHit hit;
        Physics.Raycast(ray, out hit, length, layerMask, QueryTriggerInteraction.Collide);

        if (tagOfObstacle == null)
            return hit.collider;

        if (hit.collider != null)
        {
            CustomTag customTag = hit.collider.GetComponent<CustomTag>();

            if (customTag != null && customTag.HasTag(tagOfObstacle))
                return hit.collider;
        }

        return null;
    }

    private bool CheckForSpecificObstacleIsInZone(string tagOfObject, Vector3 center, Vector3 halfExtents, bool checkForhasTag = true)
    {
        Collider[] colliders = new Collider[1];

        // tempCenter.Add(center);
        Physics.OverlapBoxNonAlloc(center, halfExtents, colliders, Quaternion.identity, 1 << LayerMask.NameToLayer("Obstacles"));

        Collider collider = colliders[0];

        if (collider == null)
            return false;

        CustomTag customTag = collider.GetComponent<CustomTag>();

        if (checkForhasTag)
        {
            return collider != null && customTag != null && customTag.HasTag(tagOfObject);
        }
        else
        {
            return collider != null && customTag != null && !customTag.HasTag(tagOfObject);
        }
    }

    private bool CheckIfZoneIsObstructed(Vector3 center, Vector3 halfExtents)
    {
        Collider[] colliders = new Collider[1];

        // tempCenter.Add(center);
        Physics.OverlapBoxNonAlloc(center, halfExtents, colliders, Quaternion.identity, 1 << LayerMask.NameToLayer("Obstacles") | 1 << LayerMask.NameToLayer("Pickups"));

        Collider collider = colliders[0];

        if (collider == null)
            return false;

        return collider != null;
    }

    //private void OnDrawGizmos()
    //{
    //    foreach (var item in tempCenter)
    //    {
    //        Gizmos.DrawCube(item, sizeOfZigPointEndedSafeArea * 2f);
    //    }
    //}

    private bool DoZiggedRayCastCoinGeneration(Vector3 startPoint, Vector3 endPoint, float direction, CoinsArray theCoinArray, float zDistanceWhereLastObstacleEnded)
    {
        //   UnityEngine.Console.Log("Generating ZIG COINS");

        Vector3 dir = (endPoint - startPoint).normalized;
        float dist = Vector3.Distance(endPoint, startPoint);

        Ray ray = new Ray(new Vector3(startPoint.x, 0 + rayCastYOffset, startPoint.z), dir);
        int layerMask = 1 << LayerMask.NameToLayer("Obstacles") | 1 << LayerMask.NameToLayer("Walkable") | 1 << LayerMask.NameToLayer("Pickups");

        // if (Physics.Raycast(ray, dist, layerMask, QueryTriggerInteraction.Collide))
        if (Physics.SphereCast(ray, 0.2f, dist, layerMask, QueryTriggerInteraction.Collide))
        {
            //  Debug.DrawRay(new Vector3(startPoint.x, 0 + rayCastYOffset, startPoint.z), dir * dist, Color.red, 10);
            return false;
        }
        else
        {
            //   float emptySpaceTillNextObstacle = FindObjectOfType<ObstaclesSpawning>().GetDistanceToNextObstacle(obstacle);
            //   float totalLength = (obstacle.GetObstacleLength + emptySpaceTillNextObstacle) - bufferLength;
            //   float rayLength = ObstaclesSpawning.GetLengthOfLastBatchSpawned - bufferLength;
            //  float obstacleEndPointZ = obstacle.transform.position.z + totalLength;
            //   float obstacleEndPointZ = ObstaclesSpawning.originOfLastBathSpawned + ObstaclesSpawning.GetLengthOfLastBatchSpawned;

            float obstacleEndPointZ = zDistanceWhereLastObstacleEnded;
            //  UnityEngine.Console.Log("EndZiggedPoint " + endPoint.z + " and obstalceEndPointZ " + obstacleEndPointZ);

            RaycastHit[] raycastHits = new RaycastHit[1];

            // Physics.OverlapBox()

            if (endPoint.z >= obstacleEndPointZ)
            {
                //  UnityEngine.Console.Log("Exceeded Zigged Returning False");
                return false;
            }

            Debug.DrawRay(new Vector3(startPoint.x, 0 + rayCastYOffset, startPoint.z), dir * dist, Color.green, 10);

            InventoryItemSO pickup = GetPickupForCurrentGenMethod(CoinsSpawnType.ZigZag, theCoinArray);

            List<Vector3> zigSpawnPoints = theCoinArray.GenerateZigZagPoints(direction, false, -1, false, -1, true);
            theCoinArray.coinsArraySpawnBuffer.Add(new CoinsSpawnInfo(CoinsSpawnType.ZigZag, zigSpawnPoints, pickup, isDoubleCoins));

            lastCoinTypeGenerated = CoinsSpawnType.ZigZag;
            return true;
        }
    }

    private bool TryGeneratingRowCoins(float origin, CoinsArray theCoinArray, float zDistanceWhereLastObstacleEnded, ref int coinsSpawnedInBatch, bool isOverrideMultiCoinChecks = false)
    {
        //   UnityEngine.Console.Log("Generate Row Coins");

        Vector3 startPosition = theCoinArray == null ? new Vector3(0, 0, origin) : theCoinArray.GetLastCoinPosition(true);

        startPosition.z += zOffsetToStartRowGen;
        startPosition.x = Mathf.Round(startPosition.x);

        Dictionary<float, RowResult> allXPoints = GetXPositionPointsForStartingCoinGeneration(startPosition, zDistanceWhereLastObstacleEnded);
        Dictionary<float, RowResult> validXPoints = new Dictionary<float, RowResult>();

        foreach (var item in allXPoints)
        {
            if (item.Value.isNotValidForGen)
                continue;

            validXPoints.Add(item.Key, item.Value);
        }

        if (validXPoints == null || validXPoints.Count == 0)
        {
            //  UnityEngine.Console.LogWarning("No or null valid XPoints for procedural coin generation");
            return false;
        }

        float startingRandXPosition;
        float rowLength;
        KeyValuePair<float, RowResult> keyValuePair;
        bool isContinuingCoinSpawn = theCoinArray.GetNumberOfCoinsSpawned(true) > 0;

        if (validXPoints.ContainsKey(startPosition.x) && isContinuingCoinSpawn)
        {
            // UnityEngine.Console.Log("Continuing Generation Row");

            startingRandXPosition = startPosition.x;
            rowLength = validXPoints[startPosition.x].length;
            keyValuePair = new KeyValuePair<float, RowResult>(startingRandXPosition, validXPoints[startPosition.x]);
        }
        else
        {
            // Chose the nearByLane
            if (isContinuingCoinSpawn)
            {
                return false;

                //UnityEngine.Console.Log("Choosing NearBy Lane For Row Generation");

                //var closestRows = validXPoints.Keys.Where((x) => { return Mathf.Approximately(Mathf.Abs(startPosition.x - x), 1); });

                //if (closestRows == null || closestRows.Count() == 0)
                //{
                //    UnityEngine.Console.Log("No nearby lanes found for row generation");
                //    return false;
                //}

                //UnityEngine.Console.Log("Picked the closest Row");

                //float key = closestRows.First();
                //RowResult value = validXPoints[key];
                //keyValuePair = new KeyValuePair<float, RowResult>(key, value);
            }
            // Randomizing
            else
            {
                // Check if 2 more lanes are availalbe for coin spawn

                float elapsedLastTimeMultipleCoinSpawned = playerSharedData.PlayerTransform.position.z - lastZDistanceMultipleRowCoinsSpawned;

                if (elapsedLastTimeMultipleCoinSpawned >= minDistanceBeforeMultipleCoinRowsCanAppear || isOverrideMultiCoinChecks)
                {
                    float minLengthForMultipleRowGen = theCoinArray.GetLengthRequiredForGivenCoins(5);

                    var rowsWithMultipleCoinGeneration = validXPoints.Where((x) => x.Value.length >= minLengthForMultipleRowGen);

                    if (rowsWithMultipleCoinGeneration.Count() >= 2)
                    {
                        // float startingZPos = theCoinArray.GetLastCoinPosition(true).z;
                        float minLengthRow = rowsWithMultipleCoinGeneration.Min((x) => x.Value.length);

                        // Check if should do multiple pickup spawning

                        float elapsedLastTimeMultiplePowerupsSpawned = playerSharedData.PlayerTransform.position.z - lastZDistanceMultiplePowerUpsSpawned;

                        // InventoryItemSO[] powerupsToSpawn = new InventoryItemSO[rowsWithMultipleCoinGeneration.Count()];
                        List<InventoryItemSO> powerupsToSpawn = new List<InventoryItemSO>();

                        if (elapsedLastTimeMultiplePowerupsSpawned >= minDistanceBeforeMultiplePowerUpsCanAppear)
                        {
                            for (int i = 0; i < rowsWithMultipleCoinGeneration.Count(); i++)
                            {
                                var pickupToSpawn = GetPickupIrrespectiveOfCurrentGenMethod(theCoinArray, powerupsToSpawn, true);
                                powerupsToSpawn.Add(pickupToSpawn);

                                UnityEngine.Console.Log($"Pickup Chosen For Multiple power {powerupsToSpawn[i]?.name}");
                            }

                            lastZDistanceMultiplePowerUpsSpawned = playerSharedData.PlayerTransform.position.z;
                        }

                        int k = 0;

                        foreach (var item in rowsWithMultipleCoinGeneration)
                        {
                            theCoinArray.lengthToGenerateCoins = Mathf.Clamp(minLengthForMultipleRowGen + (minLengthForMultipleRowGen * .25f), minLengthForMultipleRowGen, minLengthRow);

                            List<Vector3> coinSpawnPoints = theCoinArray.GenerateRowCoinPoints(item.Key, true, zOffsetToStartRowGen, -1, true, false, item.Value.startingZPos);

                            InventoryItemSO thePickup = k < powerupsToSpawn.Count ? powerupsToSpawn[k] : null;
                            theCoinArray.coinsArraySpawnBuffer.Add(new CoinsSpawnInfo(CoinsSpawnType.Row, coinSpawnPoints, thePickup, isDoubleCoins));

                            coinsSpawnedInBatch += coinSpawnPoints.Count;

                            lastCoinTypeGenerated = CoinsSpawnType.Row;

                            k++;
                        }

                        lastZDistanceMultipleRowCoinsSpawned = playerSharedData.PlayerTransform.position.z;
                        return false;
                    }
                }

                List<KeyValuePair<float, RowResult>> valuesToRandomFrom = new List<KeyValuePair<float, RowResult>>();

                foreach (var item in validXPoints)
                {
                    float lane = item.Key;

                    if (!item.Value.isAdjacentLaneBlocked)
                    {
                        valuesToRandomFrom.Add(item);
                        continue;
                    }

                    if (lane > -1)
                    {
                        float leftLane = lane - 1;

                        if (allXPoints.ContainsKey(leftLane))
                        {
                            var leftLaneRowRes = allXPoints[leftLane];

                            if (!leftLaneRowRes.isAdjacentLaneBlocked)
                            {
                                valuesToRandomFrom.Add(item);
                                continue;
                            }
                        }
                    }

                    if (lane < 1)
                    {
                        float rightLane = lane + 1;

                        if (allXPoints.ContainsKey(rightLane))
                        {
                            var rightLaneRowRes = allXPoints[rightLane];

                            if (!rightLaneRowRes.isAdjacentLaneBlocked)
                            {
                                valuesToRandomFrom.Add(item);
                            }
                        }
                    }
                }

                if (valuesToRandomFrom.Count == 0)
                {
                    UnityEngine.Console.LogWarning("No row results to random from");
                    return false;
                }

                int randIndex = UnityEngine.Random.Range(0, valuesToRandomFrom.Count);
                keyValuePair = valuesToRandomFrom.ElementAt(randIndex);
            }

            startingRandXPosition = keyValuePair.Key;

            rowLength = keyValuePair.Value.length;
        }

        bool curve = false;
        float startingCurveZPosition = -1;
        Transform obstacleT = keyValuePair.Value.obstacleTransform;

        if (obstacleT != null)
        {
            CustomTag customTag = obstacleT.GetComponent<CustomTag>();

            if (customTag != null && customTag.HasTag("JumpableObject"))
            {
                float colliderZExtent = obstacleT.GetComponent<BoxCollider>().bounds.extents.z;
                float zCoveredDuringJump = theCoinArray.GetZDistanceCoveredDuringJump();

                float endingZPointOfCurve = /*theCoinArray.GetLastCoinPosition(true).z*/keyValuePair.Value.startingZPos + rowLength + bufferLength + (2 * colliderZExtent) + (zCoveredDuringJump * 0.5f);

                //  tempCenter.Add(new Vector3(startingRandXPosition, 0.172f, endingZPointOfCurve));

                if (!CheckIfZoneIsObstructed(new Vector3(startingRandXPosition, 0.172f, endingZPointOfCurve), sizeOfCurvedEndedSafeArea))
                {
                    float tempRowLength = rowLength;
                    tempRowLength += bufferLength;
                    tempRowLength += colliderZExtent;

                    //  Debug.DrawRay(new Vector3(startingRandXPosition, 0, keyValuePair.Value.startingZPos + tempRowLength), Vector3.up * 100f, Color.cyan, 5);

                    tempRowLength -= (zCoveredDuringJump * 0.5F);

                    curve = true;
                    startingCurveZPosition = /*theCoinArray.GetLastCoinPosition(true).z*/keyValuePair.Value.startingZPos + tempRowLength;

                    float lengthForOneCoin = theCoinArray.GetLengthRequiredForGivenCoins(1);
                    float lastRowCoinPosZ = startingCurveZPosition - lengthForOneCoin;
                    float startingRowCoinPosZ = /*theCoinArray.GetLastCoinPosition(true).z*/keyValuePair.Value.startingZPos;

                    rowLength = lastRowCoinPosZ - startingRowCoinPosZ;
                }
            }
        }

        InventoryItemSO pickup = GetPickupForCurrentGenMethod(CoinsSpawnType.Row, theCoinArray);

        int possibleCoins = theCoinArray.GetPossibleCoinsForGivenLength(rowLength);

        if (possibleCoins > 5 /*&& !curve*/)
        {
            rowLength = theCoinArray.GetLengthRequiredForGivenCoins(5);
        }

        if (possibleCoins >= 2)
        {
            theCoinArray.lengthToGenerateCoins = rowLength;

            List<Vector3> coinSpawnPoints;

            if (!curve)
            {
                coinSpawnPoints = theCoinArray.GenerateRowCoinPoints(startingRandXPosition, true, zOffsetToStartRowGen, -1, true, false, keyValuePair.Value.startingZPos);
            }
            else
            {
                float lengthForOneCoin = theCoinArray.GetLengthRequiredForGivenCoins(1);
                float lastRowCoinPosZ = startingCurveZPosition - lengthForOneCoin;

                coinSpawnPoints = theCoinArray.GenerateRowCoinPoints(startingRandXPosition, true, -1, -1, true, true, lastRowCoinPosZ, true);
            }

            theCoinArray.coinsArraySpawnBuffer.Add(new CoinsSpawnInfo(CoinsSpawnType.Row, coinSpawnPoints, pickup, isDoubleCoins));

            coinsSpawnedInBatch += coinSpawnPoints.Count;
        }
        else if (!curve)
        {
            return false;
        }
        //else
        //{
        //    UnityEngine.Console.LogWarning("Generating CURVE Without ROW");
        //}

        if (curve)
        {
            TryGeneratingCurvedCoins(keyValuePair.Value.obstacleTransform, theCoinArray, new Vector3(startingRandXPosition, 0.172f, startingCurveZPosition));
        }

        //if (zig)
        //{
        //    TryGeneratingZigCoins(origin, theCoinArray, zDistanceWhereLastObstacleEnded);
        //}

        lastCoinTypeGenerated = CoinsSpawnType.Row;

        return true;
    }

    private bool TryGeneratingCurvedCoins(Transform hitTransform, CoinsArray theCoinArray, Vector3 overridePosition)
    {
        InventoryItemSO pickup = GetPickupForCurrentGenMethod(CoinsSpawnType.CurvedRay, theCoinArray);

        List<Vector3> coinSpawnPoints = theCoinArray.GenerateCurvedCoinPoints(characterData.PlayerInformation[0].jump_height, playerSharedData.JumpDuration, 1, true, overridePosition, 0, true);
        theCoinArray.coinsArraySpawnBuffer.Add(new CoinsSpawnInfo(CoinsSpawnType.CurvedRay, coinSpawnPoints, pickup, isDoubleCoins));

        return true;
    }

    private CoinsArray InstantiateCoinsArray(Vector3 position)
    {
        //   UnityEngine.Console.Log("Instantiating Coins Array");

        CoinsArray theCoinArray = null;

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
        theCoinArray.transform.position = new Vector3(position.x, coinGenerationData.coinSpawnYOffset, position.z);

        return theCoinArray;
    }

    private bool IsItSafeToSpawnSpecificNoOfRowCoinsInSpecificLane(int noOfCoins, CoinsArray theCoinArray, float zDistanceWhereLastObstacleEnded, ref int coinsSpawnedInBatch)
    {
        float lengthRequired = theCoinArray.GetLengthRequiredForGivenCoins(noOfCoins);

        Vector3 startPosition = theCoinArray.GetLastCoinPosition(true);

        float rayLength = zDistanceWhereLastObstacleEnded - startPosition.z;

        float availableSpace = 0;

        //if (availableSpace < lengthRequired)
        //    return false;

        //  startPosition.z += zOffsetToStartRowGen;
        startPosition.x = Mathf.Round(startPosition.x);

        Vector3 origin = new Vector3(startPosition.x, 0 + rayCastYOffset, startPosition.z);
        Ray ray = new Ray(origin, Vector3.forward);
        int layerMask = 1 << LayerMask.NameToLayer("Obstacles") | 1 << LayerMask.NameToLayer("Walkable") | 1 << LayerMask.NameToLayer("Pickups");

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayLength, layerMask, QueryTriggerInteraction.Collide))
        {
            availableSpace = hit.distance - bufferLength;
        }
        else
        {
            availableSpace = rayLength;
        }

        if (availableSpace >= lengthRequired)
        {
            theCoinArray.lengthToGenerateCoins = lengthRequired;

            List<Vector3> coinSpawnPoints = theCoinArray.GenerateRowCoinPoints(startPosition.x, true, /*zOffsetToStartRowGen*/ 0, -1, true);
            theCoinArray.coinsArraySpawnBuffer.Add(new CoinsSpawnInfo(CoinsSpawnType.Row, coinSpawnPoints, null, isDoubleCoins));

            coinsSpawnedInBatch += coinSpawnPoints.Count;

            return true;
        }

        return false;
    }

    private Collider DoRowGenRayCast(Ray ray, float rayLength, int layerMask)
    {
        RaycastHit hit;

        Physics.Raycast(ray, out hit, rayLength, layerMask, QueryTriggerInteraction.Collide);

        return hit.collider;
    }

    private Dictionary<float, RowResult> GetXPositionPointsForStartingCoinGeneration(Vector3 helpPosition, float zDistanceWhereLastObstacleEnded)
    {
        Dictionary<float, RowResult> xPositionForCoinGen = new Dictionary<float, RowResult>();
        Ray[] rays = new Ray[3];
        float xPosition = -1f;
        int layerMask = 1 << LayerMask.NameToLayer("Obstacles") | 1 << LayerMask.NameToLayer("Walkable") | 1 << LayerMask.NameToLayer("Pickups");

        for (int i = 0; i < rays.Length; i++)
        {
            RaycastHit hit;
            Vector3 origin = new Vector3(xPosition, 0 + rayCastYOffset, helpPosition.z);

            // Check if the obstacle behind the origin is tagged as blocked obstacle (a truck maybe)

            Collider insideOriginCollider = CheckIfOriginIsInsideObstructed(origin);
            if (insideOriginCollider)
            {
                //   UnityEngine.Console.LogWarning("An Origin is inside an obstascle");

                origin.z = insideOriginCollider.bounds.max.z + .1f;
                var collider = DoRowGenRayCast(new Ray(origin, Vector3.forward), zDistanceWhereLastObstacleEnded - origin.z, layerMask);
                xPositionForCoinGen.Add(xPosition, new RowResult(true, collider != null ? collider.transform : null));
                xPosition++;
                continue;
            }

            bool isOriginOffset = false;

            Ray backRay = new Ray(origin, -Vector3.forward);
            Collider backwardHit = GetRayCastHitForObstacle(backRay, lengthOfSafeAreaToCheckBehindObstacles, "BlockedObstacle");
            if (backwardHit != null)
            {
                //  UnityEngine.Console.LogWarning("Oh No We Hit A BackWard Blocked Obstacle");
                float offsetOrigin = backwardHit.bounds.max.z + lengthOfSafeAreaToCheckBehindObstacles;

                if (offsetOrigin >= zDistanceWhereLastObstacleEnded)
                {
                    //  UnityEngine.Console.LogWarning("Tried offsetting origin but alas");

                    var collider = DoRowGenRayCast(new Ray(origin, Vector3.forward), zDistanceWhereLastObstacleEnded - origin.z, layerMask);
                    xPositionForCoinGen.Add(xPosition, new RowResult(true, collider != null ? collider.transform : null));
                    xPosition++;
                    continue;
                }

                //  UnityEngine.Console.LogWarning("Successfully Offseted The Origin");
                origin.z = offsetOrigin;
                //    Debug.DrawRay(origin, Vector3.up * 100, Color.blue, 5);
                isOriginOffset = true;
            }

            if (isOriginOffset)
            {
                insideOriginCollider = CheckIfOriginIsInsideObstructed(origin);

                if (insideOriginCollider)
                {
                    origin.z = insideOriginCollider.bounds.max.z + .1f;

                    var collider = DoRowGenRayCast(new Ray(origin, Vector3.forward), zDistanceWhereLastObstacleEnded - origin.z, layerMask);
                    xPositionForCoinGen.Add(xPosition, new RowResult(true, collider != null ? collider.transform : null));
                    //   UnityEngine.Console.LogWarning("An Origin is inside an obstascle");
                    xPosition++;
                    continue;
                }
            }

            float rayLength = zDistanceWhereLastObstacleEnded - origin.z;
            rays[i] = new Ray(origin, Vector3.forward);

            if (Physics.Raycast(rays[i], out hit, rayLength, layerMask, QueryTriggerInteraction.Collide))
            {
                float rayHitLength = hit.distance;
                CheckAndUpdateValidRowCoinGenCollection(origin, xPosition, xPositionForCoinGen, new RowResult(rayHitLength, hit.collider.transform, origin.z));
            }
            else
            {
                float rayHitLength = rayLength;
                CheckAndUpdateValidRowCoinGenCollection(origin, xPosition, xPositionForCoinGen, new RowResult(rayHitLength, null, origin.z));
            }

            xPosition++;
        }

        // Check For adjacent blocked lanes
        float lastCheckedLaneWithBlockedObstacle = -2;
        float lastCheckedBlockedObstacleZPos = 0;
        List<float> adjacentLanes = null;

        foreach (var item in xPositionForCoinGen)
        {
            var customTag = item.Value.obstacleTransform?.GetComponent<CustomTag>();

            if (customTag != null && customTag.HasTag("BlockedObstacle"))
            {
              //  UnityEngine.Console.Log("$Blocked Obstacle Found");

                if (lastCheckedLaneWithBlockedObstacle != -2)
                {
                    bool isAdjecent = Mathf.Abs(item.Key - lastCheckedLaneWithBlockedObstacle) == 1f &&
                        (Mathf.Abs(lastCheckedBlockedObstacleZPos - item.Value.obstacleTransform.position.z) < 3f);

                    if (isAdjecent)
                    {
                       // UnityEngine.Console.Log($"Adjecent Blockage Detected {isAdjecent}");

                        Debug.DrawRay(item.Value.obstacleTransform.position, Vector3.up * 100f, Color.cyan, 10f);

                        if (adjacentLanes == null)
                            adjacentLanes = new List<float>();

                        adjacentLanes.Add(item.Key);
                        adjacentLanes.Add(lastCheckedLaneWithBlockedObstacle);

                        break;
                    }
                }

                lastCheckedLaneWithBlockedObstacle = item.Key;
                lastCheckedBlockedObstacleZPos = item.Value.obstacleTransform.position.z;
            }
        }

        if (adjacentLanes != null)
        {
            for (int i = 0; i < adjacentLanes.Count; i++)
            {
                var rowResult = xPositionForCoinGen[adjacentLanes[i]];
                rowResult.isAdjacentLaneBlocked = true;
                xPositionForCoinGen[adjacentLanes[i]] = rowResult;
            }
        }

        return xPositionForCoinGen;
    }

    private Collider CheckIfOriginIsInsideObstructed(Vector3 origin)
    {
        Collider[] colliders = new Collider[1];
        Physics.OverlapSphereNonAlloc(origin, .1f, colliders, 1 << LayerMask.NameToLayer("Obstacles") | 1 << LayerMask.NameToLayer("Pickups"));

        return colliders[0];
    }

    private void CheckAndUpdateValidRowCoinGenCollection(Vector3 origin, float xPosition, Dictionary<float, RowResult> validXPositionForCoinGen, RowResult rowResult)
    {
        float rayHitLength = rowResult.length;
        rayHitLength -= bufferLength;

        rowResult.length = rayHitLength;

        if (rayHitLength >= minimumPossibleLengthForGeneration)
        {
            validXPositionForCoinGen.Add(xPosition, rowResult);
            Debug.DrawRay(origin, Vector3.forward * rayHitLength, Color.green, 10);
        }
        else
        {
            validXPositionForCoinGen.Add(xPosition, new RowResult(true, rowResult.obstacleTransform));

            Debug.DrawRay(origin, Vector3.forward * rayHitLength, Color.magenta, 10);
        }
    }

    private void RandomizeNextPickupSpawnWay()
    {
        pickupGenMethod = (CoinsSpawnType)UnityEngine.Random.Range(0, 3);
    }

    private InventoryItemSO GetPickupForCurrentGenMethod(CoinsSpawnType genMethod, CoinsArray theCoinArray)
    {
        if (isSpawnedPickupThisBatch)
            return null;

        InventoryItemSO pickup = null;

        if (pickupGenMethod == genMethod)
        {
            pickup = CheckForPossiblePickupGeneration(theCoinArray);

            if (pickup == null)
                return null;

            bool isSafeToSpawn = pickupsUtilityHelper.isSafeToSpawn(theCoinArray.GetLastCoinPosition(true).z, pickup, pendingPickupsToSpawn);
            pickup = isSafeToSpawn ? pickup : null;
        }

        if (pickup != null)
        {
            isSpawnedPickupThisBatch = true;
        }

        return pickup;
    }

    private InventoryItemSO GetPickupIrrespectiveOfCurrentGenMethod(CoinsArray theCoinArray, List<InventoryItemSO> pickupsToExclude = null, bool getPowerUpOnly = false)
    {
        InventoryItemSO pickup;

        pickup = CheckForPossiblePickupGeneration(theCoinArray, pickupsToExclude, getPowerUpOnly);

        return pickup;
    }

    private bool isThisPickupSafeToSpawn(InventoryItemSO pickup, CoinsArray theCoinArray, List<InventoryItemSO> pickupsToExclude = null)
    {
        PickupSpawnData data = pickupGenerationData.GetPickupSpawnData[pickup];
        PickupCondition condition = pickupGenerationData.PickupCurrentStateData.GetPickupCurrentStateDictionary[data.GetPickupType];

        float currentZPosition = gamePlaySessionData.DistanceCoveredInMeters;
        float distanceCoveredAfterLastPickupSpawned = currentZPosition - condition.lastSpawnedZPosition;
        float distanceCoveredAfterAnyPickupWasLastSpawned = currentZPosition - pickupGenerationData.PickupCurrentStateData.lastPickupSpawnedDistanceZPoint;

        //if (pickupsToExclude != null && pickupsToExclude.Contains(pickup))
        //    return false;

       // if (distanceCoveredAfterAnyPickupWasLastSpawned < minDistanceBeforeAPickupCanBeSpawnedAgain && pickupGenerationData.PickupCurrentStateData.lastPickupSpawnedDistanceZPoint != -1f)
          //  return false;

        //if (condition.initialOffset >= currentZPosition)
        //    return false;

        //if (data.GetPickupType == aeroplanePickupType && currentZPosition < pickupGenerationData.PickupCurrentStateData.magnetPickupSafeArea && pickupGenerationData.PickupCurrentStateData.magnetPickupSafeArea != -1f)
        //    return false;

        //if (data.GetPickupType == pickupGenerationData.PickupCurrentStateData)
        //    return false;

        bool isSafeToSpawn = pickupsUtilityHelper.isSafeToSpawn(theCoinArray.GetLastCoinPosition(true).z, data.GetPickupType, pendingPickupsToSpawn);

        if (!isSafeToSpawn)
            return false;

        // Debug.LogError ($".....................Probability of {data.GetPickupType} is........... {condition.currentProbability}");

        // if (distanceCoveredAfterLastPickupSpawned >= data.GetMinimumDistanceBeforeSpawn || condition.lastSpawnedZPosition == -1f)
        //if (distanceCoveredAfterLastPickupSpawned >= 190 || condition.lastSpawnedZPosition == 380f) //380
        if (distanceCoveredAfterLastPickupSpawned >= data.GetMinimumDistanceBeforeSpawn || condition.lastSpawnedZPosition == -1f)
        {
            if (data.GetPickupType == specialPickupsEnumSO.FigurinePickup)
            {
                FigurinePickup figurinePickup = data.GetPrefab.GetComponent<FigurinePickup>();
                int key = charactersDataBase.GetIndexOfTheCharacterFromItsConfigData(figurinePickup.CharacterData);

                int obtainedFigurines = gamePlaySessionInventory.GetFigurinesValue(key) + inventorySystem.GetCharacterFigurines(key);

                if (obtainedFigurines >= figurinePickup.CharacterData.FigurinesToUnlock)
                {
                    return false;
                }
            }

            else if ((data.GetPickupType == specialPickupsEnumSO.ArmourPickup || data.GetPickupType == specialPickupsEnumSO.AeroPlanePickup) && playerSharedData.IsArmour)
            {
                return false;
            }
            Debug.LogError("Yahhhhhh true.....................");
            return true;
        }


        return false;
    }

    private InventoryItemSO CheckForPossiblePickupGeneration(CoinsArray theCoinArray, List<InventoryItemSO> pickupsToExclude = null, bool getPowerUpOnly = false)
    {
        if (pauseProceduralPickupGeneration)
            return null;

        if (pendingPickupsToSpawn.Count > 0)
        {
            InventoryItemSO possiblePickup = pendingPickupsToSpawn.Peek();

            if (!getPowerUpOnly || possiblePickup is PowerUpSO)
            {
                bool isSafe = isThisPickupSafeToSpawn(possiblePickup, theCoinArray, pickupsToExclude);

                if (isSafe)
                {
                    return pendingPickupsToSpawn.Dequeue();
                }
            }
        }
        Debug.LogError("CheckfroPossiblespawn.........");
        PickupSpawnData[] pickupSpawnData = pickupGenerationData.GetPickupSpawnData.Values.ToArray();
        //zzz...............................................mainh...........................................................
        //List<ProportionValue<InventoryItemSO>> possibleSpawn = new List<ProportionValue<InventoryItemSO>>();
        // for (int i = 0; i < pickupSpawnData.Length; i++)
        // {
        //     PickupSpawnData data = pickupSpawnData[i];
        //     PickupCondition condition = pickupGenerationData.PickupCurrentStateData.GetPickupCurrentStateDictionary[data.GetPickupType];

        //     if (getPowerUpOnly && !(data.GetPickupType is PowerUpSO))
        //     {
        //         continue;
        //     }

        //     bool isSafe = true;  //isThisPickupSafeToSpawn(data.GetPickupType, theCoinArray, pickupsToExclude);

        //     if (isSafe)
        //     {
        //         Debug.LogError("safe condition true.................");
        //       possibleSpawn.Add(ProportionValue.Create(condition.currentProbability, data.GetPickupType));
        //         Debug.LogError($"[Safe] Adding {data.GetPickupType.name} with original weight {condition.currentProbability}");
        //     }
        // }

        // if (possibleSpawn.Count > 0)
        // {
        //     float probabilityForEach = 1f / (float)possibleSpawn.Count;
        //     Debug.LogError($"[Normalized Probabilities] Total Items: {possibleSpawn.Count}");

        //     foreach (var item in possibleSpawn)
        //     {
        //         item.Proportion = probabilityForEach;
        //         Debug.LogError($" → {item.Value.name} assigned normalized probability: {probabilityForEach:P1}");
        //     }

        //     InventoryItemSO pickupToSpawn = possibleSpawn.ChooseByRandom();
        //     Debug.LogError($"[Chosen Pickup] {pickupToSpawn.name} was randomly selected from possible list.");

        //     //  lastSpawnedPickup = pickupToSpawn;

        //     return pickupToSpawn;
        // }
        //zzz...............................................mainh...........................................................
        List<ProportionValue<InventoryItemSO>> possibleSpawn = new List<ProportionValue<InventoryItemSO>>();

        for (int i = 0; i < pickupSpawnData.Length; i++)
        {
            PickupSpawnData data = pickupSpawnData[i];
            InventoryItemSO pickupType = data.GetPickupType;

            // Skip non-powerups if only powerups are requested
            if (getPowerUpOnly && !(pickupType is PowerUpSO))
                continue;

            bool isSafe = isThisPickupSafeToSpawn(data.GetPickupType, theCoinArray, pickupsToExclude);
            Debug.LogError("Bool value safe is............." + isSafe);
            if (isSafe)
            {
                double spawnProb = data.GetSpawnProbability;

                if (spawnProb > 0f)
                {
                    possibleSpawn.Add(ProportionValue.Create(spawnProb, pickupType));
                    Debug.LogError($"[SAFE] Added '{pickupType.name}' with custom weight: {spawnProb}");
                }
                else
                {
                    Debug.LogError($"[SKIP] '{pickupType.name}' has 0 spawn probability. Not adding to spawn list.");
                }
            }
        }

        if (possibleSpawn.Count > 0)
        {
            double totalWeight = possibleSpawn.Sum(x => x.Proportion);

            // Normalize weights so they sum to 1(ChooseByRandom should ideally handle raw weights though)
            foreach (var item in possibleSpawn)
            {
                item.Proportion /= totalWeight;
                Debug.LogError($" → {item.Value.name} normalized weight: {item.Proportion:F2}");
            }

            InventoryItemSO pickupToSpawn = possibleSpawn.ChooseByRandom();
            Debug.LogError($"[SELECTED] '{pickupToSpawn.name}' was randomly selected.");

            return pickupToSpawn;
        }
        return null;
    }

    public void OnFloatingPointReset(float movedOffset)
    {
        if (lastZCoinPosition != -1)
        {
            lastZCoinPosition -= movedOffset;
        }

        doubleCoinsLastSpawnPointZ -= movedOffset;
        lastZDistanceMultipleRowCoinsSpawned -= movedOffset;
        lastZDistanceMultiplePowerUpsSpawned -= movedOffset;
    }

    public void OnBeforeFloatingPointReset()
    {
    }

    //private void SpawnThePossiblePickup()
    //{
    //    if (selectedPickupToSpawn != null)
    //    {
    //        GameObject instantiatedPickup = Instantiate(pickupGenerationData.GetPickupSpawnData[selectedPickupToSpawn].GetPrefab);

    //        instantiatedPickup.transform.position = theCoinArray.GetLastCoinPosition(true);

    //        selectedPickupToSpawn = null;
    //    }
    //}
}