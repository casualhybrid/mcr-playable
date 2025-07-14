using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class AnimatorParameters
{
    public static int forwardspeed { get; private set; }
    public static int movestate { get; private set; }
    public static int left { get; private set; }
    public static int right { get; private set; }
    public static int slide { get; private set; }
    public static int normal { get; private set; }
    public static int jump { get; private set; }
    public static int lanechangespeed { get; private set; }
    public static int jumpspeed { get; private set; }
    public static int slideright { get; private set; }
    public static int slideleft { get; private set; }
    public static int inairright { get; private set; }
    public static int inairleft { get; private set; }
    public static int canceljump { get; private set; }
    public static int death { get; private set; }
    public static int collectiblePickup { get; private set; }
    public static int boost { get; private set; }
    public static int thurst { get; private set; }
    public static int isFlying { get; private set; }
    public static int crash { get; private set; }
    public static int idle { get; private set; }
    public static int Collision { get; private set; }
    public static int sidecrash { get; private set; }
    public static int Boost { get; private set; }
    public static int Dash { get; private set; }
    public static int Frontcrash { get; private set; }
    public static int springjump { get; private set; }
    public static int coinpickup { get; private set; }
    public static int armor { get; private set; }
    public static int magicwallet { get; private set; }
    public static int collectablepickup { get; private set; }
    public static int shockwave { get; private set; }
    public static int isBoosting { get; private set; }

    public static int Jump { get; private set; }
    public static int Slide { get; private set; }
    public static int Left { get; private set; }
    public static int Right { get; private set; }
    public static int isCrashed { get; private set; }


    public static void SetAnimationParameterHashes()
    {
        forwardspeed = Animator.StringToHash("forwardspeed");
        movestate = Animator.StringToHash("movestate");
        right = Animator.StringToHash("right");
        left = Animator.StringToHash("left");
        slide = Animator.StringToHash("slide");
        normal = Animator.StringToHash("normal");
        jump = Animator.StringToHash("jump");
        lanechangespeed = Animator.StringToHash("lanechangespeed");
        jumpspeed = Animator.StringToHash("jumpspeed");
        slideright = Animator.StringToHash("slideright");
        slideleft = Animator.StringToHash("slideleft");
        inairright = Animator.StringToHash("inairright");
        inairleft = Animator.StringToHash("inairleft");
        canceljump = Animator.StringToHash("canceljump");
        death = Animator.StringToHash("death");
        collectiblePickup = Animator.StringToHash("collectiblePickup");
        boost = Animator.StringToHash("boost");
        thurst = Animator.StringToHash("thurst");
        isFlying = Animator.StringToHash("isFlying");
        crash = Animator.StringToHash("crash");
        idle = Animator.StringToHash("idle");
        Collision = Animator.StringToHash("Collision");
        sidecrash = Animator.StringToHash("sidecrash");
        Boost = Animator.StringToHash("Boost");
        Dash = Animator.StringToHash("Dash");
        Frontcrash = Animator.StringToHash("Frontcrash");
        springjump = Animator.StringToHash("springjump");
        coinpickup = Animator.StringToHash("coinpickup");
        armor = Animator.StringToHash("armor");
        magicwallet = Animator.StringToHash("magicwallet");
        collectablepickup = Animator.StringToHash("collectablepickup");
        shockwave = Animator.StringToHash("shockwave");
        isBoosting = Animator.StringToHash("isBoosting");

        Jump = Animator.StringToHash("Jump");
        Slide = Animator.StringToHash("Slide");
        Left = Animator.StringToHash("Left");
        Right = Animator.StringToHash("Right");
        isCrashed = Animator.StringToHash("isCrashed");
    }
}


[System.Serializable]
public class AnimationnLengthDictinary : SerializableDictionaryBase<string, float>
{
}

[CreateAssetMenu(fileName = "AnimatorController", menuName = "ScriptableObjects/AnimatorController")]
public class Animator_controller : ScriptableObject
{
    public UnityAction<int> OnIKSteerWeightChange { get; set; }

    [SerializeField] private AnimationnLengthDictinary AnimationnLengthDictinary;
  //  [SerializeField] private AnimationnLengthDictinary CharacterAnimations;

    [SerializeField] private PlayerSharedData PlayerSharedData;
    [SerializeField] private PlayerContainedData PlayerContainedData;

    [SerializeField] private GameEvent coinHasBeenPickedUp;
    [SerializeField] private GameEvent playerHasStartedDash;
    [SerializeField] private GameEvent playerHasStartedBoost;
    [SerializeField] private GameEvent playerFinishedBoost;
    [SerializeField] private GameEvent springJump;
    [SerializeField] private GameEvent thurstStarted;
    [SerializeField] private GameEvent magnetHasBeenPickedUp;
    [SerializeField] private GameEvent diamondHasBeenPickedUp;
    [SerializeField] private GameEvent figurineHasBeenPickedUp;
    [SerializeField] private GameEvent mysteryHasBeenPickedUp;
    [SerializeField] private GameEvent playerHasDoneShockWave;
    [SerializeField] private GameEvent playerHasCrashedFromFront;
    [SerializeField] private GameEvent playerHasCrashedFromSide;
    [SerializeField] private GameEvent playerHasStumbled;
    [SerializeField] private GameEvent playerHasJumped;
    [SerializeField] private GameEvent playerStartedFlying;
    [SerializeField] private GameEvent playerEndedFlying;

    [System.NonSerialized] private readonly Dictionary<int, float> AnimationnLengthDictinaryHash = new Dictionary<int, float>();

    [System.NonSerialized] private bool isAnimationParamHashSet;


    // private List<string> transitionsName;
    //public static string LastAniamtion;

    private void OnEnable()
    {
        if (isAnimationParamHashSet)
            return;

        AnimatorParameters.SetAnimationParameterHashes();

        foreach (var item in AnimationnLengthDictinary)
        {
            AnimationnLengthDictinaryHash.Add(Animator.StringToHash(item.Key), item.Value);
        }

        isAnimationParamHashSet = true;
    }

    //subscribe all events
    public void SubscribeEvents()
    {
        PlayerContainedData.AnimationChannel.RightAnimationOccured += RightAnimation;
        PlayerContainedData.AnimationChannel.LeftAnimationOccured += LeftAnimation;
        PlayerContainedData.AnimationChannel.SlideAnimationOccured += SlideAnimation;
        PlayerContainedData.AnimationChannel.CancelJumpAnimationOccured += CancelJumpAnimation;
        PlayerContainedData.AnimationChannel.NormalAnimation += NormalAnimation;
        PlayerContainedData.AnimationChannel.JumpAnimationOccured += JumpAnimation;
        PlayerContainedData.AnimationChannel.DeathAnimationOccured += Death;
        coinHasBeenPickedUp.TheEvent.AddListener(CoinPickupAnimation);
        playerHasStartedDash.TheEvent.AddListener(DashAnimation);
        playerHasStartedBoost.TheEvent.AddListener(BoostAnimation);
        playerFinishedBoost.TheEvent.AddListener(BoostFinished);
        springJump.TheEvent.AddListener(SpringJumpAnimation);
        thurstStarted.TheEvent.AddListener(ThurstAnimation);
        magnetHasBeenPickedUp.TheEvent.AddListener(MagnetAnimation);
        diamondHasBeenPickedUp.TheEvent.AddListener(CollectableAnimation);
        figurineHasBeenPickedUp.TheEvent.AddListener(CollectableAnimation);
        mysteryHasBeenPickedUp.TheEvent.AddListener(CollectableAnimation);
        playerHasDoneShockWave.TheEvent.AddListener(ShockWave);
        playerHasCrashedFromFront.TheEvent.AddListener(PlayerCrashedFromFront);
        playerHasCrashedFromSide.TheEvent.AddListener(PlayerCrashedFromSide);
        playerHasStumbled.TheEvent.AddListener(PlayerCollisionAnimation);
        playerHasJumped.TheEvent.AddListener(Jump);
        playerStartedFlying.TheEvent.AddListener(Fly);
        playerEndedFlying.TheEvent.AddListener(StoppedFlying);
    }

    //unsubscribe all events
    public void UnSubscribeEvents()
    {
        PlayerContainedData.AnimationChannel.RightAnimationOccured -= RightAnimation;
        PlayerContainedData.AnimationChannel.LeftAnimationOccured -= LeftAnimation;
        PlayerContainedData.AnimationChannel.SlideAnimationOccured -= SlideAnimation;
        PlayerContainedData.AnimationChannel.NormalAnimation -= NormalAnimation;
        PlayerContainedData.AnimationChannel.JumpAnimationOccured -= JumpAnimation;
        PlayerContainedData.AnimationChannel.CancelJumpAnimationOccured -= CancelJumpAnimation;
        PlayerContainedData.AnimationChannel.DeathAnimationOccured -= Death;
        coinHasBeenPickedUp.TheEvent.RemoveListener(CoinPickupAnimation);
        playerHasStartedDash.TheEvent.RemoveListener(DashAnimation);
        playerHasStartedBoost.TheEvent.RemoveListener(BoostAnimation);
        playerFinishedBoost.TheEvent.RemoveListener(BoostFinished);
        springJump.TheEvent.RemoveListener(SpringJumpAnimation);
        thurstStarted.TheEvent.RemoveListener(ThurstAnimation);
        magnetHasBeenPickedUp.TheEvent.RemoveListener(MagnetAnimation);
        diamondHasBeenPickedUp.TheEvent.RemoveListener(CollectableAnimation);
        figurineHasBeenPickedUp.TheEvent.RemoveListener(CollectableAnimation);
        mysteryHasBeenPickedUp.TheEvent.RemoveListener(CollectableAnimation);
        playerHasDoneShockWave.TheEvent.RemoveListener(ShockWave);
        playerHasCrashedFromFront.TheEvent.RemoveListener(PlayerCrashedFromFront);
        playerHasCrashedFromSide.TheEvent.RemoveListener(PlayerCrashedFromSide);
        playerHasStumbled.TheEvent.RemoveListener(PlayerCollisionAnimation);
        playerHasJumped.TheEvent.RemoveListener(Jump);
        playerStartedFlying.TheEvent.RemoveListener(Fly);
        playerEndedFlying.TheEvent.RemoveListener(StoppedFlying);
    }

  

    //change state through float parameter
    public void ChangeState(int movestate)
    {
        PlayerSharedData.PlayerAnimator.SetInteger(AnimatorParameters.movestate, movestate);
    }

    //change state through trigger for left right
    public void ChangeStateLeftRightThroughTrigger(int trigger, float sidewaysdistancetocover, float speedmultiplier, Animator animator)
    {
        if (Mathf.Abs(sidewaysdistancetocover) > 0.6f)
        {
            //LastAniamtion = trigger;
            // ChangeTransitionDuration(AnimatorParameters.right, "AnyState", 0, 0.1f, PlayerSharedData.PlayerAnimator);
            int lanechangeSpeedString = AnimatorParameters.lanechangespeed;
            animator.SetFloat(lanechangeSpeedString, speedmultiplier);
            PlayerSharedData.ChaserAnimator.SetFloat(lanechangeSpeedString, speedmultiplier);
            PlayerSharedData.CharacterAnimator.SetFloat(lanechangeSpeedString, speedmultiplier);
            PlayerSharedData.ChaserCharacterAnimator.SetFloat(lanechangeSpeedString, speedmultiplier);

            animator.SetTrigger(trigger);
        }
    }

    public void SetSteerIKWeight(int weight)
    {
        OnIKSteerWeightChange.Invoke(weight);
    }

    //change state through trigger for anyother
    public void ChangeStatThroughTrigger(int trigger, int multiplier, float speedmultiplier, Animator animator)
    {
      //  LastAniamtion = trigger;

        animator.SetFloat(multiplier, speedmultiplier);

        animator.SetTrigger(trigger);
    }

    //change state through trigger for anyother
    public void ChangeStatThroughTrigger(int trigger, Animator animator)
    {
       // LastAniamtion = trigger;
        animator.SetTrigger(trigger);
    }

    //to check animation current state
    public bool AnimationCurrentState(int layer, string statename)
    {
        return PlayerSharedData.PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName(statename);
    }

    // to check whether certain animation is completed
    public bool AnimationCompleted(string statename, int movestate)
    {
        return PlayerSharedData.PlayerAnimator.GetCurrentAnimatorStateInfo(0).IsName(statename) &&
             PlayerSharedData.PlayerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 &&
             PlayerSharedData.PlayerAnimator.GetInteger(AnimatorParameters.movestate) == movestate
            ;
    }

    public void ResetTrigger(int trigger)
    {
        PlayerSharedData.PlayerAnimator.ResetTrigger(trigger);
        PlayerSharedData.CharacterAnimator.ResetTrigger(trigger);

    }

    private void RightAnimation(float sidewaysdistance, float speedmultiplier, int trigger, Animator animator)
    {
        float speed = IncreaseAnimationSpeed(AnimationnLengthDictinaryHash[AnimatorParameters.left], speedmultiplier);
        ChangeStateLeftRightThroughTrigger(trigger, sidewaysdistance, speed, animator);
    }

    private void LeftAnimation(float sidewaysdistance, float speedmultiplier, int trigger, Animator animator)
    {
        //UnityEngine.Console.Log(IncreaseAnimationSpeed(AnimationnLengthDictinary[AnimatorParameters.right], speedmultiplier));
        float speed = IncreaseAnimationSpeed(AnimationnLengthDictinaryHash[AnimatorParameters.right], speedmultiplier);
        ChangeStateLeftRightThroughTrigger(trigger, sidewaysdistance, speed, animator);
    }

    private void Death(Animator animator)
    {
        ChangeStatThroughTrigger(AnimatorParameters.death, animator);
    }

    private void SlideAnimation()
    {
        ChangeStatThroughTrigger(AnimatorParameters.slide, PlayerSharedData.PlayerAnimator);
    }

    private void CancelJumpAnimation()
    {
        ChangeStatThroughTrigger(AnimatorParameters.canceljump, PlayerSharedData.PlayerAnimator);
    }

    private void CoinPickupAnimation(GameEvent gameEvent)
    {
        ChangeStatThroughTrigger(AnimatorParameters.collectiblePickup, PlayerSharedData.PlayerAnimator);
        ChangeStatThroughTrigger(AnimatorParameters.coinpickup, PlayerSharedData.CharacterAnimator);
    }

    private void NormalAnimation()
    {
      // UnityEngine.Console.Log(AnimatorParameters.normal);
        ChangeStatThroughTrigger(AnimatorParameters.normal, PlayerSharedData.PlayerAnimator);
        ChangeStatThroughTrigger(AnimatorParameters.normal, PlayerSharedData.CharacterAnimator);
    }

    private void JumpAnimation(int multiplier, float speedmultiplier, Animator animator)
    {
        if (PlayerSharedData.SpringJump)
        {
            float speed = IncreaseAnimationSpeed(AnimationnLengthDictinaryHash[AnimatorParameters.springjump], speedmultiplier);
            ChangeStatThroughTrigger(AnimatorParameters.springjump, multiplier, speed, animator);
        }
        else
        {
            float speed = IncreaseAnimationSpeed(AnimationnLengthDictinaryHash[AnimatorParameters.jump], speedmultiplier);
            ChangeStatThroughTrigger(AnimatorParameters.jump, multiplier, speed, animator);
        }
    }

    private void Jump(GameEvent gameEvent)
    {
        if (PlayerSharedData.SpringJump)
        {
            float speed = IncreaseAnimationSpeed(AnimationnLengthDictinaryHash[AnimatorParameters.springjump], PlayerSharedData.JumpDuration);
            ChangeStatThroughTrigger(AnimatorParameters.springjump, AnimatorParameters.jumpspeed, speed, PlayerSharedData.CharacterAnimator);
            ChangeStatThroughTrigger(AnimatorParameters.springjump, AnimatorParameters.jumpspeed, speed, PlayerSharedData.PlayerAnimator);
        }
        else
        {
         
            float speed = IncreaseAnimationSpeed(AnimationnLengthDictinaryHash[AnimatorParameters.jump], PlayerSharedData.JumpDuration);
            ChangeStatThroughTrigger(AnimatorParameters.jump, AnimatorParameters.jumpspeed, speed, PlayerSharedData.CharacterAnimator);
            ChangeStatThroughTrigger(AnimatorParameters.jump, AnimatorParameters.jumpspeed, speed, PlayerSharedData.PlayerAnimator);
        }
    }

    private void Fly(GameEvent gameEvent)
    {
        PlayerSharedData.PlayerAnimator.SetBool(AnimatorParameters.isFlying, true);
        PlayerSharedData.PlayerAnimator.SetLayerWeight(1, 1);
    }

    private void StoppedFlying(GameEvent gameEvent)
    {
        PlayerSharedData.PlayerAnimator.SetLayerWeight(1, 0);
        PlayerSharedData.PlayerAnimator.SetBool(AnimatorParameters.isFlying, false);
    }

    private float IncreaseAnimationSpeed(float animationlength, float timetoplay)
    {
        float speed = animationlength / timetoplay;

        return speed;
    }

    private void DashAnimation(GameEvent gameEvent)
    {
        ChangeStatThroughTrigger(AnimatorParameters.Dash, PlayerSharedData.CharacterAnimator);
    }

    private void BoostAnimation(GameEvent gameEvent)
    {
        ChangeStatThroughTrigger(AnimatorParameters.boost, PlayerSharedData.PlayerAnimator);
        ChangeStatThroughTrigger(AnimatorParameters.Boost, PlayerSharedData.CharacterAnimator);

        PlayerSharedData.CharacterAnimator.SetBool(AnimatorParameters.isBoosting, true);
    }

    private void BoostFinished(GameEvent gameEvent)
    {
        PlayerSharedData.CharacterAnimator.SetBool(AnimatorParameters.isBoosting, false);
    }

    private void SpringJumpAnimation(GameEvent gameEvent)
    {
        ChangeStatThroughTrigger(AnimatorParameters.springjump, PlayerSharedData.CharacterAnimator);
    }

    private void ThurstAnimation(GameEvent gameEvent)
    {
        ChangeStatThroughTrigger(AnimatorParameters.thurst, PlayerSharedData.CharacterAnimator);
        //   ChangeStatThroughTrigger(AnimatorParameters.thurst, PlayerSharedData.PlayerAnimator);
    }

    private void MagnetAnimation(GameEvent gameEvent)
    {
        ChangeStatThroughTrigger(AnimatorParameters.magicwallet, PlayerSharedData.CharacterAnimator);
    }

    //figurine+mysterybox+Diamond Animation Working
    private void CollectableAnimation(GameEvent gameEvent)
    {
        ChangeStatThroughTrigger(AnimatorParameters.collectablepickup, PlayerSharedData.CharacterAnimator);
    }

    private void ShockWave(GameEvent gameEvent)
    {
        //ChangeStatThroughTrigger(AnimatorParameters.shockwave, PlayerSharedData.CharacterAnimator);
    }

    private void PlayerCrashedFromFront(GameEvent gameEvent)
    {
        ChangeStatThroughTrigger(AnimatorParameters.Frontcrash, PlayerSharedData.CharacterAnimator);
    }

    private void PlayerCrashedFromSide(GameEvent gameEvent)
    {
        ChangeStatThroughTrigger(AnimatorParameters.sidecrash, PlayerSharedData.CharacterAnimator);
    }

    private void PlayerCollisionAnimation(GameEvent gameEvent)
    {
        // UnityEngine.Console.Log("PlayerCollisionAnimation");
        ChangeStatThroughTrigger(AnimatorParameters.Collision, PlayerSharedData.CharacterAnimator);
    }
}