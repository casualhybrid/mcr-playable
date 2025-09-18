using deVoid.UIFramework;
using Knights.UISystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine;


[RequireComponent(typeof(CanvasGroup))]
public class DailyRewardScript : MonoBehaviour
{
    [SerializeField] private DOTweenAnimation rewardBoxAvailableAnim;
    [SerializeField] private AWindowController windowControllerObj;
    [SerializeField] private InventorySystem inventoryObj;

    [SerializeField] private GameObject notificationObj;
    [SerializeField] private TextMeshProUGUI noOfBoxTxt;
    [SerializeField] private GameEvent updateUIEvent;
    //[SerializeField] private GameObject icon;

    private float timeUntilNextMysteryBox = 86400.0f;
    private bool timeCheck = false;

    private void Start()
    {
        if (!PlayerPrefs.HasKey("SetTimer"))
        {
            PlayerPrefs.SetInt("SetTimer", 1);//First Time Login
            PlayerPrefs.SetString("InitialTimer", DateTime.Now.Ticks.ToString());
            PlayerPrefs.SetInt("AwardBox", 0);
        }


        ulong prevLoginTime = ulong.Parse(PlayerPrefs.GetString("InitialTimer")) / TimeSpan.TicksPerMillisecond;
        ulong currentLoginTIme = (ulong)DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        float secondsLeft = (float)(currentLoginTIme - prevLoginTime) / (float)1000;
        //currnetTimer = (ulong)DateTime.Now.Ticks;

        if (secondsLeft > timeUntilNextMysteryBox)
        {
            PlayerPrefs.SetInt("AwardBox", PlayerPrefs.GetInt("AwardBox") + 1);
            timeCheck = true;
        }

        updateBoxUI();
    }
    private void OnEnable()
    {
        //updateUIEvent.TheEvent.AddListener(UpdateUIFunc);
        UpdateUIFunc(null);
    }
    //private void Update()
    //{
    //    string r = "";
    //    //Hours
    //    r += ((int)secondsLeft / 3600).ToString() + "h ";
    //    secondsLeft -= ((int)secondsLeft / 3600) * 3600;
    //    //Minutes
    //    r += ((int)secondsLeft / 60).ToString("00") + "m ";
    //    //Seconds
    //    r += (secondsLeft % 60).ToString("00") + "s ";

    //    timeRemaining.text = r;
    //}
    void updateBoxUI()
    {
        if (PlayerPrefs.GetInt("AwardBox") > 0)
        {
            noOfBoxTxt.text = PlayerPrefs.GetInt("AwardBox").ToString("00");
            gameObject.GetComponent<CanvasGroup>().alpha = 1.0f;
            gameObject.SetActive(true);
            notificationObj.SetActive(true);

            rewardBoxAvailableAnim.transform.localRotation = Quaternion.Euler(0, 0, -10);
            rewardBoxAvailableAnim.CreateTween(false, false);
            rewardBoxAvailableAnim.DORestart();
        }
        else
        {
            gameObject.GetComponent<CanvasGroup>().alpha = 0.6f;
            noOfBoxTxt.text = PlayerPrefs.GetInt("AwardBox").ToString("00");
            ResetMysteryBoxTransformAndStopAnimation();
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && timeCheck)
        {
            PlayerPrefs.SetString("InitialTimer", DateTime.Now.Ticks.ToString());
            timeCheck = false;
        }
    }


    //private void OnApplicationQuit()
    //{
    //    if (timeCheck)
    //    {
    //        PlayerPrefs.SetString("InitialTimer", DateTime.Now.Ticks.ToString());
    //        timeCheck = false;
    //    }
    //}


    public void OpenMysteryBoxPanel()
    {

        if (PlayerPrefs.GetInt("AwardBox") > 0)
        {
            PushNotificationsManager.RegisterDailyRewardNotification();
            ResetMysteryBoxTransformAndStopAnimation();
            windowControllerObj.OpenTheWindow(ScreenIds.MysteryBoxPanelDailyReward);
        }
        //else
        //{
        //    gameObject.SetActive(false);
        //}
    }

    private void ResetMysteryBoxTransformAndStopAnimation()
    {
        rewardBoxAvailableAnim.DOPause();
        rewardBoxAvailableAnim.transform.rotation = Quaternion.identity;
    }

    private void UpdateUIFunc(GameEvent theEvent)
    {
        //UnityEngine.Console.LogError("UI Event Listened");
        if (noOfBoxTxt != null)
        {
            noOfBoxTxt.text = PlayerPrefs.GetInt("AwardBox").ToString("00");
        }

        if (PlayerPrefs.GetInt("AwardBox") > 0)
        {
            gameObject.GetComponent<CanvasGroup>().alpha = 1.0f;
            rewardBoxAvailableAnim.DORestart();
            if (noOfBoxTxt != null)
            {
                noOfBoxTxt.text = PlayerPrefs.GetInt("AwardBox").ToString("00");
            }

            gameObject.SetActive(true);

            if (notificationObj != null)
            {
                notificationObj.SetActive(true);
            }
        }
        else
        {
            // gameObject.SetActive(false);
            gameObject.GetComponent<CanvasGroup>().alpha = 0.6f;
            ResetMysteryBoxTransformAndStopAnimation();
            if (notificationObj != null)
            {
                notificationObj.SetActive(false);
            }
        }
    }

    private void OnDisable()
    {
        //updateUIEvent.TheEvent.RemoveListener(UpdateUIFunc);
    }
}
