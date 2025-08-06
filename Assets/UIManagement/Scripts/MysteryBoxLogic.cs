using deVoid.UIFramework;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class MysteryBoxLogic : AWindowController
{
    enum inventoryType
    {
        reward, 
        gamePlay,
        player
    };
    [SerializeField] private InventorySystem inventoryObj;
    [SerializeField] private GamePlaySessionInventory gameplayInventoryObj;
    [SerializeField] private GameObject openBtn, boxImg;
    [SerializeField] private Sprite boxClosed, boxOpened;
    [SerializeField] private GameObject congratulationTxt;
    [SerializeField] private inventoryType typeOfInventory;
    [SerializeField] private GameEvent mysteryBoxEvent;

    /* Economy Update:
     * Coins  = 20%
     * Diamonds = 5%
     * (~) Character Figurines = 30%
     * Boosts = 25%
     * Headstarts = 20%
     */

    // Start is called before the first frame update
    private Dictionary<string, float> probabilities;

    private Dictionary<string, float> updatedProbabilities;
    private Dictionary<string, State> states;
    private string currentItem = string.Empty;
    private string diamondKey = "Diamond";
    private string coinKey = "Coins";
    private string HeadStartKey = "Head Starts";
    private string CharacerFigurineKey = "CharacerFigurine";
    private string boostKey = "Boosts";
    private bool firstTime = true;
    private int amount = 0;

    private string coinInventoryKey = "AccountCoins";
    private string boostInventoryKey = "GameBoost";
    private string diamondInventoryKey = "AccountDiamonds";
    private string headStartInventoryKey = "GameHeadStart";

    //private string CoinPlayerPref = "COINS";
    //private string DiamondPlayerPref = "DIAMONDS";
    //private string HeadStartPlayerPref = "HEAD_STARTS";

    [SerializeField]
    private Text displayText;

    //string characterFigurineKey = "CharacterFigurine";

    private enum State
    {
        Dead,
        Awake
    }

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        UpdateUI();
        mysteryBoxEvent.TheEvent.AddListener(mysteryBoxPanelClose);
    }

    public void SendEvents(string eventName)
    {
        AnalyticsManager.CustomData(eventName);
    }

    private void Start()
    {
    }

    private void UpdateUI()
    {
        probabilities = new Dictionary<string, float>();
        probabilities.Add(diamondKey, 7.1429f);
        probabilities.Add(coinKey, 28.5714f);
        probabilities.Add(HeadStartKey, 28.5714f);
        probabilities.Add(boostKey, 35.7143f);
        //probabilities.Add(characterFigurineKey, 30f);

        states = new Dictionary<string, State>();
        states.Add(diamondKey, State.Awake);
        states.Add(coinKey, State.Awake);
        states.Add(HeadStartKey, State.Awake);
        states.Add(boostKey, State.Awake);
        //states.Add(characterFigurineKey, State.Awake);

        updatedProbabilities = new Dictionary<string, float>();
        updatedProbabilities.Add(diamondKey, 7.1429f);
        updatedProbabilities.Add(coinKey, 28.5714f);
        updatedProbabilities.Add(HeadStartKey, 28.5714f);
        updatedProbabilities.Add(boostKey, 35.7143f);
    }

    private void UpdateProbabilities()
    {
        float sum = 0f;
        foreach (KeyValuePair<string, float> s in updatedProbabilities)
        {
            if (s.Value != 0)
            {
                sum += s.Value;
            }
        }

        //UnityEngine.Console.Log("Sum = " + sum);

        List<string> keys = new List<string>();
        List<float> values = new List<float>();

        foreach (KeyValuePair<string, float> s in updatedProbabilities)
        {
            if (s.Value != 0)
            {
                keys.Add(s.Key);
                values.Add(s.Value);
            }
        }

        for (int i = 0; i < keys.Count; i++)
        {
            updatedProbabilities[keys[i]] = (values[i] / sum) * 100f;
        }
    }

    public void OpenMysteryBox()
    {
        
        if(typeOfInventory == inventoryType.player)
        {
            if (inventoryObj.GetIntKeyValue("GameMysteryBox") > 0)
            {
                inventoryObj.UpdateKeyValues(new List<InventoryItem<int>>() { new InventoryItem<int>("GameMysteryBox", -1) });
                MysteryBoxDetails();
            }

        }
        else if (typeOfInventory == inventoryType.reward) 
        {
            if (PlayerPrefs.GetInt("AwardBox") > 0)
            {
                PlayerPrefs.SetInt("AwardBox", PlayerPrefs.GetInt("AwardBox") - 1);
                MysteryBoxDetails();
            }

        }
        else if (typeOfInventory == inventoryType.gamePlay)
        {
            if (gameplayInventoryObj.GetIntKeyValue("GameMysteryBox") > 0)
            {
                gameplayInventoryObj.AddThisKeyToGamePlayInventory("GameMysteryBox", -1);
                MysteryBoxDetails();
            }

        }

    }
    void mysteryBoxPanelClose(GameEvent gameEvent)
    {
        if (typeOfInventory == inventoryType.player)
        {
            if (inventoryObj.GetIntKeyValue("GameMysteryBox") == 0)
            {
                CloseTheWindow(ScreenId);
            }
        }
        else if (typeOfInventory == inventoryType.reward)
        {
            if (PlayerPrefs.GetInt("AwardBox") == 0)
            {
                CloseTheWindow(ScreenId);
            }
        }
        else if (typeOfInventory == inventoryType.gamePlay)
        {
            if (gameplayInventoryObj.GetIntKeyValue("GameMysteryBox") == 0)
            {
                CloseTheWindow(ScreenId);
            }
        }
    }
    public void MysteryBoxDetails()
    {
        mysteryBoxEvent.RaiseEvent();
        float randomItemNumber = Random.Range(1f, 100);
        if (firstTime)
        {
            if (randomItemNumber >= 1 && randomItemNumber < probabilities[diamondKey])
            {
                currentItem = diamondKey;
            }
            else if (randomItemNumber >= probabilities[diamondKey] && randomItemNumber < probabilities[coinKey] + probabilities[diamondKey])
            {
                currentItem = coinKey;
            }
            else if (randomItemNumber >= probabilities[coinKey] + probabilities[diamondKey] && randomItemNumber < probabilities[HeadStartKey] + probabilities[coinKey] + probabilities[diamondKey])
            {
                currentItem = HeadStartKey;
            }
            else if (randomItemNumber >= probabilities[HeadStartKey])
            {
                currentItem = boostKey;
            }
            else
            {
                UnityEngine.Console.LogError("Something Weird Happened");
            }

            if (currentItem != null)
            {
                states[currentItem] = State.Dead;
                updatedProbabilities[currentItem] = 0f;
                UpdateProbabilities();
            }
            else
            {

            }
            firstTime = false;
        }
        else if (!firstTime)
        {
            Dictionary<string, float> awakeProbabilities = new Dictionary<string, float>();
            List<string> awakeKeys = new List<string>();
            foreach (KeyValuePair<string, float> p in updatedProbabilities)
            {
                if (p.Value != 0)
                {
                    awakeProbabilities.Add(p.Key, p.Value);
                    awakeKeys.Add(p.Key);
                }
            }

            if (randomItemNumber >= 1 && randomItemNumber < awakeProbabilities[awakeKeys[0]])
            {
                currentItem = awakeKeys[0];
                /*UnityEngine.Console.Log("Condition 1");
                UnityEngine.Console.Log("Range 1 = " + "1 - " + awakeProbabilities[awakeKeys[0]]);*/
            }
            else if (randomItemNumber >= awakeProbabilities[awakeKeys[0]] && randomItemNumber < awakeProbabilities[awakeKeys[0]] + awakeProbabilities[awakeKeys[1]])
            {
                currentItem = awakeKeys[1];
                /*UnityEngine.Console.Log("Condition 2");
                UnityEngine.Console.Log("Range 2 = " + awakeProbabilities[awakeKeys[0]] + " - " + (awakeProbabilities[awakeKeys[0]] + awakeProbabilities[awakeKeys[1]]));*/
            }
            else if (randomItemNumber >= awakeProbabilities[awakeKeys[0]] + awakeProbabilities[awakeKeys[1]] && randomItemNumber <= 100)
            {
                currentItem = awakeKeys[2];
                /*UnityEngine.Console.Log("Condition 3");
                UnityEngine.Console.Log("Range 3 = " + (awakeProbabilities[awakeKeys[0]] + awakeProbabilities[awakeKeys[1]]) + " - 100");*/
            }
            else
            {
                UnityEngine.Console.LogError("Something Weird Happened!");
            }
            if (currentItem != null)
            {
                foreach (KeyValuePair<string, float> p in probabilities)
                {
                    states[p.Key] = State.Awake;
                    updatedProbabilities[p.Key] = probabilities[p.Key];
                }
                states[currentItem] = State.Dead;
                updatedProbabilities[currentItem] = 0f;
                UpdateProbabilities();
            }
            else
            {
                //UnityEngine.Console.Log("CurrentItem is null");
            }
        }

        // UnityEngine.Console.Log("Random Number Generated : " + randomItemNumber);

    //    UnityEngine.Console.Log($"MYstery Box Item IS {currentItem}");
        if (currentItem.Equals(coinKey))
        {
            amount = 250;

            inventoryObj.UpdateKeyValues(new List<InventoryItem<int>>() { new InventoryItem<int>(coinInventoryKey, amount, true) }, true, true, $"{coinKey}MysteryBox");


            inventoryObj.LastMysteryBoxReward = new KeyValuePair<string, int>(coinInventoryKey, amount);

        }
        else if (currentItem.Equals(diamondKey))
        {
            amount = 1;

            inventoryObj.UpdateKeyValues(new List<InventoryItem<int>>() { new InventoryItem<int>(diamondInventoryKey, amount, true) }, true, true, $"{diamondKey}MysteryBox");


            inventoryObj.LastMysteryBoxReward = new KeyValuePair<string, int>(diamondInventoryKey, amount);

        }
        else if (currentItem.Equals(boostKey))
        {
            amount = 3;

            inventoryObj.UpdateKeyValues(new List<InventoryItem<int>>() { new InventoryItem<int>(boostInventoryKey, amount, true) }, true, true, $"{boostKey}MysteryBox");


            inventoryObj.LastMysteryBoxReward = new KeyValuePair<string, int>(boostInventoryKey, amount);

            //saveManagerObj.MainSaveFile.boost += amount;
            //PlayerPrefs.SetInt(AnimatorParameters.boost, PlayerPrefs.GetInt(AnimatorParameters.boost) + amount);
 
        }
        else if (currentItem.Equals(HeadStartKey))
        {
            amount = 3;

            inventoryObj.UpdateKeyValues(new List<InventoryItem<int>>() { new InventoryItem<int>(headStartInventoryKey, amount, true) }, true, true, $"{HeadStartKey}MysteryBox");

            inventoryObj.LastMysteryBoxReward = new KeyValuePair<string, int>(headStartInventoryKey, amount);

        }
    }
    private void OnDisable()
    {

        mysteryBoxEvent.TheEvent.RemoveListener(mysteryBoxPanelClose);

    }
    //void Action(string item, int amount)
    //{
    //displayText.text = "Hurray!!! You Got Reward";
    //congratulationTxt.SetActive(true);
    //congratulationTxt.GetComponent<Text>().text = "<color=\"yellow\">Congratulations!!!</color> You have achive <color=\"orange\">" + amount + " " + item + "</color>";

    //boxImg.GetComponent<Image>().sprite = boxOpened;
    //boxImg.GetComponent<DOTweenAnimation>().DOPause();
    //openBtn.SetActive(false);
    //this.UI_Close();
    //}
}