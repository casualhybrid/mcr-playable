/// Credit Feaver1968 
/// Sourced from - http://forum.unity3d.com/threads/scroll-to-the-bottom-of-a-scrollrect-in-code.310919/

namespace UnityEngine.UI.Extensions
{
    public static class ScrollRectExtensions
    {
        public static void ScrollToTop(this ScrollRect scrollRect)
        {
            scrollRect.normalizedPosition = new Vector2(0, 1);
        }
        public static void ScrollToBottom(this ScrollRect scrollRect)
        {
            scrollRect.normalizedPosition = new Vector2(0, 0);
        }

        public static void SnapScrollToRect(this ScrollRect scrollRect, RectTransform targetRect, bool keepYIntact = false, bool keepXIntact = false)
        {

            Vector2 targetScrollPos = scrollRect.transform.InverseTransformPoint(scrollRect.content.position)
                - scrollRect.transform.InverseTransformPoint(targetRect.transform.position);

            if(keepYIntact)
            targetScrollPos.y = scrollRect.content.anchoredPosition.y;

            if (keepXIntact)
                targetScrollPos.x = scrollRect.content.anchoredPosition.x;

            scrollRect.content.anchoredPosition = targetScrollPos;

        }

        public static Vector2 GetSnapToPositionToBringChildIntoView(this ScrollRect instance, RectTransform child)
        {
            Canvas.ForceUpdateCanvases();
            Vector2 viewportLocalPosition = instance.viewport.localPosition;
            Vector2 childLocalPosition = child.localPosition;
            Vector2 result = new Vector2(
                0 - (viewportLocalPosition.x + childLocalPosition.x),
                0 - (viewportLocalPosition.y + childLocalPosition.y)
            );
            return result;
        }


        public static Vector2 GetSnappedValueScrollToRect(this ScrollRect scrollRect, Transform targetRect, bool keepYIntact = false, bool keepXIntact = false)
        {

            Vector2 targetScrollPos = scrollRect.transform.InverseTransformPoint(scrollRect.content.position)
                - scrollRect.transform.InverseTransformPoint(targetRect.position);

            if (keepYIntact)
                targetScrollPos.y = scrollRect.content.anchoredPosition.y;

            if (keepXIntact)
                targetScrollPos.x = scrollRect.content.anchoredPosition.x;

            return targetScrollPos;

        }

    }
}