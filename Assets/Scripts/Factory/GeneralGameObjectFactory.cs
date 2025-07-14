using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UOP1.Factory;

[CreateAssetMenu(fileName = "GeneralGameObjectFactory", menuName = "Factory/GeneralGameObjectFactory")]
public class GeneralGameObjectFactory : FactorySO<Transform>
{
    [SerializeField] private Transform prefab;

    public override Transform Create()
    {
        return Instantiate(prefab);
    }
}
