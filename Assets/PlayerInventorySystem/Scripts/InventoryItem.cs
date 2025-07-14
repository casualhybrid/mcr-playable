using MessagePack;
using System.Collections.Generic;

[MessagePackObject]
public class InventoryItem<T>
{
    [Key(0)] [ReadOnly] public string itemName;
    [Key(1)] public T itemValue;

    [IgnoreMember] public /*readonly*/ bool isCelebrationAddition { get; set; }

    public InventoryItem(string _itemName, T _itemValue, bool _isCelebration = false)
    {
        itemName = _itemName;
        itemValue = _itemValue;
        isCelebrationAddition = _isCelebration;
    }

    public InventoryItem()
    { }
}

[MessagePackObject]
[System.Serializable]
public class IntegerValueItem : InventoryItem<int> { }

[MessagePackObject]
[System.Serializable]
public class InventoryObject
{
    [Key(0)] public string name;
    [Key(2)] public bool isObjectUnlocked;
    [Key(3)] public List<IntegerValueItem> properties;
}