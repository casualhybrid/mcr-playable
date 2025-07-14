using System;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace TheKnights.AddressableSystem
{
    public class AddressableOperationHandle
    {
        public bool isDone { get; private set; }

        public object Result { get; set; }

        public event Action<AddressableOperationHandle> Completed;

        public AsyncOperationHandle handle { get; set; }

        public void SendCompletionEvent()
        {
            isDone = true;
            Completed(this);
        }
    }

    public class AddressableOperationHandle<T>
    {
        public bool isDone { get; private set; }
        public T Result { get; set; }

        public event Action<AddressableOperationHandle<T>> Completed;

        public AsyncOperationHandle<T> handle { get; set; }

        public void SendCompletionEvent()
        {
            isDone = true;
            Completed(this);
        }
    }
}