using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "BlackListedChipset", menuName = "ScriptableObjects/BlackListedChipset")]
public class BlackListedChipset : ScriptableObject
{
    [SerializeField] public List<string> BlackListedChipsetList;

#if UNITY_EDITOR
    [Button("GenerateJSON")]
    public void ConvertToJson()
    {
        var json = JsonUtility.ToJson(this);

        UnityEngine.Console.Log(json);
    }
#endif
}
