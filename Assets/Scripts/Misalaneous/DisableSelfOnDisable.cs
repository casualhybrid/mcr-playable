using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableSelfOnDisable : MonoBehaviour
{
    private void OnDisable()
    {
        this.gameObject.SetActive(false);
    }
}
