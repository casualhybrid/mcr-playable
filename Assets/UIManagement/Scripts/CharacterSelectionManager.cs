using deVoid.UIFramework;
using Knights.UISystem;
using RotaryHeart.Lib.SerializableDictionary;
using System.Collections;
using System.Collections.Generic;
using TheKnights.SaveFileSystem;
using TMPro;
using Unity.Services.Analytics;
using UnityEngine;

using UnityEngine.UI;

public class CharacterSelectionManager : AWindowController
{
    [System.Serializable]
    private class CharToFigurinesReq : SerializableDictionaryBase<int, int>
    { }

    [SerializeField] private SaveManager saveManager;
    [SerializeField] private PlayerCharacterLoadingHandler loadingHandler;
    [SerializeField] private CharactersDataBase charactersDataBase;
    [SerializeField] private InventorySystem inventoryObj;
    [SerializeField] private InputChannel input;
    [SerializeField] private ShopPurchaseEvent purchaseEvent;


    [SerializeField] private GameObject[] charactersList, lockedCharactersList, characterButtons;
    [SerializeField] private Sprite[] charAbility;
    [SerializeField] private string[] abilities;
    [SerializeField] private GameObject rightArrow, leftArrow, lockIcon, contentPanel;
    [SerializeField] private Image fillImage, abilityIcon;
   // [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Button backButton;
    public GameObject /*SelectedArrowImage,*/ SelectButton, DiamondsButtons, WatchAdbutton, fillBar, figureIcon/*, tooltipButton*/;

    public TextMeshProUGUI FigrineBodyPartText, FigrineName, unlockableTxt, abilityName, figurineCompletedTxt, selectedText;
    [SerializeField] private GameEvent CharacterHasBeenslected;
    private int pressedCharacterNumber;

    // public string[] toolTipTexts;
    //   public TextMeshProUGUI toolTipInfoText;
    private int selectedCharacterIndex;

    private GameObject displayGameObject;
    [SerializeField] private Transform parentObjForCharacters;
    private GameObject currentDisplayingCharacter;
    [SerializeField] private GameObject loadingIconGameObject;
    [SerializeField] private Animator animatorToDisable;
    private float time;
    private AsyncOperationSpawning<PlayerCharacterAssets> currentlyActiveCharacterAssetLoadingHandler;

    [SerializeField] Sprite[] icons;
    [SerializeField] Image Icon;
    [SerializeField] GameObject profile;
    [SerializeField] TextMeshProUGUI costText;
 //   private Camera uiCamera;

    protected override void Awake()
    {
        base.Awake();
       // uiCamera = GetComponentInParent<UIFrame>().UICamera;
    }

    private void OnEnable()
    {
        if (profile)
            profile.SetActive(false);
        PersistentAudioPlayer.Instance.PanelSounds();
        //PlayerPrefs.SetInt("figurinePartsCount0", 3);
        time = GetCurrentAnimatorTime(animatorToDisable);
        StartCoroutine(turnOfAnimator());
        InitilizeUI(saveManager.MainSaveFile.currentlySelectedCharacter);
        selectedCharacterIndex = saveManager.MainSaveFile.currentlySelectedCharacter;

        arrowHandler();
        SubsribeInputEvents();
    }

    private void OnDisable()
    {
        UnSubsribeInputEvents();
        animatorToDisable.enabled = true;
    }
    public void OnClickClose()
    {
        PersistentAudioPlayer.Instance.PlayAudio();
    }
    private bool IsTheTouchOverScrollRect()
    {
        //if (Input.touchCount == 0)
        //{
        //    UnityEngine.Console.LogWarning("Swipe was detected on car selection panel but no touch count can be found");
        //    return false;
        //}

        //return RectTransformUtility.RectangleContainsScreenPoint(scrollRect.GetComponent<RectTransform>(), Input.GetTouch(0).rawPosition, uiCamera);

        // Till we need a scrol rect
        return false;
    }

    private IEnumerator turnOfAnimator()
    {
        ShowCharacter(saveManager.MainSaveFile.currentlySelectedCharacter);
        yield return new WaitForSeconds(time);
        animatorToDisable.enabled = false;
        arrowHandler();
        SnapInstance(selectedCharacterIndex);
        //   ShowCharacter(saveManager.MainSaveFile.currentlySelectedCharacter);
    }

    public float GetCurrentAnimatorTime(Animator targetAnim, int layer = 0)
    {
        AnimatorClipInfo[] animState = targetAnim.GetCurrentAnimatorClipInfo(layer);
        float currentTime = animState[0].clip.length;
        return currentTime;
    }

    private void SubsribeInputEvents()
    {
        input.SwipeRightOccured.AddListener(ShowPreviousCharacterSwiped);
        input.SwipeLeftOccured.AddListener(ShowNextCharacterSwiped);
    }

    /// <summary>
    ///unsubscribing all events that can occur during this state
    /// </summary>
    public void UnSubsribeInputEvents()
    {
        input.SwipeRightOccured.RemoveListener(ShowPreviousCharacterSwiped);
        input.SwipeLeftOccured.RemoveListener(ShowNextCharacterSwiped);
    }

    private void InitilizeUI(int val)
    {
        SelectButton.SetActive(true);
        DiamondsButtons.SetActive(false);
        FigrineBodyPartText.gameObject.SetActive(false);
        unlockableTxt.text = "UNLOCKED";
        figurineCompletedTxt.text = "COMPLETED";
        lockIcon.SetActive(false);
        fillImage.fillAmount = 1;
        abilityIcon.sprite = charAbility[val];
        abilityName.text = abilities[val];
        for (int i = 0; i < charactersList.Length; i++)
        {
            var temp = characterButtons[i].GetComponent<CharacterInstance>();
            temp.SetCharNameTxt(charactersDataBase.GetCharacterConfigurationData(i).GetName);
            temp.CurrentlySelectedCharacterTick(i == val);
            //  charactersList[i].SetActive(false);
            lockedCharactersList[i].SetActive(false);

            int reqFig = charactersDataBase.GetCharacterConfigurationData(i).FigurinesToUnlock;
            int curFig = inventoryObj.GetCharacterFigurines(i);

            if (curFig >= reqFig) //character  unlocked
            {
                if (i == val)
                {
                    temp.isSelected();
                    charactersList[i].SetActive(true);
                }
                else
                {
                    temp.isNotSelected();
                }

            }
            else
            {
                temp.isLockedAndNotSelected();
            }
        }
    }

    private void arrowHandler()
    {
        leftArrow.GetComponent<RectTransform>().localScale = new Vector3(-1, 1, 1);
        if (selectedCharacterIndex > 0 && selectedCharacterIndex < characterButtons.Length - 1)
        {
            rightArrow.GetComponent<Button>().interactable = true;
            rightArrow.GetComponent<Image>().color = new Color(255, 255, 255, 255);
            leftArrow.GetComponent<Button>().interactable = true;
            leftArrow.GetComponent<Image>().color = new Color(255, 255, 255, 255);
        }
        else if (selectedCharacterIndex == 0)
        {
            leftArrow.GetComponent<Button>().interactable = false;
            leftArrow.GetComponent<Image>().color = new Color(255, 255, 255, 75);
            rightArrow.GetComponent<Button>().interactable = true;
            rightArrow.GetComponent<Image>().color = new Color(255, 255, 255, 255);
        }
        else if (selectedCharacterIndex == characterButtons.Length - 1)
        {
            rightArrow.GetComponent<Button>().interactable = false;
            rightArrow.GetComponent<Image>().color = new Color(255, 255, 255, 75);
            leftArrow.GetComponent<Button>().interactable = true;
            leftArrow.GetComponent<Image>().color = new Color(255, 255, 255, 255);
        }
    }

    private void ShowNextCharacterSwiped()
    {
        if (IsTheTouchOverScrollRect())
            return;

        showNextCharacter();
    }

    private void ShowPreviousCharacterSwiped()
    {
        if (IsTheTouchOverScrollRect())
            return;

        showPreviousCharacter();
    }

    public void showNextCharacter()
    {
        selectedCharacterIndex += 1;
        if (selectedCharacterIndex < characterButtons.Length)
        {
            arrowHandler();
            ShowCharacter(selectedCharacterIndex);
        }
        else
        {
            arrowHandler();
            ShowCharacter(selectedCharacterIndex = characterButtons.Length - 1);
        }
    }

    public void showPreviousCharacter()
    {
        selectedCharacterIndex -= 1;
        if (selectedCharacterIndex > -1)
        {
            arrowHandler();
            ShowCharacter(selectedCharacterIndex);
        }
        else
        {
            arrowHandler();
            ShowCharacter(selectedCharacterIndex = 0);
        }
    }

    void SnapInstance(int val)
    {
        for (int i = 0; i < charactersList.Length; i++)
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

    public void ShowCharacter(int val)
    {
        pressedCharacterNumber = val;
        if(pressedCharacterNumber==1)
        {
            costText.text = "15000";
            Icon.sprite = icons[0];
        }
        else
        {
            costText.text = "300";
            Icon.sprite = icons[1];
        }

            FigrineName.text = charactersDataBase.GetCharacterConfigurationData(val).GetName;
        abilityIcon.sprite = charAbility[val];
        abilityName.text = abilities[val];
        SnapInstance(val);
        //UnityEngine.Console.Log("Character Number " + val + " is clicked!");

        for (int i = 0; i < charactersList.Length; i++)
        {
            var temp = characterButtons[i].GetComponent<CharacterInstance>();
            //   charactersList[i].SetActive(false);
            lockedCharactersList[i].SetActive(false);

            if (val == i)
            {

                int reqFig = charactersDataBase.GetCharacterConfigurationData(i).FigurinesToUnlock;
                int curFig = inventoryObj.GetCharacterFigurines(i);

                if (curFig >= reqFig) //character is unlocked
                {
                    SelectButton.SetActive(true);
                    DiamondsButtons.SetActive(false);
                    //WatchAdbutton.SetActive(false);
                    FigrineBodyPartText.gameObject.SetActive(false);
                    fillImage.fillAmount = 1;
                    lockIcon.SetActive(false);
                    //tooltipButton.SetActive(false);

                    if (val == 0)
                    {
                        unlockableTxt.text = "AVAILABLE";
                        figurineCompletedTxt.text = string.Empty;
                        fillBar.SetActive(false);
                        figureIcon.SetActive(false);
                    }
                    else
                    {
                        unlockableTxt.text = "UNLOCKED";
                        figurineCompletedTxt.text = "COMPLETED";
                        fillBar.SetActive(true);
                        figureIcon.SetActive(true);
                    }

                    temp.isSelected();

                    //    charactersList[i].SetActive(true);

                    LoadTheCharacter();
                }
                else
                {
                    if (currentDisplayingCharacter != null)
                    {
                        Destroy(currentDisplayingCharacter);
                    }

                    fillBar.SetActive(true);
                    figureIcon.SetActive(true);
                    SelectButton.SetActive(false);
                    DiamondsButtons.SetActive(true);
                    FigrineBodyPartText.gameObject.SetActive(true);
                    int curFigurine = inventoryObj.GetCharacterFigurines(val);
                    int reqFigurine = charactersDataBase.GetCharacterConfigurationData(val).FigurinesToUnlock;
                    FigrineBodyPartText.text = (curFigurine + " / " + reqFigurine);
                    fillImage.fillAmount = (float)curFigurine / (float)reqFigurine;
                    lockIcon.SetActive(true);
                    unlockableTxt.text = "UNLOCKABLE";
                    figurineCompletedTxt.text = "COMPLETED";
                    temp.isLockedSelected();
                    //  lockedCharactersList[i].SetActive(true);

                    LoadTheCharacter();
                }
             
            }
            else
            {
                int reqFigurine = charactersDataBase.GetCharacterConfigurationData(i).FigurinesToUnlock;
                int curFigurine = inventoryObj.GetCharacterFigurines(i);

                if (curFigurine >= reqFigurine) //character is unlocked
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

        if (saveManager.MainSaveFile.currentlySelectedCharacter == val)
        {
            selectedText.text = "SELECTED";
        }
        else
        {
            selectedText.text = "SELECT";
        }
        selectedCharacterIndex = val;
        arrowHandler();
        pressedCharacterNumber = val;

 
    }

    private void LoadTheCharacter()
    {
        // Lock the UI till assest have been loaded
        LockTheSelectionUI();

        // Request car assets load
        var config = charactersDataBase.GetCharacterConfigurationData(pressedCharacterNumber);

        if (currentlyActiveCharacterAssetLoadingHandler != null)
        {
            currentlyActiveCharacterAssetLoadingHandler.AssetsLoaded -= HandleCharacterAssetsLoaded;
        }

        currentlyActiveCharacterAssetLoadingHandler = loadingHandler.LoadAssets(config);

        if (currentlyActiveCharacterAssetLoadingHandler.isDone)
        {
            HandleCharacterAssetsLoaded(currentlyActiveCharacterAssetLoadingHandler.Result);
        }
        else
        {
            currentlyActiveCharacterAssetLoadingHandler.AssetsLoaded += HandleCharacterAssetsLoaded;
        }
    }

    private void HandleCharacterAssetsLoaded(PlayerCharacterAssets assets)
    {
        var configData = charactersDataBase.GetCharacterConfigurationData(pressedCharacterNumber);
        PlayerCharacterAssets playerCharacterAssets = loadingHandler.GetLoadedPlayerAssets(configData.GetName);
        displayGameObject = playerCharacterAssets.DisplayGameObject;

        UnLockTheSelectionUI();

        if (currentDisplayingCharacter != null)
        {
            Destroy(currentDisplayingCharacter);
        }

        currentDisplayingCharacter = Instantiate(displayGameObject, parentObjForCharacters, false);
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

    public void SelectCharacter()
    {
       

        //SelectedArrowImage.SetActive(true);
        selectedText.text = "SELECTED";
        saveManager.MainSaveFile.currentlySelectedCharacter = pressedCharacterNumber;
        InitilizeUI(pressedCharacterNumber);
        CharacterHasBeenslected.RaiseEvent();

        SendCharacterSelectAnalytic(charactersDataBase.GetCharacterConfigurationData(selectedCharacterIndex).GetName);
    }

    //public void ShowToolTipText()
    //{
    //    for (int i = 0; i < toolTipTexts.Length; i++)
    //    {
    //        if (i == pressedCharacterNumber)
    //        {
    //            toolTipInfoText.text = toolTipTexts[i];
    //        }
    //    }
    //}

    public void UnlockCharacterFigurineWithDiamonds()
    {
      
        List<string> thingsGot = new List<string>();//z
        List<int> amountsGot = new List<int>();//z
        if (pressedCharacterNumber == 2)
        {
            costText.text = "300";
            Icon.sprite = icons[1];
            if (inventoryObj.GetIntKeyValue("AccountDiamonds") >= 300)
            {
                int requiredFig = charactersDataBase.GetCharacterConfigurationData(pressedCharacterNumber).FigurinesToUnlock;
                inventoryObj.UpdateKeyValues(new List<InventoryItem<int>>() { new InventoryItem<int>("AccountDiamonds", -300) });
                int updatedFig = inventoryObj.UpdateCharacterFigurine(pressedCharacterNumber, 1);
                FigrineBodyPartText.text = updatedFig + " / " + requiredFig;

                fillImage.fillAmount = (float)updatedFig / (float)requiredFig;

                if (updatedFig >= requiredFig)
                {
                    SendCharacterBuyAnalytic(charactersDataBase.GetCharacterConfigurationData(pressedCharacterNumber).GetName);
                    InitilizeUI(pressedCharacterNumber);
                    ShowCharacter(pressedCharacterNumber);
                }

                thingsGot.Add("Character Figurine");
                amountsGot.Add(1);

            }

            else
            {
                OpenTheWindow(ScreenIds.ResourcesNotAvailable);
            }


            purchaseEvent.RaiseEvent(thingsGot, "AccountDiamonds", 300, amountsGot);
        }
        else if(pressedCharacterNumber == 1)
        {
            costText.text = "1500";
            Icon.sprite = icons[0];
            if (inventoryObj.GetIntKeyValue("AccountCoins") >= 1500)
            {
                int requiredFig = charactersDataBase.GetCharacterConfigurationData(pressedCharacterNumber).FigurinesToUnlock;
                inventoryObj.UpdateKeyValues(new List<InventoryItem<int>>() { new InventoryItem<int>("AccountCoins", -1500) });
                int updatedFig = inventoryObj.UpdateCharacterFigurine(pressedCharacterNumber, 1);
                FigrineBodyPartText.text = updatedFig + " / " + requiredFig;

                fillImage.fillAmount = (float)updatedFig / (float)requiredFig;

                if (updatedFig >= requiredFig)
                {
                    SendCharacterBuyAnalytic(charactersDataBase.GetCharacterConfigurationData(pressedCharacterNumber).GetName);
                    InitilizeUI(pressedCharacterNumber);
                    ShowCharacter(pressedCharacterNumber);
                }

                thingsGot.Add("Character Figurine");
                amountsGot.Add(1);

            }

            else
            {
                OpenTheWindow(ScreenIds.ResourcesNotAvailable);
            }


            purchaseEvent.RaiseEvent(thingsGot, "AccountCoins", 1500, amountsGot);
        }
    }

    private void SendCharacterBuyAnalytic(string characterName)
    {
        AnalyticsManager.CustomData("CharacterSelectionScreen_CharacterBuy", new Dictionary<string, object> { { "CharacterName", characterName } });

    }

    private void SendCharacterSelectAnalytic(string characterName)
    {
        AnalyticsManager.CustomData("CharacterSelectionScreen_CharacterSelect", new Dictionary<string, object> { { "CharacterName", characterName } });
    }
    public void CarPanelOpen(string panelName)
    {
        MainMenuManager.Instance.OpenTheWindow(panelName);

    }
}