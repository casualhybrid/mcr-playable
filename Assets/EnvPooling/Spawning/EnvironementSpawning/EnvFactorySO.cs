using System.Collections.Generic;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;
using System.Collections;

[CreateAssetMenu(fileName = "EnvFactorySO", menuName = "Factory/EnvFactory")]
public class EnvFactorySO : FactoryVarianceSO<Patch, int>
{
    [System.Serializable]
    private class EnvCategoryToPoolCountDictionary : SerializableDictionaryBase<EnvCategory, int> { }


    [SerializeField] private EnvCategoryToPoolCountDictionary overridenCategoryPoolCountDictionary;
    [SerializeField] private List<EnviornmentSO> environments;

    private void OnEnable()
    {
        environments.Clear();
    }

    public override Patch Create(Patch key)
    {
        return Instantiate(key);
    }

    public override Dictionary<int, Stack<Patch>> CreateBatch(int copiesPerItem)
    {
        Dictionary<int, Stack<Patch>> keyValuePairs = new Dictionary<int, Stack<Patch>>();

        for (int i = 0; i < environments.Count; i++)
        {
            EnviornmentSO env = environments[i];
            CreateBatchForEnv(env, keyValuePairs, copiesPerItem);
        }
      
        return keyValuePairs;
    }

    public override IEnumerator CreateBatchWithDelay(int copiesPerItem, Transform parentT)
    {
        Dictionary<int, Stack<Patch>> keyValuePairs = new Dictionary<int, Stack<Patch>>();

        for (int i = 0; i < environments.Count; i++)
        {
            EnviornmentSO env = environments[i];
            CreateBatchForEnv(env, keyValuePairs, copiesPerItem);

            yield return null;

        }

        yield return keyValuePairs;
    }

    public void AddEnvironmentForWarmup(EnviornmentSO env)
    {
        environments.Clear();
        environments.Add(env);
    }

    private void CreateBatchForEnv(EnviornmentSO enviornmentSO, Dictionary<int, Stack<Patch>> keyValuePairs, int copiesPerItem)
    {

        //   foreach (List<Patch> p in enviornmentSO.envCategoryDictionary.Values)
        foreach (KeyValuePair<EnvCategory, List<Patch>> keyValuePair in enviornmentSO.envCategoryDictionary)
        {
            List<Patch> p = keyValuePair.Value;
            EnvCategory envCategory = keyValuePair.Key;

            int copies = overridenCategoryPoolCountDictionary.ContainsKey(envCategory) ? overridenCategoryPoolCountDictionary[envCategory] : copiesPerItem;

            for (int i = 0; i < p.Count; i++)
            {
                if (keyValuePairs.ContainsKey(p[i].InstanceID))
                    continue;

                Stack<Patch> stack = new Stack<Patch>();

                for (int k = 0; k < copies; k++)
                {
                    stack.Push(Instantiate(p[i]));
                }

                keyValuePairs.Add(p[i].InstanceID, stack);
            }
        }

        //environments.Clear();
    }
}