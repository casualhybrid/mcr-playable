using System.Collections;
using System.Collections.Generic;
using TheKnights.SaveFileSystem;
using UnityEngine;

public class DisableIfBoughtAds : MonoBehaviour
{
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private GameEvent onAdsPurchased;

    private void Awake()
    {
        onAdsPurchased.TheEvent.AddListener(RemoveADSIfBought);
    }

    private void OnDestroy()
    {
        onAdsPurchased.TheEvent.RemoveListener(RemoveADSIfBought);
    }


    private void OnEnable()
    {
        RemoveADSIfBought();
    }

    private void RemoveADSIfBought(GameEvent gameEvent = null)
    {
        if (saveManager.MainSaveFile.isAdsPurchased)
        {
            this.gameObject.SetActive(false);
        }
    }
}
