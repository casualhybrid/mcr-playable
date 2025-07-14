using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharactersDataBase", menuName = "ScriptableObjects/StaticData/Characters/CharactersDataBase")]
public class CharactersDataBase : SerializedScriptableObject
{
    [SerializeField] private Dictionary<int, CharacterConfigData> charactersDataBaseDictionary;

    public Dictionary<int, CharacterConfigData> CharactersDataBaseDictionary => charactersDataBaseDictionary;

    private void OnEnable()
    {
        foreach (var pair in charactersDataBaseDictionary)
        {
            pair.Value.IndexKey = pair.Key;
        }
    }

    public CharacterConfigData GetCharacterConfigurationData(int characterIndexKey)
    {
        CharacterConfigData characterConfigurationData;

        charactersDataBaseDictionary.TryGetValue(characterIndexKey, out characterConfigurationData);

        if (characterConfigurationData == null)
            throw new System.Exception($"Failed to find character configuration data for the requested keyIndex {characterIndexKey}");

        return characterConfigurationData;
    }

    public int GetIndexOfTheCharacterFromItsConfigData(CharacterConfigData config)
    {
        if(config.IndexKey == -1)
            throw new System.Exception("index key for the supplied character config is not set: " + config.GetName);

        return config.IndexKey;

        //foreach (var pair in charactersDataBaseDictionary)
        //{
        //    if (pair.Value != config)
        //        continue;

        //    return pair.Key;
        //}

        //throw new System.Exception("Failed to find index for the supplied character config " + config.GetName);
    }
}