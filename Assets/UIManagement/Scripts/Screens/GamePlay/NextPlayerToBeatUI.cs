using System;
using System.Threading.Tasks;
using TheKnights.LeaderBoardSystem;
using TheKnights.LeaderBoardSystem.FaceBook;
using UnityEngine;
using UnityEngine.Events;

public class PlayerToBeat
{
    public Texture2D ProfilePicTex;
    public string PlayerName;
    public int Score;
    public bool isAnonymous;
}

public class PlayerToBeatBattle
{
    public PlayerToBeat FirstPlayer;
    public PlayerToBeat SecondPlayer;
    public LeaderBoardType LeaderBoardType;
}

public class NextPlayerToBeatUI : MonoBehaviour
{
    public static bool IsPlayerToBeatActive;
    public static LeaderBoardType ActiveBattleLeaderBoardType { get; private set; }
    public static PlayerToBeatBattle PlayerToBeatBattle { get; private set; }

    [SerializeField] private UnityEvent OnPlayerBeaten;

    [SerializeField] private FaceBookLeaderBoard faceBookLeaderBoard;
    [SerializeField] private TheKnights.LeaderBoardSystem.LeaderBoardManager leaderBoardManager;

    [SerializeField] private PlayerToBeatDisplay faceBookPlayerToBeatDisplay;
    [SerializeField] private PlayerToBeatDisplay weeklyLeaderBoardPlayerToBeatDisplay;

    [SerializeField] private GameEvent gameHasStarted;

    private FaceBookLeaderBoardUserData lastPlayerToBeatFaceBook;
    private LeaderBoardGroupUserData lastPlayerWeeklyLeaderBoard;

    private FaceBookLeaderBoardUserData localPlayerDataFaceBook;
    private LeaderBoardGroupUserData localPlayerDataWeekly;

    private LeaderBoardAsyncHandler<LeaderBoardQueryResult<LeaderBoardGroupQueryResponseData>> weeklyLeaderBoardHandle;

    private bool isDestroyed;

    private void Start()
    {
        gameHasStarted.TheEvent.AddListener(SetupPlayerToBeatUI);

        faceBookPlayerToBeatDisplay.OnPlayerBeaten += HandlePlayerBeaten;
        weeklyLeaderBoardPlayerToBeatDisplay.OnPlayerBeaten += HandlePlayerBeaten;
    }

    private void HandlePlayerBeaten()
    {
        if (isDestroyed)
            return;

        IsPlayerToBeatActive = false;
        PlayerToBeatBattle = null;

        OnPlayerBeaten.Invoke();
        SetupPlayerToBeatUI();
    }

    private void OnDestroy()
    {
        isDestroyed = true;
        PlayerToBeatBattle = null;
        IsPlayerToBeatActive = false;

        gameHasStarted.TheEvent.RemoveListener(SetupPlayerToBeatUI);

        if (weeklyLeaderBoardHandle != null)
        {
            weeklyLeaderBoardHandle.OnLeaderBoardOperationDone -= ProcessWeeklyLeaderBoardData;
        }

    }

    private async void SetupPlayerToBeatUI(GameEvent gameEvent = null)
    {
        try
        {
            await InitializePlayerToBeatFromFaceBookLeaderBoard();
        }
        catch (Exception e)
        {
            UnityEngine.Console.Log($"Failed to set facebook player to beat. Reason: {e}");
        }

        if (isDestroyed)
            return;

        if (IsPlayerToBeatActive)
        {
            ActiveBattleLeaderBoardType = LeaderBoardType.FaceBook;

            PlayerToBeat firstPlayer = new PlayerToBeat() { PlayerName = localPlayerDataFaceBook.name, ProfilePicTex = localPlayerDataFaceBook.profilePicTex, Score = localPlayerDataFaceBook.score };
            PlayerToBeat secondPlayer = new PlayerToBeat() { PlayerName = lastPlayerToBeatFaceBook.name, ProfilePicTex = lastPlayerToBeatFaceBook.profilePicTex, Score = lastPlayerToBeatFaceBook.score };

            PlayerToBeatBattle = new PlayerToBeatBattle() { FirstPlayer = firstPlayer, SecondPlayer = secondPlayer, LeaderBoardType = ActiveBattleLeaderBoardType };

            return;
        }

        InitializePlayerToBeatFromWeeklyLeaderBoard();
    }

    private async Task InitializePlayerToBeatFromFaceBookLeaderBoard()
    {
        FaceBookLeaderBoardData data = await faceBookLeaderBoard.GetFaceBookLeaderBoardData();

        // Maybe the scene changed while we were waiting for task
        if (isDestroyed)
            return;

        FaceBookLeaderBoardUserData playerToBeat = null;

        if (data == null || !data.isValid || data.Data.Count <= 1)
            return;

        for (int i = 0; i < data.Data.Count; i++)
        {
            FaceBookLeaderBoardUserData entry = data.Data[i];

            // Player is at top of facebook leaderboard
            if (i == 0 && (entry == data.LocalPlayerData || entry == lastPlayerToBeatFaceBook))
            {
                UnityEngine.Console.Log("Player is already at top of the FaceBook leaderboard!");
                break;
            }

            int nxtPlayerIndex = i + 1;
            if (nxtPlayerIndex < data.Data.Count)
            {
                FaceBookLeaderBoardUserData nextPlayerData = data.Data[nxtPlayerIndex];
                if (nextPlayerData == data.LocalPlayerData || nextPlayerData == lastPlayerToBeatFaceBook)
                {
                    playerToBeat = entry;
                    localPlayerDataFaceBook = data.LocalPlayerData;
                    break;
                }
            }
        }

        if (playerToBeat != null)
        {
            faceBookPlayerToBeatDisplay.gameObject.SetActive(true);
            weeklyLeaderBoardPlayerToBeatDisplay.gameObject.SetActive(false);

            faceBookPlayerToBeatDisplay.Initialize(playerToBeat.profilePicTex, playerToBeat.name, playerToBeat.score);
            IsPlayerToBeatActive = true;
            lastPlayerToBeatFaceBook = playerToBeat;
        }
    }

    private void InitializePlayerToBeatFromWeeklyLeaderBoard()
    {
        weeklyLeaderBoardHandle = leaderBoardManager.GetLeaderBoardGroupDetails();

        weeklyLeaderBoardHandle.OnLeaderBoardOperationDone += ProcessWeeklyLeaderBoardData;
    }

    private void ProcessWeeklyLeaderBoardData(LeaderBoardAsyncHandler<LeaderBoardQueryResult<LeaderBoardGroupQueryResponseData>> handle)
    {
        weeklyLeaderBoardHandle.OnLeaderBoardOperationDone -= ProcessWeeklyLeaderBoardData;

        var result = handle.Result;
        LeaderBoardGroupUserData playerToBeat = null;

        if (!result.IsValid || result.ResponseBody == null || result.ResponseBody.currentGroupInfo == null || !result.ResponseBody.currentGroupInfo.IsValid)
            return;

        GroupInfo groupInfo = result.ResponseBody.currentGroupInfo;

        for (int i = 0; i < groupInfo.usersData.Count; i++)
        {
            LeaderBoardGroupUserData entry = groupInfo.usersData[i];

            // Player is at top of facebook leaderboard
            if (i == 0 && (entry == groupInfo.localPlayerData || entry == lastPlayerWeeklyLeaderBoard))
            {
                UnityEngine.Console.Log("Player is already at top of the weekly leaderboard!");
                break;
            }

            int nxtPlayerIndex = i + 1;
            if (nxtPlayerIndex < groupInfo.usersData.Count)
            {
                LeaderBoardGroupUserData nextPlayerData = groupInfo.usersData[nxtPlayerIndex];
                if (nextPlayerData == groupInfo.localPlayerData || nextPlayerData == lastPlayerWeeklyLeaderBoard)
                {
                    localPlayerDataWeekly = groupInfo.localPlayerData;
                    playerToBeat = entry;
                    break;
                }
            }
        }

        if (playerToBeat != null)
        {
            faceBookPlayerToBeatDisplay.gameObject.SetActive(false);
            weeklyLeaderBoardPlayerToBeatDisplay.gameObject.SetActive(true);

            weeklyLeaderBoardPlayerToBeatDisplay.Initialize(playerToBeat.ProfileImageTexture, playerToBeat.userName, playerToBeat.score);
            IsPlayerToBeatActive = true;
            lastPlayerWeeklyLeaderBoard = playerToBeat;
        }

        if (!IsPlayerToBeatActive)
        {
            weeklyLeaderBoardPlayerToBeatDisplay.gameObject.SetActive(false);
            faceBookPlayerToBeatDisplay.gameObject.SetActive(false);
        }
        else
        {
            ActiveBattleLeaderBoardType = LeaderBoardType.Weekly;

            PlayerToBeat firstPlayer = new PlayerToBeat() { PlayerName = localPlayerDataWeekly.userName, ProfilePicTex = localPlayerDataWeekly.ProfileImageTexture, Score = localPlayerDataWeekly.score, isAnonymous = localPlayerDataWeekly.isAnonymous };
            PlayerToBeat secondPlayer = new PlayerToBeat() { PlayerName = lastPlayerWeeklyLeaderBoard.userName, ProfilePicTex = lastPlayerWeeklyLeaderBoard.ProfileImageTexture, Score = lastPlayerWeeklyLeaderBoard.score };

            PlayerToBeatBattle = new PlayerToBeatBattle() { FirstPlayer = firstPlayer, SecondPlayer = secondPlayer, LeaderBoardType = ActiveBattleLeaderBoardType };
        }
    }
}