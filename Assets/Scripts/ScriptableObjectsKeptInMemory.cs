using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptableObjectsKeptInMemory : MonoBehaviour
{
    [SerializeField] private ScriptableObject[] scriptableObjects;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

}
