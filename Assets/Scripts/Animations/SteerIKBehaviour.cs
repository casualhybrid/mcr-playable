using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SteerIKBehaviour : MonoBehaviour
{
    [SerializeField] private Animator_controller Animator_Controller;
    private static int steerIKTag = Animator.StringToHash("SteerIK");

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Update()
    {
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.tagHash == steerIKTag)
        {
            Animator_Controller.SetSteerIKWeight(1);
        }
        else
        {
            Animator_Controller.SetSteerIKWeight(0);
        }
    }
}