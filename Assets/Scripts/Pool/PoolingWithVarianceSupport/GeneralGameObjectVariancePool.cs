using UnityEngine;

[CreateAssetMenu(fileName = "GeneralGameObjectVariancePool", menuName = "Pool/GeneralGameObjectVariancePool")]
public class GeneralGameObjectVariancePool : ComponentPoolVarianceSO<InstanceIDUnique>
{
    [SerializeField] private GeneralVarianceTransformFactory factory;

    public override IFactoryVariance<InstanceIDUnique, int> Factory
    {
        get
        {
            return factory;
        }
        set
        {
            factory = value as GeneralVarianceTransformFactory;
        }
    }

    public override void Return(InstanceIDUnique member)
    {
        base.Return(member);

        ReturnItemBackToPool(member, member.InstanceID);
    }
}