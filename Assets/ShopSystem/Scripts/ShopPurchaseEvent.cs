using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "ShopPurchaseEvent", menuName = "ShopSystem/ShopPurchaseEvent", order = 1)]
public class ShopPurchaseEvent : ScriptableObject
{
    public UnityAction<List<string>, string, int, List<int>> PurchaseEvent;
    
    public void RaiseEvent(List<string> itemsBought, string itemSpent, int spentMoney, List<int> amountsGot)
    {
        if (PurchaseEvent == null)
        {
            UnityEngine.Console.LogWarning($"No subscribers for purchase event {itemsBought} ");
        }
        PurchaseEvent?.Invoke(itemsBought, itemSpent, spentMoney, amountsGot);
        
    }
}