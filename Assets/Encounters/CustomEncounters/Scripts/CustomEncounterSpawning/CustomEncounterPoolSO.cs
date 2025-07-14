using UnityEngine;

[CreateAssetMenu(fileName = "CustomEncounterPoolSO", menuName = "Pool/CustomEncounterPoolSO Pool")]
public class CustomEncounterPoolSO : ComponentPoolVarianceSO<CustomEncounter>
{
    [SerializeField] private CustomEncounterFactorySO customEncounterFactory;

    public override IFactoryVariance<CustomEncounter, int> Factory
    {
        get
        {
            return customEncounterFactory;
        }
        set
        {
            customEncounterFactory = value as CustomEncounterFactorySO;
        }
    }

    public override void Return(CustomEncounter member)
    {
        base.Return(member);

        ReturnItemBackToPool(member, member.InstanceID);
    }
}