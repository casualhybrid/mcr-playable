using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerDoubleBoostState", menuName = "ScriptableObjects/PlayerDoubleBoostState")]
public class PlayerDoubleBoostState : ScriptableObject
{
    [SerializeField] private GamePlaySessionInventory gamePlaySessionInventory;
    [SerializeField] private InventorySystem inventoryObj;
    [SerializeField] private PlayerSharedData PlayerSharedData;
    [SerializeField] private PlayerContainedData PlayerContainedData;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private CameraShakeVariations cameraShakeVariations;
    [SerializeField] private SpecialPickupsEnumSO specialPickupsEnumSO;

    [SerializeField] private GameEvent playerStartedBoostingEvent;
    [SerializeField] private GameEvent playerFinishedBoostingEvent;
    [SerializeField] private PowerUpDurationManager powerUpDurationManager;

    private float duration;//duration for Dash

    //onenter is like start wich will be called once when boost starts
    private void OnEnter()
    {
        //PlayerSharedData.IsDash = false;
        //PlayerSharedData.HalfDashCompleted = true;
        if (PlayerSharedData.IsDash)
        {
            PlayerContainedData.PlayerBoostState.StopDash(0, true);
        }
        duration = powerUpDurationManager.GetPowerDurationForCar(specialPickupsEnumSO.BoostPickup);
        gameManager.Invincible = true;

        //  PlayerSharedData.PlayerHitCollider.center = PlayerContainedData.PlayerData.PlayerInformation[0].HurtColliderCenterDuringBoost;
        // PlayerSharedData.PlayerHitCollider.size = PlayerContainedData.PlayerData.PlayerInformation[0].HurtColliderSizeDuringBoost;
    }

    public void Boost()
    {
        duration -= Time.deltaTime;

        if (duration <= 0)
        {
            StopBoost();
        }
    }

    public void StartBoost()
    {
        bool hasBoostInInventory = inventoryObj.GetIntKeyValue("GameBoost") > 0;
        bool hasBoostInSessionInventory = gamePlaySessionInventory.GetIntKeyValue("GameBoost") > 0;

        if (!PlayerSharedData.IsBoosting && (hasBoostInInventory || hasBoostInSessionInventory))
        {
            AnalyticsManager.CustomData("GamePlayScreen_PlayerUsedBoost");

            OnEnter();
            PlayerSharedData.IsBoosting = true;
            PlayerContainedData.SpeedHandler.ChangeGameTimeScaleInTime(PlayerContainedData.PlayerData.PlayerInformation[0].Boosterspeed, 2f, true, true);

            if (hasBoostInInventory)
            {
                inventoryObj.UpdateKeyValues(new List<InventoryItem<int>>() { new InventoryItem<int>("GameBoost", -1) });
            }
            else
            {
                gamePlaySessionInventory.AddThisKeyToGamePlayInventory("GameBoost", -1);
            }

            cameraShakeVariations.OnShakeEnded.AddListener(HandleShakeEnded);
            playerStartedBoostingEvent.RaiseEvent();
            PowerUpsChannel.RaisePowerActivatedEvent(specialPickupsEnumSO.BoostPickup as InventoryItemSO, duration);
        }
     //   else
     //   {
           // UnityEngine.Console.LogWarning("Boost is already started.");
     //   }
    }

    public void StopBoost(float stopBoostEarly = 2.5f)  
    {
        if (!PlayerSharedData.IsBoosting)
            return;

        // PlayerSharedData.PlayerHitCollider.size = hurtColliderSizeDefault;
        // PlayerSharedData.PlayerHitCollider.center = hurtColliderCenterDefault;
        gameManager.Invincible = gameManager.IsForcedInvincible;
        PlayerContainedData.SpeedHandler.RemoveOverrideGameTimeScaleMode(stopBoostEarly);
        PlayerSharedData.IsBoosting = false;

        cameraShakeVariations.OnShakeEnded.RemoveListener(HandleShakeEnded);
        PowerUpsChannel.RaisePowerDeactivatedEvent(specialPickupsEnumSO.BoostPickup as InventoryItemSO);
        playerFinishedBoostingEvent.RaiseEvent();
    }

    private void HandleShakeEnded()
    {
        if (PlayerSharedData.IsBoosting)
            cameraShakeVariations.ShakeCameraAccordingToEvent(playerStartedBoostingEvent);
        else
            cameraShakeVariations.OnShakeEnded.RemoveListener(HandleShakeEnded);
    }
}