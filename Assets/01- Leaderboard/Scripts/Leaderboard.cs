using deVoid.UIFramework;
using DG.Tweening;
using Knights.UISystem;
using System.Collections;
using System.Collections.Generic;
using TheKnights.LeaderBoardSystem;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] private UnityEvent onLeaderBoardSetupDone;
    [SerializeField] private AWindowController windowController;

    [SerializeField] private TheKnights.LeaderBoardSystem.LeaderBoardManager leaderBoardManager;
    [SerializeField] private GameObject[] leaderBoardRankBars;
    [SerializeField] private Transform[] ranksContent;
    [SerializeField] private Color[] rankEntitiesColor;
    [SerializeField] private LeaderboardPrefab leaderBoardEntityObject;

    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private ScrollRect topRunScroll;

    [SerializeField] private Image countryFlagImage;
    [SerializeField] private LayoutRefresher countryLayoutRefresher;
    [SerializeField] private TextMeshProUGUI countryNameText;
    [SerializeField] private TextMeshProUGUI weekNumberText;
    [SerializeField] private TextMeshProUGUI weekDaysRemainingText;

    [SerializeField] private GameEvent onUserClaimedLeaderBoardReward;

    [SerializeField] private GameObject connectionErrorPanel;

    private VerticalLayoutGroup topRunContentVerticalLayout;

    private bool isLoading;
    private bool isLoaded;

    private int currentlyLoadedGroupDataVersion;

    private LeaderboardPrefab localPlayerUIEntity;
    private LeaderBoardRankEntityRewardInfoUI[] leaderBoardRankEntityRewardInfoUI;

    private LeaderBoardAsyncHandler<LeaderBoardQueryResult<LeaderBoardGroupQueryResponseData>> handler;

    private Coroutine leaderboardPopulateRoutine;

    private bool isDestroyed;
    private GroupInfo currentlyDisplayingGroupInfo;

    private void Awake()
    {
        topRunContentVerticalLayout = topRunScroll.content.GetComponent<VerticalLayoutGroup>();
        leaderBoardRankEntityRewardInfoUI = topRunScroll.content.GetComponentsInChildren<LeaderBoardRankEntityRewardInfoUI>(true);
    }

    private void Start()
    {
        CoroutineRunner.Instance.StartCoroutine(InitializeLeaderBoardRanksRewardUI());
    }

    private void OnEnable()
    {
        StartCoroutine(ReloadContentLayoutAndSnapToPlayer());

        SetupLeaderBoard();

        //   onUserClaimedLeaderBoardReward.TheEvent.AddListener(SetupLeaderBoard);
        leaderBoardManager.OnPlayerGroupProfileUpdated += UpdatePlayerLocalProfileUI;
        leaderBoardManager.OnLeaderBoardGroupDetailsUpdated += HandleLeaderBoardUpdated;
    }

    private void OnDisable()
    {
        // onUserClaimedLeaderBoardReward.TheEvent.RemoveListener(SetupLeaderBoard);
        leaderBoardManager.OnPlayerGroupProfileUpdated -= UpdatePlayerLocalProfileUI;
        leaderBoardManager.OnLeaderBoardGroupDetailsUpdated -= HandleLeaderBoardUpdated;
    }

    private void OnDestroy()
    {
        if (handler != null)
        {
            handler.OnLeaderBoardOperationDone -= ChooseAndDisplayLeaderBoardData;
        }

        isDestroyed = true;
    }

    public void LoadLeaderBoard()
    {
        SetupLeaderBoard();
    }

    private void HandleLeaderBoardUpdated(LeaderBoardGroupQueryResponseData data)
    {
        SetupLeaderBoard();
    }

    private void SetupLeaderBoard(GameEvent gameEvent = null)
    {
        if (isLoaded)
        {
            if (leaderBoardManager.CurrentGroupInformation != null && leaderBoardManager.CurrentGroupInformation.Version > currentlyLoadedGroupDataVersion)
            {
                UnityEngine.Console.Log($"Version {currentlyLoadedGroupDataVersion} is less than the cached {leaderBoardManager.CurrentGroupInformation.Version} version of leaderboard. " +
                    $"Reloading leaderboard with cached new values");

                isLoaded = false;
            }
            else
            {
                if(currentlyDisplayingGroupInfo != null && currentlyDisplayingGroupInfo.IsValid && currentlyDisplayingGroupInfo.daysLeftInWeek != -1 && currentlyDisplayingGroupInfo.daysLeftInWeek <= 1)
                {
                    weekDaysRemainingText.text = currentlyDisplayingGroupInfo.GetRemainingTimeTillLeaderBoardRefresh() + " left";
                }

                return;
            }
        }

        if (leaderBoardManager.CurrentRewardedGroupBeingClaimed != null)
        {
            UnityEngine.Console.LogWarning("Leaderboard reward is already in progress. Attempt to initiate another was made!");
            return;
        }

        RequestGroupQueryData();
    }

    private void RequestGroupQueryData()
    {
        if (isLoading)
            return;

        ResetTheBoard();
        EnableLoadingGroupDetails();
        DisableConnectionErrorDetails();

        handler = leaderBoardManager.GetLeaderBoardGroupDetails();
        handler.OnLeaderBoardOperationDone += ChooseAndDisplayLeaderBoardData;
    }

    private void ChooseAndDisplayLeaderBoardData(LeaderBoardAsyncHandler<LeaderBoardQueryResult<LeaderBoardGroupQueryResponseData>> response)
    {
        handler.OnLeaderBoardOperationDone -= ChooseAndDisplayLeaderBoardData;
        handler = null;

        var result = response.Result;

        if (!result.IsValid || result.IsError)
        {
            DisableLoadingGroupDetails();
            EnableConnectionErrorDetails();
            return;
        }

        var responseBody = result.ResponseBody;

        GroupInfo pendingRewards = responseBody.pendingRewardInfo;
        GroupInfo currentGroupData = responseBody.currentGroupInfo;

        GroupInfo groupInfoToDisplay = pendingRewards != null && pendingRewards.IsValid ? pendingRewards : currentGroupData != null && currentGroupData.IsValid ? currentGroupData : null;

        if (groupInfoToDisplay == null)
        {
            DisableLoadingGroupDetails();
            EnableConnectionErrorDetails();
            return;
        }

        // Cache the loaded version
        currentlyLoadedGroupDataVersion = responseBody.Version;

        SetLeaderBoardMetaData(groupInfoToDisplay);
        PopulateLeaderBoardData(groupInfoToDisplay);

        isLoaded = true;
        currentlyDisplayingGroupInfo = groupInfoToDisplay;

        EnableTournamentEndWindowIfIsRewardGroup(groupInfoToDisplay == pendingRewards, groupInfoToDisplay);

        DisableLoadingGroupDetails();
    }

    private IEnumerator InitializeLeaderBoardRanksRewardUI()
    {
        // Set rank reward information UI
        for (int i = 0; i < leaderBoardRankEntityRewardInfoUI.Length; i++)
        {
            LeaderBoardRank rank = (LeaderBoardRank)i;

            leaderBoardRankEntityRewardInfoUI[i].SetRewardsInformation(rank);

            yield return null;
        }
    }

    private void SetLeaderBoardMetaData(GroupInfo groupInfo)
    {
        countryNameText.text = groupInfo.userCountry;
        weekNumberText.text = "Week " + groupInfo.weekNumber;
        countryFlagImage.sprite = groupInfo.countrySprite;

        if (groupInfo.daysLeftInWeek == -1)
        {
            weekDaysRemainingText.text = "Week Done";
        }
        else if (groupInfo.daysLeftInWeek <= 1)
        {
            weekDaysRemainingText.text = groupInfo.GetRemainingTimeTillLeaderBoardRefresh() + " left";
        }
        else
        {
            weekDaysRemainingText.text = "Days Left " + groupInfo.daysLeftInWeek;
        }

        // Set rank reward information UI
        for (int i = 0; i < leaderBoardRankEntityRewardInfoUI.Length; i++)
        {
            LeaderBoardRank rank = (LeaderBoardRank)i;
            string scoreToBeat = groupInfo.leaderBoardRankScoreThresholds[i].ToString();

            leaderBoardRankEntityRewardInfoUI[i].SetInformation(scoreToBeat, rank);
        }

        countryLayoutRefresher.ResfreshTheLayout();
    }

    private void PopulateLeaderBoardData(GroupInfo groupInfo)
    {
        if (isDestroyed)
            return;

        EnableRankBars();

        if (leaderboardPopulateRoutine != null)
        {
            CoroutineRunner.Instance.StopCoroutine(leaderboardPopulateRoutine);
            leaderboardPopulateRoutine = null;
        }

        leaderboardPopulateRoutine = CoroutineRunner.Instance.StartCoroutine(PopulateLeaderBoardDataRoutine(groupInfo));
    }

    private IEnumerator PopulateLeaderBoardDataRoutine(GroupInfo groupInfo)
    {
        List<LeaderBoardGroupUserData> body = groupInfo.usersData;

        for (int i = 0; i < body.Count; i++)
        {
            var entity = Instantiate(leaderBoardEntityObject, ranksContent[(int)body[i].LeaderBoardRank]);
            bool isLocal = body[i].myID == groupInfo.localPlayerData.myID;

            if (isLocal)
            {
                localPlayerUIEntity = entity;
            }

            SetupLeaderBoardEntityInformation(entity, body[i], isLocal);

            yield return null;

            if (isDestroyed)
                break;
        }

        if (this.gameObject.activeInHierarchy)
        {
            StartCoroutine(ReloadContentLayoutAndSnapToPlayer());
        }
    }

    private IEnumerator ReloadContentLayoutAndSnapToPlayer()
    {
        topRunContentVerticalLayout.enabled = false;
        yield return null;
        topRunContentVerticalLayout.enabled = true;

        if (localPlayerUIEntity == null)
            yield break;

        yield return null;

        Transform localPlayerT = localPlayerUIEntity.transform;
        Transform transformToSnap = localPlayerT;

        if (localPlayerT.GetSiblingIndex() == 1)
        {
            transformToSnap = localPlayerT.parent.GetChild(0);
        }

        Vector2 val = topRunScroll.GetSnappedValueScrollToRect(transformToSnap, false, true);
        topRunScroll.content.DOLocalMove(val, 800f).SetEase(Ease.InOutQuart).SetUpdate(true).SetSpeedBased(true);

        onLeaderBoardSetupDone.Invoke();
    }

    public void ResetTheBoard()
    {
        isLoaded = false;
        currentlyDisplayingGroupInfo = null;

        foreach (var content in ranksContent)
        {
            var childCount = content.childCount;

            for (int i = childCount - 1; i >= 0; i--)
            {
                Transform child = content.GetChild(i);

                if (child.GetComponent<LeaderBoardRankEntity>() != null)
                    continue;

                Destroy(child.gameObject);
            }
        }

        DisableRankBars();
    }

    private void UpdatePlayerLocalProfileUI(LeaderBoardGroupUserData localUserData)
    {
        if (localPlayerUIEntity == null)
            return;

        SetupLeaderBoardEntityInformation(localPlayerUIEntity, localUserData, true);
    }

    private void EnableLoadingGroupDetails()
    {
        isLoading = true;
        loadingPanel.SetActive(true);
    }

    private void DisableLoadingGroupDetails()
    {
        isLoading = false;
        loadingPanel.SetActive(false);
    }

    private void EnableConnectionErrorDetails()
    {
        connectionErrorPanel.SetActive(true);
    }

    private void DisableConnectionErrorDetails()
    {
        connectionErrorPanel.SetActive(false);
    }

    private void DisableRankBars()
    {
        for (int i = 0; i < leaderBoardRankBars.Length; i++)
        {
            leaderBoardRankBars[i].SetActive(false);
        }
    }

    private void EnableRankBars()
    {
        for (int i = 0; i < leaderBoardRankBars.Length; i++)
        {
            leaderBoardRankBars[i].SetActive(true);
        }
    }

    private void SetupLeaderBoardEntityInformation(LeaderboardPrefab entity, LeaderBoardGroupUserData data, bool isLocal = false)
    {
        Color backGroundColor = rankEntitiesColor[(int)data.LeaderBoardRank];
        entity.SetInformation(data.userName, data.score.ToString(), data.ProfileImageTexture, backGroundColor, isLocal, data.isAnonymous);
    }

    private void EnableTournamentEndWindowIfIsRewardGroup(bool isRewardGroup, GroupInfo groupInfo)
    {
        if (isRewardGroup)
        {
            // Save the reference to currently claimed award
            leaderBoardManager.CurrentRewardedGroupBeingClaimed = groupInfo;

            windowController.OpenTheWindow(ScreenIds.LeaderBoardAwardAvailable);
        }
    }
}