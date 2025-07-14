using DG.Tweening;
using System;
using System.Collections;
using TheKnights.SaveFileSystem;
using UnityEngine;
using UnityEngine.UI;

public class ActivePowerSlot : MonoBehaviour
{
    public event Action<ActivePowerSlot> OnSlotFinished;

    [SerializeField] private Canvas Canvas;
    [SerializeField] private Image powerImage;
    [SerializeField] private Image powerFillImage;
    [SerializeField] private InventoryItemsMetaData inventoryItemsMetaData;
    [SerializeField] private GamePlaySessionData gamePlaySessionData;
    [SerializeField] private PlayerSharedData playerSharedData;
    public ActivePowerSlowState State { get; private set; } = ActivePowerSlowState.Free;

    private Tween fillTween;
    private Coroutine distanceBasedTween;

    private InventoryItemSO currentPower;

    private void OnDestroy()
    {
        fillTween?.Kill();

        if (distanceBasedTween != null)
        {
            CoroutineRunner.Instance.StopCoroutine(distanceBasedTween);
        }

        PowerUpsChannel.OnPowerupDeactivated -= DeactivateIfCurrentPowerHasEnded;
    }

    public void ActivateTheSlot(InventoryItemSO power, float duration, bool isDistanceBased = false, bool usePlayerCarTDistance = false)
    {
        PowerUpsChannel.OnPowerupDeactivated += DeactivateIfCurrentPowerHasEnded;

        currentPower = power;
        State = ActivePowerSlowState.Taken;

        InventoryItemMeta meta = inventoryItemsMetaData.GetInventoryItemMeta(power);
        powerImage.sprite = meta.Sprite;
        powerFillImage.fillAmount = 1f;

        Canvas.enabled = true;

        if (!isDistanceBased)
        {
            fillTween = powerFillImage.DOFillAmount(0, duration).OnComplete(() =>
            {
                DeactivateTheSlot();
            });
        }
        else
        {
            distanceBasedTween = CoroutineRunner.Instance.StartCoroutine(FillPowerBasedOnDistance(duration, usePlayerCarTDistance));
        }
    }

    private IEnumerator FillPowerBasedOnDistance(float targetDistance, bool usePlayerCarTDistance = false)
    {
        float initialDistance = !usePlayerCarTDistance ? gamePlaySessionData.DistanceCoveredInMeters : playerSharedData.PlayerTransform.position.z;
        float distanceCovered = 0;
        float currentDistance = initialDistance;

        while (distanceCovered < targetDistance)
        {
            if (!usePlayerCarTDistance)
            {
                currentDistance = gamePlaySessionData.DistanceCoveredInMeters;
                distanceCovered = currentDistance - initialDistance;
            }
            else
            {
                float curPlayerPosZ = playerSharedData.PlayerTransform.position.z;

                if(curPlayerPosZ > currentDistance)
                {
                    float diff = curPlayerPosZ - currentDistance;
                    distanceCovered += diff;
                }

                currentDistance = curPlayerPosZ;
            }

            yield return null;

            float ratio = distanceCovered / targetDistance;
            float fill = 1 - ratio;
            powerFillImage.fillAmount = fill;
        }

        DeactivateTheSlot();
        distanceBasedTween = null;
    }

    private void DeactivateTheSlot()
    {
        PowerUpsChannel.OnPowerupDeactivated -= DeactivateIfCurrentPowerHasEnded;
        State = ActivePowerSlowState.Free;
        Canvas.enabled = false;
        fillTween?.Kill();
        if (distanceBasedTween != null)
        {
            CoroutineRunner.Instance.StopCoroutine(distanceBasedTween);
        }
        OnSlotFinished?.Invoke(this);
    }

    private void DeactivateIfCurrentPowerHasEnded(InventoryItemSO power)
    {
        if (power != currentPower)
            return;

        DeactivateTheSlot();
    }
}