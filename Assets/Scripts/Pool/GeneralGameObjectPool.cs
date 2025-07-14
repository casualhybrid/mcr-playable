using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UOP1.Factory;
using UOP1.Pool;

[CreateAssetMenu(fileName = "GeneralGameObjectPool", menuName = "Pool/GeneralGameObjectPool")]
public class GeneralGameObjectPool : ComponentPoolSO<Transform>
{
    [SerializeField] private GeneralGameObjectFactory generalGameObjectFactory;

    public override IFactory<Transform> Factory
    {
        get
        {
            return generalGameObjectFactory;
        }
        set
        {
            generalGameObjectFactory = value as GeneralGameObjectFactory;
        }
    }
}
