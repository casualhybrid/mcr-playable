using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "CelebrationEvent", menuName = "PlayerInventorySystem/CelebrationEvent", order = 1)]
public class InventoryUpdateEvent : ScriptableObject
{
    public UnityAction<string, int> celebrationEvent;
    public UnityEvent<List<InventoryItem<int>>, string, bool> celebrationBatchAdded = new UnityEvent<List<InventoryItem<int>>, string, bool>();

    public void RaiseEvent(string eventName, int eventValue)
    {
        if (celebrationEvent == null)
        {
            UnityEngine.Console.LogWarning($"No subscribers for celebration event {eventName} ");
        }

        celebrationEvent?.Invoke(eventName, eventValue);
    }


    public void RaiseBatchOfItemsToBeCelebratedAdded(List<InventoryItem<int>> batch, string context, bool isDoubleRewardPossible = true)
    {
        celebrationBatchAdded.Invoke(batch, context, isDoubleRewardPossible);
    }
}