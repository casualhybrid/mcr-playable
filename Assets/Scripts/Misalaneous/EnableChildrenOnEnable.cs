using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableChildrenOnEnable : MonoBehaviour
{
    private void OnEnable()
    {
        foreach (Transform t in transform)
        {
            t.gameObject.SetActive(true);
        }
    }
}
