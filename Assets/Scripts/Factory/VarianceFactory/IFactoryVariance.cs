using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFactoryVariance<T,U>
{
    T Create(T key);

    Dictionary<U, Stack<T>> CreateBatch(int copiesPerItem);

    IEnumerator CreateBatchWithDelay(int copiesPerItem, Transform parentT = null);
}