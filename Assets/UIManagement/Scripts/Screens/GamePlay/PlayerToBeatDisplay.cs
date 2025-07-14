using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerToBeatDisplay : MonoBehaviour
{
    public event Action OnPlayerBeaten;

    [SerializeField] private RawImage playerImage;
    [SerializeField] private TextMeshProUGUI scoreTxt;
    [SerializeField] private TextMeshProUGUI nameTxt;
    [SerializeField] private GamePlaySessionData gamePlaySessionData;

    private int targetScore;
    private const float updateRatePerSecond = 4;

    private float elapsedTimeTillUpdate;
    private bool isBeaten;

    public void Initialize(Texture2D playerProfileSprite, string playerName, int highScore)
    {
        isBeaten = false;
        elapsedTimeTillUpdate = updateRatePerSecond;
        playerImage.texture = playerProfileSprite;
        targetScore = highScore;
        scoreTxt.text = highScore.ToString();
        nameTxt.text = playerName;
    }

    private void Update()
    {
        if (isBeaten)
            return;

        elapsedTimeTillUpdate += Time.deltaTime;

        if (elapsedTimeTillUpdate < updateRatePerSecond)
            return;

        int score = targetScore - (int)gamePlaySessionData.DistanceCoveredInMeters;

        if (score <= 0)
        {
            score = 0;
            OnPlayerBeaten?.Invoke();
            isBeaten = true;
        }

        scoreTxt.text = score.ToString();
    }
}