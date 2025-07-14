using UnityEngine;

public class SyncedObstacle : MonoBehaviour
{
    private GameObject fetchedObstacle;
    public GameObject FetchedObstacle => fetchedObstacle;

    [SerializeField] private ObstaclePoolSO obstaclePoolSO = default;

    public void SpawnSyncedObstacle(GameObject obstacleToSpawn)
    {
        Obstacle originalObstacle = obstacleToSpawn.GetComponent<Obstacle>();
        if (originalObstacle == null)
        {
            fetchedObstacle = Instantiate(obstacleToSpawn, transform);
            fetchedObstacle.GetComponent<SpecialPickup>().RaiseSpecialPickupSpawnedEvent();
        }
        else
        {
            fetchedObstacle = obstaclePoolSO.Request(originalObstacle, originalObstacle.InstanceID).gameObject;
            Obstacle obstacle = fetchedObstacle.GetComponent<Obstacle>();
            obstacle.IsThisObstaclePartOfCustomEncounter = true;
            obstacle.ShoudNotOffsetOnRest = true;
            fetchedObstacle.transform.parent = transform;
            fetchedObstacle.transform.localPosition = new Vector3(0, obstacleToSpawn.transform.localPosition.y, 0);
            fetchedObstacle.transform.localRotation = Quaternion.identity;
        }
    }

    public void ReturnSyncedObstacle()
    {
        if (fetchedObstacle == null)
            return;

        Obstacle fetchedObstacleComponent = fetchedObstacle.GetComponent<Obstacle>();
        if (fetchedObstacleComponent)
        {
            fetchedObstacleComponent.RestTheObstacle();
            obstaclePoolSO.Return(fetchedObstacleComponent);
        }
        else
        {
            Destroy(fetchedObstacle);
        }
        fetchedObstacle = null;
    }
}
