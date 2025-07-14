using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;
using TheKnights.SaveFileSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "GamePlayBackGroundRewardsData", menuName = "ScriptableObjects/StaticData/GamePlayRewardData")]
public class GamePlayBackGroundRewardsData : ScriptableObject
{
    [SerializeField] private SaveManager saveManager;

    [System.Serializable]
    [SerializeField]
    private class TimeToRewardDictionary : SerializableDictionaryBase<float, ItemWithAmount>
    { }

    [SerializeField] private TimeToRewardDictionary timeToRewardDictionary;

    public bool isPlayerObtainedAllBackGroundRewards()
    {
        HashSet<float> userRewardAcquired = saveManager.MainSaveFile.gamePlayBackGroundAwardsAcquired;
        bool allRewardsObtained = true;

        foreach (var item in timeToRewardDictionary)
        {
            if (!userRewardAcquired.Contains(item.Key))
            {
                allRewardsObtained = false;
                break;
            }
        }

        return allRewardsObtained;
    }

    public ICollection<float> GetDictionaryKeys()
    {
        return timeToRewardDictionary.Keys;
    }

    public ItemWithAmount GetRewardItemForKey(float key)
    {
        return timeToRewardDictionary[key];
    }
}