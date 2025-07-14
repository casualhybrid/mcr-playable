using deVoid.UIFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class TweakAnchoredPosBasedOnSafeArea : MonoBehaviour
{
    private RectTransform rectTransform;
    private Canvas canvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        LowerTheMainPanelTakingSafeAreaInAccount();
    }

    private void LowerTheMainPanelTakingSafeAreaInAccount()
    {
        Rect safeAreaRect = Screen.safeArea;

        int totalHeight = Display.main.renderingHeight;
        int safeAreaHeight = (int)safeAreaRect.height;

        if (totalHeight == safeAreaHeight)
            return;

        float topFactor = (totalHeight - (safeAreaRect.position.y + safeAreaRect.height)) / canvas.scaleFactor;
        Vector2 pos = rectTransform.anchoredPosition;
        pos.y -= topFactor;
        rectTransform.anchoredPosition = pos;

        //rectTransform.SetBottom(safeAreaRect.position.y / parentCanvas.scaleFactor);
    }
}
