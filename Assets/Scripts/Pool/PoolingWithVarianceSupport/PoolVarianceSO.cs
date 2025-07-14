using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UOP1.Pool
{
    public abstract class PoolVarianceSO<T, U> : ScriptableObject, IPoolVariance<T>
    {
        protected Dictionary<U, Stack<T>> Available = new Dictionary<U, Stack<T>>();

        /// <summary>
        /// The factory which will be used to create <typeparamref name="T"/> on demand.
        /// </summary>
        public abstract IFactoryVariance<T, U> Factory { get; set; }

        protected bool HasBeenPrewarmed { get; set; }

        protected bool IsPreWarming { get; set; }

        protected virtual void OnEnable()
        {
            SceneManager.activeSceneChanged += SceneChanged;
        }

        protected virtual void Reset()
        {
            Available.Clear();
            HasBeenPrewarmed = false;
            IsPreWarming = false;
        }

        private void SceneChanged(Scene s, Scene d)
        {
            Reset();
        }

        protected virtual T Create(T key)
        {
            return Factory.Create(key);
        }

        protected virtual Dictionary<U, Stack<T>> CreateBatch(int copiesPerItem)
        {
            return Factory.CreateBatch(copiesPerItem);
        }

        /// <summary>
        /// Prewarms the pool with a <paramref name="num"/> of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="num">The number of members to create as a part of this pool.</param>
        /// <remarks>NOTE: This method can be called at any time, but only once for the lifetime of the pool.</remarks>
        public virtual void Prewarm(int num)
        {
            if (HasBeenPrewarmed)
            {
                UnityEngine.Console.LogWarning($"Pool {name} has already been prewarmed.");
                return;
            }

            var V = CreateBatch(num);

            Available = V;

            HasBeenPrewarmed = true;
        }

        /// <summary>
        /// Prewarms the pool with a <paramref name="num"/> of <typeparamref name="T"/> without marking prewarming as done.
        /// </summary>
        /// <param name="num">The number of members to create as a part of this pool.</param>
        /// <remarks>NOTE: This method can be called at any time, but only once for the lifetime of the pool.</remarks>
        public virtual Coroutine PreWarmWithDelayNoRecord(int num)
        {
            return CoroutineRunner.Instance.StartCoroutine(PrewarmWithDelaysRoutineNoRecord(num));
        }

        /// <summary>
        /// Prewarms the pool with a <paramref name="num"/> of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="num">The number of members to create as a part of this pool.</param>
        /// <remarks>NOTE: This method can be called at any time, but only once for the lifetime of the pool.</remarks>
        public virtual Coroutine PrewarmWithDelays(int num)
        {
            if (IsPreWarming)
            {
                UnityEngine.Console.LogWarning($"Pool {name} is already being prewarmed");
                return null;
            }

            if (HasBeenPrewarmed)
            {
                UnityEngine.Console.LogWarning($"Pool {name} has already been prewarmed.");
                return null;
            }

            IsPreWarming = true;
            return CoroutineRunner.Instance.StartCoroutine(PrewarmWithDelaysRoutine(num));
        }

        /// <summary>
        /// Prewarms the pool with a <paramref name="num"/> of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="num">The number of members to create as a part of this pool.</param>
        /// <param name="parentT">The root transform for the created objects.</param>
        /// <remarks>NOTE: This method can be called at any time, but only once for the lifetime of the pool.</remarks>
        protected virtual Coroutine PrewarmWithDelays(int num, Transform parentT = null)
        {
            if(IsPreWarming)
            {
                UnityEngine.Console.LogWarning($"Pool {name} is already being prewarmed");
                return null;
            }

            if (HasBeenPrewarmed)
            {
                UnityEngine.Console.LogWarning($"Pool {name} has already been prewarmed.");
                return null;
            }

            IsPreWarming = true;
            return CoroutineRunner.Instance.StartCoroutine(PrewarmWithDelaysRoutine(num, parentT));
        }

     
        protected virtual Coroutine PrewarmWithDelaysNoRecord(int num, Transform parentT = null)
        {
            return CoroutineRunner.Instance.StartCoroutine(PrewarmWithDelaysRoutineNoRecord(num, parentT));
        }

        private IEnumerator PrewarmWithDelaysRoutine(int num, Transform parentT = null)
        {
            CoroutineWithReturn<Dictionary<U, Stack<T>>> coroutineWithReturn = new CoroutineWithReturn<Dictionary<U, Stack<T>>>(CoroutineRunner.Instance, Factory.CreateBatchWithDelay(num, parentT));

            yield return coroutineWithReturn;

            Available = coroutineWithReturn.Result;

            IsPreWarming = false;
            HasBeenPrewarmed = true;
        }

        private IEnumerator PrewarmWithDelaysRoutineNoRecord(int num, Transform parentT = null)
        {
            CoroutineWithReturn<Dictionary<U, Stack<T>>> coroutineWithReturn = new CoroutineWithReturn<Dictionary<U, Stack<T>>>(CoroutineRunner.Instance, Factory.CreateBatchWithDelay(num, parentT));

            yield return coroutineWithReturn;

            if(Available == null || Available.Count == 0)
            {
                Available = coroutineWithReturn.Result;
            }
            else
            {
                foreach (var pair in coroutineWithReturn.Result)
                {
                    if(Available.ContainsKey(pair.Key))
                    {
                        Available[pair.Key] = pair.Value;
                    }
                    else
                    {
                        Available.Add(pair.Key, pair.Value);
                    }
                }
            }

        }

        /// <summary>
        /// Requests a <typeparamref name="T"/> from this pool.
        /// </summary>
        /// <returns>The requested <typeparamref name="T"/>.</returns>
        public virtual T Request(T req, int key)
        {
            return default(T);
        }

        protected virtual T GetRequestedItemFromPool(T req, U key)
        {
            T member;

            // UnityEngine.Console.Log("Requested Item with Unique Key " + key);

            if (Available.ContainsKey(key))
            {
                //      UnityEngine.Console.Log("Returning available from pool " + req.ToString());

                var stack = Available[key];

                if (stack.Count <= 0)
                {
                    member = Create(req);
                }
                else
                {
                    member = stack.Pop();
                }
            }
            else
            {
                // UnityEngine.Console.Log("Creating New " + req.ToString());
                member = Create(req);
            }

            return member;
        }

        /// <summary>
        /// Returns a <typeparamref name="T"/> to the pool.
        /// </summary>
        /// <param name="member">The <typeparamref name="T"/> to return.</param>
        public virtual void Return(T member)
        {
        }

        protected virtual void ReturnItemBackToPool(T member, U key)
        {
            if (Available.ContainsKey(key))
            {
                //   UnityEngine.Console.Log("Moved To Pool " + member.ToString() +" with Key "+ key);
                Available[key].Push(member);
            }
            else
            {
                //     UnityEngine.Console.Log("Created New Stack " + member.ToString() + " with Unique ID "+ key);

                Stack<T> stack = new Stack<T>();
                stack.Push(member);
                Available.Add(key, stack);
            }
        }

        public virtual void OnDisable()
        {
            SceneManager.activeSceneChanged -= SceneChanged;
            Reset();
        }
    }
}