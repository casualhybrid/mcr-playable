using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class ApplyRootMotionOnOtherTransform : MonoBehaviour
{

    [SerializeField] private Transform transformToApplyTo;
    private Animator animator;

    private Vector3 previousPosition;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    //void OnAnimatorMove()
    //{


    //    if (!animator.applyRootMotion)
    //        return;

    //    if (transformToApplyTo == null)
    //        return;

    //    if (animator)
    //    {
    //        //Vector3 newPosition = transformToApplyTo.position;
    //        //newPosition += animator.deltaPosition;

    //        //transformToApplyTo.transform.position = newPosition;

    //        //transformToApplyTo.transform.rotation *= animator.deltaRotation;

    //        animator.ApplyBuiltinRootMotion();

    //        UnityEngine.Console.Log("Delta Time " + animator.deltaPosition);

    //        transform.position -= animator.deltaPosition;

    //        Vector3 newPosition = transformToApplyTo.position;
    //       newPosition += animator.deltaPosition;

    //        transformToApplyTo.transform.position = newPosition;

    //        //transformToApplyTo.transform.rotation *= animator.deltaRotation;

    //    }
    //}


 

    void OnAnimatorMove()
    {
      //  animator.ApplyBuiltinRootMotion();

        UnityEngine.Console.Log($"Delta Rotation {animator.deltaRotation}");
        UnityEngine.Console.Log($"Delta Position {animator.deltaPosition}");

        //if (!animator.applyRootMotion)
        //    return;

        //if (transformToApplyTo == null)
        //    return;

        //Vector3 currentPosition = transform.position;

        //animator.ApplyBuiltinRootMotion();

        //Vector3 newPosition = transform.position;

        //Vector3 delta = newPosition - currentPosition;

        //transform.position -= delta;

        // transformToApplyTo.position += delta;

        //UnityEngine.Console.Log($"DELTA! {delta}");


    }




  
}