using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutSceneTrigger : MonoBehaviour
{
    [SerializeField] PlayerSharedData PlayerSharedData;
    [SerializeField] GameEvent cutSceneStumbled;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("chaser"))
        {
            PlayerSharedData.ChaserWasHit = true;
            cutSceneStumbled.RaiseEvent();

        }
    }
}
