using UnityEngine;

public class ResetDependencyInjector : MonoBehaviour
{
    [SerializeField] private string originalGameObjectName;
    [SerializeField] private IndividualObstaclesSO individualObstaclesSO;

    public string OriginalGameObjectName => originalGameObjectName;

    private void Awake()
    {
        AssignOriginalComponentReferences<ObjectDestruction>();
        AssignOriginalComponentReferences<Trafficar>();
    }

    private void AssignOriginalComponentReferences<T>()
    {
        T[] objectDestructions = GetComponentsInChildren<T>(true);
        T[] objectDestructionsOriginal = individualObstaclesSO.allIndividualObstacles[originalGameObjectName].GetComponentsInChildren<T>(true);

        for (int i = 0; i < objectDestructionsOriginal.Length; i++)
        {
            IResetObject<T> toReset = objectDestructions[i] as IResetObject<T>;
            toReset.OriginalComponent = objectDestructionsOriginal[i];
        }
    }
}