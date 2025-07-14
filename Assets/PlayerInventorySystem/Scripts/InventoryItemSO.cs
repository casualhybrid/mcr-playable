using UnityEngine;

[CreateAssetMenu(fileName = "InventoryItem", menuName = "ScriptableObjects/InventoryItem")]
public class InventoryItemSO : ScriptableObject
{
    [SerializeField] private string key;

    public string GetKey => key;
}