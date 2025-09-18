using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MATS_Spawner : MonoBehaviour
{
    [Header("Objects to Spawn")]
    public List<GameObject> objectsToSpawn;

    [Header("Spawn Points")]
    public List<Transform> spawnPoints;

    [Header("Parent for Spawned Objects")]
    public Transform spawnParent;

    [Header("Settings")]
    public float spawnDelay = 0.01f;

    private void OnEnable()
    {
        if (objectsToSpawn.Count > 0 && spawnPoints.Count > 0)
            StartCoroutine(SpawnRoutine());
        else
            Debug.LogWarning("RandomSpawner: No objects or spawn points assigned.");
    }

    private IEnumerator SpawnRoutine()
    {
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            int objIndex = Random.Range(0, objectsToSpawn.Count);

            Instantiate(
                objectsToSpawn[objIndex],
                spawnPoints[i].position,
                spawnPoints[i].rotation,
                spawnParent
            );

            yield return new WaitForSeconds(spawnDelay);
        }
    }
}