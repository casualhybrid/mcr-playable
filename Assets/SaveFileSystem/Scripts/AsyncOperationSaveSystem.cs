namespace TheKnights.SaveFileSystem
{
    using System.Threading.Tasks;
    using UnityEngine.Events;

    /// <summary>
    /// The asynchronous handle returned upon starting a save operation
    /// </summary>
    public class AsyncOperationSaveSystem
    {
        /// <summary>
        /// Mark the operation as completed independent of its success status
        /// </summary>
        public bool IsDone { get; set; }

        /// <summary>
        /// Invoked when the save operation is successfull
        /// </summary>
        public UnityEvent OnSaveSuccess = new UnityEvent();

        /// <summary>
        /// Invoked when the save operation has failed
        /// </summary>
        public UnityEvent OnSaveFailed = new UnityEvent();

        /// <summary>
        /// The awaitable task containing the result of the saving operation
        /// </summary>
        public Task<Status> Task { get; set; }

        public void InvokeSaveSuccessEvent()
        {
            OnSaveSuccess?.Invoke();
        }

        public void InvokeSaveFaliureEvent()
        {
            OnSaveFailed?.Invoke();
        }
    }
}