using UnityEngine;

public class AirplanePickupAnimation : MonoBehaviour
{
    private enum CurrentRotation
    {
        clockWise, antiClockWise
    }

    #region Variables

    [Header("Curves")]
    [SerializeField] private AnimationCurve _easedInBackOut;

    [Header("Rotation Animation")]
    [SerializeField] private float _fullRotationDuration = 2f;

    [SerializeField] private float _delayForAnticlockwiseRotation = 1.75f;
    [SerializeField] private float _delayForClockwiseRotation = 3.5f;

    #endregion Variables

    private CurrentRotation currentRotation;

    private float lerp;
    private Vector3 initialEulerAngles;
    private Vector3 targetEulerAngles;

    private float elapsedRotationTime;
    private float currentRotaionDelay;

    #region Unity Callbacks

    private void OnEnable()
    {
        ResetAnimation();
    }

    #endregion Unity Callbacks

    #region Reset

    private void ResetAnimation()
    {
        transform.localEulerAngles = Vector3.zero;
        SetValuesForAntiClockWiseRotation();
    }

    private void SetValuesForAntiClockWiseRotation()
    {
        elapsedRotationTime = 0;
        currentRotaionDelay = _delayForAnticlockwiseRotation;
        currentRotation = CurrentRotation.antiClockWise;

        lerp = 0;
        initialEulerAngles = transform.localEulerAngles;
        targetEulerAngles = initialEulerAngles + (Vector3.forward * 360);
    }

    private void SetValuesForClockWiseRotation()
    {
        elapsedRotationTime = 0;
        currentRotaionDelay = _delayForClockwiseRotation;
        currentRotation = CurrentRotation.clockWise;

        lerp = 0;
        initialEulerAngles = transform.localEulerAngles;
        targetEulerAngles = initialEulerAngles - (Vector3.forward * 360);
    }

    #endregion Reset

    private void SwitchRotationDirection()
    {
        currentRotation = currentRotation == CurrentRotation.antiClockWise ? CurrentRotation.clockWise : CurrentRotation.antiClockWise;

        if (currentRotation == CurrentRotation.antiClockWise)
        {
            SetValuesForAntiClockWiseRotation();
        }
        else
        {
            SetValuesForClockWiseRotation();
        }
    }

    private void Update()
    {
        if (elapsedRotationTime < currentRotaionDelay)
        {
            elapsedRotationTime += Time.deltaTime;
            return;
        }

        if (lerp >= 1)
        {
            SwitchRotationDirection();
            return;
        }

        lerp += Time.deltaTime / _fullRotationDuration;

        if (lerp > 1)
            lerp = 1;

        transform.localEulerAngles = (targetEulerAngles - initialEulerAngles) * _easedInBackOut.Evaluate(lerp);

        return;

     
    }

}