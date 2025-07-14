using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActivePowerSlowState
{
    Taken, Free
}


public class ActivePowerUIManager : MonoBehaviour
{
    // bottom to top order
    [SerializeField] private ActivePowerSlot[] activePowerSlots;

    private void Awake()
    {
        PowerUpsChannel.OnPowerupActivated += DisplayActivatedPowerUpUI;
    }

    private void OnDestroy()
    {
        PowerUpsChannel.OnPowerupActivated -= DisplayActivatedPowerUpUI;
    }

    private void DisplayActivatedPowerUpUI(InventoryItemSO power, float duration, bool isDistanceBased = false, bool usePlayerCarTDistance = false)
    {
        ActivePowerSlot freeSlot = null;

        for (int i = 0; i < activePowerSlots.Length; i++)
        {
            ActivePowerSlot slot = activePowerSlots[i];

            if (slot.State != ActivePowerSlowState.Free)
                continue;

            freeSlot = slot;
            break;
        }

        if(freeSlot == null)
        {
            throw new System.Exception("Failed to find a free active powerup slot");
        }

        freeSlot.ActivateTheSlot(power, duration, isDistanceBased, usePlayerCarTDistance);

    }

}
