using System.Collections.Generic;
using UnityEngine;

public class UIEffectsHandler : MonoBehaviour
{
    [SerializeField] private UIEffectsChannel uIEffectsChannel;
    [SerializeField] private ItemAddEffect itemAddEffect;
    [SerializeField] private Camera cam;
    [SerializeField] private InventoryUpdateEvent updateCelebrationEvent;

    private readonly List<ItemAddEffect> itemAddEffectsPooled = new List<ItemAddEffect>();

    private void Start()
    {
        uIEffectsChannel.OnItemAddEffectRequest += UIEffectsChannelOnItemAddEffectRequest;
        uIEffectsChannel.OnRequestToShowInventoryAddEffect += ShowItemAddedEffect;
        updateCelebrationEvent.celebrationEvent += ShowItemAddedEffect;
    }

    private void OnDestroy()
    {
        uIEffectsChannel.OnItemAddEffectRequest -= UIEffectsChannelOnItemAddEffectRequest;
        uIEffectsChannel.OnRequestToShowInventoryAddEffect -= ShowItemAddedEffect;
        updateCelebrationEvent.celebrationEvent -= ShowItemAddedEffect;
    }

    private void ShowItemAddedEffect(string itemName, int valueAdded)
    {
        Vector2 startingPos = new Vector2(Screen.width / 2f, Screen.height / 2f);
        InventoryItemAnimData animData = InventoryPositions.GetAnimDataForItem(itemName);

        if (!animData.isValid)
        {
            // throw new System.Exception($"Invalid item animation data for item named {itemName}");
            return;
        }

        Vector2 endingPos = animData.screenLocation;

        UIEffectsChannelOnItemAddEffectRequest(startingPos, endingPos, itemName);
    }

    private void UIEffectsChannelOnItemAddEffectRequest(Vector2 initialScreenPos, Vector2 targetScreenPos, string itemName, int noOfItems = -1, bool useHighGradientAlpha = false)
    {
        Vector2 startingLocalPoint, endingLocalPoint;
        ItemAddEffect addEffect;

        if (itemAddEffectsPooled.Count == 0)
        {
            addEffect = Instantiate(itemAddEffect, transform);
        }
        else
        {
            int lastIndex = itemAddEffectsPooled.Count - 1;
            addEffect = itemAddEffectsPooled[lastIndex];
            itemAddEffectsPooled.RemoveBySwap(itemAddEffectsPooled.Count - 1);
        }

        addEffect.gameObject.SetActive(true);
        addEffect.OnEffectHasEnded += SendAddEffectToThePool;

        RectTransform rt = addEffect.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, initialScreenPos, cam, out startingLocalPoint);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, targetScreenPos, cam, out endingLocalPoint);

      //  UnityEngine.Console.Log($"Starting Is {startingLocalPoint} and ending {endingLocalPoint}");

        addEffect.Initialzie(startingLocalPoint, endingLocalPoint, itemName, noOfItems, useHighGradientAlpha);
    }


    private void SendAddEffectToThePool(ItemAddEffect addEffect)
    {
        addEffect.OnEffectHasEnded -= SendAddEffectToThePool;
        itemAddEffectsPooled.Add(addEffect);
    }
}