using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System;
using deVoid.UIFramework;

public class ShopPurchase : AWindowController
{
    public enum PurchaseStatus
    {
        success,
        fail
    }

    [SerializeField] private ShopPurchaseEvent purchaseMadeEvent;
    [SerializeField] private GameObject[] Images, Bundles;
    [SerializeField] private TextMeshProUGUI[] text;
    [SerializeField] private Image[] Icons, colorImages;
    [SerializeField] private PurchaseStatus status;
    [SerializeField] private List<Sprite> rewardSprites;
    [SerializeField] private List<string> RewardKeys;
    [SerializeField] private List<Color> colors;
    [SerializeField] private CarsDataBase carsDataBase;
    


    private void OnEnable()
    {   if (status == PurchaseStatus.fail)
        {
            generalFailure();
            purchaseMadeEvent.PurchaseEvent += ShowFailureDetails;
        }  
        else if (status == PurchaseStatus.success)
            purchaseMadeEvent.PurchaseEvent += ShowSuccessDetails;
    }

    
    void generalFailure()
    {
        Bundles[0].SetActive(false);
        Bundles[1].SetActive(false);
        Bundles[2].SetActive(true);
    }
    public void ShowFailureDetails(List<string> itemsBought, string itemSpent, int spentMoney, List<int> amountsGot)
    {

        if(itemSpent == "AccountCoins")
        {
            Bundles[0].SetActive(true);
            Bundles[1].SetActive(false);
            Bundles[2].SetActive(false);
        }
        else if(itemSpent == "AccountDiamonds")
        {
            Bundles[0].SetActive(false);
            Bundles[1].SetActive(true);
            Bundles[2].SetActive(false);
        }

        
        //for (int i = 0; i < itemsBought.Count; i++)
        //{
        //    UnityEngine.Console.LogError("FAILURE ShopPurchaseEvent is raised with bought item " + itemsBought[i] + " and spent item " + itemSpent + " spentMoney value " + spentMoney + " amount got value " + amountsGot[i]);
        //}
    }

    public void ShowSuccessDetails(List<string> itemsBought, string itemSpent, int spentMoney, List<int> amountsGot)
    {
        if(itemsBought.Count == 3)
        {
            //text.text = string.Format("Congratulations, You've successfully got <color=yellow>{0}</color> <color=yellow>{1}</color>, <color=yellow>{2}</color> <color=yellow>{3}</color> and <color=yellow>{4}</color> <color=yellow>{5}</color> !", amountsGot[0], itemsBought[0], amountsGot[1], itemsBought[1], amountsGot[2], itemsBought[2]);
            //text.text = string.Format("x{0}           x{1}             x{2}", amountsGot[0], amountsGot[1], amountsGot[2]);

            Images[0].SetActive(true);
            Images[1].SetActive(true);
            Images[2].SetActive(true);
            Icons[0].sprite = rewardSprites[RewardKeys.IndexOf(itemsBought[0])];
            Icons[1].sprite = rewardSprites[RewardKeys.IndexOf(itemsBought[1])];
            Icons[2].sprite = rewardSprites[RewardKeys.IndexOf(itemsBought[2])];
            text[0].text = amountsGot[0].ToString();
            text[1].text = amountsGot[1].ToString();
            text[2].text = amountsGot[2].ToString();
            colorImages[0].color = colors[RewardKeys.IndexOf(itemsBought[0])];
            colorImages[1].color = colors[RewardKeys.IndexOf(itemsBought[1])];
            colorImages[2].color = colors[RewardKeys.IndexOf(itemsBought[2])];
        }
        else if(itemsBought.Count == 2)
        {
            //text.text = string.Format("Congratulations, You've successfully got <color=yellow>{0}</color> <color=yellow>{1}</color> and <color=yellow>{2}</color> <color=yellow>{3}</color> !", amountsGot[0], itemsBought[0], amountsGot[1], itemsBought[1]);
            //text.text = string.Format("x{0}          x{1}", amountsGot[0], amountsGot[1]);
            Images[0].SetActive(true);
            Images[1].SetActive(true);
            Images[2].SetActive(false);
            Icons[0].sprite = rewardSprites[RewardKeys.IndexOf(itemsBought[0])];
            Icons[1].sprite = rewardSprites[RewardKeys.IndexOf(itemsBought[1])];
            text[0].text = amountsGot[0].ToString();
            text[1].text = amountsGot[1].ToString();
            colorImages[0].color = colors[RewardKeys.IndexOf(itemsBought[0])];
            colorImages[1].color = colors[RewardKeys.IndexOf(itemsBought[1])];
        }
        else if(itemsBought.Count == 1)
        {
            //text.text = string.Format("Congratulations, You've successfully got <color=yellow>{0}</color> <color=yellow>{1}</color> !", amountsGot[0], itemsBought[0]);
            //text.text = string.Format("x{0}", amountsGot[0]);
            Images[0].SetActive(true);
            Images[1].SetActive(false);
            Images[2].SetActive(false);

            string item = itemsBought[0];
            
            if(item.Contains("Car"))
            {
                var resultString = Regex.Match(item, @"\d+").Value;
                int index = Int32.Parse(resultString);

                text[0].text = carsDataBase.GetCarConfigurationData(index).GetName;
                Icons[0].sprite = carsDataBase.GetCarConfigurationData(index).GetCarSprite;
                return;
            }

            Icons[0].sprite = rewardSprites[RewardKeys.IndexOf(itemsBought[0])];
            text[0].text = amountsGot[0].ToString();
            colorImages[0].color = colors[RewardKeys.IndexOf(itemsBought[0])];
        }

        //for (int i = 0; i < itemsBought.Count; i++)
        //{
        //    UnityEngine.Console.LogError("SUCCESS ShopPurchaseEvent is raised with bought item " + itemsBought[i] + " and spent item " + itemSpent + " spentMoney value " + spentMoney + " amount got value " + amountsGot[i]);
        //}
    }

    



    private void OnDisable()
    {

        if (status == PurchaseStatus.fail)
            purchaseMadeEvent.PurchaseEvent -= ShowFailureDetails;
        else if (status == PurchaseStatus.success)
            purchaseMadeEvent.PurchaseEvent -= ShowSuccessDetails;
    }
}
