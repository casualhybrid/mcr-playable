using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using deVoid.UIFramework;
using UnityEngine.UI;
using Knights.UISystem;
using TMPro;
//using Sirenix.OdinInspector.Editor;

[System.Serializable]
public class GamePlayPowerupRewardProperties : WindowProperties
{
    public InventoryItemMeta PowerUpMetaData { get; set; }
}

public class GamePlayRewardedPowerUpScreen : AWindowController<GamePlayPowerupRewardProperties>
{
    [SerializeField] private Image powerupImage;
    [SerializeField] private RewardedPowerUpInGameADSO powerupRewardedSO;

   

   
   

    protected override void OnPropertiesSet()
    {
        base.OnPropertiesSet();

        Sprite sprite = Properties.PowerUpMetaData.Sprite;
        powerupImage.sprite = sprite;
        powerupRewardedSO.AddThisItemToMetaDisplayData(new RewardedADRewardMetaData(sprite,0));
       // Timer();
    }

    public void WatchPowerupRewarded()
    {
        powerupRewardedSO.ShowRewardedAD();
    }

    public void CloseWindow()
    {
        UI_Close();
    }
   /* public void Timer()
    {
        Debug.Log("TimerStart");
        StartCoroutine(StartCountdown());
    }*/
   /* private IEnumerator StartCountdown()
    {
        float timer = 10f;

        while (timer > 0)
        {
            timeText.text = Mathf.CeilToInt(timer).ToString();
            timer -= Time.deltaTime;
            yield return null;
        }
        CloseWindow();





    }*/
}
