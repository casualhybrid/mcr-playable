using UnityEngine;

public class ActivateChildrenOnEnable : MonoBehaviour
{
    private void OnEnable()
    {
        Transform[] transforms = GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < transforms.Length; i++)
        {
            transforms[i].gameObject.SetActive(true);
        }
    }
}