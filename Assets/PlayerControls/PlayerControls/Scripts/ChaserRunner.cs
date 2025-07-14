using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class ChaserRunner : MonoBehaviour
{
    private enum ChasePhase
    {
        None, InitialApproach, Chasing, Exiting, Capturing
    }

    public static bool IsChaserEngagingPlayer { get; private set; }

    [SerializeField] private UnityEvent OnChaserActivatedWhileStumbled;
    [SerializeField] private UnityEvent OnChaserCapturePlayer;
    [SerializeField] private UnityEvent OnChaserEnabled;
    [SerializeField] private UnityEvent OnChaserDisabled;

    [SerializeField] private GameEvent playerHasDodgedLeft;
    [SerializeField] private GameEvent playerHasDodgedRight;

    [SerializeField] private GameEvent playerHasSlid;

    [SerializeField] private GameEvent playerHasStumbled;
    [SerializeField] private GameEvent gameHasStarted;
    [SerializeField] private SpeedHandler speedHandler;
    [SerializeField] private PlayerSharedData PlayerSharedData;
    [SerializeField] private PlayerBasicMovementShared playerBasicMovementShared;

    [SerializeField] private InputChannel inputChannel;

    [SerializeField] private PlayerSharedData PlayerRunTimeData;

    [SerializeField] private float rotationSpeed;
    [SerializeField] private float xPositionOffsetfromPlayer;
    [SerializeField] private float zDifference;

    [SerializeField] private float chaserDelayInitialRush;
    [SerializeField] private float initialRushOffsetFromPlayer;
    [SerializeField] private float yFollowSpeedOffsetFromPlayer;
    [SerializeField] private float xFollowSpeed;
    [SerializeField] private float xFollowDelay;
    [SerializeField] private float fallingBackFractionSpeed;

    [SerializeField] private GameEvent playerStoppedStumbling;
    [SerializeField] private GameEvent chaserInViewAfterPlayerStumbled;
    [SerializeField] private GameEvent playerHasDodged;
    [SerializeField] private GameEvent chaesrInitialRush;
    [SerializeField] private GameEvent playerHasCrashed;
    [SerializeField] private GameEvent playerHasJumped;

    [SerializeField] private GameEvent cutSceneTimeLineFinished;

    [SerializeField] private SkinnedMeshRenderer mainBody;

    private Rigidbody rb;
    [SerializeField] private Animator animator;
    [SerializeField] private Animator animatorCharacter;
    [SerializeField] private Transform chaserPivot;

    private Vector3 chaserPivotDefaultLocalPosition;

    private ChasePhase phase;

    private float elapsedTimeX;

    private float start_pos;
    private float x_target_val;

    private Coroutine xFollowRoutine;
    private float playerSideWaysSpeedBeforeCrash;

    private bool isCapturedPlayer;
   // private Transform chaserPivotParentT;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PlayerSharedData.ChaserAnimator = animator;
        PlayerSharedData.ChaserTransform = this.transform;
        PlayerSharedData.ChaserCharacterAnimator = animatorCharacter;
        cutSceneTimeLineFinished.TheEvent.AddListener(PlayableDirectorStopped);
        playerHasCrashed.TheEvent.AddListener(HandlePlayerCrashed);

        chaserPivotDefaultLocalPosition = chaserPivot.localPosition;
        IsChaserEngagingPlayer = false;
        //   chaserPivotParentT = chaserPivot.parent;
    }

    private void OnEnable()
    {
     //   UnityEngine.Console.Log("Chaser Enabled");
        OnChaserEnabled.Invoke();
        phase = ChasePhase.None;
        isCapturedPlayer = false;
        StartCoroutine(WaitAndEnableInitialRush());
        IsChaserEngagingPlayer = true;
    }

    private void PlayableDirectorStopped(GameEvent gameEvent)
    {
        animator.applyRootMotion = false;
    }

    private void OnDestroy()
    {
        cutSceneTimeLineFinished.TheEvent.RemoveListener(PlayableDirectorStopped);
        playerHasCrashed.TheEvent.RemoveListener(HandlePlayerCrashed);
    }

    private void OnDisable()
    {
        playerHasSlid.TheEvent.RemoveListener(HandlePlayerHasSlided);
        playerHasDodgedLeft.TheEvent.RemoveListener(HandlePlayerDodgedLeft);
        playerHasDodgedRight.TheEvent.RemoveListener(HandlePlayerDodgedRight);
        gameHasStarted.TheEvent.RemoveListener(HandleGameStarted);
        playerHasStumbled.TheEvent.RemoveListener(ResetChaser);
        playerHasJumped.TheEvent.RemoveListener(SwipedUp);

        // inputChannel.SwipeUpOccured.RemoveListener(SwipedUp);

        if (PlayerRunTimeData.isStumbled)
        {
            playerStoppedStumbling.RaiseEvent();
            PlayerRunTimeData.isStumbled = false;
            IsChaserEngagingPlayer = false;
        }

        OnChaserDisabled.Invoke();
    }

    private IEnumerator WaitAndEnableInitialRush()
    {
        yield return new WaitForSeconds(chaserDelayInitialRush);

        chaesrInitialRush.RaiseEvent();

        if (PlayerSharedData.isStumbled)
        {
            OnChaserActivatedWhileStumbled.Invoke();
        }

        playerHasSlid.TheEvent.AddListener(HandlePlayerHasSlided);
        gameHasStarted.TheEvent.AddListener(HandleGameStarted);
        playerHasDodgedLeft.TheEvent.AddListener(HandlePlayerDodgedLeft);
        playerHasDodgedRight.TheEvent.AddListener(HandlePlayerDodgedRight);
        playerHasJumped.TheEvent.AddListener(SwipedUp);
        playerHasStumbled.TheEvent.AddListener(ResetChaser);

        // inputChannel.SwipeUpOccured.AddListener(SwipedUp);

        phase = ChasePhase.InitialApproach;
        transform.position = new Vector3(PlayerRunTimeData.PlayerTransform.position.x, PlayerRunTimeData.PlayerTransform.position.y, PlayerRunTimeData.PlayerTransform.position.z - (zDifference * 1.4f));
    }

    private void SwipedUp(GameEvent gameEvent)
    {
        animator.SetTrigger(AnimatorParameters.Jump);
        animatorCharacter.SetTrigger(AnimatorParameters.Jump);
    }

    //private void Update()
    //{
    //    if (phase == ChasePhase.None || phase == ChasePhase.Capturing || phase == ChasePhase.Exiting)
    //    {
    //        return;
    //    }

    //    elapsedTime += Time.deltaTime;
    //    if (elapsedTime >= lifeTime)
    //    {
    //        SwitchToExitState(null);
    //    }
    //}

    private void HandlePlayerCrashed(GameEvent gameEvent)
    {
        CoroutineRunner.Instance.StartCoroutine(AdjustXPositionAfterCrashRoutine());

        //x_target_val = PlayerSharedData.PlayerRigidBody.position.x;
        //float diffX = Mathf.Abs(x_target_val - rb.transform.position.x);
        //float perctDiff = (diffX / 1f);

        //// If the X length is less than 1 decrease the side ways time
        //playerSideWaysSpeedBeforeCrash = PlayerSharedData.SidewaysSpeed * perctDiff;

        //if (GameManager.IsGameStarted)
        //{
        //    phase = ChasePhase.Capturing;
        //}
    }

    private IEnumerator AdjustXPositionAfterCrashRoutine()
    {
        float sideSpeedBeforeCrash = PlayerSharedData.SidewaysSpeed;

        yield return new WaitForEndOfFrame();

        elapsedTimeX = 0;
        x_target_val = PlayerSharedData.PlayerRigidBody.position.x;
        float diffX = Mathf.Abs(x_target_val - rb.transform.position.x);
        float perctDiff = (diffX / 1f);

        // If the X length is less than 1 decrease the side ways time
        playerSideWaysSpeedBeforeCrash = sideSpeedBeforeCrash * perctDiff;

        Vector3 curPos = transform.position;
        curPos.z = GetZPositionToReach();

        rb.interpolation = RigidbodyInterpolation.None;
        transform.position = curPos;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (GameManager.IsGameStarted)
        {
            phase = ChasePhase.Capturing;
        }
    }

    private void HandlePlayerHasSlided(GameEvent gameEvent)
    {
        animator.SetTrigger(AnimatorParameters.Slide);
        animatorCharacter.SetTrigger(AnimatorParameters.Slide);
    }

    private IEnumerator SwitchLaneAfterPlayerAfterDelay(Action action)
    {
        yield return new WaitForSeconds(xFollowDelay);
        action();
        start_pos = rb.transform.position.x;
        elapsedTimeX = 0;
        x_target_val = PlayerSharedData.isCrashed ? PlayerSharedData.PlayerRigidBody.position.x : playerBasicMovementShared.x_target_val;
    }

    private void HandlePlayerDodgedLeft(GameEvent gameEvent)
    {
        if (xFollowRoutine != null)
        {
            StopCoroutine(xFollowRoutine);
        }

        xFollowRoutine = StartCoroutine(SwitchLaneAfterPlayerAfterDelay(() =>
        {
            if (!PlayerSharedData.isJumping)
            {
                animator.SetTrigger(AnimatorParameters.Left);
                animatorCharacter.SetTrigger(AnimatorParameters.Left);
            }
        }));
    }

    private void HandlePlayerDodgedRight(GameEvent gameEvent)
    {
        if (xFollowRoutine != null)
        {
            StopCoroutine(xFollowRoutine);
        }

        xFollowRoutine = StartCoroutine(SwitchLaneAfterPlayerAfterDelay(() =>
        {
            if (!PlayerSharedData.isJumping)
            {
                animator.SetTrigger(AnimatorParameters.Right);
                animatorCharacter.SetTrigger(AnimatorParameters.Right);
            }
        }));
    }

    private void HandleGameStarted(GameEvent gameEvent)
    {
        transform.rotation = Quaternion.identity;
    }

    public void OverridePosition()
    {
        elapsedTimeX = 0;
        start_pos = transform.position.x;
        x_target_val = PlayerSharedData.PlayerTransform.position.x;
    }

    private void FixedUpdate()
    {
        if (phase == ChasePhase.None)
            return;

        Quaternion targetRotation;
        float y;
        if (PlayerSharedData.wallslideDoing)
        {
            targetRotation = PlayerRunTimeData.PlayerTransform.rotation;
            y = PlayerRunTimeData.PlayerTransform.position.y;
        }
        else
        {
            targetRotation = Quaternion.RotateTowards(transform.rotation, PlayerRunTimeData.PlayerTransform.rotation, rotationSpeed * Time.fixedDeltaTime);
            y = Mathf.MoveTowards(transform.position.y, PlayerRunTimeData.PlayerTransform.position.y, yFollowSpeedOffsetFromPlayer * Time.fixedDeltaTime);
        }

        if (phase == ChasePhase.InitialApproach)
        {
            float zPosToReach = GetZPositionToReach();
            float z = Mathf.MoveTowards(rb.position.z, zPosToReach, (initialRushOffsetFromPlayer + PlayerSharedData.ForwardSpeed));

            Vector3 posToReach = new Vector3(GetXPositionToReach(), PlayerRunTimeData.PlayerTransform.position.y, z);
            rb.MovePosition(posToReach);

            if (Mathf.Abs(posToReach.z - zPosToReach) <= Mathf.Epsilon)
            {
                start_pos = rb.transform.position.x;
                x_target_val = playerBasicMovementShared.x_target_val;
                elapsedTimeX = 0;
                chaserInViewAfterPlayerStumbled.RaiseEvent();

                phase = PlayerSharedData.isCrashed && GameManager.IsGameStarted ? ChasePhase.Capturing : ChasePhase.Chasing;
            }
        }
        else if (phase == ChasePhase.Chasing)
        {
            //float x = GetXPositionToReach();
            //x = Mathf.MoveTowards(transform.position.x, x, (Time.fixedDeltaTime * xFollowSpeed) / PlayerSharedData.SidewaysSpeed);

            float sideWaysTime = !PlayerSharedData.isCrashed ? PlayerSharedData.SidewaysSpeed : playerSideWaysSpeedBeforeCrash;
            float steer_speed = MyExtensions.elapse_time(ref elapsedTimeX, sideWaysTime);

            // float x = Mathf.SmoothStep(start_pos, x_target_val + xPositionOffsetfromPlayer, steer_speed);
            float x = Mathf.Lerp(start_pos, x_target_val + xPositionOffsetfromPlayer, steer_speed);

            //    UnityEngine.Console.Log($"STart Pos {start_pos} xtargetVal {x_target_val} xpositionOffsetFromPlayer {xPositionOffsetfromPlayer} steerspeed {steer_speed}");

            Vector3 posToReach = new Vector3(x, y, GetZPositionToReach());

            //   UnityEngine.Console.Log("Player Position " + PlayerSharedData.PlayerTransform.position.z);
            //  UnityEngine.Console.Log("Player Rigid Position " + PlayerSharedData.PlayerRigidBody.position.z);
            //   UnityEngine.Console.Log("Move Chaser To " + GetZPositionToReach());
            rb.MovePosition(posToReach);
        }
        else if (phase == ChasePhase.Capturing)
        {
            float sideWaysTime = playerSideWaysSpeedBeforeCrash;
            float steer_speed = MyExtensions.elapse_time(ref elapsedTimeX, sideWaysTime);

            float x = Mathf.Lerp(start_pos, /*x_target_val + */PlayerSharedData.PlayerRigidBody.position.x + xPositionOffsetfromPlayer, steer_speed);

            Vector3 posToReach = new Vector3(x, y, GetZPositionToReach());
            rb.MovePosition(posToReach);

            if (!isCapturedPlayer && steer_speed >= 1)
            {
                isCapturedPlayer = true;
                ExecutePlayerCaptureMechanics();
            }
        }
        else if (phase == ChasePhase.Exiting)
        {
            float z = Mathf.MoveTowards(transform.position.z, GetZPositionToReach(), PlayerSharedData.ForwardSpeed / fallingBackFractionSpeed);
            Vector3 posToReach = new Vector3(GetXPositionToReach(), y, z);
            rb.MovePosition(posToReach);

            if (!mainBody.isVisible)
            {
                this.gameObject.SetActive(false);
            }
        }

        rb.MoveRotation(targetRotation);
    }

    private float GetXPositionToReach()
    {
        return PlayerRunTimeData.PlayerTransform.position.x + xPositionOffsetfromPlayer;
    }

    private float GetZPositionToReach()
    {
        if (PlayerSharedData.isCrashed)
        {
            return PlayerRunTimeData.PlayerTransform.position.z - zDifference;
        }
        else
        {
            return playerBasicMovementShared.movement_vector.z - zDifference;
        }
    }

    public void ResetChaser(GameEvent theEvent)
    {
        chaesrInitialRush.RaiseEvent();
        phase = ChasePhase.InitialApproach;

        OnChaserActivatedWhileStumbled.Invoke();
    }

    public void SwitchToExitState(GameEvent theEvent)
    {
        //if (phase == ChasePhase.None || phase == ChasePhase.Capturing || phase == ChasePhase.Exiting)
        //{
        //    return;
        //}

        if (!PlayerRunTimeData.isStumbled)
            return;

        playerStoppedStumbling.RaiseEvent();
        PlayerRunTimeData.isStumbled = false;
        IsChaserEngagingPlayer = false;
        phase = ChasePhase.Exiting;
    }

    public void InstantlyDisableTheChaser()
    {
        PlayerRunTimeData.isStumbled = false;
        IsChaserEngagingPlayer = false;
        animatorCharacter.SetBool(AnimatorParameters.isCrashed, false);

        animatorCharacter.transform.localPosition = Vector3.zero;

        //chaserPivot.SetParent(chaserPivotParentT);
        chaserPivot.localPosition = chaserPivotDefaultLocalPosition;
        chaserPivot.localScale = Vector3.one;

        PlayerSharedData.isBeingCapturedByChaser = false;

        this.gameObject.SetActive(false);
    }

    private void ExecutePlayerCaptureMechanics()
    {
        //  chaserPivot.SetParent(null, true);
        OnChaserCapturePlayer.Invoke();
        animatorCharacter.SetBool(AnimatorParameters.isCrashed, true);
        PlayerSharedData.isBeingCapturedByChaser = true;
    }

    public void DestroyTheObject(GameObject obj)
    {
        Destroy(obj, 7);
    }
}