using System.Collections.Generic;
using UnityEngine;

public class RandomObjectEnabler : MonoBehaviour
{
    [SerializeField] private List<GameObject> objectsList;
    public bool isTutorial = false;
    private void OnEnable()
    {
        if (MATS_GameManager.instance.isTutorialPlaying)
        {
            for (int i = 0; i < objectsList.Count; i++)
            {
                objectsList[i].SetActive(false);
            }
            return;
        }
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
