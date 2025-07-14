 using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;
using DG.Tweening;

public class ProgressPanel : MonoBehaviour
{   enum panels
    {
        diamond,
        leadorboard,
        highscore,
        playerlevel
    }

    [SerializeField] panels panelType;
    [SerializeField] Image fillImage, currentImage, nextImage, currentIconImage, nextIconImage;
    [SerializeField] Sprite active, inActive, diamond;
    [SerializeField] TextMeshProUGUI currentText, NextText, boxTxt1, boxTxt2;
    [SerializeField] GameObject normalImg1, normalImg2, text1, text2;
    [SerializeField] GamePlaySessionData gamePlaySessionData;
    [SerializeField] TheKnights.SaveFileSystem.SaveManager saveManager;
    [SerializeField] private GamePlayBackGroundRewardsData gamePlayBackGroundRewardsData;
    [SerializeField] private PlayerLevelingSystem playerLevelSystem;
  //  [SerializeField] private LeaderBoardDataSO leaderboardData;
    [SerializeField] private float tweenFillImageSpeed = .5f;

    private int stashCount;
    private Tween fillTween;

    void OnEnable()
    {

        if (panelType == panels.diamond)
            diamondStash();
        else if (panelType == panels.highscore)
            highScore();
        //else if (panelType == panels.leadorboard)
        //    Leaderboard();
        else if (panelType == panels.playerlevel)
            playerLevel();

        progression();

    }

    private void OnDisable()
    {
        fillTween?.Kill();
    }


    private Queue<float> timeQueue = new Queue<float>();

    private float gamePlayTime = 0f, timeToCheck = 0f;

    private void Awake()
    {
        HashSet<float> userRewardAcquired = saveManager.MainSaveFile.gamePlayBackGroundAwardsAcquired;

        foreach (var item in gamePlayBackGroundRewardsData.GetDictionaryKeys().OrderBy(x => x))
        {
            if (!userRewardAcquired.Contains(item))
            {
                timeQueue.Enqueue(item);
            }
        }
        stashCount = 1;
        currentImage.sprite = active;
        nextImage.sprite = inActive;

        if (timeQueue.Count != 0)
            timeToCheck = timeQueue.Peek();
    }


    //private void Update()
    //{
    //    progression();
    //}

    private void PlayFillTheBarTween(Image imageToFill, float fillAmount)
    {
        fillTween?.Kill();

        imageToFill.fillAmount = 0;
        fillTween = imageToFill.DOFillAmount(fillAmount, tweenFillImageSpeed).SetEase(Ease.Linear).SetSpeedBased(true).SetUpdate(true);
    }


    void progression()
    {
        switch (panelType)
        {
            case panels.diamond:
                if (timeQueue.Count == 0)
                {
                    PlayFillTheBarTween(fillImage, 1);
                    stashCount = 4;
                    currentImage.sprite = inActive;
                    nextImage.sprite = active;
                    return;
                }
                gamePlayTime = GameManager.gameplaySessionTimeInMinutes;
                if (gamePlayTime >= timeToCheck)
                {
                    timeQueue.Dequeue();
                    if (timeQueue.Count != 0)
                    {
                        timeToCheck = timeQueue.Peek();
                        stashCount += 1;
                        diamondStash();
                    }
                }

                PlayFillTheBarTween(fillImage, gamePlayTime / timeToCheck);


                break;
            //case panels.leadorboard:
            //    if ((int)saveManager.MainSaveFile.currentLeaderBoardRank < 4)
            //    {
            //        PlayFillTheBarTween(fillImage, gamePlaySessionData.DistanceCoveredInMeters / leaderboardData.ScoreToCrossLeaderboard);

            //        if (gamePlaySessionData.DistanceCoveredInMeters > leaderboardData.ScoreToCrossLeaderboard)
            //        {
            //            currentImage.sprite = inActive;
            //            nextImage.sprite = active;
            //        }
            //    }
            //    else
            //    {
            //        PlayFillTheBarTween(fillImage, Mathf.Clamp01(25 / leaderboardData.LeaderBoardPosition + 1));

            //        if (leaderboardData.LeaderBoardPosition < 25)
            //        {
            //            currentImage.sprite = inActive;
            //            nextImage.sprite = active;
            //        }
            //    }
            //    break;
            case panels.playerlevel:
                PlayFillTheBarTween(fillImage, Mathf.Clamp01(playerLevelSystem.GetPlayerCurrentXP() / playerLevelSystem.GetXPNeededForNextLevel()));

                break;
            case panels.highscore:
                PlayFillTheBarTween(fillImage, gamePlaySessionData.DistanceCoveredInMeters / saveManager.MainSaveFile.playerHighScore);

                if(gamePlaySessionData.DistanceCoveredInMeters > saveManager.MainSaveFile.playerHighScore)
                {
                    currentImage.sprite = inActive;
                    nextImage.sprite = active;

                }
                break;
            default:
                break;
        }
    }


    void diamondStash()
    {
        generalChanges(true);
        currentText.text = "STASH " + stashCount.ToString();
        NextText.text = "STASH " + (stashCount + 1).ToString();
        currentIconImage.sprite = diamond;
        nextIconImage.sprite = diamond;
    }

    void highScore()
    {
        generalChanges(false);
        currentText.text = "Current Score";
        NextText.text = "High Score";
        boxTxt1.text = gamePlaySessionData.DistanceCoveredInMeters.ToString("0");
        boxTxt2.text = saveManager.MainSaveFile.playerHighScore.ToString();
    }
    void playerLevel()
    {
        generalChanges(false);
        //currentText.text = "Level " + playerLevelSystem.GetCurrentPlayerLevel().ToString();
        //NextText.text = "Level     " + (playerLevelSystem.GetCurrentPlayerLevel() + 1).ToString();
        //currentText.text = playerLevelSystem.GetPlayerCurrentXP().ToString("0");
        //NextText.text = playerLevelSystem.GetXPNeededForNextLevel().ToString("0");
        currentText.text = "Player's Level";
        NextText.text = "Next Level";
        boxTxt1.text = playerLevelSystem.GetCurrentPlayerLevel().ToString("0");
        boxTxt2.text = (playerLevelSystem.GetCurrentPlayerLevel() + 1).ToString("0");
    }
    //void Leaderboard()
    //{

    //    generalChanges(true);
    //    leaderboardData.TurnOnLeaderboard();
    //    currentText.text = gamePlaySessionData.DistanceCoveredInMeters.ToString("0");
    //    if ((int)saveManager.MainSaveFile.currentLeaderBoardRank == 1)
    //        NextText.text = LeaderboardRewardManager.Instance.ironTargetScore.ToString();
    //    else if ((int)saveManager.MainSaveFile.currentLeaderBoardRank == 2)
    //        NextText.text = LeaderboardRewardManager.Instance.bronzeTargetScore.ToString();
    //    else if ((int)saveManager.MainSaveFile.currentLeaderBoardRank == 3)
    //        NextText.text = LeaderboardRewardManager.Instance.silverTargetScore.ToString();
    //    else
    //    {
    //        if (NetworkManager.Instance.leaderboardTimeLeft != null)
    //        {
    //            TimeSpan ts = NetworkManager.Instance.leaderboardTimeLeft.Subtract(DateTime.Now);
    //            // timeLeftLbl.text = ts.Hours.ToString("D2") + ":" + ts.Minutes.ToString("D2") + ":" + ts.Seconds.ToString("D2");

    //            // Less Garbage
    //            NextText.text = ts.ToString("hh\\:mm\\:ss");
    //        }
    //    }


    //    currentIconImage.sprite = leaderboardData.CurrentRankSprite;
    //    nextIconImage.sprite = leaderboardData.NextRankSprite;
    //}

    void generalChanges(bool status)
    {
        normalImg1.SetActive(status);
        normalImg2.SetActive(status);
        text1.SetActive(!status);
        text2.SetActive(!status);
    }
    public void changingPanelType(int i)
    {
        if (i == 0)
        {
            panelType = panels.diamond;
            diamondStash();
            progression();
        }
        else if (i == 1)
        {
            panelType = panels.highscore;
            highScore();
            progression();

        }
        //else if (i == 2)
        //{
        //    panelType = panels.leadorboard;
        //    Leaderboard();
        //    progression();

        //}
        else if (i == 3)
        {
            panelType = panels.playerlevel;
            playerLevel();
            progression();

        }
    }




}
