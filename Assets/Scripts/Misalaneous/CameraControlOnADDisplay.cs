using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraControlOnADDisplay : MonoBehaviour
{
    [SerializeField] private AdsController adsController;
    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        adsController.OnRewardedAdFailedToShow.AddListener(TurnCamerOn);
        adsController.OnRewardedAdClosed.AddListener(TurnCamerOn);
    }

    private void OnDestroy()
    {
        adsController.OnRewardedAdFailedToShow.RemoveListener(TurnCamerOn);
        adsController.OnRewardedAdClosed.RemoveListener(TurnCamerOn);

    }

    private void TurnOffCamera()
    {
        cam.enabled = false;
    }
    
    private void TurnCamerOn()
    {
        cam.enabled = true;
    }

}
