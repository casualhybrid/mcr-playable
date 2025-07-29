using Sirenix.OdinInspector;
using TheKnights.SaveFileSystem;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class PanelTransformModifierBasedOnBannerAds : MonoBehaviour
{
    [SerializeField] private bool isPanelNameOverride = false;
    [ShowIf("isPanelNameOverride")][SerializeField] private string panelName;
    [SerializeField] private bool waitForActivationOnce = false;
    [SerializeField] private bool initializeModeIsStartFirstTime = false;
    [SerializeField] private GameEvent playerBoughtADFree;

    [SerializeField] private AdsController adsController;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private GameObject adContainerGameObject;
    [SerializeField] private RectTransform mainPanel;

    private RectTransform rectTransform;
    private Canvas parentCanvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    private void OnEnable()
    {
        ScreenSafeAreaMonitor.OnSafeAreaChanged += LowerTheMainPanelTakingSafeAreaInAccount;
        playerBoughtADFree.TheEvent.AddListener(DisableTouchBlockerAndRepositionMainPanelIfNoAds);

        LowerTheMainPanelTakingSafeAreaInAccount();
        DisableTouchBlockerAndRepositionMainPanelIfNoAds();
    }

    private void OnDisable()
    {
        playerBoughtADFree.TheEvent.RemoveListener(DisableTouchBlockerAndRepositionMainPanelIfNoAds);
        ScreenSafeAreaMonitor.OnSafeAreaChanged -= LowerTheMainPanelTakingSafeAreaInAccount;
    }

    private void LowerTheMainPanelTakingSafeAreaInAccount()
    {
        Rect safeAreaRect = Screen.safeArea;

        int totalHeight = Display.main.renderingHeight;
        int safeAreaHeight = (int)safeAreaRect.height;

        //UnityEngine.Console.Log($"totalheight {totalHeight} and safeareaheight {safeAreaHeight}");

        if (totalHeight == safeAreaHeight)
            return;

        ///rectTransform.SetTop((totalHeight - (safeAreaRect.position.y + safeAreaRect.height)) / parentCanvas.scaleFactor);
        rectTransform.SetBottom(safeAreaRect.position.y / parentCanvas.scaleFactor);
    }

    private void DisableTouchBlockerAndRepositionMainPanelIfNoAds(GameEvent gameEvent = null)
    {
        if (saveManager == null)
            return;

        if (saveManager.MainSaveFile == null)
            return;

        bool areAdsActive = !saveManager.MainSaveFile.isAdsPurchased && adsController.AreAdsEnabled;

        if (!areAdsActive)
        {
            if (adContainerGameObject != null)
            {
                adContainerGameObject.SetActive(false);
            }

            if (mainPanel != null)
            {
                mainPanel.SetBottom(0);
            }
        }
    }
}