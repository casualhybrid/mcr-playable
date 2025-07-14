using System.Collections.Generic;
using TheKnights.SaveFileSystem;
using UnityEngine;

public class MagnetPower : PowerUp
{
    [SerializeField] private MagnetPowerUpInfo magnetPowerUpInfo;
    [SerializeField] private Vector3 castedBoxSize;
    [SerializeField] private Transform targetPointT;
    [SerializeField] private GamePlaySessionData gamePlaySessionData;
    [SerializeField] private SpecialPickupsEnumSO specialPickupsEnumSO;
    [SerializeField] private PlayerSharedData playerSharedData;
    [SerializeField] private SaveManager saveManager;

    private List<CoinPickup> detectedCoinsCollection = new List<CoinPickup>();

    private float elapsedTime;
    private float currentDistanceCoveredPlayerT;

    public override void SetUp()
    {
        base.SetUp();
        PowerUpsChannel.RaisePowerActivatedEvent(specialPickupsEnumSO.MagnetPickup as InventoryItemSO, magnetPowerUpInfo.DefaultDuration, true, !saveManager.MainSaveFile.TutorialHasCompleted);
        elapsedTime = saveManager.MainSaveFile.TutorialHasCompleted ? gamePlaySessionData.DistanceCoveredInMeters : playerSharedData.PlayerTransform.position.z;
        currentDistanceCoveredPlayerT = elapsedTime;
        detectedCoinsCollection?.Clear();
        playerSharedData.isMagnetActive = true;
    }

    public override void Exit()
    {
        base.Exit();
        FinishExecutingRemainingCoins();
        detectedCoinsCollection?.Clear();
        PowerUpsChannel.RaisePowerDeactivatedEvent(specialPickupsEnumSO.MagnetPickup as InventoryItemSO);
        playerSharedData.isMagnetActive = false;
    }

    // Makes sure that the coins being attracted finish their way even if magnet power runs out
    private void FinishExecutingRemainingCoins()
    {
        for (int i = 0; i < detectedCoinsCollection.Count; i++)
        {
            CoinPickup coinPickup = GetCoinPickupAndDiscardInvalid(i);

            if (coinPickup == null)
            {
                continue;
            }

            coinPickup.StartCoroutine(coinPickup.FinishMovingTowardsPlayer(targetPointT));
        }
    }

    public override void Execute()
    {
        base.Execute();

        float currentDistanceCovered = 0;

        if (saveManager.MainSaveFile.TutorialHasCompleted)
        {
            currentDistanceCovered = gamePlaySessionData.DistanceCoveredInMeters;
        }
        else
        {
            float curPlayerPos = playerSharedData.PlayerTransform.position.z;
            if (curPlayerPos > currentDistanceCoveredPlayerT)
            {
                float diff = curPlayerPos - currentDistanceCoveredPlayerT;
                currentDistanceCovered += diff;
            }
            currentDistanceCoveredPlayerT = curPlayerPos;
        }

        if (currentDistanceCovered > magnetPowerUpInfo.DefaultDuration + elapsedTime)
        {
            Exit();
            return;
        }

        DetectCoinsToAttract();

        for (int i = 0; i < detectedCoinsCollection.Count; i++)
        {
            CoinPickup coinPickup = GetCoinPickupAndDiscardInvalid(i);

            if (coinPickup == null)
            {
                continue;
            }

            coinPickup.MoveTowardsPlayer(targetPointT);
        }
    }

    private CoinPickup GetCoinPickupAndDiscardInvalid(int i)
    {
        CoinPickup coinPickup = detectedCoinsCollection[i];

        if (!coinPickup.isActive)
        {
            // O(1) only and requires no list shuffling
            detectedCoinsCollection.RemoveBySwap(i);
            return null;
        }

        return coinPickup;
    }

    private void DetectCoinsToAttract()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position, castedBoxSize / 2f, transform.rotation, 1 << LayerMask.NameToLayer("Pickups"), QueryTriggerInteraction.Collide);

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            CoinPickup coinPickup;

            if (!collider.CompareTag("Coin"))
            {
                continue;
            }
            else
            {
                coinPickup = collider.GetComponent<CoinPickup>();

                if (coinPickup == null)
                {
                    throw new System.Exception("Failed to find Behaviour of type CoinPickup in the detected coin collider");
                }
            }

            if (!coinPickup.isMovingTowardsTarget)
            {
                detectedCoinsCollection.Add(coinPickup);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawCube(transform.position, castedBoxSize);
    }
}
