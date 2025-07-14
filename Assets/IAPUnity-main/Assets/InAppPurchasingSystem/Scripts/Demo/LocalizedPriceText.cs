using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using TheKnights.Purchasing;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedPriceText : MonoBehaviour
{
    [SerializeField] private IAPManager iapManager;
    [SerializeField] private IAPItem iapItem;

    private TextMeshProUGUI priceText;

    private void Awake()
    {
        priceText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        string localizedPrice = iapManager.GetlocalizedPriceString(iapItem);

        if (localizedPrice == null)
            return;

        priceText.text = localizedPrice;
    }


}
