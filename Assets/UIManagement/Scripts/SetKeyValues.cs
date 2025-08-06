using UnityEngine;


public class SetKeyValues : MonoBehaviour
{
    // [SerializeField] private InventorySystem inventorySystemObj;
    [SerializeField] protected GamePlaySessionInventory gamePlaySessionInventory;

    [SerializeField] private InteractionBasedEventTypes eventType;
    [SerializeField] protected string triggerTag, keyValue;
    [SerializeField] protected int rewardValue;

    private void OnTriggerEnter(Collider other)
    {
        if (eventType == InteractionBasedEventTypes.OnTriggerEnter)
        {
            if (other.gameObject.CompareTag(triggerTag))
                GiveReward();
        }
    }

    public virtual void GiveReward()
    {
        gamePlaySessionInventory.AddThisKeyToGamePlayInventory(keyValue, rewardValue);
    }

    public void SendEvent(string eventName)
    {
        AnalyticsManager.CustomData(eventName);
    }
}

/// <summary>
/// Dont change the pattern of these Enum and add new Enum at the end
/// </summary>
public enum InventoryKeyType
{
    CurrencyValue, IntValue
};