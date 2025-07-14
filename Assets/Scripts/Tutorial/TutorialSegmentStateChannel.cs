using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "TutorialSegmentStateChannel", menuName = "ScriptableObjects/Tutorial/TutorialSegmentStateChannel")]
public class TutorialSegmentStateChannel : ScriptableObject
{
    public UnityAction OnTutorialSegmentCompleted;

    public UnityAction OnTutorialSegmentUpdated;

    public UnityAction<int> OnUserResetToCheckPoint;

    public UnityAction<BoxCollider> OnGhostAnimationTriggered;

    public UnityAction OnSegmentActionsCompleted;

    public UnityAction OnRewind;

    public UnityAction GhostAnimationsCompleted;

    public UnityAction<BoxCollider> OnShowTutorialGesture;

    public UnityAction OnSlowMoRequested;

    public UnityAction OnSlowMoEndRequested;

    public UnityAction<TutorialGesture, bool> OnPlaySpecificHint;

    public UnityAction OnDisableActiveHints;
    
    public void SendDisableTheActiveHints()
    {
        OnDisableActiveHints.Invoke();
    }

    public void SendPlaySpecificHint(TutorialGesture gesture, bool loop = false)
    {
        OnPlaySpecificHint.Invoke(gesture, loop);
    }

    public void SendSegmentActionsCompelted()
    {
        OnSegmentActionsCompleted?.Invoke();
    }

    public void SendSlowMoRequestEvent()
    {
        OnSlowMoRequested?.Invoke();
    }

    public void SendSlowMoEndRequestEvent()
    {
        OnSlowMoEndRequested?.Invoke();
    }

    public void SendShowTutorialGestureEvent(BoxCollider boxCollider)
    {
        OnShowTutorialGesture?.Invoke(boxCollider);
    }

    public void SendTutorialSegmentCompletionEvent()
    {
        OnTutorialSegmentCompleted?.Invoke();
    }
    public void SendAllGhostAnimationsCompelted()
    {
        GhostAnimationsCompleted?.Invoke();
    }

    public void SendTutorialSegmentUpdatedEvent()
    {
        OnTutorialSegmentUpdated?.Invoke();
    }

    public void SendUserResetToCheckPoint(int lane)
    {
        OnUserResetToCheckPoint?.Invoke(lane);
    }

    public void SendGhostAnimationTriggered(BoxCollider boxCollider)
    {
        OnGhostAnimationTriggered?.Invoke(boxCollider);
    }

    public void SendRewindEvent()
    {
        OnRewind?.Invoke();
    }
    
}
