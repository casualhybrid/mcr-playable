using deVoid.UIFramework;
using Knights.UISystem;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Analytics;
using UnityEngine;

using UnityEngine.UI;

public class GamePlayMysteryBoxOpenPanel : AWindowController
{
    public GameObject claimPanel;
    public GameObject openRewardPanel;
    public GameObject GenericRewardHolder;

    public GameObject mysterBox, mysteryBoxLid;

    public List<Sprite> rewardSprites;
    public List<string> RewardKeys;

    public Button openBoxBtn;
    public Button closeBtn, backBtn;

    //-----------------------------------------------------------------------------------------------------------

    private enum inventoryType
    {
        reward,
        gamePlay,
        player
    };

    [SerializeField] private InventorySystem inventoryObj;
    [SerializeField] private GamePlaySessionInventory gameplayInventoryObj;
    [SerializeField] private inventoryType typeOfInventory;
    [SerializeField] private GameEvent mysteryBoxEvent;
    [SerializeField] private Animator animator;
    [SerializeField] private Image rewardImg;
    [SerializeField] private TextMeshProUGUI rewardTxt, noOfBoxLeftTxt;
    //private AnimatorClipInfo[] animationClips;
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

    private List<InventoryItem<int>> pendingItemsToReward;

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
        //--general
        openRewardPanel.SetActive(false);
        UpdateUI();
        updateObxNumTxt();
        //mysteryBoxEvent.TheEvent.AddListener(mysteryBoxPanelClose);
    }

    private void OnDisable()
    {
        UIScreenEvents.OnScreenOperationEventAfterAnimation.RemoveListener(FinishClaimingPartIfInventoryPanelClosed);
        //mysteryBoxEvent.TheEvent.RemoveListener(mysteryBoxPanelClose);
        claimPanel.SetActive(true);
    }

    private void Start()
    {//----RewardPanel Code
     //animationClips = animator.GetCurrentAnimatorClipInfo(0);

        backBtn.gameObject.SetActive(true);

        openBoxBtn.onClick.AddListener(delegate ()
        {
            Debug.LogError("Click");
            openRewardPanel.SetActive(true);
            claimPanel.SetActive(false);
            backBtn.gameObject.SetActive(false);
            OpenMysteryBox();
            animator.SetBool("AwardClaimed", false);
        });

        closeBtn.onClick.AddListener(delegate ()
        {
            PersistentAudioPlayer.Instance.gameplayAudio.volume = .1f;
            UIScreenEvents.OnScreenOperationEventAfterAnimation.AddListener(FinishClaimingPartIfInventoryPanelClosed);
            closeBtn.gameObject.SetActive(false);
            RewardPendingItems();
        });
    }

    private void FinishClaimingPartIfInventoryPanelClosed(string panel, ScreenOperation operation, ScreenType type)
    {
        if (operation == ScreenOperation.Open)
            return;

        if (panel != ScreenIds.InventoryCelebrationPanel)
            return;

        UIScreenEvents.OnScreenOperationEventAfterAnimation.RemoveListener(FinishClaimingPartIfInventoryPanelClosed);
        FinishClaimPart();
    }

    private void FinishClaimPart()
    {
        StartCoroutine(WaitAndExecute(() =>
        {
            openRewardPanel.SetActive(false);
            GenericRewardHolder.SetActive(false);
            animator.SetBool("AwardClaimed", true);
            animator.SetBool("BoxOpened", false);
            backBtn.interactable = true;
            mysteryBoxPanelClose();
            PersistentAudioPlayer.Instance.gameplayAudio.volume = .5f;

        }));
    }

    private IEnumerator WaitAndExecute(Action action)
    {
        yield return null;
        action();
    }

    public void SendEvents(string eventName)
    {
        AnalyticsManager.CustomData(eventName);
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

    private void popReward(Sprite spr, string str, int num)
    {
        GenericRewardHolder.SetActive(true);
        rewardImg.sprite = spr;
        rewardTxt.text = num.ToString() + " " + str;
        animator.SetTrigger("Reward");
        closeBtn.interactable = true;
    }

    public void OpenMysteryBox()
    {
      
        closeBtn.interactable = false;
        backBtn.interactable = false;
        animator.SetBool("BoxOpened", true);
        StartCoroutine(waitForAnimtionToEnd());
    }

    private IEnumerator waitForAnimtionToEnd()
    {
        yield return new WaitForSeconds(1.9f);

        mysterBox.SetActive(false);
        mysteryBoxLid.SetActive(false);

        MysteryBoxDetails();

        updateObxNumTxt();
    }

    private void updateObxNumTxt()
    {
        if (typeOfInventory == inventoryType.player)
        {
            noOfBoxLeftTxt.text = inventoryObj.GetIntKeyValue("GameMysteryBox").ToString();
        }
        else if (typeOfInventory == inventoryType.reward)
        {
            noOfBoxLeftTxt.text = PlayerPrefs.GetInt("AwardBox").ToString();
        }
        else if (typeOfInventory == inventoryType.gamePlay)
        {
            noOfBoxLeftTxt.text = gameplayInventoryObj.GetIntKeyValue("GameMysteryBox").ToString();
        }
    }

    private void mysteryBoxPanelClose(/*GameEvent gameEvent*/)
    {
        //claimPanel.SetActive(true);
        //mysterBox.SetActive(true);
        //mysteryBoxLid.SetActive(true);
        //CloseTheWindow(ScreenId);
        if (typeOfInventory == inventoryType.player)
        {
            if (inventoryObj.GetIntKeyValue("GameMysteryBox") == 0)
            {
                CloseTheWindow(ScreenId);
            }
            else
            {
                backBtn.gameObject.SetActive(true);
                closeBtn.gameObject.SetActive(true);
                claimPanel.SetActive(true);
                mysterBox.SetActive(true);
                mysteryBoxLid.SetActive(true);
            }
        }
        else if (typeOfInventory == inventoryType.reward)
        {
            if (PlayerPrefs.GetInt("AwardBox") == 0)
            {
                CloseTheWindow(ScreenId);
            }
            else
            {
                backBtn.gameObject.SetActive(true);
                closeBtn.gameObject.SetActive(true);
                claimPanel.SetActive(true);
                mysterBox.SetActive(true);
                mysteryBoxLid.SetActive(true);
            }
        }
        else if (typeOfInventory == inventoryType.gamePlay)
        {
            if (gameplayInventoryObj.GetIntKeyValue("GameMysteryBox") <= 0)
            {
                //  OpenTheWindow("GameOverPanel");
                CloseTheWindow(ScreenId);
            }
            else
            {
                backBtn.gameObject.SetActive(true);
                closeBtn.gameObject.SetActive(true);
                claimPanel.SetActive(true);
                mysterBox.SetActive(true);
                mysteryBoxLid.SetActive(true);
            }
        }
    }

    public void MysteryBoxDetails()
    {
        mysteryBoxEvent.RaiseEvent();
        float randomItemNumber = UnityEngine.Random.Range(1f, 100);
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
        else
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

            // inventoryObj.UpdateKeyValues(new List<InventoryItem<int>>() { new InventoryItem<int>(coinInventoryKey, amount, false/*true*/) });
            AddRewardToPending(new List<InventoryItem<int>>() { new InventoryItem<int>(coinInventoryKey, amount, true) });

            inventoryObj.LastMysteryBoxReward = new KeyValuePair<string, int>(coinInventoryKey, amount);
            popReward(rewardSprites[RewardKeys.IndexOf(coinKey)], coinKey, amount);
        }
        else if (currentItem.Equals(diamondKey))
        {
            amount = 1;

            // inventoryObj.UpdateKeyValues(new List<InventoryItem<int>>() { new InventoryItem<int>(diamondInventoryKey, amount, false/*true*/) });
            AddRewardToPending(new List<InventoryItem<int>>() { new InventoryItem<int>(diamondInventoryKey, amount, true) });

            popReward(rewardSprites[RewardKeys.IndexOf(diamondKey)], diamondKey, amount);

            inventoryObj.LastMysteryBoxReward = new KeyValuePair<string, int>(diamondInventoryKey, amount);
        }
        else if (currentItem.Equals(boostKey))
        {
            amount = 3;

            //   inventoryObj.UpdateKeyValues(new List<InventoryItem<int>>() { new InventoryItem<int>(boostInventoryKey, amount, false/*true*/) });
            AddRewardToPending(new List<InventoryItem<int>>() { new InventoryItem<int>(boostInventoryKey, amount, true) });

            inventoryObj.LastMysteryBoxReward = new KeyValuePair<string, int>(boostInventoryKey, amount);
            popReward(rewardSprites[RewardKeys.IndexOf(boostKey)], boostKey, amount);
            //saveManagerObj.MainSaveFile.boost += amount;
            //PlayerPrefs.SetInt(AnimatorParameters.boost, PlayerPrefs.GetInt(AnimatorParameters.boost) + amount);
        }
        else if (currentItem.Equals(HeadStartKey))
        {
            amount = 3;

            // inventoryObj.UpdateKeyValues(new List<InventoryItem<int>>() { new InventoryItem<int>(headStartInventoryKey, amount, false/*true*/) });
            AddRewardToPending(new List<InventoryItem<int>>() { new InventoryItem<int>(headStartInventoryKey, amount, true) });

            popReward(rewardSprites[RewardKeys.IndexOf(HeadStartKey)], HeadStartKey, amount);
            inventoryObj.LastMysteryBoxReward = new KeyValuePair<string, int>(headStartInventoryKey, amount);
        }
    }

    private void AddRewardToPending(List<InventoryItem<int>> pendingItems)
    {
        pendingItemsToReward = null;
        pendingItemsToReward = pendingItems;
    }

    private void RewardPendingItems()
    {
        if (pendingItemsToReward == null)
        {
            UnityEngine.Console.LogWarning("Requested to reward items but pending rewards are null");
            return;
        }

        if (typeOfInventory == inventoryType.player)
        {
            if (inventoryObj.GetIntKeyValue("GameMysteryBox") > 0)
            {
                pendingItemsToReward.AddRange(new List<InventoryItem<int>>() { new InventoryItem<int>("GameMysteryBox", -1) });

                inventoryObj.UpdateKeyValues(pendingItemsToReward, true, true, $"{inventoryObj.LastMysteryBoxReward.Key}MysteryBox" );
            }
        }
        else if (typeOfInventory == inventoryType.reward)
        {
            if (PlayerPrefs.GetInt("AwardBox") > 0)
            {
                PlayerPrefs.SetInt("AwardBox", PlayerPrefs.GetInt("AwardBox") - 1);
                inventoryObj.UpdateKeyValues(pendingItemsToReward, true, true, $"{inventoryObj.LastMysteryBoxReward.Key}MysteryBox");
            }
        }
        else if (typeOfInventory == inventoryType.gamePlay)
        {
            if (gameplayInventoryObj.GetIntKeyValue("GameMysteryBox") > 0)
            {
                gameplayInventoryObj.AddThisKeyToGamePlayInventory("GameMysteryBox", -1);
                pendingItemsToReward.AddRange(new List<InventoryItem<int>>() { new InventoryItem<int>("GameMysteryBox", -1) });
                inventoryObj.UpdateKeyValues(pendingItemsToReward, true, true, $"{inventoryObj.LastMysteryBoxReward.Key}MysteryBox");
            }
        }

        pendingItemsToReward = null;

        updateObxNumTxt();
    }
}