using System;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace TheKnights.SceneLoadingSystem
{
    public class AsyncSceneOperationHandle
    {
        public bool IsDone { get; set; }
        public Exception OperationException { get; set; }
        public float PercentComplete { get; set; }
        public AsyncOperationStatus Status { get; set; }

        public event Action<AsyncSceneOperationHandle> Completed;

        public event Action<AsyncSceneOperationHandle> Destroyed;

        public void SendCompletionEvent()
        {
            Completed?.Invoke(this);
        }
    }
}