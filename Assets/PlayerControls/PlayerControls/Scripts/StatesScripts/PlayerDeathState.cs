using FSM;
using UnityEngine;

[CreateAssetMenu(fileName = PlayerState.PlayerDeathState, menuName = "ScriptableObjects/PlayerDeathState")]
public class PlayerDeathState : StateBase
{
    [SerializeField] private PlayerSharedData PlayerSharedData;
    [SerializeField] private PlayerContainedData PlayerContainedData;
    [SerializeField] private GameManager gameManager;

    private RaycastHit adjustmentHit;
    private float elaspsetime;

    public override void OnEnter()
    {

        base.OnEnter();

        elaspsetime = 0;

        // temp?
        PlayerSharedData.CharacterAnimator.ResetTrigger(AnimatorParameters.normal);
        PlayerSharedData.PlayerAnimator.ResetTrigger(AnimatorParameters.normal);

        PlayerSharedData.isCrashed = true;
        PlayerContainedData.AnimationChannel.Death(PlayerSharedData.PlayerAnimator);
    }

    public override void OnExit()
    {
        base.OnExit();
        PlayerSharedData.isCrashed = false; 
    }

    public override void OnLogic()
    {
        base.OnLogic();
    }

    public override void OnFixedLogic()
    {
        base.OnFixedLogic();

        if (PlayerSharedData.WallRunBuilding)
            return;

        float y;
        bool isGrounded = Grounded(); ; 
        PlayerSharedData.IsGrounded = isGrounded;
        ApplyGravity();

        if (PlayerSharedData.IsGrounded)
        {
            y = adjustmentHit.point.y + PlayerContainedData.PlayerData.PlayerInformation[0].HoverHeight;
        }
        else
        {
            y = PlayerContainedData.PlayerData.PlayerInformation[0].UpwardPos;
        }

      PlayerSharedData.PlayerRigidBody.MovePosition(new Vector3(PlayerSharedData.PlayerTransform.position.x, y, PlayerSharedData.PlayerTransform.position.z));
    }

    public bool Grounded()
    {
        float rayLength = PlayerContainedData.PlayerData.PlayerInformation[0].RaycastHeight;


        if (Physics.BoxCast(PlayerSharedData.RaycastOriginPosition.transform.position, PlayerSharedData.BoxColliderbounds, -PlayerSharedData.PlayerTransform.up, out adjustmentHit, Quaternion.identity, rayLength, PlayerContainedData.PlayerData.PlayerInformation[0].CarLayer, QueryTriggerInteraction.Ignore))
        {
            if (!PlayerSharedData.IsGrounded)
            {
                PlayerSharedData.isFalling = false;
            }

            if (PlayerSharedData.isJumpingPhase && !PlayerSharedData.isJumping)
            {
                PlayerSharedData.isJumpingPhase = false;
            }

             Debug.DrawRay(PlayerSharedData.RaycastOriginPosition.transform.position, -PlayerSharedData.PlayerTransform.up * rayLength, Color.green);
              ExtDebug.DrawBoxCastOnHit(PlayerSharedData.RaycastOriginPosition.transform.position, PlayerSharedData.BoxColliderbounds, Quaternion.identity, -PlayerSharedData.PlayerTransform.up, PlayerContainedData.PlayerData.PlayerInformation[0].AdjustmentRaycastHeight, Color.green);

            float y = adjustmentHit.point.y + PlayerContainedData.PlayerData.PlayerInformation[0].HoverHeight;
            if (!CheckIfWalkableSurfaceIsFeasible(y) && adjustmentHit.collider.gameObject.layer == LayerMask.NameToLayer("WalkableLimited"))
            {
                return false;
            }

            return true;
        }
        else
        {
            Debug.DrawRay(PlayerSharedData.RaycastOriginPosition.transform.position, -PlayerSharedData.PlayerTransform.up * rayLength, Color.red);
              ExtDebug.DrawBoxCastOnHit(PlayerSharedData.RaycastOriginPosition.transform.position, PlayerSharedData.BoxColliderbounds, Quaternion.identity, -PlayerSharedData.PlayerTransform.up, PlayerContainedData.PlayerData.PlayerInformation[0].AdjustmentRaycastHeight, Color.red);
            return false;
        }
    }

    private bool CheckIfWalkableSurfaceIsFeasible(float height)
    {
        return (height - PlayerSharedData.PlayerTransform.position.y) <= PlayerContainedData.PlayerData.PlayerInformation[0].StepHeight;
        //  return (height - PlayerSharedData.PlayerTransform.position.y) <= PlayerContainedData.PlayerData.PlayerInformation[0].StepHeight;
     //   return PlayerSharedData.PlayerTransform.position.y > height;
    }

    public void ApplyGravity()
    {
        if (!PlayerSharedData.IsGrounded && !PlayerSharedData.WallRunBuilding && !PlayerSharedData.RotateOnbuilding)
        {
            if (PlayerContainedData.PlayerData.PlayerInformation[0].UpwardPos > 5)
            {
                elaspsetime = 1;
            }

            if (!PlayerSharedData.isFalling)
            {
                PlayerContainedData.elapsedTimeRelaxationAfterFall = 0;

                //  float height = PlayerSharedData.PlayerTransform.position.y;
                // groundHitShakeValue = MyExtensions.RangeMapping(height, 3, 0, 2, 6);
                PlayerSharedData.isFalling = true;
            }

            PlayerContainedData.elapsedTimeRelaxationAfterFall += Time.deltaTime;

            elaspsetime += Time.fixedDeltaTime;

            float vel = LerpExtensions.EaseInSine(elaspsetime) * gameManager.GetTerminalVelocity * Time.fixedDeltaTime;
            if (PlayerSharedData.PlayerTransform.position.y + vel < PlayerContainedData.PlayerData.PlayerInformation[0].PlayerStartinPosition.y)
            {
                PlayerContainedData.PlayerData.PlayerInformation[0].UpwardPos = PlayerContainedData.PlayerData.PlayerInformation[0].PlayerStartinPosition.y;
            }
            else
            {
                PlayerContainedData.PlayerData.PlayerInformation[0].UpwardPos = PlayerSharedData.PlayerTransform.position.y + vel;
            }
        }
    }
}