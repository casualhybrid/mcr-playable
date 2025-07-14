using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class SyncedObstacleColumn : MonoBehaviour, IValidatable, ICustomEncounterEntity
{
    [SerializeField] private int syncOffset;
    [ReadOnly] public List<SyncedObstacle> synchronizedObstacles = new List<SyncedObstacle>();
    private int previousUpdateSyncedObstacleCount;

    [ReadOnly] [SerializeField] private CustomEncounter _customEncounter;

    public CustomEncounter CustomEncounter
    {
        get
        {
            if (_customEncounter == null)
            {
                _customEncounter = GetComponentInParent<CustomEncounter>();

#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
                PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif
            }

            return _customEncounter;
        }

        set
        {
            _customEncounter = value;
        }
    }

    private void Awake()
    {
        if (_customEncounter == null)
        {
            _customEncounter = GetComponentInParent<CustomEncounter>();
        }
    }

    public void SpawnObstacles(GameObject[] possibleObstacles, int syncObstacleIndex)
    {
        if (synchronizedObstacles.Count == 0)
        {
            UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. Obstacles to synchronize count is zero. Column will not be considered by Obstacle Synchronizer.");
        }

        int obstacleToSpawnIndex = syncObstacleIndex + syncOffset;

        foreach (SyncedObstacle syncedObstacle in synchronizedObstacles)
        {
            if (obstacleToSpawnIndex > possibleObstacles.Length - 1)
            {
                obstacleToSpawnIndex = 0;
            }

            syncedObstacle.SpawnSyncedObstacle(possibleObstacles[obstacleToSpawnIndex]);

            obstacleToSpawnIndex++;
        }
    }

    public void ValidateAndInitialize()
    {
        int syncedObstacleCount = GetComponentsInChildren<SyncedObstacle>().Length;

        if (syncedObstacleCount == 0)
        {
            UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. Obstacles to synchronize count is zero. Column will not be considered by Obstacle Synchronizer.");
        }

        if (previousUpdateSyncedObstacleCount != syncedObstacleCount || !AreAllChildrenAlreadyInSynchronizedObstaclesList())
        {
            previousUpdateSyncedObstacleCount = syncedObstacleCount;

            synchronizedObstacles.Clear();

            synchronizedObstacles = GetComponentsInChildren<SyncedObstacle>().ToList();
        }
    }

    private bool AreAllChildrenAlreadyInSynchronizedObstaclesList()
    {
        List<SyncedObstacle> currentSynchronizedObstacles = GetComponentsInChildren<SyncedObstacle>().ToList();

        for (int i = 0; i < currentSynchronizedObstacles.Count; i++)
        {
            if (!synchronizedObstacles.Contains(currentSynchronizedObstacles[i]))
                return false;
        }

        return true;
    }

#if UNITY_EDITOR

    private void Update()
    {
        if (Application.isPlaying)
            return;

        if (CustomEncounter.IsSkeleton)
            return;

        ValidateAndInitialize();
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

        if (_customEncounter != null && _customEncounter.IsSkeleton)
            return;

        ValidateAndInitialize();
    }

#endif
}