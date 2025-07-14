using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CustomEncounterObstacleMetaData
{
    [SerializeField] public string nameOfTheObstacle;
    [SerializeField] public GameObject OriginalPrefab;
    [SerializeField] public Vector3 localPosition;
    [SerializeField] public Vector3 localRotationEulers;
    [SerializeField] public Transform ParentTransform;
    [SerializeField] public int ChildOrder;
}

[System.Serializable]
public class CustomEncounterObjectDestructionMetaData
{
    [SerializeField] public int ParentCachedInstanceID;
    [SerializeField] public Vector3 OriginalWordPosition;
}

[System.Serializable]
public class CustomEncounterSyncedObstacleMetaData
{
    [SerializeField] public int ParentCachedInstanceID;
    [SerializeField] public Vector3 OriginalWordPosition;
}

[System.Serializable]
public class ParentAndChildOrderMeta
{
    [SerializeField] public Transform ParentT;
    [SerializeField] public int ChildOrder;
}

[System.Serializable]
public class ActionTimeSubjectFirstLastGroupMetaData
{
    [SerializeField] public ParentAndChildOrderMeta firstObjectGroupMeta;
    [SerializeField] public ParentAndChildOrderMeta lastObjectGroupMeta;
}

[System.Serializable]
public class ActionTimeSubjectMetaData
{
    [SerializeField] public CustomEncounterObjectDestructionMetaData[] actionTimeSubjectsChildObjectDestructions;
    [SerializeField] public ActionTimeSubjectFirstLastGroupMetaData actionTimeSubjectFirstLastGroupMetaData;

}

[System.Serializable]
public class ShuffleObstaclePairUniqueEmptyLaneMetaData
{
    [SerializeField] public Dictionary<Transform, List<ParentAndChildOrderMeta>> shuffleObstaclePairData;
}


public class CustomEncounterSkeleton : SerializedMonoBehaviour, IFloatingReset
{
    public event Action OnObstaclesSpawnedFromMetaData;

    [SerializeField] public CustomEncounterObstacleMetaData[] customEncounterObstacleMetaDatas;
    [SerializeField] public CustomEncounterObjectDestructionMetaData[] customEncounterObjectDestructionMetaData;
    [SerializeField] public CustomEncounterSyncedObstacleMetaData[] customEncounterSyncedObstacleMetaData;
    [SerializeField] public Dictionary<ActionTimePreserverSubject, ActionTimeSubjectMetaData> actionTimeSubjectsChildObjectDestructions;
    [SerializeField] public Dictionary<ActionTimePreserver, CustomEncounterSyncedObstacleMetaData[]> actionTimePreserversChildSyncedObstacles;
    [SerializeField] public Dictionary<ShuffleObstaclePairsToProvideUniqueEmptyLanes, ShuffleObstaclePairUniqueEmptyLaneMetaData> shuffleObstaclePairData;

    [SerializeField] private ObstaclePoolSO obstaclePool;

    public bool ShoudNotOffsetOnRest { get; set; }

    private void OnEnable()
    {
       // UnityEngine.Console.Log("MetaData Set");
        SpawnObstaclesFromTheMetaData();
    }

    public void SpawnObstaclesFromTheMetaData()
    {
        for (int i = 0; i < customEncounterObstacleMetaDatas.Length; i++)
        {
            var metaDataEntity = customEncounterObstacleMetaDatas[i];
            Obstacle obstacle = metaDataEntity.OriginalPrefab.GetComponent<Obstacle>();
            Obstacle instantiatedObstacle = obstaclePool.Request(obstacle, obstacle.InstanceID);
            instantiatedObstacle.GetComponent<SpringRampHandler>()?.InitializeSpringRamp();
            instantiatedObstacle.IsThisObstaclePartOfCustomEncounter = true;
            instantiatedObstacle.ShoudNotOffsetOnRest = true;

            Transform obstacleT = instantiatedObstacle.transform;
            obstacleT.SetParent(metaDataEntity.ParentTransform);
            obstacleT.SetSiblingIndex(metaDataEntity.ChildOrder);
            obstacleT.localPosition = metaDataEntity.localPosition;
            obstacleT.localRotation = Quaternion.Euler(metaDataEntity.localRotationEulers);
        }

        OnObstaclesSpawnedFromMetaData?.Invoke();
    }

    public void OnFloatingPointReset(float movedOffset)
    {
       // throw new NotImplementedException();
    }

    public void OnBeforeFloatingPointReset()
    {
       // throw new NotImplementedException();
    }
}