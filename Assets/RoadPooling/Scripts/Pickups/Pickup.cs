using System;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField] protected InventoryItemSO pickupType;

    public event Action<Transform> OnPickupFinished;

    public InventoryItemSO GetPickupType => pickupType;

    protected void RaisePickupFinishedEvent()
    {
        OnPickupFinished?.Invoke(this.transform);
    }

    public virtual void MarkPickupAsFinished()
    {
        if (OnPickupFinished == null)
        {
            if(pickupType.GetKey== "AeroPlane")
            {
                Debug.LogError("Pickup "+ pickupType.GetKey);
            }
            this.gameObject.SetActive(false);
        }

        OnPickupFinished?.Invoke(this.transform);
    }
}