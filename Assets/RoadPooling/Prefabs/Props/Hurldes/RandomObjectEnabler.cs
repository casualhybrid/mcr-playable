using System.Collections.Generic;
using UnityEngine;

public class RandomObjectEnabler : MonoBehaviour
{
    [SerializeField] private List<GameObject> objectsList;

    private void OnEnable()
    {
        EnableRandomObject();
    }

    void EnableRandomObject()
    {
        if (objectsList == null || objectsList.Count == 0)
            return;

        // Disable all objects first
        foreach (GameObject obj in objectsList)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        // Pick a random one and enable it
        int randomIndex = Random.Range(0, objectsList.Count);
        if (objectsList[randomIndex] != null)
            objectsList[randomIndex].SetActive(true);
    }
}
