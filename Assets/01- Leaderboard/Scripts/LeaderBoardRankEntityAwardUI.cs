using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderBoardRankEntityAwardUI : MonoBehaviour
{
    [SerializeField] private Image awardIcon;
    [SerializeField] private TextMeshProUGUI amountText;

    public void SetInformation(Sprite awardSprite, string awardAmountText)
    {
        awardIcon.sprite = awardSprite;
        amountText.text = awardAmountText;
    }

    
}
