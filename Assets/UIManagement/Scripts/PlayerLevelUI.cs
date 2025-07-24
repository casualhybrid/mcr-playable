using System.Collections;
using System.Collections.Generic;
using TheKnights.SaveFileSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLevelUI : MonoBehaviour
{
    [SerializeField] private PlayerLevelingSystem playerLevelObj;

    [SerializeField] private Slider playerLevelSlider;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private TextMeshProUGUI playerLevelTxt;
    [SerializeField] private Image carIcon;
    [SerializeField] private CarsDataBase carsDataBase;
    [Header("Profile")]
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private TextMeshProUGUI playerLevelTxtProfile;
    [SerializeField] private TextMeshProUGUI totalCoins;
    [SerializeField] private TextMeshProUGUI totalGems;
    [SerializeField] private TextMeshProUGUI highScore;
    [SerializeField] private TextMeshProUGUI highScorepROFILE;
    [SerializeField] private TextMeshProUGUI totalEnergies;
    [SerializeField] private TextMeshProUGUI totalEnergiesRefrence;

    /*[SerializeField] Image mainProfile;
    [SerializeField] List<Sprite> profileIcons;*/


    [SerializeField] private InventorySystem playerInventoryObj;
    [SerializeField] private GamePlaySessionInventory gameplayInventoryObj;

    [Header("UserName")]
    [SerializeField] TMP_InputField inputField;
    private string previousText = "";
    [SerializeField] TextMeshProUGUI profileName;

    public Button changeNameButton;



    private void OnEnable()
    {
        int curPlayerLevel = playerLevelObj.GetCurrentPlayerLevel();
        playerLevelTxt.text = curPlayerLevel.ToString();

        float sliderVal = Mathf.Clamp01(playerLevelObj.GetPlayerCurrentXP() / playerLevelObj.GetXPNeededForNextLevel());
        playerLevelSlider.value = sliderVal;
        playerName.text = saveManager.MainSaveFile.leaderBoardUserName;
        profileName.text = playerName.text;
        //Profile
        totalCoins.text = playerInventoryObj.GetIntKeyValue("AccountCoins").ToString(); ;
        totalGems.text = playerInventoryObj.GetIntKeyValue("AccountDiamonds").ToString();
        highScore.text = gameplayInventoryObj.GetPlayerHighestScore().ToString();
        highScorepROFILE.text = highScore.text;
        playerLevelTxtProfile.text = "LEVEL " + playerLevelTxt.text;
        StartCoroutine(WaitForBoster());
        //Debug.Log("HighScore : "+ gameplayInventoryObj.GetPlayerHighestScore());
        CarConfigurationData maxSupportedCarByLevel = carsDataBase.GetMaximumSupportedCarForThePlayerLevel(curPlayerLevel);

        if (maxSupportedCarByLevel != null)
        {
            carIcon.sprite = maxSupportedCarByLevel.GetCarSprite;
        }
    }
    void Start()
    {
        string savedName = PlayerPrefs.GetString("PlayerName", "");
        //nameInput.text = savedName;
        playerName.text = savedName;
        profileName.text = savedName;
        /*saveButton.interactable = false; // Button disabled at start
        nameInput.onValueChanged.AddListener(OnNameChanged);
        saveButton.onClick.AddListener(SaveName);

        saveButton.interactable = !string.IsNullOrWhiteSpace(savedName);*/
        // Save the initial value
        previousText = savedName;

        // Assign listeners
        //inputField.onSelect.AddListener(OnFieldSelected);
        // inputField.onEndEdit.AddListener(OnFieldEndEdit);

        inputField.onValueChanged.AddListener(OnInputChanged);
        changeNameButton.onClick.AddListener(OnChangeNameClicked);


    }
    void OnInputChanged(string userInput)
    {
        // Enable the button as soon as the user types anything
        changeNameButton.gameObject.transform.GetChild(0).gameObject.SetActive(false);
    }
    public void OpenChangePanel()
    {
        changeNameButton.gameObject.transform.GetChild(0).gameObject.SetActive(true);
    }
    void OnChangeNameClicked()
    {
        string newName = inputField.text;
        if (!string.IsNullOrWhiteSpace(newName))
        {
            //changeNameButton.GetComponent<Image>().enabled = false;
            changeNameButton.transform.GetChild(0).gameObject.SetActive(false);
            previousText = newName;
            SaveInput(newName); // Call your save logic here
        }
    }
    void OnFieldSelected(string _)
    {
        
        inputField.text = previousText;
        //inputField.text = "";
    }
    IEnumerator WaitForBoster()
    {
        yield return new WaitForSeconds(1f);
        totalEnergies.text =/*gameplayInventoryObj.GetIntKeyValue("GameBoost").ToString();*/totalEnergiesRefrence.text;
        //Debug.Log(gameplayInventoryObj.GetIntKeyValue("GameBoost")+"Energies"+totalEnergiesRefrence.text);
    }

    void OnFieldEndEdit(string userInput)
    {
        if (string.IsNullOrWhiteSpace(userInput))
        {
            // User didn't type anything → restore old text
            inputField.text = previousText;
        }
        else
        {
            // User typed new input → save it
            previousText = userInput;
            SaveInput(userInput); // Call your save logic here
        }
        //playerName.gameObject.SetActive(true);
    }

    void SaveInput(string input)
    {
        Debug.Log("Saving: " + input);
        // Your saving logic here
        PlayerPrefs.SetString("PlayerName", input);
        playerName.text= previousText;
        profileName.text= previousText;
    }
    /* void OnNameChanged(string input)
     {
         saveButton.interactable = !string.IsNullOrWhiteSpace(input);
     }*/

    /* void SaveName()
     {
         string PlayerName = nameInput.text;
         PlayerPrefs.SetString("PlayerName", PlayerName);
         PlayerPrefs.Save();
         Debug.Log("Name Saved: " + playerName);
         playerName.text = PlayerName;
         saveButton.interactable = false;
     }*/
    /*  public void ChangeProfileIcon(int index)
      {
          mainProfile.sprite = profileIcons[index];
      }*/
}