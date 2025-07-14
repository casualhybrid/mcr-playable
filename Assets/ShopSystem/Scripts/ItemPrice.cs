using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct PriceOfItem
{
    [SerializeField] private InventoryItemSO itemType;
    [SerializeField] private int amount;

    /// <summary>
    /// The Type Of Inventory Item
    /// </summary>
    public InventoryItemSO GetItemType => itemType;

    /// <summary>
    /// The Value Of Inventory Item
    /// </summary>
    public int GetAmount => amount;
}

[CreateAssetMenu(fileName = "ItemPrice", menuName = "ScriptableObjects/ItemPrice")]
public class ItemPrice : ScriptableObject
{
    [System.Serializable]
    public enum CurrencyMode
    {
        Single, Multiple
    }

    [InfoBox("In single mode, the order of inventory item is taken into account")]
    [EnumToggleButtons] [SerializeField] private CurrencyMode currencyMode;


    [Space]
    [SerializeField] private List<PriceOfItem> pricesList;

    /// <summary>
    /// Get the currency mode for the respective inventory item
    /// </summary>
    public CurrencyMode GetCurrencyMode => currencyMode;

    /// <summary>
    /// The list of currencies for the given item
    /// </summary>
    public List<PriceOfItem> GetPricesList => pricesList;
}