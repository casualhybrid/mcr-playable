using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using deVoid.UIFramework;
using TMPro;

public class DiamondStashInstancePopUpScript : APanelController
{
    [SerializeField] TextMeshProUGUI diamondsRecieved;

    private void Update()
    {
        int i = (int)GameManager.gameplaySessionTimeInSeconds;

        if(i <= 60)
        {
            diamondsRecieved.text = 3.ToString();
        }
        else if (i == 60*4)
        {
            diamondsRecieved.text = 5.ToString();
        }
        else if (i == 60*7)
        {
            diamondsRecieved.text = 7.ToString();
        }
        else if (i == 60*10)
        {
            diamondsRecieved.text = 10.ToString();
        }
        else if(i >= 60*15)
        {
            diamondsRecieved.text = 15.ToString();
        }
    }
}
