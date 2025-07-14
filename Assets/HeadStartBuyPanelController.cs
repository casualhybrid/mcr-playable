using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using Knights.UISystem;

public class HeadStartBuyPanelController : MonoBehaviour
{
    [SerializeField] private GameEvent headStartEvent;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private InventorySystem playerInventory;

    [Header("UI Refrences"), Space(10)]
    [SerializeField] private Image backgroundImg;
    [SerializeField] private GameObject panelObject;
    [SerializeField] private TMP_Text headStartsCounterTxt;
    [SerializeField] private Button buyBtn;
    [SerializeField] private DeVoidWindowController deVoidWindowController;
    [SerializeField] private GamePlayHUDWindowController gamePlayHUDWindowController;

    private int totalHeadStarts;

    private void OnEnable()
    {
        EnablePanelAnimation();
        gameManager.PauseTheGame();

        totalHeadStarts = playerInventory.GetIntKeyValue("GameHeadStart");
        headStartsCounterTxt.text = totalHeadStarts.ToString();
    }

    public void BuyHeadStart()
    {
        if (playerInventory.GetIntKeyValue("AccountCoins") >= 100)
        {
            playerInventory.UpdateKeyValues(new List<InventoryItem<int>> { new InventoryItem<int>("AccountCoins", -100) });
            playerInventory.UpdateKeyValues(new List<InventoryItem<int>> { new InventoryItem<int>("GameHeadStart", 1) });

            totalHeadStarts++;

            headStartsCounterTxt.DOColor(Color.green, 0.25f).SetUpdate(true);
            headStartsCounterTxt.transform.DOScale(new Vector3(2, 2, 2), 0.25f).SetUpdate(true).SetEase(Ease.OutSine).OnComplete(() =>
            {
                headStartsCounterTxt.text = totalHeadStarts.ToString();
                headStartsCounterTxt.transform.DOScale(new Vector3(1, 1, 1), 0.25f).SetUpdate(true).SetEase(Ease.OutBounce);
            });
        }
        else
        {
            deVoidWindowController.OpenTheWindow(ScreenIds.ShopBundlePanel);
        }
    }

    public void ShowStorePanel()
    {

    }

    public void ClosePanel()
    {
        DisablePanelAnimation();
    }

    private void EnablePanelAnimation()
    {
        panelObject.transform.localScale = Vector3.zero;

        backgroundImg.enabled = true;
        backgroundImg.color = new Color(backgroundImg.color.r, backgroundImg.color.g, backgroundImg.color.b, 0);

        panelObject.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBounce).SetUpdate(true).OnComplete(() =>
        {
            backgroundImg.DOFade(0.75f, 0.25f).SetUpdate(true);
        });
     
    }

    private void DisablePanelAnimation(bool isRaiseEvent = false)
    {
        deVoidWindowController.OpenTheWindow(ScreenIds.GetReadyPanel);
        gamePlayHUDWindowController.HandleHeadStartButton();
        backgroundImg.enabled = false;
        panelObject.transform.DOScale(Vector3.zero, 0.35f).SetEase(Ease.InBounce).SetUpdate(true).OnComplete(() =>
        {
            if (isRaiseEvent)
            {
                headStartEvent?.RaiseEvent();
            }
            panelObject.SetActive(false);
        });
    }
}
