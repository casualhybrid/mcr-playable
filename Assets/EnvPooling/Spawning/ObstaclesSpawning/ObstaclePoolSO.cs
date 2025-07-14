using UnityEngine;

[CreateAssetMenu(fileName = "ObstaclePoolSO", menuName = "Pool/ObstaclePoolSO Pool")]
public class ObstaclePoolSO : ComponentPoolVarianceSO<Obstacle>
{
    [SerializeField] private ObstacleFactorySO obstacleFactory;

    public override IFactoryVariance<Obstacle, int> Factory
    {
        get
        {
            return obstacleFactory;
        }
        set
        {
            obstacleFactory = value as ObstacleFactorySO;
        }
    }

    public override void Return(Obstacle member)
    {
        base.Return(member);

        ReturnItemBackToPool(member, member.InstanceID);
    }
}