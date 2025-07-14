using System.Collections.Generic;
using UnityEngine;

#if (UNITY_EDITOR)

using UnityEditor.SceneManagement;

#endif

[ExecuteInEditMode]
public class ShuffleObstaclesBySwappingColumns : MonoBehaviour, IValidatable
{
    [ReadOnly] [SerializeField] private List<Transform> objectsToSwap = new List<Transform>();
    private int previousUpdateChildCount;

    public void InitializeShuffleObstaclesBySwappingColumns()
    {
        if (!Application.isPlaying)
            return;

#if (UNITY_EDITOR)
        if (PrefabStageUtility.GetCurrentPrefabStage() != null &&
        PrefabStageUtility.GetCurrentPrefabStage() == PrefabStageUtility.GetPrefabStage(transform.root.gameObject))
            return;
#endif

        if (objectsToSwap.Count != 2)
        {
            UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. This encounter does not meet the requirements of 'ShuffleObstaclesBySwap' script attached to it");
        }

        bool shouldSwapObjects = Random.Range(0, 2) == 1;

        if (shouldSwapObjects)
        {
            float xPos = objectsToSwap[0].position.x;
            objectsToSwap[0].position = new Vector3(objectsToSwap[1].position.x, objectsToSwap[0].position.y, objectsToSwap[0].position.z);
            objectsToSwap[1].position = new Vector3(xPos, objectsToSwap[1].position.y, objectsToSwap[1].position.z);
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
            UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. Children obstacle count is zero. Swapping children will not work.");
        }

        if (previousUpdateChildCount != transform.childCount || !AreAllChildrenAlreadyInSwapObjectsList())
        {
            previousUpdateChildCount = transform.childCount;

            objectsToSwap.Clear();

            if (transform.childCount == 2)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    objectsToSwap.Add(transform.GetChild(i));
                }
            }
            else
            {
                UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. Children count of '{gameObject.name}' is not equal to '2'. Swapping children will not work. Children count: {transform.childCount}");
            }
        }
    }

    private bool AreAllChildrenAlreadyInSwapObjectsList()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (!objectsToSwap.Contains(transform.GetChild(i)))
                return false;
        }

        return true;
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

#endif
}