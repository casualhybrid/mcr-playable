using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BannerHandle : MonoBehaviour
{
    [SerializeField] private AdsController adsController;
    private void OnEnable()
    {
        adsController.HideSmallBanner();
    }
}
