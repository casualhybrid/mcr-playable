using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CarInstance : MonoBehaviour
{
    [SerializeField] Sprite /*oldSelectedBg, */NormalBg, SelectedBg;
    [SerializeField] GameObject Border, LockIcon, TickImage;
    [SerializeField] Image BackGround;
    [SerializeField] TextMeshProUGUI CarnameTxt;
    public GameObject alertIconGameObject;

    public void Reset()
    {
        BackGround = GetComponent<Image>();
        Border = transform.Find("Border").gameObject;
        TickImage = transform.Find("SelectedTick").gameObject;
        LockIcon = transform.Find("LockedImage").gameObject;
        CarnameTxt = transform.Find("CarNameTxt").gameObject.GetComponent<TextMeshProUGUI>();
    }
    public void isLockedAndNotSelected()
    {
        BackGround.sprite = NormalBg;
        Border.SetActive(false);
        LockIcon.SetActive(true);
    }

    public void isLockedAndSelected()
    {
        BackGround.sprite = SelectedBg;
        Border.SetActive(true);
        LockIcon.SetActive(true);
    }

    public void SetCarNameTxt(string str)
    {
        CarnameTxt.text = str;
    }
    public void CurrentlySelectedCarTick(bool temp)
    {
        TickImage.SetActive(temp);
    }

    public void isSelected()
    {
        BackGround.sprite = SelectedBg;
        Border.SetActive(true);
        LockIcon.SetActive(false);
    }

    public void isNotSelected()
    {
        BackGround.sprite = NormalBg;
        Border.SetActive(false);
        LockIcon.SetActive(false);
    }

}
