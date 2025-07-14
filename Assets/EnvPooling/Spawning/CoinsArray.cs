using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public enum CoinsSpawnType
{
    Row, CurvedRay, ZigZag
}

public enum PickupItemPlacement
{
    Start, Center, End
}

public class CoinsArray : MonoBehaviour, IFloatingReset
{
    public bool IsInitialized;

    [SerializeField] private PickupGenerationData pickupGenData;
    [SerializeField] private GamePlaySessionData gamePlaySessionData;
    [SerializeField] private PlayerData characterData;
    [SerializeField] private PlayerSharedData playerSharedData;
    [SerializeField] private SpeedHandler speedHandler;

    [SerializeField] private GeneralGameObjectPool coinPool;

    [SerializeField] private GeneralGameObjectPool doubleCoinPool;
    [EnumToggleButtons] [SerializeField] public CoinsSpawnType coinsSpawnType;

    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject doubleCoinPrefab;

   // [ShowIf("coinsSpawnType", CoinsSpawnType.CurvedRay)] [SerializeField] private int ascendingCoins;
   // [ShowIf("coinsSpawnType", CoinsSpawnType.CurvedRay)] [SerializeField] private int descendingCoins;
    [ShowIf("coinsSpawnType", CoinsSpawnType.CurvedRay)] [SerializeField] private float distBetweeenCurvedCoins;
    [ShowIf("coinsSpawnType", CoinsSpawnType.CurvedRay)][SerializeField] private float yOffsetCurvedCoins;

    // [ShowIf("coinsSpawnType", CoinsSpawnType.Row)] [SerializeField] private int coinsToGen;
    [ShowIf("coinsSpawnType", CoinsSpawnType.Row)] [SerializeField] public float lengthToGenerateCoins;

    [ShowIf("coinsSpawnType", (CoinsSpawnType.Row | CoinsSpawnType.ZigZag))] [SerializeField] private float distBetweenCoins;

    [ShowIf("coinsSpawnType", CoinsSpawnType.ZigZag)] public float[] zigCoins;

    [HideInInspector] [SerializeField] private float[] ZcoinsArray;
    [HideInInspector] [SerializeField] private float[] YcoinsArray;
    /*[HideInInspector]*/
    public List<CoinsSpawnInfo> coinsArraySpawnBuffer { get; set; } = new List<CoinsSpawnInfo>();

    [SerializeField] private MagnetPowerUpInfo magnetPowerUpInfo;
    [SerializeField] private InventoryItemSO magnetPickupType;

    private float totalJumpZLength;
    private float startingJumpHeight;
    private float targetJumpHeight;

    private int lastCoinFetchedIndex = -1; // only for playmode as we don't want coins to be instantiated again

    private readonly List<CoinPickup> coinsSpawned = new List<CoinPickup>();
    private readonly List<SpecialPickup> specialPickupsSpawned = new List<SpecialPickup>();

    private float nonOverridenGameTimeScale // AKA current game speed
    {
        get
        {
            return speedHandler.isOverriden ? SpeedHandler.GameTimeScaleBeforeOverriden : SpeedHandler.GameTimeScale;
        }
    }

    public bool ShoudNotOffsetOnRest { get; set; }

    private void OnEnable()
    {
        //  lastCoinFetchedIndex = 0;
    }

    private void OnDisable()
    {
        ShoudNotOffsetOnRest = false;
    }

    private void Start()
    {
        // GenerateCoins();
    }

    private void Update()
    {
        ReturnAllCoinsAndPickupsThenDestroyCoinsArray(false);
    }

    public void ReturnAllCoinsAndPickupsThenDestroyCoinsArray(bool forceReturn)
    {
        float zPlayerPos = playerSharedData.PlayerTransform.position.z;

        for (int i = 0; i < coinsSpawned.Count; i++)
        {
            Transform coninT = coinsSpawned[i].transform;

            if (zPlayerPos - coninT.position.z >= 3f || forceReturn)
            {
                CoinPickup coinPickup = coninT.GetComponent<CoinPickup>();
                coinPickup.OnPickupFinished -= HandleCoinHasFinished;
                if (coinPickup.IsDoubleCoin)
                {
                    doubleCoinPool.Return(coinsSpawned[i].transform);
                }
                else
                {
                    coinPool.Return(coinsSpawned[i].transform);
                }

                coinsSpawned.Remove(coinPickup);
                i--;
            }
        }

        for (int i = 0; i < specialPickupsSpawned.Count; i++)
        {
            Transform sPickupT = specialPickupsSpawned[i].transform;

            if (zPlayerPos - sPickupT.position.z >= 3f || forceReturn)
            {
                SpecialPickup specialPickup = sPickupT.GetComponent<SpecialPickup>();

                if (!specialPickup.isSafeToDisable)
                    continue;

                specialPickup.OnPickupFinished -= HandleSpecialPickupHasFinished;

                Destroy(specialPickup.gameObject);

                specialPickupsSpawned.Remove(specialPickup);
                i--;
            }
        }

        if ((IsInitialized && coinsSpawned.Count == 0 && specialPickupsSpawned.Count == 0) || forceReturn)
            Destroy(this.gameObject,3);
    }

    [Button("Generate")]
    public void GenerateCoins()
    {
        if (!Application.isPlaying)
        {
            DestroyAllCoins();
        }

        if (coinsSpawnType == CoinsSpawnType.CurvedRay)
        {
            //  GenerateCurvedRayCoins();
        }
        else if (coinsSpawnType == CoinsSpawnType.Row)
        {
            List<Vector3> coinSpawnPoints = GenerateRowCoinPoints();
            SpawnRowCoins(coinSpawnPoints);
        }
        else if (coinsSpawnType == CoinsSpawnType.ZigZag)
        {
            GenerateZigZagCoins();
        }
    }

    public int GetPossibleCoinsForGivenLength(float length)
    {
        int coinsToGen = Mathf.FloorToInt(length / distBetweenCoins);
        return coinsToGen;
    }

    public float GetLengthRequiredForGivenCoins(int coins)
    {
        float length = coins * distBetweenCoins;
        return length;
    }

    public void RemoveLastZigFromBuffer()
    {
        if (coinsArraySpawnBuffer.Count == 0)
            return;

        var coinSpawnInfo = coinsArraySpawnBuffer.Last();

        if (coinSpawnInfo != null && coinSpawnInfo.coinsSpawnType == CoinsSpawnType.ZigZag)
        {
            coinsArraySpawnBuffer.Remove(coinSpawnInfo);
        }
    }

    public int GetNumberOfCoinsSpawned(bool useBufferInsteadOfSpawnedCoins = false)
    {
        int numberOfCoins = 0;

        if (!useBufferInsteadOfSpawnedCoins)
        {
            numberOfCoins = transform.childCount;
        }
        else
        {
            foreach (CoinsSpawnInfo info in coinsArraySpawnBuffer)
            {
                numberOfCoins += info.coinSpawnPoints.Count;
            }
        }

        return numberOfCoins;
    }

    private GameObject InstantiateTheCoin(bool isDoubleCoin = false)
    {
        if (Application.isPlaying)
        {
            //if (lastCoinFetchedIndex >= transform.childCount)
            //{
            //    throw new System.Exception("Requested coin fetch index is greater than available instantiated coins");
            //}

            //Transform coin = transform.GetChild(lastCoinFetchedIndex++);
            //return coin.gameObject;

            GeneralGameObjectPool generalGameObjectPool = isDoubleCoin ? doubleCoinPool : coinPool;
            GameObject requestedCoin = generalGameObjectPool.Request().gameObject;
            CoinPickup coinPickup = requestedCoin.GetComponent<CoinPickup>();

            coinPickup.OnPickupFinished += HandleCoinHasFinished;

            requestedCoin.transform.SetParent(this.transform);
            requestedCoin.transform.localPosition = coinPrefab.transform.localPosition;
            requestedCoin.transform.localEulerAngles = Vector3.zero;

            coinsSpawned.Add(coinPickup);

            return requestedCoin;

            // return Instantiate(requestedCoin, this.transform) as GameObject;
        }
        else
        {
#if UNITY_EDITOR
            return PrefabUtility.InstantiatePrefab(coinPrefab, this.transform) as GameObject;

#else
 return Instantiate(coinPrefab, this.transform) as GameObject;

#endif
        }
    }

    private void HandleCoinHasFinished(Transform transform)
    {
        CoinPickup coinPickup = transform.GetComponent<CoinPickup>();
        coinPickup.OnPickupFinished -= HandleCoinHasFinished;

        if (coinPickup.IsDoubleCoin)
        {
            doubleCoinPool.Return(transform);
        }
        else
        {
            coinPool.Return(transform);
        }

        coinsSpawned.Remove(coinPickup);
    }

    private void HandleSpecialPickupHasFinished(Transform transform)
    {
        SpecialPickup specialPickup = transform.GetComponent<SpecialPickup>();
        specialPickup.OnPickupFinished -= HandleSpecialPickupHasFinished;

        specialPickupsSpawned.Remove(specialPickup);
    }

    public void DestroyAllCoins()
    {
        Transform[] coinsInChildren = GetComponentsInChildren<Transform>(true);

        if (coinsInChildren.Length <= 1)
        {
            return;
        }

        for (int i = coinsInChildren.Length; i > 1; --i)
        {
#if UNITY_EDITOR
            DestroyImmediate(this.transform.GetChild(0).gameObject);
#else

            Destroy(this.transform.GetChild(0).gameObject);

#endif
        }
    }
    bool IsSafeFromObstacles1(Vector3 spawnPoint, float minDistance)
    {
        foreach (var obstacle in FindObjectsOfType<Obstacle>())
        {
            if (Vector3.Distance(obstacle.transform.position, spawnPoint) < minDistance)
            {
                return false;
            }
        }
        return true;
    }
    public void CheckObstaclesState(Vector3 spawnPoint)
    {
        foreach (var obstacle in FindObjectsOfType<Obstacle>())
        {
            if (Vector3.Distance(obstacle.transform.position, spawnPoint) < 18)
            {
                obstacle.gameObject.SetActive(false);
                Debug.LogError("name is" + obstacle.name);
            }
        }
    }
    IEnumerator DelayedCheckObstacles(Vector3 spawnPoint)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        CheckObstaclesState(spawnPoint);
    }
    public void DissolveCoinBuffer()
    {
        foreach (CoinsSpawnInfo spawnInfo in coinsArraySpawnBuffer)
        {
            switch (spawnInfo.coinsSpawnType)
            {
                case CoinsSpawnType.Row:
                    SpawnRowCoins(spawnInfo.coinSpawnPoints, spawnInfo.pickupToSpawn, spawnInfo.isDoubleCoins);
                    break;

                case CoinsSpawnType.CurvedRay:
                    SpawnCurvedCoins(spawnInfo.coinSpawnPoints, spawnInfo.pickupToSpawn, PickupItemPlacement.Center, spawnInfo.isDoubleCoins);
                    break;

                case CoinsSpawnType.ZigZag:
                    ZigTheCoins(spawnInfo.coinSpawnPoints, spawnInfo.pickupToSpawn, spawnInfo.isDoubleCoins);
                    break;
            }
        }
    }

    #region RowCoins

    public int SpawnRowCoins(List<Vector3> coinSpawnPoints, InventoryItemSO pickupToSpawn = null, bool isDouble = false)
    {
        int coinsSpawned = 0;

        for (int i = 0; i < coinSpawnPoints.Count; i++)
        {
            GameObject coinOrPickup;
            bool pickupSpawned = false;

            if (pickupToSpawn != null && i == coinSpawnPoints.Count() - 1) // this will spawn a Pickup
            {
                coinOrPickup = pickupGenData.GetPickupSpawnData[pickupToSpawn].GetPrefab;
                coinOrPickup = Instantiate(coinOrPickup, this.transform);
                Vector3 pickupPos = coinSpawnPoints[0];

                pickupPos += new Vector3(0, 0, 3f);
                coinOrPickup.transform.position = pickupPos;
                StartCoroutine(DelayedCheckObstacles(pickupPos));

                pickupSpawned = true;
            }
            else // this will spawn a coin either single or double
            {
                coinOrPickup = InstantiateTheCoin(isDouble);
            }

            coinOrPickup.transform.position = coinSpawnPoints[i];

            if (pickupSpawned)
            {
                SpecialPickup specialPickup = coinOrPickup.GetComponent<SpecialPickup>();
                AddAndSubscribeSpecialPickup(pickupToSpawn, specialPickup);
            }

            coinsSpawned++;
        }

        return coinsSpawned;
    }

    public List<Vector3> GenerateRowCoinPoints(float xPosition = 0, bool overrideXPosition = false, float startingZOffset = -1f, float currentLengthToGenerateCoins = -1f, bool useBufferInsteadOfSpawnedCoins = false, bool generateCoinAtStartingPoint = false, float overrideLastZCoinPos = -1, bool generateInReverse = false)
    {
        List<Vector3> coinGenerationPoints = new List<Vector3>();
        Vector3 pos = overrideLastZCoinPos == -1 ? GetLastCoinPosition(useBufferInsteadOfSpawnedCoins) : new Vector3(transform.position.x, transform.position.y, overrideLastZCoinPos);

        if (startingZOffset != -1f)
        {
            pos.z += !generateInReverse ? startingZOffset : -startingZOffset;
        }

        if (overrideXPosition)
        {
            pos.x = xPosition;
        }

        pos.y = transform.TransformPoint(coinPrefab.transform.position).y;

        if (currentLengthToGenerateCoins == -1f)
        {
            currentLengthToGenerateCoins = lengthToGenerateCoins;
        }

        int coinsToGen = Mathf.FloorToInt(currentLengthToGenerateCoins / distBetweenCoins);

        if (generateCoinAtStartingPoint)
        {
            coinGenerationPoints.Add(pos);
        }

        for (int i = 0; i < coinsToGen; i++)
        {
            pos.z += !generateInReverse ? distBetweenCoins : -distBetweenCoins;
            coinGenerationPoints.Add(pos);
        }

        if (generateInReverse)
        {
            coinGenerationPoints.Reverse();
        }

        return coinGenerationPoints;
    }

    #endregion RowCoins

    #region CurvedRay

    public void SpawnCurvedCoins(List<Vector3> coinSpawnPoints, InventoryItemSO pickupToSpawn = null, PickupItemPlacement pickupItemPlacement = PickupItemPlacement.Center, bool isDouble = false)
    {
        int pickupItemIndex = pickupItemPlacement == PickupItemPlacement.Start ? 0 : pickupItemPlacement == PickupItemPlacement.End ? ZcoinsArray.Length - 1 : ZcoinsArray.Length / 2;

        float pitch = 1;

        for (int i = 0; i < coinSpawnPoints.Count; i++)
        {
            GameObject coinOrPickup;
            bool pickupSpawned = false;

            if (pickupToSpawn != null && i == pickupItemIndex)
            {
                coinOrPickup = pickupGenData.GetPickupSpawnData[pickupToSpawn].GetPrefab;
                coinOrPickup = Instantiate(coinOrPickup, this.transform);
                Vector3 pickupPos = coinSpawnPoints[0];

                pickupPos += new Vector3(0, 0, 3f);
                coinOrPickup.transform.position = pickupPos;
                StartCoroutine(DelayedCheckObstacles(pickupPos));
                // var centerPos = coinSpawnPoints[i];
                // centerPos.y += pickupItemPlacement == PickupItemPlacement.Center ? 0.1f : pickupItemPlacement == PickupItemPlacement.End ? -0.1f : 0;
                // coinSpawnPoints[i] = centerPos;

                pickupSpawned = true;
            }
            else
            {
                coinOrPickup = InstantiateTheCoin(isDouble);

                CoinPickup coinPickup = coinOrPickup.GetComponent<CoinPickup>();

                if (coinPickup != null)
                {
                    coinPickup.GetCoinAudioPlayer.AudioInstanceConfig.Pitch = pitch;
                    pitch += 0.4f;
                }
            }

            coinOrPickup.transform.position = coinSpawnPoints[i];

            if (pickupSpawned)
            {
                SpecialPickup specialPickup = coinOrPickup.GetComponent<SpecialPickup>();
                AddAndSubscribeSpecialPickup(pickupToSpawn, specialPickup);
            }

            //UnityEngine.Console.Log("Setting Curved Coin Posiiton " + pickup.transform.position);
        }
    }

    public List<Vector3> GenerateCurvedCoinPoints(float jumpHeight, float curveDuration, float curveCoverValueNormalized = 1, bool overrideZPos = false, Vector3 PositionOverride = default, float jumpDistance = 0, bool useBufferInsteadOfSpawnedCoins = false, bool includeAscend = true)
    {
        List<Vector3> coinGenerationPoints = new List<Vector3>();
        Vector3 pos = GetLastCoinPosition(useBufferInsteadOfSpawnedCoins);

        if (overrideZPos)
        {
            pos = PositionOverride;
        }

        //    UnityEngine.Console.Log("Generating Curved From Pos " + pos);

        startingJumpHeight = pos.y;
        targetJumpHeight = pos.y + /*characterData.PlayerInformation[0].jump_height*/ +jumpHeight;

        /* FindZPositionsDuringJump(pos, curveDuration, curveCoverValueNormalized); */
        if (jumpDistance > 0)
        {
            FindZPositionsDuringJump(pos, curveDuration, curveCoverValueNormalized, jumpDistance);
        }
        else
        {
            FindZPositionsDuringJump(pos, curveDuration, curveCoverValueNormalized, (GetPlayerForwardSpeed()) /*The distance covered each frame*/ * ((/*playerSharedData.JumpDuration*/curveDuration) / Time.fixedDeltaTime) * 1); // Passes in the default jump distance for a 180 degree arc
        }

        FindYPositionsDuringJump(pos, curveDuration, curveCoverValueNormalized, includeAscend);

        for (int i = 0; i < ZcoinsArray.Length; i++)
        {
            coinGenerationPoints.Add(new Vector3(pos.x, YcoinsArray[i], ZcoinsArray[i]));
        }

        return coinGenerationPoints;
    }

    public float GetZDistanceCoveredDuringJump()
    {
        //  float distanceCoveredDuringAscend = (characterData.PlayerInformation[0].ForwarspeedMultiplier * characterData.PlayerInformation[0].ForwardSpeedInitialValue) * ((playerSharedData.JumpDuration / 2f) / Time.fixedDeltaTime) * 1;
        float forwardSpeed = GetPlayerForwardSpeed();
        float distanceCoveredDuringAscend = (forwardSpeed) * ((playerSharedData.JumpDuration / 2f) / Time.fixedDeltaTime) * 1;

        float totalDitanceCoveredDuringJump = distanceCoveredDuringAscend * 2f;

        totalJumpZLength = totalDitanceCoveredDuringJump;

        return totalJumpZLength;
    }

    private void FindZPositionsDuringJump(Vector3 pos, float curveDuration, float curveCoverValueNormalized, float coinGenDis)
    {
        //Vector3 pos = transform.childCount == 0 || (Application.isPlaying && lastCoinFetchedIndex == 0) ? transform.position : transform.GetChild(transform.childCount - 1).position;

        // float distanceCoveredDuringAscend = (characterData.PlayerInformation[0].ForwarspeedMultiplier * characterData.PlayerInformation[0].ForwardSpeedInitialValue) * ((playerSharedData.JumpDuration / 2f) / Time.fixedDeltaTime) * 1;
        float forwardSpeed = GetPlayerForwardSpeed();
        float distanceCoveredDuringAscend = (forwardSpeed) * ((/*playerSharedData.JumpDuration*/curveDuration / 2f) / Time.fixedDeltaTime) * 1;

        //float totalDitanceCoveredDuringJump = distanceCoveredDuringAscend * 2f;

        totalJumpZLength = /* totalDitanceCoveredDuringJump */ coinGenDis;
        float distanceCoveredDuringAscendWithCurveCoverValue = distanceCoveredDuringAscend * curveCoverValueNormalized;

        /* float sampleAscending = distanceCoveredDuringAscendWithCurveCoverValue / ((float)ascendingCoins);
        float sampleDescending = distanceCoveredDuringAscendWithCurveCoverValue / ((float)descendingCoins); */

        float startingZOffset = distanceCoveredDuringAscend - distanceCoveredDuringAscendWithCurveCoverValue;

        int totalCoins = /* ascendingCoins + descendingCoins */ (int)((totalJumpZLength - (startingZOffset * 2)) / distBetweeenCurvedCoins);

        float previous = pos.z + startingZOffset;
        ZcoinsArray = new float[totalCoins + 1];
        int i = 0;

        for (; i < ZcoinsArray.Length; i++)
        {
            if (i == 0)
            {
                //   UnityEngine.Console.Log("First Curve Coin At " + previous);
                ZcoinsArray[i] = previous;
            }
            else
            {
                ZcoinsArray[i] = previous + distBetweeenCurvedCoins;
            }

            previous = ZcoinsArray[i];
        }
    }

    private void FindYPositionsDuringJump(Vector3 pos, float curveDuration, float curveCoverValueNormalized, bool includeAscend = true)
    {
        //   Vector3 pos = transform.childCount == 0 || (Application.isPlaying && lastCoinFetchedIndex == 0) ? transform.position : transform.GetChild(transform.childCount - 1).position;
        YcoinsArray = new float[ZcoinsArray.Length];
        float halfCurveDuration = curveDuration / 2f;
        float timeRequired = 0 + (halfCurveDuration - (halfCurveDuration * curveCoverValueNormalized));

        //    UnityEngine.Console.Log("First Time Required" + (timeRequired / (/*playerSharedData.JumpDuration*/ halfCurveDuration)));

        if (includeAscend)
        {
            bool descending = false;

            //  UnityEngine.Console.Log("Target Jump Height " + targetJumpHeight);

            for (int i = 0; i < ZcoinsArray.Length; i++)
            {
                float y;
                float t = (timeRequired / halfCurveDuration);

                if (t > 1 && !descending)
                {
                    timeRequired -= halfCurveDuration;
                    t = timeRequired / halfCurveDuration;
                    descending = true;
                }

                if (descending)
                {
                    y = LerpExtensions.EaseInSineLerp(targetJumpHeight, startingJumpHeight, t);

                    timeRequired += TimeRequiredToCoverDistance(distBetweeenCurvedCoins);
                }
                else
                {
                    y = LerpExtensions.EaseOutSineLerp(startingJumpHeight, targetJumpHeight, t);

                    timeRequired += TimeRequiredToCoverDistance(distBetweeenCurvedCoins);
                }

                YcoinsArray[i] = y + yOffsetCurvedCoins;
            }
        }
        else
        {
            for (int i = 0; i < ZcoinsArray.Length; i++)
            {
                float y;
                float t = timeRequired / (/*playerSharedData.JumpDuration */ curveDuration / 2f);

                y = LerpExtensions.EaseInSineLerp(startingJumpHeight, startingJumpHeight - Mathf.Abs(targetJumpHeight - startingJumpHeight), t);
                timeRequired += TimeRequiredToCoverDistance(distBetweeenCurvedCoins);

                YcoinsArray[i] = y;
            }
        }
    }

    private float TimeRequiredToCoverTwoCoinsLength(float firstCoin, float secondCoin)
    {
        //  float velocityOfPlayer = (characterData.PlayerInformation[0].ForwarspeedMultiplier * characterData.PlayerInformation[0].ForwardSpeedInitialValue) * (1 / Time.fixedDeltaTime) * 1;
        float forwardSpeed = GetPlayerForwardSpeed();
        float velocityOfPlayer = (forwardSpeed) * (1 / Time.fixedDeltaTime) * 1;

        float dist = secondCoin - firstCoin;
        float timeTaken = dist / velocityOfPlayer;

        return timeTaken;
    }

    private float TimeRequiredToCoverDistance(float distance)
    {
        //  float velocityOfPlayer = (characterData.PlayerInformation[0].ForwarspeedMultiplier * characterData.PlayerInformation[0].ForwardSpeedInitialValue) * (1 / Time.fixedDeltaTime) * 1;
        float forwardSpeed = GetPlayerForwardSpeed();
        float velocityOfPlayer = (forwardSpeed) * (1 / Time.fixedDeltaTime) * 1;

        float timeTaken = distance / velocityOfPlayer;

        return timeTaken;
    }

    #endregion CurvedRay

    #region ZigZagCoins

    public void GenerateZigZagCoins()
    {
        for (int i = 0; i < zigCoins.Length; i++)
        {
            if (i % 2 == 0)
            {
                //   coinsToGen = Mathf.Abs(zigCoins[i]);
                lengthToGenerateCoins = Mathf.Abs(zigCoins[i]);
                List<Vector3> coinSpawnPoints = GenerateRowCoinPoints();
                SpawnRowCoins(coinSpawnPoints);
            }
            else
            {
                float direction = zigCoins[i] < 0 ? -1f : 1f;
                List<Vector3> zigSpawnPoints = GenerateZigZagPoints(direction);
                ZigTheCoins(zigSpawnPoints);
            }
        }
    }

    public void ZigTheCoins(List<Vector3> coinSpawnPoints, InventoryItemSO pickupToSpawn = null, bool isDouble = false)
    {
        for (int i = 0; i < coinSpawnPoints.Count; i++)
        {
            GameObject coinOrPickup;
            bool pickupSpawned = false;

            if (pickupToSpawn != null && i == 1) // this will spawn a Pickup
            {
                coinOrPickup = pickupGenData.GetPickupSpawnData[pickupToSpawn].GetPrefab;
                coinOrPickup = Instantiate(coinOrPickup, this.transform);
                Vector3 pickupPos = coinSpawnPoints[0];

                pickupPos += new Vector3(0, 0, 3f);
                coinOrPickup.transform.position = pickupPos;
                StartCoroutine(DelayedCheckObstacles(pickupPos));

                pickupSpawned = true;
            }
            else
            {
                coinOrPickup = InstantiateTheCoin(isDouble);

                CoinPickup coinPickup = coinOrPickup.GetComponent<CoinPickup>();

                if (coinPickup != null)
                {
                    coinPickup.GetCoinAudioPlayer.AudioInstanceConfig.Pitch = 1.2f;
                }
            }

            coinOrPickup.transform.position = coinSpawnPoints[i];

            if (pickupSpawned)
            {
                SpecialPickup specialPickup = coinOrPickup.GetComponent<SpecialPickup>();
                AddAndSubscribeSpecialPickup(pickupToSpawn, specialPickup);
            }
        }
    }

    public List<Vector3> GenerateZigZagPoints(float direction, bool overrideForwardSpeeed = false, float overridenforwardSpeed = -1, bool ovverideSideWaysSpeed = false, float overridenSideWaysSpeed = -1, bool useBufferInsteadOfSpawnedCoins = false)
    {
        List<Vector3> coinGenerationPoints = new List<Vector3>();

        // int coinsVal = Mathf.Abs(zigCoins[i]);
        int coinsVal = 2;

        //  float timeRequiredToSwitchLane = characterData.PlayerInformation[0].SideWaysInitalSpeed;
        float sideWaysSpeed = GetPlayerSideWaysSpeed();
        float timeRequiredToSwitchLane = !ovverideSideWaysSpeed ? sideWaysSpeed : overridenSideWaysSpeed;

        //float ZdistanceCoveredDuringLaneChange = (characterData.PlayerInformation[0].ForwarspeedMultiplier * characterData.PlayerInformation[0].ForwardSpeedInitialValue) * (timeRequiredToSwitchLane / Time.fixedDeltaTime) * 1;
        float forwardSpeed = GetPlayerForwardSpeed();
        float ZdistanceCoveredDuringLaneChange = (!overrideForwardSpeeed ? forwardSpeed : overridenforwardSpeed) * (timeRequiredToSwitchLane / Time.fixedDeltaTime) * 1;
        //  UnityEngine.Console.Log($"ZDistanceCoveredDuringLaneChange { ZdistanceCoveredDuringLaneChange}");

        Vector3 startPoint = GetLastCoinPosition(useBufferInsteadOfSpawnedCoins);
        Vector3 endPoint = new Vector3(startPoint.x + direction, startPoint.y, startPoint.z + ZdistanceCoveredDuringLaneChange);

        Vector3 dir = (endPoint - startPoint).normalized;
        float dist = Vector3.Distance(endPoint, startPoint);

        //  UnityEngine.Console.Log($"StartPoint {startPoint} and endpoint {endPoint}");

        float sample = dist / ((float)coinsVal);

        for (int k = 0; k < coinsVal; k++)
        {
            float offset = sample;

            //  UnityEngine.Console.Log($"StartPoint {startPoint} and dir {dir} and offset {offset}");

            Vector3 point = startPoint + (dir * offset);
            point.y = transform.TransformPoint(coinPrefab.transform.position).y;
            coinGenerationPoints.Add(point);

            sample += sample;
        }

        return coinGenerationPoints;
    }

    public Vector3 GetZiggedPosition(float direction, bool useBufferInsteadOfSpawnedCoins = false)
    {
        Vector3 startPoint = GetLastCoinPosition(useBufferInsteadOfSpawnedCoins);
        //   float timeRequiredToSwitchLane = characterData.PlayerInformation[0].SideWaysInitalSpeed;
        float sideWaysSpeed = GetPlayerSideWaysSpeed();
        float timeRequiredToSwitchLane = sideWaysSpeed;

        //  float ZdistanceCoveredDuringLaneChange = (characterData.PlayerInformation[0].ForwarspeedMultiplier * characterData.PlayerInformation[0].ForwardSpeedInitialValue) * (timeRequiredToSwitchLane / Time.fixedDeltaTime) * 1;
        float forwardSpeed = GetPlayerForwardSpeed();
        float ZdistanceCoveredDuringLaneChange = (forwardSpeed) * (timeRequiredToSwitchLane / Time.fixedDeltaTime) * 1;

        //  UnityEngine.Console.Log($"ZDistanceCoveredDuringLaneChange { ZdistanceCoveredDuringLaneChange}");

        Vector3 endPoint = new Vector3(startPoint.x + direction, startPoint.y, startPoint.z + ZdistanceCoveredDuringLaneChange);

        return endPoint;
    }

    #endregion ZigZagCoins

    public Vector3 GetLastCoinPosition(bool useBufferInsteadOfSpawnedCoins = false)
    {
        Vector3 lastCoinPos = transform.position;

        if (!useBufferInsteadOfSpawnedCoins &&
        transform.childCount > 0 &&
        (!Application.isPlaying || lastCoinFetchedIndex != 0))
        {
            lastCoinPos = transform.GetChild(transform.childCount - 1).position;
        }
        else if (coinsArraySpawnBuffer.Count > 0 && coinsArraySpawnBuffer.Last().coinSpawnPoints.Count > 0)
        {
            lastCoinPos = coinsArraySpawnBuffer.Last().coinSpawnPoints.Last();
        }

        return lastCoinPos;
    }

    private float GetPlayerForwardSpeed()
    {
        if (!Application.isPlaying)
            return speedHandler.GetForwardSpeedBasedOnCurrentOriginalTimeScale();

        return speedHandler.GetForwardSpeedBasedOnSpecificGameTimeScale(nonOverridenGameTimeScale);
    }

    private float GetPlayerSideWaysSpeed()
    {
        return speedHandler.GetSideWaysSpeedBasedOnSpecificGameTimeScale(nonOverridenGameTimeScale);
    }

    private void AddAndSubscribeSpecialPickup(InventoryItemSO pickupToSpawn, SpecialPickup specialPickup)
    {
        PickupCondition condition = pickupGenData.PickupCurrentStateData.GetPickupCurrentStateDictionary[pickupToSpawn];
        condition.lastSpawnedZPosition = gamePlaySessionData.DistanceCoveredInMeters;

        pickupGenData.PickupCurrentStateData.lastSpawnedPickup = pickupToSpawn;
        pickupGenData.PickupCurrentStateData.lastPickupSpawnedDistanceZPoint = gamePlaySessionData.DistanceCoveredInMeters;

        if (pickupToSpawn == magnetPickupType)
            pickupGenData.PickupCurrentStateData.magnetPickupSafeArea = condition.lastSpawnedZPosition + magnetPowerUpInfo.DefaultDuration;
        try
        {

            specialPickup.OnPickupFinished += HandleSpecialPickupHasFinished;
            specialPickupsSpawned.Add(specialPickup);
            specialPickup.RaiseSpecialPickupSpawnedEvent();
        }
        catch (Exception ex)
        {
            print(ex);
        }
        
    }

    public void OnFloatingPointReset(float movedOffset)
    {
    }

    public void OnBeforeFloatingPointReset()
    {
    }
}