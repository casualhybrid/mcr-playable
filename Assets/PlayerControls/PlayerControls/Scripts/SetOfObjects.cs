using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SetOfObjects<T> : ScriptableObject
{
    [SerializeField] protected List<T> TheList = new List<T>();
    public List<T> GetList => TheList;

    public void AddItemToList(T item)
    {
        TheList.Add(item);
    }

    public void RemoveItemFromList(T item)
    {
        try
        {
            TheList.Remove(item);
        }
        catch
        {
            UnityEngine.Console.LogWarning($"Trying to remove item from set of object which is not present. {this.name}");
        }
    }

    public void ClearTheList()
    {
        TheList.Clear();
    }
}
