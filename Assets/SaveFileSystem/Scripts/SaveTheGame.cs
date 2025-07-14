//using Firebase.Extensions;
using FrostyMaxSaveManager;
using MessagePack;
using System.Threading.Tasks;

namespace TheKnights.SaveFileSystem
{
    public class SaveTheGame
    {
        public static AsyncOperationSaveSystem SaveTheFile<T>(T InstanceToSave, string FileName, float ThisSessionTime = 0, bool CloudSave = false, bool OnlyToCloud = false, bool ForSyncPurpose = false)
        {
            AsyncOperationSaveSystem handle = new AsyncOperationSaveSystem();

            // Default state
            Status status = Status.Unknown;

            // Serialize on the main thread so data won't get corrupted
            byte[] bytes = MessagePackSerializer.Serialize(InstanceToSave);
        

            // Write to disk on background thread
            Task<Status> savingTask = Task.Run(async () =>
            {
                // Force it to be async
                await Task.Yield();

                FrostWrite<T> frostWrite = new FrostWrite<T>();

                status = frostWrite.SaveTheGame(bytes, FileName, ThisSessionTime, CloudSave, OnlyToCloud, ForSyncPurpose);
            })

          // This will continue on main thread as we are scheuling it on current synchronization context
          .ContinueWith((t) =>
          {
              handle.IsDone = true;

              if (status == Status.Succeeded)
              {
                  handle.InvokeSaveSuccessEvent();
              }
              else
              {
                  handle.InvokeSaveFaliureEvent();
              }

              return status;
          }, TaskScheduler.FromCurrentSynchronizationContext());

            handle.Task = savingTask;

            return handle;
        }
    }
}