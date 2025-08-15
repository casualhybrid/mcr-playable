using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialPickup : Pickup, IFloatingReset
{
    public bool ShoudNotOffsetOnRest { get; set; } = true;

    public bool isSafeToDisable { get; private set; } = true;

    [SerializeField] protected Objectset PickupSet;
    [SerializeField] private GameEvent specialPickupIsSpawned;

    private Vector3 specialPickupSpawnPosition;
    public Vector3 GetSpecialPickupSpawnPosition => specialPickupSpawnPosition;

    protected void OnEnable()
    {
        isSafeToDisable = true;
        PickupSet.AddItemToList(this.gameObject);
    }

    private void OnDisable()
    {
        PickupSet.RemoveItemFromList(this.gameObject);
    }

    private void Start()
    {
        specialPickupSpawnPosition = transform.position;
      
    }

    public void RaiseSpecialPickupSpawnedEvent()
    {
       
        specialPickupIsSpawned.RaiseEvent();
    }
  

    public override void MarkPickupAsFinished()
    {
        RaisePickupFinishedEvent();

        Destroy(this.gameObject);
    }

    public void SetSafeToDisable()
    {
        isSafeToDisable = true;
    }

    public void SetNotSafeToDisable()
    {
        isSafeToDisable = false;
    }

    public void OnBeforeFloatingPointReset()
    {

    }


    public void OnFloatingPointReset(float movedOffset)
    {
      
    }
}
