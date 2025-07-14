using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using deVoid.UIFramework;
using UnityEngine.UI;
using Knights.UISystem;

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
    }

    public void WatchPowerupRewarded()
    {
        powerupRewardedSO.ShowRewardedAD();
    }

    public void CloseWindow()
    {
        UI_Close();
    }
}
