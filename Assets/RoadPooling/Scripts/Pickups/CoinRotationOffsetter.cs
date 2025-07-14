using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinRotationOffsetter : MonoBehaviour
{
    private const float spawnedCoinYRotationOffset = 25f;
    private static float spawnedCoinYRotation;
    
    private void OnEnable()
    {
        gameObject.transform.localEulerAngles = new Vector3(0, spawnedCoinYRotation, 0);

        spawnedCoinYRotation -= spawnedCoinYRotationOffset;
        spawnedCoinYRotation = spawnedCoinYRotation % 360;
    }
}
