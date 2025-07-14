using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class TapToPlayAnimToIdle : StateMachineBehaviour
{
    [SerializeField] private GameEvent cutSceneStarted;

    private Animator anim;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        cutSceneStarted.TheEvent.AddListener(InitializeToNormalState);
        anim = animator;
    }


    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);

        cutSceneStarted.TheEvent.RemoveListener(InitializeToNormalState);
       
    }

    private void InitializeToNormalState(GameEvent gameEvent)
    {
        anim.SetTrigger(AnimatorParameters.normal);
    }
}
