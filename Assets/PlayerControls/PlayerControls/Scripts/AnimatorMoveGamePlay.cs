using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorMoveGamePlay : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private PlayerSharedData playerSharedData;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnAnimatorMove()
    {
     
        playerSharedData.PlayerTransform.position += animator.deltaPosition;
        playerSharedData.PlayerTransform.rotation *= animator.deltaRotation;
    }
}