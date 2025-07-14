/**********************************************************************;
* Program name      : FrostySaveManager
*
* Dependencies      : Google Play Services with save gamed enabled from console
*                     as well as initizlied within the code | MessagePacker for
*                     binary serialization (optional), code can be changed if you
*                     want to use default binary unity serializer
*
* Author            : Talha Hanif
*
* Date created      : 07-Dec-19
*
* Purpose           : Save and synchronizes data between PlayServices Cloud and Local Disk
*
* Revision History  :
*
* Date Author      Ref Revision (Date in YYYYMMDD format)
* 20191207    Initial Build      1      In Testing Phase.
*
/**********************************************************************/

using UnityEngine;

namespace FrostyMaxSaveManager
{
    public enum OperationFailed { ReadFromCloud, WriteToCloud, ReadFromLocal, WriteToLocal, AlreadyWriting }

    public abstract class FrostReadWrite
    {
        public static string persistentpath { get; private set; }

        // Sets the persistent data path as soon as the first scene loads
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void SetPersistentPath()
        {
            persistentpath = Application.persistentDataPath;


        }

        /// <summary>
        /// Default definition is recommanded. However, these can also be
        /// changed if needed
        /// </summary>
        //public const string GlobalTimePlayed = "GlobalTimePlayed";
        public const string TimeStampConst = "TimeStamp";

        // Any Read/Write operation faliure will be handled here.

        #region Read/Write Faliure Callbacks

        protected void WriteOperationFailed<T>(OperationFailed FailedOperation)
        {
            UnityEngine.Console.LogWarning("Write operation of type : " + FailedOperation + " failed");

            switch (FailedOperation)
            {
                case OperationFailed.WriteToLocal: { UnityEngine.Console.Log("Turning Off Is Writing Local"); FrostWrite<T>.IsWritingLocal = false; break; }
            }
        }

        protected void ReadOperationFailed<T>(OperationFailed FailedOperation, FrostRead<T> Instance = null)
        {
            UnityEngine.Console.LogWarning("Read Operation of type: " + FailedOperation + " failed");
            switch (FailedOperation)
            {
                case OperationFailed.ReadFromLocal:
                    {
                        if (Instance == null) UnityEngine.Console.LogError("Instance parameter of ReadOperationFailed was null. This should never happen");

                        break;
                    }
            }
        }

        #endregion Read/Write Faliure Callbacks
    }
}