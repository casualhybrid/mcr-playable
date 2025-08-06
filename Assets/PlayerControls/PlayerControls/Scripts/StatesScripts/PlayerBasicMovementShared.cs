using FSM;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(fileName = "PlayerBasicMovementShared", menuName = "ScriptableObjects/PlayerBasicMovementShared")]
public class PlayerBasicMovementShared : ScriptableObject
{
    private enum MoveDirection
    {
        Right, Left
    }

    [SerializeField] private GameManager gameManager;

    // GameEvents
    [SerializeField] private GameEvent playerHasDodged;

    [SerializeField] private GameEvent playerStoppedDodging;

    [SerializeField] private GameEvent playerHasDodgedLeft;
    [SerializeField] private GameEvent playerHasDodgedRight;
    [SerializeField] private GameEvent playerHasHitFromFront;
    [SerializeField] private GameEvent playerHasHitFromSide;
    [SerializeField] private GameEvent playerHasStumbled;
    [SerializeField] private GameEvent playerCompletedWallClimbTouchedGround;
    [SerializeField] private GameEvent playerStartedCurvedRamp;
    [SerializeField] private GameEvent playerEndedCurvedRamp;
    [SerializeField] private GameEvent playerBackToNormalLane;
    [SerializeField] private GameEvent playerTouchedTheGround;
    //[SerializeField] private GameEvent ArmourHasBeenPickedup;
   // [SerializeField] private GameEvent ArmourHasBeenUsedUp;
    [SerializeField] private GameEvent playerHasCrashedFromFront;
    [SerializeField] private GameEvent playerHasCrashedFromSide;
    [SerializeField] private GameEvent playerHasCrashed;

    // [SerializeField] private GameEvent destructibleDestroyed;
    [SerializeField] private GameEvent playerHasRevived;

   // [SerializeField] private GameEvent armourHasBeenUsedUp;
    [SerializeField] private PlayerSharedData PlayerSharedData;
    [SerializeField] private PlayerContainedData PlayerContainedData;
    [SerializeField] private SpeedHandler speedHandler;
    [SerializeField] private InputChannel inputChannel;
    [SerializeField] private TutorialSegmentStateChannel tutorialSegmentStateChannel;

    public int CurrentLane => current_lane;
    public Vector3 movement_vector { get; private set; }
    private float side_way_elapsed_time;
    private float start_pos;
    public float x_target_val { get; private set; } //target to move left and right
    private int current_lane = 0; //lane no to move obj to.
    private int previousLane = 0;
    public int target_lane = 0;
    private float steer_speed;

    [HideInInspector]
    public float sidewaysDistanceToCover = 0;

    private RaycastHit hit, adjustmentHit;

    private string lastObstaclePlayerCollidedWith;

    [HideInInspector]
    public float distForRayCast;

    private float curvedRampTime;
    private int lastLane;

    private bool snapBackToLane;
    private bool changingLane;

    private MoveDirection moveDirection;
    private HitDirection hitDirection;
    private float rotationSpeed;

    private Collider colliderPlayerCrashedInto;

    // Jump Retention
    private float retentionJumpTimerCurrent;

    private bool queuedRententionJump;
    private Coroutine jumpRetentionTimerRoutine;

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;

        playerHasStumbled.TheEvent.AddListener(SwitchLaneAccordingToCollision);
        playerHasCrashedFromFront.TheEvent.AddListener(HandlePlayerCrashedFromFront);
        playerHasCrashedFromSide.TheEvent.AddListener(HandlePlayerCrashedFromSide);
        playerHasCrashed.TheEvent.AddListener(SendCrashEvent);
       // ArmourHasBeenPickedup.TheEvent.AddListener(ArmourHasBeenPickedupCollision);
        // destructibleDestroyed.TheEvent.AddListener(HandleDestructibleDestroyed);
        playerHasRevived.TheEvent.AddListener(ResetLaneAfterRevive);
        inputChannel.SwipeUpOccured.AddListener(HandlePlayerSwipedUp);
        tutorialSegmentStateChannel.OnUserResetToCheckPoint += OverideCurrentPlayerLane;
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        ResetVariable();
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;

        playerHasStumbled.TheEvent.RemoveListener(SwitchLaneAccordingToCollision);
       // ArmourHasBeenPickedup.TheEvent.RemoveListener(ArmourHasBeenPickedupCollision);
        playerHasCrashedFromFront.TheEvent.RemoveListener(HandlePlayerCrashedFromFront);
        playerHasCrashedFromSide.TheEvent.RemoveListener(HandlePlayerCrashedFromSide);
        playerHasCrashed.TheEvent.RemoveListener(SendCrashEvent);
        playerHasRevived.TheEvent.RemoveListener(ResetLaneAfterRevive);

        //  destructibleDestroyed.TheEvent.RemoveListener(HandleDestructibleDestroyed);
        tutorialSegmentStateChannel.OnUserResetToCheckPoint -= OverideCurrentPlayerLane;
        inputChannel.SwipeUpOccured.RemoveListener(HandlePlayerSwipedUp);
    }

    public void ResetVariable()
    {
        queuedRententionJump = false;
        retentionJumpTimerCurrent = 0;
        changingLane = false;
        snapBackToLane = false;
        current_lane = 0;
        target_lane = 0;
        previousLane = 0;
        side_way_elapsed_time = 0;
        lastLane = 0;
        start_pos = 0;
        x_target_val = 0;
        steer_speed = 0;
        sidewaysDistanceToCover = 0;
        PlayerSharedData.CanWallRun = false;
        xpos = 0;
        ypos = 0;
        zpos = 0;
    }

    public void OverideCurrentPlayerLane(int lane)
    {
        current_lane = lane;
        steer_speed = 0;
        start_pos = lane;
        target_lane = lane;
        lastLane = current_lane;
        changingLane = false;
        previousLane = current_lane;
        lastLane = current_lane;
        x_target_val = lane;
        side_way_elapsed_time = 0;
        sidewaysDistanceToCover = 0;
        snapBackToLane = false;

    }

    /// <summary>
    ///  Lane would be changed on the basis of dir. negative value would move obj to left and positive value would move to right.
    /// </summary>

    public void change_lane(int dir)
    {
        target_lane = current_lane + dir;

        lastLane = current_lane;

        if (Mathf.Abs(target_lane) > PlayerContainedData.PlayerData.PlayerInformation[0].totalLanes || snapBackToLane)
            return;

        changingLane = true;
        previousLane = current_lane;

        //     playerHasDodged.RaiseEvent();

        if (dir == -1)
        {
            moveDirection = MoveDirection.Left;
            playerHasDodgedLeft.RaiseEvent();
        }
        else
        {
            moveDirection = MoveDirection.Right;
            playerHasDodgedRight.RaiseEvent();
        }

        //calculating for curved ramp working
        if (Mathf.Abs(target_lane) == PlayerContainedData.PlayerData.PlayerInformation[0].totalLanes)
        {
            PlayerSharedData.outsideNormalLane = true;
            curvedRampTime = PlayerContainedData.PlayerData.PlayerInformation[0].CurvedRampDuration;
        }
        //

        start_pos = PlayerSharedData.PlayerRigidBody.position.x;
        side_way_elapsed_time = 0;
        current_lane = current_lane + dir;

        //if (!PlayerSharedData.CanWallRun)
        //{
        //    x_target_val = current_lane * (PlayerContainedData.PlayerData.PlayerInformation[0].xDistToCover);
        //}
        //else
        //   {
        if (target_lane == -2)
        {
            x_target_val = (PlayerSharedData.PlayerTransform.position.x - 1.3f);
        }
        else if (target_lane == -1)
        {
            x_target_val = -1;
        }
        else
        {
            x_target_val = current_lane * (PlayerContainedData.PlayerData.PlayerInformation[0].xDistToCover);
        }
        // }
        sidewaysDistanceToCover = PlayerSharedData.PlayerRigidBody.position.x - x_target_val;
        PlayAnimation(dir);

        playerHasDodged.RaiseEvent();
    }

    [HideInInspector]
    public float ypos = 0;

    [HideInInspector]
    public float xpos = 0;

    [HideInInspector]
    public float zpos = 0;

    //  private float zCurveRampStartPos;
    private Vector3 pos;

    public void movement()
    {
        PlayerSharedData.IsGrounded = Grounded();

        ApplyGravity();

        if (PlayerSharedData.IsBoosting)
        {
            PlayerContainedData.PlayerDoubleBoostState.Boost();
        }
        //if (PlayerSharedData.IsDash)
        //{
        //    PlayerContainedData.PlayerBoostState.Dash();
        //}

        if (Mathf.Abs(target_lane) >= PlayerContainedData.PlayerData.PlayerInformation[0].totalLanes && PlayerSharedData.CanWallRun)
        {
            curvedRampTime -= Time.deltaTime;
            if (curvedRampTime <= 0)
            {
                curvedRamp();
            }
        }

        float sideWaysTime = snapBackToLane ? PlayerSharedData.SidewaysSpeed * 0.5f : PlayerSharedData.outsideNormalLane ? PlayerContainedData.SpeedHandler.GetSideWaysSpeedSynchronizedWithForwardSpeed() : PlayerSharedData.SidewaysSpeed;
        steer_speed = changingLane ? MyExtensions.elapse_time(ref side_way_elapsed_time, sideWaysTime) : 1;

        // float step = Mathf.SmoothStep(0, 1, steer_speed);
        float step = Mathf.Lerp(0, 1, steer_speed);
        xpos = Mathf.Lerp(start_pos, x_target_val, step);

        //   UnityEngine.Console.Log($"Change Lane in {sideWaysTime}");
        //  UnityEngine.Console.Log($"Step {step} startX {start_pos} endX {x_target_val}");

        if (!PlayerSharedData.WallRunBuilding && !snapBackToLane)
        {
            zpos = PlayerSharedData.PlayerRigidBody.position.z + PlayerSharedData.ForwardSpeed;

            //    UnityEngine.Console.Log("Moving with velocity " + (PlayerSharedData.ForwardSpeed / Time.deltaTime) + "Frame No. " + (CutSceneCore.frame) + " ForwardVelocity " + PlayerSharedData.ForwardSpeed);
        }
        else if (PlayerSharedData.WallRunBuilding)
        {
            pos = PlayerSharedData.PlayerRigidBody.position + (PlayerSharedData.PlayerTransform.forward * PlayerSharedData.ForwardSpeed);
            //  UnityEngine.Console.Log("Moving On Ramp Building With Velocity " + (pos - PlayerSharedData.PlayerTransform.position).magnitude/Time.fixedDeltaTime);

            zpos = pos.z;
        }

        if (step == 1f)
        {
            if (Mathf.Abs(previousLane) == 2 && PlayerSharedData.outsideNormalLane)
            {
                //  UnityEngine.Console.Log("BackToLane");
                PlayerSharedData.outsideNormalLane = false;
                playerBackToNormalLane.RaiseEvent();
            }

            playerStoppedDodging.RaiseEvent();
            changingLane = false;
            snapBackToLane = false;
        }

        //if ((PlayerSharedData.CurrentStateName != PlayerState.PlayerJumpState) && PlayerSharedData.CurrentStateName != PlayerState.PlayerThurstState && PlayerSharedData.CurrentStateName != PlayerState.PlayerAeroplaneState && PlayerSharedData.IsGrounded)
        //{
        //    PlayerContainedData.PlayerData.PlayerInformation[0].UpwardPos = hit.point.y + PlayerContainedData.PlayerData.PlayerInformation[0].HoverHeight;
        //}

        if (PlayerSharedData.IsGrounded)
        {
            
            //Debug.LogError("PlayerOnGround");
            ypos = PlayerContainedData.PlayerData.PlayerInformation[0].UpwardPos;
            elaspsetime = 0;

            if (PlayerSharedData.CurrentStateName != PlayerState.PlayerJumpState && PlayerSharedData.CurrentStateName != PlayerState.PlayerThurstState && PlayerSharedData.CurrentStateName != PlayerState.PlayerAeroplaneState)
            {
                adjustmentHit = AdjustHeightOfPlayer(xpos, ypos, zpos);

                ypos = PlayerContainedData.PlayerData.PlayerInformation[0].UpwardPos;

                float posY = adjustmentHit.point.y + (PlayerSharedData.wallslideDoing ? 0.05f : PlayerContainedData.PlayerData.PlayerInformation[0].HoverHeight);

              //  Debug.DrawRay(adjustmentHit.point, Vector3.up, Color.white) ;

                if (!CheckIfStepIsFeasible(posY) && adjustmentHit.collider.gameObject.layer == LayerMask.NameToLayer("WalkableLimited"))
                {
                    //    UnityEngine.Console.LogWarning($"STEP IS NOT FEASIBLE!!! {posY}");
                    posY = PlayerSharedData.PlayerTransform.position.y;
                }

                PlayerContainedData.PlayerData.PlayerInformation[0].UpwardPos = posY;

                PlayerSharedData.LastGroundedYPosition = posY;
                ypos = PlayerContainedData.PlayerData.PlayerInformation[0].UpwardPos;
                if (!PlayerSharedData.WallRunBuilding)
                {
                    Rotation(adjustmentHit.normal, rotationSpeed * SpeedHandler.GameTimeScale);
                }
                WaterParkEnter.Instance.WaterON();
            }
            else
            {
                WaterParkEnter.Instance.WaterOFF();
            }
        }
        else
        {
            ypos = PlayerContainedData.PlayerData.PlayerInformation[0].UpwardPos;

        }

        if (PlayerSharedData.wallslideDoing && !PlayerSharedData.IsGrounded)
        {
            adjustmentHit = AdjustHeightOfPlayer(xpos, ypos, zpos);
            ypos = adjustmentHit.point.y + PlayerContainedData.PlayerData.PlayerInformation[0].HoverHeight;
        }

        if ((PlayerSharedData.WallRunBuilding || PlayerSharedData.RotateOnbuilding))
        {
            PlayerContainedData.PlayerData.PlayerInformation[0].UpwardPos = pos.y;
            ypos = pos.y;
        }

        movement_vector = new Vector3(xpos, ypos, zpos);

        // UnityEngine.Console.Log("Move Player To " + zpos);

        //    UnityEngine.Console.Log("Before Calling Move Position " + PlayerSharedData.PlayerTransform.position.z);

        PlayerSharedData.PlayerRigidBody.MovePosition(movement_vector);
        //  UnityEngine.Console.Log("After Calling Move Position " + PlayerSharedData.PlayerTransform.position.z);

        // Input Jump Retention
        if (queuedRententionJump)
        {
            queuedRententionJump = false;
            retentionJumpTimerCurrent = 0;
            StateMachineEventsSender.SendStateMachineEvent(PlayerStateEvent.ToJump);
        }
    }

    private void HandlePlayerSwipedUp()
    {
        // Jump input retention
        if (PlayerSharedData.isFalling)
        {
           
            retentionJumpTimerCurrent = PlayerContainedData.PlayerData.PlayerInformation[0].NextJumpTimer;

            if (jumpRetentionTimerRoutine != null)
            {
                CoroutineRunner.Instance.StopCoroutine(jumpRetentionTimerRoutine);
            }

            jumpRetentionTimerRoutine = CoroutineRunner.Instance.StartCoroutine(UpdateRetentionJumpTimerRoutine());
        }
        
            
    }

    private IEnumerator UpdateRetentionJumpTimerRoutine()
    {
        while (retentionJumpTimerCurrent > 0)
        {
            retentionJumpTimerCurrent -= Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate(); 
        }
    }

    private bool IsBetween(float testValue, float bound1, float bound2)
    {
        if (bound1 > bound2)
            return (testValue >= bound2 || Mathf.Approximately(testValue, bound2))  && (testValue <= bound1 || Mathf.Approximately(testValue, bound1));

        return (testValue >= bound1 || Mathf.Approximately(testValue, bound1)) && (testValue <= bound2 || Mathf.Approximately(testValue, bound2));
    }

    private Vector3 OffsetRayCastingPointIfInBounds(Collider other, Transform rayCastT)
    {
        if (other.bounds.Contains(rayCastT.position))
        {
            //   UnityEngine.Console.Log($"In Bounds { rayCastT.name}");
            Vector3 pointOutsideBox = rayCastT.position - ((PlayerSharedData.PlayerRigidBody.velocity * Time.fixedDeltaTime * (Mathf.Epsilon * 1f + 1)));

            //  if (other.bounds.Contains(pointOutsideBox))
            //  {
            //      UnityEngine.Console.LogWarning($"Still in bounds {rayCastT.name}");
            //}

            #region DifferentWay

            // BoxCollider boxCollider = other as BoxCollider;

            // Vector3 boxSize = boxCollider.size;
            //  Vector3 playerVelocityDir = playerRunTimeData.TheRigidBody.velocity.normalized;

            //   Vector3 pointOutsideBox = rayCastT.position + (-playerVelocityDir * boxSize.magnitude * 0.5f);

            //Debug.DrawRay(rayCastT.position, -playerVelocityDir * 100f, Color.green, 3f);
            //  Debug.DrawRay(rayCastT.position, Vector3.up * 100f, Color.black, 3f);
            //  Debug.DrawRay(pointOutsideBox, Vector3.up * 100f, Color.red, 3f);

            //  Debug.DrawRay(point, Vector3.up * 100f, Color.magenta,3f);

            #endregion DifferentWay

            //Debug.DrawRay(rayCastT.position, -PlayerSharedData.PlayerRigidBody.velocity * 100f, Color.yellow, 3f);
            //Debug.DrawRay(rayCastT.position, Vector3.right * 100f, Color.red, 3f);
            //Debug.DrawRay(pointOutsideBox, Vector3.right * 100f, Color.green, 3f);
            //Debug.DrawRay(pointOutsideBox, Vector3.right * 100f, Color.green, 3f);

            return pointOutsideBox;
        }
        else
        {
            //     UnityEngine.Console.Log("Not In Bounds. " + rayCastT.name);
            return rayCastT.position;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if ((other.gameObject.layer == LayerMask.NameToLayer("Obstacles")))
        {
            if (other.GetType() != typeof(BoxCollider))
            {
                UnityEngine.Console.LogWarning($"A Box collider is needed on obstacle! {other.gameObject.name}");
                hitDirection = HitDirection.None;
                return;
            }

            hitDirection = HitDirection.None;
            hitDirection = other.gameObject.CompareTag("SideColliderL") ? HitDirection.Left : other.gameObject.CompareTag("SideColliderR") ? HitDirection.Right : HitDirection.None;

             
            if (hitDirection != HitDirection.None)
            {
                HandleResponseRespectToHitDirection(other);
                hitDirection = HitDirection.None;
                return;
            }

            List<Transform> raycastTCollection = PlayerSharedData.RaycastT.OrderBy(x => Vector3.SqrMagnitude(other.transform.position - x.position)).ToList();

            for (int i = 0; i < raycastTCollection.Count; i++)
            {
                Vector3 rayCastOrigin;
                Transform rayCastT = raycastTCollection[i];

                rayCastOrigin = OffsetRayCastingPointIfInBounds(other, rayCastT);

                //   UnityEngine.Console.Log("RayCastT " + rayCastT.name);
                hitDirection = GetSideHit.ReturnDirection(other.transform, rayCastOrigin);

                if (hitDirection != HitDirection.None)
                {
                    break;
                }
            }

            if (hitDirection == HitDirection.None)
            {
                UnityEngine.Console.LogWarning("All raycasts missed for obstacle side detection");
                HandleResponseRespectToHitDirection(other);
                hitDirection = HitDirection.None;
                return;
            }

            HandleResponseRespectToHitDirection(other);

            hitDirection = HitDirection.None;

        }
        if (other.gameObject.CompareTag("spring"))
        {
            AnalyticsManager.CustomData("GamePlayScreen_EnteredSpringRampZone");
            PlayerSharedData.SpringJump = true;
        }

        if (other.gameObject.CompareTag("wallrun"))
        {
            PlayerSharedData.CanWallRun = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("wallrun"))
        {
            if (PlayerSharedData.CanWallRun)
            {
                curvedRamp();
            }

            PlayerSharedData.CanWallRun = false;
        }

        if (other.gameObject.CompareTag("spring"))
        {
            PlayerSharedData.SpringJump = false;
        }
    }

    private void HandleResponseRespectToHitDirection(Collider other)
    {
      //   UnityEngine.Console.Log($"Hit Direction Is  { hitDirection}");

        //var destructibleObjects = other.GetComponents<IDestructibleObstacle>();

        //for (int i = 0; i < destructibleObjects.Length; i++)
        //{
        //    var destructibleObject = destructibleObjects[i];
        //    destructibleObject.DirectionThePlayerHitFrom = hitDirection;
        //}


        ObjectDestruction objectDestruction = other.GetComponent<ObjectDestruction>();

        objectDestruction?.DestroyCar(hitDirection);

        if (objectDestruction != null && PlayerSharedData.IsDash && objectDestruction.isDestroyableDuringDash)
        {
            return;
        }

        if (objectDestruction != null && objectDestruction.isDestroyDuringShockwave && PlayerSharedData.CurrentStateName == PlayerState.PlayerCanceljump)
        {
           // UnityEngine.Console.Log("Preventing Faliure!");
            return;
        }

        if (objectDestruction != null && PlayerSharedData.IsBoosting)
        {
            return;
        }
        //if (objectDestruction != null && PlayerSharedData.IsArmour)
        //{
        //    armourHasBeenUsedUp.RaiseEvent();
        //    return;
        //}

        if (hitDirection == HitDirection.None)
            return;

        Obstacle parentObstacle = other.GetComponentInParent<Obstacle>();

        if (parentObstacle == null)
        {
            UnityEngine.Console.LogWarning("No Parent Obstacle Found For Collider " + other.name);
        }
        else
        {
            lastObstaclePlayerCollidedWith = parentObstacle.name;
        }

        if (hitDirection == HitDirection.Forward)
        {

            BoxCollider boxCollider = other as BoxCollider;

            if (other.CompareTag("FrontWalkableObstacle"))
            {
                return;
            }

            CustomTag customTag = other.GetComponent<CustomTag>();

            if (customTag != null && customTag.HasTag("NoHitFromFront"))
            {
                UnityEngine.Console.Log("Front collision occurred but front collision is off");
                return;
            }

            Vector3 center = other.gameObject.transform.TransformPoint(boxCollider.center);

            Vector3 closestPointOnCollider = other.ClosestPoint(PlayerSharedData.PlayerTransform.position);

            bool isLeft = (closestPointOnCollider.x < center.x) ? true : false;

            Vector3 side = !isLeft ? center + (Vector3.right * boxCollider.bounds.size.x / 6.5f) : center - (Vector3.right * boxCollider.bounds.size.x / 6.5f);

            bool isInCenterRange = IsBetween(closestPointOnCollider.x, side.x, center.x);

            if(isInCenterRange && PlayerSharedData.WallRunBuilding)
            {
                hitDirection = HitDirection.Forward;
            }
            else if (isInCenterRange)
            {
                Vector3 maxAllowedTop = center + (Vector3.up * (boxCollider.bounds.size.y / 6));
                bool isInRangeBottomToTop = IsBetween(closestPointOnCollider.y, maxAllowedTop.y, boxCollider.bounds.min.y);

               //  UnityEngine.Console.Log($"You {closestPointOnCollider.y} Top Point {maxAllowedTop.y} and min Point { boxCollider.bounds.min.y}");

                if (!isInRangeBottomToTop && (customTag == null || !customTag.HasTag("BlockedObstacle")))
                {
                    UnityEngine.Console.LogWarning("Phew Saved You");
                    hitDirection = HitDirection.TopEdge;
                }
                else
                {
                    hitDirection = HitDirection.Forward;
                }
            }
            else
            {
                hitDirection = isLeft ? HitDirection.Left : HitDirection.Right;
            }
        }
        if (hitDirection == HitDirection.Left || hitDirection == HitDirection.Right || hitDirection == HitDirection.TopEdge)
        {
            colliderPlayerCrashedInto = other;
            playerHasHitFromSide.RaiseEvent();
        }
        else if (hitDirection == HitDirection.Forward || hitDirection == HitDirection.Bottom)
        {
            colliderPlayerCrashedInto = other;
            playerHasHitFromFront.RaiseEvent();
        }
    }

    private void SendCrashEvent(GameEvent gameEvent)
    {
        if (lastObstaclePlayerCollidedWith == null)
            return;
       
        AnalyticsManager.CustomData("GamePlayScreen_HitFail", new Dictionary<string, object>() {
                    { "ObstacleName", lastObstaclePlayerCollidedWith}
                });
        ChasingEnemyHandler.isHitFirstTime = true;

        if (PlayerPrefs.GetInt("TotalPlayTime", 0) >= 300 && /*PlayerSharedData.IsBoosting == false &&*/ gameManager.Invincible == false)
        {
            AnalyticsManager.CustomData("GamePlayScreen_CarCrashedWith", new Dictionary<string, object>() {
                    { "ObstacleName", lastObstaclePlayerCollidedWith }
                });

        }
    }

    //private void ArmourHasBeenPickedupCollision(GameEvent theEvent)
    //{
    //    PlayerSharedData.IsArmour = true;
    //}

    private void HandlePlayerCrashedFromFront(GameEvent gameEvent)
    {
        AdjustPlayerZPositionAfterCrash();
    }

    private void AdjustPlayerZPositionAfterCrash()
    {

        float adjustedPlayerZPosAfterCrash;
        Vector3 adjustedPos;

        if (PlayerSharedData.WallRunBuilding)
        {
            adjustedPlayerZPosAfterCrash = colliderPlayerCrashedInto.bounds.min.y - (PlayerSharedData.Playercollider.bounds.extents.z);
            adjustedPos = new Vector3(PlayerSharedData.PlayerTransform.position.x, adjustedPlayerZPosAfterCrash, PlayerSharedData.PlayerTransform.position.z);
        }
        else
        {
            adjustedPlayerZPosAfterCrash = colliderPlayerCrashedInto.bounds.min.z - (PlayerSharedData.Playercollider.bounds.extents.z);
            adjustedPos = new Vector3(PlayerSharedData.PlayerTransform.position.x, PlayerSharedData.PlayerTransform.position.y, adjustedPlayerZPosAfterCrash);
        }
        PlayerSharedData.PlayerRigidBody.interpolation = RigidbodyInterpolation.None;
        PlayerSharedData.PlayerTransform.position = adjustedPos;
        PlayerSharedData.PlayerRigidBody.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void AdjustPlayerXPositionAfterCrash()
    {
        float colliderXCorner = hitDirection == HitDirection.Left ? colliderPlayerCrashedInto.bounds.min.x : colliderPlayerCrashedInto.bounds.max.x;
        float adjustedPlayerXPosAfterCrash = hitDirection == HitDirection.Left ? colliderXCorner - (PlayerSharedData.Playercollider.bounds.extents.x) : colliderXCorner + (PlayerSharedData.Playercollider.bounds.extents.x);
        Vector3 adjustedPos = new Vector3(adjustedPlayerXPosAfterCrash, PlayerSharedData.PlayerTransform.position.y, PlayerSharedData.PlayerTransform.position.z);

        PlayerSharedData.PlayerRigidBody.interpolation = RigidbodyInterpolation.None;
        PlayerSharedData.PlayerTransform.position = adjustedPos;
        PlayerSharedData.PlayerRigidBody.interpolation = RigidbodyInterpolation.Interpolate;
    }

    //   private void HandleDestructibleDestroyed(GameEvent gameEvent)
    //   {
    //    UnityEngine.Console.Log("PlayerPosition Adjusting " + PlayerSharedData.PlayerTransform.position);

    //    float adjustedPlayerZPosAfterCrash;
    //    Vector3 adjustedPos;

    //    adjustedPlayerZPosAfterCrash = PlayerSharedData.LastDestructibleColliderHit.bounds.min.z - (PlayerSharedData.Playercollider.bounds.extents.z);
    //    adjustedPos = new Vector3(PlayerSharedData.PlayerTransform.position.x, PlayerSharedData.PlayerTransform.position.y, adjustedPlayerZPosAfterCrash);

    //    UnityEngine.Console.Log("Adjusted Position " + adjustedPos);

    // //   PlayerSharedData.PlayerTransform.position = adjustedPos;

    //   speedHandler.MiniPauseTheGame();
    //  }

    private void HandlePlayerCrashedFromSide(GameEvent gameEvent)
    {
        if (hitDirection == HitDirection.None)
            return;

        if (hitDirection == HitDirection.TopEdge)
        {
            AdjustPlayerZPositionAfterCrash();
        }
        else
        {
            AdjustPlayerXPositionAfterCrash();
        }
    }

    private void ResetLaneAfterRevive(GameEvent gameEvent)
    {
        OverideCurrentPlayerLane(0);
    }

    private void SwitchLaneAccordingToCollision(GameEvent theEvent)
    {
        if (hitDirection == HitDirection.None)
        {
            return;
        }

        // Throw Player Left
        if (moveDirection == MoveDirection.Right && hitDirection == HitDirection.Left)
        {
            //    playerRunTimeData.TheAnimator.SetTrigger("sideHit");
            change_lane(-1);
            snapBackToLane = true;
        }
        // Throw Player R
        else if (moveDirection == MoveDirection.Left && hitDirection == HitDirection.Right)
        {
            //    playerRunTimeData.TheAnimator.SetTrigger("sideHit");
            change_lane(1);
            snapBackToLane = true;
        }
    }

    public void curvedRamp()
    {
        //working for curved ramp

        if (Mathf.Abs(current_lane) == PlayerContainedData.PlayerData.PlayerInformation[0].totalLanes /*&& (PlayerSharedData.PlayerTransform.position.z > zCurveRampStartPos || !PlayerSharedData.CanWallRun)*/)
        {
            if (Mathf.Abs(target_lane) >= Mathf.Abs(current_lane))
            {
                lastLane = 1;
                change_lane(lastLane);
            }
        }
        //
    }

    public bool Grounded()
    {
        float rayLength = PlayerSharedData.wallslideDoing ? PlayerContainedData.PlayerData.PlayerInformation[0].RayHeightDuringWallRun : PlayerContainedData.PlayerData.PlayerInformation[0].RaycastHeight;

        if (Physics.BoxCast(PlayerSharedData.RaycastOriginPosition.transform.position, PlayerSharedData.BoxColliderbounds, -PlayerSharedData.PlayerTransform.up, out hit, Quaternion.identity, rayLength, PlayerContainedData.PlayerData.PlayerInformation[0].CarLayer, QueryTriggerInteraction.Ignore))
        {
          //  if (PlayerSharedData.isJumpingPhase)
          //  {
                float posY = hit.point.y + (PlayerSharedData.wallslideDoing ? 0.05f : PlayerContainedData.PlayerData.PlayerInformation[0].HoverHeight);

                if (!CheckIfStepIsFeasible(posY) && (hit.collider.gameObject.layer == LayerMask.NameToLayer("WalkableLimited") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Walkable")))
                {
                    return PlayerSharedData.IsGrounded;
                }
           // }

            if (PlayerSharedData.WallClimbDoing && !PlayerSharedData.IsGrounded && !PlayerSharedData.WallRunBuilding)
            {
                PlayerSharedData.WallClimbDoing = false;
                playerCompletedWallClimbTouchedGround.RaiseEvent();
                inputChannel.UnPauseInputsFromUser();

                //  UnityEngine.Console.LogWarning("Player Completed Wall Climb And Touched Ground");

                //Debug.DrawRay(PlayerSharedData.RaycastOriginPosition.transform.position, -PlayerSharedData.PlayerTransform.up * rayLength, Color.green);
            }

            if (!PlayerSharedData.IsGrounded)
            {
                if (PlayerSharedData.CurrentStateName == PlayerState.PlayerCanceljump)
                {
                    ObjectDestruction objectDestruction = hit.collider.transform.parent.GetComponentInChildren<ObjectDestruction>();
                    if (objectDestruction != null && objectDestruction.isDestroyDuringShockwave)
                    {
                        StackDestroyerShockWave.DestroyTheStack(PlayerSharedData.PlayerTransform.position, PlayerContainedData.PlayerData.PlayerInformation[0].PlayerStartinPosition.y);
                        return false;
                    }
                }

                //  UnityEngine.Console.Log($"Took {elaspsetime} seconds to land");

                PlayerSharedData.isFalling = false;

                bool shouldRaiseGroundEvent = (PlayerSharedData.CurrentStateName == PlayerState.PlayerAeroplaneState && PlayerSharedData.AeroplaneTakingOff) ||
                    (PlayerSharedData.isThursting && !PlayerSharedData.isHalfThurstingCompleted) ? false : true;

                if (shouldRaiseGroundEvent)
                {
                    //  UnityEngine.Console.Log($"PlayerStateIs {PlayerSharedData.CurrentStateName} and IsPlayerTakingOff? {PlayerSharedData.AeroplaneTakingOff}");

                 //   float posY = hit.point.y + (PlayerSharedData.wallslideDoing ? 0.05f : PlayerContainedData.PlayerData.PlayerInformation[0].HoverHeight);
                    PlayerSharedData.LastGroundedYPosition = posY;
                    playerTouchedTheGround.RaiseEvent();

                    // Jump input retention
                    if (retentionJumpTimerCurrent > 0)
                    {
                        queuedRententionJump = true;
                    }
                }
            }

            if (PlayerSharedData.isJumpingPhase && !PlayerSharedData.isJumping)
            {
                PlayerSharedData.isJumpingPhase = false;
            }
            PlayerPrefs.SetInt("doublejump", 0);

            PlayerSharedData.CurrentGroundColliderPlayerIsInContactWith = hit.collider;
            return true;
        }
        else
        {
            //  Debug.DrawRay(PlayerSharedData.RaycastOriginPosition.transform.position, -PlayerSharedData.PlayerTransform.up * rayLength, Color.red);
            return false;
        }
    }

    private bool CheckIfStepIsFeasible(float height)
    {
        return (height - PlayerSharedData.PlayerTransform.position.y) <= PlayerContainedData.PlayerData.PlayerInformation[0].StepHeight;
    }

    private Vector3 origin;

    private RaycastHit AdjustHeightOfPlayer(float x, float y, float z)
    {
        RaycastHit thehit;

        origin = PlayerSharedData.RaycastOriginPosition.transform.position;

        //  origin = new Vector3(x, origin.y, z);

        //  origin = new Vector3(x, y, z) + (PlayerSharedData.PlayerTransform.up * (0.5f));

        // Debug.DrawRay(origin, Vector3.right * 100f, Color.black);

        if (Physics.BoxCast(origin, PlayerSharedData.BoxColliderbounds, -PlayerSharedData.PlayerTransform.up, out thehit, Quaternion.identity, PlayerSharedData.wallslideDoing ? PlayerContainedData.PlayerData.PlayerInformation[0].RayHeightDuringWallRun : PlayerContainedData.PlayerData.PlayerInformation[0].AdjustmentRaycastHeight, PlayerContainedData.PlayerData.PlayerInformation[0].CarLayer, QueryTriggerInteraction.Ignore))
        {
            if (thehit.transform.tag == "curvedramp")
            {
                if (!PlayerSharedData.wallslideDoing)
                {
                    playerStartedCurvedRamp.RaiseEvent();
                    inputChannel.PauseInputsFromUser();
                }

                PlayerSharedData.wallslideDoing = true;
                rotationSpeed = PlayerContainedData.PlayerData.PlayerInformation[0].curvedRampRotationSpeed;
            }
            else if (PlayerSharedData.wallslideDoing)
            {
                if (PlayerSharedData.wallslideDoing)
                {
                    playerEndedCurvedRamp.RaiseEvent();
                    inputChannel.UnPauseInputsFromUser();
                }
                PlayerSharedData.CanWallRun = false;
                PlayerSharedData.wallslideDoing = false;
            }
            else
            {
                rotationSpeed = PlayerContainedData.PlayerData.PlayerInformation[0].RampRotationSpeed;
            }
            if (PlayerSharedData.CanWallRun)
            {
                Debug.DrawRay(origin, -PlayerSharedData.PlayerTransform.up * PlayerContainedData.PlayerData.PlayerInformation[0].RayHeightDuringWallRun, Color.green);
            }
            else
            {
                Debug.DrawRay(origin, -PlayerSharedData.PlayerTransform.up * PlayerContainedData.PlayerData.PlayerInformation[0].AdjustmentRaycastHeight, Color.green);
            }

            //    Debug.DrawRay(thehit.point, thehit.normal * 100f, Color.magenta, 1f);

            ExtDebug.DrawBoxCastOnHit(origin, PlayerSharedData.BoxColliderbounds, Quaternion.identity, -PlayerSharedData.PlayerTransform.up, PlayerSharedData.wallslideDoing ? PlayerContainedData.PlayerData.PlayerInformation[0].RayHeightDuringWallRun : PlayerContainedData.PlayerData.PlayerInformation[0].AdjustmentRaycastHeight, Color.red);

            return thehit;
        }
        else
        {
            if (PlayerSharedData.wallslideDoing)
            {
                playerEndedCurvedRamp.RaiseEvent();
            }

            PlayerSharedData.CanWallRun = false;
            PlayerSharedData.wallslideDoing = false;

            //if (PlayerSharedData.CanWallRun)
            //{
            //    Debug.DrawRay(origin, -PlayerSharedData.PlayerTransform.up * PlayerContainedData.PlayerData.PlayerInformation[0].RayHeightDuringWallRun, Color.red);
            //}
            //else
            //{
            //    Debug.DrawRay(origin, -PlayerSharedData.PlayerTransform.up * PlayerContainedData.PlayerData.PlayerInformation[0].AdjustmentRaycastHeight, Color.red);
            //}

            return hit;
        }
    }

    private float elaspsetime;
    private float groundHitShakeValue;

    public void ApplyGravity()
    {
        bool isCancelJumpState = PlayerSharedData.CurrentStateName == PlayerState.PlayerCanceljump;

        if ((isCancelJumpState || PlayerSharedData.HalfDashCompleted) && PlayerSharedData.CurrentStateName != PlayerState.PlayerThurstState && !PlayerSharedData.isJumping && PlayerSharedData.CurrentStateName != PlayerState.PlayerAeroplaneState && !PlayerSharedData.IsGrounded && !PlayerSharedData.WallRunBuilding && !PlayerSharedData.RotateOnbuilding)
        {
            //   float yVelocity = PlayerSharedData.PlayerRigidBody.velocity.y * Time.fixedDeltaTime;
            //  float easedInVal = yVelocity / (gameManager.GetTerminalVelocity * Time.fixedDeltaTime);
            //    elaspsetime = Mathf.Acos(1 - easedInVal) * (2 / Mathf.PI);
            if (isCancelJumpState || /*PlayerContainedData.PlayerData.PlayerInformation[0].UpwardPos > 5*/ PlayerSharedData.WallClimbDoing)
            {
                elaspsetime = 1;
            }

            if (!PlayerSharedData.isFalling)
            {
                PlayerContainedData.elapsedTimeRelaxationAfterFall = 0;

                float height = PlayerSharedData.PlayerTransform.position.y;
                groundHitShakeValue = MyExtensions.RangeMapping(height, 3, 0, 2, 6);
                PlayerSharedData.isFalling = true;
            }

            PlayerContainedData.elapsedTimeRelaxationAfterFall += Time.deltaTime;

            elaspsetime += Time.fixedDeltaTime;

            float vel = LerpExtensions.EaseInSine(elaspsetime) * gameManager.GetTerminalVelocity * Time.fixedDeltaTime * (PlayerSharedData.WallClimbDoing ? 6 : 1);
            if (PlayerSharedData.PlayerTransform.position.y + vel < PlayerContainedData.PlayerData.PlayerInformation[0].PlayerStartinPosition.y)
            {
                PlayerContainedData.PlayerData.PlayerInformation[0].UpwardPos = PlayerContainedData.PlayerData.PlayerInformation[0].PlayerStartinPosition.y;
            }
            else
            {
                // Shockwave on top of obstacles caused the player to crash. This was done to fix the problem

                float distanceBetweenPlayerTransformAndCastingPos = PlayerSharedData.RaycastOriginPosition.transform.position.y - PlayerSharedData.PlayerTransform.position.y;
                Debug.DrawRay(PlayerSharedData.RaycastOriginPosition.transform.position, (-PlayerSharedData.PlayerTransform.up * (-vel + distanceBetweenPlayerTransformAndCastingPos)), Color.yellow, Time.fixedDeltaTime);

                if (Physics.BoxCast(PlayerSharedData.RaycastOriginPosition.transform.position, PlayerSharedData.BoxColliderbounds, -PlayerSharedData.PlayerTransform.up, out RaycastHit thehit, Quaternion.identity, -vel + distanceBetweenPlayerTransformAndCastingPos, PlayerContainedData.PlayerData.PlayerInformation[0].CarLayer, QueryTriggerInteraction.Ignore))
                {
                    ExtDebug.DrawBoxCastOnHit(PlayerSharedData.RaycastOriginPosition.transform.position, PlayerSharedData.BoxColliderbounds, Quaternion.identity, -PlayerSharedData.PlayerTransform.up, thehit.distance + 0.01f, Color.yellow);

                    if (PlayerSharedData.PlayerTransform.position.y >= thehit.point.y && PlayerSharedData.PlayerTransform.position.y + vel < thehit.point.y)
                    {
                        PlayerContainedData.PlayerData.PlayerInformation[0].UpwardPos = thehit.point.y + PlayerContainedData.PlayerData.PlayerInformation[0].HoverHeight;
                    }
                    else
                    {
                        PlayerContainedData.PlayerData.PlayerInformation[0].UpwardPos = PlayerSharedData.PlayerTransform.position.y + vel;
                    }
                }
                else
                {
                    ExtDebug.DrawBoxCastOnHit(PlayerSharedData.RaycastOriginPosition.transform.position, PlayerSharedData.BoxColliderbounds, Quaternion.identity, -PlayerSharedData.PlayerTransform.up, -vel + distanceBetweenPlayerTransformAndCastingPos, new Color(1, 0.6f, 0));
                    PlayerContainedData.PlayerData.PlayerInformation[0].UpwardPos = PlayerSharedData.PlayerTransform.position.y + vel;
                }
            }
        }
        else
        {
            elaspsetime = 0;
        }
    }

    private void Rotation(Vector3 rotationTarget, float rotationSpeed)
    {
        Vector3 forwardProjection = Vector3.ProjectOnPlane(Vector3.forward, rotationTarget);
        Quaternion rotaiton = Quaternion.LookRotation(forwardProjection, rotationTarget);
        rotaiton = Quaternion.RotateTowards(PlayerSharedData.PlayerTransform.rotation, rotaiton, Time.fixedDeltaTime * rotationSpeed);
        PlayerSharedData.PlayerRigidBody.MoveRotation(rotaiton);
    }

    private void PlayAnimation(int dir)
    {
        if (dir == 1)
        {
            //if (PlayerSharedData.CurrentStateName == PlayerState.PlayerSlideState)
            //{
            //    PlayerContainedData.AnimationChannel.MoveRight(sidewaysDistanceToCover, PlayerSharedData.SidewaysSpeed, AnimatorParameters.slideright, PlayerSharedData.PlayerAnimator);
            //}
            //else if (PlayerSharedData.CurrentStateName == PlayerState.PlayerJumpState && PlayerContainedData.PlayerData.PlayerInformation[0].UpwardPos >= 0.5f)
            //{
            //    PlayerContainedData.AnimationChannel.Airright();
            //}
            if (PlayerSharedData.CurrentStateName != PlayerState.PlayerJumpState)
            {
                PlayerContainedData.AnimationChannel.MoveRight(sidewaysDistanceToCover, PlayerSharedData.SidewaysSpeed, AnimatorParameters.right, PlayerSharedData.PlayerAnimator);
                PlayerContainedData.AnimationChannel.MoveRight(sidewaysDistanceToCover, PlayerSharedData.SidewaysSpeed, AnimatorParameters.right, PlayerSharedData.CharacterAnimator);
            }
        }
        else if (dir == -1)
        {
            //if (PlayerSharedData.CurrentStateName == PlayerState.PlayerSlideState)
            //{
            //    PlayerContainedData.AnimationChannel.MoveRight(sidewaysDistanceToCover, PlayerSharedData.SidewaysSpeed, AnimatorParameters.slideleft, PlayerSharedData.PlayerAnimator);
            //}
            //else if (PlayerSharedData.CurrentStateName == PlayerState.PlayerJumpState && PlayerContainedData.PlayerData.PlayerInformation[0].UpwardPos >= 0.5f)
            //{
            //    PlayerContainedData.AnimationChannel.AirLeft();
            //}
            if (PlayerSharedData.CurrentStateName != PlayerState.PlayerJumpState)
            {
                PlayerContainedData.AnimationChannel.MoveRight(sidewaysDistanceToCover, PlayerSharedData.SidewaysSpeed, AnimatorParameters.left, PlayerSharedData.PlayerAnimator);

                PlayerContainedData.AnimationChannel.MoveRight(sidewaysDistanceToCover, PlayerSharedData.SidewaysSpeed, AnimatorParameters.left, PlayerSharedData.CharacterAnimator);

                //      PlayerContainedData.AnimationChannel.MoveRight(sidewaysDistanceToCover, PlayerSharedData.SidewaysSpeed, AnimatorParameters.left, PlayerSharedData.PlayerAnimator);
            }
        }
    }
}