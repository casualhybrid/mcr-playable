using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CoinsSpawnInfo 
{
    public CoinsSpawnType coinsSpawnType;
    public List<Vector3> coinSpawnPoints;
    public InventoryItemSO pickupToSpawn;
    public bool isDoubleCoins;

    public CoinsSpawnInfo(CoinsSpawnType _coinsSpawnType, List<Vector3> _coinSpawnPoints, InventoryItemSO _pickupToSpawn, bool _isDoubleCoins)
    {
        coinsSpawnType = _coinsSpawnType;
        coinSpawnPoints = _coinSpawnPoints;
        pickupToSpawn = _pickupToSpawn;
        isDoubleCoins = _isDoubleCoins;
    }
}
