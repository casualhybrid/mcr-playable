using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GeneralVarianceTransformFactory", menuName = "Factory/GeneralVarianceTransformFactory")]
public class GeneralVarianceTransformFactory : FactoryVarianceSO<InstanceIDUnique, int>
{
    [SerializeField] private List<InstanceIDUnique> objectsToWarmup;

    public override InstanceIDUnique Create(InstanceIDUnique key)
    {
        return Instantiate(key);
    }

    public override Dictionary<int, Stack<InstanceIDUnique>> CreateBatch(int copiesPerItem)
    {
        Dictionary<int, Stack<InstanceIDUnique>> keyValuePairs = new Dictionary<int, Stack<InstanceIDUnique>>();

        for (int i = 0; i < objectsToWarmup.Count; i++)
        {
            Stack<InstanceIDUnique> stack = new Stack<InstanceIDUnique>();

            InstanceIDUnique instanceIDUnique = objectsToWarmup[i];

            for (int k = 0; k < copiesPerItem; k++)
            {
                stack.Push(Instantiate(instanceIDUnique));
            }

    //       UnityEngine.Console.Log("Creating Stack with ID " + instanceIDUnique.InstanceID);
            keyValuePairs.Add(instanceIDUnique.InstanceID, stack);
        }

        return keyValuePairs;
    }
}