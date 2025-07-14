using UnityEngine;
using DG.Tweening;

public class AnchorPanelAnim : MonoBehaviour
{
    public enum Position {
        None = 0, Left = 1, Right = 2, Top = 3, Bottom = 4,
    }

    [SerializeField] protected Position origin = Position.Left;
    [SerializeField] protected bool isOutAnimation;
    [SerializeField] protected Ease ease = Ease.Linear;

    private void OnEnable()
    {
        RectTransform rTransform = gameObject.transform as RectTransform;
        var origAnchoredPos = rTransform.anchoredPosition;
        Vector3 startPosition = Vector3.zero;

        switch (origin) {
            case Position.Left:
                startPosition = new Vector3(-rTransform.rect.width, 0.0f, 0.0f);
                break;
            case Position.Right:
                startPosition = new Vector3(rTransform.rect.width, 0.0f, 0.0f);
                break;
            case Position.Top:
                startPosition = new Vector3(0.0f, rTransform.rect.height, 0.0f);
                break;
            case Position.Bottom:
                startPosition = new Vector3(0.0f, -rTransform.rect.height, 0.0f);
                break;
        }

        rTransform.anchoredPosition = /*isOutAnimation ? Vector3.zero :*/ startPosition;
        rTransform.DOKill();

        rTransform.DOAnchorPos(/*isOutAnimation ? startPosition :*/ Vector3.zero, 0.25f, true)
        .SetEase(ease).OnComplete(() => {
            rTransform.anchoredPosition = origAnchoredPos;
        }).SetUpdate(true);
    }
}
