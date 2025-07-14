using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public abstract class FactoryVarianceSO<T, U> : SerializedScriptableObject, IFactoryVariance<T, U>
{
    public abstract T Create(T key);
    public abstract Dictionary<U, Stack<T>> CreateBatch(int copiesPerItem);
    public virtual IEnumerator CreateBatchWithDelay(int copiesPerItem, Transform parentT)
    {
        yield return null;
    }

}