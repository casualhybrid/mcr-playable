using System.Collections.Generic;
using UnityEngine;

public enum TutorialGhostState
{
    WaitingForPlayer, RunningAheadOfPlayer, PerformingActions, Rewinding, Done
}

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class TutorialGhost : MonoBehaviour
{
    public TutorialGhostState tutorialGhostState { get; private set; }

    [SerializeField] private PlayerSharedData playerSharedData;

    // Channels
    [SerializeField] private TutorialSegmentStateChannel tutorialSegmentStateChannel;

    [SerializeField] private TutorialSegmentData tutorialSegmentData;

    [SerializeField] private List<TutorialGhostSegmentAnimation> tutorialGhostSegmentAnimations;
    [SerializeField] private float zOffsetToMaintainFromPlayer;

    private TutorialGhostSegmentAnimation CurrenttutorialGhostSegmentAnimation;
    private int curGhostSegmentAnimIndex;

    private Rigidbody rigidBody;

    private Vector3 ghostActionStartingPos;
    private BoxCollider ghostActionStartingCollider;
    private int ghostActionStartingIndex;

    private Vector3 diffBWGhostColliderAndPivot;
    private Collider boxCollider;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();

        tutorialSegmentStateChannel.OnTutorialSegmentUpdated += HandleSegmentUpdated;
        //tutorialSegmentStateChannel.OnUserResetToCheckPoint += HandleUserResetToCheckPoint;
        tutorialSegmentStateChannel.OnGhostAnimationTriggered += HandleGhostAnimationTriggered;
        tutorialSegmentStateChannel.OnRewind += HandleRewind;
    }

    private void Start()
    {
        CurrenttutorialGhostSegmentAnimation = tutorialGhostSegmentAnimations[curGhostSegmentAnimIndex];
        CurrenttutorialGhostSegmentAnimation.OnAnimationSelected();
        diffBWGhostColliderAndPivot = boxCollider.bounds.center - transform.position;
    }

    private void OnDestroy()
    {
        tutorialSegmentStateChannel.OnTutorialSegmentUpdated -= HandleSegmentUpdated;
        //     tutorialSegmentStateChannel.OnUserResetToCheckPoint -= HandleUserResetToCheckPoint;
        tutorialSegmentStateChannel.OnGhostAnimationTriggered -= HandleGhostAnimationTriggered;
        tutorialSegmentStateChannel.OnRewind -= HandleRewind;
    }

    private void FixedUpdate()
    {
        if (tutorialGhostState == TutorialGhostState.PerformingActions)
            return;

        if (tutorialGhostState == TutorialGhostState.WaitingForPlayer)
        {
            float zDiff = transform.position.z - playerSharedData.PlayerTransform.position.z;

            if (zDiff <= zOffsetToMaintainFromPlayer)
            {
                tutorialGhostState = TutorialGhostState.RunningAheadOfPlayer;
            }
        }

        if (tutorialGhostState == TutorialGhostState.RunningAheadOfPlayer)
        {
            Vector3 pos = transform.position;
            Vector3 moveTartgetPos = new Vector3(pos.x, pos.y, playerSharedData.PlayerTransform.position.z) + (Vector3.forward * zOffsetToMaintainFromPlayer);
            rigidBody.MovePosition(moveTartgetPos);
        }
    }

    private void DoAnimation()
    {
        tutorialGhostState = TutorialGhostState.PerformingActions;
        TutorialGhostSegmentAnimation ghostSegmentAnimation = tutorialGhostSegmentAnimations[curGhostSegmentAnimIndex];
        ghostSegmentAnimation.DoAnimation(() =>
        {
            TutorialGhostSegmentAnimation completedAnimation = CurrenttutorialGhostSegmentAnimation;

            // All animations finished
            if (curGhostSegmentAnimIndex == tutorialGhostSegmentAnimations.Count - 1)
            {
               // UnityEngine.Console.Log("All animation ghost complteded");
                tutorialSegmentStateChannel.SendAllGhostAnimationsCompelted();
                tutorialGhostState = TutorialGhostState.Done;
                return;
            }

            curGhostSegmentAnimIndex = curGhostSegmentAnimIndex >= tutorialGhostSegmentAnimations.Count - 1 ? curGhostSegmentAnimIndex : curGhostSegmentAnimIndex + 1;
            CurrenttutorialGhostSegmentAnimation = tutorialGhostSegmentAnimations[curGhostSegmentAnimIndex];
            tutorialGhostState = TutorialGhostState.WaitingForPlayer;
           // if (completedAnimation != CurrenttutorialGhostSegmentAnimation)
          //  {
                CurrenttutorialGhostSegmentAnimation.OnAnimationSelected();
           // }
        });
    }

    private void HandleSegmentUpdated()
    {
        //CurrenttutorialGhostSegmentAnimation = tutorialGhostSegmentAnimations[tutorialSegmentData.curTutorialSegmentIndex];
    }

    private void HandleGhostAnimationTriggered(BoxCollider boxCollider)
    {
        if (tutorialGhostState == TutorialGhostState.Rewinding)
            return;

      //  UnityEngine.Console.Log("Triggered Ghost Action");

        if (tutorialGhostState != TutorialGhostState.PerformingActions && curGhostSegmentAnimIndex == tutorialSegmentData.curTutorialSegmentIndex)
        {
          //  UnityEngine.Console.Log("Setting ghost starting point");
            ghostActionStartingPos = transform.position;
            ghostActionStartingCollider = boxCollider;
            ghostActionStartingIndex = curGhostSegmentAnimIndex;
        }

        DoAnimation();
    }

    private void HandleRewind()
    {
        if (tutorialSegmentData.curTutorialSegmentIndex != curGhostSegmentAnimIndex)
        {
            curGhostSegmentAnimIndex = tutorialSegmentData.curTutorialSegmentIndex;
            // Stop the current animation (Assuming that the ghost has already performed or is performing the next segment animation)
            CurrenttutorialGhostSegmentAnimation.StopAllAnimations();
            CurrenttutorialGhostSegmentAnimation = tutorialGhostSegmentAnimations[curGhostSegmentAnimIndex];
        }

        CurrenttutorialGhostSegmentAnimation.StopAllAnimations();
        tutorialGhostState = TutorialGhostState.Rewinding;
        TutorialSegment curSegment = tutorialSegmentData.GetCurrentTutorialSegment();

        float desiredZTargetPosition = curSegment.checkPoint.z + zOffsetToMaintainFromPlayer;
       // UnityEngine.Console.Log("CurrentSegment CheckPoint Is " + curSegment.checkPoint.z);

        Vector3 finalPos = new Vector3(ghostActionStartingPos.x, ghostActionStartingPos.y, desiredZTargetPosition);

        if (CurrenttutorialGhostSegmentAnimation.RepositionOnAnimationSelected)
        {
            Vector3 rewindPos = CurrenttutorialGhostSegmentAnimation.GhostPositionOnAnimSwitched;

            float diff = rewindPos.z - curSegment.checkPoint.z;

            if (diff > zOffsetToMaintainFromPlayer)
            {
                CurrenttutorialGhostSegmentAnimation.RewindGhost(rewindPos, () => { tutorialGhostState = TutorialGhostState.WaitingForPlayer; }, true);
            }
            else
            {
                rewindPos = ghostActionStartingPos;
                CurrenttutorialGhostSegmentAnimation.RewindGhost(rewindPos, () => { DoAnimation(); }, true);
            }
        }
        else if(ghostActionStartingIndex != tutorialSegmentData.curTutorialSegmentIndex || ghostActionStartingCollider == null)
        {
            Vector3 rewindPos = new Vector3(curSegment.checkPoint.x, curSegment.checkPoint.y, desiredZTargetPosition);
            CurrenttutorialGhostSegmentAnimation.RewindGhost(rewindPos, () =>
            {
                    tutorialGhostState = TutorialGhostState.WaitingForPlayer;
            }
            );

        }
        else if (desiredZTargetPosition > ghostActionStartingPos.z)
        {
            float trimmedTargetPosition = /*curSegment.checkPoint.z + (ghostActionStartingPos.z - curSegment.checkPoint.z);*/ ghostActionStartingPos.z;
        //    UnityEngine.Console.Log("Trimming The Length and rewinding to " + trimmedTargetPosition);
            CurrenttutorialGhostSegmentAnimation.RewindGhost(new Vector3(ghostActionStartingPos.x, ghostActionStartingPos.y, trimmedTargetPosition), () => { DoAnimation(); });
        }
        else
        {
         //   UnityEngine.Console.Log("Not Trimming The Length");

            Vector3 rewindPos = new Vector3(ghostActionStartingPos.x, ghostActionStartingPos.y, desiredZTargetPosition);
            CurrenttutorialGhostSegmentAnimation.RewindGhost(rewindPos, () =>
            {
                //  Vector3 colliderPosAtRewind = GetColliderPositionForGivenPivotPos(rewindPos);

                // Caution! since the ghost is being rewinded on Update(), it's collider's position may not be final here
                if (ghostActionStartingCollider.bounds.Intersects(boxCollider.bounds))
                {
            //        UnityEngine.Console.LogWarning("Ghost was reset within action collider bounds");
                    DoAnimation();
                }
                else
                {
                    tutorialGhostState = TutorialGhostState.WaitingForPlayer;
                }
            }

            );
        }
    }

    private Vector3 GetColliderPositionForGivenPivotPos(Vector3 pos)
    {
        return pos + diffBWGhostColliderAndPivot;
    }
}