using FSM;
using UnityEngine;
using DG.Tweening;
using TheKnights.SaveFileSystem;
using System.Collections;

[CreateAssetMenu(fileName = PlayerState.PlayerBoostState, menuName = "ScriptableObjects/PlayerBoostState")]
public class PlayerBoostState : StateBase
{
    [SerializeField] private PlayerSharedData PlayerSharedData;
    [SerializeField] private PlayerContainedData PlayerContainedData;
    [SerializeField] private GameEvent playerFinishedDashEvent;
    [SerializeField] private GameEvent playerStartedDashEvent;
    [SerializeField] private GameEvent playerHasCrashed;
    [SerializeField] private SaveManager saveManager;

    [SerializeField] private GameObject laserPrefab;
    private GameObject laserObj;


    public float tempSpeed;
    public float tempTime = 0.5f;
    public float tempTimeDeAcceleration = 0.5f;
    public Ease tempEase;
    public Ease stopTempEase;

    private Coroutine zadeLaserDisableRoutine;
    private Coroutine stopDashRoutine;


    //onenter is like start wich will be called once when dash starts

    private void OnEnable()
    {
        playerHasCrashed.TheEvent.AddListener(HandlePlayerHasCrashed);
    }

    private void OnDisable()
    {
        playerHasCrashed.TheEvent.RemoveListener(HandlePlayerHasCrashed);
    }

    public override void OnEnter()
    {
        base.OnEnter();
        PlayerSharedData.IsDash = true;
        PlayerSharedData.HalfDashCompleted = false;
    }

    //increasing speed during dash , it basically works in fixed update
    //public void Dash()
    //{
    //    elapse = MyExtensions.elapse_time(ref elapseTime, tempTime);

    //    if (elapse >= 1f && !PlayerSharedData.HalfDashCompleted)
    //    {
    //        PlayerSharedData.HalfDashCompleted = true;
    //        //if (elapse >= 0.6f)
    //        StopDash();
    //    }
    //}

    private void HandlePlayerHasCrashed(GameEvent gameEvent)
    {
        if (!PlayerSharedData.IsDash)
        {
            return;
        }

        PlayerSharedData.HalfDashCompleted = true;

        PlayerSharedData.IsDash = false;
        if(laserObj != null)
        {
            laserObj.SetActive(false);
        }
    }

    public void StartDash(bool isForced = false)
    {
        if(!isForced)
        {
            AnalyticsManager.CustomData("GamePlayScreen_PlayerDashed");
        }

        if(stopDashRoutine != null)
        {
            CoroutineRunner.Instance.StopCoroutine(stopDashRoutine);
        }

        OnEnter();
        playerStartedDashEvent.RaiseEvent();

        PlayerContainedData.SpeedHandler.ChangeGameTimeScaleInTime(tempSpeed, tempTime, true, true, tempEase, ()=> { StopDash(); }); //,5 Time

        if (saveManager.MainSaveFile.currentlySelectedCharacter == 2)
        {
            if (zadeLaserDisableRoutine != null)
            {
                CoroutineRunner.Instance.StopCoroutine(zadeLaserDisableRoutine);
            }

            if (laserObj == null)
            {
                laserObj = Instantiate(laserPrefab);

                laserObj.transform.SetParent(PlayerSharedData.PlayerTransform);
                laserObj.transform.localScale = new Vector3(1, 1, 1);
                laserObj.transform.transform.localPosition = new Vector3(0, 1, 0);
            }
            else
            {
                laserObj.SetActive(true);
            }
        }

    }

    public void StopDash(float stopTime = -1, bool preserveDashSpeed = false)
    {
        if (!PlayerSharedData.IsDash)
        {
            return;
        }

        float stopAfterDelayTime = .7f;

        if (stopTime == -1)
        {
            stopTime = tempTimeDeAcceleration;
        }
        else
        {
            stopAfterDelayTime = stopTime;
        }

        if (!preserveDashSpeed)
        {
            PlayerContainedData.SpeedHandler.RemoveOverrideGameTimeScaleMode(stopTime, stopTempEase);
        }

        stopDashRoutine = CoroutineRunner.Instance.StartCoroutine(StopDashAfterDelayRoutine(stopAfterDelayTime));

        if (saveManager.MainSaveFile.currentlySelectedCharacter == 2)
        {
           zadeLaserDisableRoutine = CoroutineRunner.Instance.StartCoroutine(DisableZadeLaser());
        }
    }

    private IEnumerator StopDashAfterDelayRoutine(float delay)
    {
        if (delay != 0)
        {
            yield return new WaitForSeconds(delay);
        }

        PlayerSharedData.HalfDashCompleted = true;
        PlayerSharedData.IsDash = false;
        playerFinishedDashEvent.RaiseEvent();

    }

    private IEnumerator DisableZadeLaser()
    {
        yield return new WaitForSeconds(.3f);

        if (laserObj != null)
        {
            laserObj.SetActive(false);
        }

    }



}
