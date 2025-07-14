using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableGameObjectAfterDelay : MonoBehaviour
{
    [SerializeField] private float delay;
    [SerializeField] private GameObject objectToEnable;
    [SerializeField] private bool disableOnDeactivate = true;

    private void OnEnable()
    {
        StartCoroutine(EnableAfterDelay());
    }

    private void OnDisable()
    {
        if(disableOnDeactivate)
        objectToEnable.SetActive(false);
    }

    private IEnumerator EnableAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        objectToEnable.SetActive(true);
    }

}
