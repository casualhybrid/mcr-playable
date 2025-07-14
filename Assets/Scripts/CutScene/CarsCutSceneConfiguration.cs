using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class CutSceneConfiguration
{
    [SerializeField] private Vector3 playerCarStartingPosition;
    [SerializeField] private Quaternion playerCarStartingRotation;

    [SerializeField] private Vector3 chaserCarStartingPosition;
    [SerializeField] private Quaternion chaserCarStartingRotation;

    public Vector3 GetPlayerCarStartingPosition => playerCarStartingPosition;
    public Quaternion GetPlayerCarStartingRotation => playerCarStartingRotation;

    public Vector3 GetChaserCarStartingPosition => chaserCarStartingPosition;
    public Quaternion GetChaserCarStartingRotation => chaserCarStartingRotation;
}

[CreateAssetMenu(fileName = "CutSceneConfiguration", menuName = "ScriptableObjects/CutScene/CutSceneConfiguration")]
public class CarsCutSceneConfiguration : SerializedScriptableObject
{
 


    [SerializeField] private Dictionary<int, CutSceneConfiguration> cutSceneConfiguration;


    public CutSceneConfiguration GetCarConfiguration(int keyIndex)
    {
        CutSceneConfiguration configuration;
        cutSceneConfiguration.TryGetValue(keyIndex, out configuration);

        if(configuration == null)
        {
            throw new System.Exception(($"Failed getting cutscene configration for {keyIndex} car"));
        }

        return configuration;


    }
   
}
