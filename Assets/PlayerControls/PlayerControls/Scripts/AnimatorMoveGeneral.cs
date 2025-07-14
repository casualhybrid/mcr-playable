using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Animator))]
public class AnimatorMoveGeneral : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private Transform target;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnAnimatorMove()
    {
        target.position += animator.deltaPosition;
        target.rotation *= animator.deltaRotation;
    }
}