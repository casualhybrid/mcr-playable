using TMPro;
using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;

public class RewardPanel : AWindowController
{
    public GameObject claimPanel;
    public GameObject openRewardPanel;

    //  public Sprite[] rewardSprites;
    //   public Image rewardSpriteImg;

    public Button openBoxBtn;
    public Button closeBtn;

    public GameObject coinsObj;
    public GameObject boostObj;
    public GameObject diamondObj;
    public GameObject headStartObj;
    public GameObject mysteryBoxObj;

    private void OnEnable()
    {
        openRewardPanel.SetActive(false);
        SetRewardPanel();
    }

    private void Start()
    {
        openBoxBtn.onClick.AddListener(delegate () {
            openRewardPanel.SetActive(true);
            claimPanel.SetActive(false);
        });

        closeBtn.onClick.AddListener(delegate () {
            openRewardPanel.SetActive(false);
            claimPanel.SetActive(false);
            // gameObject.SetActive(false);
           // LeaderboardRewardManager.Instance.ClosedRewardPanel();
        });
    }

    public void SetRewardPanel()
    {

        //var rewards = LeaderboardRewardManager.Instance.totalRewards;

        //if (rewards.TryGetValue(Constants.Coins, out int temp))
        //    SetObjectsValue(coinsObj, temp);

        //if (rewards.TryGetValue(Constants.Boost, out temp))
        //    SetObjectsValue(boostObj, temp);

        //if (rewards.TryGetValue(Constants.MysteryBox, out temp))
        //    SetObjectsValue(mysteryBoxObj, temp);

        //if (rewards.TryGetValue(Constants.HeadStart, out temp))
        //    SetObjectsValue(headStartObj, temp);

        //if (rewards.TryGetValue(Constants.Diamond, out temp))
        //    SetObjectsValue(diamondObj, temp);

        claimPanel.SetActive(true);
    }

    void SetObjectsValue(GameObject obj, int value)
    {
        if (value != 0)
        {
            obj.transform.Find("Lbl").GetComponent<TMP_Text>().text = "x" + value;
            obj.SetActive(true);
        }
    }
}