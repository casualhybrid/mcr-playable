using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScatterCoinBatch : MonoBehaviour, IFloatingReset
{
    private ScatterCoinGenerator scatterCoinGen;
    private readonly HashSet<Transform> ReturnedToPoolCoinsTransforms = new HashSet<Transform>();

    private Vector3 origin;

    public bool ShoudNotOffsetOnRest { get; set; }

    public void Initialize(ScatterCoinGenerator scg)
    {
        scatterCoinGen = scg;
    }

    public void GenerateDestructibleScatterCoins(GameEvent gameEvent)
    {
        int coinsToGenerate = UnityEngine.Random.Range(scatterCoinGen.destructibleVehicleCoinsMin, scatterCoinGen.destructibleVehicleCoinsMax);
        BurstCoinsRoutine(coinsToGenerate);
    }

    public void GenerateNonDestructibleScatterCoins(GameEvent gameEvent)
    {
        int coinsToGenerate = UnityEngine.Random.Range(scatterCoinGen.nonDestructibleVehicleCoinsMin, scatterCoinGen.nonDestructibleVehicleCoinsMax);
        BurstCoinsRoutine(coinsToGenerate);
    }

    private void BurstCoinsRoutine(int coinsToGenerate)
    {
        List<Transform> spawnedBatch = new List<Transform>();
        origin = scatterCoinGen.playerShatedData.LastObstacleDestroyed.parentTransform.position;
        origin.y = 0;

        float offsetFromPlayer = scatterCoinGen.zSpawnOffset * (Mathf.Clamp(SpeedHandler.GameTimeScale * scatterCoinGen.gameTimeScalePercentToConsiderForCoinsOrigin, 1, 3));
        Vector3 landingPoint = scatterCoinGen.playerShatedData.PlayerTransform.position + new Vector3(0, 0, offsetFromPlayer);
        landingPoint.y = 0;
        landingPoint.x = origin.x;

        for (int i = 0; i < coinsToGenerate; i++)
        {
            Vector3 randPointInCircle = GetRandomCartesianCoordinateInsideCircle();
            Vector3 point = landingPoint + randPointInCircle;

            StartCoroutine(MoveCoinToTargetPosition(point - origin, spawnedBatch));
        }

        StartCoroutine(ReturnCoinBatchWhenLeftBehindRoutine(spawnedBatch, landingPoint.z));
    }

    private Vector3 GetRandomCartesianCoordinateInsideCircle()
    {
        float randomLength = UnityEngine.Random.Range(0, scatterCoinGen.radius);
        float randomTheta = UnityEngine.Random.Range(0, 360f);

        float randomPointX = randomLength * Mathf.Cos(randomTheta);
        float randomPointZ = randomLength * Mathf.Sin(randomTheta);

        return new Vector3(randomPointX, 0, randomPointZ);
    }

    private IEnumerator MoveCoinToTargetPosition(Vector3 targetOffsetFromOrigin, List<Transform> coinsSpawned)
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(scatterCoinGen.minCoinJumpDelay, scatterCoinGen.maxCoinJumpDelay));

        Vector3 targetPos = origin + targetOffsetFromOrigin;

        int bouncedTimes = 0;
        GameObject spawnedCoin = scatterCoinGen.coinPool.Request().gameObject;
        CoinPickup coinPickup = spawnedCoin.GetComponent<CoinPickup>();
        coinPickup.ShoudNotOffsetOnRest = false;
        coinPickup.MainCollider.enabled = false;
        coinPickup.OnPickupFinished += HandlePickupDone;
        coinsSpawned.Add(spawnedCoin.transform);

        spawnedCoin.transform.SetPositionAndRotation(origin, Quaternion.identity);

        Transform coinT = spawnedCoin.transform;

        float diffBtwOriginAndTarget = targetPos.z - origin.z;
        Vector3 startPos = coinT.position;
        float startingHeight;
        float height = UnityEngine.Random.Range(scatterCoinGen.coinJumpHeightMin, scatterCoinGen.coinJumpHeightMax);
        bool isBouncing = false;
        float bounceSpeed = (scatterCoinGen.coinJumpSpeed * height);
        startingHeight = height;
        GameObject coinGameObject = coinT.gameObject;

        float elapsedTime = 0;
        float speed = 0;

        float spawnOffsetFactor =  Mathf.Abs((diffBtwOriginAndTarget / scatterCoinGen.zSpawnOffset) * scatterCoinGen.distanceToFirstBounceSpeedWeightage);

        bool ignoreSpeedReduction = false;

        while (Time.deltaTime * ((bounceSpeed * scatterCoinGen.playerShatedData.PlayerRigidBody.velocity.z * scatterCoinGen.bouncedSpeedReductionNormalizedPercentAfterFirstBounce) / spawnOffsetFactor) < scatterCoinGen.bounceCompletionFrequency)
        {
            while (elapsedTime <= 1 && coinGameObject.activeInHierarchy)
            {
                if(!isBouncing)
                {
                    targetPos = origin + targetOffsetFromOrigin;
                    startPos = origin;
                }
                else
                {
                    targetPos = origin + targetOffsetFromOrigin;
                    startPos = targetPos;
                }


                float previousSpeed = speed;

                speed = bouncedTimes == 0 ? Time.deltaTime * ((bounceSpeed * scatterCoinGen.playerShatedData.PlayerRigidBody.velocity.z) / spawnOffsetFactor) :
                   Time.deltaTime * ((bounceSpeed * scatterCoinGen.playerShatedData.PlayerRigidBody.velocity.z * scatterCoinGen.bouncedSpeedReductionNormalizedPercentAfterFirstBounce) / spawnOffsetFactor);

                if (ignoreSpeedReduction)
                {
                    ignoreSpeedReduction = false;
                }
                else if (speed < previousSpeed)
                {
                    speed = previousSpeed;
                }

                // UnityEngine.Console.Log($"Speed at bounce {bouncedTimes} is {speed}");

                elapsedTime += speed;

                if (elapsedTime > 1)
                {
                    coinT.position = targetPos;
                    break;
                }

                Vector3 pos = MathParabola.Parabola(startPos, targetPos, height, Mathf.Clamp01(elapsedTime));

                coinT.position = pos;

                yield return null;
            }

            isBouncing = true;

            // Height Reduction
            elapsedTime %= 1;
            height *= 1 - (scatterCoinGen.heightLossPerBounceNormalizedPercent);
            bounceSpeed = ((scatterCoinGen.coinJumpSpeed * startingHeight) / height);
            startPos = coinT.position;
            targetPos = coinT.position;

            coinPickup.MainCollider.enabled = true;

            bouncedTimes++;

            ignoreSpeedReduction = bouncedTimes == 1;
        }


        Vector3 _pos = coinT.position;
        _pos.y = 0;
        coinT.position = _pos;
    }

    private IEnumerator ReturnCoinBatchWhenLeftBehindRoutine(List<Transform> coinBatch, float batchZPosition)
    {
        float zDiff = batchZPosition - scatterCoinGen.playerShatedData.PlayerTransform.position.z;

        while (zDiff > -5f)
        {
            yield return null;

            zDiff = batchZPosition - scatterCoinGen.playerShatedData.PlayerTransform.position.z;
        }

        for (int i = 0; i < coinBatch.Count; i++)
        {
            Transform coinT = coinBatch[i];

            // Already returned to pool
            if (ReturnedToPoolCoinsTransforms.Contains(coinT))
                continue;

            CoinPickup coinPickup = coinT.GetComponent<CoinPickup>();
            coinPickup.OnPickupFinished -= HandlePickupDone;
            scatterCoinGen.coinPool.Return(coinT);
        }

        ReturnedToPoolCoinsTransforms.Clear();
        StopAllCoroutines();
        scatterCoinGen.BatchHasFinished(this);
    }

    private void HandlePickupDone(Transform coinTransform)
    {
        coinTransform.GetComponent<CoinPickup>().OnPickupFinished -= HandlePickupDone;
        ReturnedToPoolCoinsTransforms.Add(coinTransform);
        scatterCoinGen.coinPool.Return(coinTransform);
    }

    public void OnFloatingPointReset(float movedOffset)
    {
        origin.z -= movedOffset;
    }

    public void OnBeforeFloatingPointReset()
    {
        
    }
}