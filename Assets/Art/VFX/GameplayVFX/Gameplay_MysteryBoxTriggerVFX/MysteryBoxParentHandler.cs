using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MysteryBoxParentHandler : MonoBehaviour
{
    [Header("Temp")]
    [SerializeField] private GameObject _mysteryBoxParent;

    private void OnTriggerEnter(Collider other)
    {
        _mysteryBoxParent.transform.parent = other.transform;
    }
}
