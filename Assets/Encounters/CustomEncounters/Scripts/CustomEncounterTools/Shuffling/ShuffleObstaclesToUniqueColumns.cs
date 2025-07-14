using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if (UNITY_EDITOR)

using UnityEditor.SceneManagement;

#endif

[ExecuteInEditMode]
public class ShuffleObstaclesToUniqueColumns : MonoBehaviour, IValidatable, ICustomEncounterEntity
{
    [ReadOnly] [SerializeField] private List<Transform> objectsToShuffle = new List<Transform>();
    private int previousUpdateChildCount;

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

    public void InitializeShuffleObstaclesToUniqueColumns()
    {
        if (!Application.isPlaying)
            return;

#if (UNITY_EDITOR)
        if (PrefabStageUtility.GetCurrentPrefabStage() != null &&
        PrefabStageUtility.GetCurrentPrefabStage() == PrefabStageUtility.GetPrefabStage(transform.root.gameObject))
            return;
#endif

        if (objectsToShuffle.Count == 0)
        {
            UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. This encounter does not meet the requirements of 'ShuffleObstaclesToUniqueColumns' script attached to it");
        }

        List<int> uniqueLanesLeft = new List<int>() { -1, 0, 1 };

        for (int i = 0; i < objectsToShuffle.Count; i++)
        {
            int randUniqueLaneIndex = Random.Range(0, uniqueLanesLeft.Count);
            objectsToShuffle[i].position = new Vector3(uniqueLanesLeft[randUniqueLaneIndex], objectsToShuffle[i].position.y, objectsToShuffle[i].position.z);

            uniqueLanesLeft.RemoveAt(randUniqueLaneIndex);
        }
    }

    public void ValidateAndInitialize()
    {
        AssignObjectsToShuffle();
    }

    private void AssignObjectsToShuffle()
    {
        if (transform.childCount == 0)
        {
            UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. Children obstacle count is zero. Shuffling children to unique columns will not work.");
        }

        if (previousUpdateChildCount != transform.childCount || !AreAllChildrenAlreadyInShuffleObjectsList())
        {
            previousUpdateChildCount = transform.childCount;

            objectsToShuffle.Clear();

            if (transform.childCount <= 3)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    objectsToShuffle.Add(transform.GetChild(i));
                }
            }
            else
            {
                UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. Children of '{gameObject.name}' exceed the number of unique lanes that can be assigned. Shuffling children to unique columns will not work. Children count: {transform.childCount}");
            }
        }
    }

    private bool AreAllChildrenAlreadyInShuffleObjectsList()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (!objectsToShuffle.Contains(transform.GetChild(i)))
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