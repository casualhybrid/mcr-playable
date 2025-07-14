using UnityEngine;
using UnityEngine.UI;

public class EnableButtonAfterDelay : MonoBehaviour
{
    public Button targetButton; // Jo button 3.5s baad enable hoga
    public float delay = 3.5f;

    void Start()
    {
        if (targetButton != null)
        {
            targetButton.interactable = false; // Start mein non-interactable
            Invoke("EnableButton", delay);     // 3.5s baad enable karna
        }
        else
        {
            Debug.LogWarning("Button reference is missing!");
        }
    }

    void EnableButton()
    {
        targetButton.interactable = true;
        targetButton.transform.GetChild(0).gameObject.SetActive(true);
    }
}
