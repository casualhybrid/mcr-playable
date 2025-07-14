using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using deVoid.UIFramework;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class CarAvailablePanelProperties : PanelProperties
{
    [HideInInspector] public PlayableObjectDataWithIndex CarConfigData;
}

public class CarAvailableInGameUI : APanelController<CarAvailablePanelProperties>
{
   // [SerializeField] private TextMeshProUGUI carNameText;
    [SerializeField] private Image carImage;
    [SerializeField] private TextMeshProUGUI carTxt;
    [SerializeField] private float closeAfterDuration = 3f;
 

    private void OnEnable()
    {
        StartCoroutine(CloseAfterDelay());
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSecondsRealtime(closeAfterDuration);
        CloseThePanel(ScreenId);
    }

    protected override void OnPropertiesSet()
    {
        base.OnPropertiesSet();
        var playableData = Properties.CarConfigData;

        var data = playableData.basicAssetInfo as CarConfigurationData;
     //   carNameText.text = data.GetName;
        carImage.sprite = data.GetCarSprite;
        carTxt.text = data.GetName;
    }
}
