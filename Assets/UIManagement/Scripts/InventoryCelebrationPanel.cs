using deVoid.UIFramework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[System.Serializable]
public class CelebrationBatchProperties : WindowProperties
{
    [System.NonSerialized] public List<InventoryItem<int>> BatchToBeCelebrated;
    [System.NonSerialized] public string Context;
    [System.NonSerialized] public bool isDoubleRewardPossible;
}

[Preserve]
public class ACelebrationWindow : AWindowController<CelebrationBatchProperties>
{
    [Preserve]
    public ACelebrationWindow()
    {
    }
}

public class InventoryCelebrationPanel : ACelebrationWindow
{
    [SerializeField] GameObject crossButton;
    [SerializeField] private InventorySystem inventorySystem;

    [SerializeField] private GameObject doubleButton;
    [SerializeField] private GameEvent doubleRewardAdComplete;
    [SerializeField] private List<Sprite> rewardSprites;
    [SerializeField] private List<string> RewardKeys;
    [SerializeField] private List<Color> colors;
    [SerializeField] private InventoryCelebrationItemInstanceUI instance;
    [SerializeField] private Transform instancesParentT;

    private readonly Stack<InventoryCelebrationItemInstanceUI> availableCelebrationInventoryInstances = new Stack<InventoryCelebrationItemInstanceUI>();
    private bool isDoubled;
    public static bool isShop;
    private void OnEnable()
    {
        if (Properties.isDoubleRewardPossible)
        {
            doubleRewardAdComplete.TheEvent.AddListener(AddDoubleReward);

            //doubleButton.SetActive(true);
            Debug.LogError("CloseCall?");
        }
        else
        {

            doubleButton.SetActive(false);
            if (crossButton)
                crossButton.SetActive(false);
            Debug.LogError("CloseCall?");
            UI_Close();
        }
        if (MATS_GameManager.instance)
            MATS_GameManager.instance.activeInventoryCelebrationPanel = this;

        if (isShop)
            UI_Close();
    }
    public void Close()
    {
        crossButton.SetActive(false);
    }

    private void OnDisable()
    {
        isDoubled = false;
        doubleRewardAdComplete.TheEvent.RemoveListener(AddDoubleReward);
        doubleButton.SetActive(false);
        if (crossButton != null)
            crossButton.SetActive(false);
    }

    protected override void OnPropertiesSet()
    {
        if (this.gameObject.activeInHierarchy)
            return;

        base.OnPropertiesSet();

        if (Properties == null || Properties.BatchToBeCelebrated == null)
            return;

        foreach (var item in Properties.BatchToBeCelebrated)
        {
            ShowCelebrationDetails(item.itemName, item.itemValue);
        }
    }

    private void ShowCelebrationDetails(string itemName, int itemValue)
    {
        InventoryCelebrationItemInstanceUI entity;

        if (availableCelebrationInventoryInstances.Count > 0)
        {
            entity = availableCelebrationInventoryInstances.Pop();
        }
        else
        {
            entity = Instantiate(instance, instancesParentT);
        }

        entity.gameObject.SetActive(true);

        if (!isDoubled)
        {
            entity.AmountText.text = itemValue.ToString();
        }
        else
        {
            entity.AmountText.text = (itemValue * 2).ToString();
        }

        entity.RewardImage.sprite = rewardSprites[RewardKeys.IndexOf(itemName)];
        Debug.LogError(itemName + " mohsin " + entity.AmountText.text);
        entity.BackgroundImage.color = colors[RewardKeys.IndexOf(itemName)];
    }

    private void AddDoubleReward(GameEvent gameEvent)
    {
        if (Properties.Context != null)
        {
            UnityEngine.Console.Log("DOUBLE " + Properties.Context);
            AnalyticsManager.CustomData("Rewarded_2X", new Dictionary<string, object> { { "DoubleRewardType", Properties.Context } });
            Firebase.Analytics.FirebaseAnalytics.LogEvent("Rewarded_2X", "DoubleRewardType", Properties.Context);

        }

        foreach (var item in Properties.BatchToBeCelebrated)
        {
            item.isCelebrationAddition = false;
        }

        inventorySystem.UpdateKeyValues(Properties.BatchToBeCelebrated, true, true);
        isDoubled = true;
        DoubleTheRewardsAmountText();
    }

    private void DoubleTheRewardsAmountText()
    {
        foreach (var item in instancesParentT.GetComponentsInChildren<InventoryCelebrationItemInstanceUI>())
        {
            item.AmountText.text = (int.Parse(item.AmountText.text) * 2).ToString();
        }
    }

    protected override void WhileHiding()
    {
        base.WhileHiding();

        foreach (var item in instancesParentT.GetComponentsInChildren<InventoryCelebrationItemInstanceUI>())
        {
            item.gameObject.SetActive(false);
            availableCelebrationInventoryInstances.Push(item);
        }
    }


}