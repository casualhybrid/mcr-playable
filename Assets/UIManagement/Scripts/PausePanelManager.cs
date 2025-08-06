using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using deVoid.UIFramework;
using Knights.UISystem;


public class PausePanelManager : AWindowController
{
    [SerializeField] InputChannel InputChannel;
    [SerializeField] GameObject mapProgression, generalProgression;
    [SerializeField] ProgressPanel generalProgresScript;
   // [SerializeField] private LeaderBoardDataSO leaderboardData;
    [SerializeField] private MapProgressionSO mapData;

    private bool showMapOneTime = false;

    private void OnEnable()
    {
       // Time.timeScale = 0;
      //  InputChannel.PauseInputsFromUser();

        #region progression
        
        mapProgression.SetActive(!/*mapData.AllEnvironmentsCompleted*/showMapOneTime);
        generalProgression.SetActive(/*mapData.AllEnvironmentsCompleted*/showMapOneTime);
        //if (leaderboardData.onLeaderboardTop)
        //{
        //    generalProgresScript.changingPanelType(1);
        //}

        showMapOneTime = mapData.AllEnvironmentsCompleted; //this must be done in saveFile
        #endregion
    }

    public void ClosePausePanelAndOpenGetReadyPanel()
    {
        UI_Close();
        OpenTheWindow(ScreenIds.GetReadyPanel);
    }

    public void SendEvents(string eventName)
    {
        AnalyticsManager.CustomData(eventName);
    }

    //public void Resume()
    //{
    //    Time.timeScale = 1;
    //    InputChannel.UnPauseInputsFromUser();
    //    resumeBtnEvent.RaiseEvent();
    //}

    protected override void Awake()
    {
        base.Awake();
    }
}
