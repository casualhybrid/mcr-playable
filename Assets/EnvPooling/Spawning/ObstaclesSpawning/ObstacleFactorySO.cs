using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObstacleFactorySO", menuName = "Factory/ObstacleFactorySO")]
public class ObstacleFactorySO : FactoryVarianceSO<Obstacle, int>
{
    [SerializeField] private Dictionary<Obstacle, int> hurdlesToCreate;

    public override Obstacle Create(Obstacle key)
    {
        return Instantiate(key);
    }

    public override Dictionary<int, Stack<Obstacle>> CreateBatch(int copiesPerItem)
    {
        Dictionary<int, Stack<Obstacle>> keyValuePairs = new Dictionary<int, Stack<Obstacle>>();

        foreach (var pair in hurdlesToCreate)
        {
            Obstacle obstacle = pair.Key;
            int timesToSpawn = pair.Value == -1 ? copiesPerItem : pair.Value;
            Stack<Obstacle> stack = new Stack<Obstacle>();

            for (int k = 0; k < timesToSpawn; k++)
            {
                stack.Push(Instantiate(obstacle));
            }

            keyValuePairs.Add(obstacle.InstanceID, stack);
        }

        return keyValuePairs;
    }

    public override IEnumerator CreateBatchWithDelay(int copiesPerItem, Transform parentT = null)
    {
        Dictionary<int, Stack<Obstacle>> keyValuePairs = new Dictionary<int, Stack<Obstacle>>();

        foreach (var pair in hurdlesToCreate)
        {
            Obstacle obstacle = pair.Key;
            int timesToSpawn = pair.Value == -1 ? copiesPerItem : pair.Value;
            Stack<Obstacle> stack = new Stack<Obstacle>();

            for (int k = 0; k < timesToSpawn; k++)
            {
                yield return null;

                Obstacle entity = Instantiate(obstacle, parentT);
                entity.gameObject.SetActive(false);
                stack.Push(entity);
            }

            keyValuePairs.Add(obstacle.InstanceID, stack);
        }

        yield return keyValuePairs;
    }
}