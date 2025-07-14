using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequireInventoryPanel : MonoBehaviour
{
    [SerializeField] private UIEffectsChannel uIEffectsChannel;

    private void OnEnable()
    {
        uIEffectsChannel.RaiseOnRequireInventoryPanel(true);
    }

    private void OnDisable()
    {
        uIEffectsChannel.RaiseOnRequireInventoryPanel(false);
    }

    public void RaiseOnRequireInventoryPanelEvent(bool status)
    {
        uIEffectsChannel.RaiseOnRequireInventoryPanel(status);
    }
}
