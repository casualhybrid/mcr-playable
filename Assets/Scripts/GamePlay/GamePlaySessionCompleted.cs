//using TheKnights.FaceBook;
//using TheKnights.FaceBook.LeaderBoard;
using TheKnights.SaveFileSystem;
using UnityEngine;
using TheKnights.LeaderBoardSystem.FaceBook;

public class GamePlaySessionCompleted : MonoBehaviour
{
    public static bool IsHighScoreMadeLastSession { get; private set; }

    // [SerializeField] private FaceBookLeaderBoard faceBookLeaderBoard;
    [SerializeField] private GamePlaySessionInventory gamePlaySessionInventory;

    [SerializeField] private GamePlaySessionData gamePlaySessionData;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private GameEvent sessionCompletedEvent;
    [SerializeField] private GameOverManager GameOverManager;
    [SerializeField] private PlayerLevelingSystem playerLevelingSystem;
    [SerializeField] private TheKnights.LeaderBoardSystem.LeaderBoardManager leaderBoardManager;
    [SerializeField] private FaceBookLeaderBoard faceBookLeaderBoard;
    //   [SerializeField] private FaceBookManager faceBookManager;

    private void Awake()
    {
        sessionCompletedEvent.TheEvent.AddListener(HandleSessionHasCompleted);
        IsHighScoreMadeLastSession = false;
    }

    private void OnDestroy()
    {
        sessionCompletedEvent.TheEvent.RemoveListener(HandleSessionHasCompleted);
    }

    private void HandleSessionHasCompleted(GameEvent gameEvent)
    {
        float currentHighScore = saveManager.MainSaveFile.playerHighScore;

        // UnityEngine.Console.Log($"Current Score {currentHighScore} . This session score {gamePlaySessionData.DistanceCoveredInMeters}");

        if (gamePlaySessionData.DistanceCoveredInMeters > currentHighScore)
        {
            IsHighScoreMadeLastSession = true;
            //string timesHighScoreObtainedString = "timesHighScoreObtainedString";
            //int timesHighScoreObtained = PlayerPrefs.GetInt(timesHighScoreObtainedString, 0);
            //PlayerPrefs.SetInt(timesHighScoreObtainedString, ++timesHighScoreObtained);

            saveManager.MainSaveFile.playerHighScore = gamePlaySessionData.DistanceCoveredInMeters;

            HighScoreAnalyticsEvent();

            // Can be written outside this IF statement as the leaderboard score update function takes care of it
            // Update score to facebook database
            //    faceBookLeaderBoard.UpdateLeaderBoardHighScore((int)saveManager.MainSaveFile.playerHighScore);
        }

        //TEMP
        // Can be written outside this IF statement as the leaderboard score update function takes care of it
        // Update score to facebook database
        //faceBookLeaderBoard.UpdateLeaderBoardHighScore((int)saveManager.MainSaveFile.playerHighScore);

        int totalPlayTime = PlayerPrefs.GetInt("TotalPlayTime", 0);
        PlayerPrefs.SetInt("TotalPlayTime", totalPlayTime + (int)gamePlaySessionData.timeElapsedSinceSessionStarted);

        // Update leaderboard score
        UpdateLeaderBoardDataBase();

        // Update facebook leaderboard score
        UpdateFaceBookLeaderBoard();

        // Update player level in session inventory
        playerLevelingSystem.CalculateDisctanceBasedXP();

        gamePlaySessionInventory.CopyGamePlaySessionItemsToSaveFile();

        // Save The Game
        saveManager.SaveGame();
    }

    private void UpdateLeaderBoardDataBase()
    {
        leaderBoardManager.SubmitHighScoreToPlayersGroup((int)gamePlaySessionData.DistanceCoveredInMeters);
    }

    private void UpdateFaceBookLeaderBoard()
    {
        faceBookLeaderBoard.SubmitHighScoreToLeaderBoard((int)gamePlaySessionData.DistanceCoveredInMeters, leaderBoardManager.GetPlayerProfilePictureData());
    }

    private void HighScoreAnalyticsEvent()
    {
        AnalyticsManager.CustomData("GamePlayScreen_HighScore");
    }
}