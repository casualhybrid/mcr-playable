using UnityEngine;

public class ShadowRotator : MonoBehaviour
{
    private Transform root;

    void Start() {
        root = transform.parent;
    }

    void Update() {
        transform.rotation = Quaternion.Euler(90, root.eulerAngles.y, 0);
    }
}
