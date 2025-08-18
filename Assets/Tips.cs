using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tips : MonoBehaviour
{
    [SerializeField] List<string> tips;

    [SerializeField] TextMeshProUGUI tipText;
    // Start is called before the first frame update
    private void OnEnable()
    {
        int random = Random.Range(0, tips.Count);

        tipText.text=tips[random];
    }
}
