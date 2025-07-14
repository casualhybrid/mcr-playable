using Sirenix.OdinInspector;
using System.Collections.Generic;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class SwipeDetector : MonoBehaviour
{
    private enum SwipeDirection
    {
        None, Up, Down, Left, Right
    }

    #region Serialized Members

    [SerializeField] private InputChannel InputChannel;
    [SerializeField] private GameEvent gameHasStarted;

    [Space]
    [BoxGroup("Swipe Settings", true, true)] [SerializeField] private bool detectSwipeAfterRelease = false;

    [BoxGroup("Swipe Settings", true, true)] [SerializeField] private float swipeThresholdInchesHorizontal = 0.101f;
    [BoxGroup("Swipe Settings", true, true)] [SerializeField] private float swipeThresholdInchesVertical = 0.126f;
    [BoxGroup("Swipe Settings", true, true)] [SerializeField] private float swipeThresholdOffsetForContinousSameDirectionSwipeNormPerct = .6f;
    [BoxGroup("Swipe Settings", true, true)] [SerializeField] private float minVelocityForValidSwipe;
    [BoxGroup("Swipe Settings", true, true)] [ShowIf("isDetectSwipeAfterRelease")] [SerializeField] private float minContiniousSwipeAngleForSwipeReDetection;
    [BoxGroup("Swipe Settings", true, true)] [ShowIf("isDetectSwipeAfterRelease")] [SerializeField] private float velocityReductionForLastSwipeRefreshNormalizedPercent;
    [BoxGroup("Swipe Settings", true, true)] [ShowIf("isDetectSwipeAfterRelease")] [SerializeField] private float lowVelocityDurationToCountSwipeAsStationary;

    [BoxGroup("Tap Settings", true, true)] [SerializeField] private float tapThresholdInches = 0.00076f;
    [BoxGroup("Tap Settings", true, true)] [SerializeField] private float TapTimer;
    [BoxGroup("Tap Settings", true, true)] [SerializeField] private float doubleTapWindowDuraiton;

    [SerializeField] private InputSettings inputSettings;

    #endregion Serialized Members

    #region Private Members

    private bool isInitialzied;
    private float screenDPI = 1;
    private int activeFingerID = -1;

    private float SWIPE_THRESHOLD_Horizontal = 30f;
    private float SWIPE_THRESHOLD_Vertical = 40f;
    private float tapThreshold = .3f;

    private Vector2 fingerDownPos;
    private Vector2 fingerUpPos;
    private bool swipe_done = false;
    private SwipeDirection lastSwipeDirection;
    private SwipeDirection lastSwipeDirectionPresistentForReswipePurpose;
    private Vector2 touchOrigin;
    private Vector2 lastValidSwipeDetectedTouchPos;
    private bool hasSwipeOnced;
    private bool hasSwipeOncedInTouchesLifeTime;
    private float velocityAtSwipePerformed;
    private float elapsedLowVelocityTime;
    private float timeSwipeStarted;

    private float currentTapTime;
    private float currentDoubleTapWindowElapsed;
    private float tapCounter;

    private bool playerIsInGame;

    #endregion Private Members

    #region MonoBehaviour CallBacks

    private void Awake()
    {
        EnhancedTouchSupport.Enable();
        gameHasStarted.TheEvent.AddListener(HandleGameHasStarted);
    }

    private void Start()
    {
#if !UNITY_EDITOR
        CalculateThresholdsAccordingToDPI();
        minVelocityForValidSwipe = minVelocityForValidSwipe * screenDPI;
#endif
    }

    private void OnDestroy()
    {
        gameHasStarted.TheEvent.RemoveListener(HandleGameHasStarted);
    }

    #endregion MonoBehaviour CallBacks

    private float VerticalMoveValue() => Mathf.Abs(fingerDownPos.y - fingerUpPos.y);

    private float HorizontalMoveValue() => Mathf.Abs(fingerDownPos.x - fingerUpPos.x);

#if UNITY_EDITOR
    private bool isDetectSwipeAfterRelease => !detectSwipeAfterRelease;
#endif

    private void HandleGameHasStarted(GameEvent theEvent)
    {
        playerIsInGame = true;
        isInitialzied = true;
    }

    public void initialize(bool var)
    {
        isInitialzied = var;
    }

    private void Update()
    {
        if (!isInitialzied)
        {
            return;
        }

#if UNITY_EDITOR
        Inputs();
#endif

#if !UNITY_EDITOR

        MobileInput();

        currentTapTime -= Time.unscaledDeltaTime;

        if (tapCounter == 1)
        {
            currentDoubleTapWindowElapsed += Time.unscaledDeltaTime;

            //UnityEngine.Console.LogError("Single Tap");
        }

        // Check for single tap
        if (currentDoubleTapWindowElapsed >= doubleTapWindowDuraiton && tapCounter == 1)
        {
            ResetTapWindow();
        }

#endif
    }

    public void OnSwipeVelocityValueChanged(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            float val = float.Parse(text);
            minVelocityForValidSwipe = val;
        }
    }

    public void OnContinousOffsetThresholdChange(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            float val = float.Parse(text);
            swipeThresholdOffsetForContinousSameDirectionSwipeNormPerct = val;
        }
    }

    public void OnLowPercentValueChanged(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            float val = float.Parse(text);
            velocityReductionForLastSwipeRefreshNormalizedPercent = val;
        }
    }

    public void OnStationaryVelPercentValueChanged(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            float val = float.Parse(text);
            lowVelocityDurationToCountSwipeAsStationary = val;
        }
    }

    #region MobileInputs

    private void CalculateThresholdsAccordingToDPI()
    {
        float dpi = Screen.dpi;
        float targetDPI = QualitySettings.resolutionScalingFixedDPIFactor * 440f;
        float realConstantDPI = dpi;

        if (realConstantDPI > targetDPI)
        {
            dpi *= (targetDPI / dpi);
            UnityEngine.Console.Log($"Target DPI was {targetDPI} with scaling factor {QualitySettings.resolutionScalingFixedDPIFactor}. Scaled DPI to {dpi}");
        }

        if (dpi == 0)
        {
            UnityEngine.Console.LogWarning("Failed to get Device DPI. Reverting to default pixel threshold value for swipe detection");
            AnalyticsManager.CustomData("FailedGettingDPI", new Dictionary<string, object> { { "DeviceModel", SystemInfo.deviceModel } });
        }
        else
        {
            UnityEngine.Console.Log($"Denstiy Device DPI is {dpi}");

            #region AlternativeWay

            //#if !UNITY_EDITOR && UNITY_ANDROID
            //            AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            //            AndroidJavaObject activity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");

            //            AndroidJavaObject metrics = new AndroidJavaObject("android.util.DisplayMetrics");
            //            activity.Call<AndroidJavaObject>("getWindowManager").Call<AndroidJavaObject>("getDefaultDisplay").Call("getMetrics", metrics);

            //            screenDPI = (metrics.Get<float>("xdpi") + metrics.Get<float>("ydpi")) * 0.5f;

            //              UnityEngine.Console.Log($"xdhpi/ydhpi dpi is {screenDPI}");
            //#else
            //            screenDPI = dpi;
            //#endif

            #endregion AlternativeWay

            screenDPI = dpi;

            SWIPE_THRESHOLD_Horizontal = screenDPI * swipeThresholdInchesHorizontal;
            SWIPE_THRESHOLD_Vertical = screenDPI * swipeThresholdInchesVertical;
            tapThreshold = screenDPI * tapThresholdInches;

            inputSettings.tapRadius = 5f / (dpi / 352f);
            UnityEngine.Console.Log("Calculated input touch radius " + inputSettings.tapRadius);

            UnityEngine.Console.Log($"Tap threshold in inches is {tapThreshold}");
        }

        //  UnityEngine.Console.Log($"Device PPI is {screenDPI} and caclulated Horizontal Swipe Threshold is {SWIPE_THRESHOLD_Horizontal}," +
        //    $" while Vertical Swipe Threshold is {SWIPE_THRESHOLD_Vertical}");
    }

    private void ResetTapWindow()
    {
        currentDoubleTapWindowElapsed = 0;
        currentTapTime = 0;
        tapCounter = 0;
    }

    private bool IgnoreTouchIfOnGUI(UnityEngine.InputSystem.EnhancedTouch.Touch touch)
    {
        if (!playerIsInGame)
            return false;

        if (EventSystem.current.IsPointerOverGameObject(touch.touchId))
        {
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended || touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                activeFingerID = -1;
            }

            return true;
        }

        return false;
    }

    private void MobileInput()
    {
        foreach (var touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
        {
            if (activeFingerID == -1 && !IgnoreTouchIfOnGUI(touch))
            {
                activeFingerID = touch.touchId;
            }
            else if (touch.touchId != activeFingerID)
            {
                continue;
            }

            if (IgnoreTouchIfOnGUI(touch))
                return;

            //if (touch.isTap)
            //{
            //    UnityEngine.Console.Log("IS TAP Before Phase Detection. Phase: " + touch.phase);
            //}

            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                // UnityEngine.Console.Log("Touch Started");
                elapsedLowVelocityTime = 0;
                lastSwipeDirection = SwipeDirection.None;
                lastSwipeDirectionPresistentForReswipePurpose = SwipeDirection.None;
                swipe_done = false;
                hasSwipeOnced = false;
                hasSwipeOncedInTouchesLifeTime = false;
                fingerUpPos = touch.screenPosition;
                fingerDownPos = touch.screenPosition;
                currentTapTime = TapTimer;
                touchOrigin = touch.startScreenPosition;
                timeSwipeStarted = Time.unscaledTime;
            }

            // Detects Swipe while finger is still moving on screen
            else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                if (hasSwipeOnced)
                {
                    float currentVelocity = CalculateCurrentTouchVelocity(touch);
                    float velDiff = currentVelocity / velocityAtSwipePerformed;

                    if (velDiff <= velocityReductionForLastSwipeRefreshNormalizedPercent)
                    {
                        elapsedLowVelocityTime += /*touch.deltaTime*/ Time.unscaledDeltaTime;

                        if (elapsedLowVelocityTime >= lowVelocityDurationToCountSwipeAsStationary)
                        {
                            //   UnityEngine.Console.Log("Velocity While Swiping Later Got Low = " + currentVelocity);

                            ResetVariablesForReSwipeDetection(touch);
                            return;
                        }
                    }
                    else
                    {
                        elapsedLowVelocityTime = 0;
                    }

                    Vector2 touchPos = touch.screenPosition;
                    Vector2 originToCurrentTouch = touchPos - touchOrigin;
                    Vector2 originToLastValidSwipedTouch = lastValidSwipeDetectedTouchPos - touchOrigin;

                    float angle = Vector2.Angle(originToCurrentTouch, originToLastValidSwipedTouch);

                    // UnityEngine.Console.Log("Angle Was " + angle);

                    if (angle >= minContiniousSwipeAngleForSwipeReDetection)
                    {
                        //UnityEngine.Console.Log("Refreshing Swipe as angle was greater or equal to " + minContiniousSwipeAngleForSwipeReDetection);
                        swipe_done = false;
                        touchOrigin = touchPos;
                        fingerUpPos = touchPos;
                        lastValidSwipeDetectedTouchPos = touchPos + touch.delta;
                        timeSwipeStarted = Time.unscaledTime;
                    }
                }

                if (swipe_done)
                    return;

                if (!detectSwipeAfterRelease)
                {
                    lastValidSwipeDetectedTouchPos = touch.screenPosition;
                    fingerDownPos = touch.screenPosition;
                    DetectSwipe(touch);
                }
            }
            else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Stationary)
            {
                //UnityEngine.Console.Log("Touch Got Stationary");
                //ResetVariablesForReSwipeDetection(touch);
                //  lastSwipeDirectionPresistentForReswipePurpose = SwipeDirection.None;
            }

            //Detects swipe after finger is released from screen
            else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended)
            {
                activeFingerID = -1;
                fingerDownPos = touch.screenPosition;

                if (detectSwipeAfterRelease && !swipe_done)
                {
                    DetectSwipe(touch);
                }

                if (hasSwipeOncedInTouchesLifeTime)
                    return;

                if (swipe_done)
                {
                    return;
                }

                bool isTapManualDetection = !(HorizontalMoveValue() > tapThreshold || VerticalMoveValue() > tapThreshold || /*currentTapTime <= 0*/ touch.time - touch.startTime > TapTimer);

             //   UnityEngine.Console.Log($"Is mannual TAP? {isTapManualDetection}. Time {touch.time - touch.startTime}");

                if (!touch.isTap && !isTapManualDetection)
                {
                   // UnityEngine.Console.Log("Not A Tap in ended phase. Distance of touch : "+ ((touch.screenPosition - touch.startScreenPosition).magnitude)/screenDPI);
                    ResetTapWindow();
                    return;
                }


                //if(!touch.isTap && isTapManualDetection)
                //{
                //    UnityEngine.Console.LogError("GRANTED YOU A TAP!");
                //}

              //  UnityEngine.Console.Log($"Its a tap in ended phase");
                // UnityEngine.Console.Log($"Its A Tap. Took {TapTimer - currentTapTime} seconds");

                // Its a tap
                tapCounter++;

                if (tapCounter == 1)
                {
                    InputChannel.SingleTap();
                    currentDoubleTapWindowElapsed = 0;
                }
                else if (tapCounter == 2 && currentDoubleTapWindowElapsed <= doubleTapWindowDuraiton)
                {
                    ResetTapWindow();
                    InputChannel.DoubleTab();
                }
            }
            else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                UnityEngine.Console.Log("Touch cancelled");
                activeFingerID = -1;
            }
        }
    }

    private void DetectSwipe(UnityEngine.InputSystem.EnhancedTouch.Touch touch)
    {
        float swipeDistCovered = (fingerDownPos - fingerUpPos).sqrMagnitude;
        swipeDistCovered /= screenDPI;
        float swipeVel = swipeDistCovered / (Time.unscaledTime - timeSwipeStarted);

        //  UnityEngine.Console.Log($"Swipe Velocity is {swipeVel}");

        if (swipeVel < minVelocityForValidSwipe)
        {
            //  UnityEngine.Console.LogError($"Low Velocity {swipeVel}");
            // return;
        }

        float horizontalToVerticalThresholdScaleFactor = SWIPE_THRESHOLD_Vertical / SWIPE_THRESHOLD_Horizontal;

        if (VerticalMoveValue() > SWIPE_THRESHOLD_Vertical && VerticalMoveValue() > (HorizontalMoveValue() * horizontalToVerticalThresholdScaleFactor))
        {
            if (fingerDownPos.y - fingerUpPos.y > 0 && lastSwipeDirection != SwipeDirection.Up)
            {
                OnSwipeUp();
                fingerUpPos = fingerDownPos;
                velocityAtSwipePerformed = swipeVel;
                timeSwipeStarted = Time.unscaledTime;
                //  UnityEngine.Console.Log("Swipe Deteced Up");
            }
            else if (fingerDownPos.y - fingerUpPos.y < 0 && lastSwipeDirection != SwipeDirection.Down)
            {
                OnSwipeDown();
                fingerUpPos = fingerDownPos;
                velocityAtSwipePerformed = swipeVel;
                timeSwipeStarted = Time.unscaledTime;
                //    UnityEngine.Console.Log("Swipe Deteced Down");
            }
        }
        else if (HorizontalMoveValue() > SWIPE_THRESHOLD_Horizontal /*&& HorizontalMoveValue() > VerticalMoveValue()*/)
        {
            if (fingerDownPos.x - fingerUpPos.x > 0 && lastSwipeDirection != SwipeDirection.Right)
            {
                if (lastSwipeDirectionPresistentForReswipePurpose == SwipeDirection.Right && HorizontalMoveValue() < (SWIPE_THRESHOLD_Horizontal + (SWIPE_THRESHOLD_Horizontal * swipeThresholdOffsetForContinousSameDirectionSwipeNormPerct)))
                {
                    return;
                }

                OnSwipeRight();
                fingerUpPos = fingerDownPos;
                timeSwipeStarted = Time.unscaledTime;
                velocityAtSwipePerformed = swipeVel;
                //   UnityEngine.Console.Log("Swipe Deteced Right");

                // UnityEngine.Console.Log($"Velocity At Swipe Right {velocityAtSwipePerformed}");
            }
            else if (fingerDownPos.x - fingerUpPos.x < 0 && lastSwipeDirection != SwipeDirection.Left)
            {
                if (lastSwipeDirectionPresistentForReswipePurpose == SwipeDirection.Left && HorizontalMoveValue() < (SWIPE_THRESHOLD_Horizontal + (SWIPE_THRESHOLD_Horizontal * swipeThresholdOffsetForContinousSameDirectionSwipeNormPerct)))
                {
                    return;
                }

                OnSwipeLeft();
                fingerUpPos = fingerDownPos;
                timeSwipeStarted = Time.unscaledTime;
                velocityAtSwipePerformed = swipeVel;

                //    UnityEngine.Console.Log("Swipe Deteced Left");
                // UnityEngine.Console.Log($"Velocity At Swipe Left {velocityAtSwipePerformed}");
            }
        }
    }

    private float CalculateCurrentTouchVelocity(UnityEngine.InputSystem.EnhancedTouch.Touch touch)
    {
        Vector2 distVector = touch.delta;
        float vel = (distVector).sqrMagnitude;
        vel /= /*touch.deltaTime*/ Time.unscaledDeltaTime;
        vel /= screenDPI;
        return vel;
    }

    private void OnSwipeUp()
    {
        InputChannel.SwipeUp();

        SetVariablesForSuccessfullSwipe();
        lastSwipeDirection = SwipeDirection.Up;
        lastSwipeDirectionPresistentForReswipePurpose = lastSwipeDirection;
    }

    private void OnSwipeDown()
    {
        InputChannel.SwipeDown();

        SetVariablesForSuccessfullSwipe();
        lastSwipeDirection = SwipeDirection.Down;
        lastSwipeDirectionPresistentForReswipePurpose = lastSwipeDirection;
    }

    private void OnSwipeLeft()
    {
        InputChannel.SwipeLeft();

        SetVariablesForSuccessfullSwipe();
        lastSwipeDirection = SwipeDirection.Left;
        lastSwipeDirectionPresistentForReswipePurpose = lastSwipeDirection;
    }

    private void OnSwipeRight()
    {
        InputChannel.SwipeRight();

        SetVariablesForSuccessfullSwipe();
        lastSwipeDirection = SwipeDirection.Right;
        lastSwipeDirectionPresistentForReswipePurpose = lastSwipeDirection;
    }

    private void SetVariablesForSuccessfullSwipe()
    {
        hasSwipeOnced = true;
        swipe_done = true;
        hasSwipeOncedInTouchesLifeTime = true;
    }

    private void ResetVariablesForReSwipeDetection(UnityEngine.InputSystem.EnhancedTouch.Touch touch)
    {
        elapsedLowVelocityTime = 0;
        lastSwipeDirection = SwipeDirection.None;
        //     UnityEngine.Console.Log("Changeing Last Swipe Direction None");
        hasSwipeOnced = false;
        swipe_done = false;
        fingerUpPos = touch.screenPosition;
        touchOrigin = touch.screenPosition;
        timeSwipeStarted = Time.unscaledTime;
    }

    #endregion MobileInputs

    #region KeyBoardInputs

    private void Inputs()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            InputChannel.SwipeLeft();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            InputChannel.SwipeRight();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            InputChannel.SwipeUp();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            InputChannel.SwipeDown();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            InputChannel.DoubleTab();
        }

        if (Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.P) || Input.GetMouseButtonDown(3) || Input.GetKeyDown(KeyCode.RightShift))
        {
            InputChannel.SingleTap();
        }
    }

    #endregion KeyBoardInputs
}