using deVoid.UIFramework;
using UnityEngine;

public class InterstitialADIsLoading : AWindowController
{
    [SerializeField] private AdsController adsController;

    protected bool isScreenDisableCalled;

    private void OnEnable()
    {
        isScreenDisableCalled = false;
        DisableScreen();
        adsController.OnInterstitialAdCompleted.AddListener(DisableScreen);
        adsController.OnInterstitialAdFailedToShow.AddListener(DisableScreen);
        adsController.OnInterstitialAdAboutToShow.AddListener(DisableScreen);
    }

    private void OnDisable()
    {
        adsController.OnInterstitialAdCompleted.RemoveListener(DisableScreen);
        adsController.OnInterstitialAdFailedToShow.RemoveListener(DisableScreen);
        adsController.OnInterstitialAdAboutToShow.RemoveListener(DisableScreen);
    }

    private void DisableScreen()
    {
       /* if (!this.gameObject.activeInHierarchy)
            return;

        if (isScreenDisableCalled)
            return;*/

        isScreenDisableCalled = true;

        UI_Close();
    }
}