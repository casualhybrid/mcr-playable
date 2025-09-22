using DG.Tweening;
using Knights.UISystem;
using System;
using System.Collections;
using System.Collections.Generic;
using TheKnights.SaveFileSystem;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
public enum TutorialGesture
{
    SwipeDown, SwipeUp, SwipeRight, SwipeLeft, SwipeUpDown, Tap, DoubleTap, None
}

[System.Serializable]
public struct TutorialSegment
{
    public Vector3 checkPoint;
    public TutorialGesture tutorialGesture;
    public bool IsShowHintInLoop;
    public bool isSlowDownOnHint;
    public bool IsFreezeOnHint;

    public int TargetLane;

    //public List<GameObject> patchObstacles;
    public TutorialHurdles TutorialHurdles;
}

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private float rewindTime;
    [SerializeField] private float pauseTimeAfterRewind;

    [SerializeField] private AudioPlayer PlayRewindSound;
    [SerializeField] private AudioPlayer StopRewindSound;

    // Dependencies
    [SerializeField] private SaveManager saveManager;

    [SerializeField] private PlayerSharedData playerSharedData;
    [SerializeField] private PlayerBasicMovementShared playerBasicMovementShared;
    [SerializeField] private SpeedHandler speedHandler;
    [SerializeField] private PlayerContainedData playerContainedData;

    // Tutorial UI
    [SerializeField] private Canvas tutorialCanvas;

    [SerializeField] private Image tutSwipeHandImage;
    [SerializeField] private Animator tutorialHandAnimator;
    [SerializeField] private GameObject tutorialHand;

    // Tutorial Types Data
    [SerializeField] private TutorialTypesData tutorialTypesData;

    // Channels
    [SerializeField] private TutorialSegmentStateChannel tutorialSegmentStateChannel;

    [SerializeField] private InputChannel inputChannel;

    [SerializeField] private PlayerBoostState playerBoostState;

    // Game Events
    [SerializeField] private GameEvent playerHasCrashed;

    [SerializeField] private GameEvent tutorialHasStarted;
    [SerializeField] private GameEvent tutorialHasEnded;
    [SerializeField] private GameEvent cutSceneTimeLineFinished;
    [SerializeField] private GameEvent gameHasStarted;

    [SerializeField] private GameEvent playerDodgedRight;
    [SerializeField] private GameEvent playerDodgedLeft;
    [SerializeField] private GameEvent playerJumped;
    [SerializeField] private GameEvent playerSlid;
    [SerializeField] private GameEvent playerDashed;
    [SerializeField] private GameEvent playerBoosted;

    /*[ShowIf("onlyShowHintAfterExperimentCompleted")]*/
    [SerializeField] private bool readLastInputOnlyForHintDetection = true;
    [SerializeField] private bool showGestureTutorialOnCheckPointRewind = true;

    [SerializeField] private float slowDownTimeScaleDuringGesture;
    [SerializeField] private float durationToReachSlowDownTime;
    [SerializeField] private float durationToReachNormalTime;

    [SerializeField] private GamePlayUIController gamePlayUIController;
    [SerializeField] private TutorialFeedBackPanel tutorialFeedBackPanel;
    [SerializeField] private Canvas tutorialHintPanelCanvas;
    [SerializeField] private Rigidbody playerRigidBody;


    private readonly HashSet<TutorialGesture> completedGestures = new HashSet<TutorialGesture>();

    public static bool IsTutorialActive { get; private set; }
    public static bool IsTutorialRewinding { get; private set; }
    public static int ActiveTutorialType;
    public static float RewindTime;
    public static float PauseTimeAfterRewind;

    // Player Experimenting Fields
    private uint currentControlSequence;

    private readonly HashSet<uint> currentlyTriedControlSequencesForSegment = new HashSet<uint>();
    private bool listeningToInputEvents;

    // Current tutorial segment information
    private TutorialSegment currentTutorialSegment;

    private bool enteredSegmentCheckPointOnce = false;

    private bool shouldShowHint;
    private BoxCollider lastEnteredGestureShowTrigger;
    private int timesFailedThisSegment;

    private bool swipeUpCompletedOnce;

    private Tween timeScaleChangeTween;

    private TutorialSegmentData tutorialSegmentData;
    private TutorialProperties tutorialProperties;

    private int currentHintGroupIndex;
    private int lastHintGroupIndexReached = -1;

    private Coroutine loopHintRoutineRef;
    private Sequence phoneGestureSequenceTween;

    private Action unsubscribeInputEventAction;

    //private bool hasFailedAllHintGroups;

    private void Awake()
    {
        if (!saveManager.MainSaveFile.TutorialHasCompleted && RemoteConfiguration.IsTutorialRemoved)
        {
            AnalyticsManager.CustomData("RemovedTutorialUser");
            saveManager.MainSaveFile.TutorialHasCompleted = true;
            saveManager.SaveGame();
        }
        else if (!saveManager.MainSaveFile.TutorialHasCompleted && !RemoteConfiguration.IsTutorialRemoved)
        {
            AnalyticsManager.CustomData("TutorialUser");
        }

        if (saveManager.MainSaveFile.TutorialHasCompleted || RemoteConfiguration.IsTutorialRemoved)
        {
            IsTutorialActive = false;
            this.gameObject.SetActive(false);
            return;
        }

        IsTutorialActive = true;
        ActiveTutorialType = RemoteConfiguration.TutorialType;

        UnityEngine.Console.Log($"Tutorial Activated With Type {ActiveTutorialType}");

        TutorialInstanceData tutorialInstanceData = tutorialTypesData.GetTutorialInstanceData(ActiveTutorialType);
        tutorialSegmentData = tutorialInstanceData.TutorialSegmentData;
        tutorialProperties = tutorialInstanceData.TutorialProperties;

        RewindTime = rewindTime;
        PauseTimeAfterRewind = pauseTimeAfterRewind;

        tutorialCanvas.enabled = true;
        inputChannel.PauseDoubleBoostInputsFromUser();
        inputChannel.PauseSingleTapInputsFromUser();
        SubscribeEvents();
    }

    private void Start()
    {
        currentTutorialSegment = tutorialSegmentData.GetTutorialSegmentByIndex(tutorialSegmentData.curTutorialSegmentIndex);
    }

    private void OnDisable()
    {
        DeSubscribeEvents();
    }

    private void SubscribeEvents()
    {
        // Input Events
        playerJumped.TheEvent.AddListener(SwipeUpOccurred);
        playerSlid.TheEvent.AddListener(SwipeDownOccurred);
        playerDodgedLeft.TheEvent.AddListener(SwipeLeftOccurred);
        playerDodgedRight.TheEvent.AddListener(SwipeRightOccurred);
        playerDashed.TheEvent.AddListener(SingleTapOccurred);
        playerBoosted.TheEvent.AddListener(DoubleTapOccurred);

        tutorialSegmentStateChannel.OnTutorialSegmentCompleted += SegmentHasBeenCompleted;
        tutorialSegmentStateChannel.OnShowTutorialGesture += ShowTutorialGesture;
        tutorialSegmentStateChannel.OnSegmentActionsCompleted += ResetAndTurnOffActiveHints;
        tutorialSegmentStateChannel.OnSegmentActionsCompleted += SendSegmentCompletedEvent;
        tutorialSegmentStateChannel.OnSegmentActionsCompleted += ShowFeedbackTutorialIfRequired;
        tutorialSegmentStateChannel.OnSlowMoRequested += SlowDownTheTime;
        tutorialSegmentStateChannel.OnSlowMoEndRequested += ResumeTheTime;
        tutorialSegmentStateChannel.OnPlaySpecificHint += PlayHintWithSpecificGesture;
        tutorialSegmentStateChannel.OnDisableActiveHints += TurnOffActiveHints;
        playerHasCrashed.TheEvent.AddListener(RewindPlayerToActiveCheckPoint);
        playerHasCrashed.TheEvent.AddListener(TurnOffActiveHints);
        cutSceneTimeLineFinished.TheEvent.AddListener(InitializeTutorial);
        tutorialHasEnded.TheEvent.AddListener(HandleTutorialEnd);
    }

    private void DeSubscribeEvents()
    {
        // Input Events
        playerJumped.TheEvent.RemoveListener(SwipeUpOccurred);
        playerSlid.TheEvent.RemoveListener(SwipeDownOccurred);
        playerDodgedLeft.TheEvent.RemoveListener(SwipeLeftOccurred);
        playerDodgedRight.TheEvent.RemoveListener(SwipeRightOccurred);
        playerDashed.TheEvent.RemoveListener(SingleTapOccurred);
        playerBoosted.TheEvent.RemoveListener(DoubleTapOccurred);

        tutorialSegmentStateChannel.OnShowTutorialGesture -= ShowTutorialGesture;
        tutorialSegmentStateChannel.OnTutorialSegmentCompleted -= SegmentHasBeenCompleted;
        tutorialSegmentStateChannel.OnSegmentActionsCompleted -= ResetAndTurnOffActiveHints;
        tutorialSegmentStateChannel.OnSegmentActionsCompleted -= SendSegmentCompletedEvent;
        tutorialSegmentStateChannel.OnSegmentActionsCompleted -= ShowFeedbackTutorialIfRequired;
        tutorialSegmentStateChannel.OnSlowMoRequested -= SlowDownTheTime;
        tutorialSegmentStateChannel.OnSlowMoEndRequested -= ResumeTheTime;
        tutorialSegmentStateChannel.OnPlaySpecificHint -= PlayHintWithSpecificGesture;
        tutorialSegmentStateChannel.OnDisableActiveHints -= TurnOffActiveHints;
        playerHasCrashed.TheEvent.RemoveListener(RewindPlayerToActiveCheckPoint);
        playerHasCrashed.TheEvent.RemoveListener(TurnOffActiveHints);
        cutSceneTimeLineFinished.TheEvent.RemoveListener(InitializeTutorial);
        tutorialHasEnded.TheEvent.RemoveListener(HandleTutorialEnd);
    }

    private void InitializeTutorial(GameEvent gameEvent)
    {
        currentTutorialSegment = tutorialSegmentData.GetTutorialSegmentByIndex(tutorialSegmentData.curTutorialSegmentIndex);

        AnalyticsManager.CustomData("TutorialStarted");
        //  playerRigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        // playerRigidBody.interpolation = RigidbodyInterpolation.None;
        listeningToInputEvents = true;
        MATS_GameManager.instance.isTutorialPlaying = true;
        tutorialHasStarted.RaiseEvent();
    }

    private void RewindPlayerToActiveCheckPoint(GameEvent gameEvent)
    {
        if (timeScaleChangeTween != null)
        {
            timeScaleChangeTween.Kill();
            Time.timeScale = 1;
        }

        timesFailedThisSegment++;

        TutorialGesture tutorialGesture;

        // This is because the second swipe down was previously called swipeupDown
        // if (currentTutorialSegment.tutorialGesture == TutorialGesture.SwipeUp)
        // {
        //     tutorialGesture = !swipeUpCompletedOnce ? currentTutorialSegment.tutorialGesture : TutorialGesture.SwipeUpDown;
        // }
        // else
        //  {
        tutorialGesture = currentTutorialSegment.tutorialGesture;
        // }

        //  AnalyticsManager.CustomData("TutorialSegmentFailed", new Dictionary<string, object>() { { "SegmentName", tutorialGesture.ToString() }, { "TimesFailed", timesFailedThisSegment } });
        string eventName = $"TutorialSegmentFailed{tutorialGesture.ToString()}";
        eventName = completedGestures.Contains(tutorialGesture) ? eventName + "_Beta" : eventName;
        AnalyticsManager.CustomData(eventName, new Dictionary<string, object>() { { "TimesFailed", timesFailedThisSegment } });

        //   UnityEngine.Console.Log($"Segment Failed {tutorialGesture}. Times Failed {timesFailedThisSegment}");

        StartCoroutine(RewindPlayerToActiveCheckPointRoutine());
    }

    private IEnumerator RewindPlayerToActiveCheckPointRoutine()
    {
        // Should we show the tutorial gesture next time?

        bool isRepeatedAction = IsRepeatedAction();

        if (!tutorialProperties.DontDisplayHintsIfExperimenting || (shouldShowHint && isRepeatedAction))
        {
            //if (currentHintGroupIndex >= tutorialProperties.TutorialHintsInOrder.Count - 1)
            //{
            //    hasFailedAllHintGroups = true;
            //}

            bool isHintsOverriden = currentTutorialSegment.TutorialHurdles.isHintsOrderOverriden;
            var hintsInOrder = !isHintsOverriden ? tutorialProperties.TutorialHintsInOrder : currentTutorialSegment.TutorialHurdles.TutorialHintsInOrder;
            currentHintGroupIndex = Mathf.Clamp(++currentHintGroupIndex, 0, hintsInOrder.Count - 1);
        }

        if (!shouldShowHint)
        {
            shouldShowHint = !tutorialProperties.OnlyShowHintAfterExperimentCompleted ? true : isRepeatedAction;
        }

        //shouldShowHint = !tutorialProperties.OnlyShowHintAfterExperimentCompleted ? true : IsRepeatedAction();

        // Add current control sequence to already tried set
        if (!currentlyTriedControlSequencesForSegment.Contains(currentControlSequence))
        {
            currentlyTriedControlSequencesForSegment.Add(currentControlSequence);
        }

        // Clear
        currentControlSequence = 0;

        listeningToInputEvents = false;

        yield return new WaitForSeconds(2);

        playerContainedData.AnimationChannel.Normal();

        currentTutorialSegment = tutorialSegmentData.GetTutorialSegmentByIndex(tutorialSegmentData.curTutorialSegmentIndex);

        var tutorialHurdles = currentTutorialSegment.TutorialHurdles;

        if (tutorialHurdles != null)
        {
            for (int i = 0; i < tutorialHurdles.hurdles.Count; i++)
            {
                GameObject segmentObstacleGameobject = tutorialHurdles.hurdles[i];
                segmentObstacleGameobject.GetComponent<ResetObject>().ResetTheObject();
                segmentObstacleGameobject.GetComponent<RewindToInitialTransform>().DoRewind();
            }
        }

        playerSharedData.PlayerTransform.GetComponent<PlayerStateMachine>().enabled = false;
        Vector3 checkPointPosition = currentTutorialSegment.checkPoint;
        float diffZ = playerSharedData.PlayerTransform.position.z - playerSharedData.ChaserTransform.position.z;
        float diffX = playerSharedData.PlayerTransform.position.x - playerSharedData.ChaserTransform.position.x;

        IsTutorialRewinding = true;
        tutorialSegmentStateChannel.SendRewindEvent();
        PlayRewindSound.ShootAudioEvent();

        Sequence sequence = DOTween.Sequence();

        // sequence.Join(playerSharedData.PlayerTransform.DOMove(checkPointPosition, TutorialManager.RewindTime).SetEase(Ease.Linear));
        sequence.Join(playerRigidBody.DOMove(checkPointPosition, TutorialManager.RewindTime).SetEase(Ease.Linear));
        sequence.Join(playerSharedData.ChaserTransform.DOMove(new Vector3(checkPointPosition.x - diffX, checkPointPosition.y, checkPointPosition.z - diffZ), TutorialManager.RewindTime).SetEase(Ease.Linear).OnComplete(() =>
        {
            playerSharedData.ChaserTransform.GetComponent<ChaserRunner>().OverridePosition();
        }));
        sequence.AppendInterval(TutorialManager.PauseTimeAfterRewind);
        sequence.AppendCallback(() =>
        {
            IsTutorialRewinding = false;
            //GameManager.PauseAndSlowlyIncreaseTimeScale();
            StopRewindSound.ShootAudioEvent();

            tutorialSegmentStateChannel.SendUserResetToCheckPoint((int)checkPointPosition.x);
            playerSharedData.PlayerTransform.GetComponent<PlayerStateMachine>().enabled = true;
            playerSharedData.PlayerStateMachine.RequestStateChange(PlayerState.PlayerNormalMovementState, true);
            speedHandler.RevertGameTimeScaleToLastKnownNormalSpeed();
            speedHandler.IncreaseSpeed();

            listeningToInputEvents = true;

            if (!shouldShowHint)
                return;

            if (showGestureTutorialOnCheckPointRewind)
            {
                PlayHintWithSpecificGesture(TutorialGesture.None, currentTutorialSegment.IsShowHintInLoop);
            }
            // Check if the player is already inside the gesture trigger
            else if (lastEnteredGestureShowTrigger != null)
            {
                bool isIntersecting = lastEnteredGestureShowTrigger.bounds.Intersects(playerSharedData.PlayerHitCollider.bounds);

                if (!isIntersecting)
                {
                    if (lastEnteredGestureShowTrigger.bounds.center.z < playerSharedData.PlayerHitCollider.bounds.center.z)
                    {
                        PlayHintWithSpecificGesture(TutorialGesture.None, currentTutorialSegment.IsShowHintInLoop);
                    }
                }
                else
                {
                    PlayHintWithSpecificGesture(TutorialGesture.None, currentTutorialSegment.IsShowHintInLoop);
                }
            }
        });
    }

    private void ShowTutorialGesture(BoxCollider boxCollider)
    {
        if ((!shouldShowHint && tutorialProperties.OnlyShowHintAfterExperimentCompleted) || showGestureTutorialOnCheckPointRewind)
            return;

        lastEnteredGestureShowTrigger = boxCollider;

        if (IsTutorialRewinding)
            return;

        currentTutorialSegment = tutorialSegmentData.GetTutorialSegmentByIndex(tutorialSegmentData.curTutorialSegmentIndex);
        // PlayerGestureHandAnimation();
        CoroutineRunner.Instance.StartCoroutine(WaitForFixedUpdateAndPlayHints());
    }

    #region Hints

    public void PlayHintWithSpecificGesture(TutorialGesture tutorialGesture = TutorialGesture.None, bool loop = false)
    {
        TurnOffActiveHints();

        if (tutorialProperties.IsSlowDownOnHint && (currentTutorialSegment.isSlowDownOnHint || currentTutorialSegment.IsFreezeOnHint))
        {
            tutorialGesture = tutorialGesture == TutorialGesture.None ? currentTutorialSegment.tutorialGesture : tutorialGesture;

            if (tutorialGesture == TutorialGesture.SwipeRight && playerBasicMovementShared.CurrentLane == currentTutorialSegment.TargetLane)
            {
                return;
            }

            if (tutorialGesture == TutorialGesture.SwipeLeft && playerBasicMovementShared.CurrentLane == currentTutorialSegment.TargetLane)
            {
                return;
            }


            SlowDownTheTime();


            // if (!currentTutorialSegment.isSlowDownOnHint)
            // {
            switch (tutorialGesture)
            {
                case TutorialGesture.Tap:
                    inputChannel.DisableAllButThisInputEvent(inputChannel.SingleTapOccured);
                    unsubscribeInputEventAction = () => { inputChannel.SingleTapOccured.RemoveListener(ResumeTheTimeAfterInputEvent); inputChannel.EnableAllDisabledInputs(); TurnOffActiveHints(); };
                    inputChannel.SingleTapOccured.AddListener(ResumeTheTimeAfterInputEvent);
                    break;

                case TutorialGesture.DoubleTap:
                    inputChannel.DisableAllButThisInputEvent(inputChannel.DoubleTabOccured);
                    unsubscribeInputEventAction = () => { inputChannel.DoubleTabOccured.RemoveListener(ResumeTheTimeAfterInputEvent); inputChannel.EnableAllDisabledInputs(); TurnOffActiveHints(); };
                    inputChannel.DoubleTabOccured.AddListener(ResumeTheTimeAfterInputEvent);
                    break;

                case TutorialGesture.SwipeRight:
                    inputChannel.DisableAllButThisInputEvent(inputChannel.SwipeRightOccured);
                    unsubscribeInputEventAction = () => { inputChannel.SwipeRightOccured.RemoveListener(ResumeTheTimeAfterInputEvent); inputChannel.EnableAllDisabledInputs(); TurnOffActiveHints(); };
                    inputChannel.SwipeRightOccured.AddListener(ResumeTheTimeAfterInputEvent);
                    break;

                case TutorialGesture.SwipeLeft:
                    inputChannel.DisableAllButThisInputEvent(inputChannel.SwipeLeftOccured);
                    unsubscribeInputEventAction = () => { inputChannel.SwipeLeftOccured.RemoveListener(ResumeTheTimeAfterInputEvent); inputChannel.EnableAllDisabledInputs(); TurnOffActiveHints(); };
                    inputChannel.SwipeLeftOccured.AddListener(ResumeTheTimeAfterInputEvent);
                    break;

                case TutorialGesture.SwipeUp:
                    inputChannel.DisableAllButThisInputEvent(inputChannel.SwipeUpOccured);
                    unsubscribeInputEventAction = () => { inputChannel.SwipeUpOccured.RemoveListener(ResumeTheTimeAfterInputEvent); inputChannel.EnableAllDisabledInputs(); TurnOffActiveHints(); };
                    inputChannel.SwipeUpOccured.AddListener(ResumeTheTimeAfterInputEvent);
                    break;

                case TutorialGesture.SwipeDown:
                    inputChannel.DisableAllButThisInputEvent(inputChannel.SwipeDownOccured);
                    unsubscribeInputEventAction = () => { inputChannel.SwipeDownOccured.RemoveListener(ResumeTheTimeAfterInputEvent); inputChannel.EnableAllDisabledInputs(); TurnOffActiveHints(); };
                    inputChannel.SwipeDownOccured.AddListener(ResumeTheTimeAfterInputEvent);
                    break;

                default:
                    throw new System.Exception($"Gesture {tutorialGesture} isn't handled for slowing time");
            }
            // }
        }

        if (!loop)
        {
            PlayTheHints(tutorialGesture);
        }
        else
        {
            loopHintRoutineRef = StartCoroutine(ShowHintInLoop(tutorialGesture));
        }
    }

    private IEnumerator ShowHintInLoop(TutorialGesture tutorialGesture = TutorialGesture.None)
    {
        //  yield return new WaitForFixedUpdate();

        while (true)
        {
            PlayTheHints(tutorialGesture);
            yield return new WaitForSecondsRealtime(2f);
            yield return null;
        }
    }

    private IEnumerator WaitForFixedUpdateAndPlayHints()
    {
        yield return new WaitForFixedUpdate();
        PlayHintWithSpecificGesture(TutorialGesture.None, currentTutorialSegment.IsShowHintInLoop);
    }

    private void PlayTheHints(TutorialGesture tutorialGesture = TutorialGesture.None)
    {
        if (tutorialGesture == TutorialGesture.SwipeUpDown || (tutorialGesture == TutorialGesture.None && currentTutorialSegment.tutorialGesture == TutorialGesture.SwipeUpDown))
        {
            tutorialGesture = TutorialGesture.SwipeUp;
        }

        List<TutorialHints[]> hintsInOrder;
        bool isHintsOverriden = currentTutorialSegment.TutorialHurdles.isHintsOrderOverriden;

        hintsInOrder = !isHintsOverriden ? tutorialProperties.TutorialHintsInOrder : currentTutorialSegment.TutorialHurdles.TutorialHintsInOrder;

        lastHintGroupIndexReached = currentHintGroupIndex;
        TutorialHints[] hintsToPlay = hintsInOrder[currentHintGroupIndex];

        foreach (TutorialHints tutHint in hintsToPlay)
        {
            switch (tutHint)
            {
                case TutorialHints.PhoneGestures:
                    {
                        PlayPhoneGestureHint(tutorialGesture);
                        break;
                    }

                case TutorialHints.Decals:
                    {
                        PlayDecalsHint();
                        break;
                    }

                case TutorialHints.ArrowGestures:
                    {
                        PlayHandGestureHint(false, tutorialGesture);
                        break;
                    }

                case TutorialHints.HandGestures:
                    {
                        PlayHandGestureHint(true, tutorialGesture);
                        break;
                    }
                case TutorialHints.Text:
                    {
                        PlayTextHint(tutorialGesture);
                        break;
                    }
            }
        }

        //if (tutorialProperties.IsSlowDownOnHint)
        //{
        //    if (timeScaleChangeTween != null)
        //    {
        //        timeScaleChangeTween.Kill();
        //    }

        //    timeScaleChangeTween = DOTween.To(() => Time.timeScale, (x) => { Time.timeScale = x; }, 1f, durationToReachNormalTime).SetDelay(1.35f).OnComplete(()=> { GameManager.IsTimeScaleLocked = false; });
        //}

        if (tutorialProperties.IsAutoPassOnFaliure)
        {
            // Kya yad kro gay
            if (currentTutorialSegment.tutorialGesture == TutorialGesture.Tap && timesFailedThisSegment >= 1)
            {
                // inputChannel.SingleTap();
                playerBoostState.StartDash(true);
            }

            // Kya yad kro gay
            if (currentTutorialSegment.tutorialGesture == TutorialGesture.SwipeDown && timesFailedThisSegment >= 1)
            {
                inputChannel.SwipeDown();
            }
        }
    }

    private void PlayDecalsHint()
    {
        var tutorialHurdles = currentTutorialSegment.TutorialHurdles;

        if (tutorialHurdles == null)
            return;

        var decals = tutorialHurdles.Decals;

        if (decals == null)
            return;

        for (int i = 0; i < decals.Length; i++)
        {
            GameObject decal = decals[i];
            decal.SetActive(true);
        }
    }

    private void PlayPhoneGestureHint(TutorialGesture gesture = TutorialGesture.None)
    {
        TutorialGesture gestureRequired = gesture == TutorialGesture.None ? currentTutorialSegment.tutorialGesture : gesture;

        playerSharedData.CharacterAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        playerSharedData.CharacterAnimator.SetLayerWeight(4, 1);
        playerSharedData.CharacterAnimator.Play(gestureRequired.ToString(), 4, 0);

        CharacterSkeleton characterSkeleton = playerSharedData.CharacterSkeleton;
        Transform rightHandT = characterSkeleton.rightHandBoneT;

        characterSkeleton.tutorialPhone.SetActive(true);

        phoneGestureSequenceTween = DOTween.Sequence();
        phoneGestureSequenceTween.Append(rightHandT.DOScale(3, 0.2f));
        phoneGestureSequenceTween.AppendInterval(.65f);
        phoneGestureSequenceTween.Append(rightHandT.DOScale(1, .5f));
        phoneGestureSequenceTween.AppendCallback(() =>
        {
            characterSkeleton.tutorialPhone.SetActive(false);
            playerSharedData.CharacterAnimator.updateMode = AnimatorUpdateMode.Normal;
        });
        phoneGestureSequenceTween.SetUpdate(true);
    }

    private void PlayHandGestureHint(bool enableHand, TutorialGesture gesture = TutorialGesture.None)
    {
        TutorialGesture gestureRequired = gesture == TutorialGesture.None ? currentTutorialSegment.tutorialGesture : gesture;

        tutSwipeHandImage.enabled = gestureRequired == TutorialGesture.Tap || gestureRequired == TutorialGesture.DoubleTap || enableHand;
        tutorialHand.SetActive(true);

        // Only doing it for default tutorial
        if (ActiveTutorialType == 0)
        {
            if (tutorialSegmentData.curTutorialSegmentIndex == 3)
            {
                tutorialHand.GetComponent<RectTransform>().anchoredPosition = new Vector2(156, 163);
            }
            else if (tutorialSegmentData.curTutorialSegmentIndex == 4)
            {
                tutorialHand.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 163);
            }
            else
            {
                tutorialHand.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 403);
            }
        }

        tutorialHandAnimator.Play(gestureRequired.ToString(), 0, 0);
    }

    private void PlayTextHint(TutorialGesture gesture = TutorialGesture.None)
    {
        TutorialGesture tutorialGesture = gesture == TutorialGesture.None ? currentTutorialSegment.tutorialGesture : gesture;
        tutorialHintPanelCanvas.enabled = true;
        tutorialHintPanelCanvas.GetComponent<TutorialActionText>().OnPropertiesSet(tutorialGesture);
        // gamePlayUIController.ShowPanelScreen(ScreenIds.TutorialTextHintPanel, ScreenOperation.Open, new TutorialTextHintProperties() { CurrentTutorialGesture = tutorialGesture });
    }

    #endregion Hints

    private void ResetAndTurnOffActiveHints()
    {
        lastEnteredGestureShowTrigger = null;
        shouldShowHint = false;
        // hasFailedAllHintGroups = false;
        currentHintGroupIndex = 0;
        TurnOffActiveHints();
    }

    private void ShowFeedbackTutorialIfRequired()
    {
        MATS_Debug.Log($"tutorialSegmentData.curTutorialSegmentIndex MATSSSSSS {tutorialSegmentData.curTutorialSegmentIndex}");
        if (tutorialSegmentData.curTutorialSegmentIndex == 5)
        {
            tutorialFeedBackPanel.gameObject.SetActive(true);
        }
    }

    public void TurnOffActiveHints(GameEvent gameEvent = null)
    {
        if (loopHintRoutineRef != null)
        {
            StopCoroutine(loopHintRoutineRef);
        }

        if (phoneGestureSequenceTween != null)
        {
            phoneGestureSequenceTween.Kill();
        }

        if (phoneGestureSequenceTween != null)
        {
            phoneGestureSequenceTween.Kill();
        }

        CharacterSkeleton characterSkeleton = playerSharedData.CharacterSkeleton;
        characterSkeleton.rightHandBoneT.localScale = Vector3.one;
        playerSharedData.CharacterSkeleton.tutorialPhone.SetActive(false);
        playerSharedData.CharacterAnimator.updateMode = AnimatorUpdateMode.Normal;
        playerSharedData.CharacterAnimator.SetLayerWeight(4, 0);

        tutorialHand.SetActive(false);
        tutorialHintPanelCanvas.enabled = false;
        // gamePlayUIController.ShowPanelScreen(ScreenIds.TutorialTextHintPanel, ScreenOperation.Close);
    }

    public void TurnOffActiveHints()
    {
        TurnOffActiveHints(null);
    }

    private void SendSegmentCompletedEvent()
    {
        TutorialGesture tutorialGesture;

        // This is because the second swipe down was previously called swipeupDown
        //if (currentTutorialSegment.tutorialGesture == TutorialGesture.SwipeUp)
        //{
        //    tutorialGesture = !swipeUpCompletedOnce ? currentTutorialSegment.tutorialGesture : TutorialGesture.SwipeUpDown;
        //}
        //  else
        // {
        tutorialGesture = currentTutorialSegment.tutorialGesture;
        // }

        //  AnalyticsManager.CustomData("TutorialSegmentCompleted", new Dictionary<string, object>() { { "SegmentName", tutorialGesture.ToString() }, { "TimesFailed", timesFailedThisSegment } });
        string eventName = $"TutorialSegmentCompleted{tutorialGesture.ToString()}";
        eventName = completedGestures.Contains(tutorialGesture) ? eventName + "_Beta" : eventName;
        AnalyticsManager.CustomData(eventName, new Dictionary<string, object>() { { "TimesFailed", timesFailedThisSegment }, { "HintGroup", lastHintGroupIndexReached } });
        UnityEngine.Console.Log($"Segment Completed {tutorialGesture}. Times Failed {timesFailedThisSegment}. Hint Group Index {lastHintGroupIndexReached}");

        lastHintGroupIndexReached = -1;
    }

    private void SegmentHasBeenCompleted()
    {
        // First Segment checkpoint
        if (!enteredSegmentCheckPointOnce)
        {
            enteredSegmentCheckPointOnce = true;
            if (!tutorialProperties.OnlyShowHintAfterExperimentCompleted && showGestureTutorialOnCheckPointRewind)
            {
                PlayHintWithSpecificGesture(TutorialGesture.None, currentTutorialSegment.IsShowHintInLoop);
            }
            return;
        }

        // UnityEngine.Console.Log($"Segment has been completed {tutorialSegmentData.curTutorialSegmentIndex}");
        //TutorialGesture tutorialGesture;

        //// This is because the second swipe down was previously called swipeupDown
        //if (currentTutorialSegment.tutorialGesture == TutorialGesture.SwipeDown)
        //{
        //    tutorialGesture = !swipeDownCompletedOnce ? currentTutorialSegment.tutorialGesture : TutorialGesture.SwipeUpDown;
        //}
        //else
        //{
        //    tutorialGesture = currentTutorialSegment.tutorialGesture;
        //}

        //AnalyticsManager.CustomData("TutorialSegmentCompleted", new Dictionary<string, object>() { { "SegmentName", tutorialGesture.ToString() }, { "TimesFailed", timesFailedThisSegment } });
        //UnityEngine.Console.Log($"Segment Completed {tutorialGesture}. Times Failed {timesFailedThisSegment}");

        if (!completedGestures.Contains(currentTutorialSegment.tutorialGesture))
        {
            completedGestures.Add(currentTutorialSegment.tutorialGesture);
        }

        if (currentTutorialSegment.tutorialGesture == TutorialGesture.SwipeUp)
        {
            swipeUpCompletedOnce = true;
        }

        timesFailedThisSegment = 0;

        // Clear sequence data
        currentControlSequence = 0;
        currentlyTriedControlSequencesForSegment.Clear();

        currentTutorialSegment = tutorialSegmentData.GetTutorialSegmentByIndex(++tutorialSegmentData.curTutorialSegmentIndex);

        if (currentTutorialSegment.tutorialGesture == TutorialGesture.Tap)
        {
            inputChannel.UnPauseSingleTapInputsFromUser();
        }

        if (tutorialSegmentData.curTutorialSegmentIndex >= tutorialSegmentData.SegmentsCount - 1)
        {
            inputChannel.UnPauseDoubleBoostInputsFromUser();
        }

        // Send segment updated event
        tutorialSegmentStateChannel.SendTutorialSegmentUpdatedEvent();

        if (!tutorialProperties.OnlyShowHintAfterExperimentCompleted && showGestureTutorialOnCheckPointRewind)
        {
            PlayHintWithSpecificGesture(TutorialGesture.None, currentTutorialSegment.IsShowHintInLoop);
        }
    }

    private bool IsRepeatedAction()
    {
        //if (shouldShowHint)
        //    return true;

        if (currentlyTriedControlSequencesForSegment.Count == 0)
            return false;

        // Player has tried atleast one time and failed
        bool repeatedAction = DoesControlSequenceExistsInAlreadyTriedSequences(currentControlSequence);

        if (repeatedAction)
        {
            UnityEngine.Console.Log("This control sequence already exisits " + currentControlSequence);
        }

        UnityEngine.Console.Log("Should We Show Tutorial Gesture? " + repeatedAction);

        return repeatedAction;
    }

    private bool DoesControlSequenceExistsInAlreadyTriedSequences(uint controlSequence)
    {
        return currentlyTriedControlSequencesForSegment.Contains(controlSequence);
    }

    #region InputEventsListeners

    private void SwipeDownOccurred(GameEvent gameEvent)
    {
        if (!listeningToInputEvents)
            return;

        UpdateCurrentControlSequence(TutorialGesture.SwipeDown);
    }

    private void SwipeUpOccurred(GameEvent gameEvent)
    {
        if (!listeningToInputEvents)
            return;

        UpdateCurrentControlSequence(TutorialGesture.SwipeUp);
    }

    private void SwipeRightOccurred(GameEvent gameEvent)
    {
        if (!listeningToInputEvents)
            return;

        UpdateCurrentControlSequence(TutorialGesture.SwipeRight);
    }

    private void SwipeLeftOccurred(GameEvent gameEvent)
    {
        if (!listeningToInputEvents)
            return;

        UpdateCurrentControlSequence(TutorialGesture.SwipeLeft);
    }

    private void SingleTapOccurred(GameEvent gameEvent)
    {
        if (!listeningToInputEvents)
            return;

        UpdateCurrentControlSequence(TutorialGesture.Tap);
    }

    private void DoubleTapOccurred(GameEvent gameEvent)
    {
        if (!listeningToInputEvents)
            return;

        UpdateCurrentControlSequence(TutorialGesture.DoubleTap);
    }

    #endregion InputEventsListeners

    private void UpdateCurrentControlSequence(TutorialGesture tutorialGesture)
    {
        uint gestureToAdd = (uint)tutorialGesture;

        if (currentControlSequence == 0 || readLastInputOnlyForHintDetection)
        {
            currentControlSequence = gestureToAdd;
            // UnityEngine.Console.Log("Current Control Sequence " + currentControlSequence);
            return;
        }

        currentControlSequence = currentControlSequence.Concat(gestureToAdd);

        //   UnityEngine.Console.Log("Current Control Sequence " + currentControlSequence);
    }

    private void HandleTutorialEnd(GameEvent gameEvent)
    {
        tutorialFeedBackPanel.feedBackTxt.text = "You got it!";
        tutorialFeedBackPanel.gameObject.SetActive(true);

        AnalyticsManager.CustomData("TutorialEnded");
        MATS_GameManager.instance.isTutorialPlaying = false;
        playerRigidBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        playerRigidBody.interpolation = RigidbodyInterpolation.Interpolate;
        DeSubscribeEvents();
        saveManager.MainSaveFile.TutorialHasCompleted = true;
        IsTutorialActive = false;
        saveManager.SaveGame();
        Destroy(this.gameObject, 5f);
        gameHasStarted.RaiseEvent();
    }

    #region SwipeUpDownSlowMo

    private void ResumeTheTimeAfterInputEvent()
    {
        unsubscribeInputEventAction();
        ResumeTheTime();
    }

    public void SlowDownTheTime()
    {
        float targetTimeScale = currentTutorialSegment.isSlowDownOnHint ? 0.03f : 0f;

        if (timeScaleChangeTween != null)
        {
            timeScaleChangeTween.Kill();
        }

        GameManager.IsTimeScaleLocked = true;
        timeScaleChangeTween = DOTween.To(() => Time.timeScale, (x) => { Time.timeScale = x; }, /*0.03f*/ targetTimeScale, 0.02f).SetUpdate(true).SetUpdate(UpdateType.Fixed).SetEase(Ease.OutQuad).SetUpdate(true);
    }

    public void ResumeTheTime()
    {
        if (timeScaleChangeTween != null)
        {
            timeScaleChangeTween.Kill();
        }

        timeScaleChangeTween = DOTween.To(() => Time.timeScale, (x) => { Time.timeScale = x; }, 1f, 0f).SetUpdate(UpdateType.Fixed).SetUpdate(true).OnComplete(() => { GameManager.IsTimeScaleLocked = false; });
    }

    #endregion SwipeUpDownSlowMo
}