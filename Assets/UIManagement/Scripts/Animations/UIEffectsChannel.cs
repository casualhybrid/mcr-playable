using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "UIEffectsChannel", menuName = "ScriptableObjects/UI/Effects/UIEffectsChannel")]
public class UIEffectsChannel : ScriptableObject
{
    public event Action<bool> OnRequireInventoryPanel;

    public event Action<string, int > OnRequestToShowInventoryAddEffect;

    public event Action<Vector2, Vector2, string, int, bool> OnItemAddEffectRequest;

    public void RaiseItemAddEffectRequest(Vector2 initialScreenPos, Vector2 targetScreenPos, string itemName, int noOfItems = -1, bool useHighAlphaGradientTrail = false)
    {
        OnItemAddEffectRequest?.Invoke(initialScreenPos, targetScreenPos, itemName, noOfItems, useHighAlphaGradientTrail);
    }
    public void RaiseOnRequireInventoryPanel(bool status)
    {
        OnRequireInventoryPanel?.Invoke(status);
    }

    public void RaiseOnRequireInventoryPanel(string itemName, int itemAmount)
    {
        OnRequestToShowInventoryAddEffect?.Invoke(itemName, itemAmount);
    }
}
