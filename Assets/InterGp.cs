using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterGp : MonoBehaviour
{
    public void OnEnable()
    {
        MaxAdMobController.Instance.ShowInterstitialAd();
    }
}
