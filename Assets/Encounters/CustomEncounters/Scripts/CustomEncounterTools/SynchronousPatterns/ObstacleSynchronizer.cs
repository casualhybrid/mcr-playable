using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

#if (UNITY_EDITOR)

using UnityEditor.SceneManagement;

#endif

[ExecuteInEditMode]
public class ObstacleSynchronizer : MonoBehaviour, IValidatable, ICustomEncounterEntity
{
    [SerializeField] private GameObject[] possibleObstacles;
    [ReadOnly] [SerializeField] private List<SyncedObstacleColumn> synchronizedColumns = new List<SyncedObstacleColumn>();

    public List<SyncedObstacleColumn> SynchronizedColumns
    {
        get
        {
            return synchronizedColumns;
        }
    }

    [ReadOnly] [SerializeField] private CustomEncounter _customEncounter;

    [SerializeField] private GameObject aeroplanePickup;
    [SerializeField] private GamePlaySessionData gamePlaySessionData;
    [SerializeField] private PickupGenerationData pickupGenerationData;
    [SerializeField] private PickupsUtilityHelper pickupsUtility;

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

    private Bounds cachedLongestObstacleBounds { get; set; }

    private Bounds GetCachedLongestObstacleBounds
    {
        get
        {
            if (cachedLongestObstacleBounds.size == Vector3.zero)
            {
                RecalculateCachedLongestObstacleBounds();
            }

            return cachedLongestObstacleBounds;
        }
    }

    private bool isObstacleSynchronizationComplete = false;
    private int previousUpdateSyncedObstacleColumnCount;


    private void Awake()
    {
        if (_customEncounter == null)
        {
            _customEncounter = GetComponentInParent<CustomEncounter>();
        }
    }

    private void OnDrawGizmos()
    {
        if (isObstacleSynchronizationComplete)
            return;

        if (possibleObstacles.Length == 0)
            return;

        if (GetCachedLongestObstacleBounds == null)
            return;

        foreach (SyncedObstacleColumn column in synchronizedColumns)
        {
            foreach (SyncedObstacle obstacle in column.synchronizedObstacles)
            {
                Gizmos.color = new Color(1f, 0.2f, 0.2f);
                Gizmos.DrawWireCube(obstacle.transform.position + GetCachedLongestObstacleBounds.center, GetCachedLongestObstacleBounds.size);
                Gizmos.color = new Color(1f, 1f, 1f);
                Gizmos.DrawWireCube(obstacle.transform.position + GetCachedLongestObstacleBounds.center, GetCachedLongestObstacleBounds.size * 0.95f);
            }
        }
    }

    public void InitializeObstacleSynchronizer()
    {
        if (!Application.isPlaying)
            return;

#if (UNITY_EDITOR)
        if (PrefabStageUtility.GetCurrentPrefabStage() != null &&
        PrefabStageUtility.GetCurrentPrefabStage() == PrefabStageUtility.GetPrefabStage(transform.root.gameObject))
            return;
#endif

        if (synchronizedColumns.Count == 0)
        {
            UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. Obstacles to synchronize count is zero. Obstacle Synchronizer will not work.");
        }

        // Set all synced obstacle objects to default state
        SetAllSyncedObstaclesToDefaultState();

        // Spawn synced obstacles
        ShuffleList(possibleObstacles);

        CheckMagnetSafeArea();

        int syncObstacleIndex = Random.Range(0, possibleObstacles.Length);

        foreach (SyncedObstacleColumn column in synchronizedColumns)
        {
            column.SpawnObstacles(possibleObstacles, syncObstacleIndex);
        }

        isObstacleSynchronizationComplete = true;
    }

    private void ShuffleList<T>(IList<T> listToShuffle)
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

    private void CheckMagnetSafeArea()
    {
        float currentZPosition = gamePlaySessionData.DistanceCoveredInMeters;

        if (possibleObstacles.Contains(aeroplanePickup))
        {
            SpecialPickup specialPickup = aeroplanePickup.GetComponent<SpecialPickup>();
            bool isSafeToSpawn = pickupsUtility.isSafeToSpawn(transform.position.z, specialPickup.GetPickupType);

            if (!isSafeToSpawn || (currentZPosition < pickupGenerationData.PickupCurrentStateData.magnetPickupSafeArea && pickupGenerationData.PickupCurrentStateData.magnetPickupSafeArea != -1f))
            {
                possibleObstacles = possibleObstacles.Where(val => val != aeroplanePickup).ToArray();

            }
        }
    }

    #region Longest Bounds Calculation

    public void RecalculateCachedLongestObstacleBounds()
    {
        cachedLongestObstacleBounds = GetLongestObstacleBounds();
    }

    // Returns the current longest possible obstacle's bounds
    private Bounds GetLongestObstacleBounds()
    {
        Bounds longestObstacleBounds = new Bounds();

        foreach (GameObject obstacle in possibleObstacles)
        {
            Collider[] colliders = obstacle.GetComponentsInChildren<Collider>();
            Bounds obstacleBounds = new Bounds();

            foreach (Collider collider in colliders)
            {
                if (collider.GetType() != typeof(BoxCollider) &&
                collider.GetType() != typeof(MeshCollider))
                {
                    UnityEngine.Console.LogError($"{collider.GetType()} is not supported by Obstacle Synchronizer. Only BoxCollider and MeshCollider are supported.");
                    continue;
                }

                if (collider.CompareTag("ObstacleMovementTrigger"))
                    continue;

                if (collider.GetType() == typeof(BoxCollider))
                {
                    obstacleBounds = CustomEncapsulateObstacleBounds(obstacleBounds, new Bounds(collider.transform.position, Vector3.Scale((collider as BoxCollider).size, collider.transform.lossyScale)));
                }
                else if (collider.GetType() == typeof(MeshCollider))
                {
                    MeshRenderer meshRenderer = collider.GetComponent<MeshRenderer>();
                    obstacleBounds = CustomEncapsulateObstacleBounds(obstacleBounds, meshRenderer.bounds);
                }
            }

            if (obstacleBounds.size.z > longestObstacleBounds.size.z)
            {
                longestObstacleBounds = obstacleBounds;
            }
        }

        return longestObstacleBounds;
    }

    private Bounds CustomEncapsulateObstacleBounds(Bounds mainBounds, Bounds boundsToEncapsulate)
    {
        if (mainBounds.extents == Vector3.zero)
        {
            mainBounds = boundsToEncapsulate;
        }
        else
        {
            mainBounds.Encapsulate(boundsToEncapsulate);
        }

        return mainBounds;
    }

    #endregion Longest Bounds Calculation

    #region Synced obstacle properties

    private void UpdateSyncedObstacleProperties()
    {
        if (possibleObstacles.Length == 0 || GetCachedLongestObstacleBounds == null)
        {
            SetAllSyncedObstaclesToDefaultState();
        }
        else
        {
            SetAllSyncedObstaclesToObstacleState();
        }
    }

    private void SetAllSyncedObstaclesToDefaultState()
    {
        foreach (SyncedObstacleColumn column in synchronizedColumns)
        {
            foreach (SyncedObstacle obstacle in column.synchronizedObstacles)
            {
                obstacle.gameObject.layer = LayerMask.NameToLayer("Default");
                Destroy(obstacle.gameObject.GetComponent<BoxCollider>());
            }
        }
    }

    private void SetAllSyncedObstaclesToObstacleState()
    {
        foreach (SyncedObstacleColumn column in synchronizedColumns)
        {
            foreach (SyncedObstacle obstacle in column.synchronizedObstacles)
            {
                if (LayerMask.LayerToName(obstacle.gameObject.layer) != "Obstacles")
                {
                    obstacle.gameObject.layer = LayerMask.NameToLayer("Obstacles");
                }

                BoxCollider syncedObstacleCollider = obstacle.gameObject.GetComponent<BoxCollider>();

                if (syncedObstacleCollider == null)
                {
                    syncedObstacleCollider = obstacle.gameObject.AddComponent<BoxCollider>();
                }

                syncedObstacleCollider.center = GetCachedLongestObstacleBounds.center;
                syncedObstacleCollider.size = GetCachedLongestObstacleBounds.size;
            }
        }
    }

    #endregion Synced obstacle properties

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

        UpdateSyncedObstacleProperties();
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
        RecalculateCachedLongestObstacleBounds();
    }

#endif
}