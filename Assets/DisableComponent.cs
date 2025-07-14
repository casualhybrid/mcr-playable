using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DisableComponent : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void DisableAnimator()
    {
        animator.enabled = false;
    }





  //  protected override void PostPipelineStageCallback(
  //CinemachineVirtualCameraBase vcam,
  //CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
  //  {
  //      //if (stage == CinemachineCore.Stage.Finalize)
  //      //{
  //      //    Vector3 pos = state.RawPosition;

  //      //    pos.y = 0.9389665f;

  //      //    state.RawPosition = pos;
  //      //}


  //  }
}
