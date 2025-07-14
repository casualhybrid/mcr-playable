using System.Collections.Generic;
using UnityEngine;

public class intellichunk : MonoBehaviour
{
    public List<GameObject> TrafficCars;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            for (int i = 0; i < TrafficCars.Count; i++)
            {
              //  TrafficCars[i].GetComponent<Trafficar>().DoYourWork = true;
            }
        }
    }
}