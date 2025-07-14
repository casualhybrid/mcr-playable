using deVoid.UIFramework;
using System;
using TheKnights.LeaderBoardSystem;
using TheKnights.SaveFileSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderBoardRewardAvailableUI : AWindowController
{
    [SerializeField] private CharactersDataBase charactersDataBase;
    [SerializeField] private PlayerCharacterLoadingHandler loadingHandler;
    [SerializeField] private SaveManager saveManager;

    [SerializeField] private TheKnights.LeaderBoardSystem.LeaderBoardManager leaderBoardManager;
    [SerializeField] private LeaderBoardRankDataSet leaderBoardRankDataSet;

    [SerializeField] private GameObject tournamentEndedPanel;
    [SerializeField] private GameObject tournamentDetailsPanel;

    [SerializeField] private TextMeshProUGUI weekNumberText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI countryText;
    [SerializeField] private Image countryImage;
    [SerializeField] private Image rankIconImage;

    public GameObject parentObjForCharacter;
    private GameObject displayGameObject;
    private GameObject instantiatedDisplayObject;

    private void OnEnable()
    {
        // Reset
        tournamentDetailsPanel.SetActive(false);
        tournamentEndedPanel.SetActive(true);

        if (instantiatedDisplayObject != null)
        {
            Destroy(instantiatedDisplayObject);
        }

        var configData = charactersDataBase.GetCharacterConfigurationData(saveManager.MainSaveFile.currentlySelectedCharacter);
        var loader = loadingHandler.LoadAssets(configData);
        loader.AssetsLoaded += (asset) =>
        {
            displayGameObject = asset.DisplayGameObject;

            InstantiateTheCharacter();
        };
    }

    private void InstantiateTheCharacter()
    {
        instantiatedDisplayObject = Instantiate(displayGameObject, parentObjForCharacter.transform, false);
    }

    public void EnableGroupDetailsPanel()
    {
        tournamentDetailsPanel.SetActive(true);
        tournamentEndedPanel.SetActive(false);

        try
        {
         //   LeaderBoardGroupQueryResponseData leaderBoardData = leaderBoardManager.CurrentGroupInformation;
            GroupInfo pendingRewardGroupInfo = /*leaderBoardData.pendingRewardInfo*/ leaderBoardManager.CurrentRewardedGroupBeingClaimed;
            LeaderBoardGroupUserData userData = pendingRewardGroupInfo.localPlayerData;

            weekNumberText.text = "Week " + pendingRewardGroupInfo.weekNumber;
            countryText.text = pendingRewardGroupInfo.userCountry;
            countryImage.sprite = pendingRewardGroupInfo.countrySprite;
            scoreText.text = userData.score.ToString();
            rankText.text = userData.LeaderBoardRank.ToString();
            rankIconImage.sprite = leaderBoardRankDataSet.GetLeaderBoardRankSprite(userData.LeaderBoardRank);
        }
        catch (Exception e)
        {
            UnityEngine.Console.LogError($"An exception occurred while trying to display leaderboard award obtained." +
                $"This should usually never happen!. Reason: {e}");

            // Close the screen here
            UI_Close();
        }
    }
}