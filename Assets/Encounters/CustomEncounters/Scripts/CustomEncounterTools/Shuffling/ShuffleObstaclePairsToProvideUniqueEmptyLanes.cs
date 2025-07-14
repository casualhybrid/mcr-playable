using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

#if (UNITY_EDITOR)

using UnityEditor.SceneManagement;

#endif

[ExecuteInEditMode]
public class ShuffleObstaclePairsToProvideUniqueEmptyLanes : MonoBehaviour, IValidatable, ICustomEncounterEntity
{
    [ExecuteInEditMode]
    [System.Serializable]
    public class ObstaclePair
    {
        [ReadOnly] public Transform pairFolderTransform;
        [ReadOnly] public List<Transform> pairObjectTransforms;

        public ObstaclePair(Transform pairFolderTransform, List<Transform> pairObjectTransforms)
        {
            this.pairFolderTransform = pairFolderTransform;
            this.pairObjectTransforms = pairObjectTransforms;
        }
    }

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

    [SerializeField] private List<ObstaclePair> pairsToShuffle = new List<ObstaclePair>();

    public List<ObstaclePair> GetObstaclePairs => pairsToShuffle;


    private int previousUpdateChildCount;

    private void Awake()
    {
        if (_customEncounter == null)
        {
            _customEncounter = GetComponentInParent<CustomEncounter>();
        }

        if (_customEncounter == null)
            return;

        if (!Application.isPlaying)
            return;

        if (!CustomEncounter.IsSkeleton)
            return;

        CustomEncounter.CustomEncounterSkeleton.OnObstaclesSpawnedFromMetaData += CustomEncounterSkeleton_OnObstaclesSpawnedFromMetaData;
    }

    private void CustomEncounterSkeleton_OnObstaclesSpawnedFromMetaData()
    {
        var shuffleData = CustomEncounter.CustomEncounterSkeleton.shuffleObstaclePairData[this];

        foreach (var pair in pairsToShuffle)
        {
            UnityEngine.Console.Log("Loading Cached PairToShuffle");

            var childData = shuffleData.shuffleObstaclePairData[pair.pairFolderTransform];
            pair.pairObjectTransforms.Clear();

            for (int i = 0; i < pair.pairFolderTransform.childCount; i++)
            {
                Transform child = pair.pairFolderTransform.GetChild(i);

                UnityEngine.Console.Log($"Child is {child.name}");

                foreach (var item in childData)
                {
                    UnityEngine.Console.Log($"Data {item.ChildOrder } and {item.ParentT} ");

                    if (item.ChildOrder == i && item.ParentT == child.parent)
                    {
                        UnityEngine.Console.Log("Matched");
                        pair.pairObjectTransforms.Add(child);
                    }
                }
            }
        }
    }

    public void InitializeShuffleObstaclePairsToProvideUniqueEmptyLanes()
    {
        if (!Application.isPlaying)
            return;

#if (UNITY_EDITOR)
        if (PrefabStageUtility.GetCurrentPrefabStage() != null &&
        PrefabStageUtility.GetCurrentPrefabStage() == PrefabStageUtility.GetPrefabStage(transform.root.gameObject))
            return;
#endif

        if (transform.childCount == 0)
        {
            UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. This encounter does not meet the requirements of 'ShuffleObstaclePairsToProvideUniqueEmptyLanes' script attached to it");
        }

        List<int> uniqueLanesLeft = new List<int>() { -1, 0, 1 };

        for (int i = 0; i < pairsToShuffle.Count; i++)
        {
            int randUniqueEmptyLaneIndex = Random.Range(0, uniqueLanesLeft.Count);
            List<Transform> unusedObstacles = new List<Transform>(pairsToShuffle[i].pairObjectTransforms);

            for (int j = -1; j < 2; j++)
            {
                if (uniqueLanesLeft[randUniqueEmptyLaneIndex] == j)
                    continue;

                Transform unusedObstacle = unusedObstacles[unusedObstacles.Count - 1];
                unusedObstacle.position = new Vector3(j, unusedObstacle.position.y, unusedObstacle.position.z);
                unusedObstacles.RemoveAt(unusedObstacles.Count - 1);
            }

            uniqueLanesLeft.RemoveAt(randUniqueEmptyLaneIndex);
        }
    }

    public void ValidateAndInitialize()
    {
        AssignPairsToShuffle();
    }

    private void AssignPairsToShuffle()
    {
        if (transform.childCount == 0)
        {
            UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. Obstacle pair count is zero. Shuffling obstacle pairs to provide empty unique columns will not work.");
        }

        if (previousUpdateChildCount != transform.childCount || !AreAllPairsAlreadyInShufflePairsList())
        {
            previousUpdateChildCount = transform.childCount;

            pairsToShuffle.Clear();

            List<Transform> childrenObstaclePairs = new List<Transform>();

            if (transform.childCount <= 3)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    childrenObstaclePairs.Add(transform.GetChild(i));
                }

                for (int i = 0; i < childrenObstaclePairs.Count; i++)
                {
                    List<Transform> obstaclesInPair = new List<Transform>();

                    for (int j = 0; j < childrenObstaclePairs[i].transform.childCount; j++)
                    {
                        obstaclesInPair.Add(childrenObstaclePairs[i].transform.GetChild(j));
                    }

                    if (obstaclesInPair.Count == 2)
                    {
                        pairsToShuffle.Add(new ObstaclePair(childrenObstaclePairs[i], obstaclesInPair));
                    }
                    else
                    {
                        UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. Individual obstacles in obstacle pair of '{childrenObstaclePairs[i].gameObject.name}' should be 2. Shuffling obstacle pairs to provide empty unique columns will not work. Obstacles in obstacle pair count: {obstaclesInPair.Count}");
                    }
                }
            }
            else
            {
                UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. Obstacle pairs of '{gameObject.name}' exceed the number of unique lanes that can be assigned. Shuffling obstacle pairs to provide empty unique columns will not work. Children obstacle pairs count: {transform.childCount}");
            }
        }
    }

    private bool AreAllPairsAlreadyInShufflePairsList()
    {
        List<Transform> childrenObstaclePairs = new List<Transform>();

        for (int i = 0; i < transform.childCount; i++)
        {
            childrenObstaclePairs.Add(transform.GetChild(i));
        }

        for (int i = 0; i < childrenObstaclePairs.Count; i++)
        {
            List<Transform> obstaclesInPair = new List<Transform>();

            for (int j = 0; j < childrenObstaclePairs[i].transform.childCount; j++)
            {
                obstaclesInPair.Add(childrenObstaclePairs[i].transform.GetChild(j));
            }

            ObstaclePair obstaclePairToCompareTo = new ObstaclePair(childrenObstaclePairs[i], obstaclesInPair);

            if (pairsToShuffle.Count == 0 || pairsToShuffle[i].pairFolderTransform != obstaclePairToCompareTo.pairFolderTransform || !pairsToShuffle[i].pairObjectTransforms.SequenceEqual(obstaclePairToCompareTo.pairObjectTransforms))
            {
                return false;
            }
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

      if(_customEncounter != null && _customEncounter.IsSkeleton)
            return;

        ValidateAndInitialize();
    }

#endif
}