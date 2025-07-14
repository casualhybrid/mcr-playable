using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using deVoid.UIFramework;
using Knights.UISystem;

public class HUDManager : AWindowController
{
    [SerializeField] private GameEvent pauseButtonClicked;
    

    protected override void Awake()
    {
        base.Awake();
        
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        pauseButtonClicked.TheEvent.AddListener(OnPauseButtonClicked);
    }


    private void OnDisable()
    {
        pauseButtonClicked.TheEvent.RemoveListener(OnPauseButtonClicked);
    }

    private void OnPauseButtonClicked(GameEvent theEvent)
    {
        //this.OpenTheWindow(ScreenIds.PausePanel);
        if (Debug.isDebugBuild)
        {
            UnityEngine.Console.Log("PauseButtonClicked");
        }
        
    }
}
