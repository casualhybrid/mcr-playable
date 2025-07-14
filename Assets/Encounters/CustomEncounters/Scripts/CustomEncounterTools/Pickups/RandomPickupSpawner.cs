using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;

#if (UNITY_EDITOR)
using UnityEditor.SceneManagement;
#endif

[ExecuteInEditMode]
public class RandomPickupSpawner : MonoBehaviour/* , IValidatable */
{
    /* [Tooltip("Uses the same spawner which is used for procedural pickup spawning")]
    [SerializeField] private bool useDefaultPickups;
    [ShowIf("useDefaultPickups", false)] [SerializeField] private GameObject[] possiblePickups;
    
    [Header("References")]
    [SerializeField] private PickupGenerationData pickupGenerationData;

    private List<GameObject> PossiblePickups
    {
        get
        {
            if (useDefaultPickups)
            {
                return PickupSpawner.Instance.possiblePickups;
            }

            return possiblePickups.ToList();
        }
    }
    private bool isPickupSpawningDone = false;

    private void OnDrawGizmos()
    {
        if (isPickupSpawningDone)
            return;

        if (possiblePickups.Length == 0)
            return;

        Vector3 longestObstacleBoundsSize = Vector3.zero;
        Vector3 longestObstacleBoundsCenter = Vector3.zero;
        
        foreach(GameObject obstacle in possiblePickups)
        {
            MeshRenderer[] obstacleColliders = obstacle.GetComponentsInChildren<MeshRenderer>();
            Bounds obstacleBounds = new Bounds(obstacle.transform.position, Vector3.zero);

            foreach (MeshRenderer collider in obstacleColliders)
            {
                if (obstacleBounds.extents == Vector3.zero)
                    obstacleBounds = collider.bounds;

                obstacleBounds.Encapsulate(collider.bounds);
            }

            if (obstacleBounds.size.z > longestObstacleBoundsSize.z)
            {
                longestObstacleBoundsSize = obstacleBounds.size;
                longestObstacleBoundsCenter = obstacleBounds.center;
            }
        }

        foreach(SyncedObstacleColumn column in synchronizedColumns)
        {
            foreach(SyncedObstacle obstacle in column.synchronizedObstacles)
            {
                Gizmos.DrawWireCube(obstacle.transform.position + longestObstacleBoundsCenter, longestObstacleBoundsSize);
            }
        }
    }

    public void OnEnable()
    {
        if (!Application.isPlaying)
            return;

#if (UNITY_EDITOR)
        if (PrefabStageUtility.GetCurrentPrefabStage() != null && 
        PrefabStageUtility.GetCurrentPrefabStage() == PrefabStageUtility.GetPrefabStage(transform.root.gameObject))
            return;
#endif

        if (isPickupSpawningDone)
            return;

        if (synchronizedColumns.Count == 0)
        {
            UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. Obstacles to synchronize count is zero. Obstacle Synchronizer will not work.");
        }

        ShuffleList(possiblePickups);
        
        int syncObstacleIndex = Random.Range(0, possiblePickups.Length);

        foreach (SyncedObstacleColumn column in synchronizedColumns)
        {
            column.SpawnObstacles(possiblePickups, syncObstacleIndex);
        }

        isPickupSpawningDone = true;
    }

    public void ShuffleList<T>(IList<T> listToShuffle)
    {
        int listCount = listToShuffle.Count;
        int lastElementIndex = listCount - 1;
        for (var i = 0; i < lastElementIndex; ++i)
        {
            var swapTargetIndex = UnityEngine.Random.Range(i, listCount);
            var swapSubjectValue = listToShuffle[i];
            listToShuffle[i] = listToShuffle[swapTargetIndex];
            listToShuffle[swapTargetIndex] = swapSubjectValue;
        }
    }

    public void ValidateAndInitialize()
    {
        int syncedObstacleColumnCount = GetComponentsInChildren<SyncedObstacleColumn>().Length;

        if (syncedObstacleColumnCount == 0)
        {
            UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. Columns to synchronize count is zero. Obstacle Synchronizer will not work.");
        }

        if (previousUpdateSyncedObstacleColumnCount != syncedObstacleColumnCount || !AreAllChildrenAlreadyInSynchronizedColumnsList())
        {
            previousUpdateSyncedObstacleColumnCount = syncedObstacleColumnCount;

            synchronizedColumns.Clear();

            synchronizedColumns = GetComponentsInChildren<SyncedObstacleColumn>().ToList();
        }
    }

    private bool AreAllChildrenAlreadyInSynchronizedColumnsList()
    {
        List<SyncedObstacleColumn> currentSchronizedColumns = GetComponentsInChildren<SyncedObstacleColumn>().ToList();

        for (int i = 0; i < currentSchronizedColumns.Count; i++)
        {
            if (!synchronizedColumns.Contains(currentSchronizedColumns[i]))
                return false;
        }

        return false;
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Application.isPlaying)
            return; 

        ValidateAndInitialize();
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

        ValidateAndInitialize();
    }
#endif */
}
