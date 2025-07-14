using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    [SerializeField] private GameEvent magnetPickedUp;


    private Dictionary<GameEvent, PowerUp> powerDictionary = new Dictionary<GameEvent, PowerUp>();
    private List<PowerUp> activePowerUps = new List<PowerUp>();

    private void Awake()
    {
        CreatePowerDictionary();

        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        DeSubscribeToEvents();
    }

    private void CreatePowerDictionary()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform t = transform.GetChild(i);
            PowerUp powerUp = t.gameObject.GetComponent<PowerUp>();

            if (powerUp != null)
            {
                powerDictionary.Add(powerUp.PowerUpPickedEvent, powerUp);
            }
            else
            {
                UnityEngine.Console.LogWarning($"PowerUpManager child {t.name} has no PowerUp behaviour attached");
            }
        }
    }

    private void SubscribeToEvents()
    {
        magnetPickedUp.TheEvent.AddListener(HandlePowerUpPicked);
    }

    private void DeSubscribeToEvents()
    {
        magnetPickedUp.TheEvent.RemoveListener(HandlePowerUpPicked);
    }

    private void HandlePowerUpPicked(GameEvent theEvent)
    {
        PowerUp powerUp = powerDictionary[magnetPickedUp];
        if (activePowerUps.Contains(powerUp))
        {
            return;
        }

        powerUp.SetUp();
        activePowerUps.Add(powerUp);
    }

    private void Update()
    {
        if (activePowerUps.Count == 0)
        {
            return;
        }

        for (int i = 0; i < activePowerUps.Count; i++)
        {
            PowerUp powerUp = activePowerUps[i];
            powerUp.Execute();

            if (powerUp.isDone)
            {
                activePowerUps.Remove(powerUp);
                i--;
            }
        }
    }
}