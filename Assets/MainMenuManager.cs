using deVoid.UIFramework;
//using fbg;
using TheKnights.SaveFileSystem;
using Unity.Services.Analytics;
//using UnityEditor.PackageManager;
using UnityEngine;


using UnityEngine.UI;

public class MainMenuManager : AWindowController
{
    [SerializeField] GameObject effect;
    public GameEvent StopCarEngine, StartCarEngine;
    [SerializeField] private AdsController adsController;
    [SerializeField] private GameEvent cutSceneStarted;
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private GameObject carAvailableObject;
    [SerializeField] private GameEvent onCarPurchased;
    [SerializeField] private Image carAvailableImage;
    [SerializeField] private GameEvent OnTapToPlay;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private Canvas TapToPlayFirstTimeCanvas;
    [SerializeField] private GameObject profilePanel;
    [SerializeField] private GameObject leaderBoardPanel;
    private GameObject leaderBoardRefrence;
    GameObject animator;
   /* GameObject cameraRefrence;
    GameObject cameraParent;*/
    GameObject parent;
    //private Coroutine StartCutSceneDelayRoutineRef;
    private bool cutSceneHasStarted = false;

    public static MainMenuManager Instance;

    protected override void Awake()
    {
        Instance = this;
        base.Awake();
    }

    private void Start()
    {
        if(!saveManager.MainSaveFile.TutorialHasCompleted)
        {
            TapToPlayFirstTimeCanvas.transform.SetParent(null, true);
            TapToPlayFirstTimeCanvas.enabled = true;
            GetComponent<CanvasGroup>().alpha = 0;
            GetComponent<CanvasGroup>().interactable = false;
        }
        animator = CameraManager.Instance.player;
        /*cameraRefrence = CameraManager.Instance.camera;
        cameraParent = CameraManager.Instance.cameraParent;*/
        parent = CameraManager.Instance.parent;
        GameObject gb = Instantiate(effect,transform);
    }

    public void OnClickLeaderBoard()
    {

         leaderBoardPanel.SetActive(true);

    }
    public void OnClickClose()
    {
        leaderBoardPanel.SetActive(false);
    }
    private void OnEnable()
    {
        StartCarEngine.RaiseEvent();
        SetCarAvailableSpriteIfAnyCarIsAvailable();
        SubscribeToEvents();
    }
    public void OnClickProfileButton()
    {
        profilePanel.SetActive(true);
    }
    public void OnClickProfileClose()
    {
        profilePanel.SetActive(false);
    }
    private void SubscribeToEvents()
    {
        onCarPurchased.TheEvent.AddListener(SetCarAvailableSpriteIfAnyCarIsAvailable);
    }

    private void DeSubscribeToEvents()
    {
        onCarPurchased.TheEvent.RemoveListener(SetCarAvailableSpriteIfAnyCarIsAvailable);
    }

    public void SendEvents(string eventName)
    {
        AnalyticsManager.CustomData(eventName);
    }

    public void OpenRequiredWindow(string reqWindow)
    {
        this.UI_Close();
        OpenTheWindow(reqWindow);
    }

    private void OnDisable()
    {
        StopCarEngine.RaiseEvent();
        DeSubscribeToEvents();
    }

    public void TempOpenScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(3);
    }

    public void TapToPlayTapped()
    {
        // adsController.HideSmallBanner();

        /*adsController.OnInterstitialAdAboutToShow.AddListener(ContinueWithCutScene);
        adsController.OnInterstitialAdFailedToShow.AddListener(ContinueWithCutScene);
        adsController.OnInterstitialAdCompleted.AddListener(ContinueWithCutScene);*/
        PersistentAudioPlayer.Instance.PlayGameplayAudio();
        PersistentAudioPlayer.Instance.CheckMusicStatus();
        PersistentAudioPlayer.Instance.PlayTumTumSound();
        if (animator)
        {
            animator.GetComponent<Animator>().SetBool("Jump", true);
            /* animator.transform.parent.gameObject.GetComponent<Animator>().enabled = true;*/
        }

      /*  if (cameraParent)
            cameraParent.GetComponent<Animator>().enabled = true;
        if (cameraRefrence)
            cameraRefrence.GetComponent<Animator>().enabled = true;*/
        if (parent)
            parent.GetComponent<Animator>().enabled = true;
        ContinueWithCutScene();
        OnTapToPlay.RaiseEvent();

        //   StartCutSceneDelayRoutineRef = StartCoroutine(StartCutSceneAfterDelay());
        //    OpenTheWindow("AdIsLoadingBeforeCutScene");
    }

    private void ContinueWithCutScene()
    {
        if (cutSceneHasStarted)
            return;

        //if(StartCutSceneDelayRoutineRef != null)
        //{
        //    StopCoroutine(StartCutSceneDelayRoutineRef);
        //}

        /*adsController.OnInterstitialAdAboutToShow.RemoveListener(ContinueWithCutScene);
        adsController.OnInterstitialAdFailedToShow.RemoveListener(ContinueWithCutScene);
        adsController.OnInterstitialAdCompleted.RemoveListener(ContinueWithCutScene);*/

        cutSceneHasStarted = true;

        AnalyticsManager.CustomData("CutSceneStarted");
        cutSceneStarted.RaiseEvent();
    }

    // For safety, in case we don't recieve callbacks
    //private IEnumerator StartCutSceneAfterDelay()
    //{
    //    yield return new WaitForSecondsRealtime(4);
    //    ContinueWithCutScene();

    //}

    private void SetCarAvailableSpriteIfAnyCarIsAvailable(GameEvent gameEvent = null)
    {
        var data = shopManager.CheckIfCarIsAvailableWithPlayerCoinsBeingMoreThanRequired(InventoryToItemPriceRelation.Twice);

        if (!data.isValid)
        {
            carAvailableObject.SetActive(false);
            return;
        }

        var carConfigurationData = data.basicAssetInfo as CarConfigurationData;
        carAvailableImage.sprite = carConfigurationData.GetCarSprite;
        carAvailableObject.SetActive(true);
    }
}