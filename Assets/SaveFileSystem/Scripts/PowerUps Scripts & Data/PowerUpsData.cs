using UnityEngine;

[CreateAssetMenu(fileName = "PowerUpData", menuName = "The Knights UI/PowerUpData")]

public class PowerUpsData : ScriptableObject
{
    public PowerType ThisPowerUp;
    public PowerUpDetail[] PowerUps;
}

// This will help to choose what type of power Up it is
public enum PowerType 
{ 
    Dash, 
    Boost, 
    DoubleDash,
    Magnet
}

//To add power Details
[System.Serializable]
public class PowerUpDetail
{
    [Tooltip("Price to Purchase this PowerUp")]
    public int Price;

    [Tooltip("Level/Number of this power PowerUp")]
    public int powerUpLevel;

    [Tooltip("Duration in Seconds")]
    public int Duration;
}