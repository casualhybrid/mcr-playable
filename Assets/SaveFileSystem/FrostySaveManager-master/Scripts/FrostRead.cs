using MessagePack;
using System;
using System.IO;
using UnityEngine;

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

namespace FrostyMaxSaveManager
{
    public class FrostRead<T> : FrostReadWrite
    {
        // public delegate void CloudDataHasBeenRead(T data);
        //  public static event CloudDataHasBeenRead _CloudDataHasBeenRead;

        //We send this event so that FrostCompare can stop listening for the read operation to complete
        //  public delegate void CompareReadOperationHasFailed();
        //  public static event CompareReadOperationHasFailed _CompareReadOpFailed;

        public bool LoadingForCompare { get; private set; }

        private string SaveFileName;

        /// <summary>
        /// /// Loads the game from local, local and cloud or only from the cloud
        /// based on the provided parameters
        /// </summary>
        /// <param name="FileName" The name of the file to be saved></param>
        /// <param name="ForceLocal" If true the method will only load the save from local disk></param>
        /// <param name="ForComparing" If true it will mark the method as loading for comparing purpose between local and cloud></param>
        /// <returns></returns>
        public T LoadTheGame(string FileName, bool ForceLocal = true, bool ForComparing = false)
        {
            SaveFileName = FileName;
            LoadingForCompare = ForComparing;

            return DoLocalLoad();
        }

        //Loads the game locally and returns the loaded instance
        private T DoLocalLoad()
        {
            FileStream file = null;
            try
            {
                file = File.Open(persistentpath + "/" + SaveFileName, FileMode.Open);
                T save = MessagePackSerializer.Deserialize<T>(file);
                file.Close();
                UnityEngine.Console.Log("SaveGame Successfully Read");
                return save;
            }
            catch (Exception E)
            {
                if (file != null)
                    file.Close();

                UnityEngine.Console.LogWarning(E);
                ReadOperationFailed<T>(OperationFailed.ReadFromLocal, this);
                //    T o = (T)Activator.CreateInstance(typeof(T));
                // return o;
                return default(T);
            }
        }
    }
}