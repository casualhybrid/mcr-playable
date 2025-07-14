using FrostyMaxSaveManager;
using System.Threading;
using System.Threading.Tasks;

namespace TheKnights.SaveFileSystem
{
    public class LoadTheGame
    {
        /// <summary>
        /// Locally load the main save file from the ROM
        /// </summary>
        /// <returns></returns>
        public static Task<T> LoadFileFromROM<T>(string FileName)
        {
            //Un0ityEngine.UnityEngine.Console.Log("Load The Game Main Thread");

           // var context = TaskScheduler.FromCurrentSynchronizationContext();

          //  UnityEngine.UnityEngine.Console.Log("Current Synchronization Context " + context.ToString());


           //return Task.Factory.StartNew<T>(() =>
           // {
           //    // UnityEngine.UnityEngine.Console.Log("Starting Loading Task");
           //     FrostRead<T> _FrostRead = new FrostRead<T>();
           //     return _FrostRead.LoadTheGame(FileName, true);
           // },  TaskCreationOptions.LongRunning);




            return Task.Run(() =>
            {
               // UnityEngine.UnityEngine.Console.Log("Starting Loading Task");
                FrostRead<T> _FrostRead = new FrostRead<T>();
                return _FrostRead.LoadTheGame(FileName, true);
            });
        }
    }
}