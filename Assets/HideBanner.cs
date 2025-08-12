using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideBanner : MonoBehaviour
{
    public void OnEnable()
    {
        MaxAdMobController.Instance.HideAdmobBanner();
    }
    public void OnDisable()
    {
        MaxAdMobController.Instance.ShowAdmobBanner();
    }
}
