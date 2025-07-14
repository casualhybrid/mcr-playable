using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if (UNITY_EDITOR)
using UnityEditor.SceneManagement;
#endif

[ExecuteInEditMode]
public class ShuffleObstaclesBySwappingCustomGroups : MonoBehaviour, IValidatable
{
    [SerializeField] private List<Transform> swapGroupA = new List<Transform>();
    [SerializeField] private List<Transform> swapGroupB = new List<Transform>();
    
    public void InitializeShuffleObstaclesBySwappingCustomGroups()
    {
        if (!Application.isPlaying)
            return;

#if (UNITY_EDITOR)
        if (PrefabStageUtility.GetCurrentPrefabStage() != null && 
        PrefabStageUtility.GetCurrentPrefabStage() == PrefabStageUtility.GetPrefabStage(transform.root.gameObject))
            return;
#endif

        // Validation
        if (swapGroupA.Count == 0 || swapGroupB.Count == 0)
        {
            UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. This encounter does not meet the requirements of 'ShuffleObstaclesBySwappingCustomGroups' script attached to it");
            return;
        }

        if (swapGroupA[0] == null)
        {
            UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. This encounter does not meet the requirements of 'ShuffleObstaclesBySwappingCustomGroups' script attached to it");
            return;
        }


        float xPosOfFirstEntityOfGroupA = swapGroupA[0].position.x;
        for(int i = 1; i < swapGroupA.Count; i++)
        {
            if (swapGroupA[i] == null)
            {
                UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. This encounter does not meet the requirements of 'ShuffleObstaclesBySwappingCustomGroups' script attached to it");
                return;
            }

            if (swapGroupA[i].position.x != xPosOfFirstEntityOfGroupA)
            {
                UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. This encounter does not meet the requirements of 'ShuffleObstaclesBySwappingCustomGroups' script attached to it");
                return;
            }
        } 


        if (swapGroupB[0] == null)
        {
            UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. This encounter does not meet the requirements of 'ShuffleObstaclesBySwappingCustomGroups' script attached to it");
            return;
        }

        float xPosOfFirstEntityOfGroupB = swapGroupB[0].position.x;
        for(int i = 1; i < swapGroupB.Count; i++)
        {
            if (swapGroupB[i] == null)
            {
                UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. This encounter does not meet the requirements of 'ShuffleObstaclesBySwappingCustomGroups' script attached to it");
                return;
            }

            if (swapGroupB[i].position.x != xPosOfFirstEntityOfGroupB)
            {
                UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. This encounter does not meet the requirements of 'ShuffleObstaclesBySwappingCustomGroups' script attached to it");
                return;
            }
        }


        // Swapping
        bool shouldSwapObjects = Random.Range(0, 2) == 1;

        if (shouldSwapObjects)
        {
            float xPos = xPosOfFirstEntityOfGroupA;

            for(int i = 0; i < swapGroupA.Count; i++)
            {
                swapGroupA[i].position = new Vector3(xPosOfFirstEntityOfGroupB, swapGroupA[i].position.y, swapGroupA[i].position.z);
            }

            for(int i = 0; i < swapGroupB.Count; i++)
            {
                swapGroupB[i].position = new Vector3(xPos, swapGroupB[i].position.y, swapGroupB[i].position.z);
            }
        }
    }

    public void ValidateAndInitialize()
    {
        Validate();
    }

    private void Validate()
    {
        if (swapGroupA.Count == 0)
        {
            UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. SwapGroupA entity count is zero. Swapping custom groups will not work.");
            return;
        }

        if (swapGroupB.Count == 0)
        {
            UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. SwapGroupB entity count is zero. Swapping custom groups will not work.");
            return;
        }


        if (swapGroupA[0] == null)
        {
            UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. One of the enitites in SwapGroupA is null. Swapping custom groups will not work.");
            return;
        }

        float xPosOfFirstEntityOfGroupA = swapGroupA[0].position.x;
        for(int i = 1; i < swapGroupA.Count; i++)
        {
            if (swapGroupA[i] == null)
            {
                UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. One of the enitites in SwapGroupA is null. Swapping custom groups will not work.");
                return;
            }

            if (swapGroupA[i].position.x != xPosOfFirstEntityOfGroupA)
            {
                UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. Entities in SwapGroupA don't have the same x position. Swapping custom groups will not work.");
                return;
            }
        } 


        if (swapGroupB[0] == null)
        {
            UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. One of the enitites in SwapGroupB is null. Swapping custom groups will not work.");
            return;
        }

        float xPosOfFirstEntityOfGroupB = swapGroupB[0].position.x;
        for(int i = 1; i < swapGroupB.Count; i++)
        {
            if (swapGroupB[i] == null)
            {
                UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. One of the enitites in SwapGroupB is null. Swapping custom groups will not work.");
                return;
            }

            if (swapGroupB[i].position.x != xPosOfFirstEntityOfGroupB)
            {
                UnityEngine.Console.LogError($"Prefab: {gameObject.transform.root.gameObject.name}. Entities in SwapGroupB don't have the same x position. Swapping custom groups will not work.");
                return;
            }
        }
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

