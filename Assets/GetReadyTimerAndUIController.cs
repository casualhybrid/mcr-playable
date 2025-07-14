using deVoid.UIFramework;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class GetReadyTimerAndUIController : AWindowController
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private TMP_Text counterTxt;
    [SerializeField] private UnityEvent onCountDownBegin;

    private void OnEnable()
    {
        gameManager.PauseTheGame();
        CountDown();
    }

    private void CountDown()
    {
        counterTxt.text = "3";
        counterTxt.transform.localEulerAngles = Vector3.zero;

        counterTxt.transform.DOLocalRotate(new Vector3(0, 0, 360), 0.5f, RotateMode.FastBeyond360).SetDelay(0.5f).SetUpdate(true).OnComplete(() =>
        {
            counterTxt.text = "2";
            counterTxt.transform.localEulerAngles = Vector3.zero;

            counterTxt.transform.DORotate(new Vector3(0, 0, 360), 0.5f, RotateMode.FastBeyond360).SetDelay(0.5f).SetUpdate(true).OnComplete(() =>
            {
                counterTxt.text = "1";
                counterTxt.transform.localEulerAngles = Vector3.zero;

                counterTxt.transform.DORotate(new Vector3(0, 0, 360), 0.5f, RotateMode.FastBeyond360).SetDelay(0.5f).SetUpdate(true).OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    gameManager.UnPauseTheGame();
                    UI_Close();
                });
            });
        });

        onCountDownBegin.Invoke();
    }
}