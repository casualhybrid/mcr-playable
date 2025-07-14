using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using deVoid.UIFramework;
using TMPro;

[System.Serializable]
public class TutorialTextHintProperties : PanelProperties
{
    public TutorialGesture CurrentTutorialGesture { get; set; }
}

public class TutorialActionText :/*: APanelController<TutorialTextHintProperties>*/ MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tutorialText;

    public void OnPropertiesSet(TutorialGesture currTutGesture)
    {
      //  base.OnPropertiesSet();

        string text = null;

        //TutorialGesture currTutGesture = Properties.CurrentTutorialGesture;

        switch (currTutGesture)
        {
            case TutorialGesture.Tap:

                text = "TAP TO DESTROY YOUR ENEMIES!";
                break;
            case TutorialGesture.SwipeRight:

                text = "SWIPE RIGHT";
                break;
            case TutorialGesture.SwipeLeft:

                text = "SWIPE LEFT";
                break;
            case TutorialGesture.DoubleTap:

                text = "DOUBLE TAP FOR A BOOST!";
                break;
            case TutorialGesture.SwipeDown:

                text = "SWIPE DOWN";
                break;
            case TutorialGesture.SwipeUp:

                text = "SWIPE UP";
                break;
            case TutorialGesture.SwipeUpDown:

                text = "SWIPE DOWN";
                break;

            default:
                throw new System.Exception("Invalid tutorial gesture for text display");

        }

        tutorialText.text = text;


    }

}
