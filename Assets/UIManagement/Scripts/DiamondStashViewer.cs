using System;
using System.Collections;
using TheKnights.SaveFileSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiamondStashViewer : MonoBehaviour
{
    [SerializeField] private GamePlayBackGroundRewardsData gamePlayBackGroundRewardsData;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private UiInfoUpdaterSO timerInfoUpdater;
    [SerializeField] private TextMeshProUGUI daimondStashTxt;
    [SerializeField] private TextMeshProUGUI totalDiamondStashTxt;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private Sprite diamondBGSprite, highestScoreBGSprite;
    [SerializeField] private Image bgImg;
    [SerializeField] private RawImage texture;
    [SerializeField] private GameEvent openDiamondStashEvent;
    [SerializeField] private GameEvent allGamePlayRewardsEarned;
    [SerializeField] private GameObject switchGlowObj, icon1, icon2, daimondStash, totalDiamondStash, highscore, texImg, bgSpriteImg, stashText;
    [SerializeField] private GamePlaySessionData gamePlaySessionData;

    private bool isRewardsEnded = false;

    private void OnEnable()
    {
        openDiamondStashEvent.TheEvent.AddListener(StashesRewardsEnded);
        allGamePlayRewardsEarned.TheEvent.AddListener(MarkGamePlayRewardsAsCompleted);
        isRewardsEnded = gamePlayBackGroundRewardsData.isPlayerObtainedAllBackGroundRewards();
    }

    private void Update()
    {
        if (isRewardsEnded)
        {
            //stashText.SetActive(false);
            //bgSpriteImg.SetActive(false);
            //icon1.SetActive(false);
            //icon2.SetActive(false);
            //daimondStash.SetActive(false);
            //totalDiamondStash.SetActive(false);
            //highscore.SetActive(true);
            //texImg.SetActive(true);

            this.gameObject.SetActive(false);
        }
        else
        {
            bgImg.sprite = diamondBGSprite;
            daimondStashTxt.text = timerInfoUpdater.stashesTimer;
            totalDiamondStashTxt.text = timerInfoUpdater.totalStashTimer;
        }
    }

    public void StashesRewardsEnded(GameEvent theEvent)
    {
        GlowEffect();
    }

    private void MarkGamePlayRewardsAsCompleted(GameEvent theEvent)
    {
        isRewardsEnded = true;
        GlowEffect();
    }

    private void GlowEffect()
    {
        switchGlowObj.SetActive(true);
        StartCoroutine(WaitForSomeTime(() => { switchGlowObj.SetActive(false); }, 3));
    }

    private IEnumerator WaitForSomeTime(Action action, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        action?.Invoke();
    }

    private void OnDisable()
    {
        openDiamondStashEvent.TheEvent.RemoveListener(StashesRewardsEnded);
        allGamePlayRewardsEarned.TheEvent.RemoveListener(MarkGamePlayRewardsAsCompleted);

        switchGlowObj.SetActive(false);
    }
}