using UnityEngine;

[CreateAssetMenu(fileName = "CharacterConfigData", menuName = "ScriptableObjects/StaticData/Characters/CharacterConfigData")]
public class CharacterConfigData : ScriptableObject, IBasicAssetInfo
{
    [SerializeField] private string characterName;
    [SerializeField] private string characterKey;
    [SerializeField] private string characterDisplayKey;
    [SerializeField] private int figurinesToUnlock;

    public int IndexKey { get; set; } = -1;
    public string GetName => characterName;

    public string GetLoadingKeyGamePlay => characterKey;

    public string GetLoadingKeyDisplay => characterDisplayKey;

    public int FigurinesToUnlock => figurinesToUnlock;
}