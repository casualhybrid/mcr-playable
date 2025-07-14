using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetInventoryPositions : MonoBehaviour
{
    private Camera renderingCamera;

    [SerializeField] private RectTransform inventoryCoinsRT;
    [SerializeField] private RectTransform inventoryDiamondsRT;
    [SerializeField] private RectTransform inventoryBoostsRT;

    [SerializeField] private DOTweenAnimation[] inventoryCoinsAnimations;
    [SerializeField] private DOTweenAnimation[] inventoryDiamondAnimations;
    [SerializeField] private DOTweenAnimation[] inventoryBoostsAnimations;

    [SerializeField] private Canvas inventoryCanvas;
    [SerializeField] private UIEffectsChannel uIEffectsChannel;

    [SerializeField] private List<Image> plusIcons;

    private int defaultInventroyCanvasOrder;
    private GraphicRaycaster graphicRaycaster;

    private void Awake()
    {
        defaultInventroyCanvasOrder = inventoryCanvas.sortingOrder;
        graphicRaycaster = inventoryCanvas.GetComponent<GraphicRaycaster>();
    }

    private void OnEnable()
    {
        inventoryCanvas.overrideSorting = false;
        uIEffectsChannel.OnRequireInventoryPanel += HandleInventoryPanelRequest;

        SetCameraReference();

        if (renderingCamera == null)
            return;

        if (inventoryCoinsAnimations != null)
            InventoryPositions.UpdateInventoryLocation(new InventoryItemAnimData(renderingCamera.WorldToScreenPoint(inventoryCoinsRT.position), inventoryCoinsAnimations), "AccountCoins");

        if (inventoryDiamondsRT != null)
            InventoryPositions.UpdateInventoryLocation(new InventoryItemAnimData(renderingCamera.WorldToScreenPoint(inventoryDiamondsRT.position), inventoryDiamondAnimations), "AccountDiamonds");

        if (inventoryBoostsRT != null)
            InventoryPositions.UpdateInventoryLocation(new InventoryItemAnimData(renderingCamera.WorldToScreenPoint(inventoryBoostsRT.position), inventoryBoostsAnimations), "GameBoost");
    }

    private void OnDisable()
    {
        uIEffectsChannel.OnRequireInventoryPanel -= HandleInventoryPanelRequest;
        HandleInventoryPanelRequest(false);
    }

    private void HandleInventoryPanelRequest(bool status)
    {
        if (status)
        {
            inventoryCanvas.overrideSorting = true;
            inventoryCanvas.sortingOrder = 1;

            ChangeClickableStateOfInventory(false);
        }
        else
        {
            inventoryCanvas.overrideSorting = false;
            inventoryCanvas.sortingOrder = defaultInventroyCanvasOrder;

            ChangeClickableStateOfInventory(true);
        }
    }

    private void ChangeClickableStateOfInventory(bool status)
    {
        if (graphicRaycaster != null)
        {
            graphicRaycaster.enabled = status;
        }

        foreach (var icon in plusIcons)
        {
            if(icon != null)
            icon.enabled = status;
        }
    }

    private void SetCameraReference()
    {
        if (renderingCamera != null)
            return;

        Canvas canvas = transform.root.GetComponent<Canvas>();

        if (canvas == null)
        {
            throw new System.Exception("Failed to find canvas in parent while setting inventory positions");
        }

        renderingCamera = canvas.worldCamera;
    }
}