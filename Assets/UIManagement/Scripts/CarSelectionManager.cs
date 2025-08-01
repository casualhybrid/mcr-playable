using deVoid.UIFramework;
using DG.Tweening;
using Knights.UISystem;
using RotaryHeart.Lib.SerializableDictionary;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TheKnights.SaveFileSystem;
using TMPro;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class CarSelectionManager : AWindowController<CarAvailableProperties>
{
    [System.Serializable]
    private class CarToPriceDictionary : SerializableDictionaryBase<int, int>
    { }

    [SerializeField] private UnityEvent onChangeVisibleCar;
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private PlayerLevelingSystem playerLevelSystem;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private PlayerCarLoadingHandler loadingHandler;
    [SerializeField] private CarsDataBase carsDataBase;
    [SerializeField] private InventorySystem inventoryObj;
    [SerializeField] private InputChannel input;
    [SerializeField] private ShopPurchaseEvent purchaseEvent;
    [SerializeField] private Camera camera;
    [SerializeField] private GameEvent onCarPurchased;
    [SerializeField] private GameEvent onAllCarsPurchased;

    [Space]
    [Space]
    [BoxGroup("SelectionBox", true, true)] [SerializeField] private GameEvent carHasBeenSelected;

    [BoxGroup("SelectionBox", true, true)] [SerializeField] private GameEvent newCarIsGoingToSpawn;
    [BoxGroup("SelectionBox", true, true)] [SerializeField] private GameObject parentObjForCars, contentPanel;
    [BoxGroup("SelectionBox", true, true)] [SerializeField] private CarInstance[] CarList;

    [Space]
    [Space]
    [BoxGroup("UIBox", true, true)] [SerializeField] private GameObject rightArrow;

    [BoxGroup("UIBox", true, true)] [SerializeField] private GameObject leftArrow;
    [BoxGroup("UIBox", true, true)] [SerializeField] private GameObject lockIcon;
    [BoxGroup("UIBox", true, true)] [SerializeField] private Button backButton;
    [BoxGroup("UIBox", true, true)] [SerializeField] private Image thrustFill1, thrustFill2, airplaneFill1, airplaneFill2, boostFill1, boostFill2;
    [BoxGroup("UIBox", true, true)] [SerializeField] private GameObject /*SelectedArrowImage,*/ SelectButton, CoinsButton, WatchAdbutton, tooltipButton;
    [BoxGroup("UIBox", true, true)] [SerializeField] private TextMeshProUGUI carName, unlocksAtLvlTxt;
    [BoxGroup("UIBox", true, true)] [SerializeField] private GameObject levelRequiredLockObject;
    [BoxGroup("UIBox", true, true)] [SerializeField] private string[] toolTipTexts;
    [BoxGroup("UIBox", true, true)] [SerializeField] private TextMeshProUGUI toolTipInfoText, CostTextToShow, selectedText;
    [BoxGroup("UIBox", true, true)] [SerializeField] private ScrollRect scrollRect;
    [BoxGroup("UIBox", true, true)] [SerializeField] private GameObject loadingIconGameObject;

    private AsyncOperationSpawning<PlayerCarAssets> currentlyActiveCarAssetLoadingHandler;
    private int selectedCarIndex;
    private Coroutine carGeneration;
    private GameObject displayGameObject;
    [SerializeField] private Animator animatorToDisable;
    private GameObject currentDisplayingCar, previousDisplayingCar;
    private float time;
    private Camera uiCamera;

    private PlayableObjectDataWithIndex carToSnapToData;
    private Tween selectButtonPunchTween;

    private int timesEnabled;
    [SerializeField] GameObject profile;
    [SerializeField] Sprite[] icons;
    [SerializeField] Image Icon;
    //[SerializeField] AdsController adsController;

    protected override void Awake()
    {
        base.Awake();
        uiCamera = GetComponentInParent<UIFrame>().UICamera;
    }

    private void OnEnable()
    {
        if (profile)
            profile.SetActive(false);

        PersistentAudioPlayer.Instance.PanelSounds();
        timesEnabled++;

        selectedCarIndex = -1;
        time = GetCurrentAnimatorTime(animatorToDisable);
        StartCoroutine(turnOfAnimator());
        InitilizeUI(saveManager.MainSaveFile.currentlySelectedCar);
        int carToShow = carToSnapToData.isValid ? carToSnapToData.index : saveManager.MainSaveFile.currentlySelectedCar;
        ShowCar(carToShow);
       
      
        SubsribeInputEvents();
        SubscribeToEvents();

        SetCarAvailableSpriteIfAnyCarIsAvailable();
       // adsController.ShowSmallBannerAd("CarSelectionPanel");

       // OpenUnlockAllCarsPopupIfPossible();
    }

    protected override void OnPropertiesSet()
    {
        base.OnPropertiesSet();

        carToSnapToData = Properties.CarConfigData;
    }
    public void CloseSound()
    {
        PersistentAudioPlayer.Instance.PlayAudio();
    }
    private void OnDisable()
    {
        Properties.CarConfigData.isValid = false;
        carToSnapToData.isValid = false;

        scrollRect.enabled = false;
        camera.enabled = false;

        if (previousDisplayingCar != null)
        {
            Destroy(previousDisplayingCar);
        }

        UnSubsribeInputEvents();
        DeSubscribeToEvents();
        animatorToDisable.enabled = true;

        KillSelectButtonAnimAndRewind();
    }

    public override void ForeGroundLost()
    {
        base.ForeGroundLost();
        UnSubsribeInputEvents();
    }

    public override void ForeGroundReObtained()
    {
        base.ForeGroundReObtained();
        SubsribeInputEvents();
    }

    private void OpenUnlockAllCarsPopupIfPossible()
    {
        if(timesEnabled % 2 == 0 && !inventoryObj.AreAllCarsUnlocked())
        {
           // OpenTheWindow(ScreenIds.UnlockCars_IAP_Popup);
        }
    }

    private void SetCarAvailableSpriteIfAnyCarIsAvailable(GameEvent gameEvent = null)
    {
        int indexOfBlipCar = -1;
        var data = shopManager.CheckIfCarIsAvailableWithPlayerCoinsBeingMoreThanRequired(InventoryToItemPriceRelation.Twice);

        if (data.isValid)
        {
            indexOfBlipCar = data.index;
        }

        for (int i = 0; i < CarList.Length; i++)
        {
            CarInstance carInstance = CarList[i];
            carInstance.alertIconGameObject.SetActive(i == indexOfBlipCar);
        }
    }

    private IEnumerator turnOfAnimator()
    {
        yield return null;
        camera.enabled = true;
        yield return null;
        scrollRect.enabled = true;
        SnapScrollToCar(carToSnapToData.isValid ? carToSnapToData.index : selectedCarIndex); 

        yield return new WaitForSeconds(time);

      //  scrollRect.enabled = true;

        yield return null;

        animatorToDisable.enabled = false;
    //    SnapScrollToCar(carToSnapToData.isValid ? carToSnapToData.index : selectedCarIndex);
        arrowHandler();
        SnapInstance(saveManager.MainSaveFile.currentlySelectedCar);
    }

    public float GetCurrentAnimatorTime(Animator targetAnim, int layer = 0)
    {
        AnimatorClipInfo[] animState = targetAnim.GetCurrentAnimatorClipInfo(layer);
        float currentTime = animState[0].clip.length;
        return currentTime;
    }

    private void SubsribeInputEvents()
    {
        input.SwipeRightOccured.AddListener(ShowPreviousCarSwiped);
        input.SwipeLeftOccured.AddListener(ShowNextCarSwiped);
    }

    private void SubscribeToEvents()
    {
        onAllCarsPurchased.TheEvent.AddListener(RefreshUI);
        onCarPurchased.TheEvent.AddListener(SetCarAvailableSpriteIfAnyCarIsAvailable);
        onCarPurchased.TheEvent.AddListener(RefreshUI);
    }

    private void DeSubscribeToEvents()
    {
        onAllCarsPurchased.TheEvent.RemoveListener(RefreshUI);
        onCarPurchased.TheEvent.RemoveListener(SetCarAvailableSpriteIfAnyCarIsAvailable);
        onCarPurchased.TheEvent.RemoveListener(RefreshUI);
    }

    /// <summary>
    ///unsubscribing all events that can occur during this state
    /// </summary>
    public void UnSubsribeInputEvents()
    {
        input.SwipeRightOccured.RemoveListener(ShowPreviousCarSwiped);
        input.SwipeLeftOccured.RemoveListener(ShowNextCarSwiped);
    }

    private void RefreshUI(GameEvent gameEvent)
    {
        InitilizeUI(saveManager.MainSaveFile.currentlySelectedCar);
    }

    private void InitilizeUI(int val)
    {
        PopulateCarStats(saveManager.MainSaveFile.currentlySelectedCar, val);
        for (int i = 0; i < CarList.Length; i++)
        {
            var temp = CarList[i];
            SelectButton.SetActive(true);
            CoinsButton.SetActive(false);
            temp.SetCarNameTxt(carsDataBase.GetCarConfigurationData(i).GetName);
            temp.CurrentlySelectedCarTick(i == val);
            if (inventoryObj.isCarUnlocked(i)) // Car Unlocked
            {
                //     UnityEngine.Console.Log("Car is Unlocked " + i);

                temp.isNotSelected();

                if (i == val)
                {
                    levelRequiredLockObject.SetActive(false);
                    temp.isSelected();
                }
            }
            else
            {
                temp.isLockedAndNotSelected();
            }
        }
    }

    private void SnapScrollToCar(int indexToSnapTo)
    {
        RectTransform rectToSnapTo = CarList[indexToSnapTo].GetComponent<RectTransform>();
        scrollRect.SnapScrollToRect(rectToSnapTo, true);
    }

    private void PopulateCarStats(int current, int next)
    {
        thrustFill1.fillAmount = carsDataBase.GetCarConfigurationData(current).GetThrust / carsDataBase.MaxThrustDuration;
        thrustFill2.fillAmount = carsDataBase.GetCarConfigurationData(next).GetThrust / carsDataBase.MaxThrustDuration;

        airplaneFill1.fillAmount = carsDataBase.GetCarConfigurationData(current).GetAirplane / carsDataBase.MaxAeroplaneDuration;
        airplaneFill2.fillAmount = carsDataBase.GetCarConfigurationData(next).GetAirplane / carsDataBase.MaxAeroplaneDuration;

        boostFill1.fillAmount = carsDataBase.GetCarConfigurationData(current).GetBoost / carsDataBase.MaxBoostDuration;
        boostFill2.fillAmount = carsDataBase.GetCarConfigurationData(next).GetBoost / carsDataBase.MaxBoostDuration;
    }

    private void arrowHandler()
    {
        leftArrow.GetComponent<RectTransform>().localScale = new Vector3(-1, 1, 1);

        if (selectedCarIndex > 0 && selectedCarIndex < CarList.Length - 1)
        {
            rightArrow.GetComponent<Button>().interactable = true;
            rightArrow.GetComponent<Image>().color = new Color(255, 255, 255, 255);
            leftArrow.GetComponent<Button>().interactable = true;
            leftArrow.GetComponent<Image>().color = new Color(255, 255, 255, 255);
        }
        else if (selectedCarIndex == 0)
        {
            leftArrow.GetComponent<Button>().interactable = false;
            leftArrow.GetComponent<Image>().color = new Color(255, 255, 255, 75);
            rightArrow.GetComponent<Button>().interactable = true;
            rightArrow.GetComponent<Image>().color = new Color(255, 255, 255, 255);
        }
        else if (selectedCarIndex == CarList.Length - 1)
        {
            rightArrow.GetComponent<Button>().interactable = false;
            rightArrow.GetComponent<Image>().color = new Color(255, 255, 255, 75);
            leftArrow.GetComponent<Button>().interactable = true;
            leftArrow.GetComponent<Image>().color = new Color(255, 255, 255, 255);
        }
    }

    #region CarSelection

    private void ShowNextCarSwiped()
    {
        if (IsTheTouchOverScrollRect())
            return;

        showNextCar();
    }

    private void ShowPreviousCarSwiped()
    {
        if (IsTheTouchOverScrollRect())
            return;

        showPreviousCar();
    }

    private bool IsTheTouchOverScrollRect()
    {
        if (Input.touchCount == 0)
        {
            UnityEngine.Console.LogWarning("Swipe was detected on car selection panel but no touch count can be found");
            return false;
        }

        return RectTransformUtility.RectangleContainsScreenPoint(scrollRect.GetComponent<RectTransform>(), Input.GetTouch(0).rawPosition, uiCamera);
    }

    public void showNextCar()
    {
        int nextCarIndex = Mathf.Clamp(selectedCarIndex + 1, 0, CarList.Length - 1);
        ShowCar(nextCarIndex);
        SnapScrollToCar(nextCarIndex);
    }

    public void showPreviousCar()
    {
        int previousCarIndex = Mathf.Clamp(selectedCarIndex - 1, 0, CarList.Length - 1);
        ShowCar(previousCarIndex);
        SnapScrollToCar(previousCarIndex);
    }

    private void SnapInstance(int val)
    {
        for (int i = 0; i < CarList.Length; i++)
        {
            if (val == i)
            {
                contentPanel.transform.GetChild(i).localScale = new Vector2(1.1f, 1.1f);
            }
            else
            {
                contentPanel.transform.GetChild(i).localScale = new Vector2(1f, 1f);
            }
        }
    }

    public void ShowCar(int val)
    {
        if (selectedCarIndex == val)
            return;
        if (selectedCarIndex == 1 || selectedCarIndex == 2)
        {
            CostTextToShow.text= carsDataBase.GetCarConfigurationData(selectedCarIndex).GetPrice.ToString();
            Icon.sprite = icons[0];
        }
        else if(selectedCarIndex == 3 || selectedCarIndex == 4)
        {
            CostTextToShow.text = carsDataBase.GetCarConfigurationData(selectedCarIndex).GetPrice.ToString();
            Icon.sprite = icons[1];
        }
        onChangeVisibleCar.Invoke();

        carName.text = carsDataBase.GetCarConfigurationData(val).GetName.ToUpper();
        PopulateCarStats(saveManager.MainSaveFile.currentlySelectedCar, val);
        SnapInstance(val);
        KillSelectButtonAnimAndRewind();

        for (int i = 0; i < CarList.Length; i++)
        {
            var temp = CarList[i].GetComponent<CarInstance>();
            if (i == val)
            {
                if (inventoryObj.isCarUnlocked(i)) //Car i unlocked
                {
                    lockIcon.SetActive(false);
                  //  unlocksAtLvlTxt.text = "";
                    levelRequiredLockObject.SetActive(false);
                    SelectButton.SetActive(true);
                    CoinsButton.SetActive(false);

                    if (carToSnapToData.isValid)
                    {
                        PunchTheSelectButton();
                    }

                    temp.isSelected();
                }
                else
                {
                    CarConfigurationData carConfigurationData = carsDataBase.GetCarConfigurationData(val);
                    int price = carConfigurationData.GetPrice;
                    lockIcon.SetActive(true);
                    levelRequiredLockObject.SetActive(true);
                    unlocksAtLvlTxt.text = "UNLOCKS AT LVL " + (val * 5);
                    SelectButton.SetActive(false);
                    CoinsButton.SetActive(true);
                    CoinsButton.GetComponent<Button>().interactable = (playerLevelSystem.GetCurrentPlayerLevel() >= carConfigurationData.GetUnlockLevel);
                    CostTextToShow.text = price.ToString();
                    temp.isLockedAndSelected();
                }
                //UnityEngine.Console.LogError(contentPanel.transform.GetChild(i).name);
                //contentPanel.transform.GetChild(i).localScale = new Vector2(1.1f, 1.1f);
            }
            else
            {
                if (inventoryObj.isCarUnlocked(i)) //car i unlocked
                {
                    temp.isNotSelected();
                }
                else
                {
                    temp.isLockedAndNotSelected();
                }
                //contentPanel.transform.GetChild(i).localScale = new Vector2(1f, 1f);
            }
        }

        //SelectedArrowImage.SetActive(saveManager.MainSaveFile.currentlySelectedCar == val);
        if (saveManager.MainSaveFile.currentlySelectedCar == val)
        {
            selectedText.text = "SELECTED";
        }
        else
        {
            selectedText.text = "SELECT";
        }
        selectedCarIndex = val;
        arrowHandler();

        // Lock the UI till assest have been loaded
        LockTheSelectionUI();

        // Request car assets load
        var configData = carsDataBase.GetCarConfigurationData(selectedCarIndex);

        if (currentlyActiveCarAssetLoadingHandler != null)
        {
            currentlyActiveCarAssetLoadingHandler.AssetsLoaded -= HandleCarAssetsLoaded;
        }

        currentlyActiveCarAssetLoadingHandler = loadingHandler.LoadAssets(configData);

        if (currentlyActiveCarAssetLoadingHandler.isDone)
        {
            HandleCarAssetsLoaded(currentlyActiveCarAssetLoadingHandler.Result);
        }
        else
        {
            currentlyActiveCarAssetLoadingHandler.AssetsLoaded += HandleCarAssetsLoaded;
        }
    }

    private void PunchTheSelectButton()
    {
        if (selectButtonPunchTween != null)
        {
            selectButtonPunchTween.Play();
        }
        else
        {
            selectButtonPunchTween = SelectButton.transform.DOPunchScale(new Vector3(.14f, .14f, .14f), .8f, 0).SetLoops(-1);
        }
    }

    private void KillSelectButtonAnimAndRewind()
    {
        if (selectButtonPunchTween != null)
        {
            selectButtonPunchTween.Rewind();
            selectButtonPunchTween.Kill();
            selectButtonPunchTween = null;
        }
    }

    private IEnumerator CarGenerationDelay()
    {
        //  if (!displayCarBool)
        //  {
        //   displayCarBool = true;

        Animator carAnimator;

        // First Time
        if (currentDisplayingCar == null)
        {
            currentDisplayingCar = Instantiate(displayGameObject, parentObjForCars.transform, false);
            carAnimator = currentDisplayingCar.GetComponent<Animator>();
            carAnimator.SetBool("carEnter", true);
            //   displayCarBool = false;
        }
        else
        {
            previousDisplayingCar = currentDisplayingCar;
            carAnimator = previousDisplayingCar.GetComponent<Animator>();

            // Reached origin
            if (previousDisplayingCar.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                carAnimator.SetBool("carEnter", false);
                carAnimator.SetBool("carExit", true);

                StartCoroutine(DestroyCarAfterDelay(previousDisplayingCar, carAnimator));
            }
            else
            {
                Destroy(previousDisplayingCar);
            }

            currentDisplayingCar = null;
            yield return new WaitForSeconds(0.7f);

            currentDisplayingCar = Instantiate(displayGameObject, parentObjForCars.transform, false);
            currentDisplayingCar.GetComponent<Animator>().SetBool("carEnter", true);

            //  yield return new WaitForSeconds(1f);

            //  Destroy(previousDisplayingCar);
            //CurrentClipInfo = currentDisplayingCar.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0);

            //m_CurrentClipLength = CurrentClipInfo[0].clip.length;
            //if (m_CurrentClipLength > 0.9)
            //{
            //    displayCarBool = false;
            //    nextDisplayingCar = currentDisplayingCar;
            //}
        }
        // }

        //  carGeneration = null;
    }

    private IEnumerator DestroyCarAfterDelay(GameObject car, Animator animator)
    {
        yield return new WaitForEndOfFrame();

        yield return new WaitForSeconds(animator.GetNextAnimatorStateInfo(0).length);

        if (car != null)
        {
            Destroy(car);
        }
    }

    public void SelectCar()
    {
        saveManager.MainSaveFile.currentlySelectedCar = selectedCarIndex;
        selectedText.text = "SELECTED";
        //SelectedArrowImage.SetActive(true);
        InitilizeUI(selectedCarIndex);
        PopulateCarStats(saveManager.MainSaveFile.currentlySelectedCar, selectedCarIndex);
        newCarIsGoingToSpawn.RaiseEvent();
        carHasBeenSelected.RaiseEvent();
        SendCarEvent("CarSelect", carsDataBase.GetCarConfigurationData(selectedCarIndex).GetName);

        KillSelectButtonAnimAndRewind();
    }

    private void HandleCarAssetsLoaded(PlayerCarAssets playerCarAssets)
    {
        // PlayerCarAssets playerCarAssets = loadingHandler.GetLoadedPlayerAssets(selectedCarIndex);
        displayGameObject = playerCarAssets.DisplayGameObject;

        if (carGeneration != null)
        {
            StopCoroutine(carGeneration);
        }

        carGeneration = StartCoroutine(CarGenerationDelay());

        UnLockTheSelectionUI();
    }

    private void LockTheSelectionUI()
    {
        backButton.interactable = false;
        loadingIconGameObject.SetActive(true);
    }

    private void UnLockTheSelectionUI()
    {
        backButton.interactable = true;
        loadingIconGameObject.SetActive(false);
    }

    #endregion CarSelection

    public void ShowToolTipText()
    {
        for (int i = 0; i < toolTipTexts.Length; i++)
        {
            if (i == selectedCarIndex)
            {
                toolTipInfoText.text = toolTipTexts[i];
            }
        }
    }

    public void UnlockCarWithCoins()
    {
        
        List<string> thingsGot = new List<string>();//z
        List<int> amountsGot = new List<int>();//z
        if (selectedCarIndex == 1 || selectedCarIndex == 2)
        {
            int price = carsDataBase.GetCarConfigurationData(selectedCarIndex).GetPrice;
            Icon.sprite = icons[0];
            if (inventoryObj.GetIntKeyValue("AccountCoins") >= price)
            {
                inventoryObj.UpdateKeyValues(new List<InventoryItem<int>>() { new InventoryItem<int>("AccountCoins", -price) });

                inventoryObj.UnlockCar(selectedCarIndex);

                thingsGot.Add("Car");
                amountsGot.Add(1);

                InitilizeUI(selectedCarIndex);

                CarConfigurationData carConfigurationData = carsDataBase.GetCarConfigurationData(selectedCarIndex);
                SendCarEvent("CarBuyWithCoin", carConfigurationData.GetName);

                // OpenTheWindow(ScreenIds.CarUnlockedScreen, new CarAvailableProperties() { CarConfigData = new PlayableObjectDataWithIndex(carConfigurationData, selectedCarIndex) });
            }
            else
            {
                OpenTheWindow(ScreenIds.ResourcesNotAvailable);
            }
            purchaseEvent.RaiseEvent(thingsGot, "AccountCoins", price, amountsGot);
        }
        else if(selectedCarIndex == 3 || selectedCarIndex == 4)
        {
            Icon.sprite = icons[1];
            int price = carsDataBase.GetCarConfigurationData(selectedCarIndex).GetPrice;

            if (inventoryObj.GetIntKeyValue("AccountDiamonds") >= price)
            {
                inventoryObj.UpdateKeyValues(new List<InventoryItem<int>>() { new InventoryItem<int>("AccountDiamonds", -price) });

                inventoryObj.UnlockCar(selectedCarIndex);

                thingsGot.Add("Car");
                amountsGot.Add(1);

                InitilizeUI(selectedCarIndex);

                CarConfigurationData carConfigurationData = carsDataBase.GetCarConfigurationData(selectedCarIndex);
                SendCarEvent("CarBuyWithGems", carConfigurationData.GetName);

                // OpenTheWindow(ScreenIds.CarUnlockedScreen, new CarAvailableProperties() { CarConfigData = new PlayableObjectDataWithIndex(carConfigurationData, selectedCarIndex) });
            }
            else
            {
                OpenTheWindow(ScreenIds.ResourcesNotAvailable);
            }
        }
      




    }

    private void SendCarEvent(string eventName, string carName)
    {
        AnalyticsManager.CustomData($"CarSelectionScreen_{eventName}", new Dictionary<string, object>
        {
            {"Car",carName}
        });

        //UnityEngine.Console.LogError($"CarSelectionScreen_{eventName}_{carName}");
    }
    public void CarPanelOpen(string panelName)
    {
        MainMenuManager.Instance.OpenTheWindow(panelName);

    }
}