using UnityEngine;

[CreateAssetMenu(fileName = "AnimationChannel", menuName = "ScriptableObjects/AnimationChannel")]
public class AnimationChannel : ScriptableObject
{
    [HideInInspector]
    public event System.Action<float, float, int, Animator> RightAnimationOccured;

    public event System.Action<float, float, int, Animator> LeftAnimationOccured;

    public event System.Action SlideAnimationOccured;

    public event System.Action CancelJumpAnimationOccured;

    public event System.Action<Animator> DeathAnimationOccured;

    public event System.Action<int, float, Animator> JumpAnimationOccured;

    public event System.Action<int, float, Animator> ThurstAnimationOccured;

    public event System.Action NormalAnimation;

    public event System.Action InAirLeft;

    public event System.Action InAirRight;

    public void MoveRight(float sidewaysdistance, float speedmultiplier, int trigger, Animator animator)
    {
        RightAnimationOccured.Invoke(sidewaysdistance, speedmultiplier, trigger, animator);
    }

    public void MoveLeft(float sidewaysdistance, float speedmultiplier, int trigger, Animator animator)
    {
        LeftAnimationOccured.Invoke(sidewaysdistance, speedmultiplier, trigger, animator);
    }

    public void Airright()
    {
        InAirRight?.Invoke();
    }

    public void AirLeft()
    {
        InAirLeft?.Invoke();
    }

    public void SlideDown()
    {
        SlideAnimationOccured.Invoke();
    }

    public void CancelJump()
    {
        CancelJumpAnimationOccured.Invoke();
    }

    public void Jump(int multiplier, float speedmultiplier, Animator animator)
    {
        JumpAnimationOccured.Invoke(multiplier, speedmultiplier, animator);
    }

    public void Normal()
    {
        NormalAnimation.Invoke();
    }

    public void Death(Animator animator)
    {
        DeathAnimationOccured.Invoke(animator);
    }

    public void Thurst(int multiplier, float speedmultiplier, Animator animator)
    {
        ThurstAnimationOccured?.Invoke(multiplier, speedmultiplier, animator);
    }

}