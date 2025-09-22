
using System;
using System.Threading.Tasks;

//using TheKnights.FaceBook;
using UnityEngine;
using UnityEngine.Events;

namespace TheKnights.SaveFileSystem
{
    [CreateAssetMenu(fileName = "SaveManager", menuName = "ScriptableObjects/SaveManager", order = 1)]
    public class SaveManager : ScriptableObject
    {
        [SerializeField] private GameEvent[] eventsToSaveTheGame;
        //      [SerializeField] private FaceBookManager faceBookManager;

        #region Singleton

        private static SaveManager instance;

        public static SaveManager Instance
        { get { if (instance == null) instance = Resources.Load("SaveManager") as SaveManager; return instance; } }

        #endregion Singleton

        /// <summary>
        /// The Main Game Save File instance
        /// </summary>

        [SerializeField] private InventorySystem playerInventory, defaultInventory;
        [SerializeField] private DailyGoalsManagerSO dailyGoalsManager;

        private static bool CloudCompareCompletedMainSave = false;           //The cloud save process of mainsavefile has been completed. Doesn't matter if failed.
                                                                             // private static bool CloudCompareCompletedAchivements = false;       //The cloud save process of achivements save file has been completed. Doesn't matter if failed.
                                                                             //   private static bool HasAlreadyRecievedCompareCall = false;          //A call to the API for cloud save compare has been made once

        public MainSaveFile MainSaveFile { get; private set; }

        /// <summary>
        /// Invoked when the save file is read from ROM upon launching the game
        /// </summary>
        [HideInInspector] public UnityEvent OnSessionLoaded = new UnityEvent();

        [System.NonSerialized] private bool isPendingSaveRequestAvailable;
        [System.NonSerialized] private bool isCurrentlySavingGame;

        private void OnEnable()
        {
            isPendingSaveRequestAvailable = false;
            isCurrentlySavingGame = false;

            for (int i = 0; i < eventsToSaveTheGame.Length; i++)
            {
                GameEvent gameEvent = eventsToSaveTheGame[i];
                gameEvent.TheEvent.AddListener(HandleSaveGameEvent);
            }

            // faceBookManager.OnUserFaceBookProfileCreated.AddListener(UpdatePlayerName);
        }

        /// <summary>
        /// Loads/Initializes The save game one time when the game is launched
        /// </summary>
        public async void SetupTheSaveGame()
        {
            // The returned value can either be a completely new instance or a previosuly saved instance but can never be null
            Task<MainSaveFile> task = LoadTheGame.LoadFileFromROM<MainSaveFile>(MainSaveFile.MainSaveGame);

            await task;

            MainSaveFile = task.Result;

            if (MainSaveFile == null)
            {
                MainSaveFile = new MainSaveFile(defaultInventory, dailyGoalsManager);
                MainSaveFile.FileSaved = true;
                AsyncOperationSaveSystem handle = SaveTheGame.SaveTheFile(MainSaveFile, MainSaveFile.MainSaveGame);

                await handle.Task;
            }
            else
            {
                MainSaveFile.UpdateMainSaveFileIfRequired(defaultInventory, dailyGoalsManager);
                //Action toPerformIfRetrievedFromOld = null;

                //try
                //{
                //    toPerformIfRetrievedFromOld = MainSaveFile.ConvertFromOldSaveIfRequired(defaultInventory);

                //    // Something was retrieved from old file
                //    if (toPerformIfRetrievedFromOld != null)
                //    {
                //        AsyncOperationSaveSystem handle = SaveTheGame.SaveTheFile(MainSaveFile, MainSaveFile.MainSaveGame);
                //        await handle.Task;
                //        toPerformIfRetrievedFromOld.Invoke();
                //    }
                //}
                //catch(Exception e)
                //{
                //    toPerformIfRetrievedFromOld = null;
                //    UnityEngine.Debug.LogError("Ax exception occurred while trying to retrieve data form old save file. " + e.Message);
                //}
            }

            dailyGoalsManager.SetUpValuesForGame(MainSaveFile);

            playerInventory.SetUpValuesForGame(MainSaveFile);

            //  UnityEngine.Console.Log("Startup Save Initialized with save file version " + MainSaveFile.FileVersion);

            //     UnityEngine.Console.Log($"Tutorial Completion Status At Start " + MainSaveFile.TutorialHasCompleted);

            // Game has been successfully loaded
            OnSessionLoaded.Invoke();
        }

        /// <summary>
        /// Serializes and writes the save file to the ROM
        /// </summary>
        //public AsyncOperationSaveSystem SaveGame(float ThisSessionTime = 0)
        //{
        //    UnityEngine.Console.Log("Before Saving Value of string " + MainSaveFile.sessionDate);

        //    AsyncOperationSaveSystem handle = SaveTheGame.SaveTheFile(MainSaveFile, MainSaveFile.MainSaveGame);
        //    return handle;
        //}

        private void CopyingDataFromInventoryToSaveFile()
        {
            //for (int i=0; i < playerInventory.intKeyItems.Count; i++)
            //{
            //    //MainSaveFile.gameCurrency[i].itemName = playerInventory.currencyItems[i].itemName;
            //    MainSaveFile.gameIntItems[i].itemValue = playerInventory.currencyItems[i].itemValue;

            //    UnityEngine.Console.Log($"Saving { MainSaveFile.gameCurrency[i].itemName} with value of { playerInventory.currencyItems[i].itemValue}");
            //}

            for (int i = 0; i < playerInventory.intKeyItems.Count; i++)
            {
                //MainSaveFile.gameIntItems[i].itemName = playerInventory.otherIntItems[i].itemName;
                MainSaveFile.gameIntItems[i].itemValue = playerInventory.intKeyItems[i].itemValue;
            }

            for (int i = 0; i < playerInventory.gameCars.Count; i++)
            {
                //MainSaveFile.gameIntItems[i].itemName = playerInventory.otherIntItems[i].itemName;
                MainSaveFile.gameVehicles[i].isObjectUnlocked = playerInventory.gameCars[i].isObjectUnlocked;
            }

            foreach (var pair in playerInventory.CharactersFigurinesAvailable)
            {
                MainSaveFile.CharactersFigurinesAvailable[pair.Key] = pair.Value;
            }
        }

        //private void UpdatePlayerName(FaceBookUserProfile faceBookUserProfile)
        //{
        //    if (faceBookUserProfile == null)
        //        return;

        //    MainSaveFile.leaderBoardUserName = faceBookUserProfile.userName;
        //}

        private void HandleSaveGameEvent(GameEvent gameEvent)
        {
            SaveGame(0, false);
        }

        // Saves the MainSaveFile
        public async void SaveGame(float ThisSessionTime = 0, bool AlsoToCloud = false)
        {
            if (isCurrentlySavingGame)
            {
                isPendingSaveRequestAvailable = true;
                return;
            }

            if (isPendingSaveRequestAvailable)
            {
                return;
            }

            try
            {
                isCurrentlySavingGame = true;

               
                CopyingDataFromInventoryToSaveFile();

                var handle = SaveTheGame.SaveTheFile(MainSaveFile, MainSaveFile.MainSaveGame, ThisSessionTime, AlsoToCloud && CloudCompareCompletedMainSave);
                await handle.Task;

               
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("An exception occurred while trying to save the game " + e.Message);
            }
            finally
            {
                isCurrentlySavingGame = false;

                if (isPendingSaveRequestAvailable)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        if (isPendingSaveRequestAvailable)
                        {
                            isPendingSaveRequestAvailable = false;
                            SaveGame(0, false);
                        }
                    });
                }
            }
        }
    }
}