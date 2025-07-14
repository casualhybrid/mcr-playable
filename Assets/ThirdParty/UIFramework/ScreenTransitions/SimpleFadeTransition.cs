using System;
using UnityEngine;
using DG.Tweening;

namespace deVoid.UIFramework
{
    /// <summary>
    /// This is a simple fade transition implemented as a built-in example.
    /// I recommend using a free tweening library like DOTween (http://dotween.demigiant.com/)
    /// or rolling out your own.
    /// Check the Examples project for more robust and battle-tested options:
    /// https://github.com/yankooliveira/uiframework_examples
    /// </summary>
    public class SimpleFadeTransition : ATransitionComponent
    {
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private bool fadeOut = false;

        private CanvasGroup canvasGroup;
        private float timer;
        private Action currentAction;
        private Transform currentTarget;

        private float startValue;
        private float endValue;

        private bool shouldAnimate;

        public override void Animate(Transform target, Action callWhenFinished) {
            RectTransform rTransform = target as RectTransform;
            if (currentAction != null) {
                canvasGroup.alpha = endValue;
                currentAction();
            }

            canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null) {
                canvasGroup = target.gameObject.AddComponent<CanvasGroup>();
            }

            //if (fadeOut) {
            //    startValue = 1f;
            //    endValue = 0f;
            //}
            //else {
            //    startValue = 0f;
            //    endValue = 1f;
            //}

            //currentAction = callWhenFinished;
            //timer = fadeDuration;

            //canvasGroup.alpha = startValue;
            //shouldAnimate = true;
            canvasGroup.DOFade(fadeOut ? 0f : 1f, fadeDuration).
                From(fadeOut ? 1f : 0f).SetEase(Ease.OutQuad).
                OnComplete(() => {
                    Cleanup(callWhenFinished, rTransform, canvasGroup);
                }).SetUpdate(true);// * fadeDurationPercent);
        }

        //private void Update() {
        //    if (!shouldAnimate) {
        //        return;
        //    }

        //    if (timer > 0f) {
        //        timer -= Time.deltaTime;
        //        canvasGroup.alpha = Mathf.Lerp(endValue, startValue, timer / fadeDuration);
        //    }
        //    else {
        //        canvasGroup.alpha = 1f;
        //        if (currentAction != null) {
        //            currentAction();
        //        }

        //        currentAction = null;
        //        shouldAnimate = false;
        //    }
        //}

        private void Cleanup(Action callWhenFinished, RectTransform rTransform, CanvasGroup canvasGroup)
        {
            callWhenFinished();
            rTransform.localScale = Vector3.one;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }
    }
}