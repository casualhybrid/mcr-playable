using UnityEngine;
using deVoid.UIFramework;
using TMPro;
using Knights.UISystem;
using System.Collections;

[System.Serializable]
public class InformationPanelProperties : PanelProperties
{
    public string TitleText { get; set; }
    public string InformationText { get; set; }
}

public class InformationPanelScreenUI : APanelController<InformationPanelProperties>
{
    [SerializeField] private TextMeshProUGUI informationText;
    [SerializeField] private TextMeshProUGUI TitleTxt;

    protected override void OnPropertiesSet()
    {
        base.OnPropertiesSet();
        informationText.text = Properties.InformationText;
        TitleTxt.text = Properties.TitleText;
    }

    private void OnEnable()
    {
        StartCoroutine(WaitAndCloseThePanel());
    }

    private IEnumerator WaitAndCloseThePanel()
    {
        yield return new WaitForSecondsRealtime(3f);
        CloseThePanel(ScreenIds.InformationPanelScreen);
    }
}
