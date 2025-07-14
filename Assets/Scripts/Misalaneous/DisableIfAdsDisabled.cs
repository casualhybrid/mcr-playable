using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableIfAdsDisabled : MonoBehaviour
{
    [SerializeField] private AdsController adsController;

    private void OnEnable()
    {
        if (adsController.AreAdsEnabled)
            return;

        this.gameObject.SetActive(false);
    }
}
