using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerToBeatProgressPanel : MonoBehaviour
{
    [SerializeField] private RawImage firstPlayerImage;
    [SerializeField] private RawImage secondPlayerImage;
    [SerializeField] private TextMeshProUGUI firstPlayerNameTxt;
    [SerializeField] private TextMeshProUGUI secondPlayerNameTxt;

    [SerializeField] private TextMeshProUGUI detailsText;
    [SerializeField] private GamePlaySessionData gamePlaySessionData;

    private void OnEnable()
    {
        PlayerToBeatBattle battle = NextPlayerToBeatUI.PlayerToBeatBattle;
        firstPlayerImage.texture = battle.FirstPlayer.ProfilePicTex;
        secondPlayerImage.texture = battle.SecondPlayer.ProfilePicTex;

        firstPlayerNameTxt.text = battle.FirstPlayer.isAnonymous ? "You" : battle.FirstPlayer.PlayerName + " (Me)";
        secondPlayerNameTxt.text = battle.SecondPlayer.PlayerName;

        int score = battle.SecondPlayer.Score - (int)gamePlaySessionData.DistanceCoveredInMeters;

        if (score < 0)
            score = 0;

        detailsText.text = $"Only {score} more points are needed to beat {battle.SecondPlayer.PlayerName}";
    }
}