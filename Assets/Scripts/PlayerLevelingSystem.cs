using System;
using System.Collections.Generic;
using TheKnights.SaveFileSystem;
using UnityEngine;


[CreateAssetMenu(fileName = "PlayerLevelingSystem", menuName = "ScriptableObjects/PlayerLevelingSystem")]
public class PlayerLevelingSystem : ScriptableObject
{
    public event Action PlayerLevelIncreased;

    [SerializeField] private PlayerSharedData PlayerSharedData;
    [SerializeField] private PlayerXPLevelPerCar PlayerXPLevelPerCar;
    [SerializeField] private GamePlaySessionData gamePlaySessionData;
    [SerializeField] private GamePlaySessionInventory gamePlaySessionInventory;

    // [SerializeField] private GameEvent gameHasSarted;
    //  [SerializeField] private GameEvent playerHasCrahsed;
    [SerializeField] private SaveManager saveManager;

    [SerializeField] private float exponentialPower;

    //  private Vector3 carStartingPosition;
    private float xp;

    // private float totalDistanceCovered;

    //private void OnEnable()
    //{
    //    //    UnityEngine.SceneManagement.SceneManager.activeSceneChanged += (scene, mode) => { ResetVariable(); };
    //    gameHasSarted.TheEvent.AddListener(SavePlayerStartingPosition);
    //    //    playerHasCrahsed.TheEvent.AddListener(CalculateDisctanceBasedXP);
    //}

    //private void OnDisable()
    //{
    //    gameHasSarted.TheEvent.RemoveListener(SavePlayerStartingPosition);
    //    //  playerHasCrahsed.TheEvent.RemoveListener(CalculateDisctanceBasedXP);
    //}

    //private void SavePlayerStartingPosition(GameEvent theEvent)
    //{
    //    carStartingPosition = PlayerSharedData.PlayerTransform.position;
    //}

    public int GetCurrentPlayerLevelForDistanceCovered()
    {
        int playerLevel = gamePlaySessionInventory.PlayerCurrentLevel;
        float totalDistanceCovered = gamePlaySessionData.DistanceCoveredInMeters;
        float _xp = (totalDistanceCovered * PlayerXPLevelPerCar.XPLevelPerKm["PlayerCar1"].xp) / PlayerXPLevelPerCar.XPLevelPerKm["PlayerCar1"].distanceInUnits;
        float playerXP = gamePlaySessionInventory.PlayerCurrentXP + _xp;

        float xpNeededForNextLevel = PlayerXPLevelPerCar.XPLevelPerKm["PlayerCar1"].xpRequiredInitial * Mathf.Pow(gamePlaySessionInventory.PlayerCurrentLevel, exponentialPower);

        while (playerXP > xpNeededForNextLevel)
        {
            playerLevel++;
            playerXP -= xpNeededForNextLevel;

            xpNeededForNextLevel = PlayerXPLevelPerCar.XPLevelPerKm["PlayerCar1"].xpRequiredInitial * Mathf.Pow(gamePlaySessionInventory.PlayerCurrentLevel, exponentialPower);
        }

        return playerLevel;
    }

    public void CalculateDisctanceBasedXP(/*GameEvent theEvent*/)
    {
        //totalDistanceCovered = Vector3.Distance(carStartingPosition, PlayerSharedData.PlayerTransform.position);
        float totalDistanceCovered = gamePlaySessionData.DistanceCoveredInMeters;
        xp = (totalDistanceCovered * PlayerXPLevelPerCar.XPLevelPerKm["PlayerCar1"].xp) / PlayerXPLevelPerCar.XPLevelPerKm["PlayerCar1"].distanceInUnits;

        //  saveManager.MainSaveFile.PlayerCurrentXP += xp;
        gamePlaySessionInventory.PlayerCurrentXP += xp;

        //  UnityEngine.Console.Log("totalDistanceCovered" + totalDistanceCovered);

        CalculatePlayerLevel();
    }

    private void CalculatePlayerLevel()
    {
        //  float xpNeededForNextLevel = PlayerXPLevelPerCar.XPLevelPerKm["PlayerCar1"].xp * Mathf.Pow(saveManager.MainSaveFile.PlayerCurrentLevel, exponentialPower);
        //  UnityEngine.Console.Log("xpNeededForNextLevel" + xpNeededForNextLevel);
        float xpNeededForNextLevel = PlayerXPLevelPerCar.XPLevelPerKm["PlayerCar1"].xpRequiredInitial * Mathf.Pow(gamePlaySessionInventory.PlayerCurrentLevel, exponentialPower);

        if (gamePlaySessionInventory.PlayerCurrentXP > xpNeededForNextLevel)
        {
            gamePlaySessionInventory.PlayerCurrentLevel += 1;
            gamePlaySessionInventory.PlayerCurrentXP -= xpNeededForNextLevel;

            PlayerLevelIncreased?.Invoke();

            CalculatePlayerLevel();
        }

        int playerOldLevel = PlayerPrefs.GetInt("PlayerOldLevel", 0);
        if (playerOldLevel < gamePlaySessionInventory.PlayerCurrentLevel)
        {
            //Player level is upgraded
            PlayerPrefs.SetInt("PlayerOldLevel", gamePlaySessionInventory.PlayerCurrentLevel);
            AnalyticsManager.CustomData("GamePlayScreen_GameOverPanel_PlayerLevelUP", new Dictionary<string, object>
            {
                { "PlayerCurrentLevel", gamePlaySessionInventory.PlayerCurrentLevel },
                { "PlayerLevelIncrementedBy", gamePlaySessionInventory.PlayerCurrentLevel - playerOldLevel}
            });

        }
    }

    public float GetXPNeededForNextLevel()
    {
        float xpNeededForNextLevel = PlayerXPLevelPerCar.XPLevelPerKm["PlayerCar1"].xpRequiredInitial * Mathf.Pow(saveManager.MainSaveFile.PlayerCurrentLevel, exponentialPower);
        return xpNeededForNextLevel;
    }

    //public void IncreasePlayerXP(float xp)
    //{
    //}

    public float GetPlayerCurrentXP()
    {
        return saveManager.MainSaveFile.PlayerCurrentXP;
    }

    public int GetCurrentPlayerLevel()
    {
        return saveManager.MainSaveFile.PlayerCurrentLevel;
    }
}