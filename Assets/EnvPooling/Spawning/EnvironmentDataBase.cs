using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "EnvironmentDataBase", menuName = "ScriptableObjects/StaticData/Environment/EnvironmentDataBase")]
public class EnvironmentDataBase : SerializedScriptableObject
{
    [System.Serializable]
    private class EnvironmentMetaData
    {
        [SerializeField] private string loadingPath;

        public string GetLoadingPath => loadingPath;
    }

    [SerializeField] private Dictionary<Environment, EnvironmentMetaData> environmentsDictionary;


    public string GetLoadingPathForEnvironment(Environment enumSO)
    {
        EnvironmentMetaData environmentMetaData;
        environmentsDictionary.TryGetValue(enumSO, out environmentMetaData);

        if(environmentMetaData == null)
        {
            throw new System.Exception($"Failed getting environment path for {enumSO.name}");
        }

        return environmentMetaData.GetLoadingPath;
    }
  
}
