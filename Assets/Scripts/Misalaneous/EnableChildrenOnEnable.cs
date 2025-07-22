using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableChildrenOnEnable : MonoBehaviour
{
    [SerializeField] List<GameObject> obstacles;
    private void OnEnable()
    {
        foreach (var obstacle in obstacles) {
            obstacle.SetActive(false);
        }
        obstacles[Random.Range(0, obstacles.Count)].SetActive(true);
        /*foreach (Transform t in transform)
        {
            t.gameObject.SetActive(true);
        }*/

    }
}
