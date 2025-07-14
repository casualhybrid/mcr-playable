using System;
using System.Threading.Tasks;

namespace TheKnights.FaceBook
{
    public enum Status
    {
        Failed, Succeeded
    }

    public class AsyncOperationFaceBook
    {
        public event Action<Status> OnOperationCompleted;

        public Task<Status> task;
        public bool isCompleted;
        public Exception Exception;

        public void SendOperationCompletedEvent(Status status)
        {
            OnOperationCompleted?.Invoke(status);
        }
    }
}