using DG.Tweening;
using FSM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = PlayerState.PlayerBuildingRunState, menuName = "ScriptableObjects/PlayerBuildingRunState")]
public class PlayerBuildingRunState : StateBase
{
    [SerializeField] private PlayerContainedData PlayerContainedData;
    [SerializeField] private PlayerSharedData PlayerSharedData;
    [SerializeField] private CameraShakeVariations cameraShakeVariations;
    [SerializeField] private GameEvent playerStoppedVerticalMotionBuilding;
    [SerializeField] private GameEvent playerStartedBuildingClimb;
    [SerializeField] private SpeedHandler speedHandler;
    [SerializeField] private InputChannel inputChannel;
    [SerializeField] private GameEvent playerWallRunBuildingRunoff;
    [SerializeField] private GameEvent playerHasCrashed;
    [SerializeField] private GameEvent playerBackInViewAfterBuildingRunCameraSwitch;

    private bool followingPath;
    private Coroutine revertTimeScaleRoutine;
    private bool crashedWhileInState = false;
    private int consecutiveWallRuns;

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += HandleActiveSceneChanged;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
    }

    private void HandleActiveSceneChanged(Scene scene, Scene scene1)
    {
        ResetVariable();
    }

    private void ResetVariable()
    {
        playerHasCrashed.TheEvent.RemoveListener(HandlePlayerHasCrashed);
        playerBackInViewAfterBuildingRunCameraSwitch.TheEvent.RemoveListener(RevertGameTimeScaleToNormal);
        UnSubsribeInputEvents();

        consecutiveWallRuns = 0;
        crashedWhileInState = false;
        followingPath = false;
    }

    //subscribing all events that can occur during this state
    private void SubsribeInputEvents()
    {
        PlayerContainedData.InputChannel.SwipeRightOccured.AddListener(SwipedRight);
        PlayerContainedData.InputChannel.SwipeLeftOccured.AddListener(SwipedLeft);
    }

    /// <summary>
    ///unsubscribing all events that can occur during this state
    /// </summary>
    private void UnSubsribeInputEvents()
    {
        PlayerContainedData.InputChannel.SwipeRightOccured.RemoveListener(SwipedRight);
        PlayerContainedData.InputChannel.SwipeLeftOccured.RemoveListener(SwipedLeft);
    }

    public override void OnEnter()
    {
        // UnityEngine.Console.LogError("Entered Building Run"+ crashedWhileInState);

        if (crashedWhileInState)
        {
            crashedWhileInState = false;
            SubsribeInputEvents();
            cameraShakeVariations.ShakeCameraAccordingToEvent(playerBackInViewAfterBuildingRunCameraSwitch);
            return;
        }

        base.OnEnter();

        var abc = GameObject.FindGameObjectsWithTag("Path");
        GameObject required = null;

        foreach (var item in abc)
        {
            if (item.transform.position.z > PlayerSharedData.PlayerTransform.position.z)
            {
                required = item;
                break;
            }
        }

        //for (int i = 0; i < required.transform.childCount; i++)
        //{
        //    PlayerSharedData.BuildingRunPath.Add(required.transform.GetChild(i).gameObject.transform);
        //}

        PlayerSharedData.bezierBuildingCurve = required.GetComponent<BezierCurve>();

        PlayerSharedData.WallClimbDoing = true;
        PlayerSharedData.WallRunBuilding = true;
        followingPath = false;

        if (PlayerSharedData.IsBoosting)
        {
            PlayerContainedData.PlayerDoubleBoostState.StopBoost(0f);
        }

        if (PlayerSharedData.IsDash)
        {
            PlayerContainedData.PlayerBoostState.StopDash(0);
        }

        FollowThePath();

        SubsribeInputEvents();

        playerHasCrashed.TheEvent.AddListener(HandlePlayerHasCrashed);
        playerBackInViewAfterBuildingRunCameraSwitch.TheEvent.AddListener(RevertGameTimeScaleToNormal);
    }

    public override void OnExit()
    {
        if (revertTimeScaleRoutine != null)
        {
            CoroutineRunner.Instance.StopCoroutine(revertTimeScaleRoutine);
        }

        if (PlayerSharedData.PlayerStateMachine.pendingStateToBeChangedTo.name == PlayerState.PlayerDeathState)
        {
            UnSubsribeInputEvents();
            crashedWhileInState = true;
            return;
        }

        base.OnExit();
     //   PlayerSharedData.BuildingRunPath.Clear();

        UnSubsribeInputEvents();

        playerHasCrashed.TheEvent.RemoveListener(HandlePlayerHasCrashed);
        playerBackInViewAfterBuildingRunCameraSwitch.TheEvent.RemoveListener(RevertGameTimeScaleToNormal);

        speedHandler.RevertGameTimeScaleToLastKnownNormalSpeed();
    }

    public override void OnLogic()
    {
        base.OnLogic();
    }

    private void HandlePlayerHasCrashed(GameEvent gameEvent)
    {
        crashedWhileInState = true;
    }

    private void RevertGameTimeScaleToNormal(GameEvent gameEvent)
    {
        revertTimeScaleRoutine = CoroutineRunner.Instance.StartCoroutine(RevertGameTimeScaleRoutine());
    }

    private IEnumerator RevertGameTimeScaleRoutine()
    {
        yield return new WaitForSeconds(.8f);

        speedHandler.ChangeGameTimeScaleInTime(SpeedHandler.GameTimeScaleBeforeOverriden, 3 * SpeedHandler.GameTimeScaleBeforeOverriden, true, true);
    }

    private void FollowThePath()
    {
        consecutiveWallRuns++;
        PlayerSharedData.PlayerTransform.rotation = Quaternion.identity;
        followingPath = true;
        playerStartedBuildingClimb.RaiseEvent();
        AnalyticsManager.CustomData("GamePlayScreen_WallRunStarted", new Dictionary<string, object> { { "WallRunTimes", consecutiveWallRuns } });

        List<Vector3> paths = new List<Vector3>();

        var b = PlayerSharedData.bezierBuildingCurve;

        float t = 0;
        for (; t <= 1; t += Time.deltaTime)
        {
            Vector3 point = b.GetPointAt(t);

            if (point.z < PlayerSharedData.PlayerTransform.position.z)
                continue;

            point.x = PlayerSharedData.PlayerTransform.position.x;
            paths.Add(point);
        }

        speedHandler.IncreaseSpeed();

        inputChannel.PauseInputsFromUser();

        PlayerSharedData.PlayerRigidBody.DOPath(paths.ToArray(), 2.4f, PathType.Linear, PathMode.Full3D, 20).SetLookAt(.1f).SetEase(Ease.InSine).SetUpdate(UpdateType.Fixed).OnUpdate(() =>
        {
            //  UnityEngine.Console.Log($"Following Path with vel {PlayerSharedData.PlayerRigidBody.velocity.magnitude}");
            float timeScale = speedHandler.GetGameTimeScaleBasedOnSpecificVelocity(PlayerSharedData.PlayerRigidBody.velocity.magnitude * Time.fixedDeltaTime);
            speedHandler.ChangeGameTimeScaleInTime(timeScale, 0, true, true);

            // UnityEngine.Console.Log("Velocity when following path " + PlayerSharedData.PlayerRigidBody.velocity.magnitude);
        })
          .OnComplete(() =>
          {
              PlayerSharedData.PlayerTransform.rotation = Quaternion.Euler(-90, 0, 0);
              /*  PlayerContainedData.PlayerBasicMovementShared.OverideCurrentPlayerLane(0);*/
              followingPath = false; speedHandler.IncreaseSpeed();
              inputChannel.UnPauseInputsFromUser();

              //UnityEngine.Console.Log("Reverting Back To Original Speed");
              //speedHandler.ChangeGameTimeScaleInTime(speedHandler.GameTimeScaleBeforeOverriden, 3 * speedHandler.GameTimeScaleBeforeOverriden, true, true);
          });
    }

    //onfixlogic basically works as fixupdate , all work related to fixupdate will be done in this method
    public override void OnFixedLogic()
    {
        base.OnFixedLogic();

        if (!followingPath)
        {
            PlayerContainedData.PlayerBasicMovementShared.movement();
        }

        if (PlayerSharedData.RotateOnbuilding)
            BuildingRotation();
    }

    private void SwipedRight()
    {
        if (followingPath)
            return;

        PlayerContainedData.PlayerBasicMovementShared.change_lane(1);
    }

    private void SwipedLeft()
    {
        if (followingPath)
            return;

        PlayerContainedData.PlayerBasicMovementShared.change_lane(-1);
    }

    public override void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == ("wallrunbuilding"))
        {
            inputChannel.PauseInputsFromUser();
            PlayerSharedData.RotateOnbuilding = true;
            playerStoppedVerticalMotionBuilding.RaiseEvent();
            playerWallRunBuildingRunoff.RaiseEvent();

            AnalyticsManager.CustomData("GamePlayScreen_WallRunEnded", new Dictionary<string, object> { { "WallRunTimes", consecutiveWallRuns } });
        }
    }

    public override void OnTriggerEnter(Collider other)
    {
        PlayerContainedData.PlayerBasicMovementShared.OnTriggerEnter(other);
    }

    private void BuildingRotation()
    {
        Quaternion rotaiton = Quaternion.RotateTowards(PlayerSharedData.PlayerTransform.rotation, Quaternion.identity, Time.fixedDeltaTime * (PlayerContainedData.PlayerData.PlayerInformation[0].BuildingFallingRotationSpeed));
        PlayerSharedData.PlayerRigidBody.MoveRotation(rotaiton);
        if (PlayerSharedData.PlayerTransform.rotation == Quaternion.identity)
        {
            PlayerSharedData.WallRunBuilding = false;
            PlayerSharedData.RotateOnbuilding = false;
            crashedWhileInState = false;
            StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToNormalMovement);
        }
    }
}