using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimatorController : MonoBehaviour
{
    [SerializeField] GameEvent coinHasBeenPickedUp;
    [SerializeField] GameEvent playerHasStartedDash;
    [SerializeField] GameEvent playerHasStartedBoost;
    [SerializeField] GameEvent springJump;
    [SerializeField] GameEvent thurstStarted;

    [SerializeField] Animator characterAnimator;
    private void OnEnable()
    {
        coinHasBeenPickedUp.TheEvent.AddListener(CoinPickupAnimation);
        playerHasStartedDash.TheEvent.AddListener(DashAnimation);
        playerHasStartedBoost.TheEvent.AddListener(BoostAnimation);
        springJump.TheEvent.AddListener(SpringJumpAnimation);
        thurstStarted.TheEvent.AddListener(ThurstAnimation);
    }

    void CoinPickupAnimation(GameEvent gameEvent)
    {
        characterAnimator.SetTrigger(AnimatorParameters.coinpickup);
    }

    void DashAnimation(GameEvent gameEvent)
    {
        characterAnimator.SetTrigger(AnimatorParameters.Dash);
    }

    void BoostAnimation(GameEvent gameEvent)
    {
        characterAnimator.SetTrigger(AnimatorParameters.Boost);
    }

    void SpringJumpAnimation(GameEvent gameEvent)
    {
        characterAnimator.SetTrigger(AnimatorParameters.springjump);
    }

    void ThurstAnimation(GameEvent gameEvent)
    {
        characterAnimator.SetTrigger(AnimatorParameters.thurst);
    }

    float IncreaseAnimationSpeed(float animationlength, float timetoplay)
    {
        float speed = (animationlength * 1) / timetoplay;
        return speed;
    }

    public void ChangeStatThroughTrigger(int trigger, int multiplier, float speedmultiplier, Animator animator)
    {
        animator.SetFloat(multiplier, speedmultiplier);
        animator.SetTrigger(trigger);
    }
}
