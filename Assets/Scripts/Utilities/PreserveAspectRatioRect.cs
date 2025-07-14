using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class PreserveAspectRatioRect : MonoBehaviour
{
   private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        Rect rect = rectTransform.rect;
        float max = Mathf.Max(rect.width, rect.height);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, max);
    }



}
