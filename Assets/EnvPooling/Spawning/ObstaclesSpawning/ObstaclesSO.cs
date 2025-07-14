using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "ObstaclesSO", menuName = "ScriptableObjects/Obstacles/ObstaclesSO")]
public class ObstaclesSO : SerializedScriptableObject
{
    public Dictionary<EnvCategory, List<Obstacle>> envCategoryDictionary;

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += SceneChanged;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= SceneChanged;
    }

    private void SceneChanged(Scene a, Scene b)
    {
        Reset();
    }

    private void Reset()
    {
    }

    public Obstacle GetRandomObstacleFromCategoryBasedOnLength(EnvCategory envCategory, float length)
    {
     //   UnityEngine.Console.Log("Remaining Length " + length);

        List<Obstacle> obstacles = envCategoryDictionary[envCategory];
        List<Obstacle> selectedObstacles = obstacles.Where((x) => x.GetObstacleLength < length).ToList();

        if (selectedObstacles.Count == 0)
        {
            UnityEngine.Console.LogWarning($"No Obstacle was found for a max available length {length} for the category {envCategory}");
            return null;
        }
        else
        {
         

            int rand = UnityEngine.Random.Range(0, selectedObstacles.Count);
           // UnityEngine.Console.Log("Size Of Obstacle " + selectedObstacles[rand].GetObstacleLength);
            return selectedObstacles[rand];
        }
    }
}