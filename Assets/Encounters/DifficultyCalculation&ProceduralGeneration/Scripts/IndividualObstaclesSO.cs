using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IndividualObstaclesSO", menuName = "ScriptableObjects/Encounters/IndividualObstaclesSO")]
public class IndividualObstaclesSO : ScriptableObject
{

  

    #region Variables

    public IndividualObstacle[] CurrentAuthorizedIndividualObstacles
    {
        get
        {
            if (currentAuthorizedIndividualObstacles == null)
                RefreshAuthorizedIndividualObstacles();

            return currentAuthorizedIndividualObstacles;
        }
    }

    private IndividualObstacle[] currentAuthorizedIndividualObstacles;

    public Dictionary<string, GameObject> allIndividualObstacles { get; private set; } = new Dictionary<string, GameObject>();

    [Header("Obstalces")]
    public IndividualObstacle[] individualObstacles;

    public IndividualForceLaneChangeObstacle[] individualForceLaneChangeObstacle;
    public IndividualNonActionObstacle[] individualNonActionObstacles;

    [Header("References")]
    [SerializeField] private GameEvent playerStartedBuildingClimb;

    [SerializeField] private GameEvent dependenciesLoaded;
    [SerializeField] private DifficultyScaleSO difficultyScaleSO;

    #endregion Variables

    private void OnEnable()
    {
        //  SceneManager.activeSceneChanged += HandleActiveSceneChanged;
        playerStartedBuildingClimb.TheEvent.AddListener(HandlePlayerStartedBuildingClimb);
        dependenciesLoaded.TheEvent.AddListener(RefreshIndividualObstaclesOnGamePlayLoaded);

        foreach (IndividualObstacle obstacle in individualObstacles)
        {
            allIndividualObstacles.Add(obstacle.obstacleName, obstacle.obstaclePrefab);
        }

        foreach (IndividualForceLaneChangeObstacle obstacle in individualForceLaneChangeObstacle)
        {
            allIndividualObstacles.Add(obstacle.obstacleName, obstacle.obstaclePrefab);
        }

        foreach (IndividualNonActionObstacle obstacle in individualNonActionObstacles)
        {
            allIndividualObstacles.Add(obstacle.obstacleName, obstacle.obstaclePrefab);
        }
    }

    private void OnDisable()
    {
        dependenciesLoaded.TheEvent.RemoveListener(RefreshIndividualObstaclesOnGamePlayLoaded);

        //   SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
        playerStartedBuildingClimb.TheEvent.RemoveListener(HandlePlayerStartedBuildingClimb);
    }

    //public void HandleActiveSceneChanged(Scene replacedScene, Scene nextScene)
    //{
    // RefreshAuthorizedIndividualObstacles();
    //}

    public void HandlePlayerStartedBuildingClimb(GameEvent gameEvent)
    {
        RefreshAuthorizedIndividualObstacles();
    }

    private void RefreshIndividualObstaclesOnGamePlayLoaded(GameEvent gameEvent)
    {
        RefreshAuthorizedIndividualObstacles();
    }

    public void RefreshAuthorizedIndividualObstacles()
    {
        List<IndividualObstacle> authorizedIndividualObstacles = new List<IndividualObstacle>();

        for (int i = 0; i < individualObstacles.Length; i++)
        {
            if (individualObstacles[i].obstacleName == "Sporty" && difficultyScaleSO.CurrentIntendedDifficultyToMaxRatioPercent < 60)
            {
                continue;
            }

            if (individualObstacles[i].obstacleName == "Scrappy" && difficultyScaleSO.CurrentIntendedDifficultyToMaxRatioPercent < 45)
            {
                continue;
            }

            if (individualObstacles[i].obstacleName == "Mini" && difficultyScaleSO.CurrentIntendedDifficultyToMaxRatioPercent < 30)
            {
                continue;
            }

            authorizedIndividualObstacles.Add(individualObstacles[i]);
        }

        currentAuthorizedIndividualObstacles = authorizedIndividualObstacles.ToArray();
    }

    // public GameObject GetObstalceWithRequestedApproaches(int approachCount)
    // {
    //     List<GameObject> objThatSatisfyRequirement = new List<GameObject>();

    //     for (int i = 0; i < individualObstacles.Length; i++)
    //     {
    //         int individualObstacleApproachCount = 0;

    //         if (individualObstacles[i].obstacleProperties.isJumpable)
    //             individualObstacleApproachCount++;

    //         if (individualObstacles[i].obstacleProperties.isDuckable)
    //             individualObstacleApproachCount++;

    //         if (individualObstacles[i].obstacleProperties.isDashable)
    //             individualObstacleApproachCount++;

    //         if (individualObstacles[i].obstacleProperties.isShockwaveable)
    //             individualObstacleApproachCount++;

    //         if (individualObstacleApproachCount == approachCount)
    //         {
    //             objThatSatisfyRequirement.Add(individualObstacles[i].obstaclePrefab);
    //         }
    //     }

    //     if (objThatSatisfyRequirement.Count == 0)
    //     {
    //         return (GetObstalceWithRequestedApproaches(approachCount - 1));
    //     }

    //     int randObstacleIndex = Random.Range(0, objThatSatisfyRequirement.Count);

    //     return objThatSatisfyRequirement[randObstacleIndex];
    // }
}