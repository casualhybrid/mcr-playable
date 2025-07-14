using Knights.UISystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialFeedBackPanel : MonoBehaviour
{
    [SerializeField] private float lifeTime = 2.5f;
    public TextMeshProUGUI feedBackTxt;

    private void OnEnable()
    {
        StartCoroutine(WaitAndCloseThePanel());
    }

    private IEnumerator WaitAndCloseThePanel()
    {
        yield return new WaitForSecondsRealtime(lifeTime);
        this.gameObject.SetActive(false);
    }
}
