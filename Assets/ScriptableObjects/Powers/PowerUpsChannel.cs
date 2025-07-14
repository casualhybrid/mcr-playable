using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PowerUpsChannel
{
    public static event Action<InventoryItemSO,float,bool,bool> OnPowerupActivated;
    public static event Action<InventoryItemSO> OnPowerupDeactivated;

    public static void RaisePowerActivatedEvent(InventoryItemSO power, float powerDuration, bool isDistanceBased = false, bool usePlayerCarTDistance = false)
    {
        OnPowerupActivated?.Invoke(power, powerDuration, isDistanceBased, usePlayerCarTDistance);  
    }

    public static void RaisePowerDeactivatedEvent(InventoryItemSO power)
    {
        OnPowerupDeactivated?.Invoke(power);
    }
}
