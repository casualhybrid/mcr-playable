using deVoid.UIFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceUpdateAppUI : GeneralWindowScreen
{
 
    public void OpenPlayStoreLink()
    {
        Application.OpenURL("https://play.google.com/store/apps/details?id=com.tb.minicar.rush.racing.drivinggames");
        UI_Close();
    }
    
}
