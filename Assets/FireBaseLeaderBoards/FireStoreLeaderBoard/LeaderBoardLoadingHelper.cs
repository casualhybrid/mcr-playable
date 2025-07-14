using TheKnights.SaveFileSystem;
using UnityEngine;

public class LeaderBoardLoadingHelper : Singleton<LeaderBoardLoadingHelper>
{
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private TheKnights.LeaderBoardSystem.LeaderBoardManager leaderBoardManager;
    [SerializeField] private GameEvent tutorialHasCompleted;

    private bool isUserAuthenticatedToFireBase;
    private bool isSaveFileInitialized;

    private bool isInitiallyRequstedWeeklyLeaderBoard;

    protected override void Awake()
    {
        base.Awake();
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        FireBaseAuthentication.OnUserAuthenticatedToFireBase += MarkFireBaseAuthenticatedAndLoadLeaderBoard;
        saveManager.OnSessionLoaded.AddListener(MarkSaveGameInitializedAndLoadLeaderBoard);
    }

    private void DeSubscribeToEvents()
    {
        FireBaseAuthentication.OnUserAuthenticatedToFireBase -= MarkFireBaseAuthenticatedAndLoadLeaderBoard;
        saveManager.OnSessionLoaded.RemoveListener(MarkSaveGameInitializedAndLoadLeaderBoard);
    }

    private void MarkFireBaseAuthenticatedAndLoadLeaderBoard()
    {
        isUserAuthenticatedToFireBase = true; LoadWeeklyLeaderBoardIfConditionsMeet();
    }

    private void MarkSaveGameInitializedAndLoadLeaderBoard()
    {
        isSaveFileInitialized = true; LoadWeeklyLeaderBoardIfConditionsMeet();
    }

    private void LoadWeeklyLeaderBoardIfConditionsMeet()
    {
        if (isInitiallyRequstedWeeklyLeaderBoard)
            return;

        if (!isUserAuthenticatedToFireBase || !isSaveFileInitialized)
            return;

        if (!saveManager.MainSaveFile.TutorialHasCompleted)
        {
            tutorialHasCompleted.TheEvent.AddListener(RequestLeaderBoardLoad);
            return;
        }

        RequestLeaderBoardLoad();
    }

    private void RequestLeaderBoardLoad(GameEvent gameEvent = null)
    {
        if (isInitiallyRequstedWeeklyLeaderBoard)
            return;

        isInitiallyRequstedWeeklyLeaderBoard = true;
        leaderBoardManager.GetLeaderBoardGroupDetails();
        DeSubscribeToEvents();
    }
}
