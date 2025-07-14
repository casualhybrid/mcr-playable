using System;
using System.IO;

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
    public class FrostWrite<T> : FrostReadWrite
    {
        //This event is sent only if the save was performed for syncronization/compares purpose
        //We can only do cloud saves if this event is sent
        public delegate void LatestSaveAquired();

        //  public static event LatestSaveAquired _LatestSaveAcqiured;

        public delegate void CloudDataPulled(T instance);

        //  public static event CloudDataPulled _CloudDataPulled;

        // private T InstanceToSave;
        private string SaveFileName;

        //  private float timeplayedseconds;
        public static bool IsWritingLocal, isWritingCloud;

        /// <summary>
        /// //// Saves the game either to local, local and cloud or only to cloud based on
        /// the provided parameteres.
        /// </summary>
        /// <param name="_InstanceToSave" The instance you want to serialize and save></param>
        /// <param name="FileName" The name that you want to save your file as></param>
        /// <param name="SaveToCloudAlso" Also tries to save to cloud along with local></param>
        /// <param name="SaveOnlyToCloud" Only save the file to the cloud></param>
        public TheKnights.SaveFileSystem.Status SaveTheGame(byte[] instanceToSaveBytes, string FileName, float ThisSessionTime, bool SaveToCloudAlso = false, bool SaveOnlyToCloud = false, bool ForSyncPurpose = false)
        {
            UnityEngine.Console.Log("Write operation called");

            //If we want to write to local only or to both local and cloud
            //Note ** : In this case, cloud will only be written if local can be written
            if (IsWritingLocal)
            {
                WriteOperationFailed<T>(OperationFailed.AlreadyWriting);
                return TheKnights.SaveFileSystem.Status.Failed;
            }
            else if (!IsWritingLocal)
            {
                IsWritingLocal = true;
            }

            UnityEngine.Console.Log("Passed Save Barriers");

            //  InstanceToSave = _InstanceToSave;
            SaveFileName = FileName;

            //var TimeStamp = InstanceToSave.GetType().GetProperty(TimeStampConst);

            //if (TimeStamp == null)
            //{
            //    UnityEngine.Console.LogError("The instance to serialize must have a public property called " + TimeStampConst + " of type float. \n " +
            //        "ForExample: public float TimeStamp {get {return xxx;} set{ xxx=value; }}");

            //    IsWritingLocal = false;
            //    return;
            //}

            //Update the timestamp as well before the file is saved either locally or on cloud

            //  float ThisSessionTime = Time.realtimeSinceStartup;
            //float TimeStampValueCurrent = (float)TimeStamp.GetValue(InstanceToSave);

            //  SetTotalPlayedTime();
            //  float time = TimeStampValueCurrent + ThisSessionTime;
            // TimeStamp.SetValue(InstanceToSave, time);
            //  timeplayedseconds = time;

            return DoLocalSave(instanceToSaveBytes, ForSyncPurpose);
        }

        private TheKnights.SaveFileSystem.Status DoLocalSave(byte[] instanceToSaveBytes, bool ForSyncPurpose = false)
        {
            try
            {
                UnityEngine.Console.Log("Entered Local Save");
                FileStream file = null;

                using (file = File.Create(persistentpath + "/" + SaveFileName))
                {
                    byte[] bytes = instanceToSaveBytes;
                    file.Write(bytes, 0, bytes.Length);
                    IsWritingLocal = false;
                    UnityEngine.Console.Log("Local Save Successfull");

                    return TheKnights.SaveFileSystem.Status.Succeeded;
                }
            }
            catch (Exception E)
            {
                UnityEngine.Console.LogWarning(E.ToString()); IsWritingLocal = false; WriteOperationFailed<T>(OperationFailed.WriteToLocal);

                return TheKnights.SaveFileSystem.Status.Failed;
            }
        }
    }
}