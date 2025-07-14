using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnvPatchPoolSO", menuName = "Pool/EnvPatchPoolSO Pool")]
public class EnvPatchPoolSO : ComponentPoolVarianceSO<Patch>
{
    [SerializeField] private EnvFactorySO envFactory;

    public override IFactoryVariance<Patch, int> Factory
    {
        get
        {
            return envFactory;
        }
        set
        {
            envFactory = value as EnvFactorySO;
        }
    }

    public override void Return(Patch member)
    {
        base.Return(member);

        ReturnItemBackToPool(member, member.InstanceID);
    }

    public void DestroyPooledObjectsForID(int id)
    {
        Stack<Patch> patches;
        Available.TryGetValue(id, out patches);

        if (patches == null)
        {
            UnityEngine.Console.Log("A request to destroy pooled patch of id " + id + " was receieved but no pooled objects were found");
            return;
        }

        while (patches.Count != 0)
        {
           Destroy(patches.Pop().gameObject);
          
        }
    }
}