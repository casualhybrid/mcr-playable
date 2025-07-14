using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterInstance : MonoBehaviour
{
    [SerializeField] Sprite /*oldSelectedBg,*/ NormalBg, selectedBg;
    [SerializeField] GameObject Locked, TickImage;
    [SerializeField] Image BackGround, effect;
    [SerializeField] TextMeshProUGUI characterNameText;



    public void Reset()
    {
        BackGround = GetComponent<Image>();
        Locked = transform.Find("LockedImage").gameObject;
        TickImage = transform.Find("SelectedTick").gameObject;
        characterNameText = transform.Find("NameBg").Find("CharNameTxt").GetComponent<TextMeshProUGUI>();
    }

    public void isLockedAndNotSelected()
    {
        Locked.SetActive(true);
        //effect.GetComponent<Image>().color = new Color(1, 1, 0, .35f);
        //effect.color = Color.yellow;
        BackGround.sprite = NormalBg;
        effect.GetComponent<Image>().color = new Color(0, 0, 1, .35f);
    }

    public void isLockedSelected()
    {
        Locked.SetActive(true);
        //effect.GetComponent<Image>().color = new Color(1, 1, 0, .35f);
        //effect.color = Color.yellow;
        BackGround.sprite = selectedBg;
        effect.GetComponent<Image>().color = new Color(1, 1, 0, .35f);
    }

    public void SetCharNameTxt(string str)
    {
        characterNameText.text = str;
    }

    public void CurrentlySelectedCharacterTick(bool temp)
    {
        TickImage.SetActive(temp);
    }
    public void isSelected()
    {
        BackGround.sprite = selectedBg;
        Locked.SetActive(false);
        //effect.GetComponent<Image>().color = new Color(1, 0, 1, .35f);
        effect.GetComponent<Image>().color = new Color(1, 1, 0, .35f);
        //effect.color = Color.magenta;
    }

    public void isNotSelected()
    {
        BackGround.sprite = NormalBg;
        Locked.SetActive(false);
        effect.GetComponent<Image>().color = new Color(0, 0, 1, .35f);
        //effect.color = Color.blue;
    }
}
