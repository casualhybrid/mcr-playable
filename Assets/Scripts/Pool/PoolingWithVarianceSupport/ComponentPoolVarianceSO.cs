using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UOP1.Pool;

public abstract class ComponentPoolVarianceSO<T> : PoolVarianceSO<T, int> where T : Component
{
    protected override void OnEnable()
    {
        base.OnEnable();
    }

    private Transform _poolRoot;

    public Transform PoolRoot
    {
        get
        {
            if (_poolRoot == null)
            {
                _poolRoot = new GameObject(name).transform;
                _poolRoot.SetParent(_parent);
            }
            return _poolRoot;
        }
    }

    private Transform _parent;

    /// <summary>
    /// Parents the pool root transform to <paramref name="t"/>.
    /// </summary>
    /// <param name="t">The Transform to which this pool should become a child.</param>
    /// <remarks>NOTE: Setting the parent to an object marked DontDestroyOnLoad will effectively make this pool DontDestroyOnLoad.<br/>
    /// This can only be circumvented by manually destroying the object or its parent or by setting the parent to an object not marked DontDestroyOnLoad.</remarks>
    public void SetParent(Transform t)
    {
        _parent = t;
        PoolRoot.SetParent(_parent);
    }

    public override T Request(T req, int key)
    {
        T member = GetRequestedItemFromPool(req, key);
        member.gameObject.SetActive(true);
        return member;
    }

    public override Coroutine PrewarmWithDelays(int num)
    {
        return base.PrewarmWithDelays(num, PoolRoot);
    }

    public override Coroutine PreWarmWithDelayNoRecord(int num)
    {
        return base.PrewarmWithDelaysNoRecord(num, PoolRoot);
    }

    public override void Return(T member)
    {
        member.transform.SetParent(PoolRoot);
        member.gameObject.SetActive(false);
    }

    protected override T Create(T key)
    {
        T newMember = base.Create(key);
        newMember.transform.SetParent(PoolRoot);
     //   newMember.gameObject.SetActive(false);
        return newMember;
    }

    protected override Dictionary<int, Stack<T>> CreateBatch(int copiesPerItem)
    {
        Dictionary<int, Stack<T>> batch = base.CreateBatch(copiesPerItem);

        var listStack = batch.Values.ToList();

        for (int i = 0; i < listStack.Count; i++)
        {
            foreach (var item in listStack[i])
            {
                GameObject newMember = item.gameObject;
                newMember.transform.SetParent(PoolRoot);
                newMember.gameObject.SetActive(false);
            }
        }

        return batch;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (_poolRoot != null)
        {
#if UNITY_EDITOR
            DestroyImmediate(_poolRoot.gameObject);
#else
    				Destroy(_poolRoot.gameObject);
#endif
        }
    }
}